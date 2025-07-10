using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.Common.Testing;
using CargoWise.Bi.Maintenance;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	public class TableValuePairSubscriberWrapperTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var scriptRunner = new AuditPartitionScriptRunner(auditConnection, new LoggerForTest());
				scriptRunner.Run();
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestUpdatesLsnHwmEvenWithoutOperation1()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var rawSubscriber = new GenericTestTableValuePairSubscriber(
					"*D1", "Dummy Test Changed Table List Subscriber"
				);

				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: null);

				// Run it once to set high watermark
				SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
					temporaryMaxLsn: null,
					temporaryMaxPeriod: null,
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "00000000000000000000");

				// No deletes
				string sqlText = $@"
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code) VALUES
							(1710, 1, 0x0A, 0x01, 2, 0x01, '1B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1'),
							(1710, 2, 0x0A, 0x05, 3, 0x01, '8D2B5B43-910C-4FC9-BB12-AAB673A7E235', '~C1'),
							(1710, 3, 0x0A, 0x05, 4, 0x01, '8D2B5B43-910C-4FC9-BB12-AAB673A7E235', '#C1');
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code) VALUES
							(1710, 2, 0x0A, 0x05, 2, 0x01, '376CC518-06F7-4CC5-9928-3418CD83A10F', '#C2'),
							(1710, 3, 0x0A, 0x09, 2, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '^C2'),
							(1711, 6, 0x0C, 0x02, 3, 0x01, '3C154C3F-97A9-4646-840B-0A078212017B', '!C2');
						INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x0A, '2017-10-01'),
							(0x0B, '2017-11-01'),
							(0x0C, '2017-11-01'),
							(0x0D, '2017-11-01');
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbStaff'
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'OrgCreditorGroup'
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'RefCurrency', 20, 1, 1710),
							(0x0B, 'dbo', 'RefCurrency', 20, 1, 1711),
							(0x0C, 'dbo', 'RefCurrency', 20, 1, 1711),
							(0x0D, 'dbo', 'RefCurrency', 20, 1, 1711);
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'OrgCreditorGroup', 20, 1, 1710),
							(0x0B, 'dbo', 'OrgCreditorGroup', 20, 1, 1711),
							(0x0C, 'dbo', 'OrgCreditorGroup', 20, 1, 1711),
							(0x0D, 'dbo', 'OrgCreditorGroup', 20, 1, 1711);
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'GlbStaff', 20, 1, 1710),
							(0x0B, 'dbo', 'GlbStaff', 20, 1, 1711),
							(0x0C, 'dbo', 'GlbStaff', 20, 1, 1711),
							(0x0D, 'dbo', 'GlbStaff', 20, 1, 1711);
					";
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
					temporaryMaxLsn: "0x0D",
					temporaryMaxPeriod: "2017-11-01",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "0D000000000000000000");
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit } )]
		public void TestGetChangeCountHandlesInvalidObjectNameException()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var sqlException = SqlExceptionBuilder.CreateSqlException(208, "Invalid object name '$partition.PF_LsnPeriodFunction'.");
				var rawSubscriber = new GenericTestTableValuePairSubscriber(
					"*D1", "Dummy Test Changed Table List Subscriber",
					exceptionToThrowDuringProcessChanges: sqlException
				);

				try
				{
					AuditTestHelper.CleanupAuditTestData(auditConnection, rawSubscriber);
					BiMasterState.SetParameter(auditConnection, BiConstants.LastIndexRebuildUtcDt, "bingbong");

					var indexDate = BiMasterState.GetParameter(auditConnection, BiConstants.LastIndexRebuildUtcDt);
					AssertEquals("Should have last index rebuild paramater set", "bingbong", indexDate);

					// Run it once to set high watermark
					SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
						temporaryMaxLsn: null,
						temporaryMaxPeriod: null,
						expectedLogs: Array.Empty<string>()
					);
					AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "00000000000000000000");

					string sqlText = $@"
						INSERT dbo.GlbStaff ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GS_PK, GS_Code) VALUES
							(1711, 1, 0x0B, 0x01, 1, 0x01, '3BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', '~C3'), -- Has the same Lsn and SeqVal as another record
							(1711, 2, 0x0D, 0x04, 1, 0x01, 'CBFA90A3-C438-4033-8029-62AB947D5628', '!C3');
						INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x0A, '2017-10-01'),
							(0x0B, '2017-11-01'),
							(0x0C, '2017-11-01'),
							(0x0D, '2017-11-01');
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbStaff'
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'GlbStaff', 20, 1, 1710),
							(0x0B, 'dbo', 'GlbStaff', 20, 1, 1711),
							(0x0C, 'dbo', 'GlbStaff', 20, 1, 1711),
							(0x0D, 'dbo', 'GlbStaff', 20, 1, 1711);
					";
					auditConnection.ExecuteNonQuery(sqlText);

					// Should not process changes if invalid object exception thrown
					SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
						temporaryMaxLsn: "0x0D",
						temporaryMaxPeriod: "2017-11-01",
						expectedLogs: new string[] {
							$@"Audit DB Partitioning required, deleting ""{BiConstants.LastIndexRebuildUtcDt}"" and nudging ASP Service task"
						}
					);
					AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "00000000000000000000");

					indexDate = BiMasterState.GetParameter(auditConnection, BiConstants.LastIndexRebuildUtcDt);
					AssertEquals("Should have last index rebuild paramater set", string.Empty, indexDate);
				}
				finally
				{
					AuditTestHelper.CleanupAuditTestData(auditConnection, rawSubscriber);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestNotifyDelete()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var rawSubscriber = new GenericTestTableValuePairSubscriber(
					"*D1", "Dummy Test Changed Table List Subscriber"
				);

				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: null);

				// Run it once to set high watermark
				SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
					temporaryMaxLsn: null,
					temporaryMaxPeriod: null,
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "00000000000000000000");

				string sqlText = $@"
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code) VALUES
							(1710, 1, 0x0A, 0x01, 2, 0x01, '1B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1'),
							(1710, 2, 0x0A, 0x05, 3, 0x01, '8D2B5B43-910C-4FC9-BB12-AAB673A7E235', '~C1'),
							(1710, 3, 0x0A, 0x05, 4, 0x01, '8D2B5B43-910C-4FC9-BB12-AAB673A7E235', '#C1'),
							(1711, 4, 0x0B, 0x01, 1, 0x01, 'BFD9AB67-700E-43A1-9488-9CFA740E9304', '!C1');
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code) VALUES
							(1710, 1, 0x0A, 0x05, 1, 0x01, '376CC518-06F7-4CC5-9928-3418CD83A10F', '~C2'), -- deferred update
							(1710, 2, 0x0A, 0x05, 2, 0x01, '376CC518-06F7-4CC5-9928-3418CD83A10F', '#C2'),
							(1710, 3, 0x0A, 0x09, 2, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '^C2'), -- insert
							(1711, 5, 0x0B, 0x02, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '^C2'), -- this is a delete
							(1711, 6, 0x0C, 0x02, 3, 0x01, '3C154C3F-97A9-4646-840B-0A078212017B', '!C2');
						INSERT dbo.GlbStaff ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GS_PK, GS_Code) VALUES
							(1711, 1, 0x0B, 0x01, 1, 0x01, '3BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', '~C3'), -- Has the same Lsn and SeqVal as another record
							(1711, 2, 0x0D, 0x04, 1, 0x01, 'CBFA90A3-C438-4033-8029-62AB947D5628', '!C3');
						INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x0A, '2017-10-01'),
							(0x0B, '2017-11-01'),
							(0x0C, '2017-11-01'),
							(0x0D, '2017-11-01');
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbStaff'
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'OrgCreditorGroup'
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'RefCurrency', 20, 1, 1710),
							(0x0B, 'dbo', 'RefCurrency', 20, 1, 1711),
							(0x0C, 'dbo', 'RefCurrency', 20, 1, 1711),
							(0x0D, 'dbo', 'RefCurrency', 20, 1, 1711);
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'OrgCreditorGroup', 20, 1, 1710),
							(0x0B, 'dbo', 'OrgCreditorGroup', 20, 1, 1711),
							(0x0C, 'dbo', 'OrgCreditorGroup', 20, 1, 1711),
							(0x0D, 'dbo', 'OrgCreditorGroup', 20, 1, 1711);
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'GlbStaff', 20, 1, 1710),
							(0x0B, 'dbo', 'GlbStaff', 20, 1, 1711),
							(0x0C, 'dbo', 'GlbStaff', 20, 1, 1711),
							(0x0D, 'dbo', 'GlbStaff', 20, 1, 1711);
					";
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
					temporaryMaxLsn: "0x0D",
					temporaryMaxPeriod: "2017-11-01",
					expectedLogs: new string[] {
							"[dbo].[RefCurrency] - (2B64CB57-8A41-4808-BF4B-D736CB71C3B2, 376cc518-06f7-4cc5-9928-3418cd83a10f)",
							"[dbo].[OrgCreditorGroup] - (BFD9AB67-700E-43A1-9488-9CFA740E9304)",
							"[dbo].[GlbStaff] - (" +
								"3BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7, " +
								"CBFA90A3-C438-4033-8029-62AB947D5628" +
							")"
					}
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "0D000000000000000000");
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestSendLatestChanges()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var rawSubscriber = new GenericTestTableValuePairSubscriber(
					"*D1", "Dummy Test Changed Table List Subscriber");

				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: null);

				// Run it once to set high watermark
				SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
					temporaryMaxLsn: null,
					temporaryMaxPeriod: null,
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "00000000000000000000");

				string sqlText = $@"
						INSERT [dbo].[OrgCreditorGroup] ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OG_PK, OG_Code) VALUES
							(1710, 1, 0x0A, 0x01, 2, 0x01, '1B5A48D3-BA8A-4916-8921-F7063D033B2A', '~C1'),
							(1710, 1, 0x0A, 0x05, 3, 0x01, '8D2B5B43-910C-4FC9-BB12-AAB673A7E235', '~C1'),
							(1710, 1, 0x0A, 0x05, 4, 0x01, '8D2B5B43-910C-4FC9-BB12-AAB673A7E235', '#C1');
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code) VALUES
							(1710, 1, 0x0A, 0x09, 2, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2'),
							(1711, 2, 0x0B, 0x02, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', '~C2');
						INSERT dbo.GlbStaff ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], GS_PK, GS_Code) VALUES
							(1711, 2, 0x0C, 0x04, 1, 0x01, '3BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', '~C3');
						INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x0A, '2017-10-01'),
							(0x0B, '2017-11-01'),
							(0x0C, '2017-11-01'),
							(0x0D, '2017-11-01');
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbStaff'
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'OrgCreditorGroup'
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'RefCurrency', 20, 1, 1710),
							(0x0B, 'dbo', 'RefCurrency', 20, 1, 1711),
							(0x0C, 'dbo', 'RefCurrency', 20, 1, 1711),
							(0x0D, 'dbo', 'RefCurrency', 20, 1, 1711);
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'OrgCreditorGroup', 20, 1, 1710),
							(0x0B, 'dbo', 'OrgCreditorGroup', 20, 1, 1711),
							(0x0C, 'dbo', 'OrgCreditorGroup', 20, 1, 1711),
							(0x0D, 'dbo', 'OrgCreditorGroup', 20, 1, 1711);
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'GlbStaff', 20, 1, 1710),
							(0x0B, 'dbo', 'GlbStaff', 20, 1, 1711),
							(0x0C, 'dbo', 'GlbStaff', 20, 1, 1711),
							(0x0D, 'dbo', 'GlbStaff', 20, 1, 1711);
					";
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
					temporaryMaxLsn: "0x0D",
					temporaryMaxPeriod: "2017-11-01",
					expectedLogs: new string[] {
							"[dbo].[RefCurrency] - (2B64CB57-8A41-4808-BF4B-D736CB71C3B2)",
							"[dbo].[GlbStaff] - (3BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7)"
					}
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "0D000000000000000000");

				sqlText = $@"
						INSERT dbo.RefCurrency ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RX_PK, RX_Code) VALUES
							(1711, 3, 0x10, 0x01, 1, 0x01, '2E254073-FEDD-47E5-BDB3-D22F57986F60', '~2');
						INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x10, '2017-11-02');
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x10 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x10 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbStaff'
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x10 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'OrgCreditorGroup'
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x10, 'dbo', 'RefCurrency', 20, 1, 1711),
							(0x10, 'dbo', 'OrgCreditorGroup', 20, 1, 1711),
							(0x10, 'dbo', 'GlbStaff', 20, 1, 1711);
					";
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
					temporaryMaxLsn: "0x10",
					temporaryMaxPeriod: "2017-11-01",
					expectedLogs: new string[] {
							"[dbo].[RefCurrency] - (2E254073-FEDD-47E5-BDB3-D22F57986F60)"
					}
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "10000000000000000000");

				sqlText = $@"
						INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x11, '2017-11-03');
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x11 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefCurrency'
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x11 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'GlbStaff'
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x11 WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'OrgCreditorGroup'
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x11, 'dbo', 'RefCurrency', 20, 1, 1711),
							(0x11, 'dbo', 'RefCurrency', 20, 1, 1711),
							(0x11, 'dbo', 'OrgCreditorGroup', 20, 1, 1711),
							(0x11, 'dbo', 'GlbStaff', 20, 1, 1711);
					";
				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
					temporaryMaxLsn: "0x11",
					temporaryMaxPeriod: "2017-11-01",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "11000000000000000000");

				SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
					temporaryMaxLsn: "0x12",
					temporaryMaxPeriod: "2017-11-01",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "12000000000000000000");
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGetTablesWithChangesSinceLastCheckUseNolock()
		{
			var rawSubscriber = new GenericTestTableValuePairSubscriber("*UT", "Unit Test");
			using (var auditConnectionForDML = AuditTestHelper.GetAuditConnection())
			{
				var arrangeSql = $@"
					INSERT [{BiConstants.BiAdminSchemaName}].SubscriberControl
						(SubscriberCode, Description, LsnHighWaterMark, SeqValHighWaterMark)
						VALUES ('*UT', 'Unit Test', 0, 0);";
				auditConnectionForDML.ExecuteNonQuery(arrangeSql);
			}

			using (var blockerStarted = new AutoResetEvent(false))
			{
				var task = Task.Run(() =>
				{
					using (var auditConnection = AuditTestHelper.GetAuditConnection())
					{
						var testSubscriber = rawSubscriber.GetWrapper(auditConnection, new LoggerForTest()) as TableValuePairSubscriberWrapper;
						using (var transactionManager = testSubscriber.auditConnection.BeginTransactionWithManager())
						{
							testSubscriber.HasChangesToProcess();
							//testSubscriber.GetTablesWithChangesSinceLastCheck_ForTest();
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

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestTablesWithNoCorrespondingSchema()
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			{
				var rawSubscriber = new GenericTestTableValuePairSubscriber(
					"*D1", "Dummy Test Changed Table List Subscriber"
				);

				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: null);

				SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
					temporaryMaxLsn: null,
					temporaryMaxPeriod: null,
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "00000000000000000000");

				// Test OrgCustomerAddress, CrmOpportunityStageProgress, NettingFXDeal, NettingOrgChargeCode, NettingSystemChargeCode tables (ExcludedTable in Build.xml so doesn't have schema file)
				// Test RefUNLOCOUtcOffset table (Cannot resolve schema as RefUNLOCOUtcOffsetSchema refers to synonym in RefDatabase)
				string sqlText = $@"
						INSERT dbo.OrgCustomerAddress ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], OPA_PK) VALUES
							(1710, 1, 0x0A, 0x05, 1, 0x01, '376CC518-06F7-4CC5-9928-3418CD83A10F'), -- deferred update
							(1710, 2, 0x0A, 0x05, 2, 0x01, '376CC518-06F7-4CC5-9928-3418CD83A10F'),
							(1710, 3, 0x0A, 0x09, 2, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2'), -- insert
							(1711, 5, 0x0B, 0x02, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2'), -- this is a delete
							(1711, 6, 0x0C, 0x02, 3, 0x01, '3C154C3F-97A9-4646-840B-0A078212017B');
						INSERT dbo.CrmOpportunityStageProgress ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], CSP_PK) VALUES
							(1711, 1, 0x0B, 0x01, 1, 0x01, '7921ad4a-5e26-4f3f-b603-51600631af60'), -- delete
							(1711, 2, 0x0D, 0x04, 1, 0x01, 'c574562b-c2d1-4218-8865-36c90d0fb231');
						INSERT dbo.NettingFXDeal ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], NFD_PK) VALUES
							(1711, 1, 0x0B, 0x01, 1, 0x01, '00203769-552b-48dd-b751-c2f6ac21fb69'), -- delete
							(1711, 2, 0x0D, 0x04, 1, 0x01, 'b8db4b20-09dd-462b-9052-86731b9cccad');
						INSERT dbo.NettingOrgChargeCode ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], NOC_PK) VALUES
							(1711, 1, 0x0B, 0x01, 1, 0x01, '0891a6e9-82ff-4c8c-8a6a-889a93e74af1'), -- delete
							(1711, 2, 0x0D, 0x04, 1, 0x01, '6b407f53-340c-4c8b-939a-2c8d4ecb7885');
						INSERT dbo.NettingSystemChargeCode ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], NCC_PK) VALUES
							(1711, 1, 0x0B, 0x01, 1, 0x01, '04bbf235-6943-4ea3-b1cf-d34130b454e3'), -- delete
							(1711, 2, 0x0D, 0x04, 1, 0x01, '5d2ccc88-c20a-494b-ae05-4abd4e547ec8');
						INSERT dbo.RefUNLOCOUtcOffset ([__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], RLO_PK) VALUES
							(1711, 1, 0x0B, 0x01, 1, 0x01, '3BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7'), -- delete
							(1711, 2, 0x0D, 0x04, 1, 0x01, 'CBFA90A3-C438-4033-8029-62AB947D5628');
						INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
							(0x0A, '2017-10-01'),
							(0x0B, '2017-11-01'),
							(0x0C, '2017-11-01'),
							(0x0D, '2017-11-01');
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'OrgCustomerAddress'
						UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'RefUNLOCOUtcOffset'
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'OrgCustomerAddress', 20, 1, 1710),
							(0x0B, 'dbo', 'OrgCustomerAddress', 20, 1, 1711),
							(0x0C, 'dbo', 'OrgCustomerAddress', 20, 1, 1711),
							(0x0D, 'dbo', 'OrgCustomerAddress', 20, 1, 1711);
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'CrmOpportunityStageProgress', 20, 1, 1710),
							(0x0B, 'dbo', 'CrmOpportunityStageProgress', 20, 1, 1711),
							(0x0C, 'dbo', 'CrmOpportunityStageProgress', 20, 1, 1711),
							(0x0D, 'dbo', 'CrmOpportunityStageProgress', 20, 1, 1711);
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'NettingFXDeal', 20, 1, 1710),
							(0x0B, 'dbo', 'NettingFXDeal', 20, 1, 1711),
							(0x0C, 'dbo', 'NettingFXDeal', 20, 1, 1711),
							(0x0D, 'dbo', 'NettingFXDeal', 20, 1, 1711);
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'NettingOrgChargeCode', 20, 1, 1710),
							(0x0B, 'dbo', 'NettingOrgChargeCode', 20, 1, 1711),
							(0x0C, 'dbo', 'NettingOrgChargeCode', 20, 1, 1711),
							(0x0D, 'dbo', 'NettingOrgChargeCode', 20, 1, 1711);
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'NettingSystemChargeCode', 20, 1, 1710),
							(0x0B, 'dbo', 'NettingSystemChargeCode', 20, 1, 1711),
							(0x0C, 'dbo', 'NettingSystemChargeCode', 20, 1, 1711),
							(0x0D, 'dbo', 'NettingSystemChargeCode', 20, 1, 1711);
						INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
							(0x0A, 'dbo', 'RefUNLOCOUtcOffset', 20, 1, 1710),
							(0x0B, 'dbo', 'RefUNLOCOUtcOffset', 20, 1, 1711),
							(0x0C, 'dbo', 'RefUNLOCOUtcOffset', 20, 1, 1711),
							(0x0D, 'dbo', 'RefUNLOCOUtcOffset', 20, 1, 1711);";

				auditConnection.ExecuteNonQuery(sqlText);

				SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
					temporaryMaxLsn: "0x0D",
					temporaryMaxPeriod: "2017-11-01",
					expectedLogs: new string[] {
							"[dbo].[OrgCustomerAddress] - (2B64CB57-8A41-4808-BF4B-D736CB71C3B2, 376cc518-06f7-4cc5-9928-3418cd83a10f)",
							"[dbo].[CrmOpportunityStageProgress] - (7921ad4a-5e26-4f3f-b603-51600631af60, c574562b-c2d1-4218-8865-36c90d0fb231)",
							"[dbo].[NettingFXDeal] - (00203769-552b-48dd-b751-c2f6ac21fb69, b8db4b20-09dd-462b-9052-86731b9cccad)",
							"[dbo].[NettingOrgChargeCode] - (0891a6e9-82ff-4c8c-8a6a-889a93e74af1, 6b407f53-340c-4c8b-939a-2c8d4ecb7885)",
							"[dbo].[NettingSystemChargeCode] - (04bbf235-6943-4ea3-b1cf-d34130b454e3, 5d2ccc88-c20a-494b-ae05-4abd4e547ec8)",
							"[dbo].[RefUNLOCOUtcOffset] - (3BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7, CBFA90A3-C438-4033-8029-62AB947D5628)"
					}
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "0D000000000000000000");
			}
		}

		void SendLatestAuditChangesAndAssert(DbConnection auditConnection, IAuditSubscriber rawSubscriber, string temporaryMaxLsn, string temporaryMaxPeriod, string[] expectedLogs)
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
