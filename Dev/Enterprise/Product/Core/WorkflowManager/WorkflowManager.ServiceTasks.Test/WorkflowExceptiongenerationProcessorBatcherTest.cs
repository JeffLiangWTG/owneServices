using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	[TestDate]
	sealed class WorkflowExceptiongenerationProcessorBatcherTest : ManagedBatchProcessorTestCase<WorkflowExceptionGenerationProcessor.Batcher, ProcessTask>
	{
		protected override ProcessTask AddToQueue()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var overdueMilestone = WorkflowExceptionGenerationProcessorTest.CreateOverdueMilestone(job);
			bool wasTestDateActive = TestDateAttribute.IsActive;
			TestDateAttribute.Date = ZDateTime.Now.AddDays(-2).ToDateTime();
			Factory.Save();
			TestDateAttribute.Date = ZDateTime.Now.AddDays(2).ToDateTime();
			return overdueMilestone;
		}

		protected override WorkflowExceptionGenerationProcessor.Batcher GetProcessor()
		{
			return new WorkflowExceptionGenerationProcessor.Batcher(ZDateTime.Today, false);
		}

		protected override QueueStatus GetStatus(ProcessTask row)
		{
			if (row.P9_MilestoneExceptionAdded.IsEmpty)
			{
				return QueueStatus.Queued;
			}
			else if (row.P9_MilestoneExceptionAdded == ZDateTime.BrettsBirthday)
			{
				return QueueStatus.Failed;
			}
			else
			{
				return QueueStatus.Succeeded;
			}
		}
	}
}
