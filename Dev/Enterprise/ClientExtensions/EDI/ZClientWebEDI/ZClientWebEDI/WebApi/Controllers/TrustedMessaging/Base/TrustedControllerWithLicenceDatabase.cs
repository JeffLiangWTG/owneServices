using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using WTG.TrustedMessaging.MyAccount.Interfaces;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Controllers.TrustedMessaging.Base
{
	public abstract class TrustedControllerWithLicenceDatabase<TRequest> : TrustedController where TRequest : ITrustedInfo
	{
		public TrustedControllerWithLicenceDatabase() : base()
		{
		}

		public TrustedControllerWithLicenceDatabase(NLogWrapper logger) : base(logger)
		{
		}

		protected abstract LicenceDatabase GetLicenceDatabase(ITrustedContext context, TRequest info);
	}
}
