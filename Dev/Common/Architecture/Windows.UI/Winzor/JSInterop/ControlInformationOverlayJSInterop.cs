using System.Threading.Tasks;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace CargoWise.Windows.UI.JSInterop;

public interface IControlInformationOverlayJSInterop : IJSInterop
{
	Task InitializeAsync(ControlInformationOverlayComponent component);

	Task HighlightTargetControlAsync(string targetWinzorControlId);
}

public class ControlInformationOverlayJSInterop : JSInteropBase, IControlInformationOverlayJSInterop
{
	public ControlInformationOverlayJSInterop(IJSRuntime jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/CargoWise.Windows.UI/js/controlInformationOverlay.js", fileVersionHash)
	{
	}

	public async Task InitializeAsync(ControlInformationOverlayComponent component)
	{
		await InvokeJsAsync("initialize", DotNetObjectReference.Create(component));
	}

	public async Task HighlightTargetControlAsync(string targetWinzorControlId)
	{
		await InvokeJsAsync("highlightTargetControl", targetWinzorControlId);
	}
}
