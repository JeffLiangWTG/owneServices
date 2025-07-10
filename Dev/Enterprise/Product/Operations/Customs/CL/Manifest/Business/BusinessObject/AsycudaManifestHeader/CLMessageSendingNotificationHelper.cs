using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class CLMessageSendingNotificationHelper : MessageSendingNotificationHelper
	{
		public CLMessageSendingNotificationHelper(AsycudaManifestHeader header) : base(header)
		{
		}

		public override ZString GetNotifications()
		{
			var result = base.GetNotifications();

			return result.IsEmpty && GlbStaff.CurrentUser.GS_EmailAddress.IsEmpty ? (ZString)ValidationConstants.MissingEmailAddress : result;
		}

		protected override ZString GetExtraMessageSendingNotificationCore()
		{
			return GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Chile ? (ZString)ValidationsConstants.MustBeLoggedInUnderCLToSendCLMessages : base.GetExtraMessageSendingNotificationCore();
		}
	}
}
