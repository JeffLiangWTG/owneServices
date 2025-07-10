using System.Web.Http;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class TrustedContextForTest<TRequest, TResponse> : TrustedContext<TRequest, TResponse> where TRequest : TrustedInfo
	{
		public TrustedContextForTest(string product, string systemId, TRequest requestInfo, EdiTrustedSystem trustedSystem, TrustedController controller)
		{
			Product = product;
			SystemId = systemId;
			RequestInfo = requestInfo;
			TrustedSystem = trustedSystem;
			Controller = controller;
		}

		public override IHttpActionResult CreateHttpActionResult()
		{
			throw new System.NotImplementedException();
		}
	}
}
