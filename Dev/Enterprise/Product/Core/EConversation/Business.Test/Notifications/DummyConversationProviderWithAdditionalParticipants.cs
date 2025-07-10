using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.EConversation.Testing
{
	sealed class DummyConversationProviderWithAdditionalParticipants : DummyConversationProvider, IConversationAdditionalParticipantProvider
	{
		public DummyConversationProviderWithAdditionalParticipants(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			TypeDecider.TypeForLoadOverride = typeof(DummyConversationProviderWithAdditionalParticipants);
		}

		internal new ICollection<IConversationParticipant> AdditionalParticipants { get; } = new List<IConversationParticipant>();

		IEnumerable<IConversationParticipant> IConversationAdditionalParticipantProvider.GetAdditionalParticipants(IReadOnlyCollection<IConversationParticipant> subscribedParticipants, JobConversationParticipant sender)
		{
			return AdditionalParticipants;
		}
	}
}
