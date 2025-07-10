using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public partial class LicenceDatabaseLogsRequestForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public LicenceDatabaseLogsRequestForm()
		{
			InitializeComponent();
		}

		public LicenceDatabaseLogsRequestForm(LicenceDatabaseLogsRequest bo)
			: base(bo)
		{
			this.logsRequest = bo;
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, null, cancelButton);

			var validation = new NotificationCollection();
			if (!bo.VersionSupported(validation))
			{
				notSupportedLabel.Visible = true;
				if (validation.HasNotifications())
				{
					notSupportedLabel.Text = validation.GetFirstMessage();
				}
				sendButton.Enabled = false;
				dateToField.ReadOnly = true;
				dateFromField.ReadOnly = true;
				serviceTaskCodeField.ReadOnly = true;
				IncidentNumberFindBox.ReadOnly = true;
			}
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			// Stop the base class from asking the user if they want to save changes
			DisplayMode = ODisplayMode.Browse;
			base.OnClosing(e);
		}

		protected void sendButton_Click(object sender, EventArgs e)
		{
			try
			{
				logsRequest.RunPreSaveValidation();
				if (!logsRequest.HasErrors)
				{
					logsRequest.Send();
					Close();
					Globals.Message.Show("A request for the service task logs has been sent to the client's batch processor.\r\nTheir system will respond with a Report and ediProd will be automatically updated in a few minutes.",
						"Service Task Logs Requested", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					ShowErrorsDialog();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string msg = "A request for service task logs could not be sent to the client:\r\n\r\n"
					+ ex.Message;
				Globals.Message.Show(msg, "Cannot Request Service Task Logs", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		readonly LicenceDatabaseLogsRequest logsRequest;
	}
}

