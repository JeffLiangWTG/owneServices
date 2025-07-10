using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class MoveToNextEditableElementEvent : IDocumentAwareEvent
	{
		public MoveToNextEditableElementEvent(IDocument document, IDynamicContentLayoutElement content, MoveToNextDirection direction)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
			Content = content ?? throw new ArgumentNullException(nameof(content));
			Direction = direction;
		}

		public IDocument Document { get; }
		public IDynamicContentLayoutElement Content { get; }
		public MoveToNextDirection Direction { get; }
	}
}
