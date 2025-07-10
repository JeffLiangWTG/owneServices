using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	[TestedType(typeof(WorkflowModifiedFieldChangeTriggerServiceTask))]
	sealed class WorkflowModifiedFieldChangeTriggerServiceTaskTest : ServiceTaskTestCase<WorkflowModifiedFieldChangeTriggerServiceTask>
	{
		[TestDate(2000, 1, 2)]
		public void TestRunTask()
		{
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);
			var dummy = Factory.New<DummyWithWorkflow>();
			var fieldTrigger = dummy.RelatedWorkflowProvider.WorkflowItems.Triggers.AddNew();
			fieldTrigger.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			ProcessTaskNotification fieldNotification = fieldTrigger.ProcessTaskNotifications.AddNew();
			fieldNotification.PQ_TriggerType = "XXX";
			dummy.Z0_VarCharMax = "Modified";
			Factory.Save();
			TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(1).ToDateTime();

			WorkflowModifiedFieldChangeTriggerServiceTask task = new WorkflowModifiedFieldChangeTriggerServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			var query = new ZQuery(StmALogSchema.SL_Parent, fieldTrigger.PK)
				.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);

			AssertEquals(1, Factory.Load<StmALog>(query).Length);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest>
			ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		[TestDate(2000, 1, 2)]
		public void TestTriggerDoesntUnderflowBelowZero()
		{
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);
			var dummy = Factory.New<DummyWithWorkflow>();
			var fieldTrigger = dummy.RelatedWorkflowProvider.WorkflowItems.Triggers.AddNew();
			fieldTrigger.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			ProcessTaskNotification fieldNotification = fieldTrigger.ProcessTaskNotifications.AddNew();
			fieldNotification.PQ_TriggerType = "XXX";
			fieldTrigger.TriggerConditions.TriggerFiredCountdown = new ZShort(1);
			dummy.Z0_VarCharMax = "Modified";

			Factory.Save();

			dummy.Z0_VarCharMax = "Modified2";
			Factory.Save();

			TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(1).ToDateTime();
			WorkflowServiceTaskTestHelper.RunFieldChangeTriggerProcessor();
			fieldTrigger.Reload();
			AssertEquals((short)0, fieldTrigger.TriggerConditions.TriggerFiredCountdown);
		}

		[TestDate(2000, 1, 2)]
		public void TestMilestoneDoesntUnderflowBelowZero()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = dummy.RelatedWorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerFiredCountdown = 0;
			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			AssertEquals((short)0, milestone.TriggerConditions.TriggerFiredCountdown);
		}
	}
}
