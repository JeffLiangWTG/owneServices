using System;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Integration
{
	public sealed class MessageWithdrawalSentEvent : IDocumentAwareEvent
	{
		public MessageWithdrawalSentEvent(IDocument document)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
		}

		public IDocument Document { get; }
	}
}
