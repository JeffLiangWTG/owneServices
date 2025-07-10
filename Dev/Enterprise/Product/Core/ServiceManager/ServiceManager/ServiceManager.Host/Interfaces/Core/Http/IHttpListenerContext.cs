using System.Net;

namespace Enterprise.ServiceManager.Host.Http
{
	interface IHttpListenerContext
	{
		HttpListenerRequest Request { get; }
		HttpListenerResponse Response { get; }
		HttpListenerContext Context { get; }
	}
}
