using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.DocumentScanning.Business;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	/// <summary>
	/// Common Sender class for embedded-into-CW1, eRequest system.
	/// Status updates are published via system XML.
	/// </summary>
	public abstract class ERequestCustomerNotificationSender : IIncidentCustomerNotificationSender
	{
		public virtual void AddHyperlinks(SupportIncidentEmail email)
		{
		}

		public abstract void SendEmail(SupportIncidentEmail email);
		public abstract void SendChanges(SupportIncident incident, SupportIncidentEmail email);

		protected static CustomerServiceResponseAttachmentCollection ConvertToXml(SupportIncident incident, IEnumerable<StorageDocsBase> edocs)
		{
			CustomerServiceResponseAttachmentCollection result = new CustomerServiceResponseAttachmentCollection();

			foreach (StorageDocsBase doc in edocs)
			{
				var attachment = new CustomerServiceResponseAttachment();
				attachment.FileName = doc.SC_FileNameWithExtension.ReplaceIgnoringCase("\"", ZString.Empty);
				attachment.Data = doc.SC_ImageData;
				attachment.DocType = doc.SC_DocType;
				attachment.Desc = doc.SC_DescMultilingual;

				if (incident.ClientSystemSupportsBiDirectionUpdate)
				{
					attachment.DocTypeDescription = doc.DocType.RT_DescMultilingual;
				}

				result.Add(attachment);
			}

			return result;
		}

		static protected CustomerServiceResponse CreateResponse(SupportIncident incident)
		{
			CustomerServiceResponse response = new CustomerServiceResponse();
			response.ClientReferenceNumber = incident.IM_ClientIncidentReference;
			response.IncidentNumber = incident.IM_IncidentNumber;
			if (incident.ChangedPublishedEDocs.Any())
			{
				response.Attachments = ConvertToXml(incident, incident.ChangedPublishedEDocs);
			}

			return response;
		}

		public static ERequestCustomerNotificationSender CreateSenderIfSupported(SupportIncident incident)
		{
			// NOTE, only Enterprise/CW1 is expected to have a ClientCompany
			var database = incident.Database;
			if (database != null && incident.ClientCompany != null)
			{
				bool hasClientRef = !incident.IM_ClientIncidentReference.IsEmpty;
				if (hasClientRef)
				{
					if (database.VersionCanReceiveSystemMessageCSR == TriState.True)
					{
						return new ERequestEHubCustomerNotificationSender();
					}
					else
					{
						return new ERequestEmailCustomerNotificationSender();
					}
				}
				else if (database.VersionCanSupportBiDirectionIncidentMessage == TriState.True
					&& ERequestEHubCustomerNotificationSender.IsAllowedToReplicateIncidentOnClient(database)
					&& (incident.IsNewIncident || LicenceDatabase.CanSupportBiDirectionIncidentMessage(incident.ClientReportedOnVersion) == TriState.True))
				{
					// If not a new incident, but at the time it was created it would have been replicated.
					// For whatever reason we haven't received the client reference yet.
					// Keep sending updates.
					// Note: theoretically the incident may have been created as a CR6/7 and changed to a CR4
					// so it wouldn't have been replicated when it was first created in that case.
					return new ERequestEHubCustomerNotificationSender();
				}
			}

			return null;
		}

		/// <summary>
		/// Request XML was received via eHub or email.
		/// This plus the product tells us all we need to determine what sender to use for any responses.
		/// </summary>
		public static IIncidentCustomerNotificationSender CreateSenderFromXmlRequest(SupportIncident incident, bool fromEhubElseEmail)
		{
			IIncidentCustomerNotificationSender sender;
			if (!EDIDataRegistry.Instance.IncidentProductsToIgnoreForceResponseSentViaEHub.Value.ContainsCode(incident.IM_Product))
			{
				sender = fromEhubElseEmail ? new ERequestEHubCustomerNotificationSender() : new ERequestEmailCustomerNotificationSender();
			}
			else
			{
				sender = new EmailOnlyCustomerNotificationSender();
			}
			return sender;
		}
	}

	/// <summary>
	/// EHub embedded-into-CW1, eRequest system.
	/// Status updates are published via system XML via eHub.
	/// Messages to the customer are sent via direct email in V1, or in the XML in V2
	/// </summary>
	public class ERequestEHubCustomerNotificationSender : ERequestCustomerNotificationSender
	{
		public override void AddHyperlinks(SupportIncidentEmail email)
		{
			var incident = email.BusinessObjectSendingEmail;
			bool supportsERequestV2Emails = incident.Database.VersionCanSupportSendIncidentEmailFromClient == TriState.True;
			if (supportsERequestV2Emails)
			{
				var newBody = new ZStringBuilder();
				newBody.AppendLine(GetIncidentApprovalLink(incident));
				newBody.AppendLine();
				newBody.AppendLine(email.Body);

				email.Body = newBody.ToString();
			}
		}

		public override void SendEmail(SupportIncidentEmail email)
		{
			if (email == null)
			{
				return;
			}

			var incident = email.BusinessObjectSendingEmail;
			bool supportsERequestV2Emails = incident.Database.VersionCanSupportSendIncidentEmailFromClient == TriState.True;

			if (supportsERequestV2Emails)
			{
				SendFromClientSystem(incident, email);
			}
			else
			{
				email.SendEmail();
			}
		}

		public override void SendChanges(SupportIncident incident, SupportIncidentEmail email)
		{
			bool isNewERequestSystem = incident.ClientSystemSupportsBiDirectionUpdate;

			// Send the email first since it may add additional eConversation messages to go in the response
			if (email != null)
			{
				// possible simplifaction: include the email in the sent xml if any and skip this extra message
				email.ShouldSaveBizOFactoryOnSent = false;
				SendEmail(email);
			}

			if (!isNewERequestSystem)
			{
				this.CreateAndSendConversationMessageToCustomer(incident, email);
			}

			bool hasClientRef = !incident.IM_ClientIncidentReference.IsEmpty;
			ZString newStatus = incident.GetCurrentCustomerSystemStatusCode();
			bool statusHasChanges = incident.GetOriginalCustomerSystemStatusCode() != newStatus;
			bool publishedEDocsHasChanges = incident.ChangedPublishedEDocs.Any();
			bool oldStateHasChanges = statusHasChanges || publishedEDocsHasChanges || incident.IsNewIncident;
			bool shouldSendResponse;
			if (!isNewERequestSystem)
			{
				shouldSendResponse = hasClientRef && oldStateHasChanges;
			}
			else // new system
			{
				var originalSnapShot = incident.OriginalSnapShot;
				bool newStateHasChanges = incident.IM_Priority != originalSnapShot.IM_Priority
					|| incident.IM_Module != originalSnapShot.IM_Module
					|| incident.IM_Description != originalSnapShot.IM_Description
					|| incident.DetailNoteText != originalSnapShot.IncidentDetails
					|| incident.EConversation.GetNewLocalPublishedMessages().Any();

				shouldSendResponse = oldStateHasChanges || newStateHasChanges;
			}

			if (shouldSendResponse)
			{
				CustomerServiceResponse response = CreateResponse(incident);
				response.Status = newStatus;

				if (isNewERequestSystem)
				{
					bool shouldIncludeNotificationEmailToClient = email == null;

					var xmlMessages = new Xsd.ConversationMessageCollection();
					incident.EConversation.GetNewLocalPublishedMessages(xmlMessages);

					PopulateValuesForV2ClientSystem(incident, response, xmlMessages, shouldIncludeNotificationEmailToClient);
				}

				Send(incident, response);
			}
		}

		protected virtual void Send(SupportIncident incident, CustomerServiceResponse response)
		{
			var factory = new BusinessObjectFactory();
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(CustomerServiceResponse));
			IOutgoingSystemMessage sender = GetSender();
			using (MemoryStream xmlStream = new MemoryStream())
			{
				serializer.Serialize(xmlStream, response);
				xmlStream.Position = 0;
				var clientCompany = incident.ClientCompany;
				sender.CreateSecure(factory, SystemMessageList.Descriptions.CustomerServiceResponse, xmlStream, clientCompany.LicenceCode);
			}
			factory.Save();
		}

		void SendFromClientSystem(SupportIncident incident, SupportIncidentEmail email)
		{
			var response = new CustomerServiceResponse();
			response.Action = SupportIncidentLookups.LegacyActions.Email;
			response.ClientReferenceNumber = incident.IM_ClientIncidentReference;
			response.IncidentNumber = incident.IM_IncidentNumber;
			response.LicenceCode = incident.ClientCompany.LicenceCode;
			response.SentTimeUtc = ZDateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

			var xmlEmail = GetEmailForSystemMessage(email);
			response.IncidentEmails.Add(xmlEmail);
			Send(incident, response);

			email.DoAddEventAndPostSend(email.Body);
		}

		static internal CustomerServiceResponseEmail GetEmailForSystemMessage(SupportIncidentEmail email)
		{
			CustomerServiceResponseEmail result = new CustomerServiceResponseEmail();
			EmailDef clientEmail = email.GetEmail();
			result.Subject = clientEmail.Subject;
			result.Body = clientEmail.Body;
			result.Cc = clientEmail.CCRecipients.RecipientsAsDelimitedString();
			result.ToEmailAddress = clientEmail.Recipients.RecipientsAsDelimitedString();
			foreach (AttachmentDef clientEmailAttachment in clientEmail.Attachments)
			{
				CustomerServiceResponseEmailAttachment attachment = result.Attachments.AddNew();
				attachment.FileName = clientEmailAttachment.DisplayName;
				attachment.Data = clientEmailAttachment.Data;
			}

			return result;
		}

		protected virtual IOutgoingSystemMessage GetSender()
		{
			return new SystemMessage();
		}

		static void PopulateValuesForV2ClientSystem(SupportIncident incident,
			CustomerServiceResponse response,
			Xsd.ConversationMessageCollection newLocalPublishedMessages,
			bool shouldIncludeNotificationEmailToClient)
		{
			response.Action = incident.IsCreatingFromEdiProd ? SupportIncidentLookups.LegacyActions.Add : SupportIncidentLookups.LegacyActions.Update;
			response.PK = incident.IsCreatingFromEdiProd ? incident.PK.ToString() : string.Empty;
			response.LicenceCode = incident.ClientCompany.LicenceCode;
			response.Criticality = incident.PriorityDisplayedOnClientSide;
			response.Module = incident.IM_Module;
			response.ModuleDescription = incident.Lookups.ModuleListAllModules.GetDescriptionFromCode(incident.IM_Module);
			response.IncidentSummary = incident.IM_Description;
			response.IncidentDetails = incident.DetailNoteText;

			response.SystemReservedEmailAddresses = SupportIncidentLookups.SupportEmailAddress;
			if (incident.Contact != null && !incident.Contact.OC_Email.EqualsIgnoringCase(SupportIncidentLookups.SupportEmailAddress))
			{
				response.ContactEmailAddress = incident.Contact.OC_Email;
			}

			var changedPublishedEDocs = response.Attachments;

			if (!incident.IsNewIncident && shouldIncludeNotificationEmailToClient)
			{
				var email = new SupportIncidentUpdateNotificationEmail(incident, incident.OriginalSnapShot, changedPublishedEDocs, newLocalPublishedMessages);
				var emailInResponse = email.PublishClientEmailToResponse(response);
				if (emailInResponse != null)
				{
					emailInResponse.ToEmailAddress = emailInResponse.ToEmailAddress.ReplaceIgnoringCase(SupportIncidentLookups.SupportEmailAddress, "");
					emailInResponse.Cc = emailInResponse.Cc.ReplaceIgnoringCase(SupportIncidentLookups.SupportEmailAddress, "");
				}
			}

			if (newLocalPublishedMessages != null && newLocalPublishedMessages.Count > 0)
			{
				foreach (Xsd.ConversationMessage item in newLocalPublishedMessages)
				{
					response.IncidentConversationUpdate.Add(item);
				}
			}

			// Not using ZDateTime.UtcNow for response.SentTimeUtc here
			// Web service call may get updates before ehub message is constructed, which causes eConversation message or eDocs duplication
			ZDateTime calculatedSentTimeUtc = CalculateEHubMessageSentTimeInUtc(incident, newLocalPublishedMessages, changedPublishedEDocs);
			response.SentTimeUtc = calculatedSentTimeUtc.ToString("o", CultureInfo.InvariantCulture);
		}

		public static bool IsBiDirectionResponseSupported(SupportIncident incident)
		{
			var clientHasIncidentOrReplicateAllowed = (!incident.IM_ClientIncidentReference.IsEmpty || IsAllowedToReplicateIncidentOnClient(incident.Database));
			return clientHasIncidentOrReplicateAllowed
				&& incident.Database?.VersionCanSupportBiDirectionIncidentMessage == TriState.True;
		}

		internal static bool IsAllowedToReplicateIncidentOnClient(LicenceDatabase database)
		{
			bool isProductionNonEDILicence =
				database != null
				&& database.EnterpriseCode != "EDI"
				&& database.LD_LicenceType == DatabaseTypes.Codes.Production
				&& database.IsEnterpriseFamilyDatabase;

			return isProductionNonEDILicence;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZDateTime CalculateEHubMessageSentTimeInUtc(
			SupportIncident incident,
			Xsd.ConversationMessageCollection newLocalPublishedMessages,
			CustomerServiceResponseAttachmentCollection changedPublishedEDocs)
		{
			var logFactory = new BusinessObjectFactory();
			var logQuery = new ZDBOnlyQuery(typeof(StmALog));
			logQuery.AddToFilter(StmALogSchema.SL_Parent, incident.PK);
			logQuery.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			ZDateTime mostRecentIncidentLogTimeUtc = logFactory.LoadTop1<StmALog>(logQuery).SL_PostedTimeUtc;

			ZDateTime lastEConversationLogTimeUtc = ZDateTime.MinSmallDateTimeValue;
			if (newLocalPublishedMessages != null && newLocalPublishedMessages.Count > 0)
			{
				var messagesMap = newLocalPublishedMessages.Cast<Xsd.ConversationMessage>().Where(msg => msg.Id != ZGuid.Empty.ToString()).ToDictionary(msg => msg.Id);
				var newMessages = incident.EConversation.GetTimeOrderedMessages().Where(msg => messagesMap.ContainsKey(msg.Id.ToString()));
				if (newMessages.Any())
				{
					lastEConversationLogTimeUtc = newMessages.Max(msg => msg.SystemCreateTimeInUtc);
				}
			}

			ZDateTime lastEDocsLogTimeUtc = ZDateTime.MinSmallDateTimeValue;
			if (changedPublishedEDocs != null && changedPublishedEDocs.Count > 0)
			{
				var eDocsMap = changedPublishedEDocs.Cast<CustomerServiceResponseAttachment>().Distinct(new AttachmentComparer()).ToDictionary(attachment => attachment.FileName);

				ZDateTime lastFileLogTimeUtc = ZDateTime.MinSmallDateTimeValue;
				var updatedFiles = incident.DocManagerInfo.Files.Cast<StorageDocsBase>().Where(file => eDocsMap.ContainsKey(file.SC_FileNameWithExtension) && file.Logs.MostRecentLogByPostedDate != null);
				if (updatedFiles != null && updatedFiles.Any())
				{
					lastFileLogTimeUtc = updatedFiles.Max(file => file.Logs.MostRecentLogByPostedDate.SL_PostedTimeUtc);
				}

				ZDateTime lastDocumentLogTimeUtc = ZDateTime.MinSmallDateTimeValue;
				var updatedDocuments = incident.DocManagerInfo.Documents.Cast<StorageDocsBase>().Where(doc => eDocsMap.ContainsKey(doc.SC_FileNameWithExtension) && doc.Logs.MostRecentLogByPostedDate != null);
				if (updatedDocuments != null && updatedDocuments.Any())
				{
					lastDocumentLogTimeUtc = updatedDocuments.Max(doc => doc.Logs.MostRecentLogByPostedDate.SL_PostedTimeUtc);
				}

				lastEDocsLogTimeUtc = lastFileLogTimeUtc > lastDocumentLogTimeUtc ? lastFileLogTimeUtc : lastDocumentLogTimeUtc;
			}

			ZDateTime calculatedSentTimeUtc = mostRecentIncidentLogTimeUtc;
			if (calculatedSentTimeUtc < lastEConversationLogTimeUtc)
			{
				calculatedSentTimeUtc = lastEConversationLogTimeUtc;
			}
			if (calculatedSentTimeUtc < lastEDocsLogTimeUtc)
			{
				calculatedSentTimeUtc = lastEDocsLogTimeUtc;
			}

			return calculatedSentTimeUtc;
		}

		static string GetIncidentApprovalLink(SupportIncident incident)
		{
			var result = new ZStringBuilder("<a href='" + IncidentApprovalLookups.IncidentApprovalLinkMacro + "'>");
			result.Append("Incident number " + incident.IM_IncidentNumber);
			if (!incident.IM_ClientIncidentReference.IsEmpty)
			{
				result.Append(" / " + incident.IM_ClientIncidentReference);
			}
			result.Append("</a>");
			return result.ToString();
		}

		internal class AttachmentComparer : IEqualityComparer<CustomerServiceResponseAttachment>
		{
			public bool Equals(CustomerServiceResponseAttachment x, CustomerServiceResponseAttachment y)
			{
				return x.FileName == y.FileName;
			}

			public int GetHashCode(CustomerServiceResponseAttachment obj)
			{
				return obj.FileName.GetHashCode();
			}
		}
	}
}
