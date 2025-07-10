using System.Collections.Generic;
using System.Linq;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.EConversation.Business
{
	public static class EConversationParticipantProvider
	{
		public static IReadOnlyCollection<IConversationParticipant> GetParticipantsForEConversationMessageNotifications(JobConversation convo, bool staffOnly, IEnumerable<string> emailsToExclude, JobConversationParticipant sender)
		{
			var emailSetToEnsureUniqueness = new HashSet<string> { string.Empty, null };
			if (emailsToExclude != null)
			{
				emailSetToEnsureUniqueness.UnionWith(emailsToExclude);
			}
			var subscribedParticipants = GetSubscribedParticipants(emailSetToEnsureUniqueness, convo, staffOnly).ToList();
			var additionalParticipants = GetAdditionalParticipants(convo, subscribedParticipants, sender).Where(p => emailSetToEnsureUniqueness.Add(p.Email) && (!staffOnly || p is IGlbStaff));

			return subscribedParticipants.Union(additionalParticipants).ToArray();
		}

		static IEnumerable<IConversationParticipant> GetSubscribedParticipants(HashSet<string> emailSetToEnsureUniqueness, JobConversation convo, bool staffOnly)
		{
			foreach (var staff in convo.Staff)
			{
				if (staff.JCP_ParticipantID != Env.CurrentUserPK && IsActiveAndNotAlreadyChecked(emailSetToEnsureUniqueness, staff.Parent) && staff.JCP_IsSubscribed)
				{
					yield return staff.Parent;
				}
			}

			foreach (var group in convo.Groups)
			{
				if (group.JCP_IsSubscribed)
				{
					foreach (IConversationParticipant staff in ((IGlbGroup)group.Parent).Staff)
					{
						if (IsActiveAndNotAlreadyChecked(emailSetToEnsureUniqueness, staff))
						{
							yield return staff;
						}
					}
				}
			}

			if (!staffOnly)
			{
				foreach (var contact in convo.RelatedParties)
				{
					if (contact.JCP_IsSubscribed && IsActiveAndNotAlreadyChecked(emailSetToEnsureUniqueness, contact.Parent))
					{
						yield return contact.Parent;
					}
				}
			}
		}

		static IEnumerable<IConversationParticipant> GetAdditionalParticipants(JobConversation conversation, IReadOnlyCollection<IConversationParticipant> subscribedParticipants, JobConversationParticipant sender)
		{
			if (conversation.Parent is IConversationAdditionalParticipantProvider parent)
			{
				return parent.GetAdditionalParticipants(subscribedParticipants, sender);
			}
			else
			{
				return Enumerable.Empty<IConversationParticipant>();
			}
		}

		static bool IsActiveAndNotAlreadyChecked(HashSet<string> emailSetToEnsureUniqueness, IConversationParticipant participant)
		{
			return participant != null && participant.IsActive && emailSetToEnsureUniqueness.Add(participant.Email);
		}
	}
}
