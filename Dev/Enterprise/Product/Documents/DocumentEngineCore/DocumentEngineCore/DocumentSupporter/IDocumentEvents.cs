using System;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	#region Interfaces

	public interface IDocumentEvents
	{
		event DocumentCancelEventHandler DocumentPrintRequested;
		event DocumentPrintedEventHandler DocumentPrePreviewed;
		event DocumentPrintedEventHandler DocumentPrePrinted;
		event DocumentPrintedEventHandler DocumentPrinted;
	}

	public interface IDocumentEventsHandler
	{
		DocumentSupporter DocumentSupporter { get; set; }
		bool CanHandleMenuItem(IStmMenuItem menuItem);
		void HandleDocumentPrintRequested(object sender, DocumentCancelEventArgs e);
		void HandleDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e);
		void HandleDocumentPrePrinted(object sender, DocumentPrintedEventArgs e);
		void HandleDocumentPrinted(object sender, DocumentPrintedEventArgs e);
	}

	public interface IDocumentEventsForMenu : IDocumentEvents
	{
		void NotifyDocumentPrintRequested(IStmMenuItem menuItem);
		void NotifyDocumentPrePreviewed(DocumentPrintedEventArgs e);
		void NotifyDocumentPrePrinted(DocumentPrintedEventArgs e);
		void NotifyDocumentPrinted(DocumentPrintedEventArgs e);
		bool CancelPrintRequest { get; }
	}

	#endregion

	#region Delegates

	public delegate void DocumentPrintedEventHandler(object sender, DocumentPrintedEventArgs e);
	public delegate void DocumentCancelEventHandler(object sender, DocumentCancelEventArgs e);

	#endregion

	#region EventArgs Classes

	public class DocumentEventArgs : EventArgs
	{
		public DocumentEventArgs(IStmMenuItem menuItem)
		{
			this.MenuItem = menuItem;
		}

		public IStmMenuItem MenuItem;
	}

	public class DocumentCancelEventArgs : DocumentEventArgs
	{
		public DocumentCancelEventArgs(IStmMenuItem menuItem)
			: base(menuItem)
		{
		}

		public bool Cancel;
	}

	public class DocumentPrintedEventArgs : DocumentEventArgs
	{
		public DocumentPrintedEventArgs(DeliveryInstructionDestination deliveryInstructionDestinationType, IStmMenuItem menuItem, bool isDraft = false, object source = null)
			: base(menuItem)
		{
			DeliveryInstructionDestinationType = deliveryInstructionDestinationType;
			IsDraft = isDraft;
			Source = source;
		}

		public DeliveryInstructionDestination DeliveryInstructionDestinationType;

		public readonly bool IsDraft;

		public readonly object Source;
	}

	#endregion
}
