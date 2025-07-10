#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using WinzorFramework.JSInterop;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ChannelRowControl : ZUserControl
	{
		const string BorderStyleString = $"border: 1px solid gray; border-collapse: collapse;";

		public override bool UseParentDivForLayout => false;

		bool isDragging;
		bool isDragEnding;
		double? startMouseY;
		double? currentOffset;

		string OffsetTopStyle => currentOffset is null ? string.Empty : $"top: {currentOffset}px;";
		string ZIndexStyle => startMouseY is null ? string.Empty : (NoResString)"z-index: 1;";

		public override string ExtraStyleString => $"{base.ExtraStyleString}{BorderStyleString}contain:layout;";
		protected override string ControlStyleString => $"{base.ControlStyleString}{ZIndexStyle}{OffsetTopStyle}";

		IClientEventService? ClientEventService => FindForm()?.CargoWiseClientServices?.ClientEventService;

		Task<double>? getStartOffsetTask;
		RegisteredClientEvent[] dragEvents = Array.Empty<RegisteredClientEvent>();

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			GetJSInterop<IElementJSInterop>()?.PreloadInterop();
		}

		public async Task StartDrag(WebMouseEventArgs args)
		{
			if (isDragging)
			{
				return;
			}

			startMouseY = args.PageY;
			getStartOffsetTask = (GetJSInterop<IElementJSInterop>()?.GetOffsetTopAsync(ElementReference) ?? Task.FromResult(0.0));
			
			if (Parent?.CaptureElementReference == true && ClientEventService != null)
			{
				isDragging = true;
				dragEvents = await Task.WhenAll(
				ClientEventService.RegisterMouseEventListenerAsync(DragMoveAsync, ClientMouseEvent.MouseMove, Parent.ElementReference),
				ClientEventService.RegisterGlobalMouseEventListenerAsync(DragEndAsync, ClientMouseEvent.MouseUp)
				);
			}
			StateHasChanged();
		}

		public async Task DragMoveAsync(WebMouseEventArgs args)
		{
			var task = getStartOffsetTask;
			if (task != null && startMouseY != null)
			{
				var offsetTop = await task;
				currentOffset = args.PageY - startMouseY + offsetTop;
				StateHasChanged();
			}
		}

		public async Task DragEndAsync(WebMouseEventArgs args)
		{
			if (!isDragging || isDragEnding)
			{
				return;
			}

			isDragEnding = true;
			await DragMoveAsync(args);
			var y = currentOffset;
			getStartOffsetTask = null;
			startMouseY = null;
			await Task.WhenAll(
				InvokeWinzorDispatcherAsync(() =>
				{
					if (y != null)
					{
						ControlDpiScalingHelper.SetTop(this, (int)y, false);
					}
					if (viewModel is not null)
					{
						OnFinishDragging();
					}
				}),
				Task.WhenAll(dragEvents.Select(e => e.DisposeAsync().AsTask()))
			);
			dragEvents = Array.Empty<RegisteredClientEvent>();
			isDragging = false;
			isDragEnding = false;
		}
	}
}
