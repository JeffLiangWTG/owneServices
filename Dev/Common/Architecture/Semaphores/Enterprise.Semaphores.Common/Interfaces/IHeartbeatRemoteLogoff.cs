using System;

namespace Enterprise.Semaphores.Common
{
	public interface IHeartBeatRemoteLogoff
	{
		void OnRemoteLogoff();
		void OnRemoteUpgradeLogoff(DateTime upgradeDateTimeUtc, Func<bool> updateExists);
	}
}
