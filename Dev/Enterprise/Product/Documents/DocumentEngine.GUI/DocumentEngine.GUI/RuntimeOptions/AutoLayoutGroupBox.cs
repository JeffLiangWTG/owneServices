using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	public class AutoLayoutGroupBox : ZArchitecture.GUI.ZGroupBox
	{
		[DpiState(DpiState.ScaleY)]
#if DEBUG
		internal
#endif
		static readonly int TopMargin = ControlDpiScalingHelper.ScaleToCurrentDpiY(16);

		[DpiState(DpiState.ScaleY)]
#if DEBUG
		internal
#endif
		static readonly int BottomMargin = ControlDpiScalingHelper.ScaleToCurrentDpiY(8);

		[DpiState(DpiState.ScaleX)]
#if DEBUG
		internal
#endif
		static readonly int LeftMargin = ControlDpiScalingHelper.ScaleToCurrentDpiX(8);

		[DpiState(DpiState.ScaleX)]
#if DEBUG
		internal
#endif
		static readonly int RightMargin = ControlDpiScalingHelper.ScaleToCurrentDpiX(8);

		[DpiState(DpiState.ScaleX)]
		public int ColumnWidth { get; set; } = ControlDpiScalingHelper.ScaleToCurrentDpiX(500);

		internal List<int> NextHeights;
		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			AlignControls();
		}

		internal void AlignControls()
		{
			var gapsToAlign = Controls.OfType<RuntimeOptionUserControl>().GroupBy(c => c.Left / ColumnWidth);

			foreach (var column in gapsToAlign)
			{
				var longestDescription = Math.Min(column.Max(c => c.DesiredCaptionWidth) + RightMargin, ColumnWidth / 3);

				foreach (var row in column)
				{
					ControlDpiScalingHelper.SetWidth(row, ColumnWidth, false);
					row.ChangeLabelSizeForAlignment(longestDescription);
				}
			}
		}

		public AutoLayoutGroupBox()
		{
			NextHeights = new List<int> { TopMargin };
		}

		public void AddControl(Control controlToAdd)
		{
			Controls.Add(controlToAdd);
			controlToAdd.Location = GetControlLocation(controlToAdd, 0);
		}

		public void SetHeight()
		{
			ControlDpiScalingHelper.SetHeight(this, DesiredHeight, false);
		}

		public void ForceHeight(int height)
		{
			ControlDpiScalingHelper.SetHeight(this, height, false);
		}

		public void RearrangeInNColumns(int columns)
		{
			ColumnWidth = Math.Max(ColumnWidth, Controls.Cast<Control>().Select(c => c.Width).DefaultIfEmpty().Max());

			for (var i = 1; i < columns; i++)
			{
				NextHeights.Add(TopMargin);
			}

			var maxHeightOfEachColumnScaled = DesiredHeight / columns;
			var maxHeightWithoutMargin = (DesiredHeight - TopMargin - BottomMargin) / columns;

			for (var i = 0; i < Controls.Count; i++)
			{
				if ((Controls[i].Bottom + Controls[i].Top - 2 * TopMargin) / 2 > maxHeightWithoutMargin)
				{
					var columnToGoTo = Math.Min(Controls[i].Bottom / maxHeightOfEachColumnScaled, columns - 1);
					Controls[i].Location = GetControlLocation(Controls[i], columnToGoTo);
				}
				else
				{
					NextHeights[0] = Controls[i].Bottom;
				}
			}
			SetHeight();
		}

		[DpiState(DpiState.ScaleX)]
		public int DesiredWidth => Controls.Cast<Control>().Select(c => c.Right).DefaultIfEmpty().Max() + RightMargin;

		[DpiState(DpiState.ScaleY)]
		public int DesiredHeight => NextHeights.Max() + BottomMargin;

		[return: DpiState(DpiState.ScaledVariant)]
		Point GetControlLocation(Control controlToAdd, int columnToUse)
		{
			var resultScaled = ControlDpiScalingHelper.NewScaledPoint(LeftMargin + ColumnWidth * columnToUse, NextHeights[columnToUse], false);
			NextHeights[columnToUse] += controlToAdd.Height;

			return resultScaled;
		}
	}
}
