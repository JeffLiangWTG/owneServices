using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	public class DbUpgradeCaptionsTest : TestCase
	{
		public void TestDbUpgradeCaptionsDefaultValue()
		{
			// Arrange
			// Act
			var upgradeCaptions = new DbUpgradeCaptions();

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals($"{BrandingFactory.Instance.ProductName} Upgrade In Progress", upgradeCaptions.UpgradeInProgressTitle);
				AssertEquals($"Please wait, the database is in the process of being upgraded. {BrandingFactory.Instance.ProductName} will restart automatically after the upgrade is complete.", upgradeCaptions.UpgradeInProgressMessage);
			});
		}

		public void TestRefresh()
		{
			// Arrange
			const string arabic = "AR-AE";
			var defaultCaptions = new DbUpgradeCaptions();

			using (ObjectFactory.Get<IResourceStrings>().TemporarilySwitchLanguage(arabic))
			{
				var upgradeCaptions = new DbUpgradeCaptions();

				// Act
				upgradeCaptions.Refresh();

				// Assert
				AssertNotEquals(defaultCaptions.UpgradeInProgressMessage, upgradeCaptions.UpgradeInProgressMessage);
				AssertNotEquals(defaultCaptions.UpgradeInProgressTitle, upgradeCaptions.UpgradeInProgressTitle);
				AssertNotEquals(defaultCaptions.PurgeInProgressMessage, upgradeCaptions.PurgeInProgressMessage);
				AssertNotEquals(defaultCaptions.PurgeInProgressTitle, upgradeCaptions.PurgeInProgressTitle);
				AssertNotEquals(defaultCaptions.Exit, upgradeCaptions.Exit);
			}
		}
	}
}
