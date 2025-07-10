using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class DocumentSendingObjectValidation : SupportingDocSendingObjectValidation
	{
		public DocumentSendingObjectValidation(AutoSupportingDocSendingObject parent) : base(parent)
		{
		}

		protected override void CheckDocumentType() { }

		protected override void CheckLocalReferenceNumber() { }
	}
}
