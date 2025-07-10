using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class BeforeEditEvent : EditEvent
	{
		public BeforeEditEvent(IDocument document, IDynamicContentLayoutElement content)
			: base(document, content)
		{
		}
	}
}
