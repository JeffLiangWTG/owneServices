using System.Drawing;
using System.Drawing.Drawing2D;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface ICanvas
	{
		float Scale { get; }
		float DpiX { get; }
		float DpiY { get; }

		void DrawImage(Image image, RectangleF area);
		void DrawLine(IPen pen, PointF start, PointF end);
		void DrawRectangle(IPen pen, RectangleF rectangle);
		void FillRectangle(Color color, RectangleF rectangle);
		void FillRectangle(Color topColor, Color bottomBolor, RectangleF rectangle, LinearGradientMode mode);
		void FillPolygon(Color color, PointF[] points);
		void DrawString(IFont font, StringFormat stringFormat, RectangleF rectangle, float size, string text, bool printJustified);

		float ShrinkToFit(string content, SizeF area, IFont font,StringFormat stringFormat);
	}
}
