using System.Data;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using Enterprise.ZArchitecture.Schema;

namespace ZClientEDI.Business.EConversation
{
	public class EdiJobConversationParticipant : JobConversationParticipant
	{
		public EdiJobConversationParticipant(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new EdiJobConversationParticipantValidation Validation => (EdiJobConversationParticipantValidation)base.Validation;
		protected override JobConversationParticipantValidation GetNewValidation() => new EdiJobConversationParticipantValidation(this);

		protected override bool IsConversationIncidentRelated() => base.IsConversationIncidentRelated() || Conversation.JCC_ParentTableCode == IncidentManagementGroupSchema.Constants.Prefix;
	}
}
