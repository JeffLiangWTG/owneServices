using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class IDocumentEventsMock : IDocumentEvents
	{
		#region IDocumentEvents Members

		public event DocumentPrintedEventHandler DocumentPrePreviewed;
		public void NotifyDocumentPrePreviewed(DocumentPrintedEventArgs e)
		{
			DocumentPrePreviewed?.Invoke(this, e);
		}

		public event DocumentPrintedEventHandler DocumentPrePrinted;
		public void NotifyDocumentPrePrinted(DocumentPrintedEventArgs e)
		{
			if (DocumentPrePrinted != null)
			{
				DocumentPrePrinted(this, e);
			}
		}

		public event DocumentCancelEventHandler DocumentPrintRequested;
		public void NotifyDocumentPrintRequested(DocumentCancelEventArgs e)
		{
			if (DocumentPrintRequested != null)
			{
				DocumentPrintRequested(this, e);
			}
		}

		public event DocumentPrintedEventHandler DocumentPrinted;
		public void NotifyDocumentPrinted(DocumentPrintedEventArgs e)
		{
			if (DocumentPrinted != null)
			{
				DocumentPrinted(this, e);
			}
		}

		#endregion
	}
}
