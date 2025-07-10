using System.ComponentModel;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Mail.GUI
{
	public partial class CustomerServiceEmailForm : EmailContactForm
	{
		public CustomerServiceEmailForm(CustomerServiceEmail emailToContactBusinessObject)
			: base(emailToContactBusinessObject)
		{
		}

		protected override string CloseButtonText => Res.GetString("F8B57C90-6F11-44EC-B853-189F600E4364", "Cancel");

		DialogResult ShowNoEmailWarning()
		{
			var incident = (BusinessEntity as SupportIncidentEmail)?.BusinessObjectSendingEmail;

			if (incident != null)
			{
				var form = GetConfirmationForm(incident);
				return ZFormModaliser.ShowDialogAndDispose(form, this);
			}

			return Globals.Message.Show(
				Res.GetString("ADA6A93D-791C-4C2C-BE2A-A6A936AF546E", "You are about to cancel the incident update email. If you proceed the customer will not be notified of the change to the incident. Would you like to continue?"),
				Res.GetString("5879AE19-CC28-421D-844A-60AFFF95618A", "Closing without sending an email"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				DialogResult.No);
		}

		protected virtual CancelEmailSendConfirmationForm GetConfirmationForm(SupportIncident incident)
		{
			return new CancelEmailSendConfirmationForm(new SupportIncidentCancelEmailNotificationAction(incident));
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (DialogResult == DialogResult.Cancel)
			{
				var result = ShowNoEmailWarning();
				if (result == DialogResult.Cancel || result == DialogResult.No)
				{
					e.Cancel = true;
					DialogResult = DialogResult.None;
				}
			}

			base.OnClosing(e);
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}
	}
}
