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
	public partial class LicenceUsageRequestForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public LicenceUsageRequestForm()
		{
			InitializeComponent();
		}

		public LicenceUsageRequestForm(LicenceUsageRequest bo)
			: base(bo)
		{
			this.usageRequest = bo;
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
				usageRequest.RunPreSaveValidation();
				if (!usageRequest.HasErrors)
				{
					usageRequest.Send();
					Close();
					Globals.Message.Show("A request for Licence Usage has been sent to the client's batch processor.\n\nTheir system will respond with a Report and ediProd will be automatically updated in a few minutes.",
						"Licence Usage Requested", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					ShowErrorsDialog();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string msg = "A request for a Licence Usage could not be sent to the client:\r\n\r\n"
					+ ex.Message;
				Globals.Message.Show(msg, "Cannot Request Licence Usage", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		readonly LicenceUsageRequest usageRequest;
	}
}

