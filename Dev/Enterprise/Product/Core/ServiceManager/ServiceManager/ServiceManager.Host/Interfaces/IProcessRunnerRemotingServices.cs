using Enterprise.ServiceManager.Shared;

namespace Enterprise.ServiceManager.Host
{
	public interface IProcessRunnerRemotingServices
	{
		IRunnerCommandQueueProvider CreateRunnerCommandQueueProxy(int? grpcPort);
	}
}
