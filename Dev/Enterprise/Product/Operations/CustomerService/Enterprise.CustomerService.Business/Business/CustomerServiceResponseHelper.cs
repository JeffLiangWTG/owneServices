using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business
{
	public class CustomerServiceResponseHelper
	{
		public CustomerServiceResponseHelper(Xsd.CustomerServiceResponse response, IncidentApproval incident, INotifications notifications = null)
		{
			this.response = response;
			this.incident = incident;
			this.lastSyncTime = incident.LastSyncTime;
			this.notifications = notifications;
		}

		public CustomerServiceResponseHelper(Xsd.CustomerServiceResponse response, IncidentApproval incident, ZDateTime lastSyncTime)
		{
			this.response = response;
			this.incident = incident;
			this.lastSyncTime = lastSyncTime;
		}

		readonly Xsd.CustomerServiceResponse response;
		readonly IncidentApproval incident;
		readonly ZDateTime lastSyncTime;
		readonly INotifications notifications;

		~CustomerServiceResponseHelper()
		{
			Unlock();
		}

		#region Process and Save

		public void ProcessAndSave()
		{
			if (response.Action == IncidentApprovalLookups.Actions.Email)
			{
				SendEmailsFromResponse();
			}
			else
			{
				ProcessResponse();

				Mutex = new ZGlobalMutex(MutexIDs.IncidentApprovalUpdate, incident.PK.ToString());
				try
				{
					int failureCount = 0;
					int maxFailureCount = 3;
					while (failureCount < maxFailureCount)
					{
						if (Mutex.Lock())
						{
							ZDateTime lastSyncTimeInDb = ZDateTime.Empty;
							if (incident.IsInDatabase)
							{
								BusinessObjectFactory newFactory = new BusinessObjectFactory();
								lastSyncTimeInDb = newFactory.Load<IncidentApproval>(incident.PK).LastSyncTime;
							}

							ZDateTime sentTime = ZDateTimeConversionHelper.ParseRoundTripFormatString(response.SentTimeUtc);
							if (lastSyncTimeInDb.IsEmpty || (lastSyncTimeInDb < sentTime && lastSyncTime == lastSyncTimeInDb))
							{
								incident.Factory.Save();
								if (response.Attachments.Count > 0)
								{
									incident.DocManagerInfo.MasterFactory.Save();
								}

								SendEmailsFromResponse();
							}
							else
							{
								LogWarning((NoResString)"Response message is outdated. Changes are not saved.");
							}

							break;
						}
						else
						{
							failureCount++;
							Thread.Sleep(5000);
						}
					}

					if (failureCount == maxFailureCount)
					{
						LogWarning((NoResString)"Reached maximum trial count to acquire lock to save. Changes are not saved.");
					}
				}
				finally
				{
					Unlock();
				}
			}
		}

		void Unlock()
		{
			if (Mutex != null && Mutex.HasLock)
			{
				Mutex.Unlock();
				Mutex = null;
			}
		}

		ZGlobalMutex Mutex;

		#endregion

		#region Process Response

		void ProcessResponse()
		{
			incident.IA_IncidentNumber = response.IncidentNumber;
			incident.IA_Status = response.Status;

			if (!IsV1ResponseMessage)
			{
				incident.IA_LicenceCode = response.LicenceCode;

				var previousModuleType = incident.ModuleType;
				incident.IA_Criticality = response.Criticality;
				var newModuleType = IncidentApprovalLookups.GetModuleListType(incident.IA_Criticality);
				if (response.Action == IncidentApprovalLookups.Actions.Add)
				{
					incident.IA_Module = IsValidModuleCode(response.Module, newModuleType) ? response.Module : IncidentApprovalLookups.GetOtherModuleCode(newModuleType);
				}
				else if (previousModuleType != newModuleType)
				{
					incident.IA_Module = IncidentApprovalLookups.GetOtherModuleCode(newModuleType);
				}

				incident.IA_IncidentSummary = response.IncidentSummary;
				incident.IA_IncidentDetails = response.IncidentDetails.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n"); // Fixup: XML deserialization of strings converts "\r\n" to "\n"
			}

			AddEDocAttachments();
			AddEConversationUpdates();
			SetIncidentLastSyncTime();
		}

		bool IsV1ResponseMessage
		{
			get
			{
				return response.LicenceCode.IsEmpty
					&& response.Criticality.IsEmpty
					&& response.Module.IsEmpty
					&& response.ModuleDescription.IsEmpty
					&& response.IncidentSummary.IsEmpty
					&& response.IncidentDetails.IsEmpty;
			}
		}

		void AddEDocAttachments()
		{
			foreach (Xsd.CustomerServiceResponseAttachment attachment in response.Attachments)
			{
				var eDoc = incident.DocManagerInfo.AddFileOrDocument(attachment.Data, attachment.FileName, ReturnActualDocType(attachment.DocType, attachment.DocTypeDescription));
				eDoc.Description = attachment.Desc;
			}
		}

		ZString ReturnActualDocType(ZString docTypeCode, ZString docTypeDescription)
		{
			ZQuery query = new ZQuery(RefDocTypeSchema.RT_DocType, docTypeCode);
			query.AddToFilter(RefDocTypeSchema.RT_ReferenceType, new string[] { Constants.ReferenceTypes.BusinessEntityProcessWorkflow, Constants.ReferenceTypes.All });
			RefDocType docType = incident.Factory.LoadTop1<RefDocType>(query);
			if (docType == null)
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				docType = newFactory.New<RefDocType>();
				docType.RT_DocType = docTypeCode;
				docType.RT_Desc = docTypeDescription;
				docType.RT_ReferenceType = Constants.ReferenceTypes.BusinessEntityProcessWorkflow;
				docType.RT_IsActive = true;
				newFactory.Save();
			}

			return docType.RT_DocType;
		}

		void AddEConversationUpdates()
		{
			incident.EConversation.AddMessagesFromRemoteUser(response.IncidentConversationUpdate);
			incident.UpdateLastEditDetails();
		}

		void SetIncidentLastSyncTime()
		{
			ZDateTime sentTime = ZDateTimeConversionHelper.ParseRoundTripFormatString(response.SentTimeUtc);
			if (!sentTime.IsEmpty && sentTime.IsValid)
			{
				incident.LastSyncTime = DateTime.SpecifyKind(sentTime.ToDateTime(), DateTimeKind.Utc);
			}
		}

		#endregion

		#region Send Email

		void SendEmailsFromResponse()
		{
			try
			{
				foreach (Xsd.CustomerServiceResponseEmail incidentEmail in response.IncidentEmails)
				{
					EmailDef email = new EmailDef();
					email.FromAddress = Env.Registry.MailboxEmailAddress;
					email.FromDisplayName = Env.Registry.MailboxDisplayName;
					PopulateEmailContentFromResponseEmail(email, incidentEmail);

					email.ReplyTo = GetNoReplyAddress(email);

					AddNotificationRecipients(email, response.SystemReservedEmailAddresses);
					AddToEmailRecipients(email, incidentEmail.ToEmailAddress);
					AddCcRecipients(email, incidentEmail.Cc);

					if (email.Recipients.Count == 0 && !response.ContactEmailAddress.IsEmpty)
					{
						email.AddRecipientForSystemCommunication(response.ContactEmailAddress);
					}

					if (email.Recipients.Count > 0)
					{
						Env.OutgoingMailManager.CreateAndSave(email);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogError((NoResString)"Could not send notification emails: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
			}
		}

		void PopulateEmailContentFromResponseEmail(EmailDef email, Xsd.CustomerServiceResponseEmail responseEmail)
		{
			email.ContentType = EmailContentTypes.HTML;
			email.Subject = responseEmail.Subject;
			email.Body = responseEmail.Body;
			if (email.Body.Contains(IncidentApprovalLookups.IncidentApprovalLinkMacro))
			{
				string incidentHyperLink = ObjectFactory.Get<IShowEditFormUrlCreator>().CreateWithSpecifiedLicenceCode(ControllerIDs.ServiceRequest, incident.PK, incident.IA_LicenceCode);
				email.Body = email.Body.Replace(IncidentApprovalLookups.IncidentApprovalLinkMacro, incidentHyperLink);
			}

			foreach (Xsd.CustomerServiceResponseEmailAttachment attachment in responseEmail.Attachments)
			{
				email.Attachments.Add(new AttachmentDef(attachment.FileName, attachment.Data));
			}
		}

		static string GetNoReplyAddress(EmailDef email)
		{
			string result = string.Empty;
			ZString emailToExtractDomain = email.FromAddress;
			if (!emailToExtractDomain.IsEmpty && emailToExtractDomain.Contains('@'))
			{
				int index = emailToExtractDomain.IndexOf('@');
				string domain = emailToExtractDomain.SubstringSafe(index, emailToExtractDomain.Length - index);
				result = "NoReply" + domain;
			}
			return result;
		}

		void AddToEmailRecipients(EmailDef email, string delimitedToEmailAddresses)
		{
			foreach (var address in delimitedToEmailAddresses.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(address => address.Trim()))
			{
				if (!email.Recipients.Contains(address))
				{
					email.AddRecipientForSystemCommunication(address);
				}
			}
		}

		void AddCcRecipients(EmailDef email, string delimitedCcAddresses)
		{
			foreach (var address in delimitedCcAddresses.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(address => address.Trim()))
			{
				if (!email.Recipients.Contains(address) && !email.CCRecipients.Contains(address))
				{
					email.AddRecipientForSystemCommunication(address, RecipientDef.RecipientTypes.CC);
				}
			}
		}

		void AddNotificationRecipients(EmailDef email, string systemReservedEmailAddressesAsText)
		{
			List<string> recipientEmails = new List<string>(3);

			var systemReservedEmailAddresses = systemReservedEmailAddressesAsText.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(address => new ZString(address.Trim()));

			if ((SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.Value == IncidentApprovalLookups.EmailRecipientSettings.ApprovingStaff
				|| SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.Value == IncidentApprovalLookups.EmailRecipientSettings.AllParties)
				&& incident.ApprovingStaff != null && !incident.ApprovingStaff.GS_EmailAddress.IsEmpty
				&& !systemReservedEmailAddresses.Any(address => address.EqualsIgnoringCase(incident.ApprovingStaff.GS_EmailAddress)))
			{
				recipientEmails.Add(incident.ApprovingStaff.GS_EmailAddress);
			}

			if ((SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.Value == IncidentApprovalLookups.EmailRecipientSettings.ReportedByStaff
				|| SystemDataRegistry.Instance.CustomerServiceRequestNotificationRecipientsSetting.Value == IncidentApprovalLookups.EmailRecipientSettings.AllParties)
				&& incident.ReportedByStaff != null && !incident.ReportedByStaff.GS_EmailAddress.IsEmpty
				&& !systemReservedEmailAddresses.Any(address => address.EqualsIgnoringCase(incident.ReportedByStaff.GS_EmailAddress))
				&& !recipientEmails.Contains(incident.ReportedByStaff.GS_EmailAddress))
			{
				recipientEmails.Add(incident.ReportedByStaff.GS_EmailAddress);
			}

			if (incident.ReportingStaff != null && !incident.ReportingStaff.GS_EmailAddress.IsEmpty
				&& !recipientEmails.Contains(incident.ReportingStaff.GS_EmailAddress)
				&& !systemReservedEmailAddresses.Any(address => address.EqualsIgnoringCase(incident.ReportingStaff.GS_EmailAddress)))
			{
				recipientEmails.Add(incident.ReportingStaff.GS_EmailAddress);
			}

			email.AddRecipientForSystemCommunication(recipientEmails.ToArray());
		}

		#endregion

		#region Logging

		void LogWarning(string logMessage)
		{
			if (notifications != null)
			{
				notifications.AddWarning(logMessage);
			}
		}

		void LogError(string logMessage)
		{
			if (notifications != null)
			{
				notifications.AddError(logMessage);
			}
		}

		#endregion

		#region IsValidModuleCode

		static bool IsValidModuleCode(ZString moduleCode, ModuleListType moduleListType)
		{
			switch (moduleListType)
			{
				case ModuleListType.MenuSection:
					return IsValidMenuSectionCode(moduleCode);

				case ModuleListType.Cr8:
					return IsValidCr8Code(moduleCode);

				case ModuleListType.Cr9:
					return IsValidCr9Code(moduleCode);
			}

			return false;
		}

		static bool IsValidMenuSectionCode(ZString moduleCode)
		{
			var mandatoryMenuSectionList = new MandatoryCustomerServiceMenuSectionList();
			if (mandatoryMenuSectionList.ContainsKey(moduleCode))
			{
				return true;
			}

			var moduleTreeMenuSectionList = new ModuleTreeCustomerServiceMenuSectionList();
			if (moduleTreeMenuSectionList.ContainsKey(moduleCode))
			{
				return true;
			}

			return false;
		}

		static bool IsValidCr8Code(ZString moduleCode)
		{
			var cr8Modules = new Cr8ModuleList();
			return cr8Modules.ContainsCode(moduleCode);
		}

		static bool IsValidCr9Code(ZString moduleCode)
		{
			var cr9Modules = new Cr9ModuleList();
			return cr9Modules.ContainsCode(moduleCode);
		}

#if DEBUG
		internal static bool IsValidModuleCode_ExposedForTesting(ZString moduleCode, ModuleListType moduleListType)
		{
			return IsValidModuleCode(moduleCode, moduleListType);
		}
#endif

		#endregion
	}
}
