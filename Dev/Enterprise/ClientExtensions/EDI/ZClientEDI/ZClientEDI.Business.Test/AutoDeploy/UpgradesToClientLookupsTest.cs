using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.AutoDeploy.Business.Test
{
	internal class UpgradesToClientLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBuilds()
		{
			UpgradesToClient upgrade = Factory.New<UpgradesToClient>();
			AssertNotNull(upgrade.Lookups.Builds);
		}

		public void TestUpgradeMethodsList()
		{
			UpgradesToClient upgrade = Factory.New<UpgradesToClient>();
			AssertNotNull(upgrade.Lookups.UpgradeMethodsList);
		}

		public void TestUpgradeStatusList()
		{
			UpgradesToClient upgrade = Factory.New<UpgradesToClient>();
			AssertNotNull(upgrade.Lookups.UpgradeStatusList);
		}
	}
}