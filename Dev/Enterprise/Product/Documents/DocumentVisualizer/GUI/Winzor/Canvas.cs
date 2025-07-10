using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	public sealed class Canvas : ICanvas
	{
		public Canvas(BGraphics graphics, float scale)
		{
		}

		public float Scale => throw new NotImplementedException();

		public float DpiX => throw new NotImplementedException();

		public float DpiY => throw new NotImplementedException();

		public void DrawImage(Image image, RectangleF area)
		{
			throw new NotImplementedException();
		}

		public void DrawLine(IPen pen, PointF start, PointF end)
		{
			throw new NotImplementedException();
		}

		public void DrawRectangle(IPen pen, RectangleF rectangle)
		{
			throw new NotImplementedException();
		}

		public void DrawString(IFont font, StringFormat stringFormat, RectangleF rectangle, float size, string text, bool printJustified)
		{
			throw new NotImplementedException();
		}

		public void FillPolygon(Color color, PointF[] points)
		{
			throw new NotImplementedException();
		}

		public void FillRectangle(Color color, RectangleF rectangle)
		{
			throw new NotImplementedException();
		}

		public void FillRectangle(Color topColor, Color bottomBolor, RectangleF rectangle, LinearGradientMode mode)
		{
			throw new NotImplementedException();
		}

		public float ShrinkToFit(string content, SizeF area, IFont font, StringFormat stringFormat)
		{
			throw new NotImplementedException();
		}
	}
}
