using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public abstract class EditEvent : IDocumentAwareEvent
	{
		protected EditEvent(IDocument document, IDynamicContentLayoutElement content)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
			Content = content;
		}

		public IDocument Document { get; }
		public IDynamicContentLayoutElement Content { get; }
	}
}
