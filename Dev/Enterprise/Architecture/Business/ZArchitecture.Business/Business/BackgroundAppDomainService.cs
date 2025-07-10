using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture
{
	class BackgroundAppDomainService : IBackgroundAppDomainService
	{
		public IBackgroundAppDomainWorkItem GetRecentlyCompletedWorkItem()
		{
			return BackgroundAppDomainWorker.RecentlyCompletedWorkItem;
		}

		public IBackgroundAppDomainWorkItem[] GetWorkItemsInProgress()
		{
			return BackgroundAppDomainWorker.WorkItemsInProgress;
		}
	}
}
