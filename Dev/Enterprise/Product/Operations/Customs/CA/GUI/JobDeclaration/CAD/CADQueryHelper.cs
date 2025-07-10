using System;
using System.Net.Http;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	class CADQueryHelper
	{
		public CADQueryHelper(Business.CusEntryHeader cadEntry, ZForm parentForm, HttpClient httpClient = null)
		{
			this.manager = new QueryCADMessageManager(cadEntry, httpClient);
			this.manager.QueryStart += new QueryCADMessageManager.ProcessStatusEventHandler(Processing);
			this.manager.QueryFinished += new QueryCADMessageManager.ProcessStatusEventHandler(ProcessFinish);
			this.parentForm = parentForm;
		}
		readonly ZForm parentForm;
		internal readonly QueryCADMessageManager manager;

		void Processing(int recordsToExport, string message)
		{
			ProgressForm.ShowModalTo(parentForm);
			ProgressForm.SetStatusAndPercentComplete(message, recordsToExport);
		}

		internal virtual void ProcessFinish(int recordsToExport, string message)
		{
			try
			{
				ProgressForm.SetStatusAndPercentComplete(message, recordsToExport);
				ProgressForm.Hide();
			}
			finally
			{
				Dispose();
			}
		}

		public void SendCADQuery_Click()
		{
			manager.Validate();
			manager.Query();
		}

		ProgressForm ProgressForm
		{
			get
			{
				if (progressForm == null)
				{
					progressForm = new ProgressForm();
					progressForm.ShowCancelButton = true;
					progressForm.SleepBetweenRefreshMilliseconds = 0;
					progressForm.Cancelled += new EventHandler(progressForm_Cancelled);
				}
				return progressForm;
			}
		}

		void progressForm_Cancelled(object sender, EventArgs e)
		{
			try
			{
				manager.CancelQuery();
			}
			finally
			{
				Dispose();
			}
		}

		void Dispose()
		{
			if (progressForm != null)
			{
				progressForm.Cancelled -= new EventHandler(progressForm_Cancelled);
				progressForm.Dispose();
				progressForm = null;
			}
			this.manager.QueryStart -= new QueryCADMessageManager.ProcessStatusEventHandler(Processing);
			this.manager.QueryFinished -= new QueryCADMessageManager.ProcessStatusEventHandler(ProcessFinish);
		}

		protected ProgressForm progressForm;
	}
}
