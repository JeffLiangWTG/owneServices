using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class DocumentViewCreatedEvent : IDocumentAwareEvent
	{
		public DocumentViewCreatedEvent(IDocument document)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
		}

		/// <summary>
		/// Document for which the View was created
		/// </summary>
		public IDocument Document { get; }
	}
}
