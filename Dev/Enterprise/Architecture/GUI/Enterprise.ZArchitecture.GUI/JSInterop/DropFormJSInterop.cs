using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WinzorFramework.JSInterop;

namespace Enterprise.ZArchitecture.GUI.JSInterop;

public interface IDropFormJSInterop : IJSInterop
{
	Task ScrollToAsync(ElementReference dropForm, int itemHeight, int index);
}

public class DropFormJSInterop : JSInteropBase, IDropFormJSInterop
{
	public DropFormJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/Enterprise.ZArchitecture.GUI/js/dropForm.js", fileVersionHash)
	{
	}

	public async Task ScrollToAsync(ElementReference dropForm, int itemHeight, int index)
	{
		await InvokeJsAsync("scrollTo", dropForm, itemHeight, index);
	}
}
