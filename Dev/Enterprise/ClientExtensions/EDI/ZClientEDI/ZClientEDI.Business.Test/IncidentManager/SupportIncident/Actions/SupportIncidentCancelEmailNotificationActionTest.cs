using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentCancelEmailNotificationAction))]
	public class SupportIncidentCancelEmailNotificationActionTest : SupportIncidentActionTestCase
	{
		public void TestCancelEmailComment()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var action = new SupportIncidentCancelEmailNotificationAction(incident);
			action.Comment = "I cancel email because I'm minding my own business";
			action.SynchroniseToIncident();

			var message = GlbStaff.CurrentUser.GS_FullName + " canceled the customer email notification. Reason: I cancel email because I'm minding my own business";
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("Internal message should be present: " + message, true,
				messageList.Any(msg => msg.Body == message && msg.MessageType == MessageType.LocalInternal));

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var incidentInAnotherFactory = otherFactory.Load<SupportIncident>(incident.PK);
			messageList = incidentInAnotherFactory.EConversation.GetTimeOrderedMessages();
			AssertEquals("Internal message should be present: " + message, true,
				messageList.Any(msg => msg.Body == message && msg.MessageType == MessageType.LocalInternal));
		}

		public void TestCancelEmail_WithoutComment()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var action = new SupportIncidentCancelEmailNotificationAction(incident);
			action.Comment = "";
			action.SynchroniseToIncident();

			var message = GlbStaff.CurrentUser.GS_FullName + " canceled the customer email notification.";
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("Internal message should be present: " + message, true,
				messageList.Any(msg => msg.Body == message && msg.MessageType == MessageType.LocalInternal));

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var incidentInAnotherFactory = otherFactory.Load<SupportIncident>(incident.PK);
			messageList = incidentInAnotherFactory.EConversation.GetTimeOrderedMessages();
			AssertEquals("Internal message should be present: " + message, true,
				messageList.Any(msg => msg.Body == message && msg.MessageType == MessageType.LocalInternal));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var incident = Factory.New<SupportIncident>();
			return new SupportIncidentCancelEmailNotificationAction(incident);
		}
	}
}
