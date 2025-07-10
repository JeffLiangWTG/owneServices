using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.ProcessManagement.Integration;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Test
{
	[TestedType(typeof(CustomJobConversationMessage))]
	public class CustomJobConversationMessageTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new CustomJobConversationMessage(Factory.NewWithValidTestData<JobConversationMessage>());

		public void TestPopulateProperties()
		{
			var jobMessage = Factory.NewWithValidTestData<JobConversationMessage>();
			var customMessage = new CustomJobConversationMessage(jobMessage);
			var workItemNumber = jobMessage.Conversation?.Parent is IWorkItem wi ? wi.WKI_WorkItemNumber : ZString.Empty;

			AssertEquals(customMessage.PostedTimeUtc, jobMessage.JCM_PostedTimeUtc);
			AssertEquals(customMessage.Body, jobMessage.JCM_Body);
			AssertEquals(customMessage.IsInternal, jobMessage.JCM_IsInternal);
			AssertEquals(customMessage.WorkItemNumber, workItemNumber);
			AssertEquals(customMessage.Sender, jobMessage.Sender);
			AssertEquals(customMessage.IsSystemMessage, jobMessage.IsSystemMessage);
		}
	}
}
