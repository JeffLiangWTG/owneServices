using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Audit.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit.usp_IndexMaintenance))]
	internal class usp_IndexMaintenanceTest : BiCreateScriptTest
	{
		public void TestExplicitTransactionProperlyManaged()
		{
			AssertEquals("Script has 'SET XACT_ABORT ON'", true, ScriptToTest.Text.IndexOf("SET XACT_ABORT ON", StringComparison.OrdinalIgnoreCase) >= 0);

			AssertExceptionThrown(
				"Attempt to execute procedure within an existing transaction context",
				typeof(SqlException),
				"This procedure must not run in a transaction context.",
				() => TestConnection.ExecuteNonQuery($@"
					DECLARE @ErrorCode int;
					DECLARE @IsIndexRebuilt int;
					DECLARE @CdcHistorySummaryErrorCode int;
					EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name}
						@timer_in_seconds = 0,
						@print_messages = 0,
						@ErrorCode = @ErrorCode OUTPUT,
						@IsIndexRebuilt = @IsIndexRebuilt OUTPUT,
						@CdcHistorySummaryErrorCode = @CdcHistorySummaryErrorCode OUTPUT;")
			);
		}

		protected override string ScriptDbName
		{
			get { return Db.AuditDatabaseName; }
		}

		public void TestDisableStatisticAutoUpdate()
		{
			using (var conn = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(conn, () => Db.Connection.CloseConnection(), Db.AuditDatabaseName, Db.AuditDatabaseName + "-SS"))
			using (((ICurrentDbControl)conn).UseDatabase(Db.AuditDatabaseName))
			{
				conn.ExecuteNonQuery(@"UPDATE STATISTICS biadmin.TableState WITH FULLSCAN");
				AssertAutoRecomputeIsEnabled(conn, true);
				ExecuteIndexMaintenance(conn);
				AssertAutoRecomputeIsEnabled(conn, false);
			}
		}

		void ExecuteIndexMaintenance(DbConnection conn)
		{
			var sqlText = @"DECLARE @ErrorCode int
DECLARE @IsIndexRebuild int
DECLARE @CdcHistorySummaryErrorCode int
EXEC [biadmin].[usp_IndexMaintenance] @timer_in_seconds = NULL, @print_messages = 0, @ErrorCode = @ErrorCode OUTPUT, @IsIndexRebuilt = @IsIndexRebuild OUTPUT, @CdcHistorySummaryErrorCode = @CdcHistorySummaryErrorCode OUTPUT";

			conn.ExecuteNonQuery(sqlText);
		}

		void AssertAutoRecomputeIsEnabled(DbConnection conn, bool isEnabled)
		{
			var sqlText = @"SELECT count(*) AS totalCount, sum(convert(smallint, no_recompute)) AS NoRecompute
FROM sys.stats AS stat
	inner join sys.tables t on t.Object_id = stat.object_id
	inner join sys.schemas s on s.schema_id = t.schema_id
    CROSS APPLY sys.dm_db_stats_properties(stat.object_id, stat.stats_id) AS sp
WHERE t.name = 'TableState' and s.name = 'biadmin'
";
			using (var command = conn.Command(sqlText))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var noRecomputeNumber = reader["NoRecompute"].ToString();
					var totalNumber = reader["totalCount"].ToString();
					if (isEnabled)
					{
						AssertEquals("Auto Recompute should be enabled for TableState.", Int32.Parse(noRecomputeNumber), 0);
					}
					else
					{
						AssertEquals("Auto Recompute should be disabled for TableState.", Int32.Parse(noRecomputeNumber), Int32.Parse(totalNumber));
					}
				}
			}
		}
	}
}

