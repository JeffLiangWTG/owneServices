using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class ContentPanelLayoutEngine : LayoutEngine
	{
		public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
		{
			var parent = container as Control;

			if (parent == null)
			{
				return false;
			}

			var controlsToLayout = parent.Controls
				.Cast<Control>()
				.Where(c => c.Visible)
				.ToArray();

			if (!controlsToLayout.Any())
			{
				return false;
			}

			var parentDisplayRectangle = parent.DisplayRectangle;

			var nextControlLocation = parentDisplayRectangle.Location;

			var largestCaption = controlsToLayout.Any()
				? controlsToLayout.Select(MeasureCaption).Max()
				: 0f;

			const int topMargin = 6;
			const int leftMargin = 6;
			const int rightMargin = 2;

			var offsetX = (int)Math.Ceiling(largestCaption) + ControlDpiScalingHelper.ScaleToCurrentDpiX(leftMargin);

			foreach (var control in controlsToLayout)
			{
				nextControlLocation.Offset(offsetX, topMargin);

				control.Location = nextControlLocation;
				ControlDpiScalingHelper.SetWidth(control, parent.Width - offsetX - ControlDpiScalingHelper.ScaleToCurrentDpiX(rightMargin), false);

				ControlDpiScalingHelper.SetX(ref nextControlLocation, parentDisplayRectangle.X, false);
				ControlDpiScalingHelper.SetY(ref nextControlLocation, nextControlLocation.Y + control.Height, false);
			}

			return true;
		}

		float MeasureCaption(Control control)
		{
			float result = 0f;

			var checkBox = control as ZCheckBox;

			if (checkBox != null && checkBox.CheckAlign != ZContentAlignment.Right)
			{
				return result;
			}

			using (Graphics gr = Graphics.FromHwnd(IntPtr.Zero))
			{
				var render = control.GetExtension<LabelCaptionRenderer>();

				if (render != null && render.Visible)
				{
					result = gr.MeasureString(render.Caption + render.LabelSeparator, render.Font).Width;
				}
			}

			return result;
		}
	}
}