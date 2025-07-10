using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class SendOrganizationDataToCertCaptureControl : RegistryZUserControl
	{
		public SendOrganizationDataToCertCaptureControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (DataSource is SendOrganizationDataToCertCapture sendOrganizationDataToCertCapture)
			{
				yesRadioButton.Checked = sendOrganizationDataToCertCapture.EnableSend;
				noRadioButton.Checked = !sendOrganizationDataToCertCapture.EnableSend;
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			yesRadioButton.Enabled = !readOnly;
			noRadioButton.Enabled = !readOnly;
			SendAllOrganizationsButton.Enabled = !readOnly;
		}

		void SendAllOrganizationsButton_Click(object sender, EventArgs e)
		{
			if (Globals.Message.ShowConfirmation(Res.GetString("21F1FF71-EF7D-48A2-BCCE-5B85A497DF72", "Are you sure you want to send all organizations?"), Res.GetString("75728CFC-9716-491A-890C-980081319859", "Send organizations?"), "Y", MessageBoxIcon.Question) == DialogResult.OK)
			{
				var progressForm = new ProgressForm();
				try
				{
					progressForm.ShowCancelButton = false;
					progressForm.ShowModalTo(ParentForm);

					var sendResult = new EHubNativeOrgXmlSender().SendMessageToEHubForAllUsReceivableOrganizations((u, v) => progressForm.SetStatusAndPercentComplete(u, v));

					progressForm.Hide();

					if (!string.IsNullOrEmpty(sendResult))
					{
						Globals.Message.ShowInformation(sendResult);
					}
				}
				finally
				{
					progressForm.Close();
				}
			}
		}
	}
}
