using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Business;
using Enterprise.Environment;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class EntriesAndMessagesUserControlTest : TestCaseWithFactory
	{
		public void TestHtmlCapableTextFieldIsUsed()
		{
			using (var testForm = new JobDeclarationFormForTest(Declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				var interpretationText = brokerageControl.MessageUserControl.FindSingle<HtmlInterpretationBox>("HtmlInterpretationBox");
				AssertNotNull(interpretationText);
				AssertContains(EDIMessage.Schema.EM_MessageInterpretation, interpretationText.DataBindings[0].BindingMemberInfo.BindingField);
				AssertEquals(false, brokerageControl.MessageUserControl.FindSingle<ZTextBox>("InterpretedMessageTextBox").Visible);
			}
		}

		public void TestTrySendInterchangeWhereMoreThanOneIsSelected()
		{
			using (var form = new JobDeclarationFormForTest(Declaration))
			using (var userControl = new EntriesAndMessagesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_Status = EDIInterchange.Status.Sent;
				Declaration.CustomsEntryHeaders[0].Messages[0].EM_EI = interchange.PK;
				Declaration.CustomsEntryHeaders[0].Messages.AddNew();
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.SelectAllElements();
				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Select one message before trying to resend the interchange.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTrySendInterchangeNotLastSelected()
		{
			using (var form = new JobDeclarationFormForTest(Declaration))
			using (var userControl = new EntriesAndMessagesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var interchange1 = Factory.New<EDIInterchange>();
				interchange1.EI_Status = EDIInterchange.Status.Sent;
				Declaration.CustomsEntryHeaders[0].Messages[0].EM_EI = interchange1.PK;
				var interchange2 = Factory.New<EDIInterchange>();
				interchange2.EI_Status = EDIInterchange.Status.Sent;
				Declaration.CustomsEntryHeaders[0].Messages.AddNew();
				Declaration.CustomsEntryHeaders[0].Messages[1].EM_EI = interchange2.PK;
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);
				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Interchange could not be resent. Please select the last outgoing message.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTryReSendIncomingInterchange()
		{
			using (var form = new JobDeclarationFormForTest(Declaration))
			using (var userControl = new EntriesAndMessagesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				Declaration.CustomsEntryHeaders[0].Messages[0].EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				messagesGrid.Select(0);
				DoResendInterchangeClick(messagesGrid);
				AssertEquals("Only outgoing interchanges can be resent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTrySendByPersonWithoutNewSecurityRight()
		{
			using (var form = new JobDeclarationFormForTest(Declaration))
			using (var userControl = new EntriesAndMessagesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				Env.Security.ResendInterchange.IsAllowed = false;
				messagesGrid.Select(0);
				DoResendInterchangeClick(messagesGrid);
				AssertEquals(Env.Security.ResendInterchange.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTrySendMessageWithoutInterchange()
		{
			using (var form = new JobDeclarationFormForTest(Declaration))
			using (var userControl = new EntriesAndMessagesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);
				DoResendInterchangeClick(messagesGrid);
				AssertEquals("This message has no interchange, thus it cannot be resent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestInterchangeMenuWorking()
		{
			using (var form = new JobDeclarationFormForTest(Declaration))
			using (var userControl = new EntriesAndMessagesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_Status = EDIInterchange.Status.Queued;
				Declaration.CustomsEntryHeaders[0].Messages[0].EM_EI = interchange.PK;
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);
				DoResendInterchangeClick(messagesGrid);
				AssertEquals("This interchange is already queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);

				interchange.EI_Status = EDIInterchange.Status.eHubQueued;
				Declaration.CustomsEntryHeaders[0].Messages[0].EM_EI = interchange.PK;
				messagesGrid.Select(0);
				DoResendInterchangeClick(messagesGrid);
				AssertEquals("This interchange is already queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);

				interchange.EI_Status = EDIInterchange.Status.eHubPending;
				Declaration.CustomsEntryHeaders[0].Messages[0].EM_EI = interchange.PK;
				messagesGrid.Select(0);
				DoResendInterchangeClick(messagesGrid);
				AssertEquals("This interchange is already queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTryReSendCorrectInterchange()
		{
			using (var form = new JobDeclarationFormForTest(Declaration))
			using (var userControl = new EntriesAndMessagesUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_ApplicationCode = "KRC";
				interchange.EI_InterchangeNum = "101";
				interchange.EI_InterchangeType = Constants.EDIInterchangeType.DOC;
				interchange.EI_Status = EDIInterchange.Status.Sent;
				Declaration.CustomsEntryHeaders[0].Messages[0].EM_EI = interchange.PK;
				interchange.EI_SessionGUID = new ZGuid("5214058F-C14D-4E9F-A8BF-22CC1047C1D2");
				Factory.Save();
				var messagesGrid = userControl.FindSingle<ZGrid>("MessagesGrid");
				messagesGrid.Select(0);
				DoResendInterchangeClick(messagesGrid);
				interchange.Reload();
				AssertEquals(EDIInterchange.Status.eHubQueued, interchange.EI_Status);
				AssertEquals("Interchange successfully queued to be resent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

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

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
					var message = entryHeader.Messages.AddNew();
					message.EM_MessageType = Constants.EDIInterchangeType.DLT;
					message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
	}
}
