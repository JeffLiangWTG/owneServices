using System;
using CargoWise.Data;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class StartupCheckForthcomingSystemUpgradeTest : TransactionedTestCase
	{
		public void TestGetSystemUpgradeDateTime()
		{
			var task = new StartupCheckForthcomingSystemUpgradeForTest();
			DateTime upgradeDateTimeUtc = ZDateTime.BrettsBirthday.ToDateTime();

			DataUtils.DropDbExtendedProperty(TestConnection, DataUtils.DateTimeForthcomingUpgradeExtPty);
			upgradeDateTimeUtc = task.GetSystemUpgradeDateTime_Exposed();
			Assert(upgradeDateTimeUtc == DateTime.MinValue);

			DateTime upgradeRegValue = DateTime.UtcNow.AddMinutes(10);
			DateTime upgradeRefreshedRegValue = DateTime.UtcNow;

			string forthcomingUpgPtyValue = String.Format("{0}|{1}", SqlFormatInfo.ToSqlDateTimeString(upgradeRegValue), SqlFormatInfo.ToSqlDateTimeString(upgradeRefreshedRegValue));
			DataUtils.SaveDbExtendedProperty(TestConnection, DataUtils.DateTimeForthcomingUpgradeExtPty, forthcomingUpgPtyValue);
			upgradeDateTimeUtc = DateTime.MinValue;
			upgradeDateTimeUtc = task.GetSystemUpgradeDateTime_Exposed();
			Assert(Math.Abs((upgradeDateTimeUtc - upgradeRegValue).TotalSeconds) < 1);

			//expired #1
			upgradeRegValue = DateTime.UtcNow.AddMinutes(-2);
			upgradeRefreshedRegValue = DateTime.UtcNow;

			forthcomingUpgPtyValue = String.Format("{0}|{1}", SqlFormatInfo.ToSqlDateTimeString(upgradeRegValue), SqlFormatInfo.ToSqlDateTimeString(upgradeRefreshedRegValue));
			DataUtils.SaveDbExtendedProperty(TestConnection, DataUtils.DateTimeForthcomingUpgradeExtPty, forthcomingUpgPtyValue);
			upgradeDateTimeUtc = DateTime.MinValue;
			upgradeDateTimeUtc = task.GetSystemUpgradeDateTime_Exposed();
			Assert(upgradeDateTimeUtc == DateTime.MinValue);

			//expired #2
			upgradeRegValue = DateTime.UtcNow.AddMinutes(10);
			upgradeRefreshedRegValue = DateTime.UtcNow.AddMinutes(-5);

			forthcomingUpgPtyValue = String.Format("{0}|{1}", SqlFormatInfo.ToSqlDateTimeString(upgradeRegValue), SqlFormatInfo.ToSqlDateTimeString(upgradeRefreshedRegValue));
			DataUtils.SaveDbExtendedProperty(TestConnection, DataUtils.DateTimeForthcomingUpgradeExtPty, forthcomingUpgPtyValue);
			upgradeDateTimeUtc = DateTime.MinValue;
			upgradeDateTimeUtc = task.GetSystemUpgradeDateTime_Exposed();
			Assert(upgradeDateTimeUtc == DateTime.MinValue);

			//invalid format
			DataUtils.SaveDbExtendedProperty(TestConnection, DataUtils.DateTimeForthcomingUpgradeExtPty, "invalid format");
			upgradeDateTimeUtc = DateTime.Now;
			upgradeDateTimeUtc = task.GetSystemUpgradeDateTime_Exposed();
			Assert(upgradeDateTimeUtc == DateTime.MinValue);
		}
	}
}
