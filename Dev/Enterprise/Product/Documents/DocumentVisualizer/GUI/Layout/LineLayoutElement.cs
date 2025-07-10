using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class LineLayoutElement : LayoutElement<Line>
	{
		public LineLayoutElement(Line element)
			: base(element)
		{
		}

		public Line Element => element;

		protected override IPainter Painter
		{
			get { return painter ?? (painter = new LinePainter(element)); }
		}

		LinePainter painter;
	}
}
