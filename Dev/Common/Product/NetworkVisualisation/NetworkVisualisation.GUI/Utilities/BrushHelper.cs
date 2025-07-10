using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI
{
	public static class BrushHelper
	{
		public static Brush ColorsToBrush(IEnumerable<ColorOffset> colors, double angle)
		{
			if (!colors.Any())
			{
				return null;
			}
			else if (colors.Count() > 1)
			{
				var gradients = new GradientStopCollection();
				foreach (var colorOffset in colors)
				{
					var c = colorOffset.color;
					var color = Color.FromArgb(c.A, c.R, c.G, c.B);
					gradients.Add(new GradientStop(color, colorOffset.offset));
				}
				return new LinearGradientBrush(gradients, angle);
			}
			else
			{
				var c = colors.First().color;
				var color = Color.FromArgb(c.A, c.R, c.G, c.B);
				return new SolidColorBrush(color);
			}
		}

		public static Brush AddProgressPercentageBar(Brush backgroundBrush, double percent, double transitionSize)
		{
			var statusBrush = new DrawingBrush();
			var drawingGroup = new DrawingGroup();

			var percentageBrush = GetPercentageBrush(percent, transitionSize);

			var backgroundDrawing = new GeometryDrawing(backgroundBrush, null, new RectangleGeometry(new Rect(0, 0, 100, 100)));
			var percentageDrawing = new GeometryDrawing(percentageBrush, null, new RectangleGeometry(new Rect(0, 0, 100, 100)));

			drawingGroup.Children.Add(backgroundDrawing);
			drawingGroup.Children.Add(percentageDrawing);

			statusBrush.Drawing = drawingGroup;

			return statusBrush;
		}

		static Brush GetPercentageBrush(double percent, double transitionSize)
		{
			var gradientStops = new GradientStopCollection
			{
				new GradientStop
				{
					Color = Colors.Black, Offset = percent - transitionSize
				},
				new GradientStop
				{
					Color = Colors.Black, Offset = percent + transitionSize
				},
				new GradientStop
				{
					Color = Colors.Transparent, Offset = percent + transitionSize * 2
				}
			};

			return new LinearGradientBrush(gradientStops, 0);
		}
	}
}
