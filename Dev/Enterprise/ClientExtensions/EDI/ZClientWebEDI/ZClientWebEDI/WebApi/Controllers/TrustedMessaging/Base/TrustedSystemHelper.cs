using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class TrustedSystemHelper
	{
		public static LicenceDatabase GetLicenceDatabase(ITrustedSystemContext context, TrustedInfo info)
		{
			return context.TrustedSystem?.FindTenantDatabaseByTrustedInfo(info);
		}
	}
}
