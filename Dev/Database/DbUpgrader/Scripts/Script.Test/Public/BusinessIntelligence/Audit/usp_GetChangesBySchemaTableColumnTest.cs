using System;
using System.Data;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Audit.Testing
{
	[TestedType(typeof(usp_GetChangesBySchemaTableColumn))]
	class usp_GetChangesBySchemaTableColumnTest : TestCase
	{
		AdminConnection TestConnection;
		readonly DateTime EarliestValidDate = new DateTime(DateTime.UtcNow.Year - 1, 12, 31);

		public void TestGetChangesBySchemaTableColumn()
		{
			using (TestConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			using (SnapshotCreator.CreateSnapshot(TestConnection, Db.Connection.CloseConnection, Db.AuditDatabaseName, Db.AuditDatabaseName + "-SS"))
			{
				SetUpBiAdminTable();
				SetUpAuditTable();
				CreatePartition();

				var currentLsn = new byte[] { 0 };
				var maxLsn = new byte[] { 0XFF, 0XFF };
				var period = DateToLsnPeriod(EarliestValidDate.AddMonths(-1));
				var maxLsnPeriod = DateToLsnPeriod(EarliestValidDate.AddMonths(2));

				var expected = newResultSet;
				AddChangedTableResult(expected, "dbo", "GlbStaff", gsPK, new byte[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
				var result = GetResult(ref currentLsn, ref period, schemaTableColumnAsTVP, maxLsn, maxLsnPeriod);
				CombineAssertions("First run usp_GetChangesBySchemaTableColumn", () =>
				{
					AssertEquals("0x000000000000000000C8",/*200*/ "0x" + BitConverter.ToString(currentLsn).Replace("-", string.Empty));
					AssertEquals(DateToLsnPeriod(EarliestValidDate.AddMonths(1)), period);
					Assert(result.AsEnumerable().Select(row => (Guid)row["ChangedValue"]).Contains(gsPK));
					Assert(result.AsEnumerable().Select(row => (Guid)row["ChangedValue"]).Contains(ohPK));
				});

				expected = newResultSet;
				AddChangedTableResult(expected, "dbo", "GlbStaff", gsPK, new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
				AddChangedTableResult(expected, "dbo", "OrgHeader", ohPK, new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
				result = GetResult(ref currentLsn, ref period, schemaTableColumnAsTVP, maxLsn, maxLsnPeriod);
				CombineAssertions("Second run usp_GetChangesBySchemaTableColumn", () =>
				{
					AssertEquals("0x00000000000000000190",/*400*/ "0x" + BitConverter.ToString(currentLsn).Replace("-", string.Empty));
					AssertEquals(DateToLsnPeriod(EarliestValidDate.AddMonths(2)), period);
					Assert(result.AsEnumerable().Select(row => (Guid)row["ChangedValue"]).Contains(gsPK));
				});
			}
		}

		void CreatePartition()
		{
			using (var cmd = TestConnection.Command($"[{BiConstants.BiAdminSchemaName}].[usp_RecreatePartitionsAndPurgeOldData]"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddOutputParameter("@ErrorCode", SqlDbType.Int, 0, 0, 0, -1);
				cmd.AddOutputParameter("@InfoMessage", SqlDbType.VarChar, -1, 0, 0, "");
				cmd.AddOutputParameter("@ErrorMessage", SqlDbType.VarChar, -1, 0, 0, "");
				cmd.ExecuteNonQuery();
			}
		}

		DataTable GetResult(ref byte[] currentLsn, ref short currentPeriod, DataTable schemaTableColumnAsTVP, byte[] maxLsn, short maxLsnPeriod)
		{
			using (var cmd = TestConnection.Command($"[{BiConstants.BiAdminSchemaName}].[usp_GetChangesBySchemaTableColumn]"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@LsnHighWaterMark", SqlDbType.Binary, 10, 0, 0, currentLsn);
				cmd.AddParameter("@PeriodHighWaterMark", SqlDbType.SmallInt, -1, 0, 0, currentPeriod);

				cmd.AddParameter("@LatestLsn", SqlDbType.Binary, 10, 0, 0, maxLsn);
				cmd.AddParameter("@LatestPeriod", SqlDbType.SmallInt, maxLsnPeriod);
				cmd.AddOutputParameter("@CurrentLsnHighWaterMark", SqlDbType.Binary, 10, 0, 0, null);
				cmd.AddOutputParameter("@CurrentPeriodHighWaterMark", SqlDbType.SmallInt, -1, 0, 0, null);

				var result = DataUtils.GetDataTableFromCommand(cmd);
				currentLsn = (byte[])cmd.GetParameterValue("@CurrentLsnHighWaterMark");
				currentPeriod = (short)cmd.GetParameterValue("@CurrentPeriodHighWaterMark");
				return result;
			}
		}

		void SetUpBiAdminTable()
		{
			TruncateBiAdminTables();

			BatchInsertCdcHistorySummary("GlbStaff", 0, EarliestValidDate, 200);
			BatchInsertCdcHistorySummary("GlbStaff", 200, EarliestValidDate.AddMonths(1), 200);
			BatchInsertCdcHistorySummary("GlbStaff", 400, EarliestValidDate.AddMonths(2), 200);
			BatchInsertCdcHistorySummary("OrgHeader", 200, EarliestValidDate.AddMonths(1), 1);

			BatchInsertLsnTimeMapping(0, EarliestValidDate, 200);
			BatchInsertLsnTimeMapping(200, EarliestValidDate.AddMonths(1), 200);
			BatchInsertLsnTimeMapping(400, EarliestValidDate.AddMonths(2), 200);
		}

		void BatchInsertCdcHistorySummary(string table, int lsn, DateTime date, int batchSize)
		{
			var period = date.Year % 1000 * 100 + date.Month;
			using (var cmd = TestConnection.Command(BatchInsertCdcHistorySummarySql))
			{
				cmd.AddParameter("@BatchSize", SqlDbType.Int, batchSize);
				cmd.AddParameter("@Lsn", SqlDbType.Int, lsn);
				cmd.AddParameter("@Table", SqlDbType.NVarChar, table);
				cmd.AddParameter("@Period", SqlDbType.Int, period);
				cmd.AddParameter("@Date", SqlDbType.DateTime, date);

				cmd.ExecuteNonQuery();
			}
		}

		void BatchInsertLsnTimeMapping(int lsn, DateTime date, int batchSize)
		{
			using (var cmd = TestConnection.Command(BatchInsertLsnTimeMappingSql))
			{
				cmd.AddParameter("@BatchSize", SqlDbType.Int, batchSize);
				cmd.AddParameter("@Lsn", SqlDbType.Int, lsn);
				cmd.AddParameter("@Date", SqlDbType.DateTime, date);

				cmd.ExecuteNonQuery();
			}
		}

		readonly Guid gsPK = Guid.NewGuid();
		readonly Guid ohPK = Guid.NewGuid();

		void BatchInsertAuditTable(string table, string pkName, Guid pk, int lsn, int period, int batchSize)
		{
			using (var cmd = TestConnection.Command(string.Format(BatchInsertAuditTableSql, table, pkName)))
			{
				cmd.AddParameter("@BatchSize", SqlDbType.Int, batchSize);
				cmd.AddParameter("@Lsn", SqlDbType.Int, lsn);
				cmd.AddParameter("@Period", SqlDbType.Int, period);
				cmd.AddParameter("@Pk", SqlDbType.UniqueIdentifier, pk);

				cmd.ExecuteNonQuery();
			}
		}

		void InsertAuditTable(string table, string pkName, Guid pk, int lsn, int period)
		{
			using (var cmd = TestConnection.Command(string.Format(InsertAuditTableSql, table, pkName)))
			{
				cmd.AddParameter("@Lsn", SqlDbType.Int, lsn);
				cmd.AddParameter("@Period", SqlDbType.Int, period);
				cmd.AddParameter("@Pk", SqlDbType.UniqueIdentifier, pk);

				cmd.ExecuteNonQuery();
			}
		}

		void SetUpAuditTable()
		{
			TruncateAuditTables();

			BatchInsertAuditTable("GlbStaff", "GS_PK", gsPK, 0, DateToLsnPeriod(EarliestValidDate), 200);
			BatchInsertAuditTable("GlbStaff", "GS_PK", gsPK, 200, DateToLsnPeriod(EarliestValidDate.AddMonths(1)), 200);
			BatchInsertAuditTable("GlbStaff", "GS_PK", gsPK, 400, DateToLsnPeriod(EarliestValidDate.AddMonths(2)), 200);

			InsertAuditTable("OrgHeader", "OH_PK", ohPK, 200, DateToLsnPeriod(EarliestValidDate.AddMonths(1)));
		}

		void TruncateAuditTables()
		{
			TestConnection.ExecuteNonQuery(TruncateAuditTablesSql);
		}

		void TruncateBiAdminTables()
		{
			TestConnection.ExecuteNonQuery(TruncateBiAdminTablesSql);
		}

		void AddSchemaTableColumns(DataTable dt, string schema, string table, string column)
		{
			var row = dt.NewRow();
			row["SchemaName"] = schema;
			row["TableName"] = table;
			row["ColumnName"] = column;
			dt.Rows.Add(row);
		}

		void AddChangedTableResult(DataTable dt, string schema, string table, Guid changedValue, byte[] lsn)
		{
			var row = dt.NewRow();
			row["SchemaName"] = schema;
			row["TableName"] = table;
			row["ChangedValue"] = changedValue;
			row["__$start_lsn"] = lsn;
			row["__$command_id"] = 1;
			row["__$seqval"] = 1;
			row["__$operation"] = 1;
			dt.Rows.Add(row);
		}

		DataTable schemaTableColumnAsTVP
		{
			get
			{
				var dataTable = new DataTable();
				dataTable.Columns.Add("SchemaName", typeof(string));
				dataTable.Columns.Add("TableName", typeof(string));
				dataTable.Columns.Add("ColumnName", typeof(string));

				AddSchemaTableColumns(dataTable, "dbo", "GlbStaff", "GS_PK");
				AddSchemaTableColumns(dataTable, "dbo", "OrgHeader", "OH_PK");
				return dataTable;
			}
		}

		DataTable newResultSet
		{
			get
			{
				var dataTable = new DataTable();
				dataTable.Columns.Add("SchemaName", typeof(string));
				dataTable.Columns.Add("TableName", typeof(string));
				dataTable.Columns.Add("ChangedValue", typeof(Guid));

				dataTable.Columns.Add("__$start_lsn", typeof(byte[]));
				dataTable.Columns.Add("__$command_id", typeof(int));
				dataTable.Columns.Add("__$seqval", typeof(byte[]));
				dataTable.Columns.Add("__$operation", typeof(short));
				return dataTable;
			}
		}

		short DateToLsnPeriod(DateTime date) => (short)(date.Year % 1000 * 100 + date.Month);

		#region SQL Texts
		const string BatchInsertCdcHistorySummarySql = $@"
declare @iter int = 0
while(@iter < @BatchSize)
begin
	insert into {BiConstants.BiAdminSchemaName}.CdcHistorySummary
	(Lsn,			SchemaName,	ChangedTableName,	LsnPeriod,	TranEndTimeUTC,		NumberOfRows,NumberOfRowsDelete,NumberOfRowsInsert,NumberOfRowsUpdate) values
	(@Lsn,			'dbo',		@Table,			    @Period,	@Date,  			1,1,0,0),
	(@Lsn+@iter+1,	'dbo',		@Table,			    @Period,	@Date,  			1,1,0,0),
	(@Lsn+@iter+2,	'dbo',		@Table,			    @Period,	@Date,  			1,1,0,0),
	(@Lsn+@iter+3,	'dbo',		@Table,			    @Period,	@Date,  			1,1,0,0),
	(@Lsn+@iter+4,	'dbo',		@Table,			    @Period,	@Date,  			1,1,0,0);
set @iter = @iter + 5;
end
";

		const string BatchInsertLsnTimeMappingSql = @$"
declare @iter int = 0;
while(@iter < @BatchSize)
begin
insert into {BiConstants.BiAdminSchemaName}.LsnTimeMapping
(StartLsn,	TranEndTimeUtc) values
(@Lsn+@iter,	@Date),
(@Lsn+@iter+1,	@Date),
(@Lsn+@iter+2,	@Date),
(@Lsn+@iter+3,	@Date),
(@Lsn+@iter+4,	@Date);
set @iter = @iter+5;
end
";

		const string BatchInsertAuditTableSql = @"
declare @iter int = 0;
while(@iter < @BatchSize)
begin
	insert into dbo.{0}
	(__$start_lsn,	__$seqval,	__$operation,	__$update_mask,	__$lsn_period,	__$command_id,	{1})
	values
	(@Lsn + @iter,		1,			1,				1,			@Period,		1,				@Pk),
	(@Lsn + @iter+1,	1,			1,				1,			@Period,		1,				@Pk),
	(@Lsn + @iter+2,	1,			1,				1,			@Period,		1,				@Pk),
	(@Lsn + @iter+3,	1,			1,				1,			@Period,		1,				@Pk),
	(@Lsn + @iter+4,	1,			1,				1,			@Period,		1,				@Pk)
set @iter = @iter + 5
end
";

		const string InsertAuditTableSql = @"
insert into dbo.{0}
(__$start_lsn,	__$seqval,	__$operation,	__$update_mask,		__$lsn_period,		__$command_id,		{1})
values
(@Lsn,			1,			1,				1,					@Period,			1,					@Pk)
";

		const string TruncateAuditTablesSql = @"
truncate table dbo.GlbStaff;
truncate table dbo.OrgHeader;
";

		const string TruncateBiAdminTablesSql = @$"
truncate table {BiConstants.BiAdminSchemaName}.CdcHistorySummary;
truncate table {BiConstants.BiAdminSchemaName}.LsnTimeMapping;
";
		#endregion
	}
}
