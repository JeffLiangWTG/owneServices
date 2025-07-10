using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class DrawingLayoutElement : LayoutElement<Drawing>
	{
		public DrawingLayoutElement(Drawing element)
			: base(element)
		{
		}
		public Drawing Element => element;

		protected override IPainter Painter
		{
			get { return painter ?? (painter = new DrawingPainter(element)); }
		}

		DrawingPainter painter;
	}
}