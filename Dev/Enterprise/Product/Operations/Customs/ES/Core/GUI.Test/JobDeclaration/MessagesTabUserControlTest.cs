using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class MessagesTabUserControlTest : TestCaseWithFactory
	{
		public void TestMessageTextTextBox()
		{
			using (var form = new ZForm())
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var messageTextControl = control.FindSingle<ZTextBox>("MessageTextTextBox");
				AssertEquals("BindingMember", "EM_FormattedMessageText", messageTextControl.GetBindingMember());
			}
		}

		public void TestResendInterchangeMessageNotSelected()
		{
			using (var form = new ZForm(declaration.CustomsEntryHeaders[0].Messages))
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var messagesGrid = control.FindSingle<ZGrid>("MessagesGrid");

				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Select an interchange to resend", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResendInterchangeMessageMissingData()
		{
			var expectedErrorMessage = "Message 1 has never been sent.  Please verify the service tasks are running";

			using (var form = new ZForm(declaration.CustomsEntryHeaders[0].Messages))
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var messagesGrid = control.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);
				CombineAssertions(() =>
				{
					DoResendInterchangeClick(messagesGrid);
					AssertEquals("Message not in database", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					Factory.Save();
					DoResendInterchangeClick(messagesGrid);
					AssertEquals("Message in database but no interchange", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					Factory.AddInterchangeToMessage(message, "");
					DoResendInterchangeClick(messagesGrid);
					AssertEquals("message in database with interchange not in database", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestResendInterchangeMessageReceived()
		{
			Factory.AddInterchangeToMessage(message, "");
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			using (var form = new ZForm(declaration.CustomsEntryHeaders[0].Messages))
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var messagesGrid = control.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);

				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Message 1 in Interchange 1 is not an outbound message.  You may only resend outbound messages", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResendInterchangeMessageStatusHPN()
		{
			Factory.AddInterchangeToMessage(message, "HPN");
			Factory.Save();

			using (var form = new ZForm(declaration.CustomsEntryHeaders[0].Messages))
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var messagesGrid = control.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);

				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Message 1 in Interchange 1 has been acknowledged and is waiting for a response.  Are you sure you want to resend it?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResendInterchangeMessageStatusFAL()
		{
			Factory.AddInterchangeToMessage(message, "FAL");
			Factory.Save();

			using (var form = new ZForm(declaration.CustomsEntryHeaders[0].Messages))
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var messagesGrid = control.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);

				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Message 1 in Interchange 1 has been acknowledged but something failed. Before resending it, please check the logs or error message.  Are you sure you want to resend it?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResendInterchangeMessageStatusSNT()
		{
			Factory.AddInterchangeToMessage(message, "SNT");
			Factory.Save();

			using (var form = new ZForm(declaration.CustomsEntryHeaders[0].Messages))
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var messagesGrid = control.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);

				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Message 1 in Interchange 1 has been acknowledged and sent correctly.  Are you sure you want to resend it?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResendInterchangeMessageStatusHQU()
		{
			Factory.AddInterchangeToMessage(message, "HQU");
			Factory.Save();

			using (var form = new ZForm(declaration.CustomsEntryHeaders[0].Messages))
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var messagesGrid = control.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);

				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Message 1 in Interchange 1 is waiting to be sent or has just been sent.  Please verify the service tasks are running", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResendInterchangeMessageStatusQUE()
		{
			Factory.AddInterchangeToMessage(message, "QUE");
			Factory.Save();

			using (var form = new ZForm(declaration.CustomsEntryHeaders[0].Messages))
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var messagesGrid = control.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);

				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Message 1 in Interchange 1 is waiting to be sent or has just been sent.  Please verify the service tasks are running", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResendInterchangeMessageStatusOther()
		{
			Factory.AddInterchangeToMessage(message, "AAA");
			Factory.Save();

			using (var form = new ZForm(declaration.CustomsEntryHeaders[0].Messages))
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var messagesGrid = control.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);

				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Message 1 in Interchange 1 is in status AAA and is not possible to resend it", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResendInterchangeMultipleMessages()
		{
			Factory.AddInterchangeToMessage(message, "HQU");

			var message2 = Factory.New<TestEdiMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageNum = "2";
			entryHeader.Messages.Add(message2);
			Factory.AddInterchangeToMessage(message2, "AAA", "2");

			Factory.Save();

			var expectedMessage1 = "Message 1 in Interchange 1 is waiting to be sent or has just been sent.  Please verify the service tasks are running";
			var expectedMessage2 = "Message 2 in Interchange 2 is in status AAA and is not possible to resend it";

			using (var form = new ZForm(declaration.CustomsEntryHeaders[0].Messages))
			using (var control = new MessagesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var messagesGrid = control.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.SelectAllElements();

				DoResendInterchangeClick(messagesGrid);
				CombineAssertions(() =>
				{
					AssertEquals("The first message was shown", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage1));
					AssertEquals("The second message was shown", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage2));
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();

			message = Factory.New<TestEdiMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "1";
			entryHeader.Messages.Add(message);
		}
		TestEdiMessage message;
		CusEntryHeader entryHeader;
		JobDeclaration declaration;

		void DoResendInterchangeClick(ZGrid grid)
		{
			foreach (MenuItem candidate in grid.ContextMenu.MenuItems)
			{
				if (candidate.Text == "Resend Interchange")
				{
					candidate.PerformClick();
					return;
				}
			}

			return;
		}
	}
}
