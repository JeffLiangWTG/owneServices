using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class BeginEditEvent : EditEvent
	{
		public BeginEditEvent(IDocument document, IDynamicContentLayoutElement content)
			: base(document, content)
		{
		}
	}
}
