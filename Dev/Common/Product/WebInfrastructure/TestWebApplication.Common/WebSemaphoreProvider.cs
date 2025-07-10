using System;
using Enterprise.Semaphores.Common;

namespace Enterprise.Web.TestWebApplication.Common
{
	public class WebSemaphoreProvider : SemaphoreProvider
	{
		public WebSemaphoreProvider(IHeartBeatRemoteLogoff heartBeatRemoteLogoffHandler)
		{
			LogoffHandler = heartBeatRemoteLogoffHandler;
		}

		protected override IHeartbeatInfoFactory HeartbeatSessionInfoFactory => new WebHeartbeatInfoFactory();

		protected override TimeSpan HeartbeatDuration => new TimeSpan(0, 0, 1);

		protected override IHeartBeatRemoteLogoff LogoffHandler { get; }
	}
}
