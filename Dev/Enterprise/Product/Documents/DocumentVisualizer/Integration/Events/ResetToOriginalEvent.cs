using System;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Integration
{
	public sealed class ResetToOriginalEvent : IDocumentAwareEvent
	{
		public ResetToOriginalEvent(IDocument document)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
		}

		public IDocument Document { get; }
	}
}
