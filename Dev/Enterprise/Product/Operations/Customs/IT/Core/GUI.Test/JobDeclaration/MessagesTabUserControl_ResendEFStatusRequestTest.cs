using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MessagesTabUserControl_ResendEFStatusRequestTest : TestCaseWithFactory
{
	public void TestResendEFStatusRequestMenuItemVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var message1 = entryHeader.Messages.AddNew();
		var message2 = entryHeader.Messages.AddNew();

		using (var form = new ZForm(declaration))
		using (var messageUserControl = new MessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var tabControl = messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl");
			tabControl.SelectedTab = messageUserControl.FindSingle<ZTabPage>("MessageTabPage");
			var userControlMessages = (MessagesTabUserControl)messageUserControl.BaseMessageUserControl.HostedControl;
			var messagesGrid = userControlMessages.FindSingle<ZGrid>("MessagesGrid");
			var resendEFStatusRequestMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Resend EF Status Request");

			CombineAssertions("[Case 1]: no messages selected", () =>
			{
				messagesGrid.UnSelectAll();
				AssertEquals("No selected elements", 0, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !resendEFStatusRequestMenuItem.Visible);
			});

			CombineAssertions("[Case 2]: 1 message selected", () =>
			{
				messagesGrid.Select(0);
				AssertEquals("1 element selected", 1, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item is visible", resendEFStatusRequestMenuItem.Visible);
			});

			CombineAssertions("[Case 3]: more than 1 message selected", () =>
			{
				messagesGrid.SelectAllElements();
				AssertEquals("2 elements selected", 2, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item is not visible", !resendEFStatusRequestMenuItem.Visible);
			});

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			CombineAssertions("[Case 4]: Ucc6 Entry Header", () =>
			{
				messagesGrid.Select(0);
				AssertEquals("1 elements selected", 2, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !resendEFStatusRequestMenuItem.Visible);
			});
		}
	}

	public void TestResendEFStatusRequestMenuItemClick_Noerrors()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 -95751G", issueDate: new ZDateTime(2021, 01, 28));
		var message = entryHeader.Messages.AddNew();
		AssertEquals("PRE-CONDITION", 1, entryHeader.Messages.Count);
		PerformSend(declaration, message);
		CombineAssertions("POST-CONDITIONS", () =>
		{
			AssertEquals($"EF status update request has been sent to customs.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Message collection has been reloaded", 2, entryHeader.Messages.Count);
		});
	}

	public void TestResendEFStatusRequestMenuItemClick_WithError()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeaderNoRegNum = declaration.CustomsEntryHeaders.AddNew();
		var messageNoRegNum = entryHeaderNoRegNum.Messages.AddNew();
		AssertEquals("PRE-CONDITION", 1, entryHeaderNoRegNum.Messages.Count);
		PerformSend(declaration, messageNoRegNum);
		CombineAssertions("POST-CONDITIONS", () =>
		{
			AssertEquals($"This entry is not registered, the request cannot be performed.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Message has not been queued", 1, entryHeaderNoRegNum.Messages.Count);
		});
	}

	void PerformSend(JobDeclaration declaration, ITEDIMessage message)
	{
		using (var form = new ZForm(declaration))
		using (var messageUserControl = new MessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var tabControl = messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl");
			tabControl.SelectedTab = messageUserControl.FindSingle<ZTabPage>("MessageTabPage");
			var userControlMessages = (MessagesTabUserControl)messageUserControl.BaseMessageUserControl.HostedControl;
			var messagesGrid = userControlMessages.FindSingle<ZGrid>("MessagesGrid");
			messagesGrid.SelectSingleElement(message);
			var resendEFStatusRequestMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Resend EF Status Request");

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			resendEFStatusRequestMenuItem.PerformClick();
		}
	}
}
