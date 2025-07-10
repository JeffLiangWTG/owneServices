using System.Threading;

namespace Enterprise.ServiceManager.Host.Http
{
	interface IRequestQueueConsumable
	{
		IHttpListenerContext Take(CancellationToken cancellationToken);
	}
}
