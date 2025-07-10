using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Email.Html;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalDataBuss.Management
{
	public class ImportResultNotifier
	{
		readonly IEDIMessage message;
		readonly IXmlSessionTracker logger;
		readonly Guid messageCompanyPK;

		public ImportResultNotifier(IEDIMessage message, IXmlSessionTracker logger)
		{
			this.message = Argument.NotNull(message, "message");
			this.logger = Argument.NotNull(logger, "logger");
			messageCompanyPK = GetMessageCompany(message);
		}

		static Guid GetMessageCompany(IEDIMessage message)
		{
			if (message.EM_GB.IsEmpty)
			{
				return Guid.Empty;
			}

			var messageBranch = message.Factory.Load<IGlbBranch>(message.EM_GB);
			if (messageBranch == null)
			{
				return Guid.Empty;
			}

			return messageBranch.GB_GC.ToGuid();
		}

		public void Notify()
		{
			var explanationBuilder = new StringBuilder();

			if (message.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Internal)
			{
				return;
			}

			var emailsToNotify = new List<string>();
			var isUniversalEvent = message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent;

			switch (message.EM_Status)
			{
				case EDIMessageStatusList.Codes.Rejected:
					{
						var group = !isUniversalEvent
							? eServicesRegistry.Instance.ImportRejectedNotificationGroup
							: eServicesRegistry.Instance.ImportEventRejectedNotificationGroup;
						var groupPK = GetGroupPKFromRegistryItem(group);

						emailsToNotify.AddRange(new EmailGroupUtility().GetGroupEmailCollection(groupPK, false).Cast<string>());
						NotifyGroupBasedConfiguration(groupPK, group.Caption, explanationBuilder);
						break;
					}
				case EDIMessageStatusList.Codes.Discarded when !isUniversalEvent:
					{
						var groupPK = GetGroupPKFromRegistryItem(eServicesRegistry.Instance.ImportDiscardedNotificationGroup);

						emailsToNotify.AddRange(new EmailGroupUtility().GetGroupEmailCollection(groupPK, false).Cast<string>());
						NotifyGroupBasedConfiguration(groupPK, eServicesRegistry.Instance.ImportDiscardedNotificationGroup.Caption, explanationBuilder);
						break;
					}
				default:
					{
						(InboundMessageNotificationsRule rule, string caption) ruleAndCaption = default;
						switch (message.EM_Status)
						{
							case EDIMessageStatusList.Codes.Error:
								ruleAndCaption = GetNotificationRuleAndCaptionFromRegistryItem(
									isUniversalEvent
										? eServicesRegistry.Instance.ImportEventWithErrorsNotificationConfiguration
										: eServicesRegistry.Instance.ImportWithErrorsNotificationConfiguration);
								break;
							case EDIMessageStatusList.Codes.Warning when !isUniversalEvent:
								ruleAndCaption = GetNotificationRuleAndCaptionFromRegistryItem(eServicesRegistry.Instance.ImportWithWarningNotificationConfiguration);
								break;
							case EDIMessageStatusList.Codes.ProcessedOK when !isUniversalEvent:
								ruleAndCaption = GetNotificationRuleAndCaptionFromRegistryItem(eServicesRegistry.Instance.ImportOKNotificationConfiguration);
								break;
						}

						if (ruleAndCaption.rule != null)
						{
							emailsToNotify.AddRange(GetEmailsToNotify(ruleAndCaption.rule, ruleAndCaption.caption, out var explanation));
							explanationBuilder.Append(explanation);
						}

						break;
					}
			}

			if (emailsToNotify.Count > 0)
			{
				SendEmail(emailsToNotify.ToArray(), explanationBuilder.ToString());
			}
		}

		Guid GetGroupPKFromRegistryItem(GuidRegistryItem registryItem)
		{
			var groupPK = Guid.Empty;
			if (messageCompanyPK != Guid.Empty)
			{
				groupPK = registryItem.GetValueWithoutFallback(messageCompanyPK, Guid.Empty, Guid.Empty);
			}
			if (groupPK == Guid.Empty)
			{
				groupPK = registryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}

			return groupPK;
		}

		(InboundMessageNotificationsRule rule, string caption) GetNotificationRuleAndCaptionFromRegistryItem(InboundMessageNotificationsRegistryItem registryItem)
		{
			InboundMessageNotificationsRule notificationRule = null;

			if (messageCompanyPK != Guid.Empty)
			{
				notificationRule = registryItem.GetValueWithoutFallback(messageCompanyPK, Guid.Empty, Guid.Empty);
			}

			if (notificationRule == null || notificationRule.IsEmpty)
			{
				notificationRule = registryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			}

			return (notificationRule, registryItem.Caption);
		}

		protected void SendEmail(string[] emails, string explanation)
		{
			string statusDescription = GetCodeDescriptionPairDesciption(new EDIMessageStatusList(), message.EM_Status);

			string subject = Res.GetString("0B7471CD-E597-45C8-8B91-84621A94522E", "{0} {1} ({2}) – Message # {3} from [{4}]",
				GetCodeDescriptionPairDesciption(new EDIMessageSubTypeList(), message.EM_MessageSubType),
				statusDescription,
				message.EM_Status,
				message.EM_MessageNum,
				message.SenderId);

			string messageLinkUrl = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Messaging.EDIMessage, message.PK.ToGuid());
			string messageLinkLabel = Res.GetString("928e9620-ca63-4d9d-b806-38583d71391d", "Message #");
			string messageStatusLabel = Res.GetString("bd1e1900-ed75-4909-af04-27b9b6d3448b", "Status:");
			string dataImportLogLabel = Res.GetString("3f01657f-82bf-468a-b8cd-7b7f203a2e2a", "Data Import Log");

			var detailsBuilder = new StringBuilder();
			detailsBuilder.Append(((SimpleLogger)logger).ToHtml(message.EM_MessageSubType != EDIMessageSubTypeList.Codes.XmlUniversalEvent));
			if (message.EM_Status != EDIMessageStatusList.Codes.ProcessedOK)
			{
				PrepareGeneralInformation(detailsBuilder);
			}

			var footerBuilder = new StringBuilder();
			PrepareRecipientInformation(footerBuilder, explanation);

			var bodyBuilder = new StringBuilder();
			bodyBuilder.Append(EmailHtmlTags.Paragraph_Start).Append("<a href='").Append(messageLinkUrl).Append("'>").Append(EmailHtmlTags.B_Start).Append(messageLinkLabel).Append(EmailHtmlTags.B_End).Append(" ").Append(message.EM_MessageNum).Append("</a>").Append(EmailHtmlTags.Paragraph_End);
			bodyBuilder.Append(EmailHtmlTags.Paragraph_Start).Append(EmailHtmlTags.B_Start).Append(messageStatusLabel).Append(EmailHtmlTags.B_End).Append(" ").Append(message.EM_Status).Append(" - ").Append(statusDescription).Append(EmailHtmlTags.Paragraph_End);
			bodyBuilder.Append(EmailHtmlTags.Paragraph_Start).Append(EmailHtmlTags.B_Start).Append(dataImportLogLabel).Append(EmailHtmlTags.B_End).Append(EmailHtmlTags.Paragraph_End);
			bodyBuilder.Append(EmailHtmlTags.Paragraph_Start).Append(detailsBuilder).Append(EmailHtmlTags.Paragraph_End);
			bodyBuilder.Append(EmailHtmlTags.Paragraph_Start).Append(footerBuilder).Append(EmailHtmlTags.Paragraph_End);

			var notificationEmail = new HtmlEmailDef();
			notificationEmail.AddRecipientForUserCommunication(emails);
			notificationEmail.Subject = subject;
			notificationEmail.LoadHtmlUsingTemplate(bodyBuilder.ToString());

			var manager = logger.NotificationEmailManager;

			if (manager != null)
			{
				notificationEmail = manager.Process(message, notificationEmail);
				manager.Clear();
			}

			Env.OutgoingMailManager.Create(message.Factory, notificationEmail);
		}

		#region implementation

		List<string> GetEmailsToNotify(InboundMessageNotificationsRule notificationRule, string registryCaption, out string explanation)
		{
			if (notificationRule == null)
			{
				throw new ArgumentNullException(nameof(notificationRule), "The notification rule cannot be null.");
			}

			var explanationBuilder = new StringBuilder();

			var errorEmailsToNotify = new List<string>();

			if (notificationRule.NotifyUserType == InboundMessageNotificationsNotifyUserTypeList.Codes.LastUserEditing)
			{
				var lastEditUser = message.Factory.LoadFromNaturalKey<IGlbStaff>(GlbStaffSchema.GS_Code, message.EM_SystemLastEditUser);
				if (lastEditUser != null && !lastEditUser.GS_EmailAddress.IsEmpty)
				{
					errorEmailsToNotify.Add(lastEditUser.GS_EmailAddress.ToString());
					explanationBuilder.Append(EmailHtmlTags.LI_Start);
					explanationBuilder.Append(Res.GetString("9C9E79DE-DE55-4706-8C6B-B2C95E2BBA79", "The registry item '{0}' is configured to notify the 'Last Editor' for this {1} System.", registryCaption, Core.Constants.ProductName));
					explanationBuilder.Append(EmailHtmlTags.LI_End);
				}
			}

			if (notificationRule.NotifyUserType == InboundMessageNotificationsNotifyUserTypeList.Codes.AllUsersEditing)
			{
				var logs = ((EnterpriseBusinessObject)message).Logs.Find(GetUsersFilter());
				if (logs != null && logs.Length > 0)
				{
					var filteredLog = logs.Where(l => l.User != null && !l.User.GS_EmailAddress.IsEmpty).Select(l => l.User.GS_EmailAddress.ToString()).ToArray();
					errorEmailsToNotify.AddRange(filteredLog);
					explanationBuilder.Append(EmailHtmlTags.LI_Start);
					explanationBuilder.Append(Res.GetString("9C9E79DE-DE55-4706-8C6B-B2C95E2BBA80", "The registry item '{0}' is configured to notify 'All Editors' for this {1} System.", registryCaption, Core.Constants.ProductName));
					explanationBuilder.Append(EmailHtmlTags.LI_End);
				}
			}

			if (errorEmailsToNotify.Count == 0 || (errorEmailsToNotify.Count > 0 && notificationRule.NotifyGroupWhenUserFound))
			{
				if (!notificationRule.NotifyGroup.IsEmpty)
				{
					var emails = new EmailGroupUtility().GetGroupEmailCollection(notificationRule.NotifyGroup.ToGuid(), false).Cast<string>();

					if (emails.Any())
					{
						errorEmailsToNotify.AddRange(emails);
						NotifyGroupBasedConfiguration(notificationRule.NotifyGroup, registryCaption, explanationBuilder);
					}
				}
			}

			explanation = explanationBuilder.ToString();
			return errorEmailsToNotify;
		}

		void NotifyGroupBasedConfiguration(ZGuid notifyGroupPK, string registryCaption, StringBuilder explanationBuilder)
		{
			var group = message.Factory.Load<IGlbGroup>(notifyGroupPK);
			var groupCode = group != null ? group.GG_Code + " - " + group.GG_Desc : string.Empty;
			explanationBuilder.Append(EmailHtmlTags.LI_Start);
			explanationBuilder.Append(Res.GetString("9C9E79DE-DE55-4706-8C6B-B2C95E2BBA78", "The registry item '{0}' is configured to notify the group '{1}' which you are a member of in this {2} System.", registryCaption, groupCode, Core.Constants.ProductName));
			explanationBuilder.Append(EmailHtmlTags.LI_End);
		}

		void PrepareRecipientInformation(StringBuilder bodyBuilder, string explanation)
		{
			bodyBuilder.Append(EmailHtmlTags.BR + EmailHtmlTags.BR + EmailHtmlTags.B_Start);
			bodyBuilder.Append(Res.GetString("9877531A-E404-4916-AC75-02630E496535", "Notes"));
			bodyBuilder.Append(EmailHtmlTags.B_End + EmailHtmlTags.BR);
			bodyBuilder.Append(Res.GetString("9877531A-E404-4916-AC75-02630E496536", "You are receiving this Email because:"));
			bodyBuilder.Append(EmailHtmlTags.BR + EmailHtmlTags.UL_Start);
			bodyBuilder.Append(explanation);
			bodyBuilder.Append(EmailHtmlTags.UL_End);
			bodyBuilder.Append(Res.GetString("9877531A-E404-4916-AC75-02630E496537", "If you are not supposed to receive this email, please contact your system administrator to change the corresponding notification group setting."));
		}

		void PrepareGeneralInformation(StringBuilder bodyBuilder)
		{
			bodyBuilder.Append(EmailHtmlTags.BR + EmailHtmlTags.BR);
			bodyBuilder.Append(Res.GetString("9877531A-E404-4916-AC75-02630E496538", "Please check message log for details. If you are unable to solve this problem, please forward this email to the CargoWise support team."));
		}

		static string GetCodeDescriptionPairDesciption(CodeDescriptionPairList pairs, string code)
		{
			if (!string.IsNullOrWhiteSpace(code) && pairs.IndexOfCode(code) != -1)
			{
				return pairs[code].Description;
			}

			return "";
		}

		static ZQuery GetUsersFilter()
		{
			var findEditUserQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecordCode);
			findEditUserQuery.AddToFilter(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystemCode), JoinCondition.Or);
			findEditUserQuery.OrderBy = StmALogSchema.SL_EventTime.Name + OrderByClause.Descending;
			return findEditUserQuery;
		}

		#endregion
	}
}
