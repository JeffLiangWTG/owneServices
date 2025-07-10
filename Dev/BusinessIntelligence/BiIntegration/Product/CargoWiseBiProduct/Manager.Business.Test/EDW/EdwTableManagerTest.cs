using CargoWise.Bi.Common;
using CargoWise.Bi.Product.Manager.Business;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Testing.EDW
{
	class EdwTableManagerTest : TestCase
	{
		[UseSnapshotProtection(Db.EdwDatabaseSuffix)]
		public void TestRunRunInitialLoad()
		{
			using var connection = Db.NewAdminConnection(Db.EdwDatabaseName);
			connection.ExecuteNonQuery($@"
TRUNCATE TABLE [{BiConstants.BiAdminSchemaName}].[MasterState]

INSERT INTO [{BiConstants.BiAdminSchemaName}].[StagingTableState](SourceTableName, CurrentMaxLsn, CurrentState, InitialLoadRequired)
VALUES ('dbo.Test', 0x3, 'Loaded', 0)
INSERT INTO [{BiConstants.BiAdminSchemaName}].[TransformTableState](ModelTableName, CurrentState, InitialLoadRequired)
VALUES ('Test', 'Loaded', 0)
INSERT INTO [{BiConstants.BiAdminSchemaName}].[ModelTableState](ModelSchemaname, ModelTableName, CurrentState, InitialLoadRequired)
VALUES ('TestSchema', 'Test', 'Loaded', 0)
INSERT INTO [{BiConstants.BiAdminSchemaName}].[CustomTableState](ModelTableName, CurrentState, InitialLoadRequired)
VALUES ('Test', 'Loaded', 0)
");
			BiMasterState.SetParameter(connection, "TestParameter", "0");

			var edwInfo = new EdwInformation(Db.ServerName, Db.EdwDatabaseName);
			edwInfo.RefreshInfo();

			CombineAssertions("TestRunRunInitialLoad", () => 
			{
				AssertNoExceptionThrown(() => EdwTableManager.RunInitialLoad(["Test"], ["Test"], ["Test"], ["Test"]));
				Assert("Master state cleared", !connection.Exists($"from {BiConstants.BiAdminSchemaName}.MasterState"));

				Assert("Staging table state cleared", !connection.Exists($"from {BiConstants.BiAdminSchemaName}.StagingTableState where SourceTableName = 'Test' AND (InitialLoadRequired = 0 OR CurrentState <> 'New' OR CurrentMaxLsn <> 0)"));
				Assert("Base table state cleared", !connection.Exists($"from {BiConstants.BiAdminSchemaName}.TransformTableState where ModelTableName = 'Test' AND (InitialLoadRequired = 0 OR CurrentState <> 'New')"));
				Assert("Aggregate table state cleared", !connection.Exists($"from {BiConstants.BiAdminSchemaName}.ModelTableState where ModelTableName = 'Test' AND (InitialLoadRequired = 0 OR CurrentState <> 'New')"));
				Assert("Custom table state cleared", !connection.Exists($"from {BiConstants.BiAdminSchemaName}.CustomTableState where ModelTableName = 'Test' AND (InitialLoadRequired = 0 OR CurrentState <> 'New')"));
			});
		}
	}
}
