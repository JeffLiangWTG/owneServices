using System.Linq;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class TableRebuildPersisterDatabasesTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestAnotherDatabases()
		{
			var persister = new TableRebuildPersister(Db.Connection);
			persister.Clear();

			// another database
			var databaseName = Db.DatabaseName + "_RefDb_CMR_AAA";
			var table = new DbSchemaTable(databaseName, "dbo", "CMRAHECCCode");

			AssertEquals("Database does not exist", true, persister.MarkTableAsRequiringRebuild(table));
			AssertEquals("Database does not exist", 0, persister.GetTablesToRebuild().Count());

			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.CreateDbIfNotExists(connection, databaseName);

					AssertEquals("Table does not exist", true, persister.MarkTableAsRequiringRebuild(table));
					AssertEquals("Table does not exist", 0, persister.GetTablesToRebuild().Count());

					using (((ICurrentDbControl)Db.Connection).UseDatabase(databaseName))
					{
						Db.Connection.ExecuteNonQuery("CREATE TABLE dbo.CMRAHECCCode (id int);");
						AssertEquals(false, persister.MarkTableAsRequiringRebuild(table));
					}

					AssertEquals(true, persister.MarkTableAsRequiringRebuild(table));

					var tables = persister.GetTablesToRebuild().ToList();
					AssertEquals(1, tables.Count);
					AssertEquals("[" + databaseName + "].[dbo].[CMRAHECCCode]", tables[0].ToString());
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, databaseName);
				}
			}
		}
	}
}
