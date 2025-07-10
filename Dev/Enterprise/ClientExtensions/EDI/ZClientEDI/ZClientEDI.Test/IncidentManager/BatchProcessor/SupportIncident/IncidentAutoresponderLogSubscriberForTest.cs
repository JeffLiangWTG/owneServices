using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.EDI.IncidentManager.Test
{
	[Serializable]
	public class IncidentAutoresponderLogSubscriberForTest : IncidentAutoresponderLogSubscriber
	{
		public void ProcessLogQueueItemsExposed(IQueuedLog[] queuedLogs)
		{
			EDIDataRegistry.Instance.EnableAutoresponder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ProcessLogQueueItems(queuedLogs);
		}

		public JobConversationParticipant FetchJobConversationParticipantExposed(BusinessObjectFactory factory, ZGuid participantId)
		{
			return FetchJobConversationParticipant(factory, participantId);
		}
	}
}
