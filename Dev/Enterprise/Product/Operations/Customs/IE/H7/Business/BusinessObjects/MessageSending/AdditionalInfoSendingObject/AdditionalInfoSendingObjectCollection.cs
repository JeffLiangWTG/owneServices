using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AdditionalInfoSendingObjectCollection : EU.H7.Business.AdditionalInfoSendingObjectCollection
	{
		public AdditionalInfoSendingObjectCollection(AsycudaBill bill, UploadDocumentsSendingAction action) : base(bill, action)
		{
			this.bill = bill;
			this.action = action;
		}

		readonly AsycudaBill bill;

		readonly UploadDocumentsSendingAction action;

		protected override EU.H7.Business.AdditionalInfoSendingObject CreateAdditionalInfoSendingObject(RequestedDocument document) =>
			new AdditionalInfoSendingObject(bill, action, document);
	}
}
