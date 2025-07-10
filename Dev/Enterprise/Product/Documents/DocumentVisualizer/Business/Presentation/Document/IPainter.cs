using System.Drawing;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IPainter
	{
		RectangleF PaintArea { get; }
		void Paint(ICanvas canvas, bool isDiagnosticsEnabled);
	}
}
