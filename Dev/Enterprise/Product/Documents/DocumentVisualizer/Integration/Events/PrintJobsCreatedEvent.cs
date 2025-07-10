using System;
using Enterprise.Integration.DocumentEngine;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.DocumentVisualizer.Integration
{
	public sealed class PrintJobsCreatedEvent : IDocumentAwareEvent
	{
		public PrintJobsCreatedEvent(IDocument document, IStmPrintJob[] printJobs)
		{
			Document = document ?? throw new ArgumentNullException(nameof(document));
			PrintJobs = printJobs ?? Array.Empty<IStmPrintJob>();
		}

		public IDocument Document { get; }
		public IStmPrintJob[] PrintJobs { get; }
	}
}
