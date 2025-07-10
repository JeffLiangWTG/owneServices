using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class ReferenceDataUpdateMessageActionTest : TestCaseWithFactory
	{
		public void TestExecuteAction()
		{
			IMessageAction action = new ReferenceDataUpdateMessageAction(null);
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			INotifications notifications = null;
			List<ITransactionParticipant> participants;
			action.ExecuteAction(message, notifications, out participants);
			AssertEquals("participants", 1, participants.Count);
		}
	}
}
