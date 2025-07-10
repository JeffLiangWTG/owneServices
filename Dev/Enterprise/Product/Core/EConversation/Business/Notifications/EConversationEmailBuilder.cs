using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.EConversation.Business
{
	public static class EConversationEmailBuilder
	{
		public static void GenerateAndQueueEmailNotifications(JobConversation conversation)
		{
			Argument.NotNull(conversation, nameof(conversation));

			if (conversation.Parent is IConversationProvider parent)
			{
				var newMessages = EConversationMessageProvider.GetNewMessages(conversation, true);
				var groupings = newMessages.GroupBy(m => m.Sender);

				foreach (var newMessagesBySender in groupings)
				{
					GenerateNotifications(parent, conversation, newMessagesBySender.Key, newMessagesBySender);
				}
			}
		}

		public static void GenerateAndQueueEmailNotifications(JobConversation conversation, JobConversationParticipant sender, IEnumerable<JobConversationMessage> newMessages)
		{
			Argument.NotNull(conversation, nameof(conversation));
			Argument.NotNull(newMessages, nameof(newMessages));

			if (conversation.Parent is IConversationProvider parent)
			{
				GenerateNotifications(parent, conversation, sender, newMessages);
			}
		}

		public static void GenerateAndQueueStaffOnlyBroadcastEmailNotifications(JobConversation conversation, IConversationProvider parent, IEnumerable<string> emailsToExclude, IEnumerable<JobConversationMessage> newMessages)
		{
			if (parent != null)
			{
				var urlCreator = ObjectFactory.Get<IShowEditFormUrlCreator>();
				var formLink = urlCreator.Create(parent.ParentController, (parent as BusinessObject).PK.ToGuid());
				var groupings = newMessages.GroupBy(m => m.Sender);

				foreach (var newMessagesBySender in groupings)
				{
					foreach (var group in GetSubscribedUsersWithDistinctEmails(conversation, true, emailsToExclude, newMessagesBySender.Key).ToList().GroupBy(sub => sub.IsInternal))
					{
						GenerateNotificationsForEachSubscriber(conversation, group, group.Key, formLink, newMessagesBySender);
					}
				}
			}
		}

		const int numberOfPreviousMessagesToInclude = 5;

		static void GenerateNotifications(IConversationProvider provider, JobConversation conversation, JobConversationParticipant sender, IEnumerable<JobConversationMessage> newMessages)
		{
			var urlCreator = ObjectFactory.Get<IShowEditFormUrlCreator>();
			var formLink = urlCreator.Create(provider.ParentController, conversation.JCC_ParentID.ToGuid());

			foreach (var group in GetSubscribedUsersWithDistinctEmails(conversation, sender).ToList().GroupBy(sub => sub.IsInternal))
			{
				GenerateNotificationsForEachSubscriber(conversation, group, group.Key, formLink, newMessages);
			}
		}

		/// <summary>
		/// Generate one email per staff language to subscribed staff.
		/// Returns true if any email generated.
		/// Uses conversation factory.
		/// Caller is responsible for calling factory Save.
		/// </summary>
		public static bool GenerateStaffNotification(BusinessObjectFactory factory, JobConversation convo, IEnumerable<JobConversationMessage> newMessages, string emailToExclude, string formLink = null)
		{
			if (!newMessages.Any())
			{
				return false;
			}

			var result = false;
			var groupings = newMessages.GroupBy(m => m.Sender);

			foreach (var newMessagesBySender in groupings)
			{
				var group = GetSubscribedUsersWithDistinctEmails(convo, staffOnly: true, emailToExclude: emailToExclude, newMessagesBySender.Key).ToList();
				result |= GenerateEmails(factory, convo, group, newMessagesBySender, true, formLink, separateEmailPerSubscriber: false);
			}

			return result;
		}

		public static bool GenerateStaffAndContactNotifications(
			BusinessObjectFactory factory,
			JobConversation convo,
			IEnumerable<JobConversationMessage> newMessages,
			IEnumerable<string> emailsToExclude,
			string staffLink,
			string contactLink)
		{
			var provider = convo.Parent as IConversationProvider;
			var alternativeEmailTemplate = provider?.NotificationEmailTemplateOverride;
			var allowEmptyMessages = alternativeEmailTemplate != null;

			if (!allowEmptyMessages && !newMessages.Any())
			{
				return false;
			}

			var result = false;

			if (newMessages.IsNullOrEmpty())
			{
				result = GroupAndSendNotificationMessages(factory, convo, emailsToExclude, staffLink, contactLink, Enumerable.Empty<JobConversationMessage>());
			}
			else
			{
				var groupings = newMessages.GroupBy(m => m.Sender);
				foreach (var newMessagesBySender in groupings)
				{
					result = GroupAndSendNotificationMessages(factory, convo, emailsToExclude, staffLink, contactLink, newMessagesBySender);
				}
			}

			return result;
		}

		static bool GroupAndSendNotificationMessages(
			BusinessObjectFactory factory,
			JobConversation convo,
			IEnumerable<string> emailsToExclude,
			string staffLink,
			string contactLink,
			IEnumerable<JobConversationMessage> newMessages)
		{
			var result = false;
			foreach (var group in GetSubscribedUsersWithDistinctEmails(convo, staffOnly: false, emailsToExclude: emailsToExclude, sender: null)
				.ToList()
				.GroupBy(sub => sub.IsInternal)
				.OrderBy(x => x.Key ? 0 : 1)) // staff first
			{
				var formLink = group.Key ? staffLink : contactLink;
				if (group.Key)
				{
					result |= GenerateEmails(factory, convo, group, newMessages, group.Key, formLink, separateEmailPerSubscriber: false);
				}
				else
				{
					foreach (var orgContactOrEmailGroup in group.GroupBy(sub => sub is IOrgContact || sub is IOrganisationData).ToList())
					{
						if (orgContactOrEmailGroup.Key)
						{
							result |= GenerateEmails(factory, convo, orgContactOrEmailGroup, newMessages, group.Key, formLink, separateEmailPerSubscriber: false);
						}
						else
						{
							result |= GenerateEmails(factory, convo, orgContactOrEmailGroup, newMessages, group.Key, formLink, separateEmailPerSubscriber: true);
						}
					}
				}
			}

			return result;
		}

		public static bool GenerateNotificationsForNewSubscribers(BusinessObjectFactory factory,
			JobConversation convo,
			IEnumerable<IConversationParticipant> participants,
			IEnumerable<JobConversationMessage> newMessages,
			bool includeInternal,
			string formLink,
			bool separateEmailPerSubscriber)
		{
			return GenerateEmails(factory, convo, participants, newMessages, includeInternal, formLink, separateEmailPerSubscriber);
		}

		static void GenerateNotificationsForEachSubscriber(JobConversation conversation, IEnumerable<IConversationParticipant> participants, bool includeInternal, string formLink, IEnumerable<JobConversationMessage> newMessages)
		{
			if (participants.Any() && newMessages.Any())
			{
				GenerateEmails(conversation.Factory, conversation, participants, newMessages, includeInternal, formLink, separateEmailPerSubscriber: true);
			}
		}

		static bool GenerateEmails(
			BusinessObjectFactory factory,
			JobConversation conversation,
			IEnumerable<IConversationParticipant> participants,
			IEnumerable<JobConversationMessage> newMessages,
			bool includeInternal,
			string formLink,
			bool separateEmailPerSubscriber)
		{
			var result = false;

			if (!includeInternal)
			{
				newMessages = newMessages.Where(x => x.MessageType != MessageType.LocalInternal);
			}

			var provider = conversation.Parent as IConversationProvider;
			var alternativeEmailTemplate = provider?.NotificationEmailTemplateOverride;

			if (alternativeEmailTemplate != null && !newMessages.Any() && participants.Any())
			{
				result = true;
				GenerateAndCreateOutgoingEmails(factory, conversation, participants, Enumerable.Empty<JobConversationMessage>(), includeInternal, formLink, separateEmailPerSubscriber, alternativeEmailTemplate);
			}

			if (newMessages.Any() && participants.Any())
			{
				result = true;
				GenerateAndCreateOutgoingEmails(factory, conversation, participants, newMessages, includeInternal, formLink, separateEmailPerSubscriber, alternativeEmailTemplate);
			}

			return result;
		}

		static void GenerateAndCreateOutgoingEmails(BusinessObjectFactory factory,
			JobConversation conversation,
			IEnumerable<IConversationParticipant> participants,
			IEnumerable<JobConversationMessage> newMessages,
			bool includeInternal,
			string formLink,
			bool separateEmailPerSubscriber,
			NotificationEmailTemplate alternativeEmailTemplate)
		{
			var needHideStaffName = conversation.HideStaffName;
			var oldMessages = EConversationMessageProvider.GetOldMessages(conversation, includeInternal, numberOfPreviousMessagesToInclude, newMessages);

			newMessages.ForEach(m => m.HideStaffName = needHideStaffName);
			oldMessages.ForEach(m => m.HideStaffName = needHideStaffName);

			foreach (var group in participants.GroupBy(g => g.Language))
			{
				using (string.IsNullOrEmpty(group.Key) ? null : Res.TemporarilySwitchLanguage(group.Key))
				{
					var provider = conversation.Parent as IConversationProvider;
					var fromAddressOverride = provider?.FromAddressOverride;

					if (separateEmailPerSubscriber)
					{
						foreach (var participant in group)
						{
							var email = GenerateEmail(conversation, newMessages, oldMessages, formLink, fromAddressOverride, participant, alternativeEmailTemplate);
							email.AddRecipientForUserCommunication(participant.Email);
							Env.OutgoingMailManager.Create(factory, email);
						}
					}
					else
					{
						var email = GenerateEmail(conversation, newMessages, oldMessages, formLink, fromAddressOverride, null, alternativeEmailTemplate);
						foreach (var participant in group)
						{
							email.AddRecipientForUserCommunication(participant.Email);
						}
						Env.OutgoingMailManager.Create(factory, email);
					}
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		static EmailDef GenerateEmail(JobConversation conversation, IEnumerable<IConversationMessage> newMessages, IEnumerable<IConversationMessage> previousMessages, string formLink, string fromAddressOverride, IConversationParticipant recipientParticipant, NotificationEmailTemplate alternativeEmailTemplate = null)
		{
			var hyperlinkToParent = formLink;

			if (conversation.Parent is IConversationParentHyperlinkProvider hyperlinkProvider && hyperlinkProvider.ShouldUseThisProviderForHyperlink(recipientParticipant))
			{
				hyperlinkToParent = hyperlinkProvider.GetHyperlinkToConversationParent();
			}

			var template = alternativeEmailTemplate ?? SystemDataRegistry.Instance.EConversationMessageEmailTemplate.Value;
			var documentBizo = new EConversationMessageNotification(conversation, newMessages, previousMessages, hyperlinkToParent, recipientParticipant);
			var parser = new EConversationMessageNotificationParser(conversation.Factory);

			var subjectToParse = template.EmailSubject;
			if (!subjectToParse.Contains("(*ID*)"))
			{
				subjectToParse += " (*ID*)";
			}

			var subject = parser.Parse(documentBizo, subjectToParse);
			var body = parser.Parse(documentBizo, template.EmailBody);
			var displayName = Env.Registry.MailboxDisplayName;

			EmailDef email = null;

			if (conversation.Parent is IConversationEmailBehaviorProvider eMailBehaviorStrategy
				&& eMailBehaviorStrategy.ShouldSendEmailFromSender
				&& newMessages.FirstOrDefault() is JobConversationMessage jobConversationMessage
				&& jobConversationMessage.Sender.JCP_ParticipantTableCode == GlbStaffSchema.Constants.Prefix)
			{
				email = new EmailDef(jobConversationMessage.Sender.JCP_ParticipantID.ToGuid());
				displayName = email.FromDisplayName;
			}
			else
			{
				email = new EmailDef();
			}

			email.Subject = subject;
			email.Body = body;
			email.ContentType = EmailContentTypes.HTML;
			email.FromDisplayName = displayName;
			email.Headers = new Dictionary<string, string> { { "X-Auto-Response-Suppress", "All" } };

			if (!string.IsNullOrEmpty(fromAddressOverride))
			{
				email.FromAddress = fromAddressOverride;
			}

			return email;
		}

		#endregion

		static IEnumerable<IConversationParticipant> GetSubscribedUsersWithDistinctEmails(JobConversation conversation, JobConversationParticipant sender)
		{
			string emailToExclude = null;
			if (conversation.Parent is IConversationEmailBehaviorProvider eMailBehaviorStrategy
				&& eMailBehaviorStrategy.ShouldExcludeSender)
			{
				emailToExclude = sender.EmailAddress;
			}
			return GetSubscribedUsersWithDistinctEmails(conversation, staffOnly: false, emailToExclude, sender);
		}

		static IEnumerable<IConversationParticipant> GetSubscribedUsersWithDistinctEmails(JobConversation conversation, bool staffOnly, string emailToExclude, JobConversationParticipant sender)
		{
			return EConversationParticipantProvider.GetParticipantsForEConversationMessageNotifications(
				conversation,
				staffOnly,
				emailToExclude != null ? new[] { emailToExclude } : null,
				sender);
		}

		static IEnumerable<IConversationParticipant> GetSubscribedUsersWithDistinctEmails(JobConversation conversation, bool staffOnly, IEnumerable<string> emailsToExclude, JobConversationParticipant sender)
		{
			return EConversationParticipantProvider.GetParticipantsForEConversationMessageNotifications(
				conversation,
				staffOnly,
				emailsToExclude,
				sender);
		}
	}
}
