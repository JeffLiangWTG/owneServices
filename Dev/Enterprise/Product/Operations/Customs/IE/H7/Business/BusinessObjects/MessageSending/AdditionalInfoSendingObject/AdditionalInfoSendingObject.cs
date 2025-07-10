using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AdditionalInfoSendingObject : EU.H7.Business.AdditionalInfoSendingObject
	{
		public AdditionalInfoSendingObject(AsycudaBill bill, UploadDocumentsSendingAction action, RequestedDocument document) : base(bill, action, document)
		{
		}

		protected override EU.H7.Business.AdditionalInfoSendingObjectValidation GetNewValidation() => new AdditionalInfoSendingObjectValidation(this);

		public ValidationConfiguration ValidationConfiguration => new ValidationConfiguration();
	}
}
