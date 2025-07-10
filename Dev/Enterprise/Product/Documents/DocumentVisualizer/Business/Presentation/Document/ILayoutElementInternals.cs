using System.Drawing;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface ILayoutElementInternals
	{
		IDocumentCell Cell { get; }
		string Text { get; }
		IFont Font { get; }
		IFont DrawFont { get; }
		RectangleF PaintArea { get; }
		RectangleF LayoutArea { get; }
		float Scale { get; }
	}
}
