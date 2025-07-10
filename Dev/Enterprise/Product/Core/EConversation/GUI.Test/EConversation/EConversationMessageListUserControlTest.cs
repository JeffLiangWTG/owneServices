using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing.GUI
{
	public class EConversationMessageListUserControlTest : TestCaseWithFactory
	{
		public void TestHyperlinkClickingContainsHash()
		{
			var hyperLink = "https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#item=0BB8E74A-4BC2-4348-BF71-F305439F7F17&tab=faqs&faq=16D3CFD4-1189-44F2-A242-CC44669CEF48";
			var message = string.Format("Check out this link: {0}", hyperLink);
			AssertClickLink(message, hyperLink);
		}

		public void TestHyperlinkClickingNoContainsHash()
		{
			var hyperLink = "https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx";
			var message = string.Format(@"The first uri:
{0}
The second uri:
https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#item=0BB8E74A-4BC2-4348-BF71-F305439F7F17&tab=faqs&faq=16D3CFD4-1189-44F2-A242-CC44669CEF48", hyperLink);
			AssertClickLink(message, hyperLink);
		}

		void AssertClickLink(string message, string hyperLink)
		{
			var conversation = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			conversation.eConversation.AddMessageFromCurrentUser(message, isInternal: false);
			Factory.Save();

			using (ShowFormWithControl(conversation, out var control))
			{
				var messagebox = (RichTextBox)control.Controls.Find("bodyTextbox", true).Single();
				messagebox.Select(messagebox.Text.IndexOf(hyperLink), hyperLink.Length);

				ClickLink(messagebox, new LinkClickedEventArgs(hyperLink));

				CombineAssertions(() =>
				{
					Assert("Don't show 'Link Broken' for raw links.", !UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("The WebUrlLauncher should launch the appropriate", hyperLink, WebUrlLauncher.LastUrlLaunched);
				});
			}
		}

		[TestDate(2021, 6, 10, 12, 58, 0)]
		public void TestMessageLayoutPanelMessageLocations_HideInternal()
		{
			var conversation = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			conversation.eConversation.AddMessageFromCurrentUser("test message 1.", isInternal: false);
			Factory.Save();
			TestDateAttribute.AddSeconds(3);

			conversation.eConversation.AddMessageFromCurrentUser("test message 2.", isInternal: true);
			Factory.Save();
			TestDateAttribute.AddSeconds(3);

			conversation.eConversation.AddMessageFromCurrentUser("test message 3.", false);
			Factory.Save();

			using (ShowFormWithControl(conversation, out var control))
			{
				var publicOnlyButton = (KRadioButton)control.Controls.Find("userMessagesRadioButton", true).Single();
				publicOnlyButton.PerformClick();

				var messageLayoutPanel = (DoubleBufferedStackLayoutPanel)control.Controls.Find("messagesLayoutPanel", true).Single();

				CombineAssertions(() =>
				{
					AssertEquals("Date text label top location needs to be 0.", 0, messageLayoutPanel.Controls[0].Top);
					Assert("Test message 3 should right after date text label.", messageLayoutPanel.Controls[1].Top == messageLayoutPanel.Controls[0].Bottom);
					AssertEquals("test message 3.", messageLayoutPanel.Controls[1].Controls[1].Controls[2].Text);
					Assert("Test message 2 should right after test message 3.", messageLayoutPanel.Controls[2].Top == messageLayoutPanel.Controls[1].Bottom);
					AssertEquals("[Internal Only]  test message 2.", messageLayoutPanel.Controls[2].Controls[1].Controls[2].Text);
					Assert("Test message 1 should right after test message 3 because message 2 is not visible.", messageLayoutPanel.Controls[3].Top == messageLayoutPanel.Controls[1].Bottom);
					AssertEquals("test message 1.", messageLayoutPanel.Controls[3].Controls[1].Controls[2].Text);
				});

				var allMessagesButton = (KRadioButton)control.Controls.Find("allMessagesRadioButton", true).Single();
				allMessagesButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Date text label top location needs to be 0.", 0, messageLayoutPanel.Controls[0].Top);
					Assert("Test message 3 should right after date text label.", messageLayoutPanel.Controls[1].Top == messageLayoutPanel.Controls[0].Bottom);
					AssertEquals("test message 3.", messageLayoutPanel.Controls[1].Controls[1].Controls[2].Text);
					Assert("Test message 2 should right after test message 3.", messageLayoutPanel.Controls[2].Top == messageLayoutPanel.Controls[1].Bottom);
					AssertEquals("[Internal Only]  test message 2.", messageLayoutPanel.Controls[2].Controls[1].Controls[2].Text);
					Assert("Test message 1 should right after test message 2.", messageLayoutPanel.Controls[3].Top == messageLayoutPanel.Controls[2].Bottom);
					AssertEquals("test message 1.", messageLayoutPanel.Controls[3].Controls[1].Controls[2].Text);
				});
			}
		}

		public void TestMessageLayoutPanelMessageLocations()
		{
			var conversation = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			conversation.eConversation.AddMessageFromCurrentUser("test message 1.", isInternal: false);
			conversation.eConversation.AddMessageFromCurrentUser("test message 2.", isInternal: true);
			conversation.eConversation.AddMessageFromCurrentUser("test message 3.", false, true);
			Factory.Save();

			using (ShowFormWithControl(conversation, out var control))
			{
				var messageLayoutPanel = (DoubleBufferedStackLayoutPanel)control.Controls.Find("messagesLayoutPanel", true).Single();

				CombineAssertions(() =>
				{
					AssertEquals("Date text label top location needs to be 0.", 0, messageLayoutPanel.Controls[0].Top);
					Assert("Test message 1 should right after date text label.", messageLayoutPanel.Controls[1].Top == messageLayoutPanel.Controls[0].Bottom);
					Assert("Test message 2 should right after test message 1.", messageLayoutPanel.Controls[2].Top == messageLayoutPanel.Controls[1].Bottom);
					Assert("Test message 3 should right after test message 2.", messageLayoutPanel.Controls[3].Top == messageLayoutPanel.Controls[2].Bottom);
				});
			}
		}

		public void TestRefreshMessages_GetFoucus()
		{
			var conversation = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			conversation.eConversation.AddMessageFromCurrentUser("test message 1.", isInternal: false);
			conversation.eConversation.AddMessageFromCurrentUser("test message 2.", isInternal: true);
			conversation.eConversation.AddMessageFromCurrentUser("test message 3.", false, true);
			Factory.Save();

			using (var conversationFrom = ShowFormWithControl(conversation, out var control))
			using (var form = new ZForm())
			{
				conversationFrom.Show();
				control.RefreshMessages();
				Assert(conversationFrom.ContainsFocus);

				form.Show();
				Assert(form.Focused);
				Assert(!conversationFrom.ContainsFocus);

				control.RefreshMessages(true);
				Assert(form.Focused);
				Assert(!conversationFrom.ContainsFocus);

				control.RefreshMessages(false);
				Assert(conversationFrom.ContainsFocus);

				form.Focus();
				Assert(form.Focused);
				Assert(!conversationFrom.ContainsFocus);

				control.ForceSilentRefreshMessages = true;
				control.RefreshMessages(false);
				Assert(form.Focused);
				Assert("Control should not get focused when ForceSilentRefreshMessages is true", !conversationFrom.ContainsFocus);
			}
		}

		static void ClickLink(RichTextBox textbox, LinkClickedEventArgs @event)
			=> textbox.GetType().GetMethod("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(textbox, new object[] { @event });

		static ZForm ShowFormWithControl(IConversationProvider bizo, out EConversationMessageListUserControl control)
		{
			var module = ZModuleFactory.Instance.Create(bizo.ParentModule);
			var form = new ZForm(bizo);
			form.Disposed += (o, e) => module.Dispose();

			var eConv = new EConversationFullControl(module.ID) { ViewMode = EConversationViewMode.ShowOnlyEConversation };
			form.Size = ControlDpiScalingHelper.NewScaledSize(1280, 768, true);

			form.Controls.Add(eConv);
			form.Show();

			control = (EConversationMessageListUserControl)eConv.Controls.Find("chatboxControl", true).Single();

			return form;
		}

		public void TestUnsafeLinksInEConversation()
		{
			var unsafeHyperLink = "file://somefile";

			AssertLinkClick($"Check out this link: {unsafeHyperLink}", unsafeHyperLink, ZDialogResult.OK);
			AssertLinkClick($"Check out this link: {unsafeHyperLink}", unsafeHyperLink, ZDialogResult.Cancel);
			AssertLinkClick($"Check out this link: {unsafeHyperLink}", unsafeHyperLink, ZDialogResult.None);

			ErrorReporter.Clear();
		}

		void AssertLinkClick(string message, string hyperLink, ZDialogResult userAnswer)
		{
			var conversation = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			conversation.eConversation.AddMessageFromCurrentUser(message, isInternal: false);
			Factory.Save();

			using (RawDataRegistry.Instance.ShowWarningPopupBeforeLaunchingURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ShowFormWithControl(conversation, out var control))
			{
				var messagebox = (RichTextBox)control.Controls.Find("bodyTextbox", true).Single();
				messagebox.Select(messagebox.Text.IndexOf(hyperLink), hyperLink.Length);

				UnitTestUserNotification.Instance.AddAnswer(userAnswer);

				ClickLink(messagebox, new LinkClickedEventArgs(hyperLink));

				CombineAssertions(() =>
				{
					AssertEquals(string.Format(@"The following link leads to an external file and may be potentially unsafe:

{0}

Are you sure to sure to open the file?", hyperLink), UnitTestUserNotification.Instance.LastMessage.Text);

					if (userAnswer == ZDialogResult.OK)
					{
						AssertEquals("The WebUrlLauncher should launch the appropriate", hyperLink, WebUrlLauncher.LastUrlLaunched);
					}
					else
					{
						AssertEquals(string.Empty, WebUrlLauncher.LastUrlLaunched);
					}
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}
	}
}
