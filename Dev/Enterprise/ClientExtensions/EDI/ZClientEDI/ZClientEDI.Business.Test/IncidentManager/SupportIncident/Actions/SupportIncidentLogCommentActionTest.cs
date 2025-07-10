using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentLogCommentAction))]
	public class SupportIncidentLogCommentActionTest : SupportIncidentActionTestCase
	{
		public void TestChangeCriticalityComment()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var action = new SupportIncidentLogCommentAction(incident);
			action.ChangeCriticalityFrom = "CR4";
			action.Comment = "I change the criticality because I'm happy";
			incident.IM_Priority = "CR3";
			action.SynchroniseToIncident();

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Criticality changed from CR4 to CR3"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "I change the criticality because I'm happy"));
		}

		public void TestAddIntenalLog()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var action = new SupportIncidentLogCommentAction(incident);
			action.AddInternalLog = true;
			action.Comment = "It is an internal log";

			action.SynchroniseToIncident();
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "It is an internal log" && msg.MessageType == MessageType.LocalInternal));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var incident = Factory.New<SupportIncident>();
			return new SupportIncidentLogCommentAction(incident);
		}
	}
}
