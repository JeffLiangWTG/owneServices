using System.Diagnostics;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	[DebuggerDisplay("{element.Content} {LayoutRectangle}")]
	sealed class TextLayoutElement : LayoutElement<Text>, ITextLayoutElement
	{
		public TextLayoutElement(Text element)
			: base(element)
		{
		}

		public Text Element => element;
		IText ITextLayoutElement.Text => Element;

		protected override IPainter Painter => painter ?? (painter = new TextPainter(element));

		TextPainter painter;
	}
}
