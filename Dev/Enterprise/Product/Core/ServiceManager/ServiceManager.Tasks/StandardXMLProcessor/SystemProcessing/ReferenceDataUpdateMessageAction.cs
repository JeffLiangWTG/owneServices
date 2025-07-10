using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Licensing;
using Enterprise.Messaging.Business;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public class ReferenceDataUpdateMessageAction : IMessageAction
	{
		public ReferenceDataUpdateMessageAction(BusinessObjectFactoryProvider factoryProvider)
		{
		}

		bool IMessageAction.ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participants)
		{
			participants = new List<ITransactionParticipant>(1);
			SystemDataUpdater updater = new SystemDataUpdater(message.EM_MessageText);
			updater.Process(participants);
			return true;
		}

		void IMessageAction.SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
		{
		}
	}
}
