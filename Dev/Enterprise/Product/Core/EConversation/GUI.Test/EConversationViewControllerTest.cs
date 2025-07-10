using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.EConversation.Testing.GUI
{
	sealed class EConversationViewControllerTest : TestCaseWithFactory
	{
		public void TestButtons_WhenConversationIsNull_ShouldShowMessage()
		{
			void AssertButtonShowsAppropriateErrors(Func<ConversationView_ForTest, ZButton> buttonGetter)
			{
				UnitTestUserNotification.Instance.ClearMessages();

				using (var form = new ConversationView_ForTest())
				{
					form.Show();
					Application.DoEvents();

					form.Conversation = Factory.New<JobConversation>();
					var controller = new EConversationViewController();
					controller.Initialize(form);
					form.MessageTextBox.Text = "Baby Legs";

					AssertNoExceptionThrown("Clicking Send when the conversation is not null should not throw an exception. SAD!", buttonGetter(form).PerformClick);
					AssertEquals("Clicking Send when the conversation is not null should not show a message. SAD!", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				}

				UnitTestUserNotification.Instance.ClearMessages();

				using (var form = new ConversationView_ForTest())
				{
					form.Show();
					Application.DoEvents();

					var controller = new EConversationViewController();
					controller.Initialize(form);
					form.MessageTextBox.Text = "Regular Legs";

					AssertNoExceptionThrown("Clicking Send when the conversation is null should show a message, not throw an exception. SAD!", buttonGetter(form).PerformClick);
					AssertEquals("Clicking Send when the conversation is null should show a message, not throw an exception. SAD!", "Please save the form before using eConversation.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			AssertButtonShowsAppropriateErrors(form => form.SendButton);
			AssertButtonShowsAppropriateErrors(form => form.AddInternalCommentButton);
		}

		class ConversationView_ForTest : Form, IConversationView
		{
			public ConversationView_ForTest()
			{
				MessageTextBox = new TextBox();
				Controls.Add(MessageTextBox);

				SendButton = new ZButton();
				Controls.Add(SendButton);

				AddInternalCommentButton = new ZButton();
				Controls.Add(AddInternalCommentButton);
			}

			public JobConversation Conversation { get; set; }
			public TextBoxBase MessageTextBox { get; }
			public ZButton SendButton { get; }
			public ZButton AddInternalCommentButton { get; }
			public ZButton BroadcastButton { get; }
		}
	}
}
