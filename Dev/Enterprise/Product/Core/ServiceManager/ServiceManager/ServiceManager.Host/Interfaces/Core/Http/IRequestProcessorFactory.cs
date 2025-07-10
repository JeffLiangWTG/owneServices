using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Http
{
	interface IRequestProcessorFactory
	{
		IRequestProcessor CreateRequestProcessor(IActionQueue actionQueue, ITaskScheduler scheduler, ITaskStatusProvider statusProvider);
	}
}
