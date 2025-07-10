using System.Text;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	class DatabasePadlockTest : TestCase
	{
		public void TestLockDatabaseResources_OtherConnectionHasLock()
		{
			using (var otherConnection = Db.NewExtraConnectionToMainDb())
			using (var adminConn = Db.NewAdminConnection())
			using (adminConn.TemporarySetLockTimeout(123))
			{
				otherConnection.BeginTransaction();
				var tableName = otherConnection.ExecuteScalar<string>("SELECT TOP(1) name FROM sys.tables WHERE schema_id = SCHEMA_ID('dbo') AND is_ms_shipped = 0 ORDER BY name;");
				otherConnection.ExecuteNonQuery($"DELETE TOP (1) FROM dbo.[{tableName}]");

				var logger = new StringBuilder();
				bool lockResult;
				adminConn.BeginTransaction();
				try
				{
					lockResult = new DatabasePadlock().LockDatabaseResources(
						adminConn,
						new string[] { Db.DatabaseName },
						(message) => logger.AppendLine(message));
				}
				finally
				{
					adminConn.RollbackTransaction();
				}
				AssertEquals(false, lockResult);
				var expected =
					$"Unable to lock database resources. Please stop all other processes connected to the database {Db.DatabaseName}.\r\n[{Db.DatabaseName}].[dbo].[{tableName}] locked by SPID";
				AssertStartsWith("log", expected, logger.ToString());

				adminConn.RollbackTransaction();

				var finalLockTimeout = (int)adminConn.ExecuteScalar("select @@LOCK_TIMEOUT");
				AssertEquals("initial LOCK_TIMEOUT is preserved", 123, finalLockTimeout);
			}
		}

		[UseSnapshotProtection]
		public void TestLockDatabaseResources_ExtendedPropertyExists()
		{
			// Arrange
			using (var adminConn = Db.NewAdminConnection())
			{
				DataUtils.AddTableExtendedProperty(adminConn, new DbSchemaTable(Db.DatabaseName, "dbo", "StmALog"), DatabasePadlock.LockPropertyName, "true");

				adminConn.BeginTransaction();

				// Act
				// Assert
				AssertNoExceptionThrown(() => new DatabasePadlock().LockDatabaseResources(adminConn, new string[] { Db.DatabaseName }));
			}
		}

		[UseSnapshotProtection]
		public void TestSchemaModificationLock()
		{
			using (var otherConnection = Db.NewExtraConnectionToMainDb())
			using (var adminConn = Db.NewAdminConnection())
			{
				otherConnection.BeginTransaction();
				var tableName = otherConnection.ExecuteScalar<string>("SELECT TOP(1) name FROM sys.tables WHERE schema_id = SCHEMA_ID('dbo') AND is_ms_shipped = 0 ORDER BY name;");
				var table = new CargoWise.Data.SqlServer.DbSchemaTable(Db.DatabaseName, "dbo", tableName);

				var sql = $@"
FROM
	sys.dm_tran_locks
	JOIN sys.objects AS tab WITH (NOLOCK) ON tab.object_id = resource_associated_entity_id
	JOIN sys.schemas AS sch WITH (NOLOCK) ON sch.schema_id = tab.schema_id
WHERE 1=1
	AND sch.name = N'{table.SchemaName}'
	AND tab.name = N'{table.TableName}'
	AND resource_database_id = DB_ID(N'{table.DatabaseName}')
	AND request_mode = 'SCH-M'
	AND request_status = 'GRANT'
";

				if (CdcDatabase.IsEnabled(adminConn, Db.DatabaseName))
				{
					CdcDatabase.Disable(adminConn, Db.DatabaseName);
				}

				AssertEquals("SCH-M lock exists?", false, adminConn.Exists(sql));

				var propertyName = "LockDatabaseResources";
				DataUtils.AddTableExtendedProperty(otherConnection, table, propertyName, "1");
				DataUtils.DropTableExtendedProperty(otherConnection, table, propertyName);

				AssertEquals("SCH-M lock exists?", true, adminConn.Exists(sql));

				adminConn.BeginTransaction();

				var logger = new StringBuilder();
				AssertEquals(false, new DatabasePadlock().LockDatabaseResources(adminConn, new string[] { Db.DatabaseName }, (message) => logger.AppendLine(message)));

				var expected =
					$"Unable to lock database resources. Please stop all other processes connected to the database {Db.DatabaseName}.\r\n[{Db.DatabaseName}].[dbo].[{tableName}] locked by SPID";
				AssertStartsWith("log", expected, logger.ToString());
			}
		}

		public void TestAllLocks()
		{
			using (var adminConn = Db.NewAdminConnection())
			{
				adminConn.BeginTransaction();

				var logger = new StringBuilder();
				CombineAssertions(() =>
				{
					AssertEquals(true, new DatabasePadlock().LockDatabaseResources(adminConn, new string[] { Db.DatabaseName }, (message) => logger.AppendLine(message)));
					AssertEquals("", logger.ToString());
				});
			}
		}
	}
}
