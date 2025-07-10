using System;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Semaphores.Common;

namespace Enterprise.Environment.Semaphore
{
#if DEBUG
	public
#endif
	sealed class ServiceTaskSemaphoreProvider : SemaphoreProvider
	{
		internal ServiceTaskSemaphoreProvider()
		{
		}

		protected override IHeartbeatInfoFactory HeartbeatSessionInfoFactory => lazyHeartbeatInfoFactory.Value;

		protected override TimeSpan HeartbeatDuration => TimeSpan.FromSeconds(Env.Registry.ServiceTaskHeartbeatDurationSeconds);

		protected override IHeartBeatRemoteLogoff LogoffHandler => ObjectFactory.Get<IHeartBeatRemoteLogoff>();

		readonly Lazy<ServiceTaskHeartbeatInfoFactory> lazyHeartbeatInfoFactory = new Lazy<ServiceTaskHeartbeatInfoFactory>(() => new ServiceTaskHeartbeatInfoFactory());

#if DEBUG
		public IDisposable ResetInternalHeartbeatForTest()
		{
			var heartbeat = internalHeartbeat;
			internalHeartbeat = null;
			return new DisposableAction(heartbeat.Dispose);
		}
#endif
	}
}
