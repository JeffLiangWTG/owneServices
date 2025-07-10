using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.EConversation.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	public class IncidentManagementEConversationViewControllerTest : TestCaseWithFactory
	{
		public void TestClearingMessage()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Factory.Save();
			using (var form = new IncidentManagementGroupForm(group))
			{
				form.Show();
				SelectEConversation(form);
				var conversationView = (IConversationView)form.Controls.Find("EConversationFullControl", true).Single();
				var textbox = (ZAutoCompleteTextBox)conversationView.MessageTextBox;
				var sendButton = conversationView.SendButton;
				textbox.Focus();
				textbox.AppendText("NTZTest");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendButton.PerformClick();
				AssertNotNull(group.EConversation.Conversation.Messages.FirstOrDefault(x => x.Body.Contains("NTZTest")));
				AssertNullOrEmpty(textbox.Text);
				AssertEquals(group.EConversation.Conversation.NextMessage, ZBlob.Empty);

				form.FireSaveButton();
				AssertNullOrEmpty(textbox.Text);
				AssertEquals(group.EConversation.Conversation.NextMessage, ZBlob.Empty);
			}
		}

		void SelectEConversation(IncidentManagementGroupForm form)
		{
			var tabControl = form.TopLevelTabControl_Exposed;
			var tabPage = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "eConversationTabPage");
			tabControl.SelectedTab = tabPage;
		}

		public void TestMessageTextBox()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Factory.Save();

			using (var form = new IncidentManagementGroupForm(group))
			{
				form.Show();
				SelectEConversation(form);
				var conversationView = (IConversationView)form.Controls.Find("EConversationFullControl", true).Single();
				var textbox = (ZAutoCompleteTextBox)conversationView.MessageTextBox;
				var sendButton = conversationView.SendButton;
				textbox.Focus();

				Assert("Send should be disabled", !sendButton.Enabled);
				Assert("Add Internal Comment should be disabled", !conversationView.AddInternalCommentButton.Enabled);

				textbox.Text = new string('0', IncidentManagementEConversationViewController.InputTextMaxLength + 123);
				Assert("Send should be enabled", sendButton.Enabled);
				Assert("Add Internal Comment should be enabled", conversationView.AddInternalCommentButton.Enabled);

				AssertContains("The message has been truncated", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(IncidentManagementEConversationViewController.InputTextMaxLength, textbox.Text.Length);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestPopupShownWhenClickSendMessageButton()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "123@123.123";
			staff.GS_FullName = "123";
			staff.GS_IsController = true;

			Factory.Save();

			AssertEquals(0, group.EConversation.Conversation.Participants.Count);

			using (var form = new IncidentManagementGroupForm(group))
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				form.Show();
				SelectEConversation(form);
				var conversationView = (IConversationView)form.Controls.Find("EConversationFullControl", true).Single();
				var textBox = (ZAutoCompleteTextBox)conversationView.MessageTextBox;
				var sendButton = conversationView.SendButton;
				textBox.Focus();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				textBox.AppendText("staff message");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendButton.PerformClick();

				AssertNotNull(group.EConversation.Conversation.Messages.FirstOrDefault(x => x.Body.Contains("staff message")));
				AssertNullOrEmpty("There should be no popup if conversation's participants only contain internal staff", UnitTestUserNotification.Instance.LastMessage?.Text);
				AssertEquals("Staff should be added to the participants.", 1, group.EConversation.Conversation.Participants.Count);

				var orgContact = Factory.NewWithValidTestData<OrgContact>();
				var externalParticipants = group.EConversation.Conversation.RelatedParties.AddNewParticipant(orgContact);
				externalParticipants.JCP_IsSubscribed = true;

				AssertEquals(2, group.EConversation.Conversation.Participants.Count);
				AssertEquals(1, group.EConversation.Conversation.Participants.Count(x => x.JCP_IsSubscribed && x.JCP_ParticipantTableCode == OrgContactSchema.Constants.Prefix));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				textBox.AppendText("staff message again");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendButton.PerformClick();

				AssertNotNull(group.EConversation.Conversation.Messages.FirstOrDefault(x => x.Body.Contains("staff message again")));
				AssertContains("A popup should be shown if any external user have subscribed this conversation", "You are about to send this message to all subscribed recipients and the customer", UnitTestUserNotification.Instance.LastMessage?.Text);
			}
		}
	}
}
