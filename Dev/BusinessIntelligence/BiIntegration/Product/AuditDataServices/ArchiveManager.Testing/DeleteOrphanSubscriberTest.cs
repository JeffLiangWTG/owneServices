using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.Common.Testing;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Engine;
using Enterprise.AuditDataServices.ArchiveManager.Helpers;
using Enterprise.AuditDataServices.ArchiveManager.Subscribers;
using Enterprise.AuditDataServices.Notification;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.AuditDataServices.ArchiveManager.Testing
{
	public class TestAuditSubscriberProcessorTask : AuditSubscriberProcessorTask
	{
		public void RunMaintenanceTasksExposed()
		{
			base.RunMaintenanceTasks(AuditTestHelper.GetAuditConnection());
		}
	}

	[UseSnapshotProtection([DatabaseType.Main, DatabaseType.Audit], skipTransaction: true)]
	public class DeleteOrphanSubscriberTest : TransactionedTestCase
	{
		public void TestSendLatestChangesForStmALogNewerThan90DaysShouldDeleteLogs()
		{
			var sqlTextForInsertIntoJobDeclaration = $@"
				INSERT dbo.JobDeclaration ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JE_PK) VALUES
					(1711, 0x0B, 0x02, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2'); -- this is a delete
				INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
					(0x0B, '2017-11-01');
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobDeclaration'
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'StmALog'
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0B, 'dbo', 'JobDeclaration', 1, 1, 1711);
			";

			var sqlTextForInsertIntoStmALog = @"
				INSERT INTO StmaLog (SL_PK, SL_Parent, SL_Table, SL_EventTime, SL_SE_NKEvent, SL_PostedTimeUtc) VALUES
				('40BE7B0B-8612-4F66-9249-DC9F012C8D84', '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', 'JobDeclaration', DATEADD(day, -91, GETDATE()), 'XYZ', DATEADD(day, -91, GETDATE())),
				('0C88A9E2-5C75-4C97-B360-21EA2BBB8563', '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', 'JobDeclaration', DATEADD(day, -89, GETDATE()), 'XYZ', DATEADD(day, -89, GETDATE()));
			";

			SendLatestChangesTestHelper(
				sqlTextForInsertIntoJobDeclaration,
				sqlTextForInsertIntoStmALog,
				expectedMaxLSN: "0x0B",
				expectedMaxPeriod: "2017-11-01",
				expectedLogs: [
					"Deleted from: [StmaLog - ('40be7b0b-8612-4f66-9249-dc9f012c8d84','0C88A9E2-5C75-4C97-B360-21EA2BBB8563')]",
					"Nudging archive manager cleanup."
				],
				expectedHighWaterMark: "0B000000000000000000",
				targetList: [new Target("SL_Table", "SL_Parent", "StmaLog", "SL_PK", false)]
			);

			using (var cmd = Db.Connection.Command("select count(*) from dbo.StmALog where SL_PK in ('40BE7B0B-8612-4F66-9249-DC9F012C8D84', '0C88A9E2-5C75-4C97-B360-21EA2BBB8563')"))
			{
				AssertEquals("Both logs should have been deleted", 0, cmd.ExecuteScalar() as int?);
			}
		}

		public void TestSendLatestChangesForStmALogNewerThan90DaysButThereAreOnlyNewStmALogsShouldBeDeleted()
		{
			var sqlTextForInsertIntoJobDeclaration = $@"
				INSERT dbo.JobDeclaration ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JE_PK) VALUES
					(1711, 0x0B, 0x02, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2'); -- this is a delete
				INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
					(0x0B, '2017-11-01');
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobDeclaration'
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'StmNote'
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0B, 'dbo', 'JobDeclaration', 1, 1, 1711);
			";

			var sqlTextForInsertIntoStmNote = @"
				INSERT INTO dbo.StmaLog (SL_PK, SL_Parent, SL_Table, SL_EventTime, SL_SE_NKEvent, SL_PostedTimeUtc) VALUES
				('40BE7B0B-8612-4F66-9249-DC9F012C8D84', '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', 'JobDeclaration', DATEADD(day, -1, GETDATE()), 'XYZ', DATEADD(day, -1, GETDATE())),
				('0C88A9E2-5C75-4C97-B360-21EA2BBB8563', '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', 'JobDeclaration', DATEADD(day, -2, GETDATE()), 'XYZ', DATEADD(day, -2, GETDATE()));
			";

			SendLatestChangesTestHelper(
				sqlTextForInsertIntoJobDeclaration,
				sqlTextForInsertIntoStmNote,
				expectedMaxLSN: "0x0B",
				expectedMaxPeriod: "2017-11-01",
				expectedLogs: [
					"Deleted from: [StmaLog - ('0c88a9e2-5c75-4c97-b360-21ea2bbb8563','40be7b0b-8612-4f66-9249-dc9f012c8d84')]",
					"Nudging archive manager cleanup."
				],
				expectedHighWaterMark: "0B000000000000000000",
				targetList: [new Target("SL_Table", "SL_Parent", "StmaLog", "SL_PK", false)]
			);

			using (var cmd = Db.Connection.Command("select count(*) from dbo.StmALog where SL_PK in ('40BE7B0B-8612-4F66-9249-DC9F012C8D84', '0C88A9E2-5C75-4C97-B360-21EA2BBB8563')"))
			{
				AssertEquals("Both logs should have been deleted", 0, cmd.ExecuteScalar() as int?);
			}
		}

		public void TestSendLatestChangesWhenCusOutturnParentIsJobShipment()
		{
			var sqlTextToInsertIntoJobShipment = $@"
				INSERT dbo.JobShipment ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JS_PK) VALUES
					(1711, 0x0B, 0x01, 1, 0x01, 'CF23EF90-669E-471E-90E4-209718042529'); -- this is a delete
				INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
					(0x0B, '2017-11-01');
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobShipment'
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'CusOutturn'
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0B, 'dbo', 'JobShipment', 1, 1, 1711);
			";

			var sqlTextToInsertIntoOrphanAndItsChildren = @"
				DECLARE @cusOutturnHeaderPK uniqueidentifier = 'A2A3F225-8F76-4971-BE45-50137920D35A';
				DECLARE @jobShipmentPK uniqueidentifier = 'CF23EF90-669E-471E-90E4-209718042529';
				INSERT dbo.CusOutturnHeader (C6_PK) VALUES (@cusOutturnHeaderPK);
				INSERT dbo.CusOutturn (C5_PK, C5_C6, C5_ParentID, C5_ParentTableCode, C5_AcceptedQuantity, C5_ApplicationCode, C5_MessageType, C5_RejectedQuantity, C5_VolumeOutturned, C5_WeightOutturned) VALUES
					('40BE7B0B-8612-4F66-9249-DC9F012C8D84', @cusOutturnHeaderPK, @jobShipmentPK, 'JS', 0, '', '', 0, 0, 0);
				INSERT dbo.CusUnderbond (C4_PK, C4_C6, C4_ApplicationCode) VALUES
					('0C88A9E2-5C75-4C97-B360-21EA2BBB8563', @cusOutturnHeaderPK, 'AUU');
			";

			SendLatestChangesTestHelper(
				sqlTextToInsertIntoJobShipment,
				sqlTextToInsertIntoOrphanAndItsChildren,
				expectedMaxLSN: "0x0B",
				expectedMaxPeriod: "2017-11-01",
				expectedLogs: new string[] {
					"Deleted from: [CusOutturn - ('40BE7B0B-8612-4F66-9249-DC9F012C8D84')] and related records from [CusOutturnHeader - ('A2A3F225-8F76-4971-BE45-50137920D35A')], " +
					"[CusUnderbond - ('0C88A9E2-5C75-4C97-B360-21EA2BBB8563')]",
					"Nudging archive manager cleanup."
				},
				expectedHighWaterMark: "0B000000000000000000",
				targetList: [new Target("C5_ParentTableCode", "C5_ParentID", "CusOutturn", "C5_PK", true)]
			);
		}

		public void TestSendLatestChangesWhenCusOutturnParentIsNotJobShipment()
		{
			var sqlTextToInsertIntoJobShipment = $@"
				INSERT dbo.JobShipment ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JS_PK) VALUES
					(1711, 0x0B, 0x01, 1, 0x01, 'CF23EF90-669E-471E-90E4-209718042529'); -- this is a delete
				INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
					(0x0B, '2017-11-01');
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobShipment'
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'CusOutturn'
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0B, 'dbo', 'JobShipment', 1, 1, 1711);
			";

			var sqlTextToInsertIntoOrphanAndItsChildren = @"
				DECLARE @cusOutturnHeaderPK uniqueidentifier = 'A2A3F225-8F76-4971-BE45-50137920D35A';
				INSERT dbo.CusOutturnHeader (C6_PK) VALUES (@cusOutturnHeaderPK);
				INSERT dbo.CusOutturn (C5_PK, C5_C6, C5_AcceptedQuantity, C5_ApplicationCode, C5_MessageType, C5_RejectedQuantity, C5_VolumeOutturned, C5_WeightOutturned) VALUES
					('40BE7B0B-8612-4F66-9249-DC9F012C8D84', @cusOutturnHeaderPK, 0, '', '', 0, 0, 0);
				INSERT dbo.CusUnderbond (C4_PK, C4_C6, C4_ApplicationCode) VALUES
					('0C88A9E2-5C75-4C97-B360-21EA2BBB8563', @cusOutturnHeaderPK, 'AUU');
			";

			SendLatestChangesTestHelper(
				sqlTextToInsertIntoJobShipment,
				sqlTextToInsertIntoOrphanAndItsChildren,
				expectedMaxLSN: "0x0B",
				expectedMaxPeriod: "2017-11-01",
				expectedLogs: Array.Empty<string>(),
				expectedHighWaterMark: "0B000000000000000000",
				targetList: [new Target("C5_ParentTableCode", "C5_ParentID", "CusOutturn", "C5_PK", true)]
			);
		}

		public void TestSendLatestChangesWhenCusOutturnHeaderHasOtherChildren()
		{
			var sqlTextToInsertIntoJobShipment = $@"
				INSERT dbo.JobShipment ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JS_PK) VALUES
					(1711, 0x0B, 0x01, 1, 0x01, 'CF23EF90-669E-471E-90E4-209718042529'); -- this is a delete
				INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
					(0x0B, '2017-11-01');
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobShipment'
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'CusOutturn'
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0B, 'dbo', 'JobShipment', 1, 1, 1711);
			";

			var sqlTextToInsertIntoOrphanAndItsChildren = @"
				DECLARE @cusOutturnHeaderPK uniqueidentifier = 'A2A3F225-8F76-4971-BE45-50137920D35A';
				DECLARE @jobShipmentPK uniqueidentifier = 'CF23EF90-669E-471E-90E4-209718042529';
				INSERT dbo.CusOutturnHeader (C6_PK) VALUES (@cusOutturnHeaderPK);
				INSERT dbo.CusOutturn (C5_PK, C5_C6, C5_ParentID, C5_ParentTableCode, C5_AcceptedQuantity, C5_ApplicationCode, C5_MessageType, C5_RejectedQuantity, C5_VolumeOutturned, C5_WeightOutturned) VALUES
					('40BE7B0B-8612-4F66-9249-DC9F012C8D84', @cusOutturnHeaderPK, @jobShipmentPK, 'JS', 0, '', '', 0, 0, 0),
					('27E8104A-4AFE-4212-8E28-9CBFA9101C5B', @cusOutturnHeaderPK, NULL, '', 0, '', '', 0, 0, 0);
				INSERT dbo.CusUnderbond (C4_PK, C4_C6, C4_ApplicationCode) VALUES
					('0C88A9E2-5C75-4C97-B360-21EA2BBB8563', @cusOutturnHeaderPK, 'AUU');
			";

			SendLatestChangesTestHelper(
				sqlTextToInsertIntoJobShipment,
				sqlTextToInsertIntoOrphanAndItsChildren,
				expectedMaxLSN: "0x0B",
				expectedMaxPeriod: "2017-11-01",
				expectedLogs: new string[] {
					"Deleted from: [CusOutturn - ('40BE7B0B-8612-4F66-9249-DC9F012C8D84')] and related records from [CusOutturnHeader - ('A2A3F225-8F76-4971-BE45-50137920D35A')], " +
					"[CusOutturn - ('27E8104A-4AFE-4212-8E28-9CBFA9101C5B')], [CusUnderbond - ('0C88A9E2-5C75-4C97-B360-21EA2BBB8563')]",
					"Nudging archive manager cleanup."
				},
				expectedHighWaterMark: "0B000000000000000000",
				targetList: [new Target("C5_ParentTableCode", "C5_ParentID", "CusOutturn", "C5_PK", true) ]
			);
		}

		public void TestSendLatestChangesWhenCusUnderbondHasOtherChildren()
		{
			var sqlTextToInsertIntoJobShipment = $@"
				INSERT dbo.JobShipment ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JS_PK) VALUES
					(1711, 0x0B, 0x01, 1, 0x01, 'CF23EF90-669E-471E-90E4-209718042529'); -- this is a delete
				INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
					(0x0B, '2017-11-01');
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobShipment'
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'CusOutturn'
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0B, 'dbo', 'JobShipment', 1, 1, 1711);
			";

			var sqlTextToInsertIntoOrphanAndItsChildren = @"
				DECLARE @cusOutturnHeaderPK uniqueidentifier = 'A2A3F225-8F76-4971-BE45-50137920D35A';
				DECLARE @jobShipmentPK uniqueidentifier = 'CF23EF90-669E-471E-90E4-209718042529';
				INSERT dbo.CusOutturnHeader (C6_PK) VALUES (@cusOutturnHeaderPK);
				INSERT dbo.CusUnderbond (C4_PK, C4_C6, C4_ApplicationCode) VALUES
					('0C88A9E2-5C75-4C97-B360-21EA2BBB8563', @cusOutturnHeaderPK, 'AUU');
				INSERT dbo.CusOutturn (C5_PK, C5_C6, C5_ParentID, C5_ParentTableCode, C5_AcceptedQuantity, C5_ApplicationCode, C5_MessageType, C5_RejectedQuantity, C5_VolumeOutturned, C5_WeightOutturned, C5_C4_Underbond) VALUES
					('40BE7B0B-8612-4F66-9249-DC9F012C8D84', @cusOutturnHeaderPK, @jobShipmentPK, 'JS', 0, '', '', 0, 0, 0, NULL),
					('27E8104A-4AFE-4212-8E28-9CBFA9101C5B', NULL, NULL, '', 0, '', '', 0, 0, 0, '0C88A9E2-5C75-4C97-B360-21EA2BBB8563');
			";

			SendLatestChangesTestHelper(
				sqlTextToInsertIntoJobShipment,
				sqlTextToInsertIntoOrphanAndItsChildren,
				expectedMaxLSN: "0x0B",
				expectedMaxPeriod: "2017-11-01",
				expectedLogs: new string[] {
					"Deleted from: [CusOutturn - ('40BE7B0B-8612-4F66-9249-DC9F012C8D84')] and related records from [CusOutturnHeader - ('A2A3F225-8F76-4971-BE45-50137920D35A')], " +
					"[CusUnderbond - ('0C88A9E2-5C75-4C97-B360-21EA2BBB8563')], [CusOutturn - ('27E8104A-4AFE-4212-8E28-9CBFA9101C5B')]",
					"Nudging archive manager cleanup."
				},
				expectedHighWaterMark: "0B000000000000000000",
				targetList: [new Target("C5_ParentTableCode", "C5_ParentID", "CusOutturn", "C5_PK", true) ]
			);
		}

		public void TestIsRequired_MatchesRegistryValues()
		{
			var subscriber = new DeleteOrphanSubscriber();

			using (SystemDataRegistry.Instance.BiIsRequiredDeleteOrphanSubscriber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert("IsRequired when registry disabled:", !subscriber.IsRequired());
			}

			using (SystemDataRegistry.Instance.BiIsRequiredDeleteOrphanSubscriber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert("IsRequired when registry enabled:", subscriber.IsRequired());
			}
		}

		public void TestSendLatestChanges()
		{
			var sqlInsertParents = $@"
				INSERT dbo.JobDeclaration ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JE_PK) VALUES
					(1711, 0x0B, 0x02, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2'); -- this is a delete
				INSERT dbo.JobShipment ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JS_PK) VALUES
					(1711, 0x0B, 0x01, 1, 0x01, '3BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7'), -- Has the same Lsn and SeqVal as another record
					(1711, 0x0D, 0x04, 1, 0x01, 'CBFA90A3-C438-4033-8029-62AB947D5628');
				INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
					(0x0A, '2017-10-01'),
					(0x0B, '2017-11-01'),
					(0x0C, '2017-11-01'),
					(0x0D, '2017-11-01');
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobDeclaration'
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0D WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobShipment'
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0A, 'dbo', 'JobDeclaration', 20, 1, 1710),
					(0x0B, 'dbo', 'JobDeclaration', 20, 1, 1711),
					(0x0C, 'dbo', 'JobDeclaration', 20, 1, 1711),
					(0x0D, 'dbo', 'JobDeclaration', 20, 1, 1711);
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0A, 'dbo', 'JobShipment', 20, 1, 1710),
					(0x0B, 'dbo', 'JobShipment', 20, 1, 1711),
					(0x0C, 'dbo', 'JobShipment', 20, 1, 1711),
					(0x0D, 'dbo', 'JobShipment', 20, 1, 1711);
			";

			var sqlInsertOrphans = @"
				DECLARE @EDICompanyPk UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'  
				INSERT INTO dbo.JobService([ES_PK], [ES_IsValid], [ES_ServiceCode], [ES_BookedDateTimeOffset], [ES_Duration], [ES_ServiceCount], [ES_CompletedDateTimeOffset], [ES_References], [ES_OH_Contractor], [ES_OA_Location], [ES_RQ], [ES_GC], [ES_ParentID], [ES_ParentTableCode], [ES_CurrentContextID], [ES_CurrentContextTableCode], [ES_MeasurementBasis], [ES_RX_NKServiceRateCurrency], [ES_ServiceRate],[ES_SubLocation], [ES_ServiceNote], [ES_AutoVersion], [ES_SystemCreateTimeUtc], [ES_SystemCreateUser], [ES_SystemLastEditTimeUtc], [ES_SystemLastEditUser])
				VALUES('C6D2D1A1-B50C-4F18-AC2A-4FC06582499C', 1, 'TTT', NULL, NULL, 0.0, NULL, '', NULL, NULL, NULL, @EDICompanyPk, 'CBFA90A3-C438-4033-8029-62AB947D5628', 'JS', NULL, '', 'AAA', 'AAA', 1.0, '', '', 0, '2023-04-20 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST')
				INSERT INTO dbo.JobService([ES_PK], [ES_IsValid], [ES_ServiceCode], [ES_BookedDateTimeOffset], [ES_Duration], [ES_ServiceCount], [ES_CompletedDateTimeOffset], [ES_References], [ES_OH_Contractor], [ES_OA_Location], [ES_RQ], [ES_GC], [ES_ParentID], [ES_ParentTableCode], [ES_CurrentContextID], [ES_CurrentContextTableCode], [ES_MeasurementBasis], [ES_RX_NKServiceRateCurrency], [ES_ServiceRate],[ES_SubLocation], [ES_ServiceNote], [ES_AutoVersion], [ES_SystemCreateTimeUtc], [ES_SystemCreateUser], [ES_SystemLastEditTimeUtc], [ES_SystemLastEditUser])
				VALUES('D1A0B099-18CA-43FD-8058-848385838872', 1, 'TTT', NULL, NULL, 0.0, NULL, '', NULL, NULL, NULL, @EDICompanyPk, '3BA7E6EC-B0D2-44CD-AB96-FF1DF1FC93F7', 'JS', NULL, '', 'AAA', 'AAA', 1.0, '', '', 0, '2023-04-20 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST')
				INSERT INTO dbo.JobService([ES_PK], [ES_IsValid], [ES_ServiceCode], [ES_BookedDateTimeOffset], [ES_Duration], [ES_ServiceCount], [ES_CompletedDateTimeOffset], [ES_References], [ES_OH_Contractor], [ES_OA_Location], [ES_RQ], [ES_GC], [ES_ParentID], [ES_ParentTableCode], [ES_CurrentContextID], [ES_CurrentContextTableCode], [ES_MeasurementBasis], [ES_RX_NKServiceRateCurrency], [ES_ServiceRate],[ES_SubLocation], [ES_ServiceNote], [ES_AutoVersion], [ES_SystemCreateTimeUtc], [ES_SystemCreateUser], [ES_SystemLastEditTimeUtc], [ES_SystemLastEditUser])
				VALUES('0656422E-56A7-4C05-B813-AD3B782FD389', 1, 'TTT', NULL, NULL, 0.0, NULL, '', NULL, NULL, NULL, @EDICompanyPk, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', 'JE', NULL, '', 'AAA', 'AAA', 1.0, '', '', 0, '2023-04-20 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST')
			";

			SendLatestChangesTestHelper(
				sqlInsertParents,
				sqlInsertOrphans,
				expectedMaxLSN: "0x0D",
				expectedMaxPeriod: "2017-11-01",
				expectedLogs: new string[] {
					"Deleted from: [JobService - ('0656422E-56A7-4C05-B813-AD3B782FD389')]",
					"Deleted from: [JobService - ('c6d2d1a1-b50c-4f18-ac2a-4fc06582499c','d1a0b099-18ca-43fd-8058-848385838872')]",
					"Nudging archive manager cleanup."
				},
				expectedHighWaterMark: "0D000000000000000000",
				targetList: [new Target("ES_ParentTableCode", "ES_ParentID", "JobService", "ES_PK", true) ]
			);
		}

		public void TestSendLatestChangesWithSelfReferential()
		{
			var jobDeclarationPK = Guid.NewGuid();
			var sqlTextForInsertIntoJobDeclaration = $@"
				INSERT dbo.JobDeclaration ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JE_PK) VALUES
					(1711, 0x0B, 0x02, 1, 0x01, '{jobDeclarationPK}'); -- this is a delete
				INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
					(0x0B, '2017-11-01');
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobDeclaration';
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0B, 'dbo', 'JobDeclaration', 1, 1, 1711);
			";

			var factory = new BusinessObjectFactory();
			var testProcessHeader = factory.New<IProcessHeader>();
			var otherHeader = factory.New<IProcessHeader>();
			testProcessHeader.FH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			testProcessHeader.FH_ParentId = jobDeclarationPK;
			testProcessHeader.FH_WorkflowType = "WKI";
			otherHeader.FH_FH_ParentHeader = testProcessHeader.PK;
			otherHeader.FH_WorkflowType = "WKI";

			factory.Save();

			var newFactory = new BusinessObjectFactory();

			CombineAssertions("Precondition: all test records are present", () =>
			{
				AssertNotNull(newFactory.Load<IProcessHeader>(testProcessHeader.PK));
				AssertNotNull(newFactory.Load<IProcessHeader>(otherHeader.PK));
			});

			SendLatestChangesTestHelper(
				sqlTextForInsertIntoJobDeclaration,
				null,
				expectedMaxLSN: "0x0B",
				expectedMaxPeriod: "2017-11-01",
				expectedLogs: new string[] {
					$"Deleted from: [ProcessHeader - ('{testProcessHeader.PK}')] and related records from [ProcessHeader - ('{otherHeader.PK}')]",
					"Nudging archive manager cleanup."
				},
				expectedHighWaterMark: "0B000000000000000000",
				targetList: [new Target("FH_ParentTableCode", "FH_ParentID", "ProcessHeader", "FH_PK", true)]
			);

			newFactory = new BusinessObjectFactory();

			CombineAssertions("Precondition: all test records are deleted", () =>
			{
				AssertNull(newFactory.Load<IProcessHeader>(testProcessHeader.PK));
				AssertNull(newFactory.Load<IProcessHeader>(otherHeader.PK));
			});
		}

		public void TestSendLatestChangesWithGrandchildren()
		{
			var jobShipmentPK = Guid.NewGuid();
			var sqlTextForInsertIntoJobShipment = $@"
				INSERT dbo.JobShipment ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JS_PK) VALUES
					(1711, 0x0B, 0x02, 1, 0x01, '{jobShipmentPK}'); -- this is a delete
				INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
					(0x0B, '2017-11-01');
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobShipment';
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0B, 'dbo', 'JobShipment', 1, 1, 1711);
			";

			var factory = new BusinessObjectFactory();

			var childOfOrphan = factory.NewWithValidTestData<PkgPackage>();
			var testOrphan = factory.Load<PkgPackageJob>(childOfOrphan.KP_KJ_ParentPackageJob);
			testOrphan.KJ_ParentID = jobShipmentPK;
			testOrphan.KJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var grandchildOfOrphan = factory.NewWithValidTestData<PkgPackageContainer>();
			grandchildOfOrphan.K0_KP_Package = childOfOrphan.PK;

			factory.Save();

			var newFactory = new BusinessObjectFactory();

			CombineAssertions("Precondition: all related test records are present", () =>
			{
				AssertNotNull(newFactory.Load<PkgPackageJob>(testOrphan.PK));
				AssertNotNull(newFactory.Load<PkgPackage>(childOfOrphan.PK));
				AssertNotNull(newFactory.Load<PkgPackageContainer>(grandchildOfOrphan.PK));
			});

			SendLatestChangesTestHelper(
				sqlTextForInsertIntoJobShipment,
				null,
				expectedMaxLSN: "0x0B",
				expectedMaxPeriod: "2017-11-01",
				expectedLogs: new string[] {
					$"Deleted from: [PkgPackageJob - ('{testOrphan.PK}')] and related records from [PkgPackage - ('{childOfOrphan.PK}')], " +
					$"[PkgPackageContainer - ('{grandchildOfOrphan.PK}')]",
					"Nudging archive manager cleanup."
				},
				expectedHighWaterMark: "0B000000000000000000",
				targetList: [new Target("KJ_ParentTableCode", "KJ_ParentID", "PkgPackageJob", "KJ_PK", true)]
			);

			newFactory = new BusinessObjectFactory();

			CombineAssertions("All related test records are deleted", () =>
			{
				AssertNull(newFactory.Load<PkgPackageJob>(testOrphan.PK));
				AssertNull(newFactory.Load<PkgPackage>(childOfOrphan.PK));
				AssertNull(newFactory.Load<PkgPackageContainer>(grandchildOfOrphan.PK));
			});
		}

		public void TestGetAllChanges()
		{
			var sqlInsertParents = $@"
				DECLARE @iter int=0;

				WHILE @iter<10000
				BEGIN
					INSERT dbo.JobDeclaration ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JE_PK) VALUES
						(1711, convert(binary(10), @iter), 0x02, 2, 0x01, newid()); -- these are inserts
					INSERT biadmin.LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (convert(binary(10), @iter), '2017-10-01');
					SET @iter=@iter+1;
				END
				
				INSERT dbo.JobDeclaration ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JE_PK) VALUES
					(1711, 0x0E, 0x02, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2'); -- this is a delete

				INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
					(0x0E, '2017-11-02');
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0E WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobDeclaration'
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0E, 'dbo', 'JobDeclaration', 20, 1, 1711);
			";

			var sqlInsertOrphans = @"
				DECLARE @EDICompanyPk UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				INSERT INTO dbo.JobService([ES_PK], [ES_IsValid], [ES_ServiceCode], [ES_BookedDateTimeOffset], [ES_Duration], [ES_ServiceCount], [ES_CompletedDateTimeOffset], [ES_References], [ES_OH_Contractor], [ES_OA_Location], [ES_RQ], [ES_GC], [ES_ParentID], [ES_ParentTableCode], [ES_CurrentContextID], [ES_CurrentContextTableCode], [ES_MeasurementBasis], [ES_RX_NKServiceRateCurrency], [ES_ServiceRate],[ES_SubLocation], [ES_ServiceNote], [ES_AutoVersion], [ES_SystemCreateTimeUtc], [ES_SystemCreateUser], [ES_SystemLastEditTimeUtc], [ES_SystemLastEditUser])
				VALUES('0656422E-56A7-4C05-B813-AD3B782FD389', 1, 'TTT', NULL, NULL, 0.0, NULL, '', NULL, NULL, NULL, @EDICompanyPk, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', 'JE', NULL, '', 'AAA', 'AAA', 1.0, '', '', 0, '2023-04-20 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST')
			";

			SendLatestChangesTestHelper(
				sqlInsertParents,
				sqlInsertOrphans,
				expectedMaxLSN: "0x0E",
				expectedMaxPeriod: "2017-11-02",
				expectedLogs: new string[] {
					"Deleted from: [JobService - ('0656422E-56A7-4C05-B813-AD3B782FD389')]",
					"Nudging archive manager cleanup."
				},
				expectedHighWaterMark: "0E000000000000000000",
				targetList: [new Target("ES_ParentTableCode", "ES_ParentID", "JobService", "ES_PK", true)]
			);
		}

		public void TestSendLatestChangesForTableWithFullParentName()
		{
			var sqlTextForInsertIntoJobDeclaration = $@"
				INSERT dbo.JobDeclaration ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JE_PK) VALUES
					(1711, 0x0B, 0x02, 1, 0x01, '2B64CB57-8A41-4808-BF4B-D736CB71C3B2'); -- this is a delete
				INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
					(0x0B, '2017-11-01');
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobDeclaration'
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0B, 'dbo', 'JobDeclaration', 1, 1, 1711);
			";

			var sqlTextForInsertIntoStmNote = @"
				INSERT INTO dbo.StmNote (ST_PK, ST_Table, ST_ParentID, ST_IsCustomDescription, ST_NoteType, ST_Description, ST_SystemCreateTimeUtc, ST_SystemCreateUser, ST_SystemLastEditTimeUtc, ST_SystemLastEditUser) VALUES
				('C85CD6DB-D254-4E9A-99F3-F2464A3A5EBF', 'JobDeclaration', '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', 0, 'PRV', 'Test Note', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT INTO dbo.StmaLog (SL_PK, SL_Parent, SL_Table, SL_EventTime, SL_SE_NKEvent, SL_PostedTimeUtc)
				values ('40BE7B0B-8612-4F66-9249-DC9F012C8D84', '2B64CB57-8A41-4808-BF4B-D736CB71C3B2', 'JobDeclaration', DATEADD(year, -1, GETDATE()), 'XYZ', DATEADD(year, -1, GETDATE()));
			";

			SendLatestChangesTestHelper(
				sqlTextForInsertIntoJobDeclaration,
				sqlTextForInsertIntoStmNote,
				expectedMaxLSN: "0x0B",
				expectedMaxPeriod: "2017-11-01",
				expectedLogs: new string[] {
					"Deleted from: [StmNote - ('C85CD6DB-D254-4E9A-99F3-F2464A3A5EBF')]",
					"Deleted from: [StmALog - ('40BE7B0B-8612-4F66-9249-DC9F012C8D84')]",
					"Nudging archive manager cleanup."
				},
				expectedHighWaterMark: "0B000000000000000000",
				targetList:
				[
					new Target("ST_Table", "ST_ParentID", "StmNote", "ST_PK", false),
					new Target("SL_Table", "SL_Parent", "StmaLog", "SL_PK", false),
				]
			);
		}

		public void TestSendLatestChangesForProcessTask()
		{
			var sqlTextForInsertIntoJobDeclaration = $@"
			INSERT dbo.JobDeclaration ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JE_PK) VALUES
				(1711, 0x0B, 0x02, 1, 0x01, '5907C0BD-3EE5-481F-B3E4-99299BB982DE');
			INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
				(0x0B, '2017-11-01');
			UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobDeclaration'
			UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'ProcessTasks'
			INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
				(0x0B, 'dbo', 'JobDeclaration', 1, 1, 1711);
			";

			var sqlTextForInsertIntoProcessTasks = @"
				INSERT INTO ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Notes, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser) values
				('1A83C3FD-E5E0-4E34-809F-7828CB311DEF', '5907C0BD-3EE5-481F-B3E4-99299BB982DE', 'JE', 0x0123456789, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			SendLatestChangesTestHelper(
				sqlTextForInsertIntoJobDeclaration,
				sqlTextForInsertIntoProcessTasks,
				expectedMaxLSN: "0x0B",
				expectedMaxPeriod: "2017-11-01",
				expectedLogs: new string[] {
					"Deleted from: [ProcessTasks - ('1A83C3FD-E5E0-4E34-809F-7828CB311DEF')]",
					"Nudging archive manager cleanup."
				},
				expectedHighWaterMark: "0B000000000000000000",
				targetList: [new Target("P9_ParentTableCode", "P9_ParentID", "ProcessTasks", "P9_PK", true)]
			);
		}

		public void TestNoRestrictedOrphansAreImproperlyClassified()
		{
			var testSubscriber = new DeleteOrphanSubscriber();
			var queryToCheckAllUnrestrictedOrphansAreActuallyUnrestricted = $@"SELECT COUNT(*) FROM sys.columns c
JOIN sys.tables t ON t.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
LEFT JOIN sys.check_constraints cc ON cc.parent_object_id = c.object_id AND cc.parent_column_id = c.column_id
WHERE c.NAME IN ({string.Join(",", testSubscriber.UnrestrictedTargets.Select(t => "'" + t.ParentTableColumn + "'").ToArray())})
AND definition LIKE '([[]' + c.Name + ']=''%'
AND s.name NOT IN ('sys', 'cdc')
AND definition IS NOT NULL
";
			using (var cmd = Db.Connection.Command(queryToCheckAllUnrestrictedOrphansAreActuallyUnrestricted))
			{
				AssertEquals("Number of restricted orphans in the unrestricted orphan list should be 0", 0, cmd.ExecuteScalar());
			}
		}

		Dictionary<Target, List<string>> GetMappingOfTargetsToAllowedParents(DeleteOrphanSubscriber testSubscriber)
		{
			var tablesAndTheirAllowedOrphans = testSubscriber.ValidOrphanTablesByTablePrefix;
			var targetsAndTheirAllowedParents = new Dictionary<Target, List<string>>();

			foreach (var key in tablesAndTheirAllowedOrphans.Keys)
			{
				foreach (var target in tablesAndTheirAllowedOrphans[key])
				{
					if (!targetsAndTheirAllowedParents.ContainsKey(target))
					{
						targetsAndTheirAllowedParents.Add(target, new List<string>());
					}

					if (target.ParentTableColumn == "EM_LinkTable"
						|| target.ParentTableColumn.EndsWith("_Table")
						|| target.ParentTableColumn.EndsWith("_ParentTable")
						|| target.ParentTableColumn.EndsWith("_ParentTableName"))
					{
						var tableName = Db.Connection.Command($@"SELECT top 1 t.name FROM sys.columns c
JOIN sys.tables t ON t.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE c.NAME = '{key}_PK' AND s.name NOT IN ('sys', 'cdc')").ExecuteScalar();
						targetsAndTheirAllowedParents[target].Add(tableName as string);
					}
					else
					{
						targetsAndTheirAllowedParents[target].Add(key);
					}
				}
			}

			return targetsAndTheirAllowedParents;
		}

		public void TestAllOrphansAreRestrictedOrUnrestricted()
		{
			var testSubscriber = new DeleteOrphanSubscriber();
			var targetsAndTheirAllowedTables = GetMappingOfTargetsToAllowedParents(testSubscriber);

			var neitherRestrictedOrUnrestrictedTargets = testSubscriber.TargetList.Where(t =>
	!targetsAndTheirAllowedTables.ContainsKey(t) && !testSubscriber.UnrestrictedTargets.Contains(t)).ToArray();
			AssertEquals($"There should be no targets that are neither restricted nor unrestricted" +
				$"", 0, neitherRestrictedOrUnrestrictedTargets.Length);
		}

		public void TestSubscriberCorrectlyReadsDatabaseConstraints()
		{
			var testSubscriber = new DeleteOrphanSubscriber();
			var targets = testSubscriber.TargetList;
			var targetsAndTheirAllowedTables = GetMappingOfTargetsToAllowedParents(testSubscriber);
			var mapTargetColumnToParentList = new Dictionary<string, List<string>>();
			targetsAndTheirAllowedTables.Keys.ForEach(k => mapTargetColumnToParentList[k.ParentTableColumn] = targetsAndTheirAllowedTables[k]);

			var sqlToGetAllConstraintDefinitionsForTargets = @"SELECT c.Name as columnName, definition as ConstraintDefinition FROM sys.columns c
JOIN sys.tables t ON t.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
LEFT JOIN sys.check_constraints cc ON cc.parent_object_id = c.object_id AND cc.parent_column_id = c.column_id
WHERE definition LIKE '([[]' + c.Name + ']=''%'
AND s.name NOT IN ('sys', 'cdc')
AND c.name IN (SELECT value FROM @tvp)
AND definition IS NOT NULL";
			using var cmd = Db.Connection.Command(sqlToGetAllConstraintDefinitionsForTargets);
			cmd.AddTableValuedParameter("@tvp", TVPHelper.TVP_varchar, targets.Select(t => t.ParentTableColumn));
			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				var constraintDefinition = reader["ConstraintDefinition"] as string;
				var parentTableColumn = reader["columnName"] as string;
				AssertEquals($"The number of allowed parents should be equal in the DB and the subscriber, but was not for column {parentTableColumn}",
					Regex.Matches(constraintDefinition, " OR ").Count - Regex.Matches(constraintDefinition, "]=''").Count + 1,
					mapTargetColumnToParentList[parentTableColumn].Count);
			}
		}

		public void TestInitialiseStructuresUsingConstraints_WhenTargetsThatReferToParentByFullTableNameAreHandledCorrectly()
		{
			var testSubscriber = new DeleteOrphanSubscriber();
			var targetsThatReferToParentByFullTableName = testSubscriber.TargetList.Where(t => !t.ParentTableColumnStoresCode);

			Assert("Precondition: There should be at least one target that refers to its parent table by its full name.", !targetsThatReferToParentByFullTableName.IsNullOrEmpty());

			var sqlForCreatingTestConstraintsBuilder = new StringBuilder();

			foreach (var target in targetsThatReferToParentByFullTableName)
			{
				_ = sqlForCreatingTestConstraintsBuilder.AppendLine($"ALTER TABLE [{target.TableName}] WITH NOCHECK ADD CONSTRAINT" +
					$"[TestConstraint_{target.ParentTableColumn}] CHECK ({target.ParentTableColumn} in ('JobHeader', 'JobShipment'));");
			}

			using (var command = Db.Connection.Command(sqlForCreatingTestConstraintsBuilder.ToString()))
			{
				_ = command.ExecuteNonQuery();

				var validOrphanTablesByTablePrefix = testSubscriber.ValidOrphanTablesByTablePrefix;
				var jobHeaderAllowedTargets = validOrphanTablesByTablePrefix[JobHeaderSchema.Constants.Prefix];
				var jobShipmentAllowedTargets = validOrphanTablesByTablePrefix[JobShipmentSchema.Constants.Prefix];

				CombineAssertions(() =>
				{
					Assert("targetsThatReferToParentByFullTableName should be a subset of jobHeaderAllowedTargets", !targetsThatReferToParentByFullTableName.Except(jobHeaderAllowedTargets).Any());
					Assert("targetsThatReferToParentByFullTableName should be a subset of jobShipmentAllowedTargets", !targetsThatReferToParentByFullTableName.Except(jobShipmentAllowedTargets).Any());
				});
			}
		}

		public void TestAllOrphansAreIncludedInDOPSubscriber()
		{
			var allParentTableColumnsAndTablesToBeCovered = new List<(string columnName, string tableName)>();
			using (var reader = Db.Connection.Command(DeleteOrphanSubscriber.SQLForGettingAllParentTableColumns).ExecuteReader())
			{
				while (reader.Read())
				{
					allParentTableColumnsAndTablesToBeCovered.Add((reader["column_name"] as string, reader["table_name"] as string));
				}
			}

			var resolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			var testsubscriber = new DeleteOrphanSubscriber();

			AssertContainsExactElementsInAnyOrder(
				allParentTableColumnsAndTablesToBeCovered.Where(c => resolver.GetTableSchema(c.tableName) != null).Select(c => c.columnName),
				testsubscriber.TargetList.Select(t => t.ParentTableColumn));
		}

		public void TestPerformanceOfDeleteOrphanSubscriberDeleteAHugeNumberOfRowsButOnlyFromASingleParentOrphanCombination()
		{
			var numberOfBatches = 20;
			var childrenPKs = new List<Guid>();
			var insertIntoChildrenSubStatements = new List<string>();
			var factory = new BusinessObjectFactory();
			var newCompanyToUse = factory.LoadTop1<GlbCompany>(new ZQuery());

			var parentGuid = Guid.NewGuid();
			for (var j = 0; j < numberOfBatches; j++)
			{
				var insertIntoChildSubStatement = new StringBuilder().AppendLine("INSERT INTO JobService([ES_PK], [ES_IsValid], [ES_ServiceCode], [ES_BookedDateTimeOffset], [ES_Duration], [ES_ServiceCount], [ES_CompletedDateTimeOffset], [ES_References], [ES_OH_Contractor], [ES_OA_Location], [ES_RQ], [ES_GC], [ES_ParentID], [ES_ParentTableCode], [ES_CurrentContextID], [ES_CurrentContextTableCode], [ES_MeasurementBasis], [ES_RX_NKServiceRateCurrency], [ES_ServiceRate],[ES_SubLocation], [ES_ServiceNote], [ES_AutoVersion], [ES_SystemCreateTimeUtc], [ES_SystemCreateUser], [ES_SystemLastEditTimeUtc], [ES_SystemLastEditUser]) VALUES");
				var insertIntoChildrenValues = new List<string>();

				for (var i = 0; i < 200; i++)
				{
					var childGuid = Guid.NewGuid();
					var insertIntoChildValue = $@"('{childGuid}', 1, 'TTT', NULL, NULL, 0.0, NULL, '', NULL, NULL, NULL, '{newCompanyToUse.PK}', '{parentGuid}', 'JE', NULL, '', 'AAA', 'AAA', 1.0, '', '', 0, '2023-04-20 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST')";
					insertIntoChildrenValues.Add(insertIntoChildValue);
					childrenPKs.Add(childGuid);
				}

				insertIntoChildrenSubStatements.Add(insertIntoChildSubStatement.AppendLine(string.Join(",\n", insertIntoChildrenValues)).Append(";").ToString());
			}

			var insertIntoParentOverallStatement = new StringBuilder().
				AppendLine($"INSERT dbo.JobDeclaration ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JE_PK) VALUES (1711, 0x0B, 0x02, 1, 0x01, '{parentGuid}');").
				AppendLine($@"INSERT[{BiConstants.BiAdminSchemaName}].LsnTimeMapping(StartLsn, TranEndTimeUtc) VALUES (0x0B, '2017-11-01');").
				AppendLine($@"UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobDeclaration'").
				Append($@"INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES (0x0B, 'dbo', 'JobDeclaration', 1, 1, 1711);").
				ToString();
			var insertIntoChildrenOverallStatement = string.Join("\n", insertIntoChildrenSubStatements);
			SendLatestChangesTestHelper(insertIntoParentOverallStatement,
				insertIntoChildrenOverallStatement,
				expectedMaxLSN: "0x0B",
				expectedMaxPeriod: "2017-11-01",
				expectedLogs: new string[] { $"Deleted from: [JobService - ({string.Join(",", childrenPKs.Select(pk => "'" + pk.ToString() + "'"))})]",
					"Nudging archive manager cleanup."
				},
				expectedHighWaterMark: "0B000000000000000000",
				targetList: [new Target("ES_ParentTableCode", "ES_ParentID", "JobService", "ES_PK", true)]
			);
		}

		public void TestRefTablesAreNotIncludedInTargets()
		{
			var testSubscriber = new DeleteOrphanSubscriber();
			Assert("Reference data tables are not included in orphan deletion",
				!testSubscriber.TargetList.Any(target => target.TableName.StartsWith("Ref")));
		}

		public void TestExcludedTablesAreCorrect()
		{
			var testSubscriber = new DeleteOrphanSubscriber();
			var excludedTables = new List<string> { "AccCommissionHeader", "AccCurrencyAdjustmentQueue", "AccTaxConfiguration",
				"AccTransactionComplianceReportQueue", "AccTransactionPostingToGLDQueue", "AsycudaManifestHeader", "BarcodeRuleSet",
				"BMNCNShape", "CusAuthorizationUsage", "CusEntryCPDec", "CusExitHeader", "CusInBondHeader",
				"CusPollingTransaction", "CusSCAOceanBill", "CusUnderbond", "DtbBookingConsolidation", "EDIMessage", "GenCustomColumnDefinition",
				"GenRegCertAccredMaintList", "GlbHoliday", "GlbPasswordHistory", "GlbPersonPrimaryRelationship", "GlbStaffHoliday",
				"GlbWorkTime", "GteGateMovementBooking", "JobCartage", "JobChargePostingQueue", "JobChargeTarget", "JobComInvoiceLine",
				"JobDocumentDelivery", "JobHeader", "JobMawb", "JobScheduleChange", "JPAFRHeader", "LandedCostHistory", "OrgSales",
				"P4PlanLineItem", "ProcessQueue", "ProcessTaskNotification", "RateEntry", "RateLines", "RelatedActivityPivot", "StmAccessToken",
				"StmActivityLog", "StmALogQueue", "StmALogQueueWTE", "StmJobQueue", "StmLoginFailureLog", "StmMenuItem", "StmModuleFilter",
				"StmModuleFilterUserData", "StmNumberRangeMatchingDetail", "StmPrintJob", "StmProcessQueue", "StmQueueState", "StmScheduleTask",
				"StmServiceHeartBeat", "StmServiceMutex", "StmUniversalCopy", "StorageDocs", "VoteExamSurveyQuestion", "WhsItemDispatchConsignment",
				"WhsItemDispatchLoadList", "WhsItemReceiveASN", "WhsItemReceiveConsignment"
			};
			var excludedColumns = new List<string> { "ES_CurrentContextTableCode", "P9_ReferencedTableCode", "PJ_SourceTableCode" };

			CombineAssertions("Precondition: sanity check for uniqueness", () =>
			{
				AssertEquals(excludedTables.Count, excludedTables.ToHashSet().Count);
				AssertEquals(excludedColumns.Count, excludedColumns.ToHashSet().Count);
			});

			var targetListContainsExcludedTables = testSubscriber.TargetList.Any(target => excludedTables.Contains(target.TableName));
			var targetListContainsExcludedColumns = testSubscriber.TargetList.Any(target => excludedColumns.Contains(target.ParentTableColumn));

			Assert($"These tables should not be included: ({GetAllowedTableNamesString(testSubscriber, excludedTables)})", !targetListContainsExcludedTables);
			Assert($"These columns should not be included: ({GetAllowedColumnNamesString(testSubscriber, excludedColumns)})", !targetListContainsExcludedColumns);
		}

		string GetAllowedTableNamesString(DeleteOrphanSubscriber deleteOrphanSubscriber, List<string> excludedTables)
			=> string.Join(", ", deleteOrphanSubscriber.TargetList.Where(target => excludedTables.Contains(target.TableName)).Select(t => t.TableName));

		string GetAllowedColumnNamesString(DeleteOrphanSubscriber deleteOrphanSubscriber, List<string> excludedColumns)
			=> string.Join(", ", deleteOrphanSubscriber.TargetList.Where(target => excludedColumns.Contains(target.ParentTableColumn)).Select(t => t.ParentTableColumn));

		public void TestAllowedTablesAreCorrect()
		{
			var testSubscriber = new DeleteOrphanSubscriber();
			var allowedTables = new List<string> { "AccBillingHeader", "AccBillingItem", "AccChargeSupplyTypeOverride",
				"AccChargeTaxOverride", "AccCommissionLine", "AccComplianceReportTransactionPivot", "AccDraftInvoiceJob", "AccEInvoicingTransactionPivot",
				"AccGroupMemberPivot", "AccJobConfig", "AccJobConfigPivot", "AccTransactionHeaderAuthorisationRecord",
				"AccTransactionHeaderSubAccount", "AccTransactionLineSubAccount", "Alert", "BMBoardSectionChannel", "BMControlCustomisationLink",
				"BMNCNChannel", "BPMConfigurationTmpl", "CertificateOfOrigin", "ComplianceRiskStatus", "CusAddInfo", "CusBondDetail",
				"CusCAClassification", "CusCAeMHHouse", "CusCAeMHMaster", "CusCALPCO", "CusCodeData", "CusDisposition", "CusEngine",
				"CusEntryNum", "CusExitControlHeader", "CusGoodsLocation", "CusInBondCargoDesc", "CusInBondContainer", "CusInvPack",
				"CusLineTariffDetail", "CusPerson", "CusReference", "CusSeal", "CusStorageDocPivot", "CusSupportingInfo", "CusTransportMeans",
				"CusUSClassification", "CusVehicle", "DtbBookingQueue", "DtbConsignmentVariation", "DtbEquipmentItem", "EDICommunicationsMode",
				"EUMemberStateCommunication", "ExportAWBHeader",
				"ExternalEntityLink", "ExternalRequest", "GlobalCommercialInvoiceHeader", "GateBookingDetail", "GenAddOnColumn", "GenApprovalRequest", "GenCustomAddOnRuleAck",
				"GenCustomAddOnValue", "GenExportBatchSequence", "GenPivot", "GenShapeGeography", "GenSpatialData",
				"GlbAccreditationJobSkillGroup", "GlbCompanyCampaignItem", "GlbDeviceAssignmentDivot","GlbPersonLanguage", "JobCO2e", "JobComInvHeaderCharge",
				"JobConsolCost", "JobConversation", "JobConversationParticipant", "JobDocAddress", "JobDocsAndCartage", "JobDocumentData",
				"JobDocumentExclusion", "JobEquipmentItem", "JobOrderContainer", "JobRatingPreference", "JobRelatedWayBill", "JobRequiredDocument", "JobService",
				"JobServiceLink", "JobSlotAllocation", "LandCostInput", "LandedCostHeader", "MNRSurvey", "MNRWorkOrderHeader", "OrgCommissionAgreementItem",
				"OrgMatchApproval", "OrgPatternMatchAddress", "OrgSalesCallAdditionalAttendee", "OrgSalesValueAssociationPivot",
				"PatternMatchingAddress", "PatternMatchingDomain", "PatternMatchingEmail", "PatternMatchingName", "PatternMatchingPhone",
				"PatternMatchingRegCode", "PatternMatchingResult", "PkgPackageExtension", "PkgPackageItemDivot", "PkgPackageJob",
				"PkgPalletTransaction", "ProcessEstimateLog", "ProcessHeader", "ProcessJobTriggerLink", "ProcessTaskIterationLinkPivot",
				"ProcessTasks", "RatingContractNamedAccountPivot", "RatingDateConfig", "ReeferSetting", "StmALog", "StmChangeLog", "StmComplianceEvent",
				"StmDefaultPrinter", "StmDocDataOverride", "StmDocumentDelivery", "StmEntityScreeningLog", "StmNote", "StmUniversalJobLink",
				"TagLink", "TelEdge", "TimeActionSchedule", "UNDGDataItem", "UNDGSubstancePivot", "WhsBondedWarehouseAttribute",
				"WhsDocketJobPivot", "WhsItemConsignmentOrderReference", "WhsSerialNumberPivot", "JobAddressAdditionalInfo", "DashDocCoordinate", "DashDocument",
				"ContainerPenaltyDayExclusion", "ComplianceJobEntityCache" };

			var nonconventionalAllowedTables = new List<string> { "CusOutturn" };

			CombineAssertions("Precondition: sanity check for uniqueness", () =>
			{
				AssertEquals(allowedTables.Count, allowedTables.ToHashSet().Count);
				AssertEquals(nonconventionalAllowedTables.Count, nonconventionalAllowedTables.ToHashSet().Count);
			});

			var tableIncludedButNotInAllowList = testSubscriber.TargetList.Any(target => !allowedTables.Contains(target.TableName) && !nonconventionalAllowedTables.Contains(target.TableName));
			var assertMessage = @"DeleteOrphanSubscriber will automatically detect soft FK tables in the database and delete them upon parent deletion unless 
they are explicitly excluded. This test failure is happening because you have added a table which is not explicitly allowed
in the list above or explicitly forbidden in the exclusion list in DeleteOrphanSubscriber.SQLForGettingAllParentTableColumns.
This is a list of such tables causing this failure:";

			Assert($"{assertMessage} ({GetExcludedTableNamesString(testSubscriber, allowedTables, nonconventionalAllowedTables)})", !tableIncludedButNotInAllowList);
		}

		string GetExcludedTableNamesString(DeleteOrphanSubscriber deleteOrphanSubscriber, List<string> allowedTables, List<string> nonconventionalAllowedTables)
			=> string.Join(", ", deleteOrphanSubscriber.TargetList.Where(target => !allowedTables.Contains(target.TableName) && !nonconventionalAllowedTables.Contains(target.TableName)).Select(t => t.TableName));

		public void TestNonConventionalArchiveRelationshipsAreAdded()
		{
			var testSubscriber = new DeleteOrphanSubscriber();

			var archiveRelationships = testSubscriber.AddNonConventionalArchiveRelationships(new List<ArchiveableRelationship>());

			CombineAssertions("archiveRelationships should incldude these non-conventional relationships (child -> parent)", () =>
			{
				Assert("CusOutturn -> CusOutturnHeader but entry point is CusOutturn", archiveRelationships.Any(rel => rel.ParentName == CusOutturnSchema.Constants.TableName && rel.ChildName == CusOutturnHeaderSchema.Constants.TableName && rel.IsReversed));
				Assert("CusOutturn -> CusOutturnHeader", archiveRelationships.Any(rel => rel.ParentName == CusOutturnHeaderSchema.Constants.TableName && rel.ChildName == CusOutturnSchema.Constants.TableName && !rel.IsReversed));
				Assert("CusUnderbond -> CusOutturnHeader", archiveRelationships.Any(rel => rel.ParentName == CusOutturnHeaderSchema.Constants.TableName && rel.ChildName == CusUnderbondSchema.Constants.TableName && !rel.IsReversed));
				Assert("CusOutturn -> CusUnderbond", archiveRelationships.Any(rel => rel.ParentName == CusUnderbondSchema.Constants.TableName && rel.ChildName == CusOutturnSchema.Constants.TableName && !rel.IsReversed));
			});
		}

		public void TestPassTooManyChangedValuesToProcessChangesDoesNotResultInException()
		{
			var testSubscriber = new DeleteOrphanSubscriber();
			var guidsThatWePretendAreDeleted = new List<string>();
			var numberOfGuidsToGenerate = 40789;
			var parentTableSchema = new ChangedTableSchema("dbo", "JobDocsAndCartage", "JP_PK");

			for (var i = 0; i < numberOfGuidsToGenerate; i++)
			{
				guidsThatWePretendAreDeleted.Add(Guid.NewGuid().ToString());
			}

			var changedValues = new Dictionary<IChangedTableSchema, List<string>>();
			changedValues.Add(parentTableSchema, guidsThatWePretendAreDeleted);
			var factory = new BusinessObjectFactory();
			var childThatShouldGetDeleted = factory.NewWithValidTestData<JobService>();
			childThatShouldGetDeleted.ES_ParentID = new Guid(guidsThatWePretendAreDeleted[numberOfGuidsToGenerate / 2]);
			childThatShouldGetDeleted.ES_ParentTableCode = JobDocsAndCartageSchema.Constants.Prefix;

			var noteThatShouldGetDeleted = factory.NewWithValidTestData<StmNote>();
			noteThatShouldGetDeleted.ST_ParentID = new Guid(guidsThatWePretendAreDeleted[numberOfGuidsToGenerate - 1]);
			noteThatShouldGetDeleted.ST_Table = JobDocsAndCartageSchema.Constants.TableName;

			factory.Save();

			var logger = new LoggerForTest();

			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();

			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			{
				AssertNoExceptionThrown("Should not throw an exception even with such a huge number of guids",
				() => testSubscriber.ProcessChanges(logger, changedValues));
			}

			AssertNull("Child should be correctly deleted", new BusinessObjectFactory().LoadTop1<JobService>(new ZQuery()));
			AssertNull("Note should be correctly deleted",
				new BusinessObjectFactory().LoadTop1<StmNote>(new ZQuery(StmNoteSchema.PK, noteThatShouldGetDeleted.PK)));
		}

		public void TestDuplicatedTableNameInDifferentSchemaUsesDboTableForTarget()
		{
			ErrorReporter.Clear();
			var testSubscriber = new DeleteOrphanSubscriber();

			_ = TestConnection.ExecuteNonQuery(@"
				CREATE TABLE hrm.PkgPackageItemDivot
				(
					KI_PK UNIQUEIDENTIFIER NOT NULL,
					KI_TableCode varchar(3),
					KI_ParentGuid UNIQUEIDENTIFIER,
				);"
			);

			_ = TestConnection.ExecuteNonQuery(@"
				ALTER TABLE hrm.PkgPackageItemDivot
				ADD CONSTRAINT PK_RECORDID PRIMARY KEY NONCLUSTERED (KI_PK);"
			);

			var targetList = testSubscriber.AddTargets(out var invalidMatches).FindAll(target => target.TableName == "PkgPackageItemDivot");

			CombineAssertions("Check target found is correct", () =>
			{
				AssertEquals(targetList.Count, 1);
				AssertEquals("KI_PK", targetList[0].PKName);
				AssertEquals("KI_ParentTableCode", targetList[0].ParentTableColumn);
				AssertEquals("KI_ParentID", targetList[0].ParentIDColumn);
				AssertEquals(true, targetList[0].ParentTableColumnStoresCode);
			});

			var exceptions = ErrorReporter.LastExceptionsReported();
			Assert(exceptions.Exists(ex => ex.Contains("Encountered an exception while trying to add a target with DOP with current reader row")));
			ErrorReporter.Clear();

			Assert("The following pair of columns" +
						"Parent Table Column: KI_TableCode " +
						"Parent ID Column: KI_ParentID " +
						"should not have been added to invalidMatches, but they were", !invalidMatches.Any(im => im.Key == "KI_TableCode" && im.Value == "KI_ParentID"));
		}

		public void TestDuplicatedColumnNamePrefixUsesEnterpriseSchemaTableForTarget()
		{
			var testSubscriber = new DeleteOrphanSubscriber();
			_ = TestConnection.ExecuteNonQuery(@"
				CREATE TABLE dbo.Test
				(
					KI_PK UNIQUEIDENTIFIER NOT NULL,
					KI_ParentTableCode varchar(3),
					KI_ParentID UNIQUEIDENTIFIER,
				);"
			);

			_ = TestConnection.ExecuteNonQuery(@"
				ALTER TABLE dbo.Test
				ADD CONSTRAINT PK_RECORDID PRIMARY KEY NONCLUSTERED (KI_PK);"
			);
			var targetList = testSubscriber.TargetList.FindAll(target => target.PKName == "KI_PK");
			CombineAssertions("Check target found is correct", () =>
			{
				AssertEquals(targetList.Count, 1);
				AssertEquals("PkgPackageItemDivot", targetList[0].TableName);
				AssertEquals("KI_ParentTableCode", targetList[0].ParentTableColumn);
				AssertEquals("KI_ParentID", targetList[0].ParentIDColumn);
				AssertEquals(true, targetList[0].ParentTableColumnStoresCode);
			});
		}

		[UseSnapshotProtection]
		public void TestAddTargets_DoesNotAddInvalidMatches()
		{
			using (SchemaResolverHelper.SetupApplicationSchemaResolver())
			{
				var testSubscriber = new DeleteOrphanSubscriber();

				_ = TestConnection.ExecuteNonQuery(@"
				CREATE TABLE dbo.InvalidMatchTest
				(
					IMT_PK UNIQUEIDENTIFIER NOT NULL,
					IMT_ParentTableCode varchar(3),
					IMT_NonMatchingParentID UNIQUEIDENTIFIER,
				);"
				);

				_ = TestConnection.ExecuteNonQuery(@"
				ALTER TABLE dbo.InvalidMatchTest
				ADD CONSTRAINT PK_RECORDID PRIMARY KEY NONCLUSTERED (IMT_PK);"
				);

				var addedTargets = testSubscriber.AddTargets(out var invalidMatches);

				CombineAssertions(() =>
				{
					Assert("The TargetList should contain all other valid targets, but is null or empty", !addedTargets.IsNullOrEmpty());
					Assert("The invalid match should not have been added to the TargetList", !addedTargets.Any(target => target.TableName == InvalidMatchTestSchema.Constants.TableName));
					AssertEquals("The invalid match should have been correctly identified as invalid", 1, invalidMatches.Count);
					Assert("The invalid match should exist in invalidMatches", invalidMatches.Any(im => im.Key == "IMT_ParentTableCode" && im.Value == "IMT_NonMatchingParentID"));
				});
			}
		}

		[UseSnapshotProtection]
		public void TestInvalidMatchesAreNotFoundInDOTSubscriber()
		{
			var testSubscriber = new DeleteOrphanSubscriber();
			var addedTargets = testSubscriber.AddTargets(out var invalidMatches);

			var message = "The following pairs of Parent Table Columns and Parent ID Columns could not be matched. " +
			 "Please fix their naming conventions, or ensure they have been added to both the SQLForGettingAllParentTableColumns string " +
			 "and the list of accepted suffixes in TryToMapColumnToExplicitlyNamedParent.\n";

			CombineAssertions(() =>
			{
				AssertEquals(message + $"{string.Join("\n", invalidMatches.ToList().Select(kvp => $"Parent Table Column: {kvp.Key}. Parent ID Column: {kvp.Value}"))}", 0, invalidMatches.Count);
				Assert("The TargetList should contain all allowed tables, but is null or empty", !testSubscriber.TargetList.IsNullOrEmpty());
			});
		}

		static void SendLatestAuditChangesAndAssert(DbConnection auditConnection, IAuditSubscriber rawSubscriber, string temporaryMaxLsn, string temporaryMaxPeriod, string[] expectedLogs)
		{
			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();

			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
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

		static void AssertSubscriberHighWaterMark(DbConnection connection, string subscriberCode, string expected)
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

		internal static void CleanupDBOTableState(DbConnection auditConnection, string[] tablesToTruncate)
		{
			var truncateTestTablesBuilder = new StringBuilder();
			foreach (var table in tablesToTruncate)
			{
				truncateTestTablesBuilder.Append($"UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = NULL WHERE SourceSchemaName = 'dbo' AND SourceTableName = '{table}';");
			}

			auditConnection.ExecuteNonQuery(truncateTestTablesBuilder.ToString());
		}

		internal static void SendLatestChangesTestHelper(string sqlTextForInsertingIntoParents, string sqlTextForInsertingIntoOrphans, string expectedMaxLSN, string expectedMaxPeriod, string[] expectedLogs, string expectedHighWaterMark, List<Target> targetList)
		{
			using (var auditConnection = AuditTestHelper.GetAuditConnection())
			using (SystemDataRegistry.Instance.BiIsRequiredDeleteOrphanSubscriber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var auditSubscriberProcessorTask = new TestAuditSubscriberProcessorTask();
				var rawSubscriber = new DeleteOrphanSubscriber()
				{
					TargetList = targetList,
					DatabaseTraverser = new DatabaseGraphTraverser()
				};

				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: null);
				var testLogger = new LoggerForTest();
				auditSubscriberProcessorTask.ServiceLogger = testLogger;
				auditSubscriberProcessorTask.RunMaintenanceTasksExposed();

				// Run it once to set high watermark
				SendLatestAuditChangesAndAssert(
					auditConnection,
					rawSubscriber,
					temporaryMaxLsn: "0x00",
					temporaryMaxPeriod: "2000-01-01 00:00:00.000",
					expectedLogs: Array.Empty<string>()
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: "00000000000000000000");

				if (sqlTextForInsertingIntoParents != null)
				{
					_ = auditConnection.ExecuteNonQuery(sqlTextForInsertingIntoParents);
				}

				if (sqlTextForInsertingIntoOrphans != null)
				{
					_ = Db.Connection.ExecuteNonQuery(sqlTextForInsertingIntoOrphans);
				}

				SendLatestAuditChangesAndAssert(auditConnection, rawSubscriber,
					temporaryMaxLsn: expectedMaxLSN,
					temporaryMaxPeriod: expectedMaxPeriod,
					expectedLogs: expectedLogs
				);
				AssertSubscriberHighWaterMark(auditConnection, rawSubscriber.Code, expected: expectedHighWaterMark);
			}
		}
	}
}

