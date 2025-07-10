using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class DocumentSendingActionLookups : ZLookups
	{
		public DocumentSendingActionLookups(DocumentSendingAction parent) : base(parent)
		{
		}

		protected new DocumentSendingAction Parent => (DocumentSendingAction)base.Parent;

		// DEV TODO - Lookup for document type 
	}
}
