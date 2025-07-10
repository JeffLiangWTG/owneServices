using System;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public interface IBackgroundAppDomainServiceForTest : IBackgroundAppDomainService
	{
		IDisposable RunInPrimaryAppDomainWorkerWithMultiWorkItemsForTestOnly(Action<IBackgroundAppDomainWorkItem> setQueueWorkItemCompleted);

		IAsyncResult QueueWorkItemForTestOnly(string workItemDescription, Action workItemAction);

		void ClearRecentlyCompletedWorkItemForTestOnly();
	}
}
