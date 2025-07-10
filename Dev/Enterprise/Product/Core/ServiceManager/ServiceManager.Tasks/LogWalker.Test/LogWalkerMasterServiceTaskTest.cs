using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.LogWalker.Test
{
	[TestedType(typeof(LogWalkerMasterServiceTask))]
	class LogWalkerMasterServiceTaskTest : ServiceTaskTestCase<LogWalkerMasterServiceTask>
	{
		public void TestLogWalkerMasterQueue()
		{
			var queueProvider = new LogWalkerMasterQueue() as IHostedServiceQueueProvider;
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			Factory.Save();
			var startingQueue = queueProvider.QueueResult.QueueSize;

			trigger.Logs.AddNew(Events.WorkflowTriggerEvent);
			Factory.Save();
			var result = queueProvider.QueueResult;
			AssertEquals("WTE log and trigger EDT log should be added to the queue", 2, result.QueueSize - startingQueue);
			AssertNotNull(result.MaximumItemAge);

			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			result = queueProvider.QueueResult;
			AssertEquals("Z00 log should be added to the queue", 3, result.QueueSize - startingQueue);
			AssertNotNull(result.MaximumItemAge);
		}

		[UseSnapshotProtection]
		public void TestLogWalkerMasterQueueAge()
		{
			var itemDate = new DateTime(2023, 1, 1);
			Db.Connection.ExecuteNonQuery($@"
delete from dbo.StmALogQueue;
delete from dbo.StmALogQueueWTE;
insert into dbo.StmALogQueue (SLQ_EventTime, SLQ_ParentID, SLQ_PostedTimeUtc, SLQ_ParentTableName, SLQ_SE_NKEvent) values ('{itemDate}', newid(), '{itemDate}', 'a', 'b')");

			IHostedServiceQueueProvider provider = new LogWalkerMasterQueue();
			var result = provider.QueueResult.MaximumItemAge;

			var expectedAge = DateTime.UtcNow - itemDate;
			AssertNotNull(result);
			NUnit.Framework.Assert.That((int)result.TotalSeconds, NUnit.Framework.Is.EqualTo((int)expectedAge.TotalSeconds).Within(60));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						StmALogSchema.Constants.TableName,
						null),
				};
			}
		}
	}
}
