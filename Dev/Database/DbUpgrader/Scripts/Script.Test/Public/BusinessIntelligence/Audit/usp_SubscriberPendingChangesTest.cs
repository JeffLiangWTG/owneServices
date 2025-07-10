using System;
using System.Data;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Audit.Testing
{
	[TestedType(typeof(usp_SubscriberPendingChanges))]
	internal class usp_SubscriberPendingChangesTransactionedTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.AuditDatabaseName; }
		}

		public void TestSubscriberPendingChanges()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var insert = $@"
insert into biadmin.SubscriberControl
(SubscriberCode, Description, PeriodHighWaterMark, LsnHighWaterMark, SeqValHighWaterMark, CommandIdHighWaterMark, OperationHighWaterMark)
values('~T1', '~T1', 2306, 0x00000000000000000007, 0x0, 0, 0)

insert into biadmin.CdcHistorySummary with (tablock) 
(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod, TranEndTimeUTC)
values
(4, 'dbo', 'ct', 10, 2304, '2023-04-01 00:00:00.000'),		-- before lsn period
(5, 'dbo', 'ignore', 10, 2305, '2023-05-01 00:00:00.000'),	-- not the table
(6, 'dbo', 'ct', 10, 2305, '2023-05-01 00:00:00.000'),		-- lower than subscriber lsn water mark
(7, 'dbo', 'ct', 10, 2305, '2023-05-01 00:00:00.000'),		-- before lsn period
(8, 'dbo', 'ct', 10, 2306, '2023-06-01 00:00:00.000'),
(9, 'dbo', 'ct', 10, 2306, '2023-06-01 00:00:00.000'),		-- distinct
(10, 'dbo', 'ct', 10, 2307, '2023-07-01 00:00:00.000')
";
				TestConnection.ExecuteNonQuery(insert);

				byte[] nextLsnHighWaterMark, nextSeqValHighWaterMark;
				int nextCommandId, nextOperation, nextPeriodToProcess;
				GetResult("~T1", "dbo", "ct", latestPeriod: 2306,
					out nextLsnHighWaterMark, out nextSeqValHighWaterMark, out nextCommandId, out nextOperation, out nextPeriodToProcess);
				CombineAssertions(() =>
				{
					AssertEquals(nameof(nextLsnHighWaterMark), BitConverter.ToString(ToLsn(7)), BitConverter.ToString(nextLsnHighWaterMark));
					AssertEquals(nameof(nextSeqValHighWaterMark), BitConverter.ToString(ToLsn(0)), BitConverter.ToString(nextSeqValHighWaterMark));
					AssertEquals(nameof(nextCommandId), 0, nextCommandId);
					AssertEquals(nameof(nextOperation), 0, nextOperation);
					AssertEquals(nameof(nextPeriodToProcess), 2306, nextPeriodToProcess);
				});

				GetResult("~T1", "dbo", "ct", latestPeriod: 2307,
					out nextLsnHighWaterMark, out nextSeqValHighWaterMark, out nextCommandId, out nextOperation, out nextPeriodToProcess);
				CombineAssertions(() =>
				{
					AssertEquals(nameof(nextLsnHighWaterMark), BitConverter.ToString(ToLsn(7)), BitConverter.ToString(nextLsnHighWaterMark));
					AssertEquals(nameof(nextSeqValHighWaterMark), BitConverter.ToString(ToLsn(0)), BitConverter.ToString(nextSeqValHighWaterMark));
					AssertEquals(nameof(nextCommandId), 0, nextCommandId);
					AssertEquals(nameof(nextOperation), 0, nextOperation);
					AssertEquals(nameof(nextPeriodToProcess), 2306, nextPeriodToProcess);
				});
			}
		}

		static byte[] ToLsn(byte lsn)
		{
			var val = new byte[10];
			val[9] = lsn;
			return val;
		}

		void GetResult(string code, string schemaName, string tableName, short latestPeriod,
			out byte[] nextLsnHighWaterMark, out byte[] nextSeqValHighWaterMark, out int nextCommandId, out int nextOperation, out int nextPeriodToProcess)
		{
			using (var cmd = TestConnection.Command($"[{BiConstants.BiAdminSchemaName}].[usp_SubscriberPendingChanges]"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@subscriberCode", SqlDbType.Char, 3, code);
				cmd.AddParameter("@subscriberTableName", SqlDbType.VarChar, 128, tableName);
				cmd.AddParameter("@subscriberSchemaName", SqlDbType.VarChar, 128, schemaName);
				cmd.AddParameter("@latestPeriod", SqlDbType.SmallInt, latestPeriod);

				cmd.AddOutputParameter("@lsnHighWaterMark", SqlDbType.Binary, 10, 0, 0, DBNull.Value);
				cmd.AddOutputParameter("@seqValHighWaterMark", SqlDbType.Binary, 10, 0, 0, DBNull.Value);
				cmd.AddOutputParameter("@commandIdHighWaterMark", SqlDbType.Int, 0, 0, 0, DBNull.Value);
				cmd.AddOutputParameter("@operationHighWaterMark", SqlDbType.Int, 0, 0, 0, DBNull.Value);
				cmd.AddOutputParameter("@nextPeriodToProcess", SqlDbType.SmallInt, 0, 0, 0, DBNull.Value);

				var queryResult = DataUtils.GetDataTableFromCommand(cmd);

				nextLsnHighWaterMark = (byte[])cmd.GetParameterValue("@lsnHighWaterMark");
				nextSeqValHighWaterMark = (byte[])cmd.GetParameterValue("@seqValHighWaterMark");
				nextCommandId = Convert.ToInt32(cmd.GetParameterValue("@commandIdHighWaterMark"));
				nextOperation = Convert.ToInt32(cmd.GetParameterValue("@operationHighWaterMark"));
				nextPeriodToProcess = Convert.ToInt32(cmd.GetParameterValue("@nextPeriodToProcess"));
			}
		}
	}
}
