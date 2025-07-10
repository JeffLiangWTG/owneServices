using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class ERequestTrustedSystemBaseController : ERequestBaseController<ERequestInfo>
	{
		public ERequestTrustedSystemBaseController() : base()
		{
		}

		public ERequestTrustedSystemBaseController(NLogWrapper logger) : base(logger)
		{
		}

		protected override LicenceDatabase GetLicenceDatabase(ITrustedContext context, ERequestInfo info)
		{
			return TrustedSystemHelper.GetLicenceDatabase((ITrustedSystemContext)context, info);
		}

		protected void GetAutoLoginUrlCore(TrustedContext<ERequestInfo, AutoLoginResponse> context)
		{
			GetAutoLoginUrlCore(context, context.RequestInfo);
		}

		protected void UploadCore(TrustedContext<ERequestInfo, bool> context)
		{
			UploadCore(context, context.RequestInfo);
		}
	}
}
