using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class RectangleLayoutElement : LayoutElement<Rectangle>
	{
		public RectangleLayoutElement(Rectangle element)
			: base(element)
		{
		}

		public Rectangle Element => element;

		protected override IPainter Painter
		{
			get { return painter ?? (painter = new RectanglePainter(element)); }
		}

		RectanglePainter painter;
	}
}
