using Enterprise.EConversation.Business;
using Enterprise.EConversation.ServiceTasks;

namespace ZClientEDI.Business.Mail
{
	public class EConvesationAttacherWithNoSubscription : BusinessObjectEConversationAttacher
	{
		protected override JobConversationParticipant AddParticipantFromEmail(string sender, JobConversationParticipantCollection participants, JobConversation eConvo)
		{
			var participant = participants.AddNewParticipant(sender);
			participant.JCP_IsSubscribed = false;
			return participant;
		}
	}
}
