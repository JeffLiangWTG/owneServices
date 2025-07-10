using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using Enterprise.DbHealth.Shared.Test;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.IndexUpdate.Testing
{
	[UseSnapshotProtection]
	sealed class TableRebuilderTest : TestCase
	{
		public void TestTableRebuilder()
		{
			if (Db.Connection.ServerEdition != DbConnection.SqlServerEdition.EnterpriseDeveloper)
			{
				Assert(true);
				return;
			}

			var db_main = Db.Connection.CurrentDatabase;
			var db_SD = db_main + "_SD_ForTest";

			DbWorker.CreateTestDb(db_SD);

			try
			{
				var persister = new TableRebuildPersister(Db.Connection);
				persister.Clear();
				persister.MarkTableAsRequiringRebuild(new DbSchemaTable(db_SD, "dbo", "Table1"));
				persister.MarkTableAsRequiringRebuild(new DbSchemaTable(db_SD, "dbo", "Table2"));

				var timeBeforeTask = AdoTestUtils.DbServerTime();
				Thread.Sleep(1000);

				using (var connection = Db.NewAdminConnection())
				{
					var logger = new TestServiceLogger();
					new TableRebuilder(logger).Run(connection);
					AssertEquals("logger.Count", 2, logger.Count);
					AssertEquals(String.Format("Information|Rebuilding table [{0}].[dbo].[Table1]", db_SD), logger[0]);
					AssertEquals(String.Format("Information|Rebuilding table [{0}].[dbo].[Table2]", db_SD), logger[1]);

					Assert("Modify date of Table1 should change when rebuilt", (DateTime)connection.ExecuteScalar(String.Format("SELECT modify_date FROM [{0}].sys.tables WHERE name = 'Table1'", db_SD)) > timeBeforeTask);
					Assert("Modify date of Table2 should change when rebuilt", (DateTime)connection.ExecuteScalar(String.Format("SELECT modify_date FROM [{0}].sys.tables WHERE name = 'Table2'", db_SD)) > timeBeforeTask);
					AssertEquals(0, persister.GetTablesToRebuild().Count());

					var logger2 = new TestServiceLogger();
					new TableRebuilder(logger2).Run(connection);
					AssertEquals("logger.Count", 0, logger2.Count);
				}
			}
			finally
			{
				DbWorker.DropTestDbIfExists(db_SD);
			}
		}
	}
}
