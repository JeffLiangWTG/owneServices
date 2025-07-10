using System.Web.Http;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	//protocol independent context
	public abstract class TrustedContext<TRequest, TResponse> : ITrustedContext<TRequest, TResponse>, ITrustedSystemContext where TRequest : TrustedInfo
	{
		public string Product { get; protected set; }
		public string SystemId { get; protected set; }
		public string SessionId { get; protected set; }
		public bool Success { get; set; }
		public TRequest RequestInfo { get; protected set; }
		public TResponse ResponseInfo { get; set; }
		public EdiTrustedSystem TrustedSystem { get; protected set; }
		public ErrorMessages Messages { get; set; }
		public TrustedController Controller { get; protected set; }

		public abstract IHttpActionResult CreateHttpActionResult();
	}
}
