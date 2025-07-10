using System;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Integration
{
	public sealed class DocumentHardRefreshEvent : IDocumentAwareEvent
	{
		public DocumentHardRefreshEvent(IDocument document)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
		}

		public IDocument Document { get; }
	}
}
