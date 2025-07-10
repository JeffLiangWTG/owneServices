using System;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.usp_IndexMaintenance))]
	internal class usp_IndexMaintenanceTransactionedTest : BiCreateScriptTest
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
					EXEC {ScriptDbName}.{ScriptToTest.SchemaName}.{ScriptToTest.Name}
						@timer_in_seconds = 0,
						@print_messages = 0,
						@ErrorCode = @ErrorCode OUTPUT,
						@IsIndexRebuilt = @IsIndexRebuilt OUTPUT;")
			);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}

	internal class usp_IndexMaintenanceTest : TestCase
	{
		public void TestRun()
		{
			using (var conn = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(conn, () => Db.Connection.CloseConnection(), Db.EdwDatabaseName, Db.EdwDatabaseName + "-SS"))
			using (((ICurrentDbControl)conn).UseDatabase(Db.EdwDatabaseName))
			{
				CreateTestSchema(conn);
				AssertCompressedIndex(conn, false);
				ExecuteIndexMaintenance(conn);
				AssertCompressedIndex(conn, true);
			}
		}

		#region Implementation

		void CreateTestSchema(DbConnection conn)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
if not exists (select null from sys.schemas where name = 'test')
	EXEC('create schema Test');

if object_id('Test.AGG__TestTable') IS NOT NULL
	EXEC('drop table Test.AGG__TestTable')

create table [Test].[AGG__TestTable]
(
	TestTableID bigint identity(1,1),
	TestValue int
)
create clustered columnstore index [cci_Test_AGG__TestTable] ON [Test].[AGG__TestTable] WITH (DROP_EXISTING = OFF);

insert into [{0}].[ModelTableState] (ModelSchemaName, ModelTableName)
values('Test', 'AGG__TestTable')

insert into [Test].[AGG__TestTable] (TestValue)
values(1)",
				BiConstants.BiAdminSchemaName);

			conn.ExecuteNonQuery(sqlText);
		}

		void AssertCompressedIndex(DbConnection conn, bool compressed)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
if exists (select *
	from  sys.column_store_row_groups
	where object_id = OBJECT_ID(N'[Test].[AGG__TestTable]')
	 and state_description = 'OPEN')
	select 0
else
	select 1
",
				BiConstants.BiAdminSchemaName);

			AssertEquals("Index reorganized?", compressed, Convert.ToBoolean(conn.ExecuteScalar(sqlText)));
		}

		void ExecuteIndexMaintenance(DbConnection conn)
		{
			var sqlText = @"DECLARE @ErrorCode int
DECLARE @IsIndexRebuild int
EXEC [biadmin].[usp_IndexMaintenance] @timer_in_seconds = NULL, @print_messages = 0, @ErrorCode = @ErrorCode OUTPUT, @IsIndexRebuilt = @IsIndexRebuild OUTPUT";

			conn.ExecuteNonQuery(sqlText);
		}
		#endregion

		public void TestDisableStatisticAutoUpdate()
		{
			using (var conn = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(conn, () => Db.Connection.CloseConnection(), Db.EdwDatabaseName, Db.EdwDatabaseName + "-SS"))
			using (((ICurrentDbControl)conn).UseDatabase(Db.EdwDatabaseName))
			{
				conn.ExecuteNonQuery(@"UPDATE STATISTICS biadmin.MasterState WITH FULLSCAN");
				AssertAutoRecomputeIsEnabled(conn, true);
				ExecuteIndexMaintenance(conn);
				AssertAutoRecomputeIsEnabled(conn, false);
			}
		}

		void AssertAutoRecomputeIsEnabled(DbConnection conn, bool isEnabled)
		{
			var sqlText = @"SELECT count(*) AS totalCount, sum(convert(smallint, no_recompute)) AS NoRecompute
FROM sys.stats AS stat
	inner join sys.tables t on t.Object_id = stat.object_id
	inner join sys.schemas s on s.schema_id = t.schema_id
    CROSS APPLY sys.dm_db_stats_properties(stat.object_id, stat.stats_id) AS sp
WHERE t.name = 'MasterState' and s.name = 'biadmin'
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
						AssertEquals("Auto Recompute should be enabled for MasterState.", Int32.Parse(noRecomputeNumber), 0);
					}
					else
					{
						AssertEquals("Auto Recompute should be disabled for MasterState.", Int32.Parse(noRecomputeNumber), Int32.Parse(totalNumber));
					}
				}
			}
		}
	}
}

