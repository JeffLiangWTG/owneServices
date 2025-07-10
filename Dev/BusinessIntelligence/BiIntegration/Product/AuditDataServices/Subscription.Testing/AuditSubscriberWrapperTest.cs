using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	public class AuditSubscriberWrapperTest : TestCase
	{
		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestGetMaxLsnAndPeriod()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var sqlText = $@"
					DELETE FROM [{BiConstants.BiAdminSchemaName}].[MasterState] WHERE ParamName in ('{BiConstants.LastMaxLsnProcessed}', '{BiConstants.LastMaxLsnTimeProcessed}')
					INSERT INTO [{BiConstants.BiAdminSchemaName}].[MasterState](ParamName, ParamValue) VALUES ('{BiConstants.LastMaxLsnProcessed}', '0xFFFFFFFFFFFFFFFFFFFF')
					INSERT INTO [{BiConstants.BiAdminSchemaName}].[MasterState](ParamName, ParamValue) VALUES ('{BiConstants.LastMaxLsnTimeProcessed}', '2018-06-01')
					INSERT INTO [{BiConstants.BiAdminSchemaName}].[LsnTimeMapping] (StartLsn, TranEndTimeUtc) VALUES (0xFFFFFFFFFFFFFFFFFFFF, '2018-06-01')
					SELECT
						@ExpectedLsn = 0xFFFFFFFFFFFFFFFFFFFF,
						@ExpectedLsnString = CONVERT(BINARY(10), '0xFFFFFFFFFFFFFFFFFFFF', 1)
				";
				using (var cmd = auditConnection.Command(sqlText))
				{
					cmd.AddOutputParameter("@ExpectedLsn", SqlDbType.Binary, 10, 0, 0, null);
					cmd.AddOutputParameter("@ExpectedLsnString", SqlDbType.Binary, 10, 0, 0, null);
					cmd.ExecuteNonQuery();

					var expectedMaxLsn = "0xFFFFFFFFFFFFFFFFFFFF";
					var expectedLsn = (byte[])cmd.GetParameterValue("@ExpectedLsn");
					var expectedLsnString = (byte[])cmd.GetParameterValue("@ExpectedLsnString");
					var expectedPeriod = 1806;

					var testSubscriber = (ActualDataChangesAuditSubscriberWrapper)(new GenericTestDataChangeSubscriber("*UT", "Unit Test", RefCurrencySchema.Instance).GetWrapper(auditConnection, new LoggerForTest()));
					testSubscriber.AddSubscriberToSubscriberControlTable();
					CombineAssertions(() =>
					{
						AssertEquals(
							$"The NextLsnHWM (after adding a subscriber to SubscriberControl) should be the MaxLsn '{expectedMaxLsn}' but it was {testSubscriber.NextLsnHighWaterMark}",
							expectedLsn, testSubscriber.NextLsnHighWaterMark);
						AssertEquals(
							$"The binary NextLsnHWM should be the same as when you convert a string in SQL {expectedLsnString} but it was {testSubscriber.NextLsnHighWaterMark}",
							expectedLsnString, testSubscriber.NextLsnHighWaterMark);
						AssertEquals(
							$"The NextPeriodHWM (after adding a subscriber to SubscriberControl) should be the latest period '{expectedPeriod}' but it was {testSubscriber.NextPeriodHighWaterMark}",
							expectedPeriod, testSubscriber.NextPeriodHighWaterMark);
					});
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestUpdateLsnHighWaterMark()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };
				var testSubscriber = new GenericTestDataChangeSubscriber("*UT", "Unit Test", RefCurrencySchema.Instance, columns: schemaCols);

				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT INTO [{0}].SubscriberControl(SubscriberCode, Description, LsnHighWaterMark, SeqValHighWaterMark, PeriodHighWaterMark) VALUES
							('*UT', 'Unit Test', 0x0A, 0, -1)
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x0B, '2018-06-01')
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
							(0x0B, 'dbo', 'glbgroup', 3, 1806);",
						BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				var testLogger = new LoggerForTest();
				var testAuditWrapper = new TestAuditSubscriberWrapper(auditConnection, testSubscriber, testLogger);

				var expectedLsnHighWaterMark = new byte[10];
				expectedLsnHighWaterMark[0] = 0x0B;
				testAuditWrapper.SetNextLsnHighWaterMarkForTesting(new byte[] { 0x0B });
				var expectedSeqValHighWaterMark = new byte[10];
				expectedSeqValHighWaterMark[0] = 0x0C;
				testAuditWrapper.SetNextSeqValHighWaterMarkForTesting(new byte[] { 0x0C });
				testAuditWrapper.SetNextPeriodHighWaterMarkForTesting(1806);
				testAuditWrapper.UpdateLsnHighWaterMark();

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						SELECT
							@LsnHighWaterMark = LsnHighWaterMark,
							@SeqValHighWaterMark = SeqValHighWaterMark, 
							@PeriodHighWaterMark = PeriodHighWaterMark
						FROM [{0}].SubscriberControl
						WHERE SubscriberCode = '*UT'",
				BiConstants.BiAdminSchemaName);

				using (var cmd = auditConnection.Command(sqlText))
				{
					cmd.AddOutputParameter("@LsnHighWaterMark", SqlDbType.Binary, 10, 0, 0, null);
					cmd.AddOutputParameter("@SeqValHighWaterMark", SqlDbType.Binary, 10, 0, 0, null);
					cmd.AddOutputParameter("@PeriodHighWaterMark", SqlDbType.SmallInt, -1, 0, 0, null);
					cmd.ExecuteNonQuery();

					var lsnHighWaterMark = GetByteOutputParameter(cmd, "@LsnHighWaterMark");
					var seqValHighWaterMark = GetByteOutputParameter(cmd, "@SeqValHighWaterMark");
					var periodHighWaterMark = GetIntOutputParameter(cmd, "@PeriodHighWaterMark");

					AssertEquals(expectedLsnHighWaterMark, lsnHighWaterMark);
					AssertEquals(expectedSeqValHighWaterMark, seqValHighWaterMark);
					AssertEquals(1806, periodHighWaterMark);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSetLsnHighWaterMarkToMaxLsn()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };
				var testSubscriber = new GenericTestDataChangeSubscriber("*UT", "Unit Test", RefCurrencySchema.Instance, columns: schemaCols);

				var sqlText = $@"
					DELETE FROM [{BiConstants.BiAdminSchemaName}].[MasterState] WHERE ParamName in ('{BiConstants.LastMaxLsnProcessed}', '{BiConstants.LastMaxLsnTimeProcessed}')
					INSERT INTO [{BiConstants.BiAdminSchemaName}].[MasterState](ParamName, ParamValue) VALUES ('{BiConstants.LastMaxLsnProcessed}', '0xFFFFFFFFFFFFFFFFFFFF')
					INSERT INTO [{BiConstants.BiAdminSchemaName}].[MasterState](ParamName, ParamValue) VALUES ('{BiConstants.LastMaxLsnTimeProcessed}', '2018-06-01')
					INSERT INTO [{BiConstants.BiAdminSchemaName}].SubscriberControl(SubscriberCode, Description, LsnHighWaterMark, SeqValHighWaterMark, PeriodHighWaterMark) VALUES
						('*UT', 'Unit Test', 0x0A, 0x0B, -1)
					INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
						(0xFFFFFFFFFFFFFFFFFFFF, '2018-06-01')
					INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0xFFFFFFFFFFFFFFFFFFFF, 'dbo', 'glbgroup', 3, 1806)
				";
				auditConnection.ExecuteNonQuery(sqlText);

				var testLogger = new LoggerForTest();

				var testAuditWrapper = new TestAuditSubscriberWrapper(auditConnection, testSubscriber, testLogger);
				testAuditWrapper.SetNextLsnHighWaterMarkForTesting(new byte[] { 0x0A });
				testAuditWrapper.SetLsnHighWaterMarkToMaxLsn();

				var maxLsn = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

				var expectedLsnHighWaterMark = maxLsn;
				var expectedSeqValHighWaterMark = maxLsn;

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						SELECT
							@LsnHighWaterMark = LsnHighWaterMark,
							@CommandIdHighWaterMark = CommandIdHighWaterMark,
							@SeqValHighWaterMark = SeqValHighWaterMark, 
							@PeriodHighWaterMark = PeriodHighWaterMark,
							@OperationHighWaterMark = OperationHighWaterMark
						FROM [{0}].SubscriberControl
						WHERE SubscriberCode = '*UT'",
				BiConstants.BiAdminSchemaName);

				using (var cmd = auditConnection.Command(sqlText))
				{
					cmd.AddOutputParameter("@LsnHighWaterMark", SqlDbType.Binary, 10, 0, 0, null);
					cmd.AddOutputParameter("@CommandIdHighWaterMark", SqlDbType.Int, -1, 0, 0, null);
					cmd.AddOutputParameter("@SeqValHighWaterMark", SqlDbType.Binary, 10, 0, 0, null);
					cmd.AddOutputParameter("@PeriodHighWaterMark", SqlDbType.SmallInt, -1, 0, 0, null);
					cmd.AddOutputParameter("@OperationHighWaterMark", SqlDbType.Int, -1, 0, 0, null);
					cmd.ExecuteNonQuery();

					var lsnHighWaterMark = GetByteOutputParameter(cmd, "@LsnHighWaterMark");
					var commandIdHighWaterMark = GetIntOutputParameter(cmd, "@CommandIdHighWaterMark");
					var seqValHighWaterMark = GetByteOutputParameter(cmd, "@SeqValHighWaterMark");
					var periodHighWaterMark = GetIntOutputParameter(cmd, "@PeriodHighWaterMark");
					var operationHighWaterMark = GetIntOutputParameter(cmd, "@OperationHighWaterMark");

					AssertEquals("LSN high watermark", expectedLsnHighWaterMark, lsnHighWaterMark);
					AssertEquals("Command ID high watermark", int.MaxValue, commandIdHighWaterMark);
					AssertEquals("Sequence value high watermark", expectedSeqValHighWaterMark, seqValHighWaterMark);
					AssertEquals("Period high watermark", 1806, periodHighWaterMark);
					AssertEquals("Operation high watermark", int.MaxValue, operationHighWaterMark);
				}
			}
		}

		int GetIntOutputParameter(DbCommand cmd, string paramName)
		{
			var objParamValue = cmd.GetParameterValue(paramName);
			return (objParamValue == DBNull.Value) ? 0 : Convert.ToInt32(objParamValue);
		}

		byte[] GetByteOutputParameter(DbCommand cmd, string paramName)
		{
			var objParamValue = cmd.GetParameterValue(paramName);
			return (objParamValue == DBNull.Value) ? null : (byte[])objParamValue;
		}

		class TestAuditSubscriberWrapper : AuditSubscriberWrapper
		{
			public TestAuditSubscriberWrapper(DbConnection auditConnection, ActualDataChangesAuditSubscriber subscriber, ILogger logger = null)
			 : base(subscriber, auditConnection, logger)
			{
			}

			public override void ValidateSubscriber()
			{
				throw new NotImplementedException();
			}

			public override bool HasChangesToProcess()
			{
				throw new NotImplementedException();
			}

			public override bool FetchDataAndProcessChanges()
			{
				throw new NotImplementedException();
			}

			public void SetNextLsnHighWaterMarkForTesting(byte[] value)
			{
				NextLsnHighWaterMark = value;
			}

			public void SetNextSeqValHighWaterMarkForTesting(byte[] value)
			{
				NextSeqValHighWaterMark = value;
			}

			public void SetNextPeriodHighWaterMarkForTesting(int value)
			{
				NextPeriodHighWaterMark = value;
			}

			public override bool HasChangesToProcessFromCache(Dictionary<string, Lsn> tableStateLsnHighWaterMarkCache, Dictionary<string, Lsn> subscriberLsnHighWaterMarkCache, Dictionary<string, int> subscriberCommandIdHighWaterMarkCache)
			{
				throw new NotImplementedException();
			}
		}
	}
}
