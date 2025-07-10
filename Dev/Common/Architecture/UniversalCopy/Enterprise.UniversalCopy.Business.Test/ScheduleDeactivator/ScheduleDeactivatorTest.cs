using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.UniversalCopy.Business.Testing
{
	class ScheduleDeactivatorTest : TestCaseWithFactory
	{
		public void TestGetRelatedCopySchedulesGetsTaskSchedules()
		{
			IEnumerable<StmUniversalCopyScheduleTask> returnSchedules = scheduleDeactivator.GetRelatedCopySchedules(task);

			AssertContainsExactElementsInAnyOrder(schedules.AsEnumerable(), returnSchedules);
		}

		public void TestGetRelatedCopySchedulesGets_WorkflowTasksSchedules()
		{
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var workflow = bmsTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var copyScheduleTask = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			var copySchedule = Factory.NewWithValidTestData<StmUniversalCopy>();
			copySchedule.SUC_CopyObjectId = workflow.PK;
			copyScheduleTask.S5_ParentID = copySchedule.PK;

			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_ParentID = workflow.FH_ParentId;
			task.P9_ParentTableCode = workflow.FH_ParentTableCode;

			Factory.Save();

			schedules.Add(copyScheduleTask);

			var returnSchedules = scheduleDeactivator.GetRelatedCopySchedules((BusinessObject)workflow);

			AssertContainsExactElementsInAnyOrder(schedules.AsEnumerable(), returnSchedules);
		}

		public void TestGetRelatedCopySchedulesGets_WorkflowsInsideWorkflowSchedules()
		{
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = bmsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var parentWorkflow = bmsTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var childWorkflow = bmsTestHelper.CreateWorkflow(jobHeader, "Workflow 2");

			var parent_copyScheduleTask = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			var parent_copySchedule = Factory.NewWithValidTestData<StmUniversalCopy>();
			parent_copySchedule.SUC_CopyObjectId = parentWorkflow.PK;
			parent_copyScheduleTask.S5_ParentID = parent_copySchedule.PK;

			var child_copyScheduleTask = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			var child_copySchedule = Factory.NewWithValidTestData<StmUniversalCopy>();
			child_copySchedule.SUC_CopyObjectId = childWorkflow.PK;
			child_copyScheduleTask.S5_ParentID = child_copySchedule.PK;

			childWorkflow.GetOrCreateLinkToParent(parentWorkflow);

			task.P9_FH_ProcessHeader = childWorkflow.PK;
			task.P9_ParentID = childWorkflow.FH_ParentId;
			task.P9_ParentTableCode = childWorkflow.FH_ParentTableCode;

			Factory.Save();

			schedules.Add(parent_copyScheduleTask);
			schedules.Add(child_copyScheduleTask);

			var returnSchedules = scheduleDeactivator.GetRelatedCopySchedules((BusinessObject)parentWorkflow);

			AssertContainsExactElementsInAnyOrder(schedules.AsEnumerable(), returnSchedules);
		}

		public void TestGetRelatedCopySchedulesGets_AllJobSchedules()
		{
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var workflow = bmsTestHelper.CreateWorkflow(Factory, "Workflow 1");
			var job = workflow.Parent;
			AssertEquals(OrgHeaderWorkflowDescriptor.WorkflowTypeCode, job.WorkflowType);

			var workflow_copyScheduleTask = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			var workflow_copySchedule = Factory.NewWithValidTestData<StmUniversalCopy>();
			workflow_copySchedule.SUC_CopyObjectId = workflow.PK;
			workflow_copyScheduleTask.S5_ParentID = workflow_copySchedule.PK;

			var workitem_copyScheduleTask = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			var workitem_copySchedule = Factory.NewWithValidTestData<StmUniversalCopy>();
			workitem_copySchedule.SUC_CopyObjectId = job.PK;
			workitem_copyScheduleTask.S5_ParentID = workitem_copySchedule.PK;

			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_ParentID = workflow.FH_ParentId;
			task.P9_ParentTableCode = workflow.FH_ParentTableCode;

			Factory.Save();

			schedules.Add(workflow_copyScheduleTask);
			schedules.Add(workitem_copyScheduleTask);

			var returnSchedules = scheduleDeactivator.GetRelatedCopySchedules((BusinessObject)job);

			AssertContainsExactElementsInAnyOrder(schedules.AsEnumerable(), returnSchedules);
		}

		public void TestCancelTaskWithNoCopySchedule()
		{
			Globals.SetIsUserInteractiveForTest(true);

			var taskWithNoCopySchedule = Factory.NewWithValidTestData<ProcessTask>();

			var mock = new Mock<IScheduleDeactivatorView>();
			mock.Setup(m => m.GetResponseFromUser());

			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("Should proceed with cancelling tasks", true, scheduleDeactivator.ConfirmCancellationAndMaybeDeactivateCopySchedules(taskWithNoCopySchedule));
				mock.Verify(m => m.GetResponseFromUser(), Times.Never);
			}
		}

		public void TestCancelTaskWithCopySchedule_ByUserAndUserResponse_DoNotCancelOrDeactivate()
		{
			Globals.SetIsUserInteractiveForTest(true);

			var mock = new Mock<IScheduleDeactivatorView>();
			mock.Setup(m => m.GetResponseFromUser()).Returns(ScheduleDeactivatorResponse.DoNotCancelOrDeactivate);

			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("Should NOT proceed with cancelling tasks", false, scheduleDeactivator.ConfirmCancellationAndMaybeDeactivateCopySchedules(task));
				AssertEquals("Copy schedules should NOT be deactivated", true, copyScheduleTask.S5_IsActive);
				mock.Verify(m => m.GetResponseFromUser(), Times.Once);
			}
		}

		public void TestCancelTaskWithCopySchedule_ByUserAndUserResponse_CancelAndDoNotDeactivate()
		{
			Globals.SetIsUserInteractiveForTest(true);

			var mock = new Mock<IScheduleDeactivatorView>();
			mock.Setup(m => m.GetResponseFromUser()).Returns(ScheduleDeactivatorResponse.CancelAndDoNotDeactivate);

			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("Should proceed with cancelling tasks", true, scheduleDeactivator.ConfirmCancellationAndMaybeDeactivateCopySchedules(task));
				AssertEquals("Copy schedules should NOT be deactivated", true, copyScheduleTask.S5_IsActive);
				mock.Verify(m => m.GetResponseFromUser(), Times.Once);
			}
		}

		public void TestCancelTaskWithCopySchedule_ByUserAndUserResponse_CancelAndDeactivate()
		{
			Globals.SetIsUserInteractiveForTest(true);

			var mock = new Mock<IScheduleDeactivatorView>();
			mock.Setup(m => m.GetResponseFromUser()).Returns(ScheduleDeactivatorResponse.CancelAndDeactivate);

			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("Should proceed with cancelling tasks", true, scheduleDeactivator.ConfirmCancellationAndMaybeDeactivateCopySchedules(task));
				AssertEquals("Copy schedules should be deactivated", false, copyScheduleTask.S5_IsActive);
				mock.Verify(m => m.GetResponseFromUser(), Times.Once);
			}
		}

		public void TestCancelTaskWithCopySchedule_ByServiceTaskAndRegistryItemIs_DND()
		{
			Globals.SetIsUserInteractiveForTest(false);

			RawDataRegistry.Instance.DefaultRelatedCopyScheduleDeactivationBehavior.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, RawDataRegistry.DefaultRelatedCopyScheduleDeactivationBehaviorCodes.DoNotDeactivateAnyCopySchedule);

			var mock = new Mock<IScheduleDeactivatorView>();
			mock.Setup(m => m.GetResponseFromUser());

			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("Should proceed with cancelling tasks", true, scheduleDeactivator.ConfirmCancellationAndMaybeDeactivateCopySchedules(task));
				AssertEquals("Copy schedules should NOT be deactivated", true, copyScheduleTask.S5_IsActive);
				mock.Verify(m => m.GetResponseFromUser(), Times.Never);
			}
		}

		public void TestCancelTaskWithCopySchedule_ByServiceTaskAndRegistryItemIs_DEA()
		{
			Globals.SetIsUserInteractiveForTest(false);

			RawDataRegistry.Instance.DefaultRelatedCopyScheduleDeactivationBehavior.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, RawDataRegistry.DefaultRelatedCopyScheduleDeactivationBehaviorCodes.DeactivateAllCopySchedules);

			var mock = new Mock<IScheduleDeactivatorView>();
			mock.Setup(m => m.GetResponseFromUser());

			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("Should proceed with cancelling tasks", true, scheduleDeactivator.ConfirmCancellationAndMaybeDeactivateCopySchedules(task));
				AssertEquals("Copy schedules should be deactivated", false, copyScheduleTask.S5_IsActive);
				mock.Verify(m => m.GetResponseFromUser(), Times.Never);
			}
		}

		List<StmUniversalCopyScheduleTask> schedules;

		ProcessTask task;

		StmUniversalCopy copySchedule;

		StmUniversalCopyScheduleTask copyScheduleTask;

		ScheduleDeactivator scheduleDeactivator;

		protected override void SetUp()
		{
			base.SetUp();

			schedules = new List<StmUniversalCopyScheduleTask>();
			task = Factory.NewWithValidTestData<ProcessTask>();
			copySchedule = Factory.NewWithValidTestData<StmUniversalCopy>();
			copyScheduleTask = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();

			copySchedule.SUC_CopyObjectId = task.PK;

			copyScheduleTask.S5_ParentID = copySchedule.PK;
			copyScheduleTask.S5_IsActive = true;

			schedules.Add(copyScheduleTask);

			Factory.Save();

			scheduleDeactivator = new ScheduleDeactivator();
		}
	}
}
