using System;
using Enterprise.Semaphores.Common;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment
{
	class WebServicesSemaphoreProvider : SemaphoreProvider
	{
		readonly IHeartbeatInfoFactory heartbeatInfoFactory = new WebServicesHeartbeatInfoFactory();

		protected override IHeartbeatInfoFactory HeartbeatSessionInfoFactory => heartbeatInfoFactory;

		protected override TimeSpan HeartbeatDuration => new TimeSpan(0, 0, 1);

		protected override IHeartBeatRemoteLogoff LogoffHandler => new WebServicesHearbeatRemoteLogoff();
	}
}
