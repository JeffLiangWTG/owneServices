using System.Drawing;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface ILayoutElement
	{
		IDocumentElement Element { get; }

		IPageView PageView { get; set; }

		RectangleF Boundaries { get; }

		bool IsVisible { get; set; }

		void Paint(ICanvas canvas, bool isDiagnosticsEnabled);
		void Invalidate();
	}
}
