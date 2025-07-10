using System;
using System.Globalization;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.Progress
{
	public class ProgressFormAdapter : IEdiProgress
	{
		readonly ProgressForm form;

		public ProgressFormAdapter(ProgressForm form)
		{
			this.form = form;
			form.Cancelled += new EventHandler(form_Cancelled);
		}

		void form_Cancelled(object sender, EventArgs e)
		{
			IsCancelled = true;
		}

		public void SetStatusAndPercentComplete(string currentStatus, int percentComplete)
		{
			form.SetStatusAndPercentComplete(currentStatus, percentComplete);
		}

		public void SetExpectedCount(int count)
		{
			ExpectedCount = count;
			CurrentCount = -1;
		}

		public void UpdateCurrentCount(string status)
		{
			++CurrentCount;
			form.SetStatusAndPercentComplete(
				string.Format(CultureInfo.CurrentCulture, "{0} ({1} of {2})", status, CurrentCount + 1, ExpectedCount),
				Math.Min(CurrentCount, ExpectedCount) * 100 / Math.Max(ExpectedCount, 1));
		}

		public bool IsCancelled { get; set; }

		public int ExpectedCount { get; private set; }
		public int CurrentCount { get; private set; }
	}
}

