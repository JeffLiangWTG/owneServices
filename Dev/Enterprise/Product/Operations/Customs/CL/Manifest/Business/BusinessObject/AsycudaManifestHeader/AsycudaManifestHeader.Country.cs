namespace Enterprise.Customs.CL.Manifest.Business
{
	public partial class AsycudaManifestHeader
	{
		public override ASYCUDA.Business.BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			return new CLMessageSendingNotificationHelper(this);
		}
	}
}
