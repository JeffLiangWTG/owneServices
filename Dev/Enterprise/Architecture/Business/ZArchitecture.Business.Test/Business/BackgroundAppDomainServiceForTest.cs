using System;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BackgroundAppDomainServiceForTest : IBackgroundAppDomainServiceForTest
	{
		public IBackgroundAppDomainWorkItem GetRecentlyCompletedWorkItem()
		{
			return BackgroundAppDomainWorker.RecentlyCompletedWorkItem;
		}

		public IBackgroundAppDomainWorkItem[] GetWorkItemsInProgress()
		{
			return BackgroundAppDomainWorker.WorkItemsInProgress;
		}
		public IDisposable RunInPrimaryAppDomainWorkerWithMultiWorkItemsForTestOnly(Action<IBackgroundAppDomainWorkItem> setQueueWorkItemCompleted)
		{
			return BackgroundAppDomainWorkerForTest.RunInPrimaryAppDomainWorkerWithMultiWorkItemsForTest(setQueueWorkItemCompleted);
		}

		public IAsyncResult QueueWorkItemForTestOnly(string workItemDescription, Action workItemAction)
		{
			return BackgroundAppDomainWorker.QueueWorkItem(workItemDescription, workItemAction);
		}

		public void ClearRecentlyCompletedWorkItemForTestOnly() => BackgroundAppDomainWorkerForTest.ClearRecentlyCompletedWorkItem();
	}
}
