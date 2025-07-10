using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class EndEditEvent : EditEvent
	{
		public EndEditEvent(IDocument document, IDynamicContentLayoutElement content = null)
			: base(document, content)
		{
		}
	}
}
