using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.eHubMessaging.Business
{
	public interface IMessageAction
	{
		bool ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participant);
		void SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess);
	}
}
