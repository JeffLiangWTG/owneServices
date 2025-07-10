using System.Drawing;
using System.Windows.Forms.VisualStyles;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	/// <summary>
	/// When we use ComboBoxRenderer.DrawDropDownButton and change system display scale, Windows will help us create an image for combobox's arrow which will raise the arrow missing issue.
	/// So, we have to take this hack way to draw the arrow manually.
	/// Furthermore, I couldn't get system theme colors or background image from Win32 API(uxtheme.dll) to make arrow look exactly like ComboBoxRenderer.DrawDropDownButton and have to specify fixed color values.
	/// </summary>
	sealed class ZComboBoxRenderer
	{
		public static void DrawDropDownButton(Graphics graphics, Rectangle rectangle, ComboBoxState comboBoxState)
		{
			Color backgroundColor;
			Color borderColor;
			Color arrowColor;

			switch (comboBoxState)
			{
				case ComboBoxState.Pressed:
					backgroundColor = Color.FromArgb(255, 204, 228, 247);
					borderColor = Color.FromArgb(255, 0, 95, 183);
					arrowColor = Color.FromArgb(255, 98, 104, 109);
					break;
				case ComboBoxState.Hot:
					backgroundColor = Color.FromArgb(255, 229, 241, 251);
					borderColor = Color.FromArgb(255, 0, 120, 212);
					arrowColor = Color.FromArgb(255, 80, 83, 86);
					break;
				case ComboBoxState.Disabled:
					backgroundColor = Color.FromArgb(255, 255, 255, 255);
					borderColor = Color.FromArgb(255, 255, 255, 255);
					arrowColor = Color.FromArgb(255, 189, 189, 189);
					break;
				case ComboBoxState.Normal:
				default:
					backgroundColor = Color.FromArgb(255, 253, 253, 253);
					borderColor = Color.FromArgb(255, 210, 210, 210);
					arrowColor = Color.FromArgb(255, 110, 110, 110);
					break;
			}

			using (var brush = new SolidBrush(backgroundColor))
			{
				graphics.FillRectangle(brush, rectangle);
			}

			using (var pen = new Pen(borderColor))
			{
				graphics.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
			}

			var centerPoint = new Point(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2 + ControlDpiScalingHelper.ScaleToCurrentDpiY(1));
			var offset3X = ControlDpiScalingHelper.ScaleToCurrentDpiX(3);
			var offset3Y = ControlDpiScalingHelper.ScaleToCurrentDpiY(3);
			var leftPoint = new Point(centerPoint.X - offset3X, centerPoint.Y - offset3Y);
			var rightPoint = new Point(centerPoint.X + offset3X, centerPoint.Y - offset3Y);
			using (var pen = new Pen(arrowColor, 1))
			{
				graphics.DrawLine(pen, leftPoint, centerPoint);
				graphics.DrawLine(pen, centerPoint, rightPoint);
			}
		}
	}
}
