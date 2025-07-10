using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class UserAgreementHelper
	{
		public static UserAgreementResponseWrapper GetEnterpriseUserAgreementResponse(LicenceEnterprise enterprise, EnterpriseUserAgreementInfo requestInfo, UserAgreementEDocRequestHelper linkHelper, string queryTokenString)
		{
			var responseData = new UserAgreementResponseData()
			{
				Required = false,
				Title = string.Empty,
				Content = string.Empty,
				VersionNumber = 0,
				Level = string.Empty,
				AllowOnlineClickthrough = false
			};

			if (enterprise == null)
			{
				return new UserAgreementResponseWrapper
				{
					ResponseData = responseData,
				};
			}

			EdiUserAgreement availableAgreement = null;
			var factory = enterprise.Factory;
			var agreementInfo = EdiUserAgreement.GetCurrentAgreement(factory, enterprise, requestInfo.UserAgreementType, string.Empty);
			if (agreementInfo.AllowOnlineAcceptance)
			{
				availableAgreement = agreementInfo.Agreement;
				if (availableAgreement != null &&
					availableAgreement.Level.EqualsIgnoringCase(EdiUserAgreementLevelList.Codes.Corporate) &&
					!availableAgreement.HasBeenAcceptedByEnterprise(enterprise))
				{
					var eDoc = GetLatestMLAEDoc(availableAgreement);
					var docLinkUrl = string.Empty;
					if (eDoc != null)
					{
						docLinkUrl = linkHelper.GetHandlerUrl(availableAgreement.PK, eDoc.UniqueKey.ToGuid(), queryTokenString);
					}

					var parser = new DocumentParser<EdiUserAgreementWrapper, DocEdiUserAgreement>(factory);
					var wrapper = new EdiUserAgreementWrapper()
					{
						UserAgreement = availableAgreement,
						UserInfo = requestInfo,
						Database = null,
						GetAgreementDocsURL = docLinkUrl
					};

					responseData.Required = true;
					responseData.AllowOnlineClickthrough = true;
					responseData.Title = parser.Parse(wrapper, availableAgreement.ERA_Title);
					responseData.Content = parser.Parse(wrapper, availableAgreement.ERA_Content);
					responseData.VersionNumber = availableAgreement.ERA_VersionNumber;
					responseData.MinorVersionNumber = availableAgreement.ERA_MinorVersion;
					responseData.Variant = availableAgreement.ERA_VariantCode;
					responseData.Level = availableAgreement.Level;
				}
			}

			return new UserAgreementResponseWrapper
			{
				ResponseData = responseData,
				Agreement = availableAgreement,
			};
		}

		public static UserAgreementResponseWrapper GetEnterpriseUserAgreementResponse(IEnumerable<EdiUserAgreementAssignment> assignments, EnterpriseUserAgreementInfo requestInfo, UserAgreementEDocRequestHelper linkHelper, string queryTokenString)
		{
			var responseData = new UserAgreementResponseData()
			{
				Required = false,
				Title = string.Empty,
				Content = string.Empty,
				VersionNumber = 0,
				Level = string.Empty,
				AllowOnlineClickthrough = false
			};

			if (!assignments.Any())
			{
				return new UserAgreementResponseWrapper
				{
					ResponseData = responseData,
				};
			}

			EdiUserAgreement availableAgreement = null;
			var factory = assignments.First().Factory;
			var agreementInfo = EdiUserAgreement.GetCurrentAgreementInfoForAssignment(assignments.First());
			if (agreementInfo.AllowOnlineAcceptance)
			{
				availableAgreement = agreementInfo.Agreement;
				if (availableAgreement != null &&
					availableAgreement.Level.EqualsIgnoringCase(EdiUserAgreementLevelList.Codes.Corporate) &&
					assignments.Any(x => x.LastAcceptedDateUtc.IsEmpty))
				{
					var eDoc = GetLatestMLAEDoc(availableAgreement);
					var docLinkUrl = string.Empty;
					if (eDoc != null)
					{
						docLinkUrl = linkHelper.GetHandlerUrl(availableAgreement.PK, eDoc.UniqueKey.ToGuid(), queryTokenString);
					}

					var parser = new DocumentParser<EdiUserAgreementWrapper, DocEdiUserAgreement>(factory);
					var wrapper = new EdiUserAgreementWrapper()
					{
						UserAgreement = availableAgreement,
						UserInfo = requestInfo,
						Database = null,
						GetAgreementDocsURL = docLinkUrl
					};

					responseData.Required = true;
					responseData.AllowOnlineClickthrough = true;
					responseData.Title = parser.Parse(wrapper, availableAgreement.ERA_Title);
					responseData.Content = parser.Parse(wrapper, availableAgreement.ERA_Content);
					responseData.VersionNumber = availableAgreement.ERA_VersionNumber;
					responseData.MinorVersionNumber = availableAgreement.ERA_MinorVersion;
					responseData.Variant = availableAgreement.ERA_VariantCode;
					responseData.Level = availableAgreement.Level;
				}
			}

			return new UserAgreementResponseWrapper
			{
				ResponseData = responseData,
				Agreement = availableAgreement,
			};
		}

		public static IeDoc GetLatestMLAEDoc(EdiUserAgreement availableAgreement)
		{
			return availableAgreement.DocManagerInfo.AllEDocs.OfType<IeDoc>().Where(x => x.DocType == "MLA" && x.IsPublished && Path.GetExtension(x.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.DateAdded).FirstOrDefault();
		}

		public static bool AcknowledgeEnterpriseUserAgreement(LicenceEnterprise enterprise, IEnterpriseUserAgreementInfo requestInfo)
		{
			if (enterprise == null)
			{
				return false;
			}

			var factory = enterprise.Factory;
			var agreement = EdiUserAgreement.GetCurrentAgreement(enterprise.Factory, enterprise, requestInfo.UserAgreementType, string.Empty).Agreement;
			if (agreement == null)
			{
				return false;
			}

			var requestDataValidation = new UserAgreementRequestDataValidation(requestInfo);

			if (requestDataValidation.ErrorMessageBuilder.Messages.Count > 0)
			{
				return false;
			}

			var approver = enterprise.Organisation?.Contacts.Cast<OrgContact>().FirstOrDefault(x => x.OC_IsActive && string.Equals(x.OC_Email, requestInfo.Email, StringComparison.OrdinalIgnoreCase));
			if (approver == null)
			{
				approver = enterprise.Organisation.Contacts.AddNew();
				approver.OC_Email = requestInfo.Email;
				approver.OC_ContactName = OrgContactUniqueNameHelper.GenerateUniqueContactName(approver, requestInfo.FullName);
				approver.OC_Title = requestInfo.JobTitle;
				approver.OC_WebAccessEnabled = false;
			}

			AddAgreementLogToAccount(factory, agreement, requestInfo, userAccount: null, database: null, enterprise: enterprise);
			factory.Save();

			if (requestInfo.ShouldSendAgreementCopy && !string.IsNullOrWhiteSpace(requestInfo.Email))
			{
				SendAgreementCopy(enterprise.Factory, new EdiUserAgreementWrapper()
				{
					UserAgreement = agreement,
					UserInfo = requestInfo,
					Database = null,
				});
			}

			return true;
		}

		public static bool AcknowledgeEnterpriseUserAgreement(LicenceDatabase database, IEnterpriseUserAgreementInfo requestInfo)
		{
			if (database == null)
			{
				return false;
			}

			var factory = database.Factory;
			var agreementInfo = EdiUserAgreement.GetCurrentAgreement(database.Factory, database, requestInfo.UserAgreementType, string.Empty);
			if (agreementInfo.Agreement == null)
			{
				return false;
			}

			var requestDataValidation = new UserAgreementRequestDataValidation(requestInfo);

			if (requestDataValidation.ErrorMessageBuilder.Messages.Count > 0)
			{
				return false;
			}

			var approverOrg = agreementInfo.Assignment?.ClientAgreementOrg ?? database.LicEnterprise.Organisation;

			var approver = approverOrg.Contacts.Cast<OrgContact>().FirstOrDefault(x => x.OC_IsActive && string.Equals(x.OC_Email, requestInfo.Email, StringComparison.OrdinalIgnoreCase));
			if (approver == null)
			{
				approver = approverOrg.Contacts.AddNew();
				approver.OC_Email = requestInfo.Email;
				approver.OC_ContactName = OrgContactUniqueNameHelper.GenerateUniqueContactName(approver, requestInfo.FullName);
				approver.OC_Title = requestInfo.JobTitle;
				approver.OC_WebAccessEnabled = false;
			}

			AddAgreementLogToAccount(factory, agreementInfo.Agreement, requestInfo, userAccount: null, database: database, enterprise: null);
			factory.Save();

			if (requestInfo.ShouldSendAgreementCopy && !string.IsNullOrWhiteSpace(requestInfo.Email))
			{
				SendAgreementCopy(database.Factory, new EdiUserAgreementWrapper()
				{
					UserAgreement = agreementInfo.Agreement,
					UserInfo = requestInfo,
					Database = null,
				});
			}

			return true;
		}

		public static void SendAgreementCopy(BusinessObjectFactory factory, EdiUserAgreementWrapper wrapper)
		{
			var parser = new DocumentParser<EdiUserAgreementWrapper, DocEdiUserAgreement>(factory);
			wrapper.AgreementTitle = parser.Parse(wrapper, wrapper.UserAgreement.ERA_Title);

			var docUrl = wrapper.GetAgreementDocsURL;
			wrapper.GetAgreementDocsURL = string.Empty;
			wrapper.AgreementContent = parser.Parse(wrapper, wrapper.UserAgreement.ERA_Content);
			wrapper.GetAgreementDocsURL = docUrl;

			var corporateAgreement = wrapper.UserAgreement.Level.EqualsIgnoringCase(EdiUserAgreementLevelList.Codes.Corporate);
			NotificationEmailTemplate emailTemplate;
			if (corporateAgreement)
			{
				emailTemplate = EDIDataRegistry.Instance.CorporateAgreementAcknowledgementNotificationMessageTemplate.Value;
			}

			else
			{
				emailTemplate = EDIDataRegistry.Instance.UserAgreementAcknowledgementNotificationMessageTemplate.Value;
			}

			var email = new HtmlEmailDef
			{
				FromDisplayName = EDIDataRegistry.Instance.UserAgreementAcknowledgementNotificationEmailSenderName.Value,
				FromAddress = EDIDataRegistry.Instance.UserAgreementAcknowledgementNotificationEmailSenderAddress.Value,
				Subject = parser.Parse(wrapper, emailTemplate.EmailSubject),
				ContentType = EmailContentTypes.HTML
			};

			if (corporateAgreement)
			{
				var publishedEDoc = GetLatestMLAEDoc(wrapper.UserAgreement);
				if (publishedEDoc != null)
				{
					var attachment = new AttachmentDef(publishedEDoc.FileName, publishedEDoc.ImageData);
					email.Attachments.Add(attachment);
				}
				email.LoadHtmlUsingTemplate(parser.Parse(wrapper, emailTemplate.EmailBody));
			}
			else
			{
				email.LoadPlainTextUsingTemplate(parser.Parse(wrapper, emailTemplate.EmailBody));
			}

			email.AddRecipientForUserCommunication(wrapper.UserInfo.Email);
			try
			{
				Env.OutgoingMailManager.CreateAndSave(email);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce("UserAgreementController: Failed to send agreement copy", e.Message, e);
			}
		}

		public static EdiUserAgreementAcceptanceLog AddAgreementLogToAccount(BusinessObjectFactory factory, EdiUserAgreement agreement, IUserAgreementInfo request, EdiCustomerUserAccount userAccount, LicenceDatabase database, LicenceEnterprise enterprise, OrgHeader approverOrgOverride = null)
		{
			var agreementLog = factory.New<EdiUserAgreementAcceptanceLog>();
			using (agreementLog.GetValidationSuspender())
			{
				agreementLog.EUL_ERA = agreement.PK;
				agreementLog.EUL_Type = request.UserAgreementType;
				agreementLog.EUL_AcceptanceTimeUtc = request.AgreementDate ?? ZDateTime.UtcNow;
				agreementLog.EUL_AcceptedByEmail = request.Email;
				agreementLog.EUL_AcceptedByName = request.FullName;
				agreementLog.EUL_AcceptedByIPAddress = request.IPAddress;
				agreementLog.EUL_AcceptedByJobTitle = (request as IEnterpriseUserAgreementInfo)?.JobTitle ?? ZString.Empty;

				agreementLog.EUL_MajorVersion = request.MajorVersion;
				agreementLog.EUL_MinorVersion = request.MinorVersion;
				agreementLog.EUL_VariantCode = request.Variant;

				agreementLog.EUL_LD = database?.PK ?? ZGuid.Empty;
				agreementLog.EUL_LE = enterprise?.PK ?? ZGuid.Empty;
				agreementLog.EUL_EUA = userAccount?.PK ?? ZGuid.Empty;

				if (approverOrgOverride != null)
				{
					agreementLog.EUL_OH = approverOrgOverride.PK;
				}
			}

			return agreementLog;
		}
	}

	public class EnterpriseUserAgreementInfo : UserAgreementInfo, IEnterpriseUserAgreementInfo
	{
		public string JobTitle { get; set; }
		public string ClientAgreementOrgFullName { get; set; }
		public string ClientMainAddressInSingleLine { get; set; }
		public string ClientBusinessRegistration { get; set; }
	}
}
