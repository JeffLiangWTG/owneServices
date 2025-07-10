using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	/// <summary>
	/// Sender for web requests.
	/// Adds a link to the web portal to any emails.
	/// </summary>
	public class WebRequestNotificationSender : IIncidentCustomerNotificationSender
	{
		public void AddHyperlinks(SupportIncidentEmail email)
		{
			var incident = email.BusinessObjectSendingEmail;
			var newBody = new ZStringBuilder();
			newBody.AppendLine(SupportIncidentEmailBodyGeneralControls.GetIncidentGlowHyperlink(incident));
			newBody.AppendLine();
			newBody.AppendLine(email.Body);
			email.Body = newBody.ToString();
		}

		public void SendEmail(SupportIncidentEmail email)
		{
			email?.SendEmail();
		}

		public void SendChanges(SupportIncident incident, SupportIncidentEmail email)
		{
			if (email != null)
			{
				email.ShouldSaveBizOFactoryOnSent = false;
				SendEmail(email);
			}

			this.CreateAndSendConversationMessageToCustomer(incident, email);
		}
	}
}
