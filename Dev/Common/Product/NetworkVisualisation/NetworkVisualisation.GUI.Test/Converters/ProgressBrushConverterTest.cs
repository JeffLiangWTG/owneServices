using System.Globalization;
using System.Windows.Media;
using NUnit.Framework;
using static CargoWise.NetworkVisualisation.Business.NodeViewModel;
using Color = System.Drawing.Color;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(ProgressBrushConverter))]
	class ProgressBrushConverterTest : ValueConverterTestCase<ProgressBrushConverter>
	{
		public override void TestConvert()
		{
			ProgressBar[] tests = {
				null,
				new ProgressBar(Color.Red, 0),
				new ProgressBar(Color.Green, 0.3, 0.3),
				new ProgressBar(Color.Blue, 1),
			};

			foreach (var progressBar in tests)
			{
				var converter = new ProgressBrushConverter();
				var brush = converter.Convert(progressBar, typeof(Brush), null, CultureInfo.CurrentCulture);
				AssertBrush(brush, progressBar);
			}
		}

		void AssertBrush(object brush, ProgressBar progressBar)
		{
			if (progressBar is null)
			{
				AssertNull(brush);
			}
			else
			{
				var drawingBrush = (DrawingBrush)brush;
				var drawings = ((DrawingGroup)drawingBrush.Drawing).Children;
				var background = (GeometryDrawing)drawings[0];
				var backgroundBrush = (SolidColorBrush)background.Brush;
				var backgroundColor = backgroundBrush.Color;
				var fill = (GeometryDrawing)drawings[1];
				var fillGradient = ((LinearGradientBrush)fill.Brush).GradientStops;
				var fillPercent = (fillGradient[0].Offset + fillGradient[1].Offset) / 2;
				AssertColor(progressBar.background, backgroundColor);
				AssertEquals(Colors.Black, fillGradient[0].Color);
				AssertEquals(Colors.Black, fillGradient[1].Color);
				AssertEquals(Colors.Transparent, fillGradient[2].Color);
				AssertEquals(progressBar.percent, fillPercent);
				AssertEquals(progressBar.opacity, drawingBrush.Opacity);
			}
		}

		void AssertColor(Color expected, System.Windows.Media.Color actual)
		{
			AssertEquals(expected.A, actual.A);
			AssertEquals(expected.R, actual.R);
			AssertEquals(expected.G, actual.G);
			AssertEquals(expected.B, actual.B);
		}
	}
}
