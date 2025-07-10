using System.Net;
using Enterprise.ServiceManager.Host.Http;

namespace Enterprise.ServiceManager.Host
{
	class HttpListenerContextWrapper : IHttpListenerContext
	{
		public HttpListenerContextWrapper(HttpListenerContext context)
		{
			Context = context;
		}
		public virtual HttpListenerRequest Request => Context.Request;

		public virtual HttpListenerResponse Response => Context.Response;

		public HttpListenerContext Context { get; }
	}
}
