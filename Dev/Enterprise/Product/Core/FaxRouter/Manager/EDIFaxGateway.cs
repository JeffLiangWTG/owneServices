using System;
using System.Windows.Forms;

namespace Enterprise.FaxRouter.Manager
{
	public partial class EDIFaxGatewayForm : System.Windows.Forms.Form
	{
		public EDIFaxGatewayForm()
		{
			InitializeComponent();
			SetStateFaxGatewayControls();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void Exit_OnClick(object sender, EventArgs e)
		{
			if (!fGatewayStatusUserControl.StartButtonControlEnabled)
			{
				if (MessageBox.Show("Fax gateway is running! Do you really want to kill it now?", "EDI Fax Gateway", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					Application.Exit();
				}
			}
			else
			{
				Application.Exit();
			}
		}

		void SetStateFaxGatewayControls()
		{
			fGatewayStatusUserControl = new GatewayStatusUserControl();
			fGatewayStatusUserControl.Dock = DockStyle.Fill;
			StatusTabPage.Controls.Add(fGatewayStatusUserControl);

			fFaxManagmentUserControl = new FaxManagementUserControl();
			fFaxManagmentUserControl.Dock = DockStyle.Fill;
			FaxManagementTabPage.Controls.Add(fFaxManagmentUserControl);

			this.Show();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void EDIFaxGatewayForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!fGatewayStatusUserControl.StartButtonControlEnabled)
			{
				if (MessageBox.Show("Fax gateway is running! Do you really want to kill it now?", "EDI Fax Gateway", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
				{
					e.Cancel = true;
				}
			}
		}

		void FaxGatewayTabControl_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (FaxGatewayTabControl.TabPages[FaxGatewayTabControl.SelectedIndex].Name.Equals("FaxManagementTabPage"))
			{
				fFaxManagmentUserControl.RefreshGrid();
			}
		}
	}
}
