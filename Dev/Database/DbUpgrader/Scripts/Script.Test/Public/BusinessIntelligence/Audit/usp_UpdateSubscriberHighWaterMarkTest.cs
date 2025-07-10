using System;
using System.Data;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Audit.Testing
{
	[TestedType(typeof(usp_UpdateSubscriberHighWaterMark))]
	internal class usp_UpdateSubscriberHighWaterMarkTransactionedTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.AuditDatabaseName; }
		}

		public void TestUpdateSubscriberHighWaterMark()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var zeroLsn = ToLsn(0);
				var nextPeriodHighWaterMark = 2306;
				var nextLsnHighWaterMark = ToLsn(10);
				var nextCommandIdHighWaterMark = 10;
				var nextSeqValHighWaterMark = ToLsn(9);
				var nextOperationHighWaterMark = 10;

				var insert = $@"
INSERT INTO {BiConstants.BiAdminSchemaName}.SubscriberControl 
(SubscriberCode, Description, PeriodHighWaterMark, LsnHighWaterMark, CommandIdHighWaterMark, SeqValHighWaterMark, OperationHighWaterMark)
VALUES
('~T1', 'should update', 2305, 0, 2147483647, 10, 2147483647),
('~T2', 'should update', 2305, 10, 9, 10, 2147483647),
('~T3', 'should update', 2305, 10, 10, 8, 2147483647),
('~T4', 'should update', 2305, 10, 10, 9, 1),
('~T5', 'no change', 2306, 15, 2147483647, 0, 2147483647),
('~T6', 'no change', 2306, 10, 2147483647, 10, 2147483647),
('~T7', 'no change', 2306, 10, 11, 9, 0),
('~T8', 'no change', 2306, 10, 10, 9, 11),
('~T9', 'no change', 2305, 0, 2147483647, 0, 2147483647)

insert into {BiConstants.BiAdminSchemaName}.LsnTimeMapping
(StartLsn, TranEndTimeUtc)
values(0x00000000000000000010, '2023-06-01 00:00:00.000')
";

				TestConnection.ExecuteNonQuery(insert);

				UpdateSubscriberHighWaterMark("~T1", nextLsnHighWaterMark, nextSeqValHighWaterMark, nextPeriodHighWaterMark, nextCommandIdHighWaterMark, nextOperationHighWaterMark);
				UpdateSubscriberHighWaterMark("~T2", nextLsnHighWaterMark, nextSeqValHighWaterMark, nextPeriodHighWaterMark, nextCommandIdHighWaterMark, nextOperationHighWaterMark);
				UpdateSubscriberHighWaterMark("~T3", nextLsnHighWaterMark, nextSeqValHighWaterMark, nextPeriodHighWaterMark, nextCommandIdHighWaterMark, nextOperationHighWaterMark);
				UpdateSubscriberHighWaterMark("~T4", nextLsnHighWaterMark, nextSeqValHighWaterMark, nextPeriodHighWaterMark, nextCommandIdHighWaterMark, nextOperationHighWaterMark);
				UpdateSubscriberHighWaterMark("~T5", nextLsnHighWaterMark, nextSeqValHighWaterMark, nextPeriodHighWaterMark, nextCommandIdHighWaterMark, nextOperationHighWaterMark);
				UpdateSubscriberHighWaterMark("~T6", nextLsnHighWaterMark, nextSeqValHighWaterMark, nextPeriodHighWaterMark, nextCommandIdHighWaterMark, nextOperationHighWaterMark);
				UpdateSubscriberHighWaterMark("~T7", nextLsnHighWaterMark, nextSeqValHighWaterMark, nextPeriodHighWaterMark, nextCommandIdHighWaterMark, nextOperationHighWaterMark);
				UpdateSubscriberHighWaterMark("~T8", nextLsnHighWaterMark, nextSeqValHighWaterMark, nextPeriodHighWaterMark, nextCommandIdHighWaterMark, nextOperationHighWaterMark);

				CombineAssertions(() =>
				{
					AssertSubscriberStatus("~T1", 2306, nextLsnHighWaterMark, nextSeqValHighWaterMark, nextCommandIdHighWaterMark, nextOperationHighWaterMark);
					AssertSubscriberStatus("~T2", 2306, nextLsnHighWaterMark, nextSeqValHighWaterMark, nextCommandIdHighWaterMark, nextOperationHighWaterMark);
					AssertSubscriberStatus("~T3", 2306, nextLsnHighWaterMark, nextSeqValHighWaterMark, nextCommandIdHighWaterMark, nextOperationHighWaterMark);
					AssertSubscriberStatus("~T4", 2306, nextLsnHighWaterMark, nextSeqValHighWaterMark, nextCommandIdHighWaterMark, nextOperationHighWaterMark);
					AssertSubscriberStatus("~T5", 2306, ToLsn(15), zeroLsn, 2147483647, 2147483647);
					AssertSubscriberStatus("~T6", 2306, ToLsn(10), ToLsn(10), 2147483647, 2147483647);
					AssertSubscriberStatus("~T7", 2306, ToLsn(10), ToLsn(9), 11, 0);
					AssertSubscriberStatus("~T8", 2306, ToLsn(10), ToLsn(9), 10, 11);
					AssertSubscriberStatus("~T9", 2305, zeroLsn, zeroLsn, 2147483647, 2147483647);
				});
			}
		}

		void UpdateSubscriberHighWaterMark(string code, byte[] nextLsnHighWaterMark, byte[] nextSeqValHighWaterMark, int nextPeriodHighWaterMark, int nextCommandIdHighWaterMark, int nextOperationHighWaterMark)
		{
			using (var cmd = TestConnection.Command($"{BiConstants.BiAdminSchemaName}.usp_UpdateSubscriberHighWaterMark"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@subscriberCode", SqlDbType.Char, 3, code);
				cmd.AddParameter("@lsnHighWaterMark", SqlDbType.Binary, 10, nextLsnHighWaterMark);
				cmd.AddParameter("@seqValHighWaterMark", SqlDbType.Binary, 10, nextSeqValHighWaterMark);
				cmd.AddParameter("@periodHighWaterMark", SqlDbType.SmallInt, nextPeriodHighWaterMark);
				cmd.AddParameter("@commandIdHighWaterMark", SqlDbType.SmallInt, nextCommandIdHighWaterMark);
				cmd.AddParameter("@operationHighWaterMark", SqlDbType.SmallInt, nextOperationHighWaterMark);
				cmd.ExecuteNonQuery();
			}
		}

		void AssertSubscriberStatus(string code, short periodHighWaterMark, byte[] lsnHighWaterMark, byte[] seqValHighWaterMark, int commandIdHighWaterMark, int operationHighWaterMark)
		{
			using (var cmd = TestConnection.Command($"SELECT PeriodHighWaterMark, LsnHighWaterMark, SeqValHighWaterMark, CommandIdHighWaterMark, OperationHighWaterMark FROM {BiConstants.BiAdminSchemaName}.SubscriberControl WHERE SubscriberCode = '{code}'"))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					AssertEquals($"Subscriber: {code} {nameof(periodHighWaterMark)}:", periodHighWaterMark, Convert.ToInt16(reader["PeriodHighWaterMark"]));
					AssertEquals($"Subscriber: {code} {nameof(lsnHighWaterMark)}:", lsnHighWaterMark, (byte[])reader["LsnHighWaterMark"]);
					AssertEquals($"Subscriber: {code} {nameof(seqValHighWaterMark)}:", seqValHighWaterMark, (byte[])reader["SeqValHighWaterMark"]);
					AssertEquals($"Subscriber: {code} {nameof(commandIdHighWaterMark)}:", commandIdHighWaterMark, Convert.ToInt32(reader["CommandIdHighWaterMark"]));
					AssertEquals($"Subscriber: {code} {nameof(operationHighWaterMark)}:", operationHighWaterMark, Convert.ToInt32(reader["OperationHighWaterMark"]));
				}
				else
				{
					Fail($"Subscriber {code} could not be found in [biadmin].[SubscriberControl].");
				}
			}
		}

		static byte[] ToLsn(byte lsn)
		{
			var val = new byte[10];
			val[9] = lsn;
			return val;
		}
	}
}
