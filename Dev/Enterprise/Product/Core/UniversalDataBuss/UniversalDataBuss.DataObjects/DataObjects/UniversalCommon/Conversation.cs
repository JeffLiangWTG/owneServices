using System.Collections.Generic;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public partial class Conversation : IDataObject
	{
		public Conversation()
		{
		}

		public Conversation(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public List<ConversationMessage> ConversationMessageCollection { get; private set; }
		public List<ParticipantStaff> ParticipantStaffCollection { get; private set; }
		public List<ParticipantGroup> ParticipantGroupCollection { get; private set; }
		public List<ParticipantRelatedParty> ParticipantRelatedPartyCollection { get; private set; }
	}
}
