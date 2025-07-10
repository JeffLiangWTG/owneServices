using System.Drawing;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IText : IDocumentElement
	{
		IFont Font { get; }
		RectangleF Padding { get; }
		Alignment HAlignment { get; }
		Alignment VAlignment { get; }
		bool Wrap { get; }
		string Content { get; }
	}
}