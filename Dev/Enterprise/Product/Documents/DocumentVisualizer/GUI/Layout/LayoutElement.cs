using System.Diagnostics;
using System.Drawing;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	[DebuggerDisplay("{element.ElementType} {Boundaries}")]
	abstract class LayoutElement<T> : ILayoutElement
		where T : IDocumentElement
	{
		protected LayoutElement(T element)
		{
			Argument.NotNull(element, nameof(element));

			this.element = element;
			IsVisible = true;
			InvalidateBoundaries();
		}

		protected readonly T element;

		public virtual IPageView PageView { get; set; }

		IDocumentElement ILayoutElement.Element => element;

		public RectangleF Boundaries { get; set; }

		public bool IsVisible { get; set; }

		void InvalidateBoundaries()
		{
			var drawX = Util.ConvertToPixelsF(dpiX, element.Location.X * scale);
			var drawY = Util.ConvertToPixelsF(dpiX, element.Location.Y * scale);

			var drawWidth = Util.ConvertToPixelsF(dpiY, element.Size.Width * scale);
			var drawHeight = Util.ConvertToPixelsF(dpiY, element.Size.Height * scale);

			Boundaries = new RectangleF(drawX, drawY, drawWidth, drawHeight);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "WINZOR Condition issues")]
		float scale = Util.DefaultScale;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "WINZOR Condition issues")]
		float dpiX = Util.DefaultDpiX;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "WINZOR Condition issues")]
		float dpiY = Util.DefaultDpiY;

		public void Paint(ICanvas canvas, bool isDiagnosticsEnabled)
		{
#if !WINZOR
			if (canvas != null)
			{
				if (scale - canvas.Scale <= 0.01 || dpiX - canvas.DpiX <= 0.01 || dpiY - canvas.DpiY <= 0.01)
				{
					scale = canvas.Scale;
					dpiX = canvas.DpiX;
					dpiY = canvas.DpiY;

					InvalidateBoundaries();
				}

				Painter.Paint(canvas, isDiagnosticsEnabled);
			}
#endif
		}

		protected abstract IPainter Painter { get; }

		void ILayoutElement.Invalidate()
		{
			Invalidate();
		}

		protected virtual void Invalidate()
		{
		}
	}
}
