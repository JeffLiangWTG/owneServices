using System;
using System.Drawing;
using System.Windows.Forms.VisualStyles;

using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	static class ZTextBoxRenderer
	{
		public static void DrawTextBox(Graphics graphic, Rectangle area, TextBoxState state, bool drawBorder, Color backColor)
		{
			if (state != TextBoxState.Normal && state != TextBoxState.Disabled && state != TextBoxState.Readonly)
			{
				throw new NotSupportedException("TextBoxState " + state + " is not supported.");
			}

			if (state != TextBoxState.Readonly && state != TextBoxState.Disabled)
			{
				var brush = BrushProvider.FromColor(backColor);
				graphic.FillRectangle(brush, area);
			}

			if (drawBorder)
			{
				var element = VisualStyleElement.TextBox.TextEdit.Normal;
				if (state != TextBoxState.Normal)
				{
					element = VisualStyleElement.TextBox.TextEdit.ReadOnly;
				}

				var renderer = new VisualStyleRenderer(element);
				// These subtractions are one pixel adjustments, independently of the DPI configuration. It allows for proper rendering of the 1 pixel line on the borders of the textbox
				area = ControlDpiScalingHelper.NewScaledRectangle(area.Location.X, area.Location.Y, area.Width - 1, area.Height - 1, false);
				using (var pen = new Pen(renderer.GetColor(ColorProperty.BorderColor)))
				{
					graphic.DrawRectangle(pen, area);
				}
			}
		}
	}
}
