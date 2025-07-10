using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentUpdateNotificationEmail : BaseEDIRegistryNotificationEmailTemplate
	{
		public SupportIncidentUpdateNotificationEmail(SupportIncident incident,
			bool hasEDocsUpdates,
			Xsd.ConversationMessageCollection xsdConversationUpdate,
			IEnumerable<IConversationMessage> conversationUpdates,
			bool isStatusV2,
			SupportIncidentStatusSnapShot snapShotBeforeUpdate,
			bool isEmailToCustomer)
		{
			this.Incident = incident;

			SupportIncidentStatusSnapShot snapShotNow = new SupportIncidentStatusSnapShot();
			snapShotNow.TakeSnapShot(incident, false);
			status = isStatusV2 ? SupportIncident.GetCustomerSystemStatusCodeV2(snapShotNow) : SupportIncident.GetCustomerSystemStatusCodeV1(snapShotNow);
			criticality = incident.IM_Priority;
			incidentSummary = incident.IM_Description;
			incidentDetails = incident.DetailNoteText;

			originalStatus = isStatusV2 ? SupportIncident.GetCustomerSystemStatusCodeV2(snapShotBeforeUpdate) : SupportIncident.GetCustomerSystemStatusCodeV1(snapShotBeforeUpdate);
			originalCriticality = snapShotBeforeUpdate.IM_Priority;
			originalIncidentSummary = snapShotBeforeUpdate.IM_Description;
			originalIncidentDetails = snapShotBeforeUpdate.IncidentDetails;

			hasEDocsUpdate = hasEDocsUpdates;
			if (conversationUpdates != null)
			{
				hasEConversationUpdate = conversationUpdates.Any();
				conversationUpdate = conversationUpdates;
			}
			else
			{
				hasEConversationUpdate = xsdConversationUpdate != null && xsdConversationUpdate.Count > 0;
				eConversationUpdate = xsdConversationUpdate;
			}

			isEmailToClient = isEmailToCustomer;
		}

		public SupportIncidentUpdateNotificationEmail(SupportIncident incident, Xsd.CustomerServiceRequest request, SupportIncidentStatusSnapShot snapShotBeforeUpdate)
			: this(incident, request.Attachments.Count > 0, request.IncidentConversationUpdate, null, incident.ClientSystemSupportsBiDirectionUpdate, snapShotBeforeUpdate, false)
		{
		}

		public SupportIncidentUpdateNotificationEmail(SupportIncident incident, SupportIncidentStatusSnapShot originalSnapShot, Xsd.CustomerServiceResponseAttachmentCollection eDocsUpdate, Xsd.ConversationMessageCollection eConversationUpdate)
			: this(incident, eDocsUpdate != null && eDocsUpdate.Count > 0, eConversationUpdate, null, incident.ClientSystemSupportsBiDirectionUpdate, originalSnapShot, true)
		{
		}

		public SupportIncidentUpdateNotificationEmail(SupportIncident incident, Xsd.CustomerServiceRequest request, Xsd.CustomerServiceResponse response)
		{
			this.Incident = incident;

			status = response.Status;
			criticality = response.Criticality;
			incidentSummary = response.IncidentSummary;
			incidentDetails = response.IncidentDetails;

			originalStatus = request.Action == SupportIncidentLookups.LegacyActions.Update ? request.OriginalStatus : request.Status;
			originalCriticality = request.Action == SupportIncidentLookups.LegacyActions.Update ? request.OriginalCriticality : request.Criticality;
			originalIncidentSummary = request.Action == SupportIncidentLookups.LegacyActions.Update ? request.OriginalIncidentSummary : request.IncidentSummary;
			originalIncidentDetails = request.Action == SupportIncidentLookups.LegacyActions.Update ? request.OriginalIncidentDetails : request.IncidentDetails;

			hasEDocsUpdate = response.Attachments.Count > 0;
			hasEConversationUpdate = response.IncidentConversationUpdate.Count > 0;
			this.eConversationUpdate = response.IncidentConversationUpdate;

			isEmailToClient = true;
		}

		public readonly SupportIncident Incident;
		readonly bool isEmailToClient;
		readonly ZString status;
		readonly ZString criticality;
		readonly ZString incidentSummary;
		readonly ZString incidentDetails;
		readonly ZString originalStatus;
		readonly ZString originalCriticality;
		readonly ZString originalIncidentSummary;
		readonly ZString originalIncidentDetails;
		readonly bool hasEDocsUpdate;
		readonly bool hasEConversationUpdate;
		readonly Xsd.ConversationMessageCollection eConversationUpdate;
		readonly IEnumerable<IConversationMessage> conversationUpdate;

		public Xsd.CustomerServiceResponseEmail PublishClientEmailToResponse(Xsd.CustomerServiceResponse response)
		{
			Xsd.CustomerServiceResponseEmail result = null;

			if (isEmailToClient && ShouldSendEmail)
			{
				var email = SupportIncidentEmail.New(Incident, this, out var isAllowed);
				if (email != null)
				{
					result = ERequestEHubCustomerNotificationSender.GetEmailForSystemMessage(email);
					response.IncidentEmails.Add(result);
				}
			}

			return result;
		}

		bool IsAllowedSending => (Incident as IEDIEmailTriggeringRulesProvider).TriggeringRules.Allow(this);

		public string BuildAndSendEmailToAssignedStaff(bool save = true)
		{
			string staffEmail = null;

			if (!isEmailToClient && ShouldSendEmail)
			{
				var email = EDIEmailBuilder.GetInstance(Incident).BuildHtmlEmailDefByTemplate(this);
				if (email != null)
				{
					email.FromAddress = SupportIncident.SupportEmailAddress;
					email.FromDisplayName = SupportIncident.SupportDisplayName;
					email.Subject = string.Format(CultureInfo.CurrentCulture, "Customer Service Incident {0} Has Been Updated", Incident.IM_IncidentNumber);

					if (new SupportIncidentAssignedStaffEmailSender(Incident).SendEmailToAssignedStaff(email, save)
					&& email.Recipients.Count >= 1)
					{
						staffEmail = email.Recipients[0].Email;
					}
				}
			}

			return staffEmail;
		}

		void BuildAndSendEmailToAssignedStaffAndOtherSubscribers(bool save, List<string> excludeEmails = null, bool shouldSendIncidentUpdateEmail = false, bool shouldTriggerSubscribe = false)
		{
			string staffEmail = null;
			if (shouldSendIncidentUpdateEmail)
			{
				staffEmail = BuildAndSendEmailToAssignedStaff(save);
			}

			if ((hasEConversationUpdate && IsAllowedSending) || (shouldTriggerSubscribe && IsAllowedSending))
			{
				var newMessages = conversationUpdate ?? Incident.EConversation.GetNewMessagesAndClear();

				var urlStaff = SupportIncidentEmailBodyGeneralControls.GetIncidentFormUrl(Incident, ClientControllerRegistration.SupportIncident);
				var urlContact = SupportIncidentEmailBodyGeneralControls.GetIncidentGlowUrl(Incident);
				excludeEmails ??= new List<string>();
				if (staffEmail != null)
				{
					excludeEmails.Add(staffEmail);
				}

				var newJobConversationMessages = newMessages.Cast<JobConversationMessage>();
				excludeEmails.AddRange(newJobConversationMessages.Select(x => x.Sender?.Parent?.Email.ToString()).Where(x => x != null));
				excludeEmails.Add(SupportIncident.SupportEmailAddress);

				var factory = save ? new BusinessObjectFactory() { RefreshEnabled = false } : Incident.Factory;
				var hasEmailGenerated = SupportIncidentEConversationNotificationSender.GenerateStaffAndContactNotifications(
					Incident,
					factory,
					Incident.EConversation.ExistingConversation,
					newJobConversationMessages,
					excludeEmails,
					urlStaff,
					urlContact);

				if (save && hasEmailGenerated)
				{
					factory.Save();
				}
			}
		}

		public static void SendEmailToAssignedStaffAndOtherSubscribers(SupportIncident incident, SupportIncidentStatusSnapShot originalSnapShot, IEnumerable<IConversationMessage> messagesFromCustomer, bool shouldSaveEmail, List<string> excludeEmails = null, bool shouldSendIncidentUpdateEmail = false, bool shouldTriggerSubscribe = false)
		{
			var email = new SupportIncidentUpdateNotificationEmail(incident, false, null, messagesFromCustomer, true, originalSnapShot, false);
			email.BuildAndSendEmailToAssignedStaffAndOtherSubscribers(shouldSaveEmail, excludeEmails, shouldSendIncidentUpdateEmail, shouldTriggerSubscribe);
		}

		bool ShouldSendEmail
		{
			get
			{
				return (status != originalStatus && isEmailToClient && IsAllowedSending)
					|| criticality != originalCriticality
					|| incidentSummary != originalIncidentSummary
					|| incidentDetails != originalIncidentDetails
					|| hasEDocsUpdate
					|| hasEConversationUpdate;
			}
		}

		public override NotificationEmailTemplateRegistryItem RegistryItem => EDIDataRegistry.Instance.IncidentUpdateNotificationEmailTemplate;

		public override ZString TemplateCode => SupportIncidentEmailTemplateConstants.Codes.UpdateNotificationEmail;

		public override ZString TemplateDescription => SupportIncidentEmailTemplateConstants.Descriptions.UpdateNotificationEmail;

		public override ZString BuildSubject()
		{
			return new SupportIncidentParser(Incident.Factory).Parse(Incident, SubjectTemplate);
		}

		public override ZString BuildBody()
		{
			StringBuilder builder = new StringBuilder();

			if (isEmailToClient)
			{
				if (!BodyTemplate.IsEmpty)
				{
					builder.AppendLine(new SupportIncidentHtmlParser(Incident.Factory).Parse(Incident, BodyTemplate));
					builder.AppendLine();
				}
				builder.AppendLine(string.Format(CultureInfo.CurrentCulture, "<a href='{0}'>Incident number {1}{2}</a>",
					IncidentApprovalLookups.IncidentApprovalLinkMacro,
					Incident.IM_IncidentNumber,
					ClientIncidentReferenceForEmailBody));
				builder.AppendLine();
			}
			else
			{
				builder.AppendLine(string.Format(CultureInfo.CurrentCulture, "<a href=\"{0}\">Incident number {1}{2}</a>",
					 ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ClientControllerRegistration.SupportIncident, Incident.PK.ToGuid()),
					Incident.IM_IncidentNumber,
					ClientIncidentReferenceForEmailBody));
				builder.AppendLine();
			}

			if (isEmailToClient && status != originalStatus)
			{
				builder.AppendLine(string.Format(CultureInfo.CurrentCulture, "Status has been updated from {0} ({1}) to {2} ({3})",
										originalStatus,
										IncidentApprovalStatus.GetDescriptionFromCode(originalStatus),
										status,
										IncidentApprovalStatus.GetDescriptionFromCode(status)));
				builder.AppendLine();
			}

			if (criticality != originalCriticality)
			{
				builder.AppendLine(string.Format(CultureInfo.CurrentCulture, "Criticality has been updated from {0} ({1}) to {2} ({3})",
									originalCriticality,
									IncidentApprovalCriticality.GetDescriptionFromCode(originalCriticality),
									criticality,
									IncidentApprovalCriticality.GetDescriptionFromCode(criticality)));
				builder.AppendLine();
			}

			if (incidentSummary != originalIncidentSummary)
			{
				builder.AppendLine("Incident summary has been updated");
				builder.AppendLine();
			}

			if (incidentDetails != originalIncidentDetails)
			{
				builder.AppendLine("Incident details have been updated");
				builder.AppendLine();
			}

			if (hasEConversationUpdate)
			{
				if (Incident.Request.INC_Status == SupportIncidentLookups.LegacyStatusCodes.Closed && Incident.GetClosedReopenRule() == ResolutionAndClosureBehaviour.Constants.Code.NeverAllow)
				{
					builder.AppendLine("Attention: A reply was received to this Incident that has been permanently closed. An auto-reply email was sent to the customer with regards to the status of the ticket.");
					builder.AppendLine("You can override the status via Action > Re-open from the Incident form.");
					builder.AppendLine();
				}
				builder.AppendLine("eConversation has been updated");

				foreach (var body in eConversationUpdate != null
						? eConversationUpdate.Cast<Xsd.ConversationMessage>().Select(message => message.Body.CleanUpTextForHTML()).ToArray()
						: conversationUpdate.Select(m => m.Body.CleanUpTextForHTML()).ToArray())
				{
					builder.AppendLine(" - " + body);
				}

				if (!isEmailToClient)
				{
					builder.AppendLine();

					ZString userName = (eConversationUpdate != null)
						? eConversationUpdate[eConversationUpdate.Count - 1].UserName
						: conversationUpdate.Last().SenderDisplayName;

					if (!userName.IsEmpty)
					{
						builder.AppendLine(userName);
					}

					if (!Incident.ClientName.IsEmpty)
					{
						builder.AppendLine(Incident.ClientName);
					}
				}

				builder.AppendLine();
			}

			if (hasEDocsUpdate)
			{
				builder.AppendLine("eDocs have been updated");
				builder.AppendLine();
			}

			return builder.ToString().Replace("\r\n", "<br />");
		}

		string ClientIncidentReferenceForEmailBody
			=> Incident.IM_ClientIncidentReference.IsEmpty ? string.Empty : " / " + Incident.IM_ClientIncidentReference;

		CodeDescriptionPairList IncidentApprovalStatus
		{
			get { return incidentApprovalStatus ?? (incidentApprovalStatus = new IncidentApprovalLookups(null).StatusList); }
		}
		CodeDescriptionPairList incidentApprovalStatus;

		CodeDescriptionPairList IncidentApprovalCriticality
		{
			get { return incidentApprovalCriticality ?? (incidentApprovalCriticality = new IncidentApprovalLookups(null).CriticalityList); }
		}
		CodeDescriptionPairList incidentApprovalCriticality;
	}
}

