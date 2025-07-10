using System;
using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.BufferManagement.GUI
{
	public class BorderSpec
	{
		public BorderSpec(string borderThickness)
		{
			var thicknesses = borderThickness.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (thicknesses.Length == 1)
			{
				LeftThickness = TopThickness = RightThickness = BottomThickness = TryParse(thicknesses[0]);
			}
			else if (thicknesses.Length >= 4)
			{
				LeftThickness = TryParse(thicknesses[0]);
				TopThickness = TryParse(thicknesses[1]);
				RightThickness = TryParse(thicknesses[2]);
				BottomThickness = TryParse(thicknesses[3]);
			}
		}

		public Color BorderColor { get; set; }

		public int LeftThickness { get; private set; }
		public int RightThickness { get; private set; }
		public int TopThickness { get; private set; }
		public int BottomThickness { get; private set; }

		public void SetBorderStyles(ButtonBorderStyle borderStyle)
		{
			LeftBorderStyle = GetStyleOrNoneIfZeroWidth(borderStyle, LeftThickness);
			RightBorderStyle = GetStyleOrNoneIfZeroWidth(borderStyle, RightThickness);
			TopBorderStyle = GetStyleOrNoneIfZeroWidth(borderStyle, TopThickness);
			BottomBorderStyle = GetStyleOrNoneIfZeroWidth(borderStyle, BottomThickness);
		}

		public ButtonBorderStyle LeftBorderStyle { get; set; }
		public ButtonBorderStyle RightBorderStyle { get; set; }
		public ButtonBorderStyle TopBorderStyle { get; set; }
		public ButtonBorderStyle BottomBorderStyle { get; set; }

		static ButtonBorderStyle GetStyleOrNoneIfZeroWidth(ButtonBorderStyle borderStyle, int width)
		{
			return width > 0 ? borderStyle : ButtonBorderStyle.None;
		}

		static int TryParse(string s)
		{
			int i;
			return int.TryParse(s, out i) ? i : 0;
		}
	}
}
