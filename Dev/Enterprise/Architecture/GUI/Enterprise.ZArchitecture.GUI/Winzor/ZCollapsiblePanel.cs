using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZCollapsiblePanel : ZPanel, ISplitterLayoutSaveProvider, IAcceptNegativeSplitterPosition
	{
		KSplitter linkedSplitter;

		public const int CaptionHeight = 20;
		protected virtual bool IsVertical => Dock == DockStyle.Left || Dock == DockStyle.Right;
		protected virtual bool IsHorizontal => Dock == DockStyle.Top || Dock == DockStyle.Bottom;
		public bool isCollapsed { get; set; }
		int previousSize = 200;
		public string CollapsedLabelClass => IsVertical ? "collapsible-panel__label--vertical" : string.Empty;
		public string InnerPanelClass => IsVertical ? "collapsible-panel__inner--vertical" : "collapsible-panel__inner--horizontal";
		public int Rotation {
			get
			{
				return (IsCollapsed && Dock == DockStyle.Right || !IsCollapsed && Dock != DockStyle.Right) ? -90 : 90;
			}
		}
		internal async Task OnButtonClickAsync(WebMouseEventArgs e)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				IsCollapsed = !IsCollapsed;
			});
			StateHasChanged();
		}
		public bool IsCollapsed
		{
			get
			{
				return isCollapsed;
			}
			set
			{
				if (isCollapsed != value)
				{
					isCollapsed = value;
					UpdateSizeAndRepaint();
				}
			}
		}

		void UpdateSizeAndRepaint()
		{
			if (IsCollapsed)
			{
				if (linkedSplitter != null)
				{
					linkedSplitter.Visible = false;
				}

				foreach (Control control in Controls)
				{
					control.Visible = false;
				}

				if (IsVertical)
				{
					previousSize = base.Width;
					base.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(CaptionHeight);
				}
				else if (IsHorizontal)
				{
					previousSize = base.Height;
					base.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(CaptionHeight);
				}
			}
			else
			{
				if (IsVertical)
				{
					base.Width = ControlDpiScalingHelper.MarkAsScaled(previousSize);
				}
				else if (IsHorizontal)
				{
					base.Height = ControlDpiScalingHelper.MarkAsScaled(previousSize);
				}

				foreach (Control control in Controls)
				{
					control.Visible = true;
				}

				if (linkedSplitter != null)
				{
					linkedSplitter.Visible = true;
				}
			}

			Refresh();
		}

		int ISplitterLayoutSaveProvider.SplitterPosition
		{
			get
			{
				var scaledPosition = IsCollapsed ? -previousSize : (IsVertical ? Width : Height);
				var unscaledPosition = IsVertical
					? ControlDpiScalingHelper.UnscaleFromCurrentDpiX(scaledPosition)
					: ControlDpiScalingHelper.UnscaleFromCurrentDpiY(scaledPosition);
				return unscaledPosition;
			}
			set
			{
				if (value == 0)
				{
					return; // Not initialized value
				}

				if (value < 0)
				{
					IsCollapsed = true;
					previousSize = ControlDpiScalingHelper.ScaleToCurrentDpiX(-value);
				}
				else
				{
					var scaledPosition = IsVertical
						? ControlDpiScalingHelper.ScaleToCurrentDpiX(value)
						: ControlDpiScalingHelper.ScaleToCurrentDpiY(value);

					if (IsCollapsed)
					{
						previousSize = scaledPosition;
						IsCollapsed = false;
					}
					else
					{
						if (IsVertical)
						{
							Width = ControlDpiScalingHelper.MarkAsScaled(scaledPosition);
						}
						else
						{
							Height = ControlDpiScalingHelper.MarkAsScaled(scaledPosition);
						}
					}
				}
			}
		}

		int ISplitterLayoutSaveProvider.ContainerSize => ControlDpiScalingHelper.ScaleToCurrentDpiX(100); // Keep it same size (not related to parent form size)

		bool ISplitterLayoutSaveProvider.IsSplitterFixed => false;

		bool ISplitterLayoutSaveProvider.IsLayoutRestored { get; set; }

		public void LinkSplitter(KSplitter splitter)
		{
			linkedSplitter = splitter;
			if (linkedSplitter != null)
			{
				linkedSplitter.Visible = !IsCollapsed;
				linkedSplitter.DoNotSaveSplitterLayout = true;
			}
		}
	}
}
