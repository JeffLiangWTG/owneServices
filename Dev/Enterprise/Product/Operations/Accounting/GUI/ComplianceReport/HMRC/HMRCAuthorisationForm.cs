using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ComplianceReport
{
	public partial class HMRCAuthorisationForm : ZChildForm
	{
		UrlHandler Handler;

		public HMRCAuthorisationForm()
		{
			InitializeComponent();
		}

		public void NavigateToHMRCAuthorization()
		{
			Handler = new WebCallbackUrlHandler(UrlHandlerCallback);

			EnterpriseUrlHandlerService.RegisterUrlHandler(Handler);

			WebUrlLauncher.Launch(MTDClient.OAuthUri.AbsoluteUri);
		}

		void UrlHandlerCallback(QueryString query)
		{
			OAuthCode = query["code"];
			DialogResult = DialogResult.OK;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			EnterpriseUrlHandlerService.UnregisterUrlHandler(Handler);
		}

		public string OAuthCode { get; private set; }

		void zButtonConnect_Click(object sender, System.EventArgs e)
		{
			NavigateToHMRCAuthorization();
		}

		void zButtonCancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		protected override bool ShowStatusBar => false;

		protected override void UpdateStatusBar(string notification, INotificationType state)
		{
			// This override is needed to avoid a NRE with the status bar hidden.
		}
	}
}
