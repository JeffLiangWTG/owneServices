using CargoWise.Bi.Maintenance;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;
using NUnit.Framework;

namespace CargoWise.Bi.Product.DataLoad.Testing
{
	class CdcHistorySummaryPopulatorTest : TestCase
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRun()
		{
			var auditDbName = Db.AuditDatabaseName;
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(mainDbConnection, Db.DatabaseName);
				var logger = new LoggerForTest();
				var cdchistorySummaryPopulator = new CdcHistorySummaryPopulator(mainDbConnection, logger);
				AssertNoExceptionThrown(() => cdchistorySummaryPopulator.Run());
			}
		}
	}
}
