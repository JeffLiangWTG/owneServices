using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Mail.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	/// <summary>
	/// Notifier for ediProd staff changing an incident via the UI.
	/// </summary>
	public class InteractiveIncidentCustomerNotifier : StaffChangeIncidentCustomerNotifier
	{
		public InteractiveIncidentCustomerNotifier(SupportIncident incident, IIncidentCustomerNotificationSender sender)
			: base(incident, sender)
		{
		}

		protected override bool CanPreview => true;

		protected override bool ConfirmSendEmail(SupportIncidentEmail email)
		{
			if (email.UserCanEdit)
			{
				var result = ZFormModaliser.ShowDialogAndDispose(new CustomerServiceEmailForm(email));
				email.UserCanEdit = false;
				return result == DialogResult.OK;
			}
			else
			{
				return true;
			}
		}
	}

	public class InteractiveIncidentCustomerNotifierFactory : IInteractiveIncidentCustomerNotifierFactory
	{
		public IIncidentCustomerNotifier CreateNotifier(SupportIncident incident, IIncidentCustomerNotificationSender sender)
		{
			return new InteractiveIncidentCustomerNotifier(incident, sender);
		}
	}
}
