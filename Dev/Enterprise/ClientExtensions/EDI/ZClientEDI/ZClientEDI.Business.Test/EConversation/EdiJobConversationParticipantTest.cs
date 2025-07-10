using Enterprise.EConversation.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ZClientEDI.Business.EConversation;

namespace ZClientEDI.Business.Test
{
	[TestedType(typeof(EdiJobConversationParticipant))]
	public class EdiJobConversationParticipantTest : JobConversationParticipantTest
	{
		public void TestOrgParticipantsNotAutomaticallySubscribed_WhenParentTableCodeIsING()
		{
			var participant = Factory.NewWithValidTestData<EdiJobConversationParticipant>();

			participant.Conversation.JCC_ParentTableCode = IncidentManagementGroupSchema.Constants.Prefix;
			participant.RelatedPartyTypeName = "Organization";
			AssertEquals(OrgHeaderSchema.Constants.Prefix, participant.JCP_ParticipantTableCode);
			AssertEquals("The org participants aren't automatically subscribed", false, participant.JCP_IsSubscribed);
		}
	}
}
