using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class AEMessageSendingNotificationHelper : MessageSendingNotificationHelper
{
	public AEMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
	{
	}

	protected override ZString GetExtraMessageSendingNotificationCore()
	{
		var notificationMessage = Res.GetString("4221432f-a03e-4abf-a6fd-d1f3101e1bd7", "To create a message for the United Arab Emirates you must link the current logged in company to an AE Branch in the Registry, Customs>Country or Region Specific>United Arab Emirates>Default Branch for Manifest Submission. This is so that the correct OrgProxy is selected for determining the Manifest EDI Profile.");
		return ManifestMessageExtensions.AeOrgProxyForManifestMessage(header.Factory) == null ? (ZString)notificationMessage : base.GetExtraMessageSendingNotificationCore();
	}
}
