using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	[TestedType(typeof(WorkflowExceptionGenerationServiceTask))]
	sealed class WorkflowExceptionGenerationServiceTaskTest : ServiceTaskTestCase<WorkflowExceptionGenerationServiceTask>
	{
		[TestDate(2000, 1, 2, 1, 0, 0)]
		public void TestRunTask()
		{
			WorkflowExceptionGenerationServiceTask task = new WorkflowExceptionGenerationServiceTask();
			InitialiseTaskSchedule(task);

			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			RunTaskSchedule(task);

			dummy.WorkflowItems.Load();
			AssertEquals("No exceptions generated initially", 0, dummy.WorkflowItems.Exceptions.Count);

			ProcessTask overdueMilestone = dummy.WorkflowItems.Milestones.AddNew();
			overdueMilestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Now;
			overdueMilestone.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired;
			SaveFactoryWithoutRaisingMilestoneException();

			RunTaskSchedule(task);
			dummy.WorkflowItems.Load();
			AssertEquals("No exceptions generated until 20 minutes have passed", 0, dummy.WorkflowItems.Exceptions.Count);

			TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
			RunTaskSchedule(task);
			dummy.WorkflowItems.Load();
			AssertEquals("1 exception generated for overdue milestone", 1, dummy.WorkflowItems.Exceptions.Count);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		void SaveFactoryWithoutRaisingMilestoneException()
		{
			TestDateAttribute.Date = ZDateTime.Now.AddDays(-1).ToDateTime();
			Factory.Save();
			TestDateAttribute.Date = ZDateTime.Now.AddDays(1).ToDateTime();
		}
	}
}
