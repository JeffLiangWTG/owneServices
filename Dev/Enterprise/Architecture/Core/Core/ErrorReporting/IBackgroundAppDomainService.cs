using System;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Core
{
	public interface IBackgroundAppDomainService
	{
		IBackgroundAppDomainWorkItem GetRecentlyCompletedWorkItem();

		IBackgroundAppDomainWorkItem[] GetWorkItemsInProgress();
	}

	#region Work Item Status Enum

	public enum BackgroundAppDomainWorkItemStatus
	{
		Queued,
		Running,
	}

	#endregion

	#region Work Item Interface

	public interface IBackgroundAppDomainWorkItem
	{
		string Description { get; }
		BackgroundAppDomainWorkItemStatus Status { get; }
		ZDateTime SubmittedTime { get; }
		IAsyncResult AsyncResult { get; }
	}

	#endregion
}
