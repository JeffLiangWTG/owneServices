using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.MainDb;
using Enterprise.ChangeDataCapture.Common;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.MainDb
{
	[TestedType(typeof(CdcScan))]
	class CdcScanTest : DbCreateScriptTest
	{
	}

	class CdcScanNonTransactionedTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestWhenMaxTransactionsParameterIs2()
		{
			using (var connection = Db.NewAdminConnection())
			{
				ResetCdcOnDatabase(connection);
				CreateCdcTable(connection);

				InsertRows(connection, numberOfRows: 10);

				ExecuteCdcScan(connection, maxTrans: 2, maxScans: 1);
				AssertLastProcessedTransactionCount(connection, expectedCount: 2);
				ExecuteCdcScan(connection, maxTrans: 2, maxScans: 1);
				AssertLastProcessedTransactionCount(connection, expectedCount: 2);
				ExecuteCdcScan(connection, maxTrans: 2, maxScans: 1);
				AssertLastProcessedTransactionCount(connection, expectedCount: 2);
				ExecuteCdcScan(connection, maxTrans: 2, maxScans: 1);
				AssertLastProcessedTransactionCount(connection, expectedCount: 2);
				ExecuteCdcScan(connection, maxTrans: 2, maxScans: 1);
				AssertLastProcessedTransactionCount(connection, expectedCount: 2);
				ExecuteCdcScan(connection, maxTrans: 2, maxScans: 1);
				AssertLastProcessedTransactionCount(connection, expectedCount: 0);
			}
		}

		[UseSnapshotProtection]
		public void TestWhenMaxTransactionsParameterIs5()
		{
			using (var connection = Db.NewAdminConnection())
			{
				ResetCdcOnDatabase(connection);
				CreateCdcTable(connection);

				InsertRows(connection, numberOfRows: 10);

				ExecuteCdcScan(connection, maxTrans: 5, maxScans: 1);
				AssertLastProcessedTransactionCount(connection, expectedCount: 5);
				ExecuteCdcScan(connection, maxTrans: 5, maxScans: 1);
				AssertLastProcessedTransactionCount(connection, expectedCount: 5);
				ExecuteCdcScan(connection, maxTrans: 5, maxScans: 1);
				AssertLastProcessedTransactionCount(connection, expectedCount: 0);
			}
		}

		#region Implementation

		#region CDC Database

		void ResetCdcOnDatabase(AdminConnection connection)
		{
			if (CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CdcDatabase.Enable(connection, Db.DatabaseName);
			}
			CdcDatabase.Enable(connection, Db.DatabaseName);
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
			for (int i = 0; i < numberOfRows; i++)
			{
				var sqlText = $"INSERT INTO [Test].[{testTableName}] (Test_PK, Test_Value) SELECT newid(), {i}";
				connection.ExecuteNonQuery(sqlText);
			}
		}

		#endregion

		#region Execute SP

		void ExecuteCdcScan(DbConnection connection, int maxTrans, int maxScans)
		{
			using (var cmd = connection.Command("dbo.CdcScan"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@maxtrans", SqlDbType.Int, maxTrans);
				cmd.AddParameter("@maxscans", SqlDbType.Int, maxScans);
				cmd.ExecuteNonQuery();
			}
		}

		int GetLastProcessedTransactionCount(DbConnection connection)
		{
			var sqlText = @"SELECT TOP 1 tran_count FROM sys.dm_cdc_log_scan_sessions WHERE scan_phase = 'Done' ORDER BY end_time DESC, session_id DESC";
			return Convert.ToInt32(connection.ExecuteScalar(sqlText));
		}

		#endregion

		#region Assert

		void AssertLastProcessedTransactionCount(DbConnection connection, int expectedCount)
		{
			AssertEquals($"{GetCdcLogScanRecords(connection)}\r\n\r\nLast Processed Transaction Count", expectedCount, GetLastProcessedTransactionCount(connection));
		}

		string GetCdcLogScanRecords(DbConnection connection)
		{
			var result = new StringBuilder();
			var sqlText = "SELECT * FROM sys.dm_cdc_log_scan_sessions WHERE scan_phase = 'Done' ORDER BY end_time DESC, session_id DESC";
			using (var cmd = connection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				var columnNames = new List<string>();
				var columnDetails = reader.GetSchemaTable().Rows;
				foreach (DataRow row in columnDetails)
				{
					columnNames.Add(row["ColumnName"].ToString());
				}
				result.AppendLine(string.Join("\t", columnNames));

				while (reader.Read())
				{
					var values = new List<string>();
					foreach (var columnName in columnNames)
					{
						values.Add(reader[columnName].ToString());
					}
					result.AppendLine(string.Join("\t", values));
				}
			}
			return result.ToString();
		}

		#endregion

		#endregion
	}
}
