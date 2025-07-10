using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	static class LayoutElementFactory
	{
		public static ILayoutElement Create(IDocumentElement element)
		{
			ILayoutElement layout = null;

			switch (element.ElementType)
			{
				case ElementType.Line:
					var line = (Line)element;
					layout = new LineLayoutElement(line);
					break;

				case ElementType.Rectangle:
					var rectangle = (Rectangle)element;
					layout = new RectangleLayoutElement(rectangle);
					break;

				case ElementType.Drawing:
					var drawing = (Drawing)element;
					layout = new DrawingLayoutElement(drawing);
					break;

				case ElementType.Text:
					var text = (Text)element;
					layout = new TextLayoutElement(text);
					break;

				case ElementType.DynamicContent:
					var editor = (DynamicContent)element;
					layout = new DynamicContentLayoutElement(editor);
					break;
			}

			return layout;
		}
	}
}