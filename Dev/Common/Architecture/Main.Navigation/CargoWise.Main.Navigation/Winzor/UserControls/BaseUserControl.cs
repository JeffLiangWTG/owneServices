using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;

namespace CargoWise.Main.Navigation;

public abstract class BaseUserControl : UserControl, IWinzorSupportedWPFContent, IWinzorControl
{
	public Task InvokeAsync(Action action) => InvokeWinzorDispatcherAsync(action);

	public void PopupMenu(object sender, ToolStripDropDownItem toolStripDropDown, Point point)
	{
		if (toolStripDropDown != null)
		{
			toolStripDropDown.DropDown.Show(this, point);
		}
	}
	public void UpdateMouseData(WebMouseEventArgs args) => SetMouseData(args);

	public async Task<ClientRect?> GetBoundingClientRectAsync(ElementReference element)
	{
		var tcs = new TaskCompletionSource<ClientRect?>();

		if (ElementInterop is not null && !element.Equals(default(ElementReference)))
		{
			await InvokeAsync(async () =>
			{
				await ExceptionHandlerExtension.HandleJSExceptionAsync(async () =>
				{
					var clientRect = await ElementInterop.GetBoundingClientRectAsync(element);
					tcs.SetResult(clientRect);
				});
			});
		}
		else
		{
			throw new InvalidOperationException("ElementInterop or ElementReference is null or not defined");
		}

		return await tcs.Task;
	}

	public void NotifyStateChanged() => NotifyRenderRequired();

	IElementJSInterop? elementInterop;
	IElementJSInterop? ElementInterop => elementInterop ??= GetJSInterop<IElementJSInterop>();
}
