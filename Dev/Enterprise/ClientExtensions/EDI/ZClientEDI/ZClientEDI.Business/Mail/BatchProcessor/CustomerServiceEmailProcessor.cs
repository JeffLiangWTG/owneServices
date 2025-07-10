using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using MimeKit;
using ZClientEDI.Business.IncidentManager.EmailNotification.SupportIncident.Templates;
using ZClientEDI.Business.Mail;

namespace Enterprise.Client.EDI.Mail.Business
{
	public class CustomerServiceEmailProcessor : EDIBusinessObjectEmailProcessor<SupportIncident>
	{
		#region Create / Process

		const string AutoReplySubjectConstants = "Automatic reply,Vacation";

		IDictionary<string, string> AutoReplyHeaderConstants
		{
			get
			{
				Dictionary<string, string> replyStrings = new Dictionary<string, string>();
				replyStrings.Add("X-Autoreply", "yes");
				replyStrings.Add("Auto-Submitted", "auto-replied");
				replyStrings.Add("x-ms-exchange-generated-message-source", "Mailbox Rules Agent");
				return replyStrings;
			}
		}

		protected override bool IgnoreEmail(Email email)
		{
			var message = MimeMessageExtensions.CreateMessageFromEml(email.GetEmlBytes());
			foreach (var entry in AutoReplyHeaderConstants)
			{
				string headerItem = message.Headers[entry.Key];
				if (!string.IsNullOrEmpty(headerItem) && headerItem.Equals(entry.Value, StringComparison.OrdinalIgnoreCase))
				{
					return email.IsEmailProcessingSkipped = true;
				}
			}

			string[] subjects = AutoReplySubjectConstants.Split(',');
			foreach (var entry in subjects)
			{
				if (!string.IsNullOrEmpty(message.Subject) && message.Subject.StartsWith(entry, StringComparison.OrdinalIgnoreCase))
				{
					return email.IsEmailProcessingSkipped = true;
				}
			}

			return base.IgnoreEmail(email);
		}

		protected override bool ProcessMailItem(MailItem mailItem, IEmailProcessorLogger logger)
		{
			SendSupportAutoReplyEmail(mailItem);

			var currentMailItem = mailItem;
			for (var retries = 1; retries <= MaxRetryCounts; ++retries)
			{
				try
				{
					return base.ProcessMailItem(currentMailItem, logger);
				}
				catch (ZSaveException ex)
				{
					logger.Log(LogType.Warning, false, "[{0}]Some errors occurred during the saving: {1}", retries, ex.ToString());
					if (retries == MaxRetryCounts)
					{
						throw;
					}

					System.Threading.Thread.Sleep(500);
					var factory = new BusinessObjectFactory() { RefreshEnabled = false };
					currentMailItem = CopyMailItemWithNewFactory(currentMailItem, factory);
				}
			}

			return false;
		}
		static readonly int MaxRetryCounts = 3;

		MailItem CopyMailItemWithNewFactory(MailItem mailItem, BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(MailItem)).AddToFilter(MailDBItemsSchema.PK, mailItem.PK);
			if (factory.LoadTop1<MailItem>(query) is MailItem mail)
			{
				return mail;
			}

			var newMailItem = factory.New<MailItem>();
			newMailItem.RawMIMEString = mailItem.RawMIMEString;
			newMailItem.MI_Application = mailItem.MI_Application;
			newMailItem.MI_Status = mailItem.MI_Status;
			newMailItem.MI_Direction = mailItem.MI_Direction;
			return newMailItem;
		}

		protected override bool ShouldAttachEmailAndSave(MailItem mailItem, SupportIncident incident)
		{
			return true; //Customer service email should be always processed and deleted regardless validation result
		}

		protected override void AttachEmailToAnExistingBusinessObject(MailItem mailItem, SupportIncident incident, IEmailProcessorLogger logger)
		{
			if (ValidateEmail(mailItem, incident, logger))
			{
				var fileNumberBeforeAttach = incident.DocManagerInfo.EDocsView.Count;
				base.AttachEmailToAnExistingBusinessObject(mailItem, incident, logger);

				var sender = mailItem.GetFromEmailAddress();
				if (fileNumberBeforeAttach != incident.DocManagerInfo.EDocsView.Count && EDIDataRegistry.Instance.EmailAddressBlockList.Value.Contains(sender, StringComparer.OrdinalIgnoreCase))
				{
					var eDocMailName = BusinessObjectEmailAttacher.GenerateLegalFileName(mailItem.MI_Subject);
					var attachedFile = incident.DocManagerInfo.EDocsView.GetMostRecentEDoc(DocType);
					if (attachedFile != null && eDocMailName == attachedFile.FileName)
					{
						attachedFile.IsPublished = false;
					}

					logger.Log(LogType.Information, verboseModeOnly: false, $"`{mailItem.MI_Subject}` was only add into the eDoc of {incident.IM_IncidentNumber}, because the email from {sender}.");
					return;
				}

				var emailAddressFromRegistry = EDIDataRegistry.Instance.IncidentFromEmailAddress.Value;
				var mailItemAsEmail = new Email((byte[])mailItem.RawEmailBytes);

				BusinessObjectEConversationAttacher eConvoAttacher;
				if (mailItemAsEmail.SenderAddress.Equals(emailAddressFromRegistry, StringComparison.OrdinalIgnoreCase))
				{
					eConvoAttacher = new EConvesationAttacherWithNoSubscription();
				}
				else
				{
					eConvoAttacher = new CustomerServiceEConversationAttacher();
				}

				var newMessage = eConvoAttacher.AttachEmailToJobConversation(mailItemAsEmail, DocType, incident);

				incident.ManagementGroup?.ProcessIncidentMessageReceived(incident);
				incident.EnsureOpenTask(isLegacyReopenRequest: false, isByNewMessage: false, isByNewEmail: true);

				if (SupportIncidentEmailTriggeringRules.IsAllSuppressed(incident))
				{
					logger.Log(LogType.Information, verboseModeOnly: false, $"Email({mailItem.MI_Subject}) from {mailItem.MI_From} will be added to eDoc, but CSE will not send replies because incident notification is disabled");
					return;
				}

				var shouldSendReply = !this.IsReceivedEmailsCountOutOfLimit(mailItem);
				if (shouldSendReply)
				{
					if (newMessage?.Sender?.IsEmailParticipant ?? false)
					{
						SupportIncidentUpdateNotificationEmail.SendEmailToAssignedStaffAndOtherSubscribers(incident, incident.OriginalSnapShot, new List<IConversationMessage> { newMessage }, shouldSaveEmail: false);
					}
					incident.IncidentManagementLink?.TrySendGroupAutoReply();
					SendFinalClosureAutoReplyEmail(mailItem, incident);
				}
				else
				{
					var message = string.Empty;
					if (SupportIncidentEmailTriggeringRules.SuppressAll(incident))
					{
						message = $"Email({mailItem.MI_Subject}) from {mailItem.MI_From} exceeds the maximum number of mail received (more than {EDIDataRegistry.Instance.EmailLoopMaximumEmailCount.Value} emails in {EDIDataRegistry.Instance.EmailLoopDetectingWindowDuration.Value} minutes). {incident.Number}'s notification is disabled.";
					}
					else
					{
						message = $"Email({mailItem.MI_Subject}) from {mailItem.MI_From} exceeds the maximum number of mail received (more than {EDIDataRegistry.Instance.EmailLoopMaximumEmailCount.Value} emails in {EDIDataRegistry.Instance.EmailLoopDetectingWindowDuration.Value} minutes). {incident.Number}'s notification was not disabled because of missing ALL tag.";
					}

					ErrorReporter.ReportOnce("Possible mail loop detected", message);
					logger.Log(LogType.Warning, verboseModeOnly: false, message);
				}
			}
		}

		void SendFinalClosureAutoReplyEmail(MailItem mailItem, SupportIncident incident)
		{
			if (mailItem.MI_From != EDIDataRegistry.Instance.CustomerServiceMailBox.UserName && incident.IsClosedOrCancelled && incident.GetClosedReopenRule() == ResolutionAndClosureBehaviour.Constants.Code.NeverAllow)
			{
				var contentBuilder = new CustomerServiceFinalClosureAutoReplyEmailContentBuilder(incident, false, incident.IM_Product);
				var email = SupportIncidentEmail.New(incident, contentBuilder, out var isAllowed, shouldUseFollowUpTemplate: true);
				if (isAllowed)
				{
					email.XAutoResponseSuppressHeaderValue = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
					email.ToEmailAddress = mailItem.MI_From;
					email.SendEmail();
				}
			}
		}

		#region ValidEmail

		#region log message

		const string ValidateEmailWithUniqueBodyFail = "Email doesn't have displayed token (uniqueId).";

		const string ValidateEmailWithSenderSuccess = "Sender email address {0} is in the valid list.";

		const string ValidateEmailWithSenderFail = "Sender email address {0} is not in the valid list.";

		const string ValidEmailWithUniqueIdInHtmlTagFail = "Email doesn't have hidden token (uniqueId).";

		const string ValidateEmailFail = "{0}\r\nEmail is not attached to {1}.";

		const string ValidateEmailWithMarkSuccess = "Email ID {0} matches original ID {1}.";

		const string ValidateEmailWithMarkFail = "Email ID {0} doesn't match original ID {1}.";

		#endregion

		Func<MailItem, SupportIncident, StringBuilder, bool>[] ValidationEmailMethods => new Func<MailItem, SupportIncident, StringBuilder, bool>[]
		{
			ValidateEmailWithSender,
			ValidEmailWithUniqueIDInHtmlTag,
			ValidateEmailWithUniqueBody
		};

		bool ValidateEmailWithUniqueBody(MailItem mailItem, SupportIncident incident, StringBuilder messageBuilder)
		{
			var uniqueIdBody = CustomerServiceEmailUniqueIdUtil.GetUniqueIDFromBody(incident, mailItem);
			if (string.IsNullOrEmpty(uniqueIdBody))
			{
				messageBuilder.AppendLine(ValidateEmailWithUniqueBodyFail);
				return false;
			}

			return ValidateEmailWithMark(uniqueIdBody, incident, messageBuilder);
		}

		bool ValidateEmailWithSender(MailItem mailItem, SupportIncident supportIncident, StringBuilder messageBuilder)
		{
			var sender = mailItem.GetFromEmailAddress();
			var activeOrgSenderQuery = new ZQuery();
			activeOrgSenderQuery.AddToFilter(OrgContactSchema.OC_Email, sender);
			activeOrgSenderQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
			activeOrgSenderQuery.AddToFilter(OrgContactSchema.OC_OH, supportIncident.IM_OH_Client);
			var senderIsWTGStaffQuery = new ZQuery(GlbStaffSchema.GS_EmailAddress, sender).AddToFilter(GlbStaffSchema.GS_IsActive, true);

			var result = supportIncident.EConversation.Conversation.Participants.Any(x => x.EmailAddress.EqualsIgnoringCase(sender)) ||
				((supportIncident.Client?.OH_IsActive ?? false) && supportIncident.Factory.ExistsInDatabase(OrgContactSchema.Constants.TableName, activeOrgSenderQuery)) ||
				supportIncident.Factory.ExistsInDatabase(GlbStaffSchema.Constants.TableName, senderIsWTGStaffQuery);
			if (!result)
			{
				messageBuilder.AppendLine(string.Format(ValidateEmailWithSenderFail, sender));
			}
			else
			{
				messageBuilder.AppendLine(string.Format(ValidateEmailWithSenderSuccess, sender));
			}
			return result;
		}

		bool ValidEmailWithUniqueIDInHtmlTag(MailItem mailItem, SupportIncident incident, StringBuilder messageBuilder)
		{
			var uniqueId = CustomerServiceEmailUniqueIdUtil.GetUniqueID(mailItem);
			if (string.IsNullOrEmpty(uniqueId))
			{
				messageBuilder.AppendLine(ValidEmailWithUniqueIdInHtmlTagFail);
				return false;
			}

			return ValidateEmailWithMark(uniqueId, incident, messageBuilder);
		}

		protected virtual bool ValidateEmail(MailItem mailItem, SupportIncident incident, IEmailProcessorLogger logger)
		{
			var result = false;
			var messageBuilder = new StringBuilder();
			foreach (var validator in ValidationEmailMethods)
			{
				result = validator(mailItem, incident, messageBuilder);
				if (result)
				{
					logger.Log(LogType.Information, verboseModeOnly: false, messageBuilder.ToString());
					return true;
				}
			}

			logger.Log(LogType.Information, verboseModeOnly: false, string.Format(ValidateEmailFail, messageBuilder, incident.IM_IncidentNumber));
			return result;
		}

		bool ValidateEmailWithMark(string originalUniqueId, SupportIncident supportIncident, StringBuilder message)
		{
			var uniqueEmailId = CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(supportIncident);
			if (originalUniqueId.Contains(uniqueEmailId))
			{
				message.AppendLine(string.Format(ValidateEmailWithMarkSuccess, uniqueEmailId, originalUniqueId));
				return true;
			}

			message.AppendLine(string.Format(ValidateEmailWithMarkFail, uniqueEmailId, originalUniqueId));
			return false;
		}

		#endregion

		void SendSupportAutoReplyEmail(MailItem customServiceEmail)
		{
			if (!string.IsNullOrEmpty(EDIDataRegistry.Instance.CustomerSupportAutoReplyEmailTemplate))
			{
				if (this.IsReceivedEmailsCountOutOfLimit(customServiceEmail))
				{
					return;
				}

				string recipient = !customServiceEmail.MI_ReplyTo.IsEmpty ? customServiceEmail.MI_ReplyTo : customServiceEmail.MI_From;
				if (MailboxAddress.TryParse(recipient, out var recipientMailBox))
				{
					if (recipientMailBox.Address.Equals(SupportIncidentLookups.SupportEmailAddress, StringComparison.CurrentCultureIgnoreCase))
					{
						return;
					}

					var template = new CustomerSupportAutoReplyEmailContentBuilder(customServiceEmail);

					var incident = LoadFromIdentifier(customServiceEmail.Factory, GetBusinessObjectIdentifierFromSubject(customServiceEmail.MI_Subject) ?? string.Empty);
					var email = EDIEmailBuilder.GetInstance(incident).BuildHtmlEmailDefByTemplate(template);
					if (email != null)
					{
						email.AddRecipientForUserCommunication(recipient);
						email.FromAddress = IncidentConstants.ManagerEmailAddress;
						email.FromDisplayName = "ediProd System";
						email.Body = RequestHyperlinkButtonChecker.CheckAndGetEmailBodyWithButton(incident, email.Body);
						Env.OutgoingMailManager.CreateAndSave(email);
					}
				}
			}
		}

		#endregion

		public override string EmailTypeName
		{
			get { return "Support"; }
		}

		public override string MailApplicationCode
		{
			get { return EDIMailApplication.CustomerService; }
		}

		protected override string DocType => "COR";

		internal override string GetBusinessObjectIdentifierFromSubject(string subject)
		{
			return GetIncidentNumberFromSubject(subject);
		}

		protected override SupportIncident LoadFromIdentifier(BusinessObjectFactory factory, string identifier)
		{
			return factory.LoadFromNaturalKey<SupportIncident>(IncidentMainSchema.IM_IncidentNumber, identifier);
		}

		internal string GetIncidentNumberFromSubject(ZString subject)
		{
			var incidentNumber = ZString.Empty;

			if (subject.Contains(ClientNumberFountainRegistration.CustomerServiceIncidentNoPrefix, StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					var pattern = new Regex(@"\b[cC][sS][0-9]+\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));
					incidentNumber = pattern.Match(subject).Value;
					incidentNumber = Regex.Replace(incidentNumber, "CS", "", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));

					if (!incidentNumber.IsEmpty && incidentNumber.IsNumbersOnlyOrEmpty && incidentNumber.Length == 8)
					{
						incidentNumber = ClientNumberFountainRegistration.CustomerServiceIncidentNoPrefix + incidentNumber;
					}
					else
					{
						incidentNumber = "";
					}
				}
				catch (RegexMatchTimeoutException e)
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($"Regex Timeout Error while processing email subject. Pattern: {e.Pattern} Input: {e.Input.Substring(0, 100)}"), e);
				}
			}

			return incidentNumber;
		}

		internal bool AttachEmailToIncidentForTest(MailItem mailItem, SupportIncident incident, IEmailProcessorLogger logger)
		{
			return AttachEmailCore(mailItem, incident, logger);
		}
	}
}
