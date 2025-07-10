using System.Net;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class TrustedMessagingBaseController : TrustedController
	{
		public TrustedMessagingBaseController() : base()
		{
		}

		public TrustedMessagingBaseController(NLogWrapper logger) : base(logger)
		{
		}

		protected void CertificateCore(TrustedContext<CertificateInfo, CertificateResponse> context)
		{
			if (!context.Success)
			{
				return;
			}

			var certOwner = EdiTrustedSystem.Load(Factory, context.RequestInfo.CertificateOwnerProduct, context.RequestInfo.CertificateOwnerSystemId);
			var remoteCertCfg = certOwner?.CertificateConfig;
			if (remoteCertCfg != null && !remoteCertCfg.ETM_CertificateData.IsEmpty)
			{
				context.ResponseInfo = new CertificateResponse() { CertificateData = remoteCertCfg.ETM_CertificateData };
			}
			else
			{
				AddErrorLog("Remote system certificate not found", ((int)HttpStatusCode.NotFound), context.SessionId, routingPath: RoutingPath, product: context.RequestInfo.Product, systemId: context.RequestInfo.SystemId);
			}
		}
	}
}
