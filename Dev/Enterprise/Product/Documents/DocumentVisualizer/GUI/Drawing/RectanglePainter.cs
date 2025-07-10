using System.Drawing;
using System.Drawing.Drawing2D;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class RectanglePainter : Painter
	{
		public RectanglePainter(Rectangle rectangle)
		{
			Argument.NotNull(rectangle, nameof(rectangle));

			this.rectangle = rectangle;
		}

		readonly Rectangle rectangle;

		protected override RectangleF CalculatePaintArea()
		{
			var x = Util.ConvertToPixelsF(Dpi.X, rectangle.Location.X * Zoom);
			var width = Util.ConvertToPixelsF(Dpi.X, rectangle.Size.Width * Zoom);

			var y = Util.ConvertToPixelsF(Dpi.Y, rectangle.Location.Y * Zoom);
			var height = Util.ConvertToPixelsF(Dpi.Y, rectangle.Size.Height * Zoom);

			return new RectangleF(x, y, width, height);
		}

		protected override void Paint(ICanvas canvas, bool isDiagnosticsEnabled)
		{
			if (!rectangle.SolidColor.IsEmpty)
			{
				canvas.FillRectangle(rectangle.SolidColor, PaintArea);
			}
			else
			{
				canvas.FillRectangle(rectangle.GradientColors.Item1, rectangle.GradientColors.Item2, PaintArea, LinearGradientMode.Vertical);
			}
		}
	}
}
