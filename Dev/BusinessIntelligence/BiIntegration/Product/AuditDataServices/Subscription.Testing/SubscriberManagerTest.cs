using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Bi.Common.Testing;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	class SubscriberManagerTest : TransactionedTestCase
	{
		[UseSnapshotProtection(Db.AuditDatabaseSuffix)]
		public void TestDoesNotUpdateSubscriberControlHwmForFailedSubscriber()
		{
			var testSubscribers = new IAuditSubscriber[] {
				new ThrowsExceptionDuringProcessChangesSubscriber("!XX", "Exception Throwing Subscriber", RefCountrySchema.Instance),
				new GenericTestDataChangeSubscriber("!RN", "Dummy Test Actual Data Changes Subscriber", RefCountrySchema.Instance),
				new GenericTestDataChangeSubscriber("!RC", "Dummy Test No Data Changes Subscriber", RefCurrencySchema.Instance)
			};

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x01"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2017-12-03 00:00:00.0000"))
				{
					auditConnection.ExecuteNonQuery($@"
							UPDATE {BiConstants.BiAdminSchemaName}.TableState SET AetHWMHistorySummaryLsn = 0xffffffffff WHERE SourceSchemaName = 'dbo' AND (SourceTableName = 'RefCurrency' OR SourceTableName = 'RefCountry')
						");

					foreach (var subscriber in testSubscribers)
					{
						AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
					}

					string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.RefCountry ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RN_PK, RN_Code, RN_Desc) VALUES
							(1712, 1, 0x01, 0x01, 1, 0x001, '84FE72F0-0A3F-4392-9B49-4CC1E1D6B33A', '~1', '~Test~Country~1');
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x01, '2017-12-03');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
							(0x01, 'dbo', 'RefCountry', 1, 1712);
						",
						BiConstants.BiAdminSchemaName);
					auditConnection.ExecuteNonQuery(sqlText);

					RepeatNotificationCycleUntilNoChangesAndAssert(
						testSubscribers,
						expectedLogs: new string[] {
								"[!XX] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1712)",
								"[!XX] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1712)",
								"Running !XX Subscriber",
								"[!RN] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1712)",
								"[!RN] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1712)",
								"Running !RN Subscriber",
								"[!RC] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1712)",
								"[!RC] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1712)",
								"[!RC] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1712)",
								"Required subscribers: !XX, !RN",
								"[!RN] > Processing 1 change(s)",
								"[!RN] 01000000000000000000.01000000000000000000 - Deleted  - PK [84fe72f0-0a3f-4392-9b49-4cc1e1d6b33a] = Code [~1] - Desc [~Test~Country~1]",
								"[!RN] > Processing completed",
								"[!RN] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: 01000000000000000000, Period: 1712)",
								"[!XX] > Processing 1 change(s)",
								"[!RN] > No relevant changes found",
								"[!XX] Exception thrown while processing subscriber changes.",
								"[!RN] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: 01000000000000000000, Period: 1712)",
						}
					);

					AssertSubscriberHighWaterMark(auditConnection, "!RN", expected: "01000000000000000000.FFFFFFFFFFFFFFFFFFFF.1712");
					AssertSubscriberHighWaterMark(auditConnection, "!XX", expected: "");
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestDoesNotThrowErrorMessageDuringDbUpgrade()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscriber = new GenericExceptionThrowingSubscriber(new DatabaseUpgradeInProgressException());
				var testLogger = new LoggerForTest();
				RunSubscriberManager(new IAuditSubscriber[] { testSubscriber }, auditConnection, testLogger, "0x00", "2000-01-01 00:00:00.0000");

				AuditTestHelper.AssertLog(
					testLogger.LogEntries,
					expectedLogs: new string[] {
						"The database is in the process of being upgraded, please try again later."
					}
				);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestAuditSubscriberStopsProcessingChangesAboveMaxLsn()
		{
			var refCurrencySchemaCols = new[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };
			var testSubscribers = new IAuditSubscriber[] {
				new GenericTestDataChangeSubscriber("!AA", "Dummy Test Data Changed Table List Subscriber 1", RefCurrencySchema.Instance, refCurrencySchemaCols)
			};

			var orgHeaderSchemaCols = new[] { new SchemaStringColumnForTest("OH_Code"), new SchemaStringColumnForTest("OH_FullName") };
			var redundantSubscribers = new IAuditSubscriber[] {
				new GenericTestDataChangeSubscriber("!AA", "Dummy Test Data Changed Table List Subscriber 2", OrgHeaderSchema.Instance, orgHeaderSchemaCols) };

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				auditConnection.ExecuteNonQuery($@"
						UPDATE {BiConstants.BiAdminSchemaName}.TableState SET AetHWMHistorySummaryLsn = 0xffffffffff WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
					");

				foreach (var subscriber in testSubscribers)
				{
					AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
				}

				addTestDataLsn1(auditConnection);
				addTestDataLsn2to4(auditConnection);
				addRedundantTestDataLsn2to4(auditConnection);
				var testLogger = new LoggerForTest();
				RunSubscriberManager(testSubscribers, auditConnection, testLogger, "0x02", "2016-06-02 00:00:00.0000");

				AuditTestHelper.AssertLog(
					testLogger.LogEntries,
					expectedLogs: new string[] {
							"[!AA] > Audit DB HWM: (LSN: 02000000000000000000, Period: 1606)",
							"[!AA] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1606)",
							"Running !AA Subscriber",
							"Required subscribers: !AA",
							"[!AA] > Processing 11 change(s)",
							"[!AA] 01000000000000000000.00000000000000000001 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [~TEST~CURRENCY~1]=>[#TEST#CURRENCY#1]",
							"[!AA] 01000000000000000000.00000000000000000002 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [#TEST#CURRENCY#1]=>[*TEST*CURRENCY*1]",
							"[!AA] 01000000000000000000.00000000000000000003 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [*TEST*CURRENCY*1]=>[%TEST%CURRENCY%1]",
							"[!AA] 01000000000000000000.00000000000000000004 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [%TEST%CURRENCY%1]=>[~TEST~CURRENCY~1]",
							"[!AA] 01000000000000000000.00000000000000000006 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [~TEST~CURRENCY~1]=>[#TEST#CURRENCY#1]",
							"[!AA] 01000000000000000000.00000000000000000007 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [#TEST#CURRENCY#1]=>[*TEST*CURRENCY*1]",
							"[!AA] 01000000000000000000.00000000000000000008 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [*TEST*CURRENCY*1]=>[%TEST%CURRENCY%1]",
							"[!AA] 01000000000000000000.00000000000000000009 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [%TEST%CURRENCY%1]=>[~TEST~CURRENCY~1]",
							"[!AA] 02000000000000000000.f0000000000000000000 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [~TEST~CURRENCY~1]=>[#TEST#CURRENCY#1]",
							"[!AA] 02000000000000000000.f1000000000000000000 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [#TEST#CURRENCY#1]=>[*TEST*CURRENCY*1]",
							"[!AA] 02000000000000000000.f2000000000000000000 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [*TEST*CURRENCY*1]=>[%TEST%CURRENCY%1]",
							"[!AA] > Processing completed",
							"[!AA] > No relevant changes found",
							"[!AA] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: F2000000000000000000, Period: 1606)",
							"[!AA] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: F2000000000000000000, Period: 1606)",
					}
				);
			}
		}

		static void RunSubscriberManager(IAuditSubscriber[] testSubscribers, DbConnection auditConnection, LoggerForTest testLogger, string maxLsn, string maxLsnTime)
		{
			var subManager = new SubscriberManager(testLogger, "TST");

			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, maxLsn))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, maxLsnTime))
			{
				subManager.Run(CancellationToken.None, testSubscribers);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestBatchProcessingShouldAlwaysProcessAllRecordsWithChanges()
		{
			var refCurrencySchemaCols = new[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };
			var testSubscribers = new IAuditSubscriber[] {
				new GenericTestDataChangeSubscriber("!AA", "Dummy Test Data Changed Table List Subscriber 1", RefCurrencySchema.Instance, refCurrencySchemaCols) };

			var orgHeaderSchemaCols = new[] { new SchemaStringColumnForTest("OH_Code"), new SchemaStringColumnForTest("OH_FullName") };
			var redundantSubscribers = new IAuditSubscriber[] {
				new GenericTestDataChangeSubscriber("!AA", "Dummy Test Data Changed Table List Subscriber 2", OrgHeaderSchema.Instance, orgHeaderSchemaCols) };

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				auditConnection.ExecuteNonQuery($@"
						UPDATE {BiConstants.BiAdminSchemaName}.TableState SET AetHWMHistorySummaryLsn = 0xffffffffff WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
					");

				foreach (var subscriber in testSubscribers)
				{
					AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
				}

				addTestDataLsn1(auditConnection);
				addTestDataLsn2to4(auditConnection);
				addRedundantTestDataLsn2to4(auditConnection);

				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x04"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2016-06-04 00:00:00.0000"))
				{
					RepeatNotificationCycleUntilNoChangesAndAssert(
						testSubscribers,
						expectedLogs: new string[] {
								"[!AA] > Audit DB HWM: (LSN: 04000000000000000000, Period: 1606)",
								"[!AA] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1606)",
								"Running !AA Subscriber",
								"Required subscribers: !AA",
								"[!AA] > Processing 12 change(s)",
								"[!AA] 01000000000000000000.00000000000000000001 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [~TEST~CURRENCY~1]=>[#TEST#CURRENCY#1]",
								"[!AA] 01000000000000000000.00000000000000000002 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [#TEST#CURRENCY#1]=>[*TEST*CURRENCY*1]",
								"[!AA] 01000000000000000000.00000000000000000003 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [*TEST*CURRENCY*1]=>[%TEST%CURRENCY%1]",
								"[!AA] 01000000000000000000.00000000000000000004 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [%TEST%CURRENCY%1]=>[~TEST~CURRENCY~1]",
								"[!AA] 01000000000000000000.00000000000000000006 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [~TEST~CURRENCY~1]=>[#TEST#CURRENCY#1]",
								"[!AA] 01000000000000000000.00000000000000000007 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [#TEST#CURRENCY#1]=>[*TEST*CURRENCY*1]",
								"[!AA] 01000000000000000000.00000000000000000008 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [*TEST*CURRENCY*1]=>[%TEST%CURRENCY%1]",
								"[!AA] 01000000000000000000.00000000000000000009 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [%TEST%CURRENCY%1]=>[~TEST~CURRENCY~1]",
								"[!AA] 02000000000000000000.f0000000000000000000 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [~TEST~CURRENCY~1]=>[#TEST#CURRENCY#1]",
								"[!AA] 02000000000000000000.f1000000000000000000 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [#TEST#CURRENCY#1]=>[*TEST*CURRENCY*1]",
								"[!AA] 02000000000000000000.f2000000000000000000 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [*TEST*CURRENCY*1]=>[%TEST%CURRENCY%1]",
								"[!AA] 03000000000000000000.f3000000000000000000 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [%TEST%CURRENCY%1]=>[~TEST~CURRENCY~1]",
								"[!AA] > Processing completed",
								"[!AA] > No relevant changes found",
								"[!AA] > Updating Subscriber High Water Mark (LSN: 04000000000000000000, SeqVal: F4000000000000000000, Period: 1606)",
								"[!AA] > Updating Subscriber High Water Mark (LSN: 04000000000000000000, SeqVal: F4000000000000000000, Period: 1606)",
						}
					);
				}
			}
		}

		void addTestDataLsn1(DbConnection auditConnection)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @rows INT = 2
						DECLARE @iterations INT = 0
						DECLARE @Seq binary(10) = 0x00
						DECLARE @Cid INT = 0
						WHILE @iterations < @rows
						BEGIN
							INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsActive, RX_IsSystem) VALUES
							(1606, @Cid+1, 0x01, @Seq+1, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1', 1, 1),
							(1606, @Cid+2, 0x01, @Seq+1, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 1, 1),
							(1606, @Cid+3, 0x01, @Seq+2, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 1, 1),
							(1606, @Cid+4, 0x01, @Seq+2, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 1, 1),
							(1606, @Cid+5, 0x01, @Seq+3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 1, 1),
							(1606, @Cid+6, 0x01, @Seq+3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, @Cid+7, 0x01, @Seq+4, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, @Cid+8, 0x01, @Seq+4, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1', 1, 1),
							(1606, @Cid+9, 0x01, @Seq+5, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CA', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, @Cid+10, 0x01, @Seq+5, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CA', '~1', '%TEST%CURRENCY%1', 0, 0)
							SET @Seq = @Seq + 5
							SET @Cid = @Cid + 10
							SET @iterations = @iterations + 1
						END
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x01, '2016-06-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod, TranEndTimeUtc) VALUES
						(0x01, 'dbo', 'RefCurrency', 20, 1606, '2016-06-01');",
			BiConstants.BiAdminSchemaName);
			auditConnection.ExecuteNonQuery(sqlText);
		}

		void addTestDataLsn2to4(DbConnection auditConnection)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x02, '2016-06-02');
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x03, '2016-06-03');
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x04, '2016-06-04');

						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod, TranEndTimeUtc) VALUES
						(0x02, 'dbo', 'RefCurrency', 6, 1606, '2016-06-02');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod, TranEndTimeUtc) VALUES
						(0x03, 'dbo', 'RefCurrency', 2, 1606, '2016-06-03');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod, TranEndTimeUtc) VALUES
						(0x04, 'dbo', 'RefCurrency', 2, 1606, '2016-06-04');

						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsActive, RX_IsSystem) VALUES
							(1606, 10, 0x02, 0xF0, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1', 1, 1),
							(1606, 11, 0x02, 0xF0, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 1, 1),
							(1606, 12, 0x02, 0xF1, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 1, 1),
							(1606, 13, 0x02, 0xF1, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 1, 1),
							(1606, 14, 0x02, 0xF2, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 1, 1),
							(1606, 15, 0x02, 0xF2, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, 10, 0x03, 0xF3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, 11, 0x03, 0xF3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1', 1, 1),
							(1606, 10, 0x04, 0xF4, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CA', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, 11, 0x04, 0xF4, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CA', '~1', '%TEST%CURRENCY%1', 0, 0);",
			BiConstants.BiAdminSchemaName);
			auditConnection.ExecuteNonQuery(sqlText);
		}

		void addRedundantTestDataLsn2to4(DbConnection auditConnection)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod, TranEndTimeUtc) VALUES
						(0x02, 'dbo', 'OrgHeader', 6, 1606, '2016-06-02');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod, TranEndTimeUtc) VALUES
						(0x03, 'dbo', 'OrgHeader', 2, 1606, '2016-06-03');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod, TranEndTimeUtc) VALUES
						(0x04, 'dbo', 'OrgHeader', 2, 1606, '2016-06-04');

						INSERT dbo.OrgHeader ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OH_PK, OH_Code, OH_FullName, OH_IsActive) VALUES
							(1606, 20, 0x02, 0xF0, 3, 0x01, '5FEDE06A-E579-41F8-A2BF-E8B637A5EB53', '~1', '~TEST~ORGANIZATION~1', 1),
							(1606, 21, 0x02, 0xF0, 4, 0x01, '5FEDE06A-E579-41F8-A2BF-E8B637A5EB53', '~1', '#TEST#ORGANIZATION#1', 1),
							(1606, 22, 0x02, 0xF1, 3, 0x01, '5FEDE06A-E579-41F8-A2BF-E8B637A5EB53', '~1', '#TEST#ORGANIZATION#1', 1),
							(1606, 23, 0x02, 0xF1, 4, 0x01, '5FEDE06A-E579-41F8-A2BF-E8B637A5EB53', '~1', '*TEST*ORGANIZATION*1', 1),
							(1606, 24, 0x02, 0xF2, 3, 0x01, '5FEDE06A-E579-41F8-A2BF-E8B637A5EB53', '~1', '*TEST*ORGANIZATION*1', 1),
							(1606, 25, 0x02, 0xF2, 4, 0x01, '5FEDE06A-E579-41F8-A2BF-E8B637A5EB53', '~1', '%TEST%ORGANIZATION%1', 1),
							(1606, 20, 0x03, 0xF3, 3, 0x01, '5FEDE06A-E579-41F8-A2BF-E8B637A5EB53', '~1', '%TEST%ORGANIZATION%1', 1),
							(1606, 21, 0x03, 0xF3, 4, 0x01, '5FEDE06A-E579-41F8-A2BF-E8B637A5EB53', '~1', '~TEST~ORGANIZATION~1', 1),
							(1606, 20, 0x04, 0xF4, 3, 0x01, '5FEDE06A-E579-41F8-A2BF-E8B637A5EB53', '~1', '%TEST%ORGANIZATION%1', 1),
							(1606, 21, 0x04, 0xF4, 4, 0x01, '5FEDE06A-E579-41F8-A2BF-E8B637A5EB53', '~1', '%TEST%ORGANIZATION%1', 0);",
			BiConstants.BiAdminSchemaName);
			auditConnection.ExecuteNonQuery(sqlText);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestBatchProcessingShouldAlwaysSkipBatchesWithNoChangesAndContinue()
		{
			var refCurrencySchemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };

			var testSubscribers = new IAuditSubscriber[] {
				new GenericTestDataChangeSubscriber("!AA", "Dummy Test Data Changed Table List Subscriber 1", RefCurrencySchema.Instance, refCurrencySchemaCols) };

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				auditConnection.ExecuteNonQuery($@"
						UPDATE {BiConstants.BiAdminSchemaName}.TableState SET AetHWMHistorySummaryLsn = 0xffffffffff WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
					");
				foreach (var subscriber in testSubscribers)
				{
					AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
				}

				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @rows INT = 2
						DECLARE @iterations INT = 0
						DECLARE @Seq binary(10) = 0x00
						DECLARE @Cid INT = 0
						WHILE @iterations < @rows
						BEGIN
							INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsActive, RX_IsSystem) VALUES
							(1606, @Cid+1, 0x01, @Seq+1, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, @Cid+2, 0x01, @Seq+1, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 0, 0),
							(1606, @Cid+3, 0x01, @Seq+2, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 0, 0),
							(1606, @Cid+4, 0x01, @Seq+2, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, @Cid+5, 0x01, @Seq+3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, @Cid+6, 0x01, @Seq+3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 0, 0),
							(1606, @Cid+7, 0x01, @Seq+4, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 0, 0),
							(1606, @Cid+8, 0x01, @Seq+4, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, @Cid+9, 0x01, @Seq+5, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CA', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, @Cid+10, 0x01, @Seq+5, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CA', '~1', '%TEST%CURRENCY%1', 0, 0)
							SET @Seq = @Seq + 5
							SET @Cid = @Cid + 10
							SET @iterations = @iterations + 1
						END
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x01, '2016-06-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x01, 'dbo', 'RefCurrency', 20, 1606);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				string sqlText2 = string.Format(CultureInfo.InvariantCulture,
					@"INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x02, '2016-06-02');
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x03, '2016-06-03');
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x04, '2016-06-04');

						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x02, 'dbo', 'RefCurrency', 6, 1606);
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x03, 'dbo', 'RefCurrency', 2, 1606);
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x04, 'dbo', 'RefCurrency', 2, 1606);

						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsActive, RX_IsSystem) VALUES
							(1606, 10, 0x02, 0xF0, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1', 1, 1),
							(1606, 11, 0x02, 0xF0, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 1, 1),
							(1606, 12, 0x02, 0xF1, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 1, 1),
							(1606, 13, 0x02, 0xF1, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 1, 1),
							(1606, 14, 0x02, 0xF2, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 1, 1),
							(1606, 15, 0x02, 0xF2, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, 10, 0x03, 0xF3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, 11, 0x03, 0xF3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1', 1, 1),
							(1606, 10, 0x04, 0xF4, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CA', '~1', '%TEST%CURRENCY%1', 1, 1),
							(1606, 11, 0x04, 0xF4, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CA', '~1', '%TEST%CURRENCY%1', 0, 0);
						UPDATE [{0}].SubscriberControl SET LsnHighWaterMark = 0x01 where SubscriberCode = '!AA'",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText2);

				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x04"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2016-06-04 00:00:00.0000"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.AspBatchSize, "10"))
				{
					RepeatNotificationCycleUntilNoChangesAndAssert(
						testSubscribers,
						expectedLogs: new string[] {
								"[!AA] > Audit DB HWM: (LSN: 04000000000000000000, Period: 1606)",
								"[!AA] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1606)",
								"Running !AA Subscriber",
								"Required subscribers: !AA",
								"[!AA] > Processing 4 change(s)",
								"[!AA] 02000000000000000000.f0000000000000000000 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [~TEST~CURRENCY~1]=>[#TEST#CURRENCY#1]",
								"[!AA] 02000000000000000000.f1000000000000000000 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [#TEST#CURRENCY#1]=>[*TEST*CURRENCY*1]",
								"[!AA] 02000000000000000000.f2000000000000000000 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [*TEST*CURRENCY*1]=>[%TEST%CURRENCY%1]",
								"[!AA] 03000000000000000000.f3000000000000000000 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [%TEST%CURRENCY%1]=>[~TEST~CURRENCY~1]",
								"[!AA] > Processing completed",
								"[!AA] > No relevant changes found",
								"[!AA] > Updating Subscriber High Water Mark (LSN: 04000000000000000000, SeqVal: F4000000000000000000, Period: 1606)",
								"[!AA] > Updating Subscriber High Water Mark (LSN: 04000000000000000000, SeqVal: F4000000000000000000, Period: 1606)",
						}
					);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRunWithLsnPeriodOnMonthBoundaryUsesUtc()
		{
			var subscriberCode = "ABC";
			var subscribers = new[] { new GenericTestDataChangeSubscriber(subscriberCode, "Dummy Test Changed Table List Subscriber 1", AccAccountFeeSchema.Instance) };
			var logger = new LoggerForTest();
			var subscriberManager = new SubscriberManager(logger, "NMT");

			var serverUtcOffsetMinutes = Db.Connection.ExecuteScalar<int>("SELECT DATEDIFF(minute, GETUTCDATE(), GETDATE());");
			var testDateLocal = serverUtcOffsetMinutes switch
			{
				> 0 => "2024-05-01 0:05:0.000",
				< 0 => "2024-03-31 23:55:0.000",
				_ => "2024-04-15 0:00:0.000", // 0 or NaN
			};
			var expectedPeriod = 2404;

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x01"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, testDateLocal))
				{
					auditConnection.ExecuteNonQuery(@$"
						TRUNCATE TABLE biadmin.LsnTimeMapping;
						INSERT INTO biadmin.LsnTimeMapping (StartLsn, TranEndTimeUTC) VALUES (0x01, DATEADD(minute, {-serverUtcOffsetMinutes}, '{testDateLocal}'));
					");

					subscriberManager.Run(CancellationToken.None, subscribers);

					var subscriberPeriodWatermark = (int)auditConnection.ExecuteScalar<short>($"SELECT PeriodHighWaterMark FROM biadmin.SubscriberControl WHERE SubscriberCode = '{subscriberCode}';");
					AssertEquals("SubscriberControl period high water mark should use UTC time from LsnTimeMapping", expectedPeriod, subscriberPeriodWatermark);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRun()
		{
			var testSubscribers = new IAuditSubscriber[] {
				new GenericTestDataChangeSubscriber("!RN", "Dummy Test Changed Table List Subscriber 1", RefCountrySchema.Instance),
				new GenericTestDataChangeSubscriber("!GE","Dummy Test Changed Table List Subscriber 2", GlbDepartmentSchema.Instance),
				new GenericTestChangedTableListSubscriber("*XX", "Dummy Test Changed Table List Subscriber 3", new ITableSchema[] { GlbDepartmentSchema.Instance, GlbStaffSchema.Instance })
			};

			string sqlText = string.Empty;

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x01"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2018-03-14 00:00:00.0000"))
				{
					foreach (var subscriber in testSubscribers)
					{
						AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
					}

					auditConnection.ExecuteNonQuery($@"
							UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0xffffffffff WHERE SourceSchemaName = 'dbo' AND (SourceTableName = 'RefCountry' OR SourceTableName = 'GlbDepartment' OR SourceTableName = 'GlbStaff');
							UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x01 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCountry'
							INSERT dbo.RefCountry ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RN_PK, RN_Code, RN_Desc) VALUES
								(1803, 1, 0x01, 0x01, 2, 0x01, 'BA80F35A-C2B4-4E04-A40B-5602FDCF8796', '~1', '~Test~Country~1');
							INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
								(0x01, '2018-03-10');
							UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x01 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCountry'
							INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
								(0x01, 'dbo', 'RefCountry', 1, 1803);"
					);

					// Run it once to add to SubscriberControl
					RepeatNotificationCycleUntilNoChangesAndAssert(
						testSubscribers,
						expectedLogs: new string[] {
								"[!RN] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1803)",
								"[!RN] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"Running !RN Subscriber",
								"[!GE] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1803)",
								"[!GE] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"[!GE] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"[*XX] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1803)",
								"[*XX] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"[*XX] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"Required subscribers: !RN",
								"[!RN] > Processing 1 change(s)",
								"[!RN] 01000000000000000000.01000000000000000000 - Added    - PK [ba80f35a-c2b4-4e04-a40b-5602fdcf8796] = Code [~1] - Desc [~Test~Country~1]",
								"[!RN] > Processing completed",
								"[!RN] > No relevant changes found",
								"[!RN] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: 01000000000000000000, Period: 1803)",
								"[!RN] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: 01000000000000000000, Period: 1803)",
						}
					);
				}

				sqlText = $@"
							INSERT dbo.RefCountry ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RN_PK, RN_Code, RN_Desc) VALUES
								(1803, 11, 0x02, 0x01, 3, 0x01, 'BA80F35A-C2B4-4E04-A40B-5602FDCF8796', '~1', '~Test~Country~1'),
								(1803, 12, 0x02, 0x01, 4, 0x01, 'BA80F35A-C2B4-4E04-A40B-5602FDCF8796', '~1', '#Test#Country#1'),
								(1803, 13, 0x02, 0x02, 2, 0x01, 'A726E56E-2037-46F6-AE7C-BFF92F0C1EED', '~2', '~Test~Country~2'),
								(1803, 14, 0x02, 0x03, 1, 0x01, '40EA4DDA-5500-4849-AA36-E0B41931C581', '~3', '~Test~Country~3');
							INSERT dbo.GlbDepartment ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GE_PK, GE_Code, GE_Desc) VALUES
								(1803, 10, 0x03, 0x01, 2, 0x01, '1EAD206B-2948-4125-B077-073023F9DE8E', '~1', '~Test~Department~1'),
								(1803, 11, 0x03, 0x02, 1, 0x01, '78277244-89DE-4325-B004-2DEC00DA223A', '~2', '~Test~Department~2'),
								(1803, 12, 0x04, 0x02, 3, 0x01, '71FEC2EA-28B9-46F8-88AB-740B897B38C6', NULL, '~Test~Department~2'),
								(1803, 13, 0x04, 0x02, 4, 0x01, '71FEC2EA-28B9-46F8-88AB-740B897B38C6', 'NEW', '~Test~Department~2'),
								(1803, 14, 0x05, 0x02, 3, 0x01, '71FEC2EA-28B9-46F8-88AB-740B897B38C6', 'NEW', '~Test~Department~2'),
								(1803, 15, 0x05, 0x02, 4, 0x01, '71FEC2EA-28B9-46F8-88AB-740B897B38C6', NULL, '~Test~Department~2');
							INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
								(0x02, '2018-03-11'),
								(0x03, '2018-03-12'),
								(0x04, '2018-03-13'),
								(0x05, '2018-03-14');
							UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x02 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCountry'
							UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x05 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbDepartment'
							INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
								(0x02, 'dbo', 'RefCountry', 4, 1803),
								(0x03, 'dbo', 'GlbDepartment', 2, 1803),
								(0x04, 'dbo', 'GlbDepartment', 2, 1803),
								(0x05, 'dbo', 'GlbDepartment', 2, 1803);
						";
				auditConnection.ExecuteNonQuery(sqlText);

				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x05"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2018-03-14 00:00:00.0000"))
				{
					RepeatNotificationCycleUntilNoChangesAndAssert(
						testSubscribers,
						expectedLogs: new string[] {
								"[!RN] > Audit DB HWM: (LSN: 05000000000000000000, Period: 1803)",
								"[!RN] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"Running !RN Subscriber",
								"[!GE] > Audit DB HWM: (LSN: 05000000000000000000, Period: 1803)",
								"[!GE] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"Running !GE Subscriber",
								"[*XX] > Audit DB HWM: (LSN: 05000000000000000000, Period: 1803)",
								"[*XX] > Current Subscriber High Water Marks (LSN: 05000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"Running *XX Subscriber",
								"Required subscribers: !RN, !GE, *XX",
								"[!RN] > Processing 3 change(s)",
								"[!RN] 02000000000000000000.01000000000000000000 - Modified - PK [ba80f35a-c2b4-4e04-a40b-5602fdcf8796] = Code [~1]=>[~1] - Desc [~Test~Country~1]=>[#Test#Country#1]",
								"[!RN] 02000000000000000000.02000000000000000000 - Added    - PK [a726e56e-2037-46f6-ae7c-bff92f0c1eed] = Code [~2] - Desc [~Test~Country~2]",
								"[!RN] 02000000000000000000.03000000000000000000 - Deleted  - PK [40ea4dda-5500-4849-aa36-e0b41931c581] = Code [~3] - Desc [~Test~Country~3]",
								"[!RN] > Processing completed",
								"[!GE] > Processing 4 change(s)",
								"[!GE] 03000000000000000000.01000000000000000000 - Added    - PK [1ead206b-2948-4125-b077-073023f9de8e] = Code [~1] - Desc [~Test~Department~1]",
								"[!GE] 03000000000000000000.02000000000000000000 - Deleted  - PK [78277244-89de-4325-b004-2dec00da223a] = Code [~2] - Desc [~Test~Department~2]",
								"[!GE] 04000000000000000000.02000000000000000000 - Modified - PK [71fec2ea-28b9-46f8-88ab-740b897b38c6] = Code []=>[NEW] - Desc [~Test~Department~2]=>[~Test~Department~2]",
								"[!GE] 05000000000000000000.02000000000000000000 - Modified - PK [71fec2ea-28b9-46f8-88ab-740b897b38c6] = Code [NEW]=>[] - Desc [~Test~Department~2]=>[~Test~Department~2]",
								"[!GE] > Processing completed",
								"[!GE] > Updating Subscriber High Water Mark (LSN: 05000000000000000000, SeqVal: 02000000000000000000, Period: 1803)",
								"[*XX] [dbo].[GlbDepartment]",
								"[!RN] > No relevant changes found",
								"[!GE] > No relevant changes found",
								"[*XX] > Updating Subscriber High Water Mark (LSN: 05000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"[*XX] > Current Subscriber High Water Marks (LSN: 05000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"[!RN] > Updating Subscriber High Water Mark (LSN: 05000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"[!GE] > Updating Subscriber High Water Mark (LSN: 05000000000000000000, SeqVal: 02000000000000000000, Period: 1803)",
								"[*XX] > Current Subscriber High Water Marks (LSN: 05000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
						}
					);
				}

				sqlText = $@"
						INSERT dbo.GlbDepartment ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GE_PK, GE_Code, GE_Desc) VALUES
							(1803, 10, 0x06, 0x01, 1, 0x01, 'C51C9A14-C771-4543-AAA1-F09E4AC1436F', '~3', '~Test~Department~3'),
							(1803, 11, 0x06, 0x02, 2, 0x01, '5C729056-95B9-4D12-9887-0FEC7566101C', '~4', '~Test~Department~4');
						INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x06, '2018-03-14');
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x06 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbDepartment'
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
							(0x06, 'dbo', 'GlbDepartment', 2, 1803);
						";
				auditConnection.ExecuteNonQuery(sqlText);

				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x06"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2018-03-14 00:00:00.0000"))
				{
					RepeatNotificationCycleUntilNoChangesAndAssert(
						testSubscribers,
						expectedLogs: new string[] {
								"[!RN] > Audit DB HWM: (LSN: 06000000000000000000, Period: 1803)",
								"[!GE] > Audit DB HWM: (LSN: 06000000000000000000, Period: 1803)",
								"[!GE] > Current Subscriber High Water Marks (LSN: 05000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"[*XX] > Audit DB HWM: (LSN: 06000000000000000000, Period: 1803)",
								"[*XX] > Current Subscriber High Water Marks (LSN: 06000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"Running *XX Subscriber",
								"Running !GE Subscriber",
								"Required subscribers: !GE, *XX",
								"[!GE] > Processing 2 change(s)",
								"[!GE] 06000000000000000000.01000000000000000000 - Deleted  - PK [C51C9A14-C771-4543-AAA1-F09E4AC1436F] = Code [~3] - Desc [~Test~Department~3]",
								"[!GE] 06000000000000000000.02000000000000000000 - Added    - PK [5C729056-95B9-4D12-9887-0FEC7566101C] = Code [~4] - Desc [~Test~Department~4]",
								"[!GE] > Processing completed",
								"[!GE] > Updating Subscriber High Water Mark (LSN: 06000000000000000000, SeqVal: 02000000000000000000, Period: 1803)",
								"[*XX] [dbo].[GlbDepartment]",
								"[!GE] > No relevant changes found",
								"[*XX] > Updating Subscriber High Water Mark (LSN: 06000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"[*XX] > Current Subscriber High Water Marks (LSN: 06000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"[!GE] > Updating Subscriber High Water Mark (LSN: 06000000000000000000, SeqVal: 02000000000000000000, Period: 1803)",
						}
					);

					RepeatNotificationCycleUntilNoChangesAndAssert(
						testSubscribers,
						expectedLogs: new string[] {
								"[!RN] > Audit DB HWM: (LSN: 06000000000000000000, Period: 1803)",
								"[!GE] > Audit DB HWM: (LSN: 06000000000000000000, Period: 1803)",
								"[*XX] > Audit DB HWM: (LSN: 06000000000000000000, Period: 1803)",
								"[*XX] > Current Subscriber High Water Marks (LSN: 06000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"[*XX] > Updating Subscriber High Water Mark (LSN: 06000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
}
					);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRunningSubscriberForTheFirstTimeDoesNotLoadTheEntireBacklogOfChanges()
		{
			var testSubscribers = new IAuditSubscriber[] {
				new GenericTestDataChangeSubscriber("!RN", "Dummy Test Changed Table List Subscriber 1", RefCountrySchema.Instance),
				new GenericTestDataChangeSubscriber("!GE","Dummy Test Changed Table List Subscriber 2", GlbDepartmentSchema.Instance),
				new GenericTestChangedTableListSubscriber("*XX", "Dummy Test Changed Table List Subscriber 3", new ITableSchema[] { GlbDepartmentSchema.Instance, GlbStaffSchema.Instance })
			};

			string sqlText = string.Empty;

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				auditConnection.ExecuteNonQuery("INSERT INTO biadmin.LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x02, '2018-03-01')");
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x02"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2018-03-01 00:00:00.0000"))
				{
					auditConnection.ExecuteNonQuery($@"
							UPDATE {BiConstants.BiAdminSchemaName}.TableState SET AetHWMHistorySummaryLsn = 0xffffffffff WHERE SourceSchemaName = 'dbo' AND (SourceTableName = 'RefCountry' OR SourceTableName = 'GlbDepartment' OR SourceTableName = 'GlbStaff')
						");

					foreach (var subscriber in testSubscribers)
					{
						AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
					}

					// Run it once to add to SubscriberControl
					RepeatNotificationCycleUntilNoChangesAndAssert(
						testSubscribers,
						expectedLogs: new string[] {
								"[!RN] > Audit DB HWM: (LSN: 02000000000000000000, Period: 1803)",
								"[!RN] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"[!RN] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"[!GE] > Audit DB HWM: (LSN: 02000000000000000000, Period: 1803)",
								"[!GE] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"[!GE] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"[*XX] > Audit DB HWM: (LSN: 02000000000000000000, Period: 1803)",
								"[*XX] > Current Subscriber High Water Marks (LSN: 02000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"[*XX] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
						}
					);

					// Add backlog
					sqlText = $@"
							UPDATE [{BiConstants.BiAdminSchemaName}].SubscriberControl SET PeriodHighWaterMark = 1803, LsnHighWaterMark = 0x01 WHERE SubscriberCode = '!RN';
							UPDATE [{BiConstants.BiAdminSchemaName}].SubscriberControl SET PeriodHighWaterMark = 1803, LsnHighWaterMark = 0x01 WHERE SubscriberCode = '!GE';

							INSERT dbo.RefCountry ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RN_PK, RN_Code, RN_Desc) VALUES
								(1803, 1, 0x01, 0x01, 2, 0x01, 'BA80F35A-C2B4-4E04-A40B-5602FDCF8796', '~1', '~Test~Country~1'),
								(1803, 1, 0x02, 0x01, 3, 0x01, 'BA80F35A-C2B4-4E04-A40B-5602FDCF8796', '~1', '~Test~Country~1'),
								(1803, 1, 0x02, 0x01, 4, 0x01, 'BA80F35A-C2B4-4E04-A40B-5602FDCF8796', '~1', '#Test#Country#1'),
								(1803, 1, 0x02, 0x02, 2, 0x01, 'A726E56E-2037-46F6-AE7C-BFF92F0C1EED', '~2', '~Test~Country~2'),
								(1803, 1, 0x02, 0x03, 1, 0x01, '40EA4DDA-5500-4849-AA36-E0B41931C581', '~3', '~Test~Country~3');
							INSERT dbo.GlbDepartment ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GE_PK, GE_Code, GE_Desc) VALUES
								(1803, 1, 0x01, 0x01, 2, 0x01, '1EAD206B-2948-4125-B077-073023F9DE8E', '~1', '~Test~Department~1'),
								(1803, 1, 0x01, 0x02, 1, 0x01, '78277244-89DE-4325-B004-2DEC00DA223A', '~2', '~Test~Department~2'),
								(1803, 1, 0x02, 0x02, 3, 0x01, '71FEC2EA-28B9-46F8-88AB-740B897B38C6', NULL, '~Test~Department~2'),
								(1803, 1, 0x02, 0x02, 4, 0x01, '71FEC2EA-28B9-46F8-88AB-740B897B38C6', 'NEW', '~Test~Department~2'),
								(1803, 1, 0x03, 0x02, 3, 0x01, '71FEC2EA-28B9-46F8-88AB-740B897B38C6', 'NEW', '~Test~Department~2'),
								(1803, 1, 0x03, 0x02, 4, 0x01, '71FEC2EA-28B9-46F8-88AB-740B897B38C6', NULL, '~Test~Department~2');
							INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
								(0x01, '2018-03-10'),
								(0x03, '2018-03-12'),
								(0x04, '2018-03-13'),
								(0x05, '2018-03-14');
							UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x02 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCountry'
							UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x05 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbDepartment'
							INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
								(0x01, 'dbo', 'RefCountry', 1, 1803),
								(0x02, 'dbo', 'RefCountry', 4, 1803),
								(0x03, 'dbo', 'GlbDepartment', 2, 1803),
								(0x04, 'dbo', 'GlbDepartment', 2, 1803),
								(0x05, 'dbo', 'GlbDepartment', 2, 1803);
						";
					auditConnection.ExecuteNonQuery(sqlText);
				}

				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x05"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2018-03-14 00:00:00.0000"))
				{
					RepeatNotificationCycleUntilNoChangesAndAssert(
						testSubscribers,
						expectedLogs: new string[] {
								"[!RN] > Audit DB HWM: (LSN: 05000000000000000000, Period: 1803)",
								"[!RN] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)", // LSN != 0x02 bc of the added backlog
								"Running !RN Subscriber",
								"[!GE] > Audit DB HWM: (LSN: 05000000000000000000, Period: 1803)",
								"[!GE] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)", // LSN != 0x02 bc of the added backlog
								"Running !GE Subscriber",
								"[*XX] > Audit DB HWM: (LSN: 05000000000000000000, Period: 1803)",
								"[*XX] > Current Subscriber High Water Marks (LSN: 05000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"Running *XX Subscriber",
								"Required subscribers: !RN, !GE, *XX",
								"[!GE] > Updating Subscriber High Water Mark (LSN: 03000000000000000000, SeqVal: 02000000000000000000, Period: 1803)",
								"[*XX] > Updating Subscriber High Water Mark (LSN: 05000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"[!RN] > Processing 3 change(s)",
								"[!RN] 02000000000000000000.01000000000000000000 - Modified - PK [ba80f35a-c2b4-4e04-a40b-5602fdcf8796] = Code [~1]=>[~1] - Desc [~Test~Country~1]=>[#Test#Country#1]",
								"[!RN] 02000000000000000000.02000000000000000000 - Added    - PK [a726e56e-2037-46f6-ae7c-bff92f0c1eed] = Code [~2] - Desc [~Test~Country~2]",
								"[!RN] 02000000000000000000.03000000000000000000 - Deleted  - PK [40ea4dda-5500-4849-aa36-e0b41931c581] = Code [~3] - Desc [~Test~Country~3]",
								"[!RN] > Processing completed",
								"[!RN] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: 03000000000000000000, Period: 1803)",
								"[!GE] > Processing 2 change(s)",
								"[!GE] 02000000000000000000.02000000000000000000 - Modified - PK [71fec2ea-28b9-46f8-88ab-740b897b38c6] = Code []=>[NEW] - Desc [~Test~Department~2]=>[~Test~Department~2]",
								"[!GE] 03000000000000000000.02000000000000000000 - Modified - PK [71fec2ea-28b9-46f8-88ab-740b897b38c6] = Code [NEW]=>[] - Desc [~Test~Department~2]=>[~Test~Department~2]",
								"[!GE] > Processing completed",
								"[*XX] [dbo].[GlbDepartment]",
								"[!RN] > No relevant changes found",
								"[!GE] > No relevant changes found",
								"[*XX] > Current Subscriber High Water Marks (LSN: 05000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"[!RN] > Updating Subscriber High Water Mark (LSN: 05000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
								"[!GE] > Updating Subscriber High Water Mark (LSN: 05000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1803)",
						}
					);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRunWithExceptions()
		{
			var testSubscribers = new IAuditSubscriber[] {
				new GenericTestDataChangeSubscriber("!RN","Dummy Test Data Change Subscriber RN", RefCountrySchema.Instance),
				new GenericTestDataChangeSubscriber("!~1","Dummy Test Data Change Subscriber 1", StmJobQueueSchema.Instance),
				new GenericTestDataChangeSubscriber("!~2","Dummy Test Data Change Subscriber 2", null),
				new GenericTestChangedTableListSubscriber("*RN", "Dummy Test Changed Table List Subscriber RN", new ITableSchema[] { RefCountrySchema.Instance }),
				new GenericTestChangedTableListSubscriber("*^3", "Dummy Test Changed Table List Subscriber 3", null),
				new GenericTestChangedTableListSubscriber("*^4", "Dummy Test Changed Table List Subscriber 4", Array.Empty<ITableSchema>()),
				new GenericTestChangedTableListSubscriber("*^5", "Dummy Test Changed Table List Subscriber 5", new ITableSchema[] { RefCountrySchema.Instance, null })
			};

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x00"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2000-01-10 00:00:00.0000"))
				{
					foreach (var sub in testSubscribers)
					{
						sub.GetWrapper(auditConnection, new LoggerForTest()).AddSubscriberToSubscriberControlTable();
					}
				}

				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x01"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2017-12-03 00:00:00.0000"))
				{
					auditConnection.ExecuteNonQuery($@"
							UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0xffffffffff WHERE SourceSchemaName = 'dbo' AND (SourceTableName = 'RefCountry' OR SourceTableName = 'StmJobQueue');
							INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x01, 'dbo', 'RefCountry', 1, 1706);
							INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES (0x00, 'dbo', 'StmJobQueue', 1, 1706);
						");

					string sqlText = $@"
							INSERT dbo.RefCountry ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RN_PK, RN_Code, RN_Desc) VALUES
								(1712, 1, 0x01, 0x01, 1, 0x001, '84FE72F0-0A3F-4392-9B49-4CC1E1D6B33A', '~1', '~Test~Country~1');
							INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x01, '2017-12-03');
							UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x01 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCountry'
							INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
								(0x01, 'dbo', 'RefCountry', 1, 1712);
						";
					auditConnection.ExecuteNonQuery(sqlText);

					RunNotificationCycleOnceAndAssert(
						testSubscribers,
						auditConnection,
						expectedLogs: new string[] {
								"[!RN] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1712)",
								"[!RN] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1706)",
								"Running !RN Subscriber",
								"[!~1] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1712)",
								"[!~2] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1712)",
								"[*RN] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1712)",
								"[*RN] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
								"Running *RN Subscriber",
								"[*RN] [dbo].[RefCountry]",
								"[*^3] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1712)",
								"[*^4] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1712)",
								"[*^5] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1712)",
								"[!RN] > Processing 1 change(s)",
								"[!RN] 01000000000000000000.01000000000000000000 - Deleted  - PK [84fe72f0-0a3f-4392-9b49-4cc1e1d6b33a] = Code [~1] - Desc [~Test~Country~1]",
								"[!RN] > Processing completed",
								"[!RN] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: 01000000000000000000, Period: 1712)",
								"[*RN] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 0)",
						}
					);
				}
			}
		}

		void RepeatNotificationCycleUntilNoChangesAndAssert(IAuditSubscriber[] testSubscribers, string[] expectedLogs)
		{
			var testLogger = new LoggerForTest();
			var testNotificationManager = new SubscriberManagerForTest(testLogger, testSubscribers);
			testNotificationManager.Run();
			AuditTestHelper.AssertLog(testLogger.LogEntries, expectedLogs);
		}

		void RunNotificationCycleOnceAndAssert(IAuditSubscriber[] testSubscribers, DbConnection auditConnection, string[] expectedLogs)
		{
			var testLogger = new LoggerForTest();
			var testNotificationManager = new SubscriberManagerForTest(testLogger, testSubscribers);
			testNotificationManager.RunSubscribersOnce(auditConnection);
			AuditTestHelper.AssertLog(testLogger.LogEntries, expectedLogs);
		}

		public void TestLoadAllSubscribers()
		{
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var notificationManager = new NotificationManagerForSubscriberLoadingTest(null);
				var subscribers = notificationManager.LoadAllSubscribers_Exposed();

				AssertEquals("Any subscribers?", true, subscribers.Any());
				AssertEquals("DummyOrgHeaderSubscriber included?", true, subscribers.Count(s => s.Code == "~OH") == 1);
				AssertEquals("DummyRefCountrySubscriber included?", true, subscribers.Count(s => s.Code == "~RN") == 1);
				AssertEquals("DummyChangedTableListSubscriber included?", true, subscribers.Count(s => s.Code == "^CT") == 1);
				AssertEquals("DummyNotRequiredSubscriber included?", true, subscribers.Any(s => s.Code == "~**"));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRemoveObsoleteSubscribers()
		{
			var testSubscribers = new IAuditSubscriber[] {
				new GenericTestDataChangeSubscriber("TS1","Dummy Test Subscriber 1", null),
				new GenericTestDataChangeSubscriber("TS2","Dummy Test Subscriber 2", null),
				new GenericTestDataChangeSubscriber("TS3","Dummy Test Subscriber 3", null)
			};

			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				foreach (var subscriber in testSubscribers)
				{
					AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
				}

				CombineAssertions(() =>
				{
					CheckSubscriberExists(auditConnection, "TS1", exists: true);
					CheckSubscriberExists(auditConnection, "TS2", exists: true);
					CheckSubscriberExists(auditConnection, "TS3", exists: true);
				});

				SubscriberManager.RemoveObsoleteSubscribers(auditConnection);

				CombineAssertions(() =>
				{
					CheckSubscriberExists(auditConnection, "TS1", exists: false);
					CheckSubscriberExists(auditConnection, "TS2", exists: false);
					CheckSubscriberExists(auditConnection, "TS3", exists: false);
				});
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestRunDisposesThreadConnections()
		{
			var testSubscribers = new IAuditSubscriber[] { new GenericTestDataChangeSubscriber("!GS", "Test Subscriber", GlbStaffSchema.Instance) };
			var testLogger = new DummyLogger();
			var notificationManager = new SubscriberManagerForTest(testLogger, testSubscribers);
			notificationManager.Run();
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRunSameSubscriberAtSameTimeFails()
		{
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				var subscriber = new GenericTestDataChangeSubscriber("!RC", "Test Subscriber", RefCountrySchema.Instance);
				var testSubscribers = new IAuditSubscriber[] { subscriber };
				var testLogger = new LoggerForTest();

				using (var anotherConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(anotherConnection, BiConstants.LastMaxLsnProcessed, "0x01"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(anotherConnection, BiConstants.LastMaxLsnTimeProcessed, "2017-12-03 00:00:00.0000"))
				{
					SqlApplicationLock biLock;
					if (!anotherConnection.TryGetLock(BiConstants.AspMaintainanceLockKey + "NMT", TimeSpan.Zero, out biLock))
					{
						Fail("Failed to acquire initial subscriber lock.");
					}
					using (biLock)
					{
						auditConnection.ExecuteNonQuery($@"
								UPDATE {BiConstants.BiAdminSchemaName}.TableState SET AetHWMHistorySummaryLsn = 0xffffffffff WHERE SourceSchemaName = 'dbo' AND (SourceTableName = 'RefCurrency' OR SourceTableName = 'RefCountry')
							");

						AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
						string sqlText = string.Format(CultureInfo.InvariantCulture, @"
								INSERT dbo.RefCountry ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RN_PK, RN_Code, RN_Desc) VALUES
									(1712, 1, 0x01, 0x01, 1, 0x001, '84FE72F0-0A3F-4392-9B49-4CC1E1D6B33A', '~1', '~Test~Country~1');
								INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x01, '2017-12-03');
								INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
									(0x00, 'dbo', 'RefCountry', 1, 1712);
								",
							BiConstants.BiAdminSchemaName);
						auditConnection.ExecuteNonQuery(sqlText);

						var notificationManager = new SubscriberManagerForTest(testLogger, testSubscribers);
						notificationManager.RunSubscribersOnce(auditConnection);

						AuditTestHelper.AssertLog(
							testLogger.LogEntries,
							expectedLogs: new string[] {
									"[!RC] > Audit DB HWM: (LSN: 01000000000000000000, Period: 1712)",
									"[!RC] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 1712)",
									"Running !RC Subscriber",
									"Subscriber task NMT run skipped due to lock"
							}
						);
					}
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestHasChangesToProcess()
		{
			var testSubscribers = new IAuditSubscriber[] { new GenericTestDataChangeSubscriber("!GS", "Test Subscriber", GlbStaffSchema.Instance) };
			var testLogger = new LoggerForTest();
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				foreach (var subscriber in testSubscribers)
				{
					AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
				}
				auditConnection.ExecuteNonQuery($@"
								UPDATE {BiConstants.BiAdminSchemaName}.TableState SET AetHWMHistorySummaryLsn = 0xffffffffff WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbStaff'
							");
				var notificationManager = new SubscriberManagerForTest(testLogger, testSubscribers);

				var result = notificationManager.HasChangesToProcess_Exposed(testSubscribers[0], auditConnection);

				Assert(result);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGetSubscriberCommandIdHighWaterMarkCache()
		{
			var testSubscribers = new IAuditSubscriber[] { new GenericTestDataChangeSubscriber("!GS", "Test Subscriber", GlbStaffSchema.Instance) };
			var testLogger = new LoggerForTest();
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				foreach (var subscriber in testSubscribers)
				{
					AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
				}
				auditConnection.ExecuteNonQuery($@"
						UPDATE {BiConstants.BiAdminSchemaName}.SubscriberControl SET CommandIdHighWaterMark = 12345678 WHERE SubscriberCode = '!GS'
					");
				var notificationManager = new SubscriberManagerForTest(testLogger, testSubscribers);

				Assert("!GS should exist in the cache", notificationManager.SubScriberCommandIdHighWaterMarkCache_Exposed.ContainsKey("!GS"));
				AssertEquals("LsnHWM cache should be the same as the value in the database", 12345678, notificationManager.SubScriberCommandIdHighWaterMarkCache_Exposed["!GS"]);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGetSubscriberLsnHighWaterMarkCache()
		{
			var testSubscribers = new IAuditSubscriber[] { new GenericTestDataChangeSubscriber("!GS", "Test Subscriber", GlbStaffSchema.Instance) };
			var testLogger = new LoggerForTest();
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				foreach (var subscriber in testSubscribers)
				{
					AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
				}
				auditConnection.ExecuteNonQuery($@"
						UPDATE {BiConstants.BiAdminSchemaName}.SubscriberControl SET LsnHighWaterMark = 0xffffffffff WHERE SubscriberCode = '!GS'
					");
				var notificationManager = new SubscriberManagerForTest(testLogger, testSubscribers);

				Assert("!GS should exist in the cache", notificationManager.SubscriberLsnHighWaterMarkCache_Exposed.ContainsKey("!GS"));
				AssertEquals("LsnHWM cache should be the same as the value in the database", new byte[] { 255, 255, 255, 255, 255, 0, 0, 0, 0, 0 }, notificationManager.SubscriberLsnHighWaterMarkCache_Exposed["!GS"].Value);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGetTableStateLsnHighWaterMarkCache()
		{
			var testSubscribers = new IAuditSubscriber[] { new GenericTestDataChangeSubscriber("!GS", "Test Subscriber", GlbStaffSchema.Instance) };
			var testLogger = new LoggerForTest();
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				auditConnection.ExecuteNonQuery($@"
						UPDATE {BiConstants.BiAdminSchemaName}.TableState SET AetHWMHistorySummaryLsn = 0xffffffffff WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbStaff'
					");
				var notificationManager = new SubscriberManagerForTest(testLogger, testSubscribers);

				Assert("[dbo].[GlbStaff] should exist in the cache", notificationManager.TableStateLsnHighWaterMarkCache_Exposed.ContainsKey("[dbo].[GlbStaff]"));
				AssertEquals("LsnHWM cache should be the same as the value in the database", new byte[] { 255, 255, 255, 255, 255, 0, 0, 0, 0, 0 }, notificationManager.TableStateLsnHighWaterMarkCache_Exposed["[dbo].[GlbStaff]"].Value);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		[ExpectNoExceptions]
		public void TestReadFromHRMSchemaWithTableValuePairSubscriber()
		{
			var tableAndColumnDictionary = new Dictionary<ITableSchema, SchemaColumn>()
			{
				{ GlbStaffReviewSchema.Instance, GlbStaffReviewSchema.PK }
			};

			var testSubscribers = new IAuditSubscriber[] {
				new GenericTestTableValuePairSubscriber("*UT", "Unit Test")
			};

			var testLogger = new LoggerForTest();
			var notificationManager = new SubscriberManagerForTest(testLogger, testSubscribers);

			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x01"))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2017-12-03 00:00:00.0000"))
			{
				var sqlText = $@"
						UPDATE {BiConstants.BiAdminSchemaName}.TableState SET AetHWMHistorySummaryLsn = 0x01 WHERE SourceSchemaName = 'hrm' AND SourceTableName = 'GlbStaffReview'
						INSERT hrm.GlbStaffReview ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [GSV_AutoEffectiveEndDate], [GSV_EffectiveDate], [GSV_GS_NKReviewer], [GSV_GS_Staff], [GSV_PK], [GSV_Score]) VALUES
							(1712, 1, 0x01, 0x01, 1, 0x001, CAST('2007-05-08 12:35:29.1234567 +12:15' AS datetimeoffset(7)), CAST('2007-05-08 12:35:29.1234567 +12:15' AS datetimeoffset(7)), 'TST', '84FE72F0-0A3F-4392-9B49-4CC1E1D6B33A', '71FEC2EA-28B9-46F8-88AB-740B897B38C6', 1);
						INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x01, '2017-12-03');
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
									(0x00, 'hrm', 'GlbStaffReview', 1, 1712);";
				auditConnection.ExecuteNonQuery(sqlText);
				foreach (var subscriber in testSubscribers)
				{
					AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
				}

				notificationManager.Run();
			}
		}

		public void TestHandleInfrastructureDbError()
		{
			var testLogger = new BetterLoggerForTest();
			var manager = new SubscriberManagerForInfrastructureDbErrorTest(testLogger);
			var errorMessage = "8645";
			var subscriberWarraper = new ErrorTestWrapper("TST", errorMessage);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() =>
				{
					manager.RunSubscribers(new IAuditSubscriberWrapper[] { subscriberWarraper } , new CancellationToken());
				});
				var actual = testLogger.Logs.Select(log => log.message);
				AssertContainsExactElementsInAnyOrder(new string[] { $"[{subscriberWarraper.Code}] An infrastructure Db error occurred while processing subscriber changes.{System.Environment.NewLine}{errorMessage}" }, actual);
			});
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRunSubscribersInProvidedOrder()
		{
			var testSubscribers = new IAuditSubscriber[] {
				new GenericTestDataChangeSubscriber("!RN", "Dummy Test RefCountry Data Change Subscriber 1", RefCountrySchema.Instance),
				new GenericTestDataChangeSubscriber("!RX","Dummy Test RefCurrency Data Change Subscriber 2", RefCurrencySchema.Instance),
				new GenericTestDataChangeSubscriber("!AR","Dummy Test AccGroups Data Change Subscriber 3", AccGroupsSchema.Instance),
				new GenericTestDataChangeSubscriber("!GE", "Dummy Test GlbDepartment Data Change Subscriber 4", GlbDepartmentSchema.Instance),
				new GenericTestDataChangeSubscriber("!GG", "Dummy Test GlbGroup Data Change Subscriber 5", GlbGroupSchema.Instance),
				new GenericTestDataChangeSubscriber("!SE", "Dummy Test StmEvent Data Change Subscriber 6", StmEventSchema.Instance),
				new GenericTestDataChangeSubscriber("!OG", "Dummy Test OrgCreditorGroup Data Change Subscriber 7", OrgCreditorGroupSchema.Instance),
				new GenericTestDataChangeSubscriber("!OJ", "Dummy Test OrgDebtorGroup Data Change Subscriber 8", OrgDebtorGroupSchema.Instance),
			};

			string sqlText = string.Empty;

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x01"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2024-04-01 00:00:00.0000"))
				{
					foreach (var subscriber in testSubscribers)
					{
						AddSubscriber(auditConnection, subscriber.Code, subscriber.Description);
					}

					auditConnection.ExecuteNonQuery($@"
							UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x01 WHERE SourceSchemaName = 'dbo' AND SourceTableName IN ('RefCountry', 'RefCurrency', 'AccGroups', 'GlbDepartment', 'GlbGroup', 'StmEvent', 'OrgCreditorGroup', 'OrgDebtorGroup');
							INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
								(0x01, '2024-04-10');
						");

					// Run it once to add to SubscriberControl
					RepeatNotificationCycleUntilNoChangesAndAssert(
						testSubscribers,
						expectedLogs: new string[] {
								"[!RN] > Audit DB HWM: (LSN: 01000000000000000000, Period: 2404)",
								"[!RN] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!RN] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!RX] > Audit DB HWM: (LSN: 01000000000000000000, Period: 2404)",
								"[!RX] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!RX] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!AR] > Audit DB HWM: (LSN: 01000000000000000000, Period: 2404)",
								"[!AR] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!AR] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!GE] > Audit DB HWM: (LSN: 01000000000000000000, Period: 2404)",
								"[!GE] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!GE] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!GG] > Audit DB HWM: (LSN: 01000000000000000000, Period: 2404)",
								"[!GG] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!GG] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!SE] > Audit DB HWM: (LSN: 01000000000000000000, Period: 2404)",
								"[!SE] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!SE] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!OG] > Audit DB HWM: (LSN: 01000000000000000000, Period: 2404)",
								"[!OG] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!OG] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!OJ] > Audit DB HWM: (LSN: 01000000000000000000, Period: 2404)",
								"[!OJ] > Current Subscriber High Water Marks (LSN: 00000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!OJ] > Updating Subscriber High Water Mark (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
						}
					);
				}

				sqlText = $@"
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x02 WHERE SourceSchemaName = 'dbo' AND SourceTableName IN ('RefCountry', 'RefCurrency', 'AccGroups', 'GlbDepartment', 'GlbGroup', 'StmEvent', 'OrgCreditorGroup', 'OrgDebtorGroup');
						INSERT dbo.RefCountry ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RN_PK, RN_Code, RN_Desc) 
						VALUES
							(2404, 1, 0x02, 0x01, 2, 0x01, 'f09047a6-35a0-4e25-afee-4c495ebf312a', 'A', 'A Country');
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc) 
						VALUES
							(2404, 1, 0x02, 0x02, 2, 0x01, '09a60371-9aa3-40ce-89f3-525502868fce', 'B', 'B Currency');
						INSERT dbo.AccGroups ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], AR_PK, AR_Code, AR_Desc) 
						VALUES
							(2404, 1, 0x02, 0x03, 2, 0x01, 'ca8cb304-3b15-4182-ae34-b6383dc5a8af', 'C', 'C AccGroup');
						INSERT dbo.GlbDepartment ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GE_PK, GE_Code, GE_Desc) 
						VALUES
							(2404, 1, 0x02, 0x04, 2, 0x01, 'cf6f4ec6-a234-408b-ac94-c32b994d2f74', 'D', 'D Dept');
						INSERT dbo.GlbGroup ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GG_PK, GG_Code, GG_Desc) 
						VALUES
							(2404, 1, 0x02, 0x05, 2, 0x01, '4382e58d-920e-476a-be6e-009d78e39cda', 'E', 'E Group');
						INSERT dbo.StmEvent ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], SE_PK, SE_Code, SE_Desc) 
						VALUES
							(2404, 1, 0x02, 0x06, 2, 0x01, 'd41f66ef-6c2e-4d36-aea7-22396aa354db', 'F', 'F Event');
						INSERT dbo.OrgCreditorGroup ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code, OG_Desc) 
						VALUES
							(2404, 1, 0x02, 0x07, 2, 0x01, '35712f7c-eec8-4232-9c65-faba3fd48869', 'G', 'G Creditor');
						INSERT dbo.OrgDebtorGroup ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OJ_PK, OJ_Code, OJ_Desc) 
						VALUES
							(2404, 1, 0x02, 0x08, 2, 0x01, '3182f05e-2eb6-441d-a59f-3331704f63bb', 'H', 'H Debtor');
						INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x02, '2024-04-11');
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x01 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'DtbBookingConsolidation'
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
							(0x02, 'dbo', 'RefCountry', 1, 2404),
							(0x02, 'dbo', 'RefCurrency', 1, 2404),
							(0x02, 'dbo', 'AccGroups', 1, 2404),
							(0x02, 'dbo', 'GlbDepartment', 1, 2404),
							(0x02, 'dbo', 'GlbGroup', 1, 2404),
							(0x02, 'dbo', 'StmEvent', 1, 2404),
							(0x02, 'dbo', 'OrgCreditorGroup', 1, 2404),
							(0x02, 'dbo', 'OrgDebtorGroup', 1, 2404);
						";
				auditConnection.ExecuteNonQuery(sqlText);

				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x02"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2024-04-14 00:00:00.0000"))
				{
					RepeatNotificationCycleUntilNoChangesAndAssert(
						testSubscribers,
						expectedLogs: new string[] {
								"[!RN] > Audit DB HWM: (LSN: 02000000000000000000, Period: 2404)",
								"[!RN] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"Running !RN Subscriber",
								"[!RX] > Audit DB HWM: (LSN: 02000000000000000000, Period: 2404)",
								"[!RX] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"Running !RX Subscriber",
								"[!AR] > Audit DB HWM: (LSN: 02000000000000000000, Period: 2404)",
								"[!AR] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"Running !AR Subscriber",
								"[!GE] > Audit DB HWM: (LSN: 02000000000000000000, Period: 2404)",
								"[!GE] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"Running !GE Subscriber",
								"[!GG] > Audit DB HWM: (LSN: 02000000000000000000, Period: 2404)",
								"[!GG] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"Running !GG Subscriber",
								"[!SE] > Audit DB HWM: (LSN: 02000000000000000000, Period: 2404)",
								"[!SE] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"Running !SE Subscriber",
								"[!OG] > Audit DB HWM: (LSN: 02000000000000000000, Period: 2404)",
								"[!OG] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"Running !OG Subscriber",
								"[!OJ] > Audit DB HWM: (LSN: 02000000000000000000, Period: 2404)",
								"[!OJ] > Current Subscriber High Water Marks (LSN: 01000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"Running !OJ Subscriber",
								"Required subscribers: !RN, !RX, !AR, !GE, !GG, !SE, !OG, !OJ",
								"[!RN] > Processing 1 change(s)",
								"[!RN] 02000000000000000000.01000000000000000000 - Added    - PK [f09047a6-35a0-4e25-afee-4c495ebf312a] = Code [A] - Desc [A Country]",
								"[!RN] > Processing completed",
								"[!RN] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: 01000000000000000000, Period: 2404)",
								"[!RX] > Processing 1 change(s)",
								"[!RX] 02000000000000000000.02000000000000000000 - Added    - PK [09a60371-9aa3-40ce-89f3-525502868fce] = Code [B] - Desc [B Currency]",
								"[!RX] > Processing completed",
								"[!RX] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: 02000000000000000000, Period: 2404)",
								"[!AR] > Processing 1 change(s)",
								"[!AR] 02000000000000000000.03000000000000000000 - Added    - PK [ca8cb304-3b15-4182-ae34-b6383dc5a8af] = Code [C] - Desc [C AccGroup]",
								"[!AR] > Processing completed",
								"[!AR] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: 03000000000000000000, Period: 2404)",
								"[!GE] > Processing 1 change(s)",
								"[!GE] 02000000000000000000.04000000000000000000 - Added    - PK [cf6f4ec6-a234-408b-ac94-c32b994d2f74] = Code [D] - Desc [D Dept]",
								"[!GE] > Processing completed",
								"[!GE] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: 04000000000000000000, Period: 2404)",
								"[!GG] > Processing 1 change(s)",
								"[!GG] 02000000000000000000.05000000000000000000 - Added    - PK [4382e58d-920e-476a-be6e-009d78e39cda] = Code [E] - Desc [E Group]",
								"[!GG] > Processing completed",
								"[!GG] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: 05000000000000000000, Period: 2404)",
								"[!SE] > Processing 1 change(s)",
								"[!SE] 02000000000000000000.06000000000000000000 - Added    - PK [d41f66ef-6c2e-4d36-aea7-22396aa354db] = Code [F] - Desc [F Event]",
								"[!SE] > Processing completed",
								"[!SE] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: 06000000000000000000, Period: 2404)",
								"[!OG] > Processing 1 change(s)",
								"[!OG] 02000000000000000000.07000000000000000000 - Added    - PK [35712f7c-eec8-4232-9c65-faba3fd48869] = Code [G] - Desc [G Creditor]",
								"[!OG] > Processing completed",
								"[!OG] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: 07000000000000000000, Period: 2404)",
								"[!OJ] > Processing 1 change(s)",
								"[!OJ] 02000000000000000000.08000000000000000000 - Added    - PK [3182f05e-2eb6-441d-a59f-3331704f63bb] = Code [H] - Desc [H Debtor]",
								"[!OJ] > Processing completed",
								"[!OJ] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: 08000000000000000000, Period: 2404)",
								"[!RN] > No relevant changes found",
								"[!RN] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!RX] > No relevant changes found",
								"[!RX] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!AR] > No relevant changes found",
								"[!AR] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!GE] > No relevant changes found",
								"[!GE] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!GG] > No relevant changes found",
								"[!GG] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!SE] > No relevant changes found",
								"[!SE] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!OG] > No relevant changes found",
								"[!OG] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
								"[!OJ] > No relevant changes found",
								"[!OJ] > Updating Subscriber High Water Mark (LSN: 02000000000000000000, SeqVal: FFFFFFFFFFFFFFFFFFFF, Period: 2404)",
						}
					);
				}
			}
		}

		void AddSubscriber(DbConnection auditConnection, string code, string description)
		{
			using (((ICurrentDbControl)auditConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
					"INSERT INTO [{0}].SubscriberControl (SubscriberCode, Description) VALUES(@Code, @Description)", BiConstants.BiAdminSchemaName);

				using (var cmd = auditConnection.Command(sqlText))
				{
					cmd.AddParameter("@Code", SqlDbType.Char, 3, code);
					cmd.AddParameter("@Description", SqlDbType.VarChar, 128, description);

					cmd.ExecuteNonQuery();
				}
			}
		}

		void AssertSubscriberHighWaterMark(DbConnection connection, string subscriberCode, string expected)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				SELECT convert(varchar(20), [LsnHighWaterMark], 2) + '.' + convert(varchar(20), [SeqValHighWaterMark], 2) + '.' + convert(varchar(20), [PeriodHighWaterMark], 2)
				FROM [{1}].SubscriberControl
				WHERE SubscriberCode = '{0}'",
				subscriberCode,
				BiConstants.BiAdminSchemaName);
			var actualObj = connection.ExecuteScalar(sqlText);
			string actualValue = (actualObj == null) ? null : actualObj.ToString();
			AssertEquals("High Water Mark for subscriber [" + subscriberCode + "]", expected, actualValue);
		}

		void CheckSubscriberExists(DbConnection auditConnection, string code, bool exists)
		{
			using (((ICurrentDbControl)auditConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
					"IF EXISTS (SELECT NULL FROM [{0}].SubscriberControl WHERE SubscriberCode = @Code) SELECT 1 ELSE SELECT 0", BiConstants.BiAdminSchemaName);

				using (var cmd = auditConnection.Command(sqlText))
				{
					cmd.AddParameter("@Code", SqlDbType.Char, 3, code);
					AssertEquals(string.Format(CultureInfo.InvariantCulture, "Subscriber [{0}]", code), exists, Convert.ToBoolean(cmd.ExecuteScalar()));
				}
			}
		}

		class SubscriberManagerForTest : SubscriberManager
		{
			public SubscriberManagerForTest(ILogger logger, params IAuditSubscriber[] testSubscribers)
			: base(logger, "NMT")
			{
				this.testSubscribers = testSubscribers;
			}

			public void RunSubscribersOnce(DbConnection auditConnection)
			{
				var subscribers = GetSubscribersToRun(testSubscribers, auditConnection);
				RunSubscribers(subscribers, CancellationToken.None);
			}

			protected override IEnumerable<IAuditSubscriber> LoadAllSubscribers()
			{
				return testSubscribers;
			}

			readonly IAuditSubscriber[] testSubscribers;

			public IEnumerable<IAuditSubscriber> LoadAllSubscribers_Exposed()
			{
				return LoadAllSubscribers();
			}

			public bool HasChangesToProcess_Exposed(IAuditSubscriber subscriber, DbConnection auditConnection)
			{
				return this.HasChangesToProcess(subscriber.GetWrapper(auditConnection, new SubscriberLogger(serviceLogger, subscriber.Code)));
			}

			public Dictionary<string, int> SubScriberCommandIdHighWaterMarkCache_Exposed => this.subscriberCommandIdHighWaterMarkCache;
			public Dictionary<string, Lsn> SubscriberLsnHighWaterMarkCache_Exposed => this.subscriberLsnHighWaterMarkCache;
			public Dictionary<string, Lsn> TableStateLsnHighWaterMarkCache_Exposed => this.tableStateLsnHighWaterMarkCache;

			public bool ShouldNotSendAuditChangeNotifications { get; set; }
			public bool HasSendAuditChangeNotificationsBeenCalled { get; set; }
			public IEnumerable<IAuditSubscriberWrapper> ValidSubscribers = new List<IAuditSubscriberWrapper>();
			protected override void SendAuditChangeNotifications(IEnumerable<IAuditSubscriberWrapper> validSubscribers, CancellationToken token)
			{
				ValidSubscribers = validSubscribers;
				if (!ShouldNotSendAuditChangeNotifications)
				{
					base.SendAuditChangeNotifications(validSubscribers, token);
				}
				HasSendAuditChangeNotificationsBeenCalled = true;
			}

			internal void Run()
			{
				var allSubscribers = LoadAllSubscribers();
				Run(CancellationToken.None, allSubscribers);
			}
		}

		class NotificationManagerForSubscriberLoadingTest : SubscriberManager
		{
			public NotificationManagerForSubscriberLoadingTest(ILogger logger)
				: base(logger, string.Empty)
			{
			}

			public IEnumerable<IAuditSubscriber> LoadAllSubscribers_Exposed()
			{
				return LoadAllSubscribers();
			}
		}

		class SubscriberManagerForInfrastructureDbErrorTest : SubscriberManager
		{
			public SubscriberManagerForInfrastructureDbErrorTest(ILogger logger)
				: base(logger, string.Empty)
			{
			}

			public IEnumerable<IAuditSubscriber> LoadAllSubscribers_Exposed()
			{
				yield return new DummySubscriber();
			}

			class DummySubscriber : IAuditSubscriber
			{
				public string Code
				{
					get
					{
						throw SqlExceptionBuilder.CreateSqlException(8645, "Error Num:8645");
					}
				}

				public string Description => throw new NotImplementedException();

				public IAuditSubscriberWrapper GetWrapper(DbConnection auditConnection, ILogger logger)
				{
					return null;
				}

				public bool IsRequired()
				{
					throw new NotImplementedException();
				}
			}
		}

		class ErrorTestWrapper : IAuditSubscriberWrapper
		{
			readonly string errorMessage;
			readonly string code;
			public ErrorTestWrapper(string code, string errorMessage)
			{
				this.errorMessage = errorMessage;
				this.code = code;
			}

			public string Code => code;

			public bool FetchDataAndProcessChanges()
			{
				throw SqlExceptionBuilder.CreateSqlException(8645, errorMessage);
			}

			public bool ExistsInSubscriberControlTable => throw new NotImplementedException();

			public ILogger Logger => throw new NotImplementedException();

			public DbConnection auditConnection => throw new NotImplementedException();

			public int BatchSize => throw new NotImplementedException();

			public byte[] NextLsnHighWaterMark { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public byte[] NextSeqValHighWaterMark { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public int NextPeriodHighWaterMark { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

			public string Description => throw new NotImplementedException();

			public bool IsValid => true;

			public void AddSubscriberToSubscriberControlTable()
			{
				throw new NotImplementedException();
			}

			public IAuditSubscriberWrapper GetWrapper(DbConnection auditConnection, ILogger logger)
			{
				throw new NotImplementedException();
			}

			public bool HasChangesToProcess()
			{
				throw new NotImplementedException();
			}

			public bool IsRequired()
			{
				throw new NotImplementedException();
			}

			public bool ShouldRunSubscriber()
			{
				throw new NotImplementedException();
			}

			public void UpdateLsnHighWaterMark()
			{
				throw new NotImplementedException();
			}

			public void ValidateSubscriber()
			{
				throw new NotImplementedException();
			}

			public bool HasChangesToProcessFromCache(Dictionary<string, Lsn> tableStateLsnHighWaterMarkCache, Dictionary<string, Lsn> subscriberLsnHighWaterMarkCache, Dictionary<string, int> subscriberCommandIdHighWaterMarkCache)
			{
				throw new NotImplementedException();
			}
		}
	}
}
