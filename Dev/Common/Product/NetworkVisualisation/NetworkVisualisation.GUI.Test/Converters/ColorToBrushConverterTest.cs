using NUnit.Framework;
using Color = System.Drawing.Color;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(ColorToBrushConverter))]
	class ColorToBrushConverterTest : ValueConverterTestCase<ColorToBrushConverter>
	{
		public override void TestConvert()
		{
			Color[] colors = { Color.Red, Color.Green, Color.Blue };
			foreach (var color in colors)
			{
				AssertBrushConvertResult(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B), color);
			}

			AssertBrushConvertResult(System.Windows.Media.Color.FromArgb(0, 255, 255, 255), Color.Empty);
			AssertBrushConvertResult(System.Windows.Media.Color.FromArgb(0, 255, 255, 255), null);
		}
	}
}
