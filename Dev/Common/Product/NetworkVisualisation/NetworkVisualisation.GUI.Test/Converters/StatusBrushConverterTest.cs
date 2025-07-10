using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
using CargoWise.NetworkVisualisation.Business;
using NUnit.Framework;
using Color = System.Drawing.Color;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(StatusBrushConverter))]
	class StatusBrushConverterTest : ValueConverterTestCase<StatusBrushConverter>
	{
		public override void TestConvert()
		{
			var colors = new List<ColorOffset>() { new ColorOffset(Color.Red, 0), new ColorOffset(Color.Green, 0.4) };
			var converter = new StatusBrushConverter();
			var brush = (LinearGradientBrush)converter.Convert(new NodeColors(colors, 90, 0.7), typeof(LinearGradientBrush), null, CultureInfo.CurrentCulture);
			var horizontalBrush = (LinearGradientBrush)converter.Convert(new NodeColors(colors, 0, 0.7), typeof(LinearGradientBrush), null, CultureInfo.CurrentCulture);
			Assert(brush.EndPoint.X < 0.001);
			AssertEquals(1.0, brush.EndPoint.Y);
			AssertEquals(1.0, horizontalBrush.EndPoint.X);
			Assert(horizontalBrush.EndPoint.Y < 0.001);
			AssertColor(Color.Red, brush.GradientStops[0].Color);
			AssertEquals(0.0, brush.GradientStops[0].Offset);
			AssertColor(Color.Green, brush.GradientStops[1].Color);
			AssertEquals(0.4, brush.GradientStops[1].Offset);
			AssertEquals(0.7, brush.Opacity);
		}

		void AssertColor(Color color, System.Windows.Media.Color mediaColor)
		{
			AssertEquals(color.A, mediaColor.A);
			AssertEquals(color.R, mediaColor.R);
			AssertEquals(color.G, mediaColor.G);
			AssertEquals(color.B, mediaColor.B);
		}
	}
}
