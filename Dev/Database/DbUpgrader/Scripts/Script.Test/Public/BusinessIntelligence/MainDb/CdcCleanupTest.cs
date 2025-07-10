using System;
using System.Data;
using System.Globalization;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.MainDb;
using Enterprise.ChangeDataCapture.Common;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.MainDb
{
	[TestedType(typeof(CdcCleanup))]
	class CdcCleanupTest : DbCreateScriptTest
	{
	}

	class CdcCleanupNonTransactionedTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestCdcCleanup()
		{
			using (var connection = Db.NewAdminConnection())
			{
				ResetCdcOnDatabase(connection);
				CreateCdcTable(connection);

				InsertRows(connection, numberOfRows: 10);
				ExecuteCdcScan(connection);

				var initialRecordCount = GetCdcRecordCount(connection);
				AssertEquals($"CDC record count", 10, initialRecordCount);

				var retVal = ExecuteCdcCleanup();
				Assert("CDC cleanup was not successful.", retVal == 0);

				var newRecordCount = GetCdcRecordCount(connection);
				AssertLessThan("CDC table was not cleaned up.", newRecordCount, initialRecordCount);
			}
		}

		[UseSnapshotProtection]
		public void TestCdcCleanup_WithFailedParameter()
		{
			using (var connection = Db.NewAdminConnection())
			{
				ResetCdcOnDatabase(connection);
				CreateCdcTable(connection);

				InsertRows(connection, numberOfRows: 10);
				ExecuteCdcScan(connection);

				var initialRecordCount = GetCdcRecordCount(connection);
				AssertEquals($"CDC record count", 10, initialRecordCount);

				var retVal = ExecuteCleanupProcedureWithCleanupFailedParameter();
				Assert("CDC cleanup was not successful.", retVal == 0);

				var newRecordCount = GetCdcRecordCount(connection);
				AssertLessThan("CDC table was not cleaned up.", newRecordCount, initialRecordCount);
			}
		}

		#region Implementation

		#region CDC Database

		void ResetCdcOnDatabase(AdminConnection connection)
		{
			if (IsDatabaseCdcEnabled(connection))
			{
				CdcDatabase.Disable(connection, Db.DatabaseName);
			}
			CdcDatabase.Enable(connection, Db.DatabaseName);
		}

		bool IsDatabaseCdcEnabled(DbConnection connection)
		{
			var sqlText = $"SELECT is_cdc_enabled FROM sys.databases WHERE name = '{Db.DatabaseName}'";
			return Convert.ToBoolean(connection.ExecuteScalar(sqlText));
		}

		#endregion

		#region CDC Table

		const string testTableName = "TestTable";

		void CreateCdcTable(DbConnection connection)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'Test')
	EXEC ('CREATE SCHEMA [Test]');

CREATE TABLE [Test].[{0}]
(
	Test_PK uniqueidentifier NOT NULL,
	Test_Value int
)
ALTER TABLE [Test].[{0}]
ADD CONSTRAINT [PK_UX__{0}_Test_PK] PRIMARY KEY NONCLUSTERED ([Test_PK] ASC)

EXEC sys.sp_cdc_enable_table @source_schema = N'Test',
	@source_name = N'{0}',
	@role_name = NULL,
	@capture_instance = N'Test_{0}'

EXEC sys.sp_cdc_scan @continuous = 0", testTableName);

			connection.ExecuteNonQuery(sqlText);
		}

		void InsertRows(DbConnection connection, int numberOfRows)
		{
			for (int i = 0; i < numberOfRows - 1; i++)
			{
				var sqlText = $"INSERT INTO [Test].[{testTableName}] (Test_PK, Test_Value) SELECT newid(), {i}";
				connection.ExecuteNonQuery(sqlText);
			}

			Thread.Sleep(30);

			var lastRowSqlText = $"INSERT INTO [Test].[{testTableName}] (Test_PK, Test_Value) SELECT newid(), {numberOfRows - 1}";
			connection.ExecuteNonQuery(lastRowSqlText);
		}

		#endregion

		#region Execute SP

		void ExecuteCdcScan(DbConnection connection)
		{
			using (var cmd = connection.Command("dbo.CdcScan"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.ExecuteNonQuery();
			}
		}

		int ExecuteCdcCleanup()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			using (var cmd = conn.Command("dbo.CdcCleanup"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@capture_instance", SqlDbType.NVarChar, "Test_TestTable");
				cmd.AddParameter("@low_water_mark", SqlDbType.Binary, GetLowWaterMark(conn));
				cmd.AddParameter("@threshold", SqlDbType.Int, 5000);
				cmd.AddOutputParameter("@retVal", SqlDbType.Int, 32, 0, 0, null);

				cmd.ExecuteNonQuery();

				return Convert.ToInt32(cmd.GetParameterValue("@retVal"));
			}
		}

		int ExecuteCleanupProcedureWithCleanupFailedParameter()
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			using (var cmd = conn.Command("dbo.CdcCleanup"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@capture_instance", SqlDbType.NVarChar, "Test_TestTable");
				cmd.AddParameter("@low_water_mark", SqlDbType.Binary, GetLowWaterMark(conn));
				cmd.AddParameter("@threshold", SqlDbType.Int, 5000);
				cmd.AddOutputParameter("@retVal", SqlDbType.Int, 32, 0, 0, null);
				cmd.AddOutputParameter("@cleanup_failed", SqlDbType.Bit, 32, 0, 0, 0);
				cmd.ExecuteNonQuery();

				var result = Convert.ToInt32(cmd.GetParameterValue("@retVal"));
				var cleanupFailedRaw = cmd.GetParameterValue("@cleanup_failed");
				var cleanupFailed = Convert.ToBoolean(cleanupFailedRaw == DBNull.Value ? false : cleanupFailedRaw);
				if (cleanupFailed)
				{
					throw new Exception($"Cleanup of capture instance failed");
				}

				return result;
			}
		}

		byte[] GetLowWaterMark(DbConnection connection)
		{
			var sqlText = @"declare @low_water_mark binary(10);
select top 1 @low_water_mark = start_lsn from cdc.lsn_time_mapping order by tran_end_time desc
select coalesce(@low_water_mark, 0x0)";
			return (byte[])connection.ExecuteScalar(sqlText);
		}

		#endregion

		#region Assert

		int GetCdcRecordCount(DbConnection connection)
		{
			var sqlText = "select count(*) from cdc.Test_TestTable_CT";
			return Convert.ToInt32(connection.ExecuteScalar(sqlText));
		}

		#endregion

		#endregion
	}
}
