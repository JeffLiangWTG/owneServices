using System.Threading;
using CargoWise.IO;

namespace CargoWise.Loader.Common
{
	public class CW1Splash : ISplash
	{
		readonly CW1ProcessStatusFormManager processStatusFormManager;

		public int ProgressValue => processStatusFormManager.ProgressValue;

		public string Status => processStatusFormManager.Status;

		public CW1Splash(Configuration configuration)
		{
			this.processStatusFormManager = new CW1ProcessStatusFormManager(configuration);
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

	class CW1ProcessStatusFormManager : ProcessStatusFormManager<CW1ProgressForm>
	{
		public CW1ProcessStatusFormManager(Configuration configuration)
			: base((object sender, ThreadExceptionEventArgs e) => { })
		{
			this.configuration = configuration;
			EnableHideOnModal();
		}

		protected override CW1ProgressForm CreateForm()
		{
			var form = CW1ProgressForm.New();
			form.Text = configuration.ApplicationName;
			return form;
		}

		readonly Configuration configuration;
	}
}
