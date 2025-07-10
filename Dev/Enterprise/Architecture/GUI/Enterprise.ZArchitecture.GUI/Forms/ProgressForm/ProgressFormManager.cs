using System;
using CargoWise.Data;
using CargoWise.IO;

namespace Enterprise.ZArchitecture.GUI
{
	public class ProgressFormManager : ZProcessStatusFormManager<ProcessStatusProgressForm>
	{
		ProcessStatusProgressForm form;

		public bool IsCancelButtonVisible
		{
			get { return isCancelButtonVisible; }
			set
			{
				isCancelButtonVisible = value;
				if (form != null)
				{
					form.ShowCancelButton = value;
				}
			}
		}
		bool isCancelButtonVisible;

		public bool IsProgressBarVisible
		{
			get { return isProgressBarVisible; }
			set
			{
				isProgressBarVisible = value;
				if (form != null)
				{
					form.ShowProgressBar = value;
				}
			}
		}
		bool isProgressBarVisible = true;

		protected override ProcessStatusProgressForm CreateFormCore()
		{
			disposableActionForDbConnection = Db.DisposableActionForDbConnection();

			var localForm = new ProcessStatusProgressForm();
			localForm.Cancelled += Cancelled;
			localForm.Cancelled += form_Cancelled;
			localForm.ShowCancelButton = isCancelButtonVisible;
			localForm.ShowProgressBar = isProgressBarVisible;

			form = localForm;
			return form;
		}

		protected override void DisposeCore()
		{
			if (form != null)
			{
				form.Cancelled -= Cancelled;
				form.Cancelled -= form_Cancelled;
				form = null;
			}
		}

		void form_Cancelled(object sender, EventArgs e)
		{
			form.Dispose();
			Dispose();
		}

		public event EventHandler Cancelled;
	}

	public class ProcessStatusProgressForm : ProgressForm, IProcessStatus
	{
		public void UpdateStatus(string status, int progressValue)
		{
			ProgressLabel.Text = status;
			ProgressBar.Value = progressValue;
		}
	}
}
