using System;
using Enterprise.Environment;
using Enterprise.Semaphores.Common;

namespace Enterprise.ZArchitecture.Web.GlobalBase
{
	public class WebSemaphoreProvider : SemaphoreProvider
	{
		public WebSemaphoreProvider(IHeartBeatRemoteLogoff heartBeatRemoteLogoffHandler)
		{
			LogoffHandler = heartBeatRemoteLogoffHandler;
		}

		protected override IHeartbeatInfoFactory HeartbeatSessionInfoFactory => new WebHeartbeatInfoFactory();

		protected override TimeSpan HeartbeatDuration => TimeSpan.FromSeconds(Env.Registry.HeartbeatDurationSeconds);

		protected override IHeartBeatRemoteLogoff LogoffHandler { get; }
	}
}
