using System;

namespace CargoWise.EntityFramework
{
	[Flags]
	public enum DataRefreshAction
	{
		None = 0,
		UpdateDeletedSubscriberWhenPublisherUpdated = 1,
		UpdateDeletedSubscriberWhenPublisherDeleted = 2,
		UpdateNonDeletedSubscriberWhenPublisherDeleted = 4,
		UpdateNonDeletedSubscriberWhenPublisherUpdated = 8,
	}

	public interface ICanApplyDataRefresh
	{
		bool CanApplyDataRefresh(DataRefreshAction action, BusinessObject publisher);
	}
}
