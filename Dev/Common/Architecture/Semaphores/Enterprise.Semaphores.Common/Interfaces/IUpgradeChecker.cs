using System;

namespace Enterprise.Semaphores.Common
{
	public interface IUpgradeChecker
	{
		DateTime CheckForUpgrade(bool isForegroundThread, TimeSpan upgradeCheckDuration);
	}
}
