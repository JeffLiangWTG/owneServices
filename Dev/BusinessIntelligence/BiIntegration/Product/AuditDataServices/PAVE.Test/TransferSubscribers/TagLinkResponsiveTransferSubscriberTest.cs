using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.PAVE.Subscribers;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.AuditDataServices.PAVE.Test
{
	[TestedType(typeof(TagLinkResponsiveTransferSubscriber))]
	class TagLinkResponsiveTransferSubscriberTest : PAVESubscriberBaseTest
	{
		protected override string ExpectedCode => "PTT";

		protected override ITableSchema ExpectedTable => TagLinkSchema.Instance;

		protected override IEnumerable<SchemaColumn> ExpectedSpecificColumns => new SchemaColumn[4] {
			TagLinkSchema.TGL_ParentId,
			TagLinkSchema.TGL_ParentTableCode,
			TagLinkSchema.TGL_Magnitude,
			TagLinkSchema.TGL_TGM_Magnitude
		};

		public override void TestIsEnabled()
		{
			DataTable CreateDataTable()
			{
				var changeTable = CreateTagLinkTable();
				var row1 = CreateTagLinkRow(changeTable, new Guid(), parentId: new Guid(), magnitude: 1, magnitudeFK: Guid.NewGuid());
				row1.AcceptChanges();
				row1.SetModified();
				var row2 = CreateTagLinkRow(changeTable, new Guid(), parentId: new Guid(), magnitude: 2, magnitudeFK: Guid.NewGuid());
				row2.AcceptChanges();
				row2.SetModified();
				return changeTable;
			}

			void AssertIfIsDisabled()
			{
				var changeTable = CreateDataTable();
				AssertNoExceptionThrown(() => ProcessChanges(changeTable));
				loggerMock.Verify(l => l.Log(LogType.Debug, $"2 changes were skipped as the PTT subscriber is disabled."), Times.Once);
				loggerMock.Reset();
			}

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.TransferWorkflowComponentOnChanges = false;
			AssertIfIsDisabled();

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.TransferWorkflowComponentOnChanges = true;
			AssertIfIsDisabled();

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.TransferWorkflowComponentOnChanges = false;
			AssertIfIsDisabled();

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.TransferWorkflowComponentOnChanges = true;
			AssertIfIsDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.TransferWorkflowComponentOnChanges = false;
			AssertIfIsDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.TransferWorkflowComponentOnChanges = true;
			AssertIfIsDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.TransferWorkflowComponentOnChanges = false;
			AssertIfIsDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.TransferWorkflowComponentOnChanges = true;
			var changeTable = CreateDataTable();
			ProcessChanges(changeTable);

			loggerMock.Verify(l => l.Log(LogType.Debug, $"2 changes were skipped as the PTT subscriber is disabled."), Times.Never);

			//Other flags should not affect
			BMSRegistry.EnablePaveExperimentalFeatures = false;
			BMSRegistry.AutoAssignCapabilityTasksOnChanges = false;
			changeTable = CreateTagLinkTable();
			changeTable = CreateDataTable();
			ProcessChanges(changeTable);

			loggerMock.Verify(l => l.Log(LogType.Debug, $"2 changes were skipped as the PTT subscriber is disabled."), Times.Never);
		}

		public void TestSubscriberCommonProperties()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertNotNull(subscriber);
			Assert(subscriber is TagLinkResponsiveTransferSubscriber);

			AssertEquals(TagLinkSchema.Constants.TableName, subscriber.Table.TableName);
			Assert(subscriber.NotifyInsert);
			Assert(subscriber.NotifyDelete);
			Assert(subscriber.NotifyUpdate);

			AssertContainsExactElementsInAnyOrder(
				new string[]
				{
					TagLinkSchema.Constants.TGL_ParentId,
					TagLinkSchema.Constants.TGL_ParentTableCode,
					TagLinkSchema.Constants.TGL_Magnitude,
					TagLinkSchema.Constants.TGL_TGM_Magnitude
				},
				subscriber.SpecificColumns.Select(c => c.Name));
		}

		public void TestTagLinkSubscriber_WhenTagLinkedToProcessHeader_ShouldCauseResponsiveTransferToUpdateProcessHeader()
		{
			var factory = new BusinessObjectFactory();
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var processHeaderBizo = bmsTestHelper.CreateWorkflow(factory, "A");

			factory.Save();

			var processHeaderChangeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();

			var processHeaderRowPK = processHeaderBizo.PK.ToGuid();
			var processHeaderRow = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(processHeaderChangeTable, processHeaderRowPK, currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid());
			processHeaderRow[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "A";
			processHeaderRow.AcceptChanges();
			processHeaderRow.SetModified();
			processHeaderRow[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "B";

			var tagLinkChangeTable = CreateTagLinkTable();

			var tagLinkRowPK = Guid.NewGuid();
			var tagLinkRow = CreateTagLinkRow(tagLinkChangeTable, tagLinkRowPK, parentId: processHeaderRowPK, magnitude: 3, magnitudeFK: Guid.NewGuid());
			tagLinkRow.AcceptChanges();
			tagLinkRow.SetModified();

			ProcessChanges(tagLinkChangeTable);

			var toBeProcessed = new[] { processHeaderRowPK };
			AssertContainsExactElementsInAnyOrder(toBeProcessed, transferablePKsRequested);
		}

		public void TestTagLinkSubscriber_WhenTagLinkedToProcessJobHeader_ShouldCauseResponsiveTransferToUpdateAllProcessHeadersLinkedToJobExceptJobHeader()
		{
			var factory = new BusinessObjectFactory();
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeaderBizo = bmsTestHelper.CreateJobHeader<OrgHeader>(factory, addDefaultProcessHeaderIfNone: false);
			var processHeader1Bizo = bmsTestHelper.CreateWorkflow(jobHeaderBizo, "A");
			var processHeader2Bizo = bmsTestHelper.CreateWorkflow(jobHeaderBizo, "C");

			factory.Save();

			var processHeaderChangeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();

			var jobHeaderRowPK = jobHeaderBizo.PK.ToGuid();
			var jobHeaderRow = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(processHeaderChangeTable, jobHeaderRowPK, currentComponentPK: Guid.NewGuid(), parentHeaderPK: null);
			jobHeaderRow[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "A";
			jobHeaderRow.AcceptChanges();
			jobHeaderRow.SetModified();
			jobHeaderRow[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "B";

			var processHeaderRow1PK = processHeader1Bizo.PK.ToGuid();
			var processHeader1Row = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(processHeaderChangeTable, processHeaderRow1PK, currentComponentPK: Guid.NewGuid(), parentHeaderPK: jobHeaderRowPK);
			processHeader1Row[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "A";
			processHeader1Row.AcceptChanges();
			processHeader1Row.SetModified();
			processHeader1Row[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "B";

			var processHeaderRow2PK = processHeader2Bizo.PK.ToGuid();
			var processHeader2Row = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(processHeaderChangeTable, processHeaderRow2PK, currentComponentPK: Guid.NewGuid(), parentHeaderPK: jobHeaderRowPK);
			processHeader2Row[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "C";
			processHeader2Row.AcceptChanges();
			processHeader2Row.SetModified();
			processHeader2Row[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "D";

			var tagLinkChangeTable = CreateTagLinkTable();

			var tagLinkRowPK = Guid.NewGuid();
			var tagLinkRow = CreateTagLinkRow(tagLinkChangeTable, tagLinkRowPK, parentId: jobHeaderRowPK, magnitude: 3, magnitudeFK: Guid.NewGuid());
			tagLinkRow.AcceptChanges();
			tagLinkRow.SetModified();

			ProcessChanges(tagLinkChangeTable);

			var toBeProcessed = new[] { processHeaderRow1PK, processHeaderRow2PK };
			AssertContainsExactElementsInAnyOrder(toBeProcessed, transferablePKsRequested);
		}

		[TestDate(2025, 05, 29)]
		public void TestProcessChanges_ShouldNotProcess_AndShouldNudgeBMSInstead_IfExceedsMax_NudgeWithDelay()
		{
			BMSRegistry.EnableNudgingBMSServiceTaskByThePVEServiceTask = true;

			const int nudgeDelay = 3;
			BMSRegistry.DelayForNudgingTheBMSServiceTaskByThePVEServiceTask = nudgeDelay;

			var expectedNudgeTime = ZDateTime.UtcNow.AddMinutes(nudgeDelay);

			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			{
				AssertDoesNotProcess_WhenThereAreTooManyChanges();
				TestDateAttribute.AddMinutes(1);
				AssertDoesNotProcess_WhenThereAreTooManyChanges();
			}

			var schedules = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals("Should schedule just one action", 1, schedules.Count);
			var schedule = schedules.Single();
			AssertEquals("Should schedule with proper execution time - should not postpone time with multiple schedules", expectedNudgeTime, schedule.TAS_ExecutionDateTimeUtc);
			AssertEquals("Should schedule nudging a proper service task", TransferRuleRunnerServiceTask.Code, schedule.TAS_JsonParameter);
			AssertEquals("Should schedule with the proper action code", PAVEScheduledServiceTaskNudger.Code, schedule.TAS_ActionCode);
			AssertEquals(ZGuid.Empty, schedule.TAS_TargetPK);
			AssertEquals("n/a", schedule.TAS_TargetTableCode);
			AssertEquals(EnvProxy.Instance.CurrentBranchPK, schedule.TAS_GB_Branch.ToGuid());
			AssertEquals(EnvProxy.Instance.CurrentDepartmentPK, schedule.TAS_GE_Department.ToGuid());

			loggerMock.Verify(l => l.Log(LogType.Information, "Scheduled nudging the BMS service task with a 3 minute delay (at 29-May-25 00:03:00)."), Times.Once);
			loggerMock.Verify(l => l.Log(LogType.Information, "No need to schedule nudging the BMS service task as it is already scheduled at 29-May-25 00:03:00."), Times.Once);
			serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
		}

		[TestDate(2025, 05, 29)]
		public void TestProcessChanges_ShouldNotProcess_AndShouldNudgeBMSInstead_IfExceedsMax_ImmediateNudge()
		{
			BMSRegistry.EnableNudgingBMSServiceTaskByThePVEServiceTask = true;

			const int nudgeDelay = 0;
			BMSRegistry.DelayForNudgingTheBMSServiceTaskByThePVEServiceTask = nudgeDelay;

			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			{
				AssertDoesNotProcess_WhenThereAreTooManyChanges();
			}

			var schedules = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals("Should schedule just one action", 1, schedules.Count);
			var schedule = schedules.Single();
			AssertEquals("Should schedule with proper execution time - should not postpone time with multiple schedules", ZDateTime.UtcNow, schedule.TAS_ExecutionDateTimeUtc);
			AssertEquals("Should schedule nudging a proper service task", TransferRuleRunnerServiceTask.Code, schedule.TAS_JsonParameter);
			AssertEquals("Should schedule with the proper action code", PAVEScheduledServiceTaskNudger.Code, schedule.TAS_ActionCode);
			AssertEquals(ZGuid.Empty, schedule.TAS_TargetPK);
			AssertEquals("n/a", schedule.TAS_TargetTableCode);
			AssertEquals(EnvProxy.Instance.CurrentBranchPK, schedule.TAS_GB_Branch.ToGuid());
			AssertEquals(EnvProxy.Instance.CurrentDepartmentPK, schedule.TAS_GE_Department.ToGuid());

			loggerMock.Verify(l => l.Log(LogType.Information, "Scheduled nudging the BMS service task immediately (at 29-May-25 00:00:00)."), Times.Once);
			serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
		}

		[TestDate(2025, 05, 29)]
		public void TestProcessChanges_ShouldNotProcess_AndShouldNotNudgeBMSInstead_IfExceedsMax_AndNudgingIsDisabled()
		{
			BMSRegistry.EnableNudgingBMSServiceTaskByThePVEServiceTask = false;

			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			{
				AssertDoesNotProcess_WhenThereAreTooManyChanges();
			}

			var schedules = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals("Should not schedule when nudging is disabled", 0, schedules.Count);
			loggerMock.Verify(l => l.Log(LogType.Debug, "Skipped nudging the BMS service task as disabled in the registry."), Times.Once);
			serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
		}

		void AssertDoesNotProcess_WhenThereAreTooManyChanges()
		{
			BMSRegistry.ProcessAllTransferRulesLinksOnNextBMSRun = false;
			BMSRegistry.TransferWorkflowComponentMaximumNumberOfCDCChanges = 10;

			var factory = new BusinessObjectFactory();
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var processHeaderBizos = Enumerable.Range(0, 15).Select(i => bmsTestHelper.CreateWorkflow(factory, $"A{i}")).ToArray();

			var processHeaderChangeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();
			var tagLinkChangeTable = CreateTagLinkTable();

			for (var i = 0; i < 15; i++)
			{
				var processHeaderRowPK = processHeaderBizos[i].PK.ToGuid();
				var processHeaderRow = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(processHeaderChangeTable, processHeaderRowPK, currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid());
				processHeaderRow[ProcessHeaderSchema.Constants.FH_CompletionStatement] = $"A{i}";
				processHeaderRow.AcceptChanges();
				processHeaderRow.SetModified();
				processHeaderRow[ProcessHeaderSchema.Constants.FH_CompletionStatement] = $"B{i}";

				var tagLinkRowPK = Guid.NewGuid();
				var tagLinkRow = CreateTagLinkRow(tagLinkChangeTable, tagLinkRowPK, parentId: processHeaderRowPK, magnitude: 3, magnitudeFK: Guid.NewGuid());
				tagLinkRow.AcceptChanges();
				tagLinkRow.SetModified();
			}

			ProcessChanges(tagLinkChangeTable);

			AssertNull("No workflows should be processed", transferablePKsRequested);
			loggerMock.Verify(l => l.Log(LogType.Warning, "Number of changes: 15 greater than TransferWorkflowComponentMaximumNumberOfCDCChanges: 10; Workflow Transfers will not be processed responsively and will be deferred to the Buffer Management Schematic Transfer Runner service task."), Times.AtLeastOnce);
			AssertEquals(true, BMSRegistry.ProcessAllTransferRulesLinksOnNextBMSRun);
			AssertNull(transferablePKsRequested);
		}

		public void TestTagLinkSubscriber_WhenGetWorkflowPKs_ShouldUseTableValuedParameterAndUseUnion()
		{
			var subscriber = NewDataChangeSubscriber();
			var factory = new BusinessObjectFactory();

			var tagLinkChangeTable = CreateTagLinkTable();
			var tagLinkRowPK = Guid.NewGuid();
			var parentId = Guid.NewGuid();
			var tagLinkRow = CreateTagLinkRow(tagLinkChangeTable, Guid.NewGuid(), parentId: parentId, magnitude: 3, magnitudeFK: Guid.NewGuid());

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				ProcessChanges(tagLinkChangeTable);

				var (query, plan) = TestConnection.ExecutedCommandsAndQueryPlans.Single(c => c.Item1.Contains("ProcessHeader"));
				AssertContains("Should use UseTableValuedParameter", "IN (SELECT * FROM @PKs)", query);
				AssertContains("Should have UNION", "UNION", query);

				var queryPlanAnalyzer = new QueryPlanalyzer(plan.First());

				Assert("There should be no table scans.", !queryPlanAnalyzer.TableScans.Any());
			}
		}

		public void TestTagLinkSubscriber_WhenDeletedRow_ShouldNotThrowException()
		{
			var factory = new BusinessObjectFactory();
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeaderBizo = bmsTestHelper.CreateJobHeader<DummyWithWorkflow>(factory, addDefaultProcessHeaderIfNone: false);
			var workflow = bmsTestHelper.CreateWorkflow(jobHeaderBizo, "A");
			factory.Save();
			var jobHeaderRowPK = workflow.PK.ToGuid();

			var tagLinkChangeTable = CreateTagLinkTable();
			var tagLinkRowPK = Guid.NewGuid();
			var tagLinkRow1 = CreateTagLinkRow(tagLinkChangeTable, Guid.NewGuid(), parentId: jobHeaderRowPK, magnitude: 3, magnitudeFK: Guid.NewGuid());
			tagLinkRow1.AcceptChanges();
			tagLinkRow1.Delete();
			var tagLinkRow2 = CreateTagLinkRow(tagLinkChangeTable, Guid.NewGuid(), parentId: jobHeaderRowPK, magnitude: 3, magnitudeFK: Guid.NewGuid());
			tagLinkRow2.AcceptChanges();
			tagLinkRow2.Delete();

			AssertNoExceptionThrown(() => ProcessChanges(tagLinkChangeTable));

			AssertContainsExactElementsInAnyOrder(new[] { jobHeaderRowPK }, transferablePKsRequested);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new TagLinkResponsiveTransferSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		#region Implementation

		protected IEnumerable<Guid> transferablePKsRequested;

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.TransferWorkflowComponentOnChanges = true;

			transferablePKsRequested = null;

			var schematicServiceClientMock = new Mock<ISchematicService>();
			schematicServiceClientMock.Setup(m => m.ProcessTransferRules(It.IsAny<IEnumerable<Guid>>(), It.IsAny<ILogger>()))
				.Callback((IEnumerable<Guid> workflowPKs, ILogger logger) =>
				{
					transferablePKsRequested = workflowPKs;
					AssertEquals(loggerMock.Object, logger);
				});

			serviceClientFactoryMock.Setup(m => m.GetSchematicServiceClient())
				.Returns(schematicServiceClientMock.Object);
		}

		static DataTable CreateTagLinkTable()
		{
			var changeTable = new DataTable(TagLinkSchema.Constants.TableName);
			changeTable.Columns.Add("TranEndTimeUtc", typeof(DateTime));

			changeTable.Columns.Add(TagLinkSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(TagLinkSchema.Constants.TGL_ParentId, typeof(Guid));
			changeTable.Columns.Add(TagLinkSchema.Constants.TGL_ParentTableCode, typeof(string));
			changeTable.Columns.Add(TagLinkSchema.Constants.TGL_Magnitude, typeof(decimal));
			changeTable.Columns.Add(TagLinkSchema.Constants.TGL_TGM_Magnitude, typeof(Guid));

			return changeTable;
		}

		static DataRow CreateTagLinkRow(DataTable dataTable, Guid pk, Guid parentId, decimal magnitude, Guid magnitudeFK, DateTime? tranEndTimeUtc = null)
		{
			var row = dataTable.NewRow();

			row["TranEndTimeUtc"] = tranEndTimeUtc ?? ZDateTime.UtcNow.ToDateTime();

			row[TagLinkSchema.Constants.PK] = pk;
			row[TagLinkSchema.Constants.TGL_ParentId] = parentId;
			row[TagLinkSchema.Constants.TGL_Magnitude] = magnitude;
			row[TagLinkSchema.Constants.TGL_TGM_Magnitude] = magnitudeFK;
			row[TagLinkSchema.Constants.TGL_ParentTableCode] = ProcessHeaderSchema.Constants.Prefix;

			dataTable.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable() => CreateTagLinkTable();

		#endregion
	}
}
