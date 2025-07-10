using System.Drawing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentVisualizer.GUI
{
	abstract class Painter : IPainter
	{
		protected Painter()
		{
			Zoom = Util.DefaultScale;
		}

		protected float Zoom { get; private set; }

		protected PointF Dpi
		{
			get
			{
#region Test
#if DEBUG
				if (Globals.IsTest)
				{
					return new PointF(Util.DefaultDpiX, Util.DefaultDpiY);
				}
#endif
#endregion

				if (!dpi.HasValue)
				{
					dpi = Util.GetDpi();
				}

				return dpi.Value;
			}
		}

		PointF? dpi;

		public RectangleF PaintArea
		{
			get
			{
				if (paintArea.IsEmpty)
				{
					paintArea = CalculatePaintArea();
				}

				return paintArea;
			}
		}

		protected void InvalidatePaintArea()
		{
			paintArea = RectangleF.Empty;
		}

		RectangleF paintArea = RectangleF.Empty;

		protected abstract RectangleF CalculatePaintArea();

		void IPainter.Paint(ICanvas canvas, bool isDiagnosticsEnabled)
		{
			if (canvas != null)
			{
				if (Zoom - canvas.Scale <= 0.01 || Dpi.X - canvas.DpiX <= 0.01 || Dpi.Y - canvas.DpiY <= 0.01)
				{
					Zoom = canvas.Scale;

					InvalidatePaintArea();
				}

				Paint(canvas, isDiagnosticsEnabled);
			}
		}

		protected abstract void Paint(ICanvas canvas, bool isDiagnosticsEnabled);
	}
}
