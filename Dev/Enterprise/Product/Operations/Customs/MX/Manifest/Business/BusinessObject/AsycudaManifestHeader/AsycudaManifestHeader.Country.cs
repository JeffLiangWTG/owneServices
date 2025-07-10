using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public partial class AsycudaManifestHeader
	{
		public override BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			return new MXMessageSendingNotificationHelper(this);
		}
	}
}
