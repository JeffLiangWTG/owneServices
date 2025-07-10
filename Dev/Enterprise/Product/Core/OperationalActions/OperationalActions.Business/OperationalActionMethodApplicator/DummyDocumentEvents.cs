using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Services.OperationalActions.Business
{
	class DummyDocumentEvents : IDocumentEvents
	{
		public event DocumentCancelEventHandler DocumentPrintRequested;
		public event DocumentPrintedEventHandler DocumentPrePreviewed;
		public event DocumentPrintedEventHandler DocumentPrePrinted;
		public event DocumentPrintedEventHandler DocumentPrinted;

		public bool OnDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			if (DocumentPrintRequested != null)
			{
				DocumentPrintRequested(sender, e);
			}

			return e.Cancel;
		}

		public void OnDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
		{
			DocumentPrePreviewed?.Invoke(sender, e);
		}

		public void OnDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
		{
			if (DocumentPrePrinted != null)
			{
				DocumentPrePrinted(sender, e);
			}
		}

		public void OnDocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			if (DocumentPrinted != null)
			{
				DocumentPrinted(sender, e);
			}
		}
	}
}
