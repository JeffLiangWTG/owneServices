using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(CancelEmailSendConfirmationForm))]
	public class CancelEmailSendConfirmationFormTest : BaseIncidentPopupFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentCancelEmailNotificationAction(incident);
			return new CancelEmailSendConfirmationForm(action);
		}
	}
}
