using System;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Upgrades;

namespace Enterprise.DbUpgrader.Shared
{
	public interface IDbUpgraderRunner
	{
		ValidationResponse FullUpgrade(IVersionChangeInfo versionInfo, UpgradeInfo softwareUpgrade, UpgraderEvent upgradeEventHandler, Func<BaseUpgradeManager, bool> runUpgradeGui);
		ValidationResponse FullSilentUpgrade(IVersionChangeInfo versionInfo, UpgradeInfo softwareUpgrade, UpgraderEvent upgradeEventHandler);
		void RemoveOldUpgradePackages();
	}
}
