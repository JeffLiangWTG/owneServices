using System.IO;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	/// <summary>
	/// Original embedded-into-CW1, eRequest system.
	/// Status updates are published via system XML in an email.
	/// Messages to the customer are sent via direct email.
	/// </summary>
	public class ERequestEmailCustomerNotificationSender : ERequestCustomerNotificationSender
	{
		public override void SendEmail(SupportIncidentEmail email)
		{
			email?.SendEmail();
		}

		public override void SendChanges(SupportIncident incident, SupportIncidentEmail email)
		{
			if (email != null)
			{
				email.ShouldSaveBizOFactoryOnSent = false;
				email.SendEmail();
			}

			this.CreateAndSendConversationMessageToCustomer(incident, email);

			if (!incident.IM_ClientIncidentReference.IsEmpty
				&& incident.Database != null
				&& !incident.Database.LD_PublicEmailAddressForUpdate.IsEmpty)
			{
				ZString newStatus = incident.GetCurrentCustomerSystemStatusCode();
				bool statusHasChanges = incident.GetOriginalCustomerSystemStatusCode() != newStatus;
				bool publishedEDocsHasChanges = incident.ChangedPublishedEDocs.Any();
				bool oldStateHasChanges = statusHasChanges || publishedEDocsHasChanges || incident.IsNewIncident;
				if (oldStateHasChanges)
				{
					Xsd.CustomerServiceResponse response = CreateResponse(incident);
					response.Status = newStatus;
					Send(incident, response);
				}
			}
		}

		static void Send(SupportIncident incident, Xsd.CustomerServiceResponse response)
		{
			EmailDef email = new EmailDef();
			email.FromDisplayName = SupportIncident.SupportDisplayName;
			email.FromAddress = EDIDataRegistry.Instance.IncidentFromEmailAddress.Value;
			email.AddRecipientForUserCommunication(incident.Database.LD_PublicEmailAddressForUpdate);

			email.Subject = CustomerService.Business.ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject + ": " + incident.IM_ClientIncidentReference;

			using (MemoryStream xmlStream = new MemoryStream())
			{
				ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceResponse));
				serializer.Serialize(xmlStream, response);
				email.Attachments.Add(new AttachmentDef("Incident Details.xml", xmlStream.ToArray()));
			}

			Env.OutgoingMailManager.CreateAndSave(email);
		}
	}
}

