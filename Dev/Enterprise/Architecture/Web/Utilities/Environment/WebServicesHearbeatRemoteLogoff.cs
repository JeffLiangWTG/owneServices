using System;
using Enterprise.Semaphores.Common;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment
{
	internal class WebServicesHearbeatRemoteLogoff : IHeartBeatRemoteLogoff
	{
		public int OnRemoteLogoffCalls { get; private set; }

		public int OnRemoteUpgradeLogoffCalls { get; private set; }

		public void OnRemoteLogoff() => ++OnRemoteLogoffCalls;

		public void OnRemoteUpgradeLogoff(DateTime upgradeDateTimeUtc, Func<bool> updateExists) => ++OnRemoteUpgradeLogoffCalls;
	}
}