namespace Enterprise.Customs.IE.H7.Business
{
	public class UploadDocumentsSendingActionParent<TMessageSendingObject> : EU.H7.Business.UploadDocumentsSendingActionParent<TMessageSendingObject> where TMessageSendingObject : UploadDocumentsSendingAction
	{
		public UploadDocumentsSendingActionParent(EU.H7.Business.AsycudaManifestHeader manifestHeader) : base(manifestHeader)
		{
		}
	}
}
