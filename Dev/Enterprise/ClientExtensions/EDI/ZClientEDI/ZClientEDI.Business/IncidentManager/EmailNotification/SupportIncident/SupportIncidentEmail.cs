using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentEmail : CustomerServiceEmail
	{
		readonly bool shouldUseFollowUpTemplate;
		readonly bool shouldAddConfirmResolvedButton;

		static readonly List<ZString> NeedSendNotificationCode = new List<ZString>
		{
			SupportIncidentEmailTemplateConstants.Codes.AwaitingResponseNotificationMessage,
			SupportIncidentEmailTemplateConstants.Codes.IncidentClosedNotificationEmail,
			SupportIncidentEmailTemplateConstants.Codes.IncidentResolvedNotificationEmail,
			SupportIncidentEmailTemplateConstants.Codes.CriticalityChangedEmail,
			SupportIncidentEmailTemplateConstants.Codes.WorkItemCreatedEmail,
			SupportIncidentEmailTemplateConstants.Codes.WorkItemCompletedEmail,
		};

		protected SupportIncidentEmail(SupportIncident incident, IEDIEmailTemplateBuilder template = null, bool shouldUseFollowUpTemplate = false, bool shouldAddConfirmResolvedButton = false)
			: base(incident)
		{
			this.TemplateBuilder = template ?? new DefaultNotificationEmailTemplate();
			this.shouldUseFollowUpTemplate = shouldUseFollowUpTemplate;
			this.shouldAddConfirmResolvedButton = shouldAddConfirmResolvedButton;

			// We know the email addresses are valid
			using (GetValidationSuspender())
			{
#pragma warning disable CS0618 // Obsolete set by constructor
				Client = incident.Client;
#pragma warning restore CS0618 // Obsolete set by constructor
				Contact = incident.Contact;

				var convo = incident.EConversation.ExistingConversation;
				if (convo != null)
				{
					const int MaxAddresses = 120;

					var otherActiveContactEmails = convo.Participants
						.Where(x => x.JCP_IsSubscribed
							&& (x.JCP_ParticipantTableCode == OrgContactSchema.Constants.Prefix || x.JCP_ParticipantTableCode == OrgHeaderSchema.Constants.Prefix)
							&& x.Parent.IsActive
							&& !x.Parent.Email.IsEmpty
							&& x.Parent.Email != Contact.Email)
						.Select(x => x.Parent.Email)
						.Distinct()
						.OrderBy(x => x)
						.Take(MaxAddresses)
						.ToList();
					var builder = new StringBuilder();
					var maxLength = CcInfo.MaxLength;
					foreach (var email in otherActiveContactEmails)
					{
						if (builder.Length + email.Length + 1 > maxLength)
						{
							break;
						}
						if (builder.Length > 0)
						{
							builder.Append(';');
						}
						builder.Append(email);
					}

					Cc = builder.ToString();
				}
			}

			this.Body = TemplateBuilder.BuildBody();
			this.Subject = TemplateBuilder.BuildSubject();
		}

		public static SupportIncidentEmail New(SupportIncident incident, IEDIEmailTemplateBuilder template, out bool isAllowed, bool shouldUseFollowUpTemplate = false, bool shouldAddConfirmResolvedButton = false)
		{
			isAllowed = true;
			if (template != null && !template.IsEmpty)
			{
				if (incident != null && (incident as IEDIEmailTriggeringRulesProvider).TriggeringRules.Allow(template))
				{
					return new SupportIncidentEmail(incident, template, shouldUseFollowUpTemplate, shouldAddConfirmResolvedButton);
				}
				isAllowed = false;
				EDIEmailBuilder.AddLogIfGenerationFailed(incident, template);
			}
			return null;
		}

		public static SupportIncidentEmail New(SupportIncident incident)
		{
			return new SupportIncidentEmail(incident);
		}

		public IEDIEmailTemplateBuilder TemplateBuilder { get; }
		IEDIEmailTriggeringRulesProvider EmailTriggeringRulesProvider => BusinessObjectSendingEmail;

		[Obsolete("set by constructor")]
		public new OrgHeader Client
		{
			get { return base.Client; }
			set { base.Client = value; }
		}

		public string PublicLogComment { get; set; }
		public string InternalLogComment { get; set; }
		public IeDoc eDoc { get; set; }

		public bool ShouldAppendNewCustomerMessages { get; set; } = true;

		/// <summary>
		/// Resolution message need special processing to stop it appearing twice in the email.
		/// It is added to the incident as ResolutionNoteText and also as a conversation message.
		/// </summary>
		public bool BodyContainsResolutionMessage { get; set; }

		/// <summary>
		/// Indicates user gets to preview and edit the email.
		/// While true, the email will not send, since send is typically handled in IIncidentCustomerNotificationSender.
		/// Set to true. Run the edit form on the email. If the user attempted to send the email, then UserCanEdit will be set to false.
		/// Then call SendEmail to send via standard email, or GetEmail for the caller to handle sending.
		/// </summary>
		public bool UserCanEdit { get; set; }

		#region Headers

		public string XAutoResponseSuppressHeaderValue { get; set; }

		#endregion

		public new SupportIncident BusinessObjectSendingEmail
		{
			get { return (SupportIncident)base.BusinessObjectSendingEmail; }
		}

		protected override void AddEvent(string emailRecipients)
		{
			if (!string.IsNullOrWhiteSpace(PublicLogComment))
			{
				BusinessObjectSendingEmail.AddPublicSystemLogMessage(PublicLogComment);
				BusinessObjectSendingEmail.IM_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			}
			if (!string.IsNullOrWhiteSpace(InternalLogComment))
			{
				BusinessObjectSendingEmail.AddInternalSystemLogMessage(InternalLogComment);
				BusinessObjectSendingEmail.IM_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			}

			base.AddEvent(emailRecipients);
		}

		protected override void PreSendEmail()
		{
			if (!UserCanEdit && EmailTriggeringRulesProvider.TriggeringRules.Allow(TemplateBuilder))
			{
				base.PreSendEmail();
			}
		}

		protected override void SendEmailCore(bool systemCommunication = false)
		{
			if (!UserCanEdit && EmailTriggeringRulesProvider.TriggeringRules.Allow(TemplateBuilder))
			{
				base.SendEmailCore(systemCommunication);
				var emailCode = this.TemplateBuilder?.GetIEDIEmailTemplate()?.TemplateCode ?? ZString.Empty;
				if (NeedSendNotificationCode.Contains(emailCode))
				{
					var excludeEmails = this?.AllRecipients.ToList();
					BusinessObjectSendingEmail.EConversation.ExistingConversation.Staff.ToArray().ForEach(x => excludeEmails.Add(x.EmailAddress));
					SupportIncidentUpdateNotificationEmail.SendEmailToAssignedStaffAndOtherSubscribers(BusinessObjectSendingEmail, BusinessObjectSendingEmail.OriginalSnapShot, BusinessObjectSendingEmail.EConversation.GetNewLocalPublishedCustomerMessages(), shouldSaveEmail: false, excludeEmails: excludeEmails, shouldTriggerSubscribe: true);
				}
			}
		}

		protected override void PostSendEmail()
		{
			if (!UserCanEdit && EmailTriggeringRulesProvider.TriggeringRules.Allow(TemplateBuilder))
			{
				base.PostSendEmail();
			}
			else
			{
				UserCanEdit = false;
			}
		}

		protected override EmailDef GetEmailCore()
		{
			var email = base.GetEmailCore();
			if (eDoc != null)
			{
				email.Attachments.Add(new AttachmentDef(eDoc.FileName, eDoc.ImageData));
			}

			CustomerServiceEmailUniqueIdUtil.AppendMark(email, BusinessObjectSendingEmail);
			SetEmailHeader(email);
			return email;
		}

		//When adding new headers, Enterprise/Product/Core/EConversation/Business/Notifications/EConversationEmailBuilder
		//will need to be updated as well for eConversation update Emails
		void SetEmailHeader(EmailDef email)
		{
			if (!string.IsNullOrEmpty(XAutoResponseSuppressHeaderValue))
			{
				email.Headers[SupportIncidentEmailHeaderConstants.Headers.XAutoResponseSuppress] = XAutoResponseSuppressHeaderValue;
			}
		}

		public override ZString GetFooterHyperlink()
		{
			var result = ZString.Empty;

			if (RequestHyperlinkButtonChecker.CanAddButton(BusinessObjectSendingEmail, Body))
			{
				if (shouldUseFollowUpTemplate)
				{
					result = RequestHyperlinkButtonChecker.GetCreateFollowUpERequestButton(BusinessObjectSendingEmail);
				}
				else
				{
					result = RequestHyperlinkButtonChecker.GetActionButtons(BusinessObjectSendingEmail, shouldAddConfirmResolvedButton);
				}
			}

			return result;
		}

		/// <summary>
		/// Used when the email body is extracted and sent via eHub, to do the normal post send processing (i.e., add a send event and add the body to eDocs)
		/// </summary>
		/// <param name="emailBody"></param>
		public void DoAddEventAndPostSend(ZString emailBody)
		{
			if (ShouldAddNoteAndEvent)
			{
				AddNoteAndEvent(emailBody);
			}
			PostSendEmail();
		}
	}
}
