using System;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;

namespace CargoWise.eHub.Gateway
{
	public class InboxOutboxMessageHandler : MessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			var inboxAccessor = GetInboxAccessor();
			inboxAccessor.InsertToInboxAndOutbox(senderID, envelopeTrackingID, message);
		}

		internal virtual IInboxAccessor GetInboxAccessor()
		{
			return DataAccessFactories.NewInboxAccessorInstance();
		}
	}
}