using System.Web.Http;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Interfaces;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public interface ITrustedContext
	{
		public string Product { get; }
		public bool Success { get; set; }
		public ErrorMessages Messages { get; set; }
		public TrustedController Controller { get; }

		public IHttpActionResult CreateHttpActionResult();
	}

	public interface ITrustedContext<TResponse> : ITrustedContext
	{
		public TResponse ResponseInfo { get; set; }
	}

	public interface ITrustedContext<TRequest, TResponse> : ITrustedContext<TResponse> where TRequest : ITrustedInfo
	{
		public TRequest RequestInfo { get; }
	}
}
