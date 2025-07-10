using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class SystemUpgradeWarningTest : TransactionedTestCase
	{
		public void TestSetUpgradeComingRegistryAndSleep()
		{
			var upgWarning = new SystemUpgradeWarning(TestConnection);

			DateTime regDateTimeUpgrade;
			DateTime regDateRefreshed;

			int diffMinUpgrade;
			int diffMinRefresh;
			int pausePeriod;

			DataUtils.DropDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty);
			DeleteSystemUpgradeWarningPeriodRegistryItem();
			pausePeriod = upgWarning.SetSystemUpgradeWarningPeriodAndPause(new DummyLoggerForTest());
			GetForthcomingUpgPtyValue(out regDateTimeUpgrade, out regDateRefreshed);
			diffMinUpgrade = Math.Abs((regDateTimeUpgrade - DateTime.UtcNow).Minutes);
			diffMinRefresh = Math.Abs((regDateRefreshed - DateTime.UtcNow).Minutes);
			Assert(diffMinUpgrade == 0 && diffMinRefresh == 0 && pausePeriod == 0);

			upgWarning.ClearSystemUpgradeWarningPeriod();
			Assert(DataUtils.LoadDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty) == null);

			upgWarningPeriodRegItem.SaveValue(22, Db.Connection);
			pausePeriod = upgWarning.SetSystemUpgradeWarningPeriodAndPause(new DummyLoggerForTest());
			GetForthcomingUpgPtyValue(out regDateTimeUpgrade, out regDateRefreshed);
			diffMinUpgrade = Math.Abs((regDateTimeUpgrade - DateTime.UtcNow).Minutes);
			diffMinRefresh = Math.Abs((regDateRefreshed - DateTime.UtcNow).Minutes);
			Assert(diffMinRefresh == 0 && pausePeriod == 22);
			Assert(diffMinUpgrade == 21 || diffMinUpgrade == 22 || diffMinUpgrade == 23);

			upgWarningPeriodRegItem.SaveValue(99, Db.Connection);
			upgWarning.ClearSystemUpgradeWarningPeriod();
			Assert(DataUtils.LoadDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty) == null);
		}

		void GetForthcomingUpgPtyValue(out DateTime datetTimeUpgrade, out DateTime dateTimeRefreshed)
		{
			datetTimeUpgrade = dateTimeRefreshed = DateTime.MinValue;

			var value = DataUtils.LoadDbExtendedProperty(Db.Connection, DataUtils.DateTimeForthcomingUpgradeExtPty);

			if (!string.IsNullOrWhiteSpace(value))
			{
				var values = value.Split('|');
				datetTimeUpgrade = SqlFormatInfo.FromSqlDateTime(values[0]);
				dateTimeRefreshed = SqlFormatInfo.FromSqlDateTime(values[1]);
			}
		}

		void DeleteSystemUpgradeWarningPeriodRegistryItem()
		{
			var sqlText = string.Format("DELETE dbo.StmData WHERE SD_Name = '{0}';", upgWarningPeriodRegItem.ItemName);
			Db.Connection.ExecuteNonQuery(sqlText);
		}

		readonly IntDbRegistryItem upgWarningPeriodRegItem = DbRegistry.GetNewSystemUpgradeWarningPeriodItem();
	}
}
