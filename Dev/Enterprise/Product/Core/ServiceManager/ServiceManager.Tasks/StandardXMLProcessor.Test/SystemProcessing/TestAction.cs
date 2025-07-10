using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class TestAction : IMessageAction
	{
		public TestAction(BusinessObjectFactoryProvider factoryProvider)
		{
		}

		public bool ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participant)
		{
			participant = null;
			return true;
		}

		public void SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
		{
		}
	}
}
