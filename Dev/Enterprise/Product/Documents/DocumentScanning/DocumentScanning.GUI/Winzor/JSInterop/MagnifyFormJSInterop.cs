using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WinzorFramework.JSInterop;

namespace Enterprise.DocumentScanning.GUI.JSInterop;

public interface IMagnifyFormJSInterop : IJSInterop
{
	Task EnableDraggablePositionAsync(ElementReference pictureBox);
	Task EnableMovementByToolbarButtonsAsync(ElementReference pictureBox, ElementReference toolbar, int distance);
	Task EnableZoomByDropEditAsync(ElementReference pictureBox, ElementReference dropDown);
	Task FillToWidthAsync(ElementReference pictureBox);
	Task RemoveSelectionBoxAsync(ElementReference pictureBox);
}

public sealed class MagnifyFormJSInterop : JSInteropBase, IMagnifyFormJSInterop
{
	public MagnifyFormJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/Enterprise.DocumentScanning.GUI/js/magnifyForm.js", fileVersionHash)
	{
	}

	public async Task EnableDraggablePositionAsync(ElementReference pictureBox)
	{
		await InvokeJsAsync("pictureBoxEnableDraggablePositionAsync", pictureBox);
	}

	public async Task EnableMovementByToolbarButtonsAsync(ElementReference pictureBox, ElementReference toolbar, int distance)
	{
		await InvokeJsAsync("pictureBoxEnableMovementByToolbarButtons", pictureBox, toolbar, distance);
	}

	public async Task EnableZoomByDropEditAsync(ElementReference pictureBox, ElementReference dropDown)
	{
		await InvokeJsAsync("pictureBoxEnableZoomByDropEdit", pictureBox, dropDown);
	}

	public async Task FillToWidthAsync(ElementReference pictureBox)
	{
		await InvokeJsAsync("pictureBoxFillToWidth", pictureBox);
	}

	public async Task RemoveSelectionBoxAsync(ElementReference pictureBox)
	{
		await InvokeJsAsync("removeSelectionBox", pictureBox);
	}
}
