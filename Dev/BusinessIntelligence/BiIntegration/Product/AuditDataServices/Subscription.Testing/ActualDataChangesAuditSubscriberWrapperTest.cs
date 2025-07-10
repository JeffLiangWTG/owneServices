namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using CargoWise.Bi.Common;
	using CargoWise.Bi.Common.Testing;
	using CargoWise.Data;
	using CargoWise.Data.Testing;
	using CargoWise.Schema;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.Integration;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	class ActualDataChangesAuditSubscriberWrapperTest : TestCase
	{
		[UseSnapshotProtection(Db.AuditDatabaseSuffix)]
		public void TestCommandIdDoesntAddUnnecessaryRecords()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RX_Code") };
				var testSubscriber = new GenericTestDataChangeSubscriber("!RX", "Test Actual Data Changes Subscriber", RefCurrencySchema.Instance, columns: schemaCols);

				SendLatestAuditChangesAndAssert(auditConnection,
						testSubscriber,
						temporaryMaxLsn: "0x00",
						temporaryMaxPeriod: "2000-01-10 00:00:00.000",
						// add to SubscriberControl table
						expectedLogs: Array.Empty<string>()
					);

				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc) VALUES
							(1001, 1, 0x0A, 0x01, 3, 0x0, 'EC8567A3-0AF4-40B8-9463-53A0239B4208', '~1', 'Currency1'),
							(1001, 1, 0x0A, 0x01, 4, 0x0, 'EC8567A3-0AF4-40B8-9463-53A0239B4208', '~1', 'Currency2'),
							(1001, 2, 0x0A, 0x02, 3, 0x0, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', 'Currency3'),
							(1001, 2, 0x0A, 0x02, 4, 0x0, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', 'Currency4');
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0A, '2010-01-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0A, 'dbo', 'RefCurrency', 4, 1001);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection,
						testSubscriber,
						temporaryMaxLsn: "0x0A",
						temporaryMaxPeriod: "2010-01-10 00:00:00.000",
						// does not process update to RX_Desc
						expectedLogs: Array.Empty<string>()
					);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "0A000000000000000000.02000000000000000000.1001");
			}
		}

		[UseSnapshotProtection(Db.AuditDatabaseSuffix)]
		public void TestDoesNotTimeoutDueToTabLock()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				auditConnection.ExecuteNonQuery(@"INSERT biadmin.LsnTimeMapping  (StartLsn, TranEndTimeUtc) VALUES (0x0A, '2016-06-01')
					INSERT biadmin.LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0B, '2016-06-01')");
			}

			using (var testConnection2 = AuditTestHelper.GetAuditConnection())
			using (testConnection2.TemporarySetDefaultCommandTimeOut(10))
			using (var testConnection1 = AuditTestHelper.GetAuditConnection())
			using (var manager = testConnection1.BeginTransactionWithManager())
			{
				var schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };
				var testSubscriber = new GenericTestDataChangeSubscriber("!RX", "Test Actual Data Changes Subscriber", RefCurrencySchema.Instance, columns: schemaCols);

				var sqlText = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO [{0}].CdcHistorySummary with(tablock) (Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0A, 'dbo', 'RefCurrency', 20, 1606);", BiConstants.BiAdminSchemaName);
				testConnection1.ExecuteNonQuery(sqlText);

				sqlText = string.Format(CultureInfo.InvariantCulture, @"INSERT [{0}].LsnTimeMapping with(tablock) (StartLsn, TranEndTimeUtc) VALUES (0x0C, '2016-06-01')", BiConstants.BiAdminSchemaName);
				testConnection1.ExecuteNonQuery(sqlText);

				sqlText = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO [{0}].CdcHistorySummary with(tablock) (Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0B, 'dbo', 'RefCurrency',6, 1606);", BiConstants.BiAdminSchemaName);
				testConnection1.ExecuteNonQuery(sqlText);

				AssertSubscriberHighWaterMark(testConnection2, ((IAuditSubscriber)testSubscriber).Code, expected: null);

				SendLatestAuditChangesAndAssert(testConnection2,
					testSubscriber,
					temporaryMaxLsn: "0x0A",
					temporaryMaxPeriod: null,
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(testConnection2, ((IAuditSubscriber)testSubscriber).Code, expected: "0A000000000000000000.FFFFFFFFFFFFFFFFFFFF.1606"); // Lsn High Water mark is now 0x0B because we use the CdcHistorySummary table

				string irrelevantChangesSQLText = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @rows INT = 2
						DECLARE @iterations INT = 0
						DECLARE @Seq binary(10) = 0x00
						WHILE @iterations < @rows
						BEGIN
							DECLARE @cid INT = @iterations*10
							INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
							(1606, @cid +  1, 0x0A, @Seq+1, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +  2, 0x0A, @Seq+1, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid +  3, 0x0A, @Seq+2, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid +  4, 0x0A, @Seq+2, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +  5, 0x0A, @Seq+3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +  6, 0x0A, @Seq+3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid +  7, 0x0A, @Seq+4, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid +  8, 0x0A, @Seq+4, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +  9, 0x0A, @Seq+5, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid + 10, 0x0A, @Seq+5, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1)
							SET @Seq = @Seq + 5
							SET @iterations = @iterations + 1
						END",
					BiConstants.BiAdminSchemaName);
				testConnection2.ExecuteNonQuery(irrelevantChangesSQLText);

				string batch1Add = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @Seq binary(10) = 0xA0
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
						(1606,91, 0x0B, @Seq+2, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1', 0), -- defferred update (same seqval)
						(1606,92, 0x0B, @Seq+2, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 0),
						(1606,93, 0x0B, @Seq+3, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 0), -- defferred update (diff seqval, non sequential cmdId)
						(1606,95, 0x0B, @Seq+4, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 0),
						(1606,96, 0x0B, @Seq+5, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 0), -- defferred update (diff seqval)
						(1606,97, 0x0B, @Seq+6, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 0)",
					BiConstants.BiAdminSchemaName);
				testConnection2.ExecuteNonQuery(batch1Add);

				//no records to process because uncommitted lsntimemapping and cdchistorysummary from testConnection1
				SendLatestAuditChangesAndAssert(testConnection2,
					testSubscriber,
					temporaryMaxLsn: "0x0A",
					temporaryMaxPeriod: null,
					expectedLogs: Array.Empty<string>(),
					batchSize: 10
				);

				manager.CommitTransaction();

				//once lsntimemapping and cdchistorysummary changes are committed we should now have records processed.
				SendLatestAuditChangesAndAssert(testConnection2, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: null,
					expectedLogs: new string[] {
							"0b000000000000000000.00000000000000000002 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [~TEST~CURRENCY~1]=>[#TEST#CURRENCY#1]",
							"0b000000000000000000.00000000000000000003 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [#TEST#CURRENCY#1]=>[*TEST*CURRENCY*1]",
							"0b000000000000000000.00000000000000000005 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [*TEST*CURRENCY*1]=>[%TEST%CURRENCY%1]",
				});
				//1 since we processed all changes already - no changes found.
				SendLatestAuditChangesAndAssertCount(testConnection2, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: null,
					expectedLogsCount: 0
				);

				SendLatestAuditChangesAndAssert(testConnection2,
					testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: null,
					expectedLogs: Array.Empty<string>()
				);
			}
		}

		[UseSnapshotProtection(Db.AuditDatabaseSuffix)]
		public void TestTreatDeleteInsertAsUpdate()
		{
			SchemaStringColumnForTest[] schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };
			var testSubscriber = new GenericTestDataChangeSubscriber("!RX", "Dummy Test Changed Table List Subscriber", RefCurrencySchema.Instance, columns: schemaCols);

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: null);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x09",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "09000000000000000000.FFFFFFFFFFFFFFFFFFFF.1606");

				string irrelevantChangesSQLText = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @rows INT = 2
						DECLARE @iterations INT = 0
						DECLARE @Seq binary(10) = 0x00
						WHILE @iterations < @rows
						BEGIN
							INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
							(1606, 1, 0x09, @Seq+1, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, 2, 0x09, @Seq+1, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, 3, 0x09, @Seq+2, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, 4, 0x09, @Seq+2, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, 5, 0x09, @Seq+3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, 6, 0x09, @Seq+3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, 7, 0x09, @Seq+4, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, 8, 0x09, @Seq+4, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, 9, 0x09, @Seq+5, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606,10, 0x09, @Seq+5, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1)
							SET @Seq = @Seq + 5
							SET @iterations = @iterations + 1
						END
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0A, '2016-06-01')
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0A, 'dbo', 'RefCurrency', 20, 1606);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(irrelevantChangesSQLText);

				//first time no records
				SendLatestAuditChangesAndAssertCount(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0A",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogsCount: 0,
					batchSize: 10
				);

				string batch1Add = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @Seq binary(10) = 0xA0
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
						(1606, 1, 0x0B, @Seq+2, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1', 0),	-- Deffered update, same SeqVal
						(1606, 2, 0x0B, @Seq+2, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 0),
						(1606, 3, 0x0B, @Seq+3, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 0),	-- Deffered update, different SeqVal
						(1606, 4, 0x0B, @Seq+4, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 0),
						(1606, 5, 0x0B, @Seq+5, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 0)	-- delete
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0B, '2016-06-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0B, 'dbo', 'RefCurrency',6, 1606);
					", BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(batch1Add);

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: new string[] {
							"0b000000000000000000.00000000000000000002 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [~TEST~CURRENCY~1]=>[#TEST#CURRENCY#1]",
							"0b000000000000000000.00000000000000000003 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [#TEST#CURRENCY#1]=>[*TEST*CURRENCY*1]",
							"0b000000000000000000.00000000000000000005 - Deleted  - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1] - Desc [*TEST*CURRENCY*1]",
					},
					batchSize: 10
					);
				//1 since we processed all changes already - no changes found.
				SendLatestAuditChangesAndAssertCount(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogsCount: 0
				);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
			}
		}

		[UseSnapshotProtection(Db.AuditDatabaseSuffix)]
		public void TestDeleteOnly()
		{
			SchemaStringColumnForTest[] schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };
			var testSubscriber = new GenericTestDataChangeSubscriber("!RX", "Dummy Test Changed Table List Subscriber", RefCurrencySchema.Instance, columns: schemaCols, notifyInsert: false, notifyUpdate: false, notifyDelete: true);

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: null);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0A",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "0A000000000000000000.FFFFFFFFFFFFFFFFFFFF.1606");

				string irrelevantChangesSQLText = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @rows INT = 2
						DECLARE @iterations INT = 0
						DECLARE @Seq binary(10) = 0x00
						WHILE @iterations < @rows
						BEGIN
							DECLARE @cid INT = @iterations*10
							INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
							(1606, @cid +  1, 0x0A, @Seq+1, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +  2, 0x0A, @Seq+1, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid +  3, 0x0A, @Seq+2, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid +  4, 0x0A, @Seq+2, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +  5, 0x0A, @Seq+3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +  6, 0x0A, @Seq+3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid +  7, 0x0A, @Seq+4, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid +  8, 0x0A, @Seq+4, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +  9, 0x0A, @Seq+5, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid + 10, 0x0A, @Seq+5, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1)
							SET @Seq = @Seq + 5
							SET @iterations = @iterations + 1
						END
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0A, 'dbo', 'RefCurrency', 20, 1606);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(irrelevantChangesSQLText);

				//first time no records
				SendLatestAuditChangesAndAssertCount(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0A",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogsCount: 0,
					batchSize: 10
				);

				string batch1Add = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @Seq binary(10) = 0xA0
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
						(1606, 91, 0x0B, @Seq+2, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1', 0), -- defferred update (same seqval)
						(1606, 92, 0x0B, @Seq+2, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 0),
						(1606, 93, 0x0B, @Seq+3, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 0), -- delete
						(1606, 96, 0x0B, @Seq+5, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 0), -- defferred update (diff seqval)
						(1606, 97, 0x0B, @Seq+6, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 0)
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0B, 'dbo', 'RefCurrency',6, 1606);
					", BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(batch1Add);

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: new string[] {
							"0b000000000000000000.00000000000000000003 - Deleted  - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1] - Desc [#TEST#CURRENCY#1]",
					},
					batchSize: 10
					);

				//1 since we processed all changes already - no changes found.
				SendLatestAuditChangesAndAssertCount(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogsCount: 0
				);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
			}
		}

		[UseSnapshotProtection(Db.AuditDatabaseSuffix)]
		public void TestAddOnly()
		{
			SchemaStringColumnForTest[] schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };
			var testSubscriber = new GenericTestDataChangeSubscriber("!RX", "Dummy Test Changed Table List Subscriber", RefCurrencySchema.Instance, columns: schemaCols, notifyInsert: true, notifyUpdate: false, notifyDelete: false);

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: null);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0A",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "0A000000000000000000.FFFFFFFFFFFFFFFFFFFF.1606");

				string irrelevantChangesSQLText = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @rows INT = 2
						DECLARE @iterations INT = 0
						DECLARE @Seq binary(10) = 0x00
						WHILE @iterations < 0 --@rows
						BEGIN
							DECLARE @cid INT = @iterations*10
							INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
							(1606, @cid +  1, 0x0A, @Seq+1, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +  2, 0x0A, @Seq+1, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid +  3, 0x0A, @Seq+2, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid +  4, 0x0A, @Seq+2, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +  5, 0x0A, @Seq+3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +  6, 0x0A, @Seq+3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid +  7, 0x0A, @Seq+4, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid +  8, 0x0A, @Seq+4, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +  9, 0x0A, @Seq+5, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid + 10, 0x0A, @Seq+5, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1)
							SET @Seq = @Seq + 5
							SET @iterations = @iterations + 1
						END
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
							(0x0A, 'dbo', 'RefCurrency', 20, 1606);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(irrelevantChangesSQLText);

				//first time no records
				SendLatestAuditChangesAndAssertCount(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0A",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogsCount: 0,
					batchSize: 10
				);

				string batch1Add = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @Seq binary(10) = 0xA0
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
						(1606, 91, 0x0B, @Seq+2, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1', 0), -- defferred update (same seqval)
						(1606, 92, 0x0B, @Seq+2, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 0),
						(1606, 95, 0x0B, @Seq+4, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 0), -- add
						(1606, 96, 0x0B, @Seq+5, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 0), -- defferred update (diff seqval)
						(1606, 97, 0x0B, @Seq+6, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 0)
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0B, '2016-06-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0B, 'dbo', 'RefCurrency',6, 1606);
					", BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(batch1Add);

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: new string[] {
							"0b000000000000000000.00000000000000000004 - Added    - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1] - Desc [*TEST*CURRENCY*1]",
					},
					batchSize: 10
					);
				//1 since we processed all changes already - no changes found.
				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0C",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestUpdateOnly()
		{
			SchemaStringColumnForTest[] schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };
			var testSubscriber = new GenericTestDataChangeSubscriber("!RX", "Dummy Test Changed Table List Subscriber", RefCurrencySchema.Instance, columns: schemaCols, notifyInsert: false, notifyDelete: false, notifyUpdate: true);

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: null);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x09",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "09000000000000000000.FFFFFFFFFFFFFFFFFFFF.1606");

				string irrelevantChangesSQLText = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @rows INT = 2
						DECLARE @iterations INT = 0
						DECLARE @Seq binary(10) = 0x00
						WHILE @iterations < @rows
						BEGIN
							DECLARE @cid INT = @iterations*10
							INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
							(1606, @cid + 1, 0x0A, @Seq+1, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid + 2, 0x0A, @Seq+1, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid + 3, 0x0A, @Seq+2, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid + 4, 0x0A, @Seq+2, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid + 5, 0x0A, @Seq+3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid + 6, 0x0A, @Seq+3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid + 7, 0x0A, @Seq+4, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid + 8, 0x0A, @Seq+4, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid + 9, 0x0A, @Seq+5, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +10, 0x0A, @Seq+5, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1)
							SET @Seq = @Seq + 5
							SET @iterations = @iterations + 1
						END
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0A, '2016-06-01')
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0A, 'dbo', 'RefCurrency', 20, 1606);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(irrelevantChangesSQLText);

				//first time no records
				SendLatestAuditChangesAndAssertCount(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0A",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogsCount: 0,
					batchSize: 10
				);

				string batch1Add = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @Seq binary(10) = 0xA0
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
						(1606, 91, 0x0B, @Seq+2, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1', 0),
						(1606, 92, 0x0B, @Seq+2, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 0),
						(1606, 93, 0x0B, @Seq+3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 0),
						(1606, 94, 0x0B, @Seq+3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 0),
						(1606, 95, 0x0B, @Seq+5, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 0),
						(1606, 96, 0x0B, @Seq+5, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 0),
						(1606, 97, 0x0B, @Seq+6, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 0),
						(1606, 98, 0x0B, @Seq+7, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1', 0)
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0B, '2016-06-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0B, 'dbo', 'RefCurrency', 8, 1606);
					", BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(batch1Add);

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: new string[] {
							"0b000000000000000000.00000000000000000002 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [~TEST~CURRENCY~1]=>[#TEST#CURRENCY#1]",
							"0b000000000000000000.00000000000000000003 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [#TEST#CURRENCY#1]=>[*TEST*CURRENCY*1]",
							"0b000000000000000000.00000000000000000005 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [%TEST%CURRENCY%1]=>[*TEST*CURRENCY*1]"
					},
					batchSize: 10
				);
				//1 since we processed all changes already - no changes found.
				SendLatestAuditChangesAndAssertCount(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogsCount: 0,
					batchSize: 10
				);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: Array.Empty<string>(),
					batchSize: 10

				);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestOperationsWithSeparatedSameSequenceValues()
		{
			SchemaStringColumnForTest[] schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };
			var testSubscriber = new GenericTestDataChangeSubscriber("!RX", "Dummy Test Changed Table List Subscriber", RefCurrencySchema.Instance, columns: schemaCols);

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: null);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x000000000000000000",
					temporaryMaxPeriod: "2000-01-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "00000000000000000000.FFFFFFFFFFFFFFFFFFFF.1");

				string irrelevantChangesSQLText = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @rows INT = 1
						DECLARE @iterations INT = 0
						DECLARE @Seq binary(10) = 0x00
						WHILE @iterations < @rows
						BEGIN
							INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
							(1606, 1, 0x0A, @Seq+1, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, 2, 0x0A, @Seq+1, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, 3, 0x0A, @Seq+2, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, 4, 0x0A, @Seq+2, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, 5, 0x0A, @Seq+3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, 6, 0x0A, @Seq+3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, 7, 0x0A, @Seq+4, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, 8, 0x0A, @Seq+4, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, 9, 0x0A, @Seq+5, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, 10, 0x0A, @Seq+5, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1)
							SET @Seq = @Seq + 5
							SET @iterations = @iterations + 1
						END
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0A, '2016-06-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0A, 'dbo', 'RefCurrency', 20, 1606);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(irrelevantChangesSQLText);

				//first time no records
				SendLatestAuditChangesAndAssertCount(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x000000000000000000",
					temporaryMaxPeriod: "2000-01-10 00:00:00.000",
					expectedLogsCount: 0, // ShouldRunChanges = False
					batchSize: 10
				);

				string batch1Add = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @Seq binary(10) = 0xA0
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
						(1606, 1, 0x0B, @Seq+2, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1', 0),
						(1606, 3, 0x0B, @Seq+3, 1, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 0),
						(1606, 5, 0x0B, @Seq+4, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1', 0),
						(1606, 2, 0x0B, @Seq+2, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1', 0)
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0B, '2016-06-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0B, 'dbo', 'RefCurrency', 4, 1606);
					", BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(batch1Add);

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: new string[] {
							"0b000000000000000000.00000000000000000002 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [~TEST~CURRENCY~1]=>[#TEST#CURRENCY#1]",
							"0b000000000000000000.00000000000000000003 - Modified - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1]=>[~1] - Desc [#TEST#CURRENCY#1]=>[*TEST*CURRENCY*1]",
					});
				//1 since we processed all changes already - no changes found.
				SendLatestAuditChangesAndAssertCount(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogsCount: 0
				);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0C0000000000000000",
					temporaryMaxPeriod: "2016-06-10 00:00:00.000",
					expectedLogs: Array.Empty<string>());
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendLatestChangesWithConcurrentInserts()
		{
			SchemaStringColumnForTest[] schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RX_Code"), new SchemaStringColumnForTest("RX_Desc") };
			var testSubscriber = new GenericTestDataChangeSubscriber("!RX", "Dummy Test Changed Table List Subscriber", RefCurrencySchema.Instance, columns: schemaCols);

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: null);

				// Run it once to set high watermark
				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x000000000000000000",
					temporaryMaxPeriod: "2000-01-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "00000000000000000000.FFFFFFFFFFFFFFFFFFFF.1");

				string irrelevantChangesSQLText = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @rows INT = 2
						DECLARE @iterations INT = 0
						DECLARE @Seq binary(10) = 0x00
						WHILE @iterations < @rows
						BEGIN
							DECLARE @cid INT = @iterations*10
							INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc, RX_IsSystem) VALUES
							(1606, @cid + 1, 0x0A, @Seq+1, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid + 2, 0x0A, @Seq+1, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid + 3, 0x0A, @Seq+2, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid + 4, 0x0A, @Seq+2, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid + 5, 0x0A, @Seq+3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid + 6, 0x0A, @Seq+3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid + 7, 0x0A, @Seq+4, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1),
							(1606, @cid + 8, 0x0A, @Seq+4, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid + 9, 0x0A, @Seq+5, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',0),
							(1606, @cid +10, 0x0A, @Seq+5, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1',1)
							SET @Seq = @Seq + 5
							SET @iterations = @iterations + 1
						END
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0A, '2016-06-01')
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0A, 'dbo', 'RefCurrency', 20, 1606);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(irrelevantChangesSQLText);

				//batch size is 10 in test mode. above inserts 20 rows into Audit DB RefCurrency table = 10 pairs of changes (CDC operation type 3 and 4)
				//first time no records
				SendLatestAuditChangesAndAssertCount(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0A",
					temporaryMaxPeriod: "2016-06-01 00:00:00.000",
					expectedLogsCount: 0, // No relevant changes
					batchSize: 10
				);

				var relevantChangesSQLText = string.Format(CultureInfo.InvariantCulture, @"
						DECLARE @rows INT = 2
						DECLARE @iterations INT = 0
						DECLARE @Seq binary(10) = 0xA0
						WHILE @iterations < @rows
						BEGIN
							DECLARE @cid INT = 100 + @iterations*10
							INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc) VALUES
							(1606, @cid + 1, 0x0B, @Seq+1, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1'),
							(1606, @cid + 2, 0x0B, @Seq+1, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1'),
							(1606, @cid + 3, 0x0B, @Seq+2, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#CURRENCY#1'),
							(1606, @cid + 4, 0x0B, @Seq+2, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1'),
							(1606, @cid + 5, 0x0B, @Seq+3, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '*TEST*CURRENCY*1'),
							(1606, @cid + 6, 0x0B, @Seq+3, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1'),
							(1606, @cid + 7, 0x0B, @Seq+4, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '%TEST%CURRENCY%1'),
							(1606, @cid + 8, 0x0B, @Seq+4, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1'),
							(1606, @cid + 9, 0x0B, @Seq+5, 3, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY~1'),
							(1606, @cid +10, 0x0B, @Seq+5, 4, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~CURRENCY!1')
							SET @Seq = @Seq + 5
							SET @iterations = @iterations + 1
						END
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0B, '2016-06-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0B, 'dbo', 'RefCurrency', 20, 1606);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(relevantChangesSQLText);

				//first log record is "processing 10 change(s)", last log record is "processing completed" which is why we expect 12
				SendLatestAuditChangesAndAssertCount(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-01 00:00:00.000",
					expectedLogsCount: 12,
					batchSize: 10
				);
				//1 since we processed all changes already - no changes found.
				SendLatestAuditChangesAndAssertCount(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-01 00:00:00.000",
					expectedLogsCount: 0,
					batchSize: 10
				);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2016-06-01 00:00:00.000",
					expectedLogs: Array.Empty<string>(),
					batchSize: 10
				);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendLatestChangesWithFilter()
		{
			SchemaStringColumnForTest[] schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("OG_Code"), new SchemaStringColumnForTest("OG_Desc") };
			var testSubscriber = new GenericTestDataChangeSubscriber(
				"!OG",
				"Dummy Test Changed Table List Subscriber",
				OrgCreditorGroupSchema.Instance,
				columns: schemaCols,
				notifyInsert: true, notifyUpdate: true, notifyDelete: false,
				customFilter: (DataRow row) =>
				{
					if (!Convert.ToBoolean(row["OG_IsValid"]))
					{
						row.Delete();
					}
				});

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x00",
					temporaryMaxPeriod: "2000-01-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "00000000000000000000.FFFFFFFFFFFFFFFFFFFF.1");

				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code, OG_Desc, OG_IsValid) VALUES
							(1602, 1, 0x0A, 0x01, 2, 0x01, '0B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1', '~TEST~CREDITORGROUP~1', 0),
							(1602, 1, 0x0A, 0x05, 3, 0x01, '0B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1', '~TEST~CREDITORGROUP~1', 1),
							(1602, 1, 0x0A, 0x05, 4, 0x01, '0B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1', '#TEST#CREDITORGROUP#1', 1),
							(1602, 1, 0x0A, 0x09, 2, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2', '~TEST~CREDITORGROUP~2', 1),
							(1602, 1, 0x0B, 0x01, 2, 0x01, '8C5916AA-3739-4496-8443-B13414BCD630', '~C3', '~TEST~CREDITORGROUP~3', 0),
							(1602, 1, 0x0B, 0x02, 1, 0x01, '0B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1', '#TEST#CREDITORGROUP#1', 1),
							(1602, 1, 0x0C, 0x01, 3, 0x01, '8C5916AA-3739-4496-8443-B13414BCD630', '~C3', '~TEST~CREDITORGROUP~3', 0),
							(1602, 1, 0x0C, 0x01, 4, 0x01, '8C5916AA-3739-4496-8443-B13414BCD630', '@C3', '@TEST@CREDITORGROUP@3', 0),
							(1602, 1, 0x0C, 0x04, 2, 0x01, '7BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', '~C4', '~TEST~CREDITORGROUP~4', 1),
							(1602, 1, 0x0D, 0x01, 3, 0x01, '7BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', '~C4', '~TEST~CREDITORGROUP~4', 0),
							(1602, 1, 0x0D, 0x01, 4, 0x01, '7BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', '~C4', '@TEST@CREDITORGROUP@4', 0);
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x0A, '2016-02-01'),
							(0x0B, '2016-02-01'),
							(0x0C, '2016-02-01'),
							(0x0D, '2016-02-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0A, 'dbo', 'OrgCreditorGroup', 4, 1602);
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0B, 'dbo', 'OrgCreditorGroup', 2, 1602);
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0C, 'dbo', 'OrgCreditorGroup', 3, 1602);
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0D, 'dbo', 'OrgCreditorGroup', 2, 1602);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0D",
					temporaryMaxPeriod: "2016-02-10 00:00:00.000",
					expectedLogs: new string[] {
							//"Processing 3 change(s)",
							"0A000000000000000000.05000000000000000000 - Modified - PK [0b5a48d3-ba8a-4916-8921-f7063d033b2a] = Code [~C1]=>[~C1] - Desc [~TEST~CREDITORGROUP~1]=>[#TEST#CREDITORGROUP#1]",
							"0A000000000000000000.09000000000000000000 - Added    - PK [2b64cb57-8a41-4808-bf4b-d736cb71c3b2] = Code [~C2] - Desc [~TEST~CREDITORGROUP~2]",
							"0C000000000000000000.04000000000000000000 - Added    - PK [7ba7e6ec-b0d2-44cd-ab96-ff1df1fc93f7] = Code [~C4] - Desc [~TEST~CREDITORGROUP~4]"
					}
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "0C000000000000000000.04000000000000000000.1602");

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code, OG_Desc, OG_IsValid) VALUES
							(1602, 2, 0x10, 0x01, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2', '~TEST~CREDITORGROUP~2', 1),
							(1602, 2, 0x10, 0x03, 2, 0x01, 'CEA590BA-97E3-4A5B-A35D-E6D4AF3AB9D2', '~C6', '~TEST~CREDITORGROUP~6', 1);
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2016-02-28');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x10, 'dbo', 'OrgCreditorGroup', 2, 1602);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x10",
					temporaryMaxPeriod: "2016-03-01 00:00:00.000",
					expectedLogs: new string[] {
							//"Processing 1 change(s)",
							"10000000000000000000.03000000000000000000 - Added    - PK [CEA590BA-97E3-4A5B-A35D-E6D4AF3AB9D2] = Code [~C6] - Desc [~TEST~CREDITORGROUP~6]"
					}
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "10000000000000000000.03000000000000000000.1602");

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code, OG_Desc, OG_IsValid) VALUES
							(1603, 3, 0x11, 0x01, 1, 0x01, 'CEA590BA-97E3-4A5B-A35D-E6D4AF3AB9D2', '~C6', '~TEST~CREDITORGROUP~6', 1);
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x11, '2016-03-15');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x11, 'dbo', 'OrgCreditorGroup', 1, 1603);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x11",
					temporaryMaxPeriod: "2016-03-01 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "11000000000000000000.01000000000000000000.1603");
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendLatestChangesForNullSpecificColumns()
		{
			var testSubscriber = new GenericTestDataChangeSubscriber(
				"!OG",
				"Dummy Test Changed Table List Subscriber",
				OrgCreditorGroupSchema.Instance,
				null,
				notifyInsert: true, notifyUpdate: true, notifyDelete: true);

			AssertSendLatestOgDescChanges(testSubscriber);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendLatestChangesForSpecificColumns()
		{
			var testSubscriber = new GenericTestDataChangeSubscriber(
				"!OG",
				"Dummy Test Changed Table List Subscriber",
				OrgCreditorGroupSchema.Instance,
				new SchemaColumn[] { OrgCreditorGroupSchema.OG_Desc },
				notifyInsert: true, notifyUpdate: true, notifyDelete: true);

			AssertSendLatestOgDescChanges(testSubscriber);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendLatestChangesForSpecificColumns_UpdateOnly()
		{
			var testSubscriber = new GenericTestDataChangeSubscriber(
				"!OG",
				"Dummy Test Changed Table List Subscriber",
				OrgCreditorGroupSchema.Instance,
				new SchemaColumn[] { OrgCreditorGroupSchema.OG_Code, OrgCreditorGroupSchema.OG_Desc },
				notifyInsert: false, notifyUpdate: true, notifyDelete: false);

			AssertSendLatestOgDescChanges(testSubscriber);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendLatestChangesForSpecificColumns_WithNoChangeOnSubscribedColumn()
		{
			SchemaStringColumnForTest[] schemaCols = new SchemaStringColumnForTest[] {
				new SchemaStringColumnForTest("OG_PK", SqlDbType.UniqueIdentifier)
			};
			var testSubscriber = new GenericTestDataChangeSubscriber(
				"!OG",
				"Dummy Test Changed Table List Subscriber",
				OrgCreditorGroupSchema.Instance,
				schemaCols,
				notifyInsert: false, notifyUpdate: true, notifyDelete: false);

			AssertSendLatestOgDescChanges(testSubscriber);
		}

		void AssertSendLatestOgDescChanges(ActualDataChangesAuditSubscriber testSubscriber)
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: null);

				var expectedLogs = new List<string>();

				// Run it once to set high watermark
				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x000000000000000000",
					temporaryMaxPeriod: "2001-01-10 00:00:00.000",
					expectedLogs.ToArray());
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "00000000000000000000.FFFFFFFFFFFFFFFFFFFF.101");

				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code, OG_Desc, OG_IsValid) VALUES
							(1505, 1, 0x0A, 0x01, 2, 0x0, '0B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1', '~TEST~CREDITORGROUP~1', 0),

							(1505, 2, 0x0A, 0x05, 3, 0x0, '0B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1', '~TEST~CREDITORGROUP~1', 0),
							(1505, 3, 0x0A, 0x05, 4, 0x0, '0B5A48D3-BA8A-4916-8921-F7063D033B2A', '#C1', '#TEST#CREDITORGROUP#1', 0),

							(1505, 4, 0x0A, 0x09, 2, 0x0, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2', '~TEST~CREDITORGROUP~2', 0),

							(1505, 5, 0x0B, 0x01, 3, 0x0, '0B5A48D3-BA8A-4916-8921-F7063D033B2A', '#C1', '#TEST#CREDITORGROUP#1', 0),
							(1505, 6, 0x0B, 0x01, 4, 0x0, '0B5A48D3-BA8A-4916-8921-F7063D033B2A', '#C1', '#TEST#CREDITORGROUP#1', 1),

							(1505, 7, 0x0B, 0x02, 3, 0x0, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2', '~TEST~CREDITORGROUP~2', 0),
							(1505, 8, 0x0B, 0x02, 4, 0x0, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2', '#TEST#CREDITORGROUP#2', 1),

							(1505, 9, 0x0C, 0x01, 3, 0x0, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2', '#TEST#CREDITORGROUP#2', 1),
							(1505,10, 0x0C, 0x01, 4, 0x0, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '#C2', '#TEST#CREDITORGROUP#2', 1),
							(1505,11, 0x0C, 0x03, 3, 0x0, '6F76E088-FEBF-488D-99BF-80CEC9983F76', '~C2', NULL, 1),
							(1505,12, 0x0C, 0x03, 4, 0x0, '6F76E088-FEBF-488D-99BF-80CEC9983F76', '~C2', '#TEST#CREDITORGROUP#3', 1),
							(1505,13, 0x0C, 0x05, 3, 0x0, '05D68C63-39E7-4B6E-847C-95BCB6F1F80B', '~C2', '#TEST#CREDITORGROUP#4', 1),
							(1505,14, 0x0C, 0x05, 4, 0x0, '05D68C63-39E7-4B6E-847C-95BCB6F1F80B', '~C2', NULL, 1);
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x0A, '2015-05-01'),
							(0x0B, '2015-05-01'),
							(0x0C, '2015-05-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0A, 'dbo', 'OrgCreditorGroup', 4, 1505);
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0B, 'dbo', 'OrgCreditorGroup', 4, 1505);
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0C, 'dbo', 'OrgCreditorGroup', 6, 1505);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				expectedLogs.Clear();

				if (testSubscriber.NotifyInsert)
				{
					expectedLogs.Add("0A000000000000000000.01000000000000000000 - Added    - PK [0B5A48D3-BA8A-4916-8921-F7063D033B2A] = Code [~C1] - Desc [~TEST~CREDITORGROUP~1]");
					expectedLogs.Add("0A000000000000000000.09000000000000000000 - Added    - PK [2B64CB57-8A41-4808-BF4B-D736CB71C3B2] = Code [~C2] - Desc [~TEST~CREDITORGROUP~2]");
				}

				if (testSubscriber.NotifyUpdate)
				{
					if (testSubscriber.SpecificColumns == null)
					{
						expectedLogs.Add("0A000000000000000000.05000000000000000000 - Modified - PK [0B5A48D3-BA8A-4916-8921-F7063D033B2A] = Code [~C1]=>[#C1] - Desc [~TEST~CREDITORGROUP~1]=>[#TEST#CREDITORGROUP#1]");
						expectedLogs.Add("0B000000000000000000.01000000000000000000 - Modified - PK [0B5A48D3-BA8A-4916-8921-F7063D033B2A] = Code [#C1]=>[#C1] - Desc [#TEST#CREDITORGROUP#1]=>[#TEST#CREDITORGROUP#1]");
						expectedLogs.Add("0B000000000000000000.02000000000000000000 - Modified - PK [2B64CB57-8A41-4808-BF4B-D736CB71C3B2] = Code [~C2]=>[~C2] - Desc [~TEST~CREDITORGROUP~2]=>[#TEST#CREDITORGROUP#2]");
						expectedLogs.Add("0C000000000000000000.01000000000000000000 - Modified - PK [2B64CB57-8A41-4808-BF4B-D736CB71C3B2] = Code [~C2]=>[#C2] - Desc [#TEST#CREDITORGROUP#2]=>[#TEST#CREDITORGROUP#2]");
						expectedLogs.Add("0C000000000000000000.03000000000000000000 - Modified - PK [6F76E088-FEBF-488D-99BF-80CEC9983F76] = Code [~C2]=>[~C2] - Desc []=>[#TEST#CREDITORGROUP#3]");
						expectedLogs.Add("0C000000000000000000.05000000000000000000 - Modified - PK [05D68C63-39E7-4B6E-847C-95BCB6F1F80B] = Code [~C2]=>[~C2] - Desc [#TEST#CREDITORGROUP#4]=>[]");
					}
					else if (testSubscriber.SpecificColumns.Contains(OrgCreditorGroupSchema.OG_Code) && testSubscriber.SpecificColumns.Contains(OrgCreditorGroupSchema.OG_Desc))
					{
						expectedLogs.Add("0A000000000000000000.05000000000000000000 - Modified - PK [0B5A48D3-BA8A-4916-8921-F7063D033B2A] = Code [~C1]=>[#C1] - Desc [~TEST~CREDITORGROUP~1]=>[#TEST#CREDITORGROUP#1]");
						expectedLogs.Add("0B000000000000000000.02000000000000000000 - Modified - PK [2B64CB57-8A41-4808-BF4B-D736CB71C3B2] = Code [~C2]=>[~C2] - Desc [~TEST~CREDITORGROUP~2]=>[#TEST#CREDITORGROUP#2]");
						expectedLogs.Add("0C000000000000000000.01000000000000000000 - Modified - PK [2B64CB57-8A41-4808-BF4B-D736CB71C3B2] = Code [~C2]=>[#C2] - Desc [#TEST#CREDITORGROUP#2]=>[#TEST#CREDITORGROUP#2]");
						expectedLogs.Add("0C000000000000000000.03000000000000000000 - Modified - PK [6F76E088-FEBF-488D-99BF-80CEC9983F76] = Code [~C2]=>[~C2] - Desc []=>[#TEST#CREDITORGROUP#3]");
						expectedLogs.Add("0C000000000000000000.05000000000000000000 - Modified - PK [05D68C63-39E7-4B6E-847C-95BCB6F1F80B] = Code [~C2]=>[~C2] - Desc [#TEST#CREDITORGROUP#4]=>[]");
					}
					else if (testSubscriber.SpecificColumns.Contains(OrgCreditorGroupSchema.OG_Code))
					{
						expectedLogs.Add("0A000000000000000000.05000000000000000000 - Modified - PK [0B5A48D3-BA8A-4916-8921-F7063D033B2A] = Code [~C1]=>[#C1] - Desc [~TEST~CREDITORGROUP~1]=>[#TEST#CREDITORGROUP#1]");
						expectedLogs.Add("0C000000000000000000.01000000000000000000 - Modified - PK [2B64CB57-8A41-4808-BF4B-D736CB71C3B2] = Code [~C2]=>[#C2] - Desc [#TEST#CREDITORGROUP#2]=>[#TEST#CREDITORGROUP#2]");
					}
					else if (testSubscriber.SpecificColumns.Contains(OrgCreditorGroupSchema.OG_Desc))
					{
						expectedLogs.Add("0A000000000000000000.05000000000000000000 - Modified - PK [0B5A48D3-BA8A-4916-8921-F7063D033B2A] = Code [~C1]=>[#C1] - Desc [~TEST~CREDITORGROUP~1]=>[#TEST#CREDITORGROUP#1]");
						expectedLogs.Add("0B000000000000000000.02000000000000000000 - Modified - PK [2B64CB57-8A41-4808-BF4B-D736CB71C3B2] = Code [~C2]=>[~C2] - Desc [~TEST~CREDITORGROUP~2]=>[#TEST#CREDITORGROUP#2]");
						expectedLogs.Add("0C000000000000000000.03000000000000000000 - Modified - PK [6F76E088-FEBF-488D-99BF-80CEC9983F76] = Code [~C2]=>[~C2] - Desc []=>[#TEST#CREDITORGROUP#3]");
						expectedLogs.Add("0C000000000000000000.05000000000000000000 - Modified - PK [05D68C63-39E7-4B6E-847C-95BCB6F1F80B] = Code [~C2]=>[~C2] - Desc [#TEST#CREDITORGROUP#4]=>[]");
					}
				}

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0C0000000000000000",
					temporaryMaxPeriod: "2015-05-01 00:00:00.000",
					expectedLogs.ToArray());

				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "0C000000000000000000.05000000000000000000.1505");

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code, OG_Desc, OG_IsValid) VALUES
							(1506,15, 0x0D, 0x01, 1, 0x0, '0B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1', '#TEST#CREDITORGROUP#1', 1);
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0D, '2015-06-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0D, 'dbo', 'OrgCreditorGroup', 1, 1506);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				expectedLogs.Clear();

				if (testSubscriber.NotifyDelete)
				{
					expectedLogs.Add("0D000000000000000000.01000000000000000000 - Deleted  - PK [0B5A48D3-BA8A-4916-8921-F7063D033B2A] = Code [~C1] - Desc [#TEST#CREDITORGROUP#1]");
				}

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0D0000000000000000",
					temporaryMaxPeriod: "2015-06-01 00:00:00.000",
					expectedLogs.ToArray());
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "0D000000000000000000.01000000000000000000.1506");

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code, OG_Desc, OG_IsValid) VALUES
							(1507,16, 0x0E, 0x01, 3, 0x0, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2', '#TEST#CREDITORGROUP#2', 1),
							(1507,17, 0x0E, 0x01, 4, 0x0, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2', '#TEST#CREDITORGROUP#2', 0);
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0E, '2015-07-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0E, 'dbo', 'OrgCreditorGroup', 2, 1507);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				expectedLogs.Clear();

				if (testSubscriber.SpecificColumns == null)
				{
					expectedLogs.Add("0e000000000000000000.01000000000000000000 - Modified - PK [2b64cb57-8a41-4808-bf4b-d736cb71c3b2] = Code [~C2]=>[~C2] - Desc [#TEST#CREDITORGROUP#2]=>[#TEST#CREDITORGROUP#2]");
				}

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x0E0000000000000000",
					temporaryMaxPeriod: "2015-07-01 00:00:00.000",
					expectedLogs.ToArray());
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "0E000000000000000000.01000000000000000000.1507");
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendLatestChangesWithConcurrentEtl()
		{
			SchemaStringColumnForTest[] schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RN_Code"), new SchemaStringColumnForTest("RN_Desc") };
			var testSubscriber = new GenericTestDataChangeSubscriber("!RN", "Dummy Test Changed Table List Subscriber", RefCountrySchema.Instance, columns: schemaCols);
			using (var auditConnectionForInserts = AuditTestHelper.GetAuditConnection())
			using (var auditConnectionForEtl = AuditTestHelper.GetAuditConnection())
			{
				SendLatestAuditChangesAndAssert(auditConnectionForEtl, testSubscriber,
					temporaryMaxLsn: "0x000000000000000000",
					temporaryMaxPeriod: "2001-01-10 00:00:00.000",
					Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnectionForEtl, ((IAuditSubscriber)testSubscriber).Code, expected: "00000000000000000000.FFFFFFFFFFFFFFFFFFFF.101");

				//
				// Concurrent ETL inserts first batch of audit table and LSN mapping records transactionally
				//
				auditConnectionForInserts.BeginTransaction();

				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.RefCountry ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RN_PK, RN_Code, RN_Desc) VALUES
							(1802, 1, 0x01, 0x01, 2, 0x01, '015FA4F1-265C-4A88-882B-413283A3D23E', '~1', '~Test~Country~1'),
							(1802, 1, 0x01, 0x02, 2, 0x01, 'AE1520F9-5F19-4D1A-B3D1-B387FC076B58', '~2', '~Test~Country~2'),
							(1802, 2, 0x02, 0x01, 1, 0x01, 'AE1520F9-5F19-4D1A-B3D1-B387FC076B58', '~2', '~Test~Country~2');
						INSERT [{0}].LsnTimeMapping(StartLsn, TranEndTimeUtc) VALUES
							(0x01, '2018-02-20'),
							(0x02, '2018-02-21');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x01, 'dbo', 'RefCountry', 2, 1802);
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x02, 'dbo', 'RefCountry', 1, 1802);",
					BiConstants.BiAdminSchemaName);
				auditConnectionForInserts.ExecuteNonQuery(sqlText);

				// Send latest changes => no committed audit data to send
				SendLatestAuditChangesAndAssert(auditConnectionForEtl,
					testSubscriber,
					temporaryMaxLsn: "0x000000000000000000",
					temporaryMaxPeriod: "2001-01-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnectionForEtl, ((IAuditSubscriber)testSubscriber).Code, expected: "00000000000000000000.FFFFFFFFFFFFFFFFFFFF.101");

				// Commit first batch of audit records
				auditConnectionForInserts.CommitTransaction();

				//
				// Concurrent ETL inserts more audit table and LSN mapping records transactionally
				//
				auditConnectionForInserts.BeginTransaction();

				// Insert audit records
				sqlText = @"
						INSERT dbo.RefCountry ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RN_PK, RN_Code, RN_Desc) VALUES
							(1803, 3, 0x03, 0x01, 3, 0x01, '015FA4F1-265C-4A88-882B-413283A3D23E', '~1', '~Test~Country~1'),
							(1803, 3, 0x03, 0x01, 4, 0x01, '015FA4F1-265C-4A88-882B-413283A3D23E', '#1', '#Test#Country#1'),
							(1803, 3, 0x04, 0x01, 2, 0x01, 'BF01900B-0C2C-4A90-B553-8F07FA5EE13B', '~3', '~Test~Country~3');";
				auditConnectionForInserts.ExecuteNonQuery(sqlText);

				// Send latest changes => should only send committed audit data (first ETL transaction)
				SendLatestAuditChangesAndAssert(auditConnectionForEtl,
					testSubscriber,
					temporaryMaxLsn: "0x02",
					temporaryMaxPeriod: "2018-03-10 00:00:00.000",
					expectedLogs: new string[] {
							"01000000000000000000.01000000000000000000 - Added    - PK [015FA4F1-265C-4A88-882B-413283A3D23E] = Code [~1] - Desc [~TEST~COUNTRY~1]",
							"01000000000000000000.02000000000000000000 - Added    - PK [AE1520F9-5F19-4D1A-B3D1-B387FC076B58] = Code [~2] - Desc [~TEST~COUNTRY~2]",
							"02000000000000000000.01000000000000000000 - Deleted  - PK [AE1520F9-5F19-4D1A-B3D1-B387FC076B58] = Code [~2] - Desc [~TEST~COUNTRY~2]"
					}
				);
				AssertSubscriberHighWaterMark(auditConnectionForEtl, ((IAuditSubscriber)testSubscriber).Code, expected: "02000000000000000000.01000000000000000000.1802");

				// Insert LSN mapping rows
				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [{0}].LsnTimeMapping(StartLsn, TranEndTimeUtc) VALUES
							(0x03, '2018-03-02'),
							(0x04, '2018-03-10');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x03, 'dbo', 'RefCountry', 2, 1803);
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x04, 'dbo', 'RefCountry', 1, 1803);",
					BiConstants.BiAdminSchemaName);
				auditConnectionForInserts.ExecuteNonQuery(sqlText);

				// Send latest changes => no committed audit data to send
				SendLatestAuditChangesAndAssert(auditConnectionForEtl,
					testSubscriber,
					temporaryMaxLsn: "0x020000000000000000",
					temporaryMaxPeriod: "2018-03-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnectionForEtl, ((IAuditSubscriber)testSubscriber).Code, expected: "02000000000000000000.FFFFFFFFFFFFFFFFFFFF.1803");

				// Commit second batch of audit records
				auditConnectionForInserts.CommitTransaction();

				// Send latest changes => should send second batch of audit records (second ETL transaction)
				SendLatestAuditChangesAndAssert(auditConnectionForEtl,
					testSubscriber,
					temporaryMaxLsn: "0x040000000000000000",
					temporaryMaxPeriod: "2018-03-10 00:00:00.000",
					expectedLogs: new string[] {
							"03000000000000000000.01000000000000000000 - Modified - PK [015FA4F1-265C-4A88-882B-413283A3D23E] = Code [~1]=>[#1] - Desc [~TEST~COUNTRY~1]=>[#TEST#COUNTRY#1]",
							"04000000000000000000.01000000000000000000 - Added    - PK [BF01900B-0C2C-4A90-B553-8F07FA5EE13B] = Code [~3] - Desc [~TEST~COUNTRY~3]"
					}
				);
				AssertSubscriberHighWaterMark(auditConnectionForEtl, ((IAuditSubscriber)testSubscriber).Code, expected: "04000000000000000000.01000000000000000000.1803");
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestUpdateSubscriberControlHWMAfterProcessEmptyBatch()
		{
			SchemaStringColumnForTest[] schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("RN_Code"), new SchemaStringColumnForTest("RN_Desc") };
			var testSubscriber = new GenericTestDataChangeSubscriber("!RN", "Dummy Test Changed Table List Subscriber", RefCountrySchema.Instance, columns: schemaCols);
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					temporaryMaxLsn: "0x000000000000000000",
					temporaryMaxPeriod: "2001-01-10 00:00:00.000",
					Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "00000000000000000000.FFFFFFFFFFFFFFFFFFFF.101");

				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.RefCountry ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RN_PK, RN_Code, RN_Desc) VALUES
							(1803, 1, 0x01, 0x01, 2, 0x01, '015FA4F1-265C-4A88-882B-413283A3D23E', '~1', '~Test~Country~1'),
							(1803, 1, 0x01, 0x02, 2, 0x01, 'AE1520F9-5F19-4D1A-B3D1-B387FC076B58', '~2', '~Test~Country~2'),
							(1803, 2, 0x02, 0x01, 1, 0x01, 'AE1520F9-5F19-4D1A-B3D1-B387FC076B58', '~2', '~Test~Country~2');
						INSERT [{0}].LsnTimeMapping(StartLsn, TranEndTimeUtc) VALUES
							(0x01, '2018-03-01'),
							(0x02, '2018-03-02');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x01, 'dbo', 'RefCountry', 2, 1803);
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x02, 'dbo', 'RefCountry', 1, 1803);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				// Send latest changes => ThereAreRelevantChanges is true
				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x010000000000000000",
					temporaryMaxPeriod: "2018-03-10 00:00:00.000",
					expectedLogs: new string[] {
							"01000000000000000000.01000000000000000000 - Added    - PK [015FA4F1-265C-4A88-882B-413283A3D23E] = Code [~1] - Desc [~TEST~COUNTRY~1]",
							"01000000000000000000.02000000000000000000 - Added    - PK [AE1520F9-5F19-4D1A-B3D1-B387FC076B58] = Code [~2] - Desc [~TEST~COUNTRY~2]"
					}
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "01000000000000000000.02000000000000000000.1803");

				// Send latest changes => ThereAreRelevantChanges is true
				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x020000000000000000",
					temporaryMaxPeriod: "2018-03-10 00:00:00.000",
					expectedLogs: new string[] {
							"02000000000000000000.01000000000000000000 - Deleted  - PK [AE1520F9-5F19-4D1A-B3D1-B387FC076B58] = Code [~2] - Desc [~TEST~COUNTRY~2]"
					}
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "02000000000000000000.01000000000000000000.1803");

				// Send latest changes => ThereAreRelevantChanges is false (processed an empty batch), MaxLsn is increased, MaxLsnPeriod is unchanged
				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x030000000000000000",
					temporaryMaxPeriod: "2018-03-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "03000000000000000000.FFFFFFFFFFFFFFFFFFFF.1803");

				// Send latest changes => ThereAreRelevantChanges is false (processed an empty batch), MaxLsn is increased, MaxLsnPeriod is increased
				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x040000000000000000",
					temporaryMaxPeriod: "2018-04-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "04000000000000000000.FFFFFFFFFFFFFFFFFFFF.1804");
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendBatchOfLatestChanges()
		{
			SchemaStringColumnForTest[] schemaCols = new SchemaStringColumnForTest[] { new SchemaStringColumnForTest("GG_Code"), new SchemaStringColumnForTest("GG_Desc") };
			var testSubscriber = new GenericTestDataChangeSubscriber("!GG", "Dummy Test Changed Table List Subscriber", GlbGroupSchema.Instance, columns: schemaCols);

			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: null);

				// Run it once to set high watermark
				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x000000000000000000",
					temporaryMaxPeriod: "2000-01-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "00000000000000000000.FFFFFFFFFFFFFFFFFFFF.1");

				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.GlbGroup ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GG_PK, GG_Code, GG_Desc) VALUES
							(1805, 1, 0x0A, 0x01, 2, 0x, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~GROUP~1'), -- batch 1
							(1805, 2, 0x0A, 0x05, 3, 0x, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '~TEST~GROUP~1'),
							(1805, 3, 0x0A, 0x05, 4, 0x, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#GROUP#1'),
							(1805, 4, 0x0A, 0x08, 2, 0x, '2E254073-FEDD-47E5-BDB3-D22F57986F60', '~2', '~TEST~GROUP~2'),
							(1805, 5, 0x0A, 0x09, 2, 0x, '3FC78F38-C844-4B45-8526-ADD7DAE0F687', '~3', '~TEST~GROUP~3'),
							(1805, 6, 0x0A, 0x0A, 1, 0x, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1', '#TEST#GROUP#1'), 
							(1806, 7, 0x0B, 0x01, 3, 0x, '2E254073-FEDD-47E5-BDB3-D22F57986F60', '~2', '~TEST~GROUP~2'),-- batch 2
							(1806, 8, 0x0B, 0x01, 4, 0x, '2E254073-FEDD-47E5-BDB3-D22F57986F60', '@2', '@TEST@GROUP@2'),
							(1806, 9, 0x0B, 0x02, 1, 0x, '2E254073-FEDD-47E5-BDB3-D22F57986F60', '@2', '@TEST@GROUP@2');
						INSERT INTO [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x0A, '2018-05-01'),
							(0x0B, '2018-06-02');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0A, 'dbo', 'glbgroup', 6, 1805),
						(0x0B, 'dbo', 'glbgroup', 3, 1806);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendOneBatchOfLatestAuditChangesAndAssert(
					auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0A",
					temporaryMaxPeriod: "2018-05-01 00:00:00.000",
					expectedLogs: new string[] {
							//"Processing 5 change(s)",
							"0A000000000000000000.01000000000000000000 - Added    - PK [1322484A-114A-4AD4-849F-15EEE60BA1CB] = Code [~1] - Desc [~TEST~GROUP~1]",
							"0A000000000000000000.05000000000000000000 - Modified - PK [1322484A-114A-4AD4-849F-15EEE60BA1CB] = Code [~1]=>[~1] - Desc [~TEST~GROUP~1]=>[#TEST#GROUP#1]",
							"0A000000000000000000.08000000000000000000 - Added    - PK [2E254073-FEDD-47E5-BDB3-D22F57986F60] = Code [~2] - Desc [~TEST~GROUP~2]",
							"0A000000000000000000.09000000000000000000 - Added    - PK [3FC78F38-C844-4B45-8526-ADD7DAE0F687] = Code [~3] - Desc [~TEST~GROUP~3]",
							"0a000000000000000000.0a000000000000000000 - Deleted  - PK [1322484a-114a-4ad4-849f-15eee60ba1cb] = Code [~1] - Desc [#TEST#GROUP#1]"
					},
					batchSize: 4
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "0A000000000000000000.0A000000000000000000.1805");

				SendOneBatchOfLatestAuditChangesAndAssert(
					auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2018-06-01 00:00:00.000",
					expectedLogs: new string[] {
							//"Processing 2 change(s)",
							"0b000000000000000000.01000000000000000000 - Modified - PK [2e254073-fedd-47e5-bdb3-d22f57986f60] = Code [~2]=>[@2] - Desc [~TEST~GROUP~2]=>[@TEST@GROUP@2]",
							"0b000000000000000000.02000000000000000000 - Deleted  - PK [2e254073-fedd-47e5-bdb3-d22f57986f60] = Code [@2] - Desc [@TEST@GROUP@2]"
					},
					batchSize: 4
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "0B000000000000000000.02000000000000000000.1806");

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.GlbGroup ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GG_PK, GG_Code, GG_Desc) VALUES
							(1806, 2, 0x0C, 0x01, 1, 0x, '3FC78F38-C844-4B45-8526-ADD7DAE0F687', '~3', '~TEST~GROUP~3');
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x0C, '2018-06-01');
						INSERT INTO [{0}].CdcHistorySummary(Lsn, SchemaName, ChangedTableName, NumberOfRows, LsnPeriod) VALUES
						(0x0C, 'dbo', 'glbgroup', 1, 1806);",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendOneBatchOfLatestAuditChangesAndAssert(
					auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0C",
					temporaryMaxPeriod: "2018-06-01 00:00:00.000",
					expectedLogs: new string[] {
							//"Processing 1 change(s)",
							"0C000000000000000000.01000000000000000000 - Deleted  - PK [3FC78F38-C844-4B45-8526-ADD7DAE0F687] = Code [~3] - Desc [~TEST~GROUP~3]"
					},
					batchSize: 10
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "0C000000000000000000.01000000000000000000.1806");

				SendLatestAuditChangesAndAssert(auditConnection,
					testSubscriber,
					temporaryMaxLsn: "0x0C",
					temporaryMaxPeriod: "2018-07-10 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, ((IAuditSubscriber)testSubscriber).Code, expected: "0C000000000000000000.FFFFFFFFFFFFFFFFFFFF.1807");
			}
		}

		void SendOneBatchOfLatestAuditChangesAndAssert(DbConnection auditConnection, ActualDataChangesAuditSubscriber testSubscriber, string temporaryMaxLsn, string temporaryMaxPeriod, string[] expectedLogs, int batchSize)
		{
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, temporaryMaxLsn))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, temporaryMaxPeriod))
			{
				var testLogger = new LoggerForTest();
				var testAuditWrapper = new ActualDataChangesAuditSubscriberWrapperForTest(auditConnection, testSubscriber, testLogger, batchSize);

				if (testAuditWrapper.ShouldRunSubscriber())
				{
					testAuditWrapper.FetchDataAndProcessChanges();
				}

				AuditTestHelper.AssertLog(testLogger.LogEntries, expectedLogs);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestValidateSubscriberThrowsExceptionWhenSubscriberTableIsNull()
		{
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x00"))
			{
				var testSubscriber = new GenericTestDataChangeSubscriber("!~~", "Dummy Test Subscriber", null);
				var sub = testSubscriber.GetWrapper(auditConnection, new LoggerForTest());
				AssertExceptionThrown(typeof(ArgumentException), "Subscriber table cannot be null.", sub.ValidateSubscriber);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestExceptionThrownWhenSubscriberTableIsNotAudited()
		{
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x00"))
			{
				var testSubscriber = new GenericTestDataChangeSubscriber("!~~", "Dummy Test Changed Table List Subscriber", StmJobQueueSchema.Instance);
				var testAuditWrapper = new ActualDataChangesAuditSubscriberWrapperForTest(auditConnection, testSubscriber, new LoggerForTest());
				testAuditWrapper.AddSubscriberToSubscriberControlTable();
				AssertExceptionThrown(typeof(InvalidOperationException), "Table [dbo].[StmJobQueue] is not audited.", () => testAuditWrapper.FetchDataAndProcessChanges());
			}
		}

		#region Assertions

		void SendLatestAuditChangesAndAssert(DbConnection auditConnection, ActualDataChangesAuditSubscriber testSubscriber, string temporaryMaxLsn, string temporaryMaxPeriod, string[] expectedLogs, int batchSize = 1000)
		{
			if (temporaryMaxPeriod != null)
			{
				auditConnection.ExecuteNonQuery(@$"DELETE FROM biadmin.LsnTimeMapping WHERE StartLsn = CONVERT(binary(10), {temporaryMaxLsn}, 2);
					INSERT INTO biadmin.LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (CONVERT(binary(10), {temporaryMaxLsn}, 2), '{temporaryMaxPeriod}');");
			}

			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, temporaryMaxLsn))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, temporaryMaxPeriod))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.AspBatchSize, batchSize.ToString()))
			{
				var testLogger = new LoggerForTest();
				var testAuditWrapper = new ActualDataChangesAuditSubscriberWrapperForTest(auditConnection, testSubscriber, testLogger, batchSize);
				HasChangesToProcess(testAuditWrapper, testLogger, auditConnection, temporaryMaxLsn);
				if (testAuditWrapper.ShouldRunSubscriber())
				{
					if (testAuditWrapper.TableMaxLsn == null)
					{
						testAuditWrapper.TableMaxLsn = testAuditWrapper.NextLsnHighWaterMark;
					}
					testAuditWrapper.FetchDataAndProcessChanges();
				}
				AuditTestHelper.AssertLog(testLogger.LogEntries, expectedLogs);
			}
		}

		void SendLatestAuditChangesAndAssertCount(DbConnection auditConnection, ActualDataChangesAuditSubscriber testSubscriber, string temporaryMaxLsn, string temporaryMaxPeriod, int expectedLogsCount, int batchSize = 1000)
		{
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, temporaryMaxLsn))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, temporaryMaxPeriod))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.AspBatchSize, batchSize.ToString()))
			{
				var testLogger = new LoggerForTest();
				var testAuditWrapper = new ActualDataChangesAuditSubscriberWrapperForTest(auditConnection, testSubscriber, testLogger, batchSize);
				HasChangesToProcess(testAuditWrapper, testLogger, auditConnection, temporaryMaxLsn);
				if (testAuditWrapper.ShouldRunSubscriber())
				{
					if (testAuditWrapper.TableMaxLsn == null)
					{
						testAuditWrapper.TableMaxLsn = testAuditWrapper.NextLsnHighWaterMark;
					}
					testAuditWrapper.FetchDataAndProcessChanges();
				}
				AssertCountOfProcessingOrErrorLogs(expectedLogsCount, testLogger);
			}
		}

		void AssertCountOfProcessingOrErrorLogs(int expectedLogsCount, LoggerForTest testLogger)
		{
			var actualLogs = new List<string>();
				//testLogger.LogEntries.Where(log => !excludedLogs.Any(excluded => log.Contains(excluded))).ToList();
			foreach (var log in testLogger.LogEntries)
			{
				if (!ContainsAnyExcludedLogs(log))
				{
					actualLogs.Add(log);
				}
			}

			AssertEquals($"Actual logs:\r\n{string.Join("\r\n", testLogger.LogEntries)}", expectedLogsCount, actualLogs.Count);
		}

		bool ContainsAnyExcludedLogs(string log)
		{
			// Excludes the following logs which are used for debugging
			var excludedLogs = new List<string>()
			{
				"No relevant changes found",
				"Audit DB HWM: (LSN: ",
				"Current Subscriber High Water Marks (LSN: ",
				"Updating Subscriber High Water Mark (LSN: "
			};

			var containsAnyExcludedLogs = false;
			foreach (var excluded in excludedLogs)
			{
				if (log.Contains(excluded))
				{
					containsAnyExcludedLogs = true;
					break;
				}
			}

			return containsAnyExcludedLogs;
		}

		void HasChangesToProcess(ActualDataChangesAuditSubscriberWrapperForTest testAuditWrapper, LoggerForTest testLogger, DbConnection auditConnection, string temporaryMaxLsn)
		{
			var testSubscribers = new IAuditSubscriber[] { testAuditWrapper };
			var subscriberManager = new SubscriberManagerForTest(testLogger, string.Empty, testSubscribers);

			auditConnection.ExecuteNonQuery($"UPDATE biadmin.TableState SET AetHWMHistorySummaryLsn = {temporaryMaxLsn} WHERE SourceSchemaName = '{testAuditWrapper.Table.SqlSchemaName}' AND SourceTableName = '{testAuditWrapper.Table.TableName}'");

			subscriberManager.HasChangesToProcess_Exposed(testAuditWrapper, auditConnection);
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

		#endregion
	}

	class ActualDataChangesAuditSubscriberWrapperForTest : ActualDataChangesAuditSubscriberWrapper
	{
		public ActualDataChangesAuditSubscriberWrapperForTest(DbConnection auditConnection, ActualDataChangesAuditSubscriber subscriber, ILogger logger = null, int batchSize = 1000)
			 : base(subscriber, auditConnection, logger)
		{
			this.BatchSize = batchSize;
		}

		public void SetBatchSizeForTest(int batchSize)
		{
			this.BatchSize = batchSize;
		}
	}

	class SubscriberManagerForTest : SubscriberManager
	{
		public SubscriberManagerForTest(ILogger logger, string serviceTaskCode, params IAuditSubscriber[] testSubscribers)
			: base(logger, serviceTaskCode)
		{
			this.testSubscribers = testSubscribers;
		}

		protected override IEnumerable<IAuditSubscriber> LoadAllSubscribers()
		{
			return testSubscribers;
		}

		readonly IAuditSubscriber[] testSubscribers;

		public bool HasChangesToProcess_Exposed(IAuditSubscriber subscriber, DbConnection auditConnection)
		{
			return this.HasChangesToProcess(subscriber.GetWrapper(auditConnection, new SubscriberLogger(serviceLogger, subscriber.Code)));
		}
	}
}
