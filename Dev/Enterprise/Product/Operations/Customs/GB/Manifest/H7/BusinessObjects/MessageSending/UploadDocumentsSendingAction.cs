using System.Linq;

namespace Enterprise.Customs.GB.H7.Business
{
	public class UploadDocumentsSendingAction : EU.H7.Business.UploadDocumentsSendingAction
	{
		readonly AsycudaBill bill;
		public UploadDocumentsSendingAction(EU.H7.Business.AsycudaBill bill) : base(bill)
		{
			this.bill = bill as AsycudaBill;
		}

		protected override void SetDefaultData()
		{
			base.SetDefaultData();
			ShouldSend = false;
		}

		DocumentSendingObjectCollection eDocsCollection;
		public DocumentSendingObjectCollection EDocsCollection
		{
			get
			{
				if (eDocsCollection == null)
				{
					eDocsCollection = new DocumentSendingObjectCollection(bill, this);
					RegisterEditableChildObject(eDocsCollection);
				}
				return eDocsCollection;
			}
		}

		public bool DocumentsAttached => EDocsCollection.Count > 0 && !EDocsCollection.Cast<DocumentSendingObject>().Any(e => e.EDoc.IsEmpty);
	}
}
