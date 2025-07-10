using Enterprise.DbUpgrader.Shared;
using Enterprise.Upgrades;

namespace Enterprise.Startup.Testing
{
	public class TestDbUpgraderDirector : DbUpgraderDirector
	{
		public TestDbUpgraderDirector() : base() { }

		public TestDbUpgraderDirector(ValidationResponse upgradeResult, bool loginResult)
			: base()
		{
			doUpgradeResult = upgradeResult;
			loginForUpgradeResult = loginResult;
		}

		protected override ValidationResponse DoUpgrade()
		{
			doUpgradeResult.Successful = true;
			return doUpgradeResult;
		}
		readonly ValidationResponse doUpgradeResult;

		protected override bool LoginForUpgrade()
		{
			return loginForUpgradeResult;
		}
		readonly bool loginForUpgradeResult;

		public void SetSoftwareUpgrade(UpgradeInfo softwareUpgrade)
		{
			this.softwareUpgrade = softwareUpgrade;
		}

		internal new string UpgradeLogDirectory => base.UpgradeLogDirectory;
	}
}
