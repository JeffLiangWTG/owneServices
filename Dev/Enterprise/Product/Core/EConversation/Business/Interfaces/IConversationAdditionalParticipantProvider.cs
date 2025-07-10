using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.EConversation.Business
{
	public interface IConversationAdditionalParticipantProvider
	{
		IEnumerable<IConversationParticipant> GetAdditionalParticipants(IReadOnlyCollection<IConversationParticipant> subscribedParticipants, JobConversationParticipant sender);
	}
}
