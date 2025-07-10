using System;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Core.Environment.Semaphores;
using Enterprise.Semaphores.Common;

namespace Enterprise.Environment.Semaphore
{
	class EnterpriseSemaphoreProvider : SemaphoreProvider
	{
		protected override TimeSpan HeartbeatDuration
		{
			get { return TimeSpan.FromSeconds(Env.Registry.HeartbeatDurationSeconds); }
		}

		protected override IHeartbeatInfoFactory HeartbeatSessionInfoFactory
		{
			get { return heartbeatInfoFactory; }
		}

		readonly IHeartbeatInfoFactory heartbeatInfoFactory = new EnterpriseHeartbeatInfoFactory();

		protected override IHeartBeatRemoteLogoff LogoffHandler
		{
			get { return ObjectFactory.Get<IHeartBeatRemoteLogoff>(); }
		}

		protected override IWindowsTimer HeartBeatWindowsTimer => System.Environment.UserInteractive ? ObjectFactory.Get<IWindowsTimer>() : null;
	}
}
