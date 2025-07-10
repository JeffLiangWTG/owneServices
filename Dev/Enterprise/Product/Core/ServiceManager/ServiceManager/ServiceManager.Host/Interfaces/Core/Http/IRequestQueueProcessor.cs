using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Http
{
	interface IRequestQueueProcessor : IHttpRequestProcessorInitialiser, IServiceManagerTask
	{ }
}
