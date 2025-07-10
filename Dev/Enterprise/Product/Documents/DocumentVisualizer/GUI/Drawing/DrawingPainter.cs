using System.Drawing;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class DrawingPainter : Painter
	{
		public DrawingPainter(Drawing drawing)
		{
			Argument.NotNull(drawing, nameof(drawing));

			this.drawing = drawing;
		}

		readonly Drawing drawing;

		protected override RectangleF CalculatePaintArea()
		{
			var x = Util.ConvertToPixelsF(Dpi.X, drawing.Location.X * Zoom);
			var width = Util.ConvertToPixelsF(Dpi.X, drawing.Size.Width * Zoom);

			var y = Util.ConvertToPixelsF(Dpi.Y, drawing.Location.Y * Zoom);
			var height = Util.ConvertToPixelsF(Dpi.Y, drawing.Size.Height * Zoom);

			return new RectangleF(x, y, width, height);
		}

		protected override void Paint(ICanvas canvas, bool isDiagnosticsEnabled)
		{
			var image = drawing.Image;

			if (image != null)
			{
				canvas.DrawImage(image, PaintArea);
			}
		}
	}
}
