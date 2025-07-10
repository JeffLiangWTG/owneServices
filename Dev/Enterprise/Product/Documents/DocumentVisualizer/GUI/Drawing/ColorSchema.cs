using System;
using System.Drawing;

namespace Enterprise.DocumentVisualizer.GUI
{
	static class ColorSchema
	{
		public readonly static Color Editable = Color.Green;
		public readonly static Color Notifications = Color.FromArgb(255, 255, 192);
		public readonly static Color IsOverridden = Color.Orange;
		public readonly static Color StatusBarBackground = Color.FromArgb(0, 168, 255);
		public readonly static Color StatusBarText = Color.White;
		public readonly static Tuple<Color, Color> ErrorBackgroundGradient = new Tuple<Color, Color>(Color.FromArgb(255, 213, 213), Color.FromArgb(255, 234, 234));
		public readonly static Tuple<Color, Color> MessageErrorBackgroundGradient = new Tuple<Color, Color>(Color.FromArgb(213, 234, 255), Color.FromArgb(234, 243, 255));
		public readonly static Tuple<Color, Color> WarningBackgroundGradient = new Tuple<Color, Color>(Color.FromArgb(255, 244, 171), Color.FromArgb(255, 251, 221));
	}
}