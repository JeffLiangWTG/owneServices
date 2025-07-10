using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public interface ITaskRunRequestProcessor
	{
		TaskRunRequestResult ProcessRunRequest(ITaskRunRequest request, IServiceRunner runner);
	}
}
