namespace Enterprise.Customs.AR.Manifest.Business
{
	public partial class AsycudaManifestHeader
	{
		public override ASYCUDA.Business.BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			return new ARMessageSendingNotificationHelper(this);
		}
	}
}
