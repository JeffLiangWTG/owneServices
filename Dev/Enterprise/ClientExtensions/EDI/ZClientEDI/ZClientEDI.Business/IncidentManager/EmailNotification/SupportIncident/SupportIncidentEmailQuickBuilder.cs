using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.EConversation.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public static class SupportIncidentEmailQuickBuilder
	{
		public static (SupportIncidentEmail, bool) CreateAwaitingResponseEmail(SupportIncident incident, IEnumerable<JobConversationMessage> messages, bool isForBroadcast = false)
		{
			var emailContents = new SupportIncidentAwaitingResponseEmailContentBuilder(incident, messages);
			var email = SupportIncidentEmail.New(incident, emailContents, out var isAllowed, shouldAddConfirmResolvedButton: true, shouldUseFollowUpTemplate: false);
			if (email != null)
			{
				email.MarkAsNoNeedSaveToEDocs();
				email.OverrideSignOffNameAndTitleWithCurrentUsersNameAndTitle = false;
				email.ShouldSaveBizOFactoryOnSent = !isForBroadcast;
				email.XAutoResponseSuppressHeaderValue = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
			}

			return (email, isAllowed);
		}

		public static (SupportIncidentEmail, bool) CreateIncidentClosedEmail(SupportIncident incident, bool hasEConversation, ZString dispositionCode, bool displayNameIsSupportButSignedByCurrentUser, bool shouldUseFollowUpTemplate = false)
		{
			var template = new SupportIncidentClosedEmailContentBuilder(incident, hasEConversation, incident.IM_Product, dispositionCode, shouldUseFollowUpTemplate: shouldUseFollowUpTemplate);
			return CreateIncidentClosedOrResolvedEmail(template, incident, displayNameIsSupportButSignedByCurrentUser, shouldUseFollowUpTemplate: shouldUseFollowUpTemplate);
		}

		public static (SupportIncidentEmail, bool) CreateFinalIncidentClosedEmail(SupportIncident incident, bool displayNameIsSupportButSignedByCurrentUser)
		{
			var template = new CustomerServiceFinalClosureAutoReplyEmailContentBuilder(incident, false, incident.IM_Product);
			return CreateIncidentClosedOrResolvedEmail(template, incident, displayNameIsSupportButSignedByCurrentUser, shouldUseFollowUpTemplate: true, shouldAddConfirmResolvedButton: false);
		}

		public static (SupportIncidentEmail, bool) CreateIncidentResolvedEmail(SupportIncident incident, bool hasEConversation, ZString dispositionCode, bool displayNameIsSupportButSignedByCurrentUser)
		{
			var template = new SupportIncidentResolvedEmailContentBuilder(incident, hasEConversation, incident.IM_Product, dispositionCode);

			return CreateIncidentClosedOrResolvedEmail(template, incident, displayNameIsSupportButSignedByCurrentUser, shouldAddConfirmResolvedButton: true);
		}

		static (SupportIncidentEmail, bool) CreateIncidentClosedOrResolvedEmail(BaseEDIRegistryCodeDescriptionEmailTemplate template, SupportIncident incident, bool displayNameIsSupportButSignedByCurrentUser, bool shouldUseFollowUpTemplate = false, bool shouldAddConfirmResolvedButton = false)
		{
			var email = SupportIncidentEmail.New(incident, template, out var isAllowed, shouldUseFollowUpTemplate: shouldUseFollowUpTemplate, shouldAddConfirmResolvedButton: shouldAddConfirmResolvedButton);
			if (email != null)
			{
				email.XAutoResponseSuppressHeaderValue = SupportIncidentEmailHeaderConstants.XAutoResponseSuppressValues.SuppressAll;
				if (displayNameIsSupportButSignedByCurrentUser)
				{
					email.OverrideSignOffNameAndTitleWithCurrentUsersNameAndTitle = true;
				}

				email.PublicLogComment = "Support Incident " + incident.IncidentCloseTypeText + " email sent to contact";
				if (template.BodyTemplate.Contains("(*ResolutionCommentText*)", StringComparison.Ordinal) || template.BodyTemplate.Contains("(*ResolutionNoteText*)", StringComparison.Ordinal))
				{
					email.BodyContainsResolutionMessage = true;
				}
			}

			return (email, isAllowed);
		}
	}
}
