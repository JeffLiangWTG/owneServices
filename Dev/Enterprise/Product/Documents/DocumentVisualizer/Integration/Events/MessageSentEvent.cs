using System;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Integration
{
	public sealed class MessageSentEvent : IDocumentAwareEvent
	{
		public MessageSentEvent(IDocument document)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
		}

		public IDocument Document { get; }
	}
}
