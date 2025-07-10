using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocumentEventsForMenuForTesting : IDocumentEventsForMenu
	{
		public bool CancelPrintRequest => false;

		public void NotifyDocumentPrePreviewed(DocumentPrintedEventArgs e)
		{
			DocumentPrePreviewed?.Invoke(this, e);
		}

		public void NotifyDocumentPrePrinted(DocumentPrintedEventArgs e)
		{
			if (DocumentPrePrinted != null)
			{
				DocumentPrePrinted(this, e);
			}
		}

		public void NotifyDocumentPrintRequested(IStmMenuItem menuItem)
		{
			if (DocumentPrintRequested != null)
			{
			}
		}

		public void NotifyDocumentPrinted(DocumentPrintedEventArgs e)
		{
			if (DocumentPrinted != null)
			{
				DocumentPrinted(this, e);
			}
		}

		public event DocumentPrintedEventHandler DocumentPrePreviewed;
		public event DocumentPrintedEventHandler DocumentPrePrinted;
		public event DocumentCancelEventHandler DocumentPrintRequested;
		public event DocumentPrintedEventHandler DocumentPrinted;
	}
}
