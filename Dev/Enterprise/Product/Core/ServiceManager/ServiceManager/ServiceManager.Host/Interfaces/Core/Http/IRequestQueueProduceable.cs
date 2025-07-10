using System.Threading;

namespace Enterprise.ServiceManager.Host.Http
{
	interface IRequestQueueProduceable : IRequestQueueCountable
	{
		void Add(IHttpListenerContext request, CancellationToken cancellationToken);
	}
}
