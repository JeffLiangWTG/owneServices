using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class DummyCanvas : ICanvas
	{
		public DummyCanvas(Action<string> logFunc)
		{
			this.logFunc = logFunc;
			Zoom = Util.DefaultScale;
		}

		readonly Action<string> logFunc;

		public float Zoom { get; set; }

		void AppendLog(string message)
		{
			if (logFunc != null)
			{
				logFunc(message);
			}
		}

		float ICanvas.Scale
		{
			get { return Zoom; }
		}

		float ICanvas.DpiX
		{
			get { return Util.DefaultDpiX; }
		}

		float ICanvas.DpiY
		{
			get { return Util.DefaultDpiY; }
		}

		void ICanvas.DrawImage(Image image, RectangleF area)
		{
			AppendLog("Drawing image");
		}

		void ICanvas.DrawLine(IPen pen, PointF start, PointF end)
		{
			AppendLog("Drawing line");
		}

		void ICanvas.DrawRectangle(IPen pen, RectangleF rectangle)
		{
			AppendLog("Drawing rectangle");
		}

		void ICanvas.FillRectangle(Color color, RectangleF rectangle)
		{
			AppendLog(string.Format("Filling rectangle with {0} color", color.Name));
		}

		public void FillRectangle(Color topColor, Color bottomColor, RectangleF rectangle, LinearGradientMode mode)
		{
			AppendLog(string.Format("Filling rectangle with {0} gradient {1} and {2}", mode, topColor.Name, bottomColor.Name));
		}

		void ICanvas.FillPolygon(Color color, PointF[] points)
		{
			AppendLog(string.Format("Filling plygon with {0} color", color.Name));
		}

		void ICanvas.DrawString(IFont font, StringFormat stringFormat, RectangleF rectangle, float size, string text, bool justified)
		{
			AppendLog(string.Format("Drawing text '{0}'", text));
		}

		public float ShrinkToFit(string content, SizeF area, IFont font, StringFormat stringFormat)
		{
			return font?.Size ?? 12f;
		}
	}
}
