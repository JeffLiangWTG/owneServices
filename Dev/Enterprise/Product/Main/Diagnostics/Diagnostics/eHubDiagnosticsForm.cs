using System;
using System.Drawing;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.Main.DiagnosticsAndTesting.Res;

namespace Enterprise.Diagnostics
{
	public partial class eHubDiagnosticsForm : ZChildForm
	{
		public eHubDiagnosticsForm()
			: base(new eHubDiagnostics())
		{
			InitializeComponent();
			this.CheckButton.Enabled = false;
			SendInfoLabel.Visible = false;
		}

		protected eHubDiagnostics eHubDiagnostics
		{
			get { return (eHubDiagnostics)BusinessEntity; }
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			ProceedOrPopupErrorMessage(sender, e);
		}

		internal virtual bool ProceedOrPopupErrorMessage(object sender, EventArgs e)
		{
			if (GlbCompany.CurrentCompany.GC_Code == "DEM" &&
				string.IsNullOrEmpty(ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode))
			{
				PopupErrorMessage(Res.GetString("0045BD04-6790-4e72-82AC-B9C186702D85", "You cannot use this feature when you are logged into the DEMO company. Please log into a different company."));
				return false;
			}
			else
			{
				eHubDiagnostics.Send();
				this.CheckButton.Enabled = true;
				this.SendButton.Enabled = false;
				SendInfoLabel.Visible = true;

				return true;
			}
		}

		internal virtual void PopupErrorMessage(string message)
		{
			Globals.Message.ShowError(message);
		}

		void CheckButton_Click(object sender, EventArgs e)
		{
			if (eHubDiagnostics.Check())
			{
				CheckStatusLabel.Text =  Res.GetString("4488f722-9ec2-4b85-9ba1-a712b444298a", "eHub Test message received. eHub connection successfully tested.");
				CheckStatusLabel.ForeColor = Color.Green;
			}
			else
			{
				CheckStatusLabel.Text = Res.GetString("8f4fa999-fd1d-4c54-a9ba-7bfe78f31063", "Test message has not arrived yet. Wait 1 minute and then press 'Check Now' again.");
				CheckStatusLabel.ForeColor = Color.Red;
			}
		}
	}
}
