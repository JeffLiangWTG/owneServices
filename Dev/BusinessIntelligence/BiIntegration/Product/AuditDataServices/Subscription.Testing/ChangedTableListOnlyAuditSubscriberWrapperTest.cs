namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using CargoWise.Bi.Common;
	using CargoWise.Bi.Common.Testing;
	using CargoWise.Data;
	using CargoWise.Data.Testing;
	using CargoWise.Schema;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.ChangeDataCapture.Common;
	using Enterprise.Integration;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	class ChangedTableListOnlyAuditSubscriberWrapperTest : TestCase
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendLatestChanges()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscriber = new GenericTestChangedTableListSubscriber(
					"*RX", "Dummy Test Changed Table List Subscriber", new ITableSchema[] { RefCurrencySchema.Instance }
					);

				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: null);

				// Run it once to set high watermark
				SendLatestAuditChangesAndAssert(
					auditConnection,
					testSubscriber,
					expectedLogs: Array.Empty<string>(),
					temporaryMaxLsn: "0x00",
					temporaryMaxPeriod: "2001-01-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "00000000000000000000");

				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code) VALUES
							(1710, 1, 0x0A, 0x01, 2, 0x01, '1322484A-114A-4AD4-849F-15EEE60BA1CB', '~1'),
							(1711, 2, 0x0C, 0x01, 3, 0x01, '2E254073-FEDD-47E5-BDB3-D22F57986F60', '~2'),
							(1711, 2, 0x0C, 0x01, 4, 0x01, '2E254073-FEDD-47E5-BDB3-D22F57986F60', '@2');
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x0C WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0C, '2017-11-01');",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(
					auditConnection,
					testSubscriber,
					expectedLogs: new string[] {
							"[dbo].[RefCurrency]"
					},
					temporaryMaxLsn: "0x0C",
					temporaryMaxPeriod: "2017-11-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "0C000000000000000000");

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code) VALUES
							(1711, 3, 0x10, 0x01, 1, 0x01, '2E254073-FEDD-47E5-BDB3-D22F57986F60', '~2');
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x10 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2017-11-02');",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(
					auditConnection,
					testSubscriber,
					expectedLogs: new string[] {
							"[dbo].[RefCurrency]"
					},
					temporaryMaxLsn: "0x10",
					temporaryMaxPeriod: "2017-11-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "10000000000000000000");

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x11, '2017-11-03');",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(
					auditConnection,
					testSubscriber,
					expectedLogs: Array.Empty<string>(),
					temporaryMaxLsn: "0x11",
					temporaryMaxPeriod: "2017-11-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "11000000000000000000");

				SendLatestAuditChangesAndAssert(
					auditConnection,
					testSubscriber,
					expectedLogs: Array.Empty<string>(),
					temporaryMaxLsn: "0x11",
					temporaryMaxPeriod: "2017-11-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "11000000000000000000");
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendingLatestChangesForTheFirstTimeDoesNotLoadTheEntireBacklogOfChanges()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscriber = new GenericTestChangedTableListSubscriber(
					"*RX", "Dummy Test Changed Table List Subscriber", new ITableSchema[] { RefCurrencySchema.Instance }
					);

				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: null);

				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc) VALUES
							(1001, 1, 0x0A, 0x01, 2, 0x0, 'EC8567A3-0AF4-40B8-9463-53A0239B4208', '~1', '~Rx1'),
							(1001, 1, 0x0A, 0x05, 3, 0x0, 'EC8567A3-0AF4-40B8-9463-53A0239B4208', '~1', '~Rx1'),
							(1001, 1, 0x0A, 0x05, 4, 0x0, 'EC8567A3-0AF4-40B8-9463-53A0239B4208', '~1', '#RxUno');
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x0A WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0A, '2010-01-01');",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				// Sending latest changes for the first time => does not load change backlog
				// ...and sets high watermark to the latest existing change transaction
				SendLatestAuditChangesAndAssert(
					auditConnection,
					testSubscriber,
					expectedLogs: Array.Empty<string>(),
					temporaryMaxLsn: "0x0A",
					temporaryMaxPeriod: "2010-01-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "0A000000000000000000");

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code, RX_Desc) VALUES
							(1001, 2, 0x0B, 0x01, 1, 0x0, 'EC8567A3-0AF4-40B8-9463-53A0239B4208', '~1', '#RxUno');
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0B, '2010-01-02');",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				// Sending latest changes for the second time => only loads the changes which happened after the first time
				SendLatestAuditChangesAndAssert(
					auditConnection,
					testSubscriber,
					expectedLogs: new string[] {
							"[dbo].[RefCurrency]"
					},
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2010-01-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "0B000000000000000000");

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					expectedLogs: Array.Empty<string>(),
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2010-01-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "0B000000000000000000");
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendLatestChangesWithMultipleTables()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var testSubscriber = new GenericTestChangedTableListSubscriber(
					"*XX", "Dummy Test Changed Table List Subscriber", new ITableSchema[] { RefCurrencySchema.Instance, OrgCreditorGroupSchema.Instance, GlbStaffSchema.Instance }
					);

				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: null);

				// Run it once to set high watermark
				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					expectedLogs: Array.Empty<string>(),
					temporaryMaxLsn: null,
					temporaryMaxPeriod: null
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "00000000000000000000");

				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code) VALUES
							(1710, 1, 0x0A, 0x01, 2, 0x01, '1B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1'),
							(1710, 1, 0x0A, 0x05, 3, 0x01, '1B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1'),
							(1710, 1, 0x0A, 0x05, 4, 0x01, '1B5A48D3-BA8A-4916-8921-F7063D033B2A', '#C1');
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code) VALUES
							(1710, 1, 0x0A, 0x09, 2, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2'),
							(1711, 2, 0x0B, 0x02, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2');
						INSERT dbo.GlbStaff ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GS_PK, GS_Code) VALUES
							(1711, 2, 0x0C, 0x04, 1, 0x01, '3BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', '~C3');
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x0A WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'OrgCreditorGroup'
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbStaff'
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
								(0x0A, '2017-10-01'),
								(0x0D, '2017-11-01');",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					expectedLogs: new string[] {
							"[dbo].[RefCurrency]",
							"[dbo].[OrgCreditorGroup]",
							"[dbo].[GlbStaff]"
					},
					temporaryMaxLsn: "0x0D",
					temporaryMaxPeriod: "2017-11-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "0D000000000000000000");

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code) VALUES
							(1711, 2, 0x10, 0x01, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2');
						INSERT dbo.GlbStaff ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GS_PK, GS_Code) VALUES
							(1711, 2, 0x10, 0x01, 3, 0x01, '4BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', '~C4'),
							(1711, 2, 0x10, 0x01, 4, 0x01, '4BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', '~C4');
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x10 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'OrgCreditorGroup'
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x10 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbStaff'
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x10, '2017-11-02');",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					expectedLogs: new string[] {
							"[dbo].[OrgCreditorGroup]",
							"[dbo].[GlbStaff]"
					},
					temporaryMaxLsn: "0x10",
					temporaryMaxPeriod: "2017-11-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "10000000000000000000");

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code) VALUES
							(1711, 3, 0x11, 0x01, 1, 0x01, '5EA590BA-97E3-4A5B-A35D-E6D4AF3AB9D2', '~C5');
						INSERT dbo.OrgHeader ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OH_PK, OH_Code) VALUES
							(1711, 3, 0x11, 0x04, 2, 0x01, '6BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', '~C6');
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x11 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x11, '2017-11-03');",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					expectedLogs: new string[] {
							"[dbo].[RefCurrency]"
					},
					temporaryMaxLsn: "0x11",
					temporaryMaxPeriod: "2017-11-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "11000000000000000000");

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.AccGroups ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], AR_PK, AR_Code) VALUES
							(1711, 4, 0x1A, 0x04, 2, 0x01, '7BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', '~C7');
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x1A, '2017-11-04');",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					expectedLogs: Array.Empty<string>(),
					temporaryMaxLsn: "0x1A",
					temporaryMaxPeriod: "2017-11-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "1A000000000000000000");
			}
		}

		/// <summary>
		/// Note: SubscriberLoaderTest ensures no IChangedTableListOnlyAuditSubscriber subscribes for non-audited tables.
		/// </summary>
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendChangedTableListWithNonAuditedTableThrowsNoExceptions()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				AssertTableIsNotAudited("dbo", "RouteSegment", auditConnection);
				var testSubscriber = new GenericTestChangedTableListSubscriber(
					"*SL", "Dummy Test Changed Table List Subscriber", new ITableSchema[] { RouteSegmentSchema.Instance }
					);

				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: null);

				// Run it once to set high watermark
				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					expectedLogs: Array.Empty<string>(),
					temporaryMaxLsn: null,
					temporaryMaxPeriod: null
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "00000000000000000000");

				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x0C WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						INSERT [{0}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x0C, '2017-12-03');",
					BiConstants.BiAdminSchemaName);
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					expectedLogs: Array.Empty<string>(),
					temporaryMaxLsn: "0x0C",
					temporaryMaxPeriod: "2017-12-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "0C000000000000000000");

				SendLatestAuditChangesAndAssert(auditConnection, testSubscriber,
					expectedLogs: Array.Empty<string>(),
					temporaryMaxLsn: "0x0C",
					temporaryMaxPeriod: "2017-12-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnection, testSubscriber.Code, expected: "0C000000000000000000");
			}
		}

		void AssertTableIsNotAudited(string schemaName, string tableName, DbConnection auditConnection)
		{
			var cdcTable = new CdcTable(schemaName, tableName);
			Assert($"{schemaName}.{tableName} should not be audited", !cdcTable.IsCdcEnabled(auditConnection));
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendLatestChangesWithConcurrentEtl()
		{
			using (var auditConnectionForInserts = AuditTestHelper.GetAuditConnection())
			using (var auditConnectionForEtl = AuditTestHelper.GetAuditConnection())
			{
				var testSubscriber = new GenericTestChangedTableListSubscriber(
					"*XX", "Dummy Test Changed Table List Subscriber", new ITableSchema[] { RefCurrencySchema.Instance, OrgCreditorGroupSchema.Instance, GlbStaffSchema.Instance }
				);

				AuditTestHelper.CleanupAuditTestData(auditConnectionForEtl, testSubscriber);
				AuditTestHelper.ResetSubscriberControlWaterMark(auditConnectionForEtl, testSubscriber);

				AssertSubscriberHighWaterMark(auditConnectionForEtl, testSubscriber.Code, expected: "00000000000000000000");

				//
				// Concurrent ETL inserts first batch of audit table and LSN mapping records transactionally
				//
				auditConnectionForInserts.BeginTransaction();

				string sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code) VALUES
							(1802, 1, 0x0A, 0x01, 2, 0x01, '1B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1'),
							(1802, 1, 0x0A, 0x05, 3, 0x01, '1B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1'),
							(1802, 1, 0x0A, 0x05, 4, 0x01, '1B5A48D3-BA8A-4916-8921-F7063D033B2A', '#C1');
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code) VALUES
							(1802, 1, 0x0A, 0x09, 2, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2'),
							(1803, 1, 0x0B, 0x02, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2');
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x0A WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'OrgCreditorGroup'
						INSERT [{0}].LsnTimeMapping(StartLsn, TranEndTimeUtc) VALUES
							(0x0A, '2018-02-20'),
							(0x0B, '2018-03-10');",
					BiConstants.BiAdminSchemaName);
				auditConnectionForInserts.ExecuteNonQuery(sqlText);

				// Send latest changes => no committed audit data to send
				SendLatestAuditChangesAndAssert(auditConnectionForEtl, testSubscriber,
					expectedLogs: Array.Empty<string>(),
					temporaryMaxLsn: null,
					temporaryMaxPeriod: null
				);
				AssertSubscriberHighWaterMark(auditConnectionForEtl, testSubscriber.Code, expected: "00000000000000000000");

				// Commit first batch of audit records
				auditConnectionForInserts.CommitTransaction();

				//
				// Concurrent ETL inserts more audit table and LSN mapping records transactionally
				//
				auditConnectionForInserts.BeginTransaction();

				// Insert audit records
				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT dbo.GlbStaff ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GS_PK, GS_Code) VALUES
							(1803, 1, 0x0C, 0x04, 1, 0x01, '3BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', '~C3');
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code) VALUES
							(1803, 1, 0x10, 0x01, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2');
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x0C WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbStaff'
						UPDATE [biadmin].TableState SET AetHWMHistorySummaryLsn = 0x10 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'OrgCreditorGroup'
						",
					BiConstants.BiAdminSchemaName);
				auditConnectionForInserts.ExecuteNonQuery(sqlText);

				// Send latest changes => should only send committed audit data (first ETL transaction)
				SendLatestAuditChangesAndAssert(auditConnectionForEtl, testSubscriber,
					expectedLogs: new string[] {
							"[dbo].[RefCurrency]",
							"[dbo].[OrgCreditorGroup]"
					},
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2018-03-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnectionForEtl, ((IAuditSubscriber)testSubscriber).Code, expected: "0B000000000000000000");

				// Insert LSN mapping rows
				sqlText = string.Format(CultureInfo.InvariantCulture, @"
						INSERT [{0}].LsnTimeMapping(StartLsn, TranEndTimeUtc) VALUES
							(0x0C, '2018-03-15'),
							(0x10, '2018-03-17');",
					BiConstants.BiAdminSchemaName);
				auditConnectionForInserts.ExecuteNonQuery(sqlText);

				// Send latest changes => no committed audit data to send
				SendLatestAuditChangesAndAssert(auditConnectionForEtl, testSubscriber,
					expectedLogs: Array.Empty<string>(),
					temporaryMaxLsn: "0x0B",
					temporaryMaxPeriod: "2018-03-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnectionForEtl, ((IAuditSubscriber)testSubscriber).Code, expected: "0B000000000000000000");

				// Commit second batch of audit records
				auditConnectionForInserts.CommitTransaction();

				// Send latest changes => should send second batch of audit records (second ETL transaction)
				SendLatestAuditChangesAndAssert(auditConnectionForEtl, testSubscriber,
					expectedLogs: new string[] {
							"[dbo].[GlbStaff]",
							"[dbo].[OrgCreditorGroup]"
					},
					temporaryMaxLsn: "0x10",
					temporaryMaxPeriod: "2018-03-10 00:00:00.000"
				);
				AssertSubscriberHighWaterMark(auditConnectionForEtl, ((IAuditSubscriber)testSubscriber).Code, expected: "10000000000000000000");
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGetTablesWithChangesSinceLastCheckUseNolock()
		{
			var tables = new ITableSchema[] { RefCurrencySchema.Instance };
			var rawSubscriber = new GenericTestChangedTableListSubscriber("*UT", "Unit Test", tables);
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var arrangeSql = $@"
					INSERT [{BiConstants.BiAdminSchemaName}].SubscriberControl
						(SubscriberCode, Description, LsnHighWaterMark, SeqValHighWaterMark)
						VALUES ('*UT', 'Unit Test', 0, 0);";
				auditConnection.ExecuteNonQuery(arrangeSql);
			}

			using (var blockerStarted = new AutoResetEvent(false))
			{
				var task = Task.Run(() =>
				{
					using (var auditConnection = AuditTestHelper.GetAuditConnection())
					{
						var testSubscriber = rawSubscriber.GetWrapper(auditConnection, new LoggerForTest());
						using (var transactionManager = testSubscriber.auditConnection.BeginTransactionWithManager())
						{
							testSubscriber.HasChangesToProcess();
							blockerStarted.Set();
							testSubscriber.auditConnection.ExecuteNonQuery("WAITFOR DELAY '00:00:05'");
						}
					}
				});
				var actSql = $@"
						UPDATE [{BiConstants.BiAdminSchemaName}].SubscriberControl
						SET LsnHighWaterMark = 1
						WHERE SubscriberCode = '*UT' AND Description = 'Unit Test'
					";

				blockerStarted.WaitOne();
				using (var auditConnectionForDML = AuditTestHelper.GetAuditConnection())
				using (auditConnectionForDML.TemporarySetLockTimeout(TimeSpan.FromSeconds(2d)))
				{
					AssertNoExceptionThrown(() => auditConnectionForDML.ExecuteNonQuery(actSql));
				}
			}
		}

		public void TestValidateSubscriberThrowsExceptionWhenSubscribedTablesPropertyIsNull()
		{
			using (var conn = AuditTestHelper.GetAuditConnection())
			{
				var testSubscriber = new GenericTestChangedTableListSubscriber("*~~", "Dummy Test Changed Table List Subscriber", null).GetWrapper(conn, new DummyLogger());
				AssertExceptionThrown(typeof(ArgumentException), "Subscriber tables property cannot be null.", testSubscriber.ValidateSubscriber);
			}
		}

		public void TestValidateSubscriberThrowsExceptionWhenAtLeastOneSubscribedTableIsNull()
		{
			using (var conn = AuditTestHelper.GetAuditConnection())
			{
				var testSubscriber = new GenericTestChangedTableListSubscriber("*~~", "Dummy Test Changed Table List Subscriber", new ITableSchema[] { RefCountrySchema.Instance, null }).GetWrapper(conn, new DummyLogger());
				AssertExceptionThrown(typeof(ArgumentException), "Subscriber tables property cannot contain any null elements.", testSubscriber.ValidateSubscriber);
			}
		}

		void SendLatestAuditChangesAndAssert(DbConnection auditConnection, IAuditSubscriber rawSubscriber, string[] expectedLogs, string temporaryMaxLsn, string temporaryMaxPeriod)
		{
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, temporaryMaxLsn))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, temporaryMaxPeriod))
			{
				var testSubscriber = rawSubscriber.GetWrapper(auditConnection, new LoggerForTest());
				var logger = (LoggerForTest)testSubscriber.Logger;
				logger.ClearLog();
				if (testSubscriber.ShouldRunSubscriber())
				{
					testSubscriber.FetchDataAndProcessChanges();
				}
				AuditTestHelper.AssertLog(logger.LogEntries, expectedLogs);
			}
		}

		void AssertSubscriberHighWaterMark(DbConnection connection, string subscriberCode, string expected)
		{
			string sqlText = $@"
				SELECT LsnHighWaterMark
				FROM [{BiConstants.BiAdminSchemaName}].SubscriberControl
				WHERE SubscriberCode = '{subscriberCode}'
			";
			var actualObj = connection.ExecuteScalar(sqlText);
			string actualValue = (actualObj == null) ? null : string.Join("", ((byte[])actualObj).Select(b => string.Format("{0:x2}", b))).ToUpper();
			AssertEquals("LsnHighWaterMark for subscriber [" + subscriberCode + "]", expected, actualValue);
		}
	}
}
