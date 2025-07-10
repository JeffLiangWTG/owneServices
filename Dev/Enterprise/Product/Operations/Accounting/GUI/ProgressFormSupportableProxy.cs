using System;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.GUI
{
	/// <summary>
	/// Implementation of IProgressFormSupportable, which can be used when no business object is available to report progress (eg: in static methods or event handlers).
	/// </summary>
	public class ProgressFormSupportableProxy : IProgressFormSupportable
	{
		public ProgressFormSupportableProxy()
		{
		}

		public ProgressFormSupportableProxy(int totaItemsToComplete)
		{
			TotaItemsToComplete = totaItemsToComplete;
		}

		public ProgressFormSupportableProxy(Action<IProgressFormSupportable, bool> progressEventHandler)
		{
			RaiseProgressUpdateEvent = progressEventHandler;
		}

		public string CurrentStatusText { get; private set; }

		public string Log { get; private set; }

		public int CompletedItems { get; private set; }

		public int TotaItemsToComplete { get; private set; }

		public event Action<IProgressFormSupportable, bool> RaiseProgressUpdateEvent;

		/// <summary>
		/// Set the total number of items this object is expected to complete.
		/// Also resets the number of CompletedItems to zero.
		/// </summary>
		public void ResetTotalItemsToComplete(int itemsToComplete)
		{
			TotaItemsToComplete = itemsToComplete;
			CompletedItems = 0;

			RaiseProgressUpdateEvent?.Invoke(this, CompletedItems >= this.TotaItemsToComplete);
		}

		/// <summary>
		/// Set the total number of items this object is expected to complete.
		/// Does not change the number of items completed so far.
		/// </summary>
		public void SetTotalItemsToComplete(int itemsToComplete)
		{
			TotaItemsToComplete = itemsToComplete;
			RaiseProgressUpdateEvent?.Invoke(this, CompletedItems >= this.TotaItemsToComplete);
		}

		/// <summary>
		/// Update the progress to have completed the next item, with status text and optional detailed log message.
		/// </summary>
		public void UpdateProgressStatus(string statusText, string logMessage = null)
		{
			CurrentStatusText = statusText;
			CompletedItems = CompletedItems + 1;
			Log = logMessage;

			RaiseProgressUpdateEvent?.Invoke(this, CompletedItems >= this.TotaItemsToComplete);
		}
	}
}
