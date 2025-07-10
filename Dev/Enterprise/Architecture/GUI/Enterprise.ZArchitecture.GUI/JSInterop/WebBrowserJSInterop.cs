using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WinzorFramework.JSInterop;

namespace Enterprise.ZArchitecture.GUI.JSInterop;

public interface IWebBrowserJSInterop : IJSInterop
{
	Task UpdateAnchorAttributesAsync();
	Task ResetScrollPositionAsync();
	Task ScrollToAsync(ElementReference element);
	Task ScrollToIDAsync(string elementID);
}

public class WebBrowserJSInterop : JSInteropBase, IWebBrowserJSInterop
{
	public WebBrowserJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/Enterprise.ZArchitecture.GUI/js/webBrowser.js", fileVersionHash)
	{
	}

	public async Task ScrollToAsync(ElementReference element)
	{
		await InvokeJsAsync("scrollTo", element);
	}

	public async Task ScrollToIDAsync(string elementID)
	{
		await InvokeJsAsync("scrollToElementId", elementID);
	}

	public async Task UpdateAnchorAttributesAsync()
	{
		await InvokeJsAsync("updateAnchorAttributes", "");
	}

	public async Task ResetScrollPositionAsync()
	{
		await InvokeJsAsync("ResetScrollPosition", "");
	}
}
