using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class DataValidatedEvent : IDocumentAwareEvent
	{
		public DataValidatedEvent(IDocument document)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
		}

		/// <summary>
		/// Document for which the View was created
		/// </summary>
		public IDocument Document { get; }
	}
}
