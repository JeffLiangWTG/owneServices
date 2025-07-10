using System;

namespace Enterprise.Accounting.Business
{
	public interface IProgressFormSupportable
	{
		string CurrentStatusText { get; }
		string Log { get; }
		int CompletedItems { get; }
		int TotaItemsToComplete { get; }

		event Action<IProgressFormSupportable, bool> RaiseProgressUpdateEvent;
	}
}
