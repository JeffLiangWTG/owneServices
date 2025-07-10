using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace Enterprise.DocumentEngine.GUI.JSInterop
{
	public interface IFlexCelPreviewJSInterop : IJSInterop
	{
		Task InitializeMainIntersectionObserverAsync(DotNetObjectReference<FlexCelPreview> dotNetReference, ElementReference previewReference);
		Task RegisterMainKeyEventHandlerAsync(ElementReference previewReference);
		Task ScrollMainPageIntoViewAsync(ElementReference previewReference, int pageNo);
		Task ScrollSinglePageIntoViewAsync(ElementReference previewReference, bool scrollToTop);
		Task ScrollThumbPageIntoViewAsync(ElementReference previewReference, int pageNo, int ySep, int realYSep);
		Task StartDragScrollAsync(DotNetObjectReference<FlexCelPreview> dotNetReference, ElementReference previewReference, int pageNo);
	}

	public sealed class FlexCelPreviewJSInterop : JSInteropBase, IFlexCelPreviewJSInterop
	{
		public FlexCelPreviewJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
			: base(jsRuntime, "/_content/Enterprise.DocumentEngine.GUI/js/documentPreview.js", fileVersionHash)
		{
		}

		public async Task ScrollMainPageIntoViewAsync(ElementReference previewReference, int pageNo)
		{
			await InvokeJsAsync("scrollMainPageIntoView", previewReference, pageNo);
		}

		public async Task ScrollSinglePageIntoViewAsync(ElementReference previewReference, bool scrollToTop)
		{
			await InvokeJsAsync("scrollSinglePageIntoView", previewReference, scrollToTop);
		}

		public async Task ScrollThumbPageIntoViewAsync(ElementReference previewReference, int pageNo, int ySep, int realYSep)
		{
			await InvokeJsAsync("scrollThumbPageIntoView", previewReference, pageNo, ySep, realYSep);
		}

		public async Task StartDragScrollAsync(DotNetObjectReference<FlexCelPreview> dotNetReference, ElementReference previewReference, int pageNo)
		{
			await InvokeJsAsync("startDragScroll", dotNetReference, previewReference, pageNo);
		}

		public async Task RegisterMainKeyEventHandlerAsync(ElementReference previewReference)
		{
			await InvokeJsAsync("registerMainKeyEventHandler", previewReference);
		}

		public async Task InitializeMainIntersectionObserverAsync(DotNetObjectReference<FlexCelPreview> dotNetReference, ElementReference previewReference)
		{
			await InvokeJsAsync("initialiseMainIntersectionObserver", dotNetReference, previewReference);
		}
	}
}
