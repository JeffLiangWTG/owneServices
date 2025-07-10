using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public abstract class LoginServiceTrustedSystemBaseController : LoginServiceBaseController<TrustedUserInfo>
	{
		public LoginServiceTrustedSystemBaseController() : base()
		{
		}

		public LoginServiceTrustedSystemBaseController(NLogWrapper logger) : base(logger)
		{
		}

		protected override LicenceDatabase GetLicenceDatabase(ITrustedContext context, TrustedUserInfo info)
		{
			return TrustedSystemHelper.GetLicenceDatabase((ITrustedSystemContext)context, info);
		}
	}
}
