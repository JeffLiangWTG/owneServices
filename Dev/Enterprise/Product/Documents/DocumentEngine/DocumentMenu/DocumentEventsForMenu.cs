using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine
{
	public class DocumentEventsForMenu : IDocumentEventsForMenu
	{
		public DocumentEventsForMenu()
		{
		}

		public DocumentEventsForMenu(BusinessObject businessObject)
		{
			if (businessObject is IDocumentSupportable documentSupportable)
			{
				documentSupportable.DocumentSupporter.Initialise(this);
			}
		}

		public event DocumentCancelEventHandler DocumentPrintRequested;
		public event DocumentPrintedEventHandler DocumentPrePreviewed;
		public event DocumentPrintedEventHandler DocumentPrePrinted;
		public event DocumentPrintedEventHandler DocumentPrinted;
		public void NotifyDocumentPrintRequested(IStmMenuItem menuItem)
		{
			cancelPrintRequest = false;
			var args = new DocumentCancelEventArgs(menuItem);
			if (DocumentPrintRequested != null)
			{
				DocumentPrintRequested(this, args);
				cancelPrintRequest = args.Cancel;
			}
		}

		public void NotifyDocumentPrePreviewed(DocumentPrintedEventArgs e)
		{
			DocumentPrePreviewed?.Invoke(this, e);
		}

		public void NotifyDocumentPrePrinted(DocumentPrintedEventArgs e)
		{
			DocumentPrePrinted?.Invoke(this, e);
		}

		public void NotifyDocumentPrinted(DocumentPrintedEventArgs e)
		{
			DocumentPrinted?.Invoke(this, e);
		}

		public bool CancelPrintRequest
		{
			get { return cancelPrintRequest; }
		}
		bool cancelPrintRequest;
	}
}