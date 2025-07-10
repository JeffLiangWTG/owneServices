using System;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public interface IGrpcClientSynchronizer : IDisposable
	{
		bool WaitForServerReadySignal(IRunnableServiceTask task, IGrpcPortResolver portOpened, IProcess process, TimeSpan timeout);
	}
}
