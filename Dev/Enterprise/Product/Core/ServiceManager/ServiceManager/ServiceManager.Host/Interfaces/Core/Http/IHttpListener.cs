using System;
using System.Net;

namespace Enterprise.ServiceManager.Host.Http
{
	interface IHttpListener : IDisposable
	{
		IAsyncResult BeginGetContext(AsyncCallback callback, object state);
		HttpListenerContext EndGetContext(IAsyncResult result);
	}
}
