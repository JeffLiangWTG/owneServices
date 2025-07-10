using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.PAVE.Subscribers;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(TransferTypeList))]

namespace Enterprise.AuditDataServices.PAVE.Test
{
	[TestedType(typeof(ProcessHeaderResponsiveTransferSubscriber))]
	class ProcessHeaderResponsiveTransferSubscriberTest : PAVESubscriberBaseTest
	{
		protected override string ExpectedCode => "PTH";

		protected override ITableSchema ExpectedTable => ProcessHeaderSchema.Instance;

		protected override IEnumerable<SchemaColumn> ExpectedSpecificColumns => null;

		public override void TestIsEnabled()
		{
			void AssertIfIsDisabled()
			{
				var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable(5);
				ProcessChanges(changeTable);

				loggerMock.Verify(l => l.Log(LogType.Debug, $"5 changes were skipped as the PTH subscriber is disabled."), Times.Once);
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
			var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable(5);
			ProcessChanges(changeTable);

			loggerMock.Verify(l => l.Log(LogType.Debug, $"5 changes were skipped as the PTH subscriber is disabled."), Times.Never);

			//Other flags should not affect"
			BMSRegistry.EnablePaveExperimentalFeatures = false;
			BMSRegistry.AutoAssignCapabilityTasksOnChanges = false;
			changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable(5);
			ProcessChanges(changeTable);

			loggerMock.Verify(l => l.Log(LogType.Debug, $"5 changes were skipped as the PTH subscriber is disabled."), Times.Never);
		}

		[TestUtcOffset(10, 0, 0)]
		[TestDate(2020, 05, 12, 0, 5, 0)]
		public void TestProcessChanges_ShouldIgnoreChangesIfBeforeToday(bool dynamicallyFilterTransferRules)
		{
			BMSRegistry.ProcessAllTransferRulesLinksOnNextBMSRun = false;
			BMSRegistry.DynamicallyFilterTransferRules = dynamicallyFilterTransferRules;
			var utcToday = DateTime.SpecifyKind(ZDateTime.UtcToday.ToDateTime(), DateTimeKind.Unspecified);
			var oneMillisecondInTheFuture = utcToday.AddMilliseconds(1);

			var oneMillisecondInThePast = utcToday.AddMilliseconds(-1);
			var tenMinutesInThePast = utcToday.AddMinutes(-10);

			var toProcessPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

			var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();

			var rowOneMillisecondInTheFuture1 = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, toProcessPKs[0], currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid(),
				tranEndTimeUtc: oneMillisecondInTheFuture);
			rowOneMillisecondInTheFuture1.AcceptChanges();
			rowOneMillisecondInTheFuture1.SetModified();
			var rowOneMillisecondInTheFuture2 = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, toProcessPKs[1], currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid(),
				tranEndTimeUtc: oneMillisecondInTheFuture);
			rowOneMillisecondInTheFuture2.AcceptChanges();
			rowOneMillisecondInTheFuture2.SetModified();

			var rowOneMillisecondInThePast = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, Guid.NewGuid(), currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid(),
				tranEndTimeUtc: oneMillisecondInThePast);
			rowOneMillisecondInThePast.AcceptChanges();
			rowOneMillisecondInThePast.SetModified();

			var rowTenMinutesInThePast = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, Guid.NewGuid(), currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid(),
				tranEndTimeUtc: tenMinutesInThePast);
			rowTenMinutesInThePast.AcceptChanges();
			rowTenMinutesInThePast.SetModified();

			ProcessChanges(changeTable);
			AssertEquals(true, BMSRegistry.ProcessAllTransferRulesLinksOnNextBMSRun);
			AssertNull(transferablePKsRequested);
		}

		public void TestProcessChanges_ShouldCallSchematicTransfer_OnlyForActiveProcessHeaders()
		{
			var toProcessPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
			var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();

			var rowWithActiveProcessHeader1 = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, toProcessPKs[0], currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid(), active: true);
			rowWithActiveProcessHeader1.AcceptChanges();
			rowWithActiveProcessHeader1.SetModified();
			var rowWithActiveProcessHeader2 = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, toProcessPKs[1], currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid(), active: true);
			rowWithActiveProcessHeader2.AcceptChanges();
			rowWithActiveProcessHeader2.SetModified();

			var rowWithInactiveProcessHeader1 = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, Guid.NewGuid(), currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid(), active: false);
			rowWithInactiveProcessHeader1.AcceptChanges();
			rowWithInactiveProcessHeader1.SetModified();

			ProcessChanges(changeTable);

			AssertContainsExactElementsInAnyOrder(toProcessPKs, transferablePKsRequested);
		}

		[TestDate(2020, 8, 20, 10, 11, 12)]
		public void TestProcessChanges_ShouldNotProcessAgain_WhenOnlyChangeComponentAndLastTransferTypeIsNotManual()
		{
			var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();

			var rowPKNoComponentChange = Guid.NewGuid();
			var rowNoComponentChange = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, rowPKNoComponentChange, currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid());
			rowNoComponentChange[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "A";
			rowNoComponentChange.AcceptChanges();
			rowNoComponentChange.SetModified();
			rowNoComponentChange[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "B";

			var rowPKComponentAndOtherChange = Guid.NewGuid();
			var rowComponentAndOtherChange = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, rowPKComponentAndOtherChange, currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid());
			rowComponentAndOtherChange[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] = Guid.NewGuid();
			rowComponentAndOtherChange[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "A";
			rowComponentAndOtherChange.AcceptChanges();
			rowComponentAndOtherChange.SetModified();
			rowComponentAndOtherChange[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] = Guid.NewGuid();
			rowComponentAndOtherChange[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "B";
			rowComponentAndOtherChange[ProcessHeaderSchema.Constants.FH_ReleaseDateTime] = ZDateTime.UtcNow.AddMinutes(1).ToDateTime();

			AssertEquals("Precondition - Manual TransferCode should be MAN", TransferTypeList.Codes.ManualTransfer, ProcessHeaderResponsiveTransferSubscriber.ManualTransferCode);

			var rowPKComponentManualChange = Guid.NewGuid();
			var rowComponentManualChange = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, rowPKComponentManualChange, currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid());
			rowComponentManualChange[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] = Guid.NewGuid();
			rowComponentManualChange.AcceptChanges();
			rowComponentManualChange.SetModified();
			rowComponentManualChange[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] = Guid.NewGuid();
			rowComponentManualChange[ProcessHeaderSchema.Constants.FH_LastTransferType] = TransferTypeList.Codes.ManualTransfer;
			rowComponentManualChange[ProcessHeaderSchema.Constants.FH_ReleaseDateTime] = ZDateTime.UtcNow.AddMinutes(1).ToDateTime();

			var rowPKComponentResponsiveChange = Guid.NewGuid();
			var rowComponentResponsiveChange = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, rowPKComponentResponsiveChange, currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid());
			rowComponentResponsiveChange[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] = Guid.NewGuid();
			rowComponentResponsiveChange.AcceptChanges();
			rowComponentResponsiveChange.SetModified();
			rowComponentResponsiveChange[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] = Guid.NewGuid();
			rowComponentResponsiveChange[ProcessHeaderSchema.Constants.FH_LastTransferType] = TransferTypeList.Codes.ResponsiveTransfer;
			rowComponentResponsiveChange[ProcessHeaderSchema.Constants.FH_ReleaseDateTime] = ZDateTime.UtcNow.AddMinutes(1).ToDateTime();

			var rowPKComponentSchematicChange = Guid.NewGuid();
			var rowComponentSchematicChange = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, rowPKComponentSchematicChange, currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid());
			rowComponentSchematicChange[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] = Guid.NewGuid();
			rowComponentSchematicChange.AcceptChanges();
			rowComponentSchematicChange.SetModified();
			rowComponentSchematicChange[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] = Guid.NewGuid();
			rowComponentSchematicChange[ProcessHeaderSchema.Constants.FH_LastTransferType] = TransferTypeList.Codes.SchematicTransfer;
			rowComponentSchematicChange[ProcessHeaderSchema.Constants.FH_ReleaseDateTime] = ZDateTime.UtcNow.AddMinutes(1).ToDateTime();

			var rowPKComponentReleaseChange = Guid.NewGuid();
			var rowComponentReleaseChange = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, rowPKComponentReleaseChange, currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid());
			rowComponentReleaseChange[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] = Guid.NewGuid();
			rowComponentReleaseChange.AcceptChanges();
			rowComponentReleaseChange.SetModified();
			rowComponentReleaseChange[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] = Guid.NewGuid();
			rowComponentReleaseChange[ProcessHeaderSchema.Constants.FH_LastTransferType] = TransferTypeList.Codes.ReleaseGate;
			rowComponentReleaseChange[ProcessHeaderSchema.Constants.FH_ReleaseDateTime] = ZDateTime.UtcNow.AddMinutes(1).ToDateTime();

			ProcessChanges(changeTable);

			var toBeProcessed = new[] { rowPKNoComponentChange, rowPKComponentAndOtherChange, rowPKComponentManualChange };
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

			var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();

			for (int i = 0; i <= BMSRegistry.TransferWorkflowComponentMaximumNumberOfCDCChanges; i++)
			{
				var row = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, Guid.NewGuid(), currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid());
				row.AcceptChanges();
				row.SetModified();
			}

			ProcessChanges(changeTable);

			AssertNull("No workflows should be processed", transferablePKsRequested);
			loggerMock.Verify(l => l.Log(LogType.Warning, "Number of changes: 11 greater than TransferWorkflowComponentMaximumNumberOfCDCChanges: 10; Workflow Transfers will not be processed responsively and will be deferred to the Buffer Management Schematic Transfer Runner service task."), Times.AtLeastOnce);
			AssertEquals(true, BMSRegistry.ProcessAllTransferRulesLinksOnNextBMSRun);
			AssertNull(transferablePKsRequested);
		}

		[TestDate(2025, 04, 04)]
		public void TestProcessChanges_ShouldNotProcess_WhenOnlyDedicatedBufferChanged()
		{
			var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();

			var rowWithDescriptionChangedOnlyPK = Guid.NewGuid();
			var rowWithDescriptionChangedOnly = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, rowWithDescriptionChangedOnlyPK, currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid());
			rowWithDescriptionChangedOnly.AcceptChanges();
			rowWithDescriptionChangedOnly.SetModified();
			rowWithDescriptionChangedOnly[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "New description";
			rowWithDescriptionChangedOnly[ProcessHeaderSchema.Constants.FH_SystemLastEditTimeUtc] = ZDateTime.UtcNow.AddMinutes(1).ToDateTime();
			rowWithDescriptionChangedOnly[ProcessHeaderSchema.Constants.FH_SystemLastEditUser] = "TST";

			var rowWithDedicatedBufferChangedOnlyPK = Guid.NewGuid();
			var rowWithDedicatedBufferChangedOnly = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderRow(changeTable, rowWithDedicatedBufferChangedOnlyPK, currentComponentPK: Guid.NewGuid(), parentHeaderPK: Guid.NewGuid(), dedicatedBufferPK: Guid.NewGuid());
			rowWithDedicatedBufferChangedOnly.AcceptChanges();
			rowWithDedicatedBufferChangedOnly.SetModified();
			rowWithDedicatedBufferChangedOnly[ProcessHeaderSchema.Constants.FH_FC_DedicatedBuffer] = Guid.NewGuid();
			rowWithDedicatedBufferChangedOnly[ProcessHeaderSchema.Constants.FH_SystemLastEditTimeUtc] = ZDateTime.UtcNow.AddMinutes(1).ToDateTime();
			rowWithDedicatedBufferChangedOnly[ProcessHeaderSchema.Constants.FH_SystemLastEditUser] = "TST";

			ProcessChanges(changeTable);

			Assert("Should process rows with changed description", transferablePKsRequested.Contains(rowWithDescriptionChangedOnlyPK));
			Assert("Should not process rows with changed dedicated buffer", !transferablePKsRequested.Contains(rowWithDedicatedBufferChangedOnlyPK));
			AssertContainsExactElementsInAnyOrder([rowWithDescriptionChangedOnlyPK], transferablePKsRequested);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new ProcessHeaderResponsiveTransferSubscriber();

			var table = GetTestDataTable();
			var row1 = GetPopulatedDataRow(table, false);
			var row2 = GetPopulatedDataRow(table, false);
			GetPopulatedDataRow(table, true);
			GetPopulatedDataRow(table, true);
			table.AcceptChanges();

			for (var i = 0; i < 4; i++)
			{
				RunCustomFilter(table.Rows[i], subscriber);
			}
			table.AcceptChanges();

			AssertEquals(2, table.Rows.Count);
			AssertEquals(row1, table.Rows[0]);
			AssertEquals(row2, table.Rows[1]);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = table.NewRow();
			var testId = Guid.NewGuid().ToString();
			row[ProcessHeaderSchema.PK.Name] = testId;
			if (shouldBeFiltered)
			{
				row[ProcessHeaderSchema.FH_FC_CurrentComponent.Name] = DBNull.Value;
				row[ProcessHeaderSchema.FH_P0_Template.Name] = testId;
				row[ProcessHeaderSchema.FH_FH_ParentHeader.Name] = DBNull.Value;
			}
			else
			{
				row[ProcessHeaderSchema.FH_FC_CurrentComponent.Name] = testId;
				row[ProcessHeaderSchema.FH_P0_Template.Name] = DBNull.Value;
				row[ProcessHeaderSchema.FH_FH_ParentHeader.Name] = testId;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable() => PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();

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
					if (transferablePKsRequested != null)
					{
						workflowPKs = transferablePKsRequested.Concat(workflowPKs);
					}

					transferablePKsRequested = workflowPKs;
					AssertEquals(loggerMock.Object, logger);
				});

			serviceClientFactoryMock.Setup(m => m.GetSchematicServiceClient())
				.Returns(schematicServiceClientMock.Object);
		}

		#endregion
	}
}
