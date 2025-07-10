using System;
using System.Net;
using Enterprise.ServiceManager.Host.Http;

namespace Enterprise.ServiceManager.Host
{
	class HttpListenerWrapperFactory : IHttpListenerFactory
	{
		public virtual IHttpListener Create(string uriPrefix)
		{
			return new HttpListenerWrapper(uriPrefix);
		}
	}

	class HttpListenerWrapper : IHttpListener
	{
		public HttpListenerWrapper(string uriPrefix)
		{
			listener = new HttpListener
			{
				IgnoreWriteExceptions = true,
			};
			listener.Prefixes.Add(uriPrefix);
			listener.Start();
		}

		public virtual IAsyncResult BeginGetContext(AsyncCallback callback, object state)
		{
			return listener.BeginGetContext(callback, state);
		}

		public HttpListenerContext EndGetContext(IAsyncResult result)
		{
			return listener.EndGetContext(result);
		}

		readonly HttpListener listener;

		#region IDisposable

		public void Dispose()
		{
			listener.Stop();
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				listener.Close();
			}
		}
		#endregion
	}
}
