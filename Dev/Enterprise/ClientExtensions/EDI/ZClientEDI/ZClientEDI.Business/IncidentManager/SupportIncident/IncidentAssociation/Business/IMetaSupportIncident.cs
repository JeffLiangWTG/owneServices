using System;
using System.Collections.Generic;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public interface IMetaSupportIncident
	{
		IReadOnlyCollection<MetaConversationItem> ConversationItems { get; }
		string Country { get; }
		string Description { get; }
		string Module { get; }
		string NoteText { get; }
		Guid PK { get; }
		string IncidentNumber { get; }
		string Priority { get; }
		string Product { get; }
		string ProgramArea { get; }
		DateTime SystemCreateTimeUtc { get; }
		DateTime SystemLastEditTimeUtc { get; }
		IReadOnlyCollection<MetaWorkItem> WorkItems { get; }

		string ToString();
	}
}
