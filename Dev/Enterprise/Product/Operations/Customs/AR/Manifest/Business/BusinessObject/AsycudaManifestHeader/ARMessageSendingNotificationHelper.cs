using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class ARMessageSendingNotificationHelper : MessageSendingNotificationHelper
	{
		public ARMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		protected override ZString GetExtraMessageSendingNotificationCore()
		{
			return GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Argentina ? (ZString)ValidationsConstants.MustBeLoggedInUnderARToSendARMessages : base.GetExtraMessageSendingNotificationCore();
		}
	}
}
