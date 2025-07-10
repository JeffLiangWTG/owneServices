using System.Threading.Tasks;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace CargoWise.Windows.UI.JSInterop
{
	public interface IControlHighlightJSInterop : IJSInterop
	{
		Task InitializeAsync(bool enableTranslationFeedbackHighlight);
	}

	public class ControlHighlightJSInterop : JSInteropBase, IControlHighlightJSInterop
	{
		public ControlHighlightJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
			: base(jsRuntime, "/_content/CargoWise.Windows.UI/js/controlHighlight.js", fileVersionHash)
		{
		}

		public async Task InitializeAsync(bool enableTranslationFeedbackHighlight)
		{
			await InvokeJsAsync("initialize", enableTranslationFeedbackHighlight);
		}
	}
}
