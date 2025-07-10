using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Http
{
	interface IHttpRequestProcessorInitialiser
	{
		void ConfigureHttpRequestProcessor(ITaskScheduler scheduler, ITaskStatusProvider statusProvider, IActionQueue actionQueue);
	}
}
