using System.Threading;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BackgroundAppDomainServiceTest : TransactionedTestCase
	{
		public void TestBackgroundAppDomainService()
		{
			var backgroundAppDomainService = ObjectFactory.Get<IBackgroundAppDomainServiceForTest>();
			using (backgroundAppDomainService.RunInPrimaryAppDomainWorkerWithMultiWorkItemsForTestOnly(null))
			{
				var asyncResult1 = BackgroundAppDomainWorker.QueueWorkItem("TestWorkItem1", new ThreadStart(delegate
				{ Thread.Sleep(250); }));
				var asyncResult2 = BackgroundAppDomainWorker.QueueWorkItem("TestWorkItem2", new ThreadStart(delegate
				{ Thread.Sleep(500); }));

				var workItemsInProgress = backgroundAppDomainService.GetWorkItemsInProgress();
				AssertEquals("there are 2 work items in progress.", 2, workItemsInProgress.Length);
				AssertEquals("TestWorkItem1", workItemsInProgress[0].Description);
				AssertEquals("TestWorkItem2", workItemsInProgress[1].Description);

				for (int i = 0; i < 200; i++)
				{
					if (asyncResult1.IsCompleted && asyncResult2.IsCompleted)
					{
						break;
					}
					Thread.Sleep(50);
				}
				Assert("asyncResult1 is completed.", asyncResult1.IsCompleted);
				Assert("asyncResult2 is completed.", asyncResult2.IsCompleted);

				Thread.Sleep(100);
				AssertEquals("After all jobs have completed", 0, BackgroundAppDomainWorker.WorkItemsInProgress.Length);

				var recentlyCompletedWorkItem = backgroundAppDomainService.GetRecentlyCompletedWorkItem();
				AssertNotNull(recentlyCompletedWorkItem);
				AssertEquals("TestWorkItem2 should be the last one.", "TestWorkItem2", recentlyCompletedWorkItem.Description);
			}
		}
	}
}
