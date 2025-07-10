using System.Threading.Tasks;
using CargoWise.Main.Navigation.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace CargoWise.Main.Navigation.JsInterop;

public interface IQuickSearchJsInterop : IJSInterop
{
	Task RegisterFocusShortcutAsync(DotNetObjectReference<QuickSearch> dotNet);
	Task UpdateResultsAsync(string? popoverId);
	Task RegisterPopoverNavigationAsync(DotNetObjectReference<QuickSearch> dotNet, string? popoverId);
	Task RegisterFocusOutHandlerAsync(DotNetObjectReference<QuickSearch> dotNet, ElementReference? parentRef,
		string handleFocusOutMethodName);
}

public class QuickSearchJsInterop : JsInteropBaseWithSpecificExceptionIgnore, IQuickSearchJsInterop
{
	public QuickSearchJsInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/CargoWise.Main.Navigation/js/quickSearch.js", fileVersionHash)
	{
	}

	public async Task RegisterFocusShortcutAsync(DotNetObjectReference<QuickSearch> dotNet)
	{
		await InvokeJsAsync("registerFocusShortcut", dotNet);
	}

	public async Task UpdateResultsAsync(string? popoverId)
	{
		if (!string.IsNullOrEmpty(popoverId))
		{
			await InvokeJsAsync("updateResults", popoverId);
		}
	}

	public async Task RegisterPopoverNavigationAsync(DotNetObjectReference<QuickSearch> dotNet, string? popoverId)
	{
		if (!string.IsNullOrEmpty(popoverId))
		{
			await InvokeJsAsync("registerPopoverNavigation", dotNet, popoverId);
		}
	}

	public async Task RegisterFocusOutHandlerAsync(DotNetObjectReference<QuickSearch> dotNet, ElementReference? parentRef,
		string handleFocusOutMethodName)
	{
		if (parentRef.HasValue)
		{
			await InvokeJsAsync("registerFocusOutHandler", dotNet, parentRef.Value, handleFocusOutMethodName);
		}
	}
}
