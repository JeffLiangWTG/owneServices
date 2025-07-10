using System.Threading;
using CargoWise.IO;

namespace CargoWise.Loader.Common
{
	public class CWNextSplash : ISplash
	{
		readonly CWNextProcessStatusFormManager processStatusFormManager;

		public int ProgressValue => processStatusFormManager.ProgressValue;

		public string Status => processStatusFormManager.Status;

		public CWNextSplash(Configuration configuration)
		{
			this.processStatusFormManager = new CWNextProcessStatusFormManager(configuration);
		}

		public void Dispose()
		{
			processStatusFormManager.Dispose();
		}

		public void Start()
		{
			processStatusFormManager.Start();
		}

		public void UpdateBranding()
		{
			processStatusFormManager.InvokeOnForm(form => form.UpdateBranding());
		}

		public void UpdateStatus(string status, int progressValue)
		{
			processStatusFormManager.UpdateStatus(status, progressValue);
		}
	}

	class CWNextProcessStatusFormManager : ProcessStatusFormManager<CWNextProgressForm>
	{
		public CWNextProcessStatusFormManager(Configuration configuration)
			: base((object sender, ThreadExceptionEventArgs e) => { })
		{
			this.configuration = configuration;
			EnableHideOnModal();
		}

		protected override CWNextProgressForm CreateForm()
		{
			var form = CWNextProgressForm.New();
			form.Text = configuration.ApplicationName;
			return form;
		}

		readonly Configuration configuration;
	}
}
