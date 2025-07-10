using System;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.Common.Testing;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.AuditDataServices.ArchiveManager.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.AuditDataServices.ArchiveManager.Testing
{
	[TestedType(typeof(DeleteOrphanSubscriberServiceTask))]
	internal class DeleteOrphanSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<DeleteOrphanSubscriberServiceTask>
	{
		protected override bool IsClientSpecific => false;

		public override DeleteOrphanSubscriberServiceTask GenerateServiceTask()
		{
			return new DeleteOrphanSubscriberServiceTask();
		}

		public override string ServiceTaskName()
		{
			return DeleteOrphanSubscriberServiceTask.Description;
		}

		public override void TestSubscriberServiceTaskAssemblyName()
		{
			var subscriberTask = GenerateServiceTask();

			AssertEquals("Enterprise.AuditDataServices.ArchiveManager", subscriberTask.AssemblyName);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRunTask_WhenOrphansDeleted_ThenNudgeACL()
		{
			var sqlInsertParents = $@"
				INSERT dbo.JobDeclaration ([__$lsn_period], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], JE_PK) VALUES
					(1711, 0x0B, 0x02, 1, 0x01, '5907C0BD-3EE5-481F-B3E4-99299BB982DE');
				INSERT [{BiConstants.BiAdminSchemaName}].LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES
					(0x0B, '2017-11-01');
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'JobDeclaration'
				UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET AetHWMHistorySummaryLsn = 0x0B WHERE SourceSchemaName = 'dbo' AND SourceTableName = 'ProcessTasks'
				INSERT INTO [{BiConstants.BiAdminSchemaName}].CdcHistorySummary (Lsn, SchemaName, ChangedTableName, NumberOfRows, NumberOfRowsDelete, LsnPeriod) VALUES
					(0x0B, 'dbo', 'JobDeclaration', 1, 1, 1711);
			";

			var sqlInsertOrphan = @"
				INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_Notes, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser) VALUES
					('1A83C3FD-E5E0-4E34-809F-7828CB311DEF', '5907C0BD-3EE5-481F-B3E4-99299BB982DE', 'JE', 0x0123456789, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (SystemDataRegistry.Instance.BiIsRequiredDeleteOrphanSubscriber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var subscriberTask = GenerateServiceTask();
				var testLogger = InitialiseTaskSchedule(subscriberTask);
				var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
				{
					using (var auditConnection = AuditTestHelper.GetAuditConnection())
					{
						using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x00"))
						using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2000-01-01 00:00:00.000"))
						{
							var rawSubscriber = new DeleteOrphanSubscriber();
							var testSubscriber = rawSubscriber.GetWrapper(auditConnection, new LoggerForTest());
							testSubscriber.AddSubscriberToSubscriberControlTable();

							var auditSubscriberProcessorTask = new TestAuditSubscriberProcessorTask();
							auditSubscriberProcessorTask.ServiceLogger = testLogger;
							auditSubscriberProcessorTask.RunMaintenanceTasksExposed();
						}

						_ = auditConnection.ExecuteNonQuery(sqlInsertParents);
						_ = Db.Connection.ExecuteNonQuery(sqlInsertOrphan);

						using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x0B"))
						using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2017-11-01 00:00:00.000"))
						{
							RunTaskSchedule(subscriberTask);
						}
					}

					mockServiceTaskNudger.Verify(x => x.NudgeServiceTask("ACL", null), Times.Once());

					Assert("When orphans are deleted, then should nudge the ACL service task.", testLogger.ToString().Contains("Nudging archive manager cleanup."));
					AssertContains("Deleted from: [ProcessTasks - ('1A83C3FD-E5E0-4E34-809F-7828CB311DEF')]", testLogger.ToString(), ignoreCase: true);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRunTask_WhenNoOrphansDeleted_ThenDoNotNudgeACL()
		{
			using (SystemDataRegistry.Instance.BiIsRequiredDeleteOrphanSubscriber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var subscriberTask = GenerateServiceTask();
				var testLogger = InitialiseTaskSchedule(subscriberTask);
				var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();

				using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
				{
					using (var auditConnection = AuditTestHelper.GetAuditConnection())
					{
						using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x00"))
						using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2000-01-01 00:00:00.000"))
						{
							var rawSubscriber = new DeleteOrphanSubscriber();
							var testSubscriber = rawSubscriber.GetWrapper(auditConnection, new LoggerForTest());
							testSubscriber.AddSubscriberToSubscriberControlTable();
						}

						using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnProcessed, "0x0B"))
						using (BiTemporaryMasterState.SetParameterTemporaryValue(auditConnection, BiConstants.LastMaxLsnTimeProcessed, "2017-11-01 00:00:00.000"))
						{
							RunTaskSchedule(subscriberTask);
						}
					}

					mockServiceTaskNudger.Verify(x => x.NudgeServiceTask("ACL", null), Times.Never);

					CombineAssertions(() =>
					{
						AssertNotContains("When orphans are not deleted, then ACL service task should not be nudged.", "Nudging archive manager cleanup.", testLogger.ToString());
						AssertNotContains("Deleted from: ", testLogger.ToString(), ignoreCase: true);
					});
				}
			}
		}

		public override void TestIsLoaded()
		{
			//it is not ActualDataChangesAuditSubscriber			
			Assert(true);
		}
	}
}
