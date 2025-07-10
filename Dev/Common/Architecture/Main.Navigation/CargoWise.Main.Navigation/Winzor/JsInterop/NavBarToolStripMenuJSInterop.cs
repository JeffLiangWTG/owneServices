using System.Threading.Tasks;
using CargoWise.Main.Navigation.JsInterop;
using CargoWise.Main.Navigation.ViewModels;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace CargoWise.Main.Navigation;

public interface INavBarToolStripMenuInterop
{
	Task Setup();
}

public class NavBarToolStripMenuJSInterop : JSInteropBase, INavBarToolStripMenuInterop
{
	public NavBarToolStripMenuJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/CargoWise.Main.Navigation/js/navBarToolStripMenu.js", fileVersionHash)
	{
	}

	public async Task Setup()
	{
		await InvokeJsAsync("InitScrollMenu");
	}
}
