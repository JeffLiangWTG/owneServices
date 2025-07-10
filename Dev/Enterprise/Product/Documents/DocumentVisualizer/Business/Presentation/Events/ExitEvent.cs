using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class ExitEvent : IDocumentAwareEvent
	{
		public ExitEvent(IDocument document)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
		}

		public IDocument Document { get; }
	}
}
