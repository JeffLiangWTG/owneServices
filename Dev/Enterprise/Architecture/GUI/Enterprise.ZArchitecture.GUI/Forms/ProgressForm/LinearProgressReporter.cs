using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public sealed class LinearProgressReporter : IProgressReporter
	{
#if DEBUG
		internal
#endif
		ProgressForm form;
		readonly ProgressStatus progressStatus;
		readonly int itemsBetweenUpdates;
		readonly int numberToProcess;
		readonly bool allowCancel;

		const int minItemsBetweenUpdate = 1;
		const int maxItemsBetweenUpdate = 20;

		public LinearProgressReporter(int numberToProcess, bool allowCancel = false)
			: this(numberToProcess, Math.Min(Math.Max(numberToProcess / 100, minItemsBetweenUpdate), maxItemsBetweenUpdate), allowCancel)
		{
		}

		public LinearProgressReporter(int numberToProcess, int itemsBetweenUpdates, bool allowCancel = false)
		{
			this.allowCancel = allowCancel;
			this.numberToProcess = numberToProcess;
			this.itemsBetweenUpdates = itemsBetweenUpdates;
			progressStatus = new ProgressStatus(numberToProcess);
			Report(0);
		}

		public void Report(int totalItemsProcessedSoFar)
		{
			ItemsProcessed = totalItemsProcessedSoFar;

			if (form != null && numberToProcess > 0 && totalItemsProcessedSoFar % itemsBetweenUpdates == 0)
			{
				form.SetStatusAndPercentComplete(progressStatus.GetStatusReport(totalItemsProcessedSoFar), 100 * totalItemsProcessedSoFar / numberToProcess);
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (form != null)
			{
				form.Cancelled -= ProcessForm_Cancelled;
				form.Dispose();
			}
		}

		#endregion

		#region IProgressReporter Members

		public int ItemsProcessed { get; private set; }
		public bool IsCancelled { get; private set; }

		void ProcessForm_Cancelled(object sender, EventArgs e)
		{
			IsCancelled = true;
		}

		public void ShowForm(IComponent parentFormToShowModallyTo, string initialMessage)
		{
			if (this.form != null)
			{
				throw new InvalidOperationException("Cannot re-show form");
			}

			form = new ProgressForm(initialMessage);
			form.ShowCancelButton = allowCancel;
			form.Cancelled += ProcessForm_Cancelled;
			form.ShowModalTo((Form)parentFormToShowModallyTo);
		}

		#endregion
	}
}
