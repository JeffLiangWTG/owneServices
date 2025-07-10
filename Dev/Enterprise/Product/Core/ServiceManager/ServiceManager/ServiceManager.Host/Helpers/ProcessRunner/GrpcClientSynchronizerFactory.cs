using Enterprise.ServiceManager.Shared;

namespace Enterprise.ServiceManager.Host
{
	public class GrpcClientSynchronizerFactory : IGrpcClientSynchronizerFactory
	{
		public IGrpcClientSynchronizer Create(GrpcEventHandleNames eventHandleNames)
		{
			return new GrpcClientSynchronizer(eventHandleNames);
		}
	}
}
