using Enterprise.ServiceManager.Shared;

namespace Enterprise.ServiceManager.Host
{
	public interface IGrpcClientSynchronizerFactory
	{
		IGrpcClientSynchronizer Create(GrpcEventHandleNames eventHandleNames);
	}
}
