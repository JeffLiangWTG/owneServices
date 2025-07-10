using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class ResetEvent : IDocumentAwareEvent
	{
		public ResetEvent(IDocument document, string storeName)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
			StoreName = storeName ?? throw new ArgumentNullException(nameof(storeName));
		}

		public IDocument Document { get; }
		public string StoreName { get; }
	}
}
