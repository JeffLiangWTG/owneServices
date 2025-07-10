using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.H7.Business
{
	public class UploadDocumentsSendingAction : EU.H7.Business.UploadDocumentsSendingAction
	{
		public UploadDocumentsSendingAction(AsycudaBill bill) : base(bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		protected override void SetDefaultData()
		{
			ShouldSend = true;
			Action = AISOutgoingMessageTypeList.Codes.DocumentsReceived;
		}

		public override EU.H7.Business.MessageSender CreateSender()
		{
			return new IEMessageSender(this);
		}

		protected override EU.H7.Business.AdditionalInfoSendingObjectCollection CreateAddInfoCollection() => new AdditionalInfoSendingObjectCollection(bill, this);
	}
}
