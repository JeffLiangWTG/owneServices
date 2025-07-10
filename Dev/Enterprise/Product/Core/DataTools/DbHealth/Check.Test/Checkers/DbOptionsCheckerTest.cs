using System.Linq;
using CargoWise.BrandManager;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	sealed class DbOptionsCheckerTest : CheckerTestCaseBase
	{
		[UseSnapshotProtection]
		public void TestCheckOptions_DelayedDurabilityAndCdcEnabled()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				if (!CdcDatabase.IsEnabled(adminConnection, Db.DatabaseName))
				{
					CdcDatabase.Enable(adminConnection, Db.DatabaseName);
				}
				var dbChecker = new DbOptionsChecker();
				dbChecker.IsDelayedDurabilityEnabledWithCDCQuery = "SELECT 1";
				var warningList = new DbHealthWarningListForTesting();
				var logger = new LoggerForTest();

				dbChecker.Check(Db.Connection, warningList, logger);

				Assert("If you add more checks to this class then this test must account for them.", logger.LogEntries.Count() == 1);
				AssertCollectionContains(
					"Should log a warning when the delayed_durability option is true",
					$"{Db.DatabaseName} database has Delayed Durability enabled, contact {BrandingFactory.Instance.ProductSupportName} to disable Delayed Durability on your database. Delayed Durability is not compatible with Change Data Capture.",
					logger.LogEntries);
			}
		}

		[UseSnapshotProtection]
		public void TestCheckOptions()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				if (!CdcDatabase.IsEnabled(adminConnection, Db.DatabaseName))
				{
					CdcDatabase.Enable(adminConnection, Db.DatabaseName);
				}
				var dbChecker = new DbOptionsChecker();
				var warningList = new DbHealthWarningListForTesting();
				var logger = new LoggerForTest();

				dbChecker.Check(Db.Connection, warningList, logger);

				Assert("If you add more checks to this class then this test must account for them.", !logger.LogEntries.Any());
			}
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new DbOptionsChecker();
		}
	}
}
