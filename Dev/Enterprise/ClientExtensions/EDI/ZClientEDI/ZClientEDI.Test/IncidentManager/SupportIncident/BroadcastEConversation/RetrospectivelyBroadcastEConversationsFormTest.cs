using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Test
{
	[TestedType(typeof(RetrospectivelyBroadcastEConversationsForm))]
	public class RetrospectivelyBroadcastEConversationsFormTest : ZFormBasherTest
	{
		public void TestSendButton()
		{
			var incidents = CreateIncidentList();
			using (var form = new RetrospectivelyBroadcastEConversationsFormForTest(CreateJobConversationMessageList(), incidents))
			{
				form.Show();

				AssertEquals(1, incidents[0].EConversation.Conversation.Messages.Count);

				form.SelectAll();
				form.SendButtonClick_Exposed();

				AssertEquals(2, incidents[0].EConversation.Conversation.Messages.Count);
				AssertEquals("test broadcast", incidents[0].EConversation.Conversation.Messages[0].Body);
			}
		}

		public void TestSendButton_MultipleMessages()
		{
			var incidents = CreateIncidentList();
			using (var form = new RetrospectivelyBroadcastEConversationsFormForTest(CreateJobConversationMessageList(true), incidents))
			{
				form.Show();

				AssertEquals(1, incidents[0].EConversation.Conversation.Messages.Count);

				form.SelectAll();
				form.SendButtonClick_Exposed();

				var messages = incidents[0].EConversation.Conversation.Messages;
				AssertEquals(5, messages.Count);
				AssertEquals("test broadcast 4", messages[0].Body);
				AssertEquals("test broadcast 3", messages[1].Body);
				AssertEquals("test broadcast 2", messages[2].Body);
				AssertEquals("test broadcast", messages[3].Body);
				AssertEquals("Jim (TESORGSYD) has been added to the conversation.", messages[4].Body);

				var msgDiff1 = messages[0].SystemCreateTimeInUtc - messages[1].SystemCreateTimeInUtc;
				Assert(msgDiff1.Seconds >= 1);
				var msgDiff2 = messages[1].SystemCreateTimeInUtc - messages[2].SystemCreateTimeInUtc;
				Assert(msgDiff2.Seconds >= 1);
				var msgDiff3 = messages[2].SystemCreateTimeInUtc - messages[3].SystemCreateTimeInUtc;
				Assert(msgDiff3.Seconds >= 1);
			}
		}

		public void TestCloseButtonMessage()
		{
			using (var form = new RetrospectivelyBroadcastEConversationsFormForTest(CreateJobConversationMessageList(), CreateIncidentList()))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();

				form.CloseButtonClick_Exposed();

				Assert("Should not show message if no selected items", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
			}

			using (var form = new RetrospectivelyBroadcastEConversationsFormForTest(CreateJobConversationMessageList(), CreateIncidentList()))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();

				form.SelectAll();
				form.CloseButtonClick_Exposed();

				AssertEquals("Are you sure you want to close this form?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDuplicateMessage()
		{
			var incidents = CreateIncidentList();
			var messageList = CreateJobConversationMessageList();
			using (var form = new RetrospectivelyBroadcastEConversationsFormForTest(messageList, incidents))
			{
				form.Show();

				AssertEquals(1, incidents[0].EConversation.Conversation.Messages.Count);

				form.SelectAll();
				form.SendButtonClick_Exposed();

				AssertEquals(2, incidents[0].EConversation.Conversation.Messages.Count);
				AssertEquals("test broadcast", incidents[0].EConversation.Conversation.Messages[0].Body);
				Factory.Save();
			}

			using (var form = new RetrospectivelyBroadcastEConversationsFormForTest(messageList, incidents))
			{
				form.Show();

				AssertEquals("Should have two messages", 2, incidents[0].EConversation.Conversation.Messages.Count);

				form.SelectAll();
				form.SendButtonClick_Exposed();

				AssertEquals("Should not add the same message again", 2, incidents[0].EConversation.Conversation.Messages.Count);
				AssertEquals("test broadcast", incidents[0].EConversation.Conversation.Messages[0].Body);
			}
		}

		public void TestEmailIsCreated()
		{
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var incidents = CreateIncidentList();
			Factory.Save();
			using (var form = new RetrospectivelyBroadcastEConversationsFormForTest(CreateJobConversationMessageList(), incidents))
			{
				form.Show();

				AssertEquals(1, incidents[0].EConversation.Conversation.Messages.Count);

				form.SelectAll();
				form.SendButtonClick_Exposed();

				AssertEquals(2, incidents[0].EConversation.Conversation.Messages.Count);
				AssertEquals("test broadcast", incidents[0].EConversation.Conversation.Messages[0].Body);

				Factory.Save();

				AssertEquals("two emails should be created.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				Assert(Env.OutgoingMailManager.EmailsCreated[0].Subject.Contains("Update on Incident: CS0000001"));
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new RetrospectivelyBroadcastEConversationsForm(CreateJobConversationMessageList(), CreateIncidentList());
		}

		public override void TestBashingForm()
		{
			// Custom form
			Assert(true);
		}

		List<JobConversationMessage> CreateJobConversationMessageList(bool multipleBroadcastMessages = false)
		{
			var jobConversationMessages = new List<JobConversationMessage>();

			var workitem1 = Factory.NewWithValidTestData<NewWorkItem>();
			Factory.Save();
			workitem1.Conversation.Messages.AddNew(null, "test broadcast", false, false, true);
			workitem1.Conversation.Messages.AddNew(null, "test no broadcast", false, false, false);

			if (multipleBroadcastMessages)
			{
				workitem1.Conversation.Messages.AddNew(null, "test broadcast 2", false, false, true);
				workitem1.Conversation.Messages.AddNew(null, "test broadcast 3", false, false, true);
				workitem1.Conversation.Messages.AddNew(null, "test broadcast 4", false, false, true);
			}

			jobConversationMessages.AddRange(workitem1.GetAllEConversationBroadcastMessages());

			return jobConversationMessages;
		}

		List<SupportIncident> CreateIncidentList()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test Org 2";
			org.OH_RL_NKClosestPort = "AUSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Jim";
			contact.OC_Email = "jim@test.com.au";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS0000001";
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;

			var incidents = new List<SupportIncident>();
			incidents.Add(incident);
			return incidents;
		}

		class RetrospectivelyBroadcastEConversationsFormForTest : RetrospectivelyBroadcastEConversationsForm
		{
			public CustomJobConversationMessageUserControl CustomJobConversationMessageUserControl_Exposed => CustomJobConversationMessageUserControl;
			public CustomSupportIncidentUserControl CustomSupportIncidentUserControl_Exposed => CustomSupportIncidentUserControl;

			public RetrospectivelyBroadcastEConversationsFormForTest(List<JobConversationMessage> jobConversationMessages, List<SupportIncident> incidents)
				: base(jobConversationMessages, incidents)
			{
			}

			public void SelectAll()
			{
				CustomJobConversationMessageUserControl_Exposed.SelectAllForTest();
				CustomSupportIncidentUserControl_Exposed.SelectAllForTest();
			}

			protected override DialogResult SendConfirmationMessage() => DialogResult.Yes;

			public void SendButtonClick_Exposed()
			{
				SendButton_Click(null, null);
			}

			public void CloseButtonClick_Exposed()
			{
				CloseButton_Click(null, null);
			}
		}
	}
}
