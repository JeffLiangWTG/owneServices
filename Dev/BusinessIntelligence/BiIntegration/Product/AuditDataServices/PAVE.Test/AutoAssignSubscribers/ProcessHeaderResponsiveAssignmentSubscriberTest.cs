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

namespace Enterprise.AuditDataServices.PAVE.Test
{
	[TestedType(typeof(ProcessHeaderResponsiveAssignmentSubscriber))]
	class ProcessHeaderResponsiveAssignmentSubscriberTest : PAVESubscriberBaseTest
	{
		protected override string ExpectedCode => "PAH";

		protected override ITableSchema ExpectedTable => ProcessHeaderSchema.Instance;

		protected override IEnumerable<SchemaColumn> ExpectedSpecificColumns => null;

		public override void TestIsEnabled()
		{
			void AssertIfIsDisabled()
			{
				var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable(5);
				ProcessChanges(changeTable);

				loggerMock.Verify(l => l.Log(LogType.Debug, $"5 changes were skipped as the PAH subscriber is disabled."), Times.Once);
				loggerMock.Reset();
			}

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.TransferWorkflowComponentOnChanges = false;
			AssertIfIsDisabled();

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.AutoAssignCapabilityTasksOnChanges = true;
			AssertIfIsDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.AutoAssignCapabilityTasksOnChanges = false;
			AssertIfIsDisabled();

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			AssertIfIsDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.AutoAssignCapabilityTasksOnChanges = true;
			BMSRegistry.WorkflowManagementMode = "BWF";
			AssertIfIsDisabled();

			BMSRegistry.WorkflowManagementMode = "EWF";
			AssertIfIsDisabled();

			BMSRegistry.WorkflowManagementMode = "BUF";
			var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable(5);
			ProcessChanges(changeTable);

			loggerMock.Verify(l => l.Log(LogType.Debug, $"5 changes were skipped as the PAH subscriber is disabled."), Times.Never);
			loggerMock.Reset();

			BMSRegistry.WorkflowManagementMode = "PLN";

			//Other flags should not affect"
			BMSRegistry.EnablePaveExperimentalFeatures = false;
			BMSRegistry.TransferWorkflowComponentOnChanges = false;
			changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable(5);
			ProcessChanges(changeTable);

			loggerMock.Verify(l => l.Log(LogType.Debug, $"5 changes were skipped as the PAH subscriber is disabled."), Times.Never);
			loggerMock.Reset();
		}

		public void TestProcessChanges_ShouldAutoAssignCapabilityTasks_OnlyForActiveProcessHeaders()
		{
			var toProcessPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

			var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();

			var rowWithActiveProcessHeader1 = CreateRowForAutoAssignment(changeTable, toProcessPKs[0], active: true);
			rowWithActiveProcessHeader1.AcceptChanges();
			rowWithActiveProcessHeader1.SetModified();
			var rowWithActiveProcessHeader2 = CreateRowForAutoAssignment(changeTable, toProcessPKs[1], active: true);
			rowWithActiveProcessHeader2.AcceptChanges();
			rowWithActiveProcessHeader2.SetModified();

			var rowWithInactiveProcessHeader1 = CreateRowForAutoAssignment(changeTable, Guid.NewGuid(), active: false);
			rowWithInactiveProcessHeader1.AcceptChanges();
			rowWithInactiveProcessHeader1.SetModified();

			ProcessChanges(changeTable);

			AssertContainsExactElementsInAnyOrder(toProcessPKs, workflowPKsRequestedForAutoAssignment);
		}

		[TestDate(2021, 1, 12)]
		public void TestProcessChanges_ShouldAutoAssignCapabilityTasks_OnlyForReleasedProcessHeaders()
		{
			var utcNow = DateTime.SpecifyKind(ZDateTime.UtcNow.ToDateTime(), DateTimeKind.Unspecified);
			var sometimeInThePast = utcNow.AddDays(-5);

			var toProcessPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

			var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();

			var rowWithReleasedProcessHeader1 = CreateRowForAutoAssignment(changeTable, toProcessPKs[0]);
			rowWithReleasedProcessHeader1.AcceptChanges();
			rowWithReleasedProcessHeader1.SetModified();
			var rowWithReleasedProcessHeader2 = CreateRowForAutoAssignment(changeTable, toProcessPKs[1], releaseDateTime: sometimeInThePast);
			rowWithReleasedProcessHeader2.AcceptChanges();
			rowWithReleasedProcessHeader2.SetModified();

			var rowWithNonReleasedProcessHeader1 = CreateRowForAutoAssignment(changeTable, Guid.NewGuid(), isReleased: false);
			rowWithNonReleasedProcessHeader1.AcceptChanges();
			rowWithNonReleasedProcessHeader1.SetModified();

			ProcessChanges(changeTable);

			AssertContainsExactElementsInAnyOrder(toProcessPKs, workflowPKsRequestedForAutoAssignment);
		}

		public void TestProcessChanges_ShouldAutoAssignCapabilityTasks_OnlyForProcessHeadersThatAllowAutoAssignment()
		{
			var toProcessPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

			var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();

			var rowWhichAllowsAutoAssignment1 = CreateRowForAutoAssignment(changeTable, toProcessPKs[0], allowTaskAutoAssignment: true);
			rowWhichAllowsAutoAssignment1.AcceptChanges();
			rowWhichAllowsAutoAssignment1.SetModified();
			var rowWhichAllowsAutoAssignment2 = CreateRowForAutoAssignment(changeTable, toProcessPKs[1], allowTaskAutoAssignment: true);
			rowWhichAllowsAutoAssignment2.AcceptChanges();
			rowWhichAllowsAutoAssignment2.SetModified();

			var rowWhichRefusesAutoAssignment1 = CreateRowForAutoAssignment(changeTable, Guid.NewGuid(), allowTaskAutoAssignment: false);
			rowWhichRefusesAutoAssignment1.AcceptChanges();
			rowWhichRefusesAutoAssignment1.SetModified();

			ProcessChanges(changeTable);

			AssertContainsExactElementsInAnyOrder(toProcessPKs, workflowPKsRequestedForAutoAssignment);
		}

		[TestDate(2025, 05, 29)]
		public void TestProcessChanges_ShouldNotProcess_AndShouldNudgeBMTInstead_IfExceedsMax_NudgeWithDelay()
		{
			BMSRegistry.EnableNudgingBMTServiceTaskByThePVEServiceTask = true;

			const int nudgeDelay = 3;
			BMSRegistry.DelayForNudgingTheBMTServiceTaskByThePVEServiceTask = nudgeDelay;

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
			AssertEquals("Should schedule nudging a proper service task", CapabilityTaskAutoAssignmentServiceTask.Code, schedule.TAS_JsonParameter);
			AssertEquals("Should schedule with the proper action code", PAVEScheduledServiceTaskNudger.Code, schedule.TAS_ActionCode);
			AssertEquals(ZGuid.Empty, schedule.TAS_TargetPK);
			AssertEquals("n/a", schedule.TAS_TargetTableCode);
			AssertEquals(EnvProxy.Instance.CurrentBranchPK, schedule.TAS_GB_Branch.ToGuid());
			AssertEquals(EnvProxy.Instance.CurrentDepartmentPK, schedule.TAS_GE_Department.ToGuid());

			loggerMock.Verify(l => l.Log(LogType.Information, "Scheduled nudging the BMT service task with a 3 minute delay (at 29-May-25 00:03:00)."), Times.Once);
			loggerMock.Verify(l => l.Log(LogType.Information, "No need to schedule nudging the BMT service task as it is already scheduled at 29-May-25 00:03:00."), Times.Once);
			serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
		}

		[TestDate(2025, 05, 29)]
		public void TestProcessChanges_ShouldNotProcess_AndShouldNudgeBMTInstead_IfExceedsMax_ImmediateNudge()
		{
			BMSRegistry.EnableNudgingBMTServiceTaskByThePVEServiceTask = true;

			const int nudgeDelay = 0;
			BMSRegistry.DelayForNudgingTheBMTServiceTaskByThePVEServiceTask = nudgeDelay;

			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			{
				AssertDoesNotProcess_WhenThereAreTooManyChanges();
			}

			var schedules = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals("Should schedule just one action", 1, schedules.Count);
			var schedule = schedules.Single();
			AssertEquals("Should schedule with proper execution time - should not postpone time with multiple schedules", ZDateTime.UtcNow, schedule.TAS_ExecutionDateTimeUtc);
			AssertEquals("Should schedule nudging a proper service task", CapabilityTaskAutoAssignmentServiceTask.Code, schedule.TAS_JsonParameter);
			AssertEquals("Should schedule with the proper action code", PAVEScheduledServiceTaskNudger.Code, schedule.TAS_ActionCode);
			AssertEquals(ZGuid.Empty, schedule.TAS_TargetPK);
			AssertEquals("n/a", schedule.TAS_TargetTableCode);
			AssertEquals(EnvProxy.Instance.CurrentBranchPK, schedule.TAS_GB_Branch.ToGuid());
			AssertEquals(EnvProxy.Instance.CurrentDepartmentPK, schedule.TAS_GE_Department.ToGuid());

			loggerMock.Verify(l => l.Log(LogType.Information, "Scheduled nudging the BMT service task immediately (at 29-May-25 00:00:00)."), Times.Once);
			serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
		}

		[TestDate(2025, 05, 29)]
		public void TestProcessChanges_ShouldNotProcess_AndShouldNotNudgeBMTInstead_IfExceedsMax_AndNudgingIsDisabled()
		{
			BMSRegistry.EnableNudgingBMTServiceTaskByThePVEServiceTask = false;

			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			{
				AssertDoesNotProcess_WhenThereAreTooManyChanges();
			}

			var schedules = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals("Should not schedule when nudging is disabled", 0, schedules.Count);
			loggerMock.Verify(l => l.Log(LogType.Debug, "Skipped nudging the BMT service task as disabled in the registry."), Times.Once);
			serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
		}

		void AssertDoesNotProcess_WhenThereAreTooManyChanges()
		{
			BMSRegistry.AutoAssignCapabilityTasksMaxNumberOfCDCChanges = 10;

			var changeTable = PAVEProcessHeaderSubscribersTestHelper.CreateProcessHeaderTable();

			for (int i = 0; i <= 10; i++)
			{
				var row = CreateRowForAutoAssignment(changeTable, Guid.NewGuid(), allowTaskAutoAssignment: true);
				row.AcceptChanges();
				row.SetModified();
			}

			ProcessChanges(changeTable);

			AssertNull("No workflows should be processed", workflowPKsRequestedForAutoAssignment);
			loggerMock.Verify(l => l.Log(LogType.Warning, "Number of changes: 11 greater than AutoAssignCapabilityTasksMaxNumberOfCDCChanges: 10; Auto-Assignment will not be processed responsively and will be deferred to the Capability Task Auto-Assignment service task."), Times.AtLeastOnce);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new ProcessHeaderResponsiveAssignmentSubscriber();

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

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { ProcessHeaderSchema.PK, ProcessHeaderSchema.FH_FC_CurrentComponent, ProcessHeaderSchema.FH_P0_Template, ProcessHeaderSchema.FH_FH_ParentHeader };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}

		#region Implementation

		static DataRow CreateRowForAutoAssignment(DataTable dataTable, Guid pk, bool active = true, bool allowTaskAutoAssignment = true, bool isReleased = true, DateTime? releaseDateTime = null, DateTime? tranEndTimeUtc = null)
		{
			var utcNow = DateTime.SpecifyKind(ZDateTime.UtcNow.ToDateTime(), DateTimeKind.Unspecified);

			var row = dataTable.NewRow();

			row["TranEndTimeUtc"] = tranEndTimeUtc ?? ZDateTime.UtcNow.ToDateTime();

			row[ProcessHeaderSchema.Constants.PK] = pk;
			row[ProcessHeaderSchema.Constants.FH_Status] = "STS";
			row[ProcessHeaderSchema.Constants.FH_IsActive] = active;
			row[ProcessHeaderSchema.Constants.FH_P0_Template] = DBNull.Value;
			row[ProcessHeaderSchema.Constants.FH_CompletionStatement] = "Test";
			row[ProcessHeaderSchema.Constants.FH_ParentId] = Guid.NewGuid();
			row[ProcessHeaderSchema.Constants.FH_FC_CurrentComponent] = Guid.NewGuid();
			row[ProcessHeaderSchema.Constants.FH_AllowTaskAutoAssignment] = allowTaskAutoAssignment;
			row[ProcessHeaderSchema.Constants.FH_ReleaseDateTime] = isReleased
				? (object)releaseDateTime ?? utcNow
				: DBNull.Value;

			dataTable.Rows.Add(row);
			return row;
		}

		IEnumerable<Guid> workflowPKsRequestedForAutoAssignment;

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.AutoAssignCapabilityTasksOnChanges = true;

			workflowPKsRequestedForAutoAssignment = null;

			var capabilityTaskAutoAssignmentServiceClientMock = new Mock<ICapabilityTaskAutoAssignmentService>();
			capabilityTaskAutoAssignmentServiceClientMock.Setup(m => m.AutoAssignCapabilityTasks(It.IsAny<IEnumerable<Guid>>(), It.IsAny<ILogger>()))
				.Callback((IEnumerable<Guid> workflowPKs, ILogger logger) =>
				{
					workflowPKsRequestedForAutoAssignment = workflowPKs;
					AssertEquals(loggerMock.Object, logger);
				});

			serviceClientFactoryMock.Setup(m => m.GetCapabilityTaskAutoAssignmentServiceClient())
				.Returns(capabilityTaskAutoAssignmentServiceClientMock.Object);
		}

		#endregion
	}
}
