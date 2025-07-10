using System.Drawing;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class LinePainter : Painter
	{
		public LinePainter(Line line)
		{
			Argument.NotNull(line, nameof(line));

			this.line = line;
		}

		readonly Line line;

		protected override RectangleF CalculatePaintArea()
		{
			var x = Util.ConvertToPixelsF(Dpi.X, line.Location.X * Zoom);
			var width = Util.ConvertToPixelsF(Dpi.X, line.Size.Width * Zoom);

			var y = Util.ConvertToPixelsF(Dpi.Y, line.Location.Y * Zoom);
			var height = Util.ConvertToPixelsF(Dpi.Y, line.Size.Height * Zoom);

			return new RectangleF(x, y, width, height);
		}

		protected override void Paint(ICanvas canvas, bool isDiagnosticsEnabled)
		{
			var start = new PointF(PaintArea.X, PaintArea.Y);
			var end = new PointF(PaintArea.X + PaintArea.Width, PaintArea.Y + PaintArea.Height);

			canvas.DrawLine(line.Pen, start, end);
		}
	}
}
