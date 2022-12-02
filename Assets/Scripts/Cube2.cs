using UnityEngine;
using UnityEngine.UI;
using mcDesktopCapture;
using System.Linq;
using System.Threading.Tasks;

public class Cube2 : MonoBehaviour
{
    [SerializeField]
    private Dropdown dropdown;

    private WindowProperty[] list = { };

    private bool isRunning = false;
    private bool setTexture = false;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;

        DesktopCapture2.Init();

        list = DesktopCapture2.WindowList;
        var non = new WindowProperty
        {
            windowID = -999,
            owningApplication = new WindowProperty.RunningApplication
            {
                applicationName = "Stop"
            },
            frame = new WindowProperty.Frame(),
            isOnScreen = true
        };
        // Append Property for Stop
        list = list.Append(non).ToArray();
        dropdown.options = list.ToList()
            .ConvertAll(window => new Dropdown.OptionData($"{window.owningApplication.applicationName}({window.windowID})"));

        for (int i = 0; i < list.Length; i++)
        {
            if (list[i].owningApplication.applicationName == "Google Chrome" && list[i].isOnScreen)
            {
                Debug.Log($"{list[i]}, {list[i].windowID}, {list[i].frame.width}, {list[i].frame.height}");
                DesktopCapture2.StartCaptureWithWindowID(list[i].windowID, list[i].frame.width, list[i].frame.height, true);
                isRunning = true;
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //transform.Rotate(-0.2f, -0.3f, 0.4f);

        if (!isRunning) return;

        if (setTexture) return;

        var texture = DesktopCapture2.GetTexture2D();
        if (texture == null) return;

        setTexture = true;

        Renderer m_Renderer = GetComponent<Renderer>();
        m_Renderer.material.SetTexture("_MainTex", texture);
    }

    void OnDisable()
    {
        Debug.Log("OnDisable");
        DesktopCapture2.StopCapture();
        DesktopCapture2.Destroy();
    }

    public void DropdownValueChanged()
    {
        Debug.Log($"dropdown: {dropdown.value}");
        dropdown.enabled = false;
        isRunning = false;
        DesktopCapture2.StopCapture();
        setTexture = false;
        Debug.Log("restarting...");
        Restart();
    }

    async void Restart()
    {
        await Task.Delay(500);
        var id = list[dropdown.value].windowID;
        if (id < 0)
        {
            Debug.Log("non");
            isRunning = false;
            dropdown.enabled = true;
            return;
        }
        DesktopCapture2.StartCaptureWithWindowID(id, list[dropdown.value].frame.width, list[dropdown.value].frame.height, true);
        await Task.Delay(500);
        Debug.Log("restart!");
        isRunning = true;
        dropdown.enabled = true;
    }
}

