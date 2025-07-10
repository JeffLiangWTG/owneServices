using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class DocManagerSchemaSynchronisationWrapperTest : TestCase
	{
		public void TestTemplateDbName()
		{
			var manager = new DummyUpgradeManager();
			var expected = UpgUtils.UpgraderPrefix + "NewTemplateDB_MainDb_SD";

			var wrapper = new DocManagerSchemaSynchroniserForTest(manager, "MainDb_SD001", Db.Connection);
			AssertEquals(expected, wrapper.TemplateDb_Exposed);

			wrapper = new DocManagerSchemaSynchroniserForTest(manager, "MainDb_SD", Db.Connection);
			AssertEquals(expected, wrapper.TemplateDb_Exposed);

			wrapper = new DocManagerSchemaSynchroniserForTest(manager, "MainDb", Db.Connection);
			AssertEquals(expected, wrapper.TemplateDb_Exposed);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestRestoreReadonlyTriggerAfterSyncActionDropReadonlyTrigger()
		{
			var eDocDb = Db.DatabaseName + "_SD999";
			try
			{
				// Arrange	
				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.CreateDatabase(eDocDb);
					((ICurrentDbControl)adminConnection).UseDatabase(eDocDb);
					adminConnection.ExecuteNonQuery(@"
CREATE TABLE dbo.StorageDocs
(
	SC_PK UNIQUEIDENTIFIER NOT NULL,
	SC_IsDeleted CHAR(1) NOT NULL,
)");
					adminConnection.AlterDbWriteableStateForDocManager(eDocDb, false);
				}

				var manager = new DummyUpgradeManager();
				var wrapper = new DocManagerSchemaSynchroniserForTest(manager, eDocDb, Db.Connection);

				// Act
				wrapper.Run();

				// Assert
				Assert(!DocManagerUtils.IsDbWriteableForDocManager(eDocDb));
			}
			finally
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery($"drop database [{eDocDb}]");
				}
			}
		}
	}
}
