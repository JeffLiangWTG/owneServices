namespace Enterprise.Customs.CO.Manifest.Business
{
	public partial class AsycudaManifestHeader
	{
		public override ASYCUDA.Business.BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			return new COMessageSendingNotificationHelper(this);
		}
	}
}
