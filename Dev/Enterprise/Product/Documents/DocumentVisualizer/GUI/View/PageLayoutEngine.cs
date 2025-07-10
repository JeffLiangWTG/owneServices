using System;
using System.Reactive.Disposables;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using CargoWise.Windows.UI;

namespace Enterprise.DocumentVisualizer.GUI
{
	public sealed class PageLayoutEngine : LayoutEngine
	{
		public PageLayoutEngine(ScrollableControl parent)
		{
			this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
		}

		readonly ScrollableControl parent;

		public const int MaxSupportedControlSize = 32767;

#if !WINZOR
		public void HandleScroll()
		{
			if (parent.VerticalScroll.Maximum - parent.VerticalScroll.Value < parent.VerticalScroll.LargeChange * 2)
			{
				PerformLayout();
			}
		}
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1040", Justification = "false positive")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1017", Justification = "false positive")]
		public void PerformLayout()
		{
			if (isPerformingLayout)
			{
				return;
			}

			using (Disposable.Create(() => isPerformingLayout = false))
			{
				isPerformingLayout = true;

				var parentDisplayRectangle = parent.DisplayRectangle;
				var nextControlLocation = parentDisplayRectangle.Location;

				var reachedMaxPagesLayoutLimit = false;
				var aggregatedLaidOutPagesHeight = 0;

				foreach (Control control in parent.Controls)
				{
					reachedMaxPagesLayoutLimit = reachedMaxPagesLayoutLimit
						|| aggregatedLaidOutPagesHeight > MaxSupportedControlSize;

					if (reachedMaxPagesLayoutLimit)
					{
						control.Visible = false;
						continue;
					}

					control.Visible = true;

					var offset = (parentDisplayRectangle.Width - control.Size.Width) / 2;
					int offsetX = Math.Max(10, offset);

					nextControlLocation.Offset(offsetX, control.Margin.Top);

					control.Location = nextControlLocation;

					ControlDpiScalingHelper.SetX(ref nextControlLocation, parentDisplayRectangle.X, false);
					ControlDpiScalingHelper.SetY(ref nextControlLocation, nextControlLocation.Y + control.Height, false);

					aggregatedLaidOutPagesHeight += nextControlLocation.Y + control.Height;
				}
			}
		}

		bool isPerformingLayout;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Testing")]
		public override bool Layout(object container, LayoutEventArgs layoutEventArgs = null)
		{
			if (string.Compare(layoutEventArgs?.AffectedProperty, nameof(Control.Bounds), StringComparison.OrdinalIgnoreCase) == 0)
			{
				PerformLayout();
			}
			return false;
		}
	}
}
