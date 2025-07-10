using System.Drawing;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormsLocalizedTest]
	internal partial class ColourLegendElement : ZUserControl
	{
		public ColourLegendElement()
		{
			InitializeComponent();
		}

		public Color Colour
		{
			get { return ColourPart.BackColor; }
			set { ColourPart.BackColor = value; }
		}

		public override string Text
		{
			get { return TextPart.Text; }
			set
			{
				TextPart.Text = value;
				using (var g = CreateGraphics())
				{
					SizeF width = g.MeasureString(value, Font);
					ControlDpiScalingHelper.SetWidth(this, (int)width.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(35), false);
				}
			}
		}
	}
}
