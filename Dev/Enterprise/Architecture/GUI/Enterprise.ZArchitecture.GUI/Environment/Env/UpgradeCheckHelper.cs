using Enterprise.ZArchitecture.Core;

namespace Enterprise.Environment
{
	public interface IUpgradeCheckHelper
	{
		public bool HasBeenUpgraded();
	}

	class UpgradeCheckHelper : IUpgradeCheckHelper
	{
		public bool HasBeenUpgraded()
		{
			var upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
			var currentVersionInDatabase = upgradeManager.QueryCurrentVersion();

			return currentVersionInDatabase != null && !ReleaseInfo.Instance.VersionNumber.ToVersion().Equals(currentVersionInDatabase.Version);
		}
	}
}
