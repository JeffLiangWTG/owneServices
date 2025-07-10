using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class BackgroundAppDomainWorkItemLeakListener : BaseTestListener, Enterprise.Integration.ZArchitecture.IBackgroundAppDomainWorkItemLeakListener
	{
		public override void StartTest(TestCase test, DateTime startTime)
		{
			base.StartTest(test, startTime);
			workItemsInProgressCountBeforeTest = BackgroundAppDomainWorker.WorkItemsInProgress.Length;
		}

		public override void EndTest(TestCase test, DateTime endTime)
		{
			base.EndTest(test, endTime);
			Assertion.AssertEquals(
				"There must be no BackgroundAppDomainWorker work items remaining after a test.\r\n" +
				"Put 'using (BackgroundAppDomainWorker.RunInPrimaryAppDomainForTest())' around your test if you are queuing background work items.",
				workItemsInProgressCountBeforeTest, BackgroundAppDomainWorker.WorkItemsInProgress.Length);
		}

		int workItemsInProgressCountBeforeTest;
	}
}
