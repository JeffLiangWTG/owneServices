using System;
using System.Collections.Concurrent;
using System.Threading;
using Enterprise.ServiceManager.Host.Http;

namespace Enterprise.ServiceManager.Host
{
	sealed class RequestQueue : IRequestQueueProduceable, IRequestQueueConsumable, IDisposable
	{
		public RequestQueue()
		{
			httpRequestQueue = new BlockingCollection<IHttpListenerContext>(new ConcurrentQueue<IHttpListenerContext>());
		}

		public void Add(IHttpListenerContext request, CancellationToken cancellationToken)
		{
			httpRequestQueue.Add(request, cancellationToken);
		}

		public IHttpListenerContext Take(CancellationToken cancellationToken)
		{
			return httpRequestQueue.Take(cancellationToken);
		}

		public int Count => httpRequestQueue.Count;

		public void Dispose()
		{
			httpRequestQueue.Dispose();
		}

		readonly BlockingCollection<IHttpListenerContext> httpRequestQueue;
	}
}
