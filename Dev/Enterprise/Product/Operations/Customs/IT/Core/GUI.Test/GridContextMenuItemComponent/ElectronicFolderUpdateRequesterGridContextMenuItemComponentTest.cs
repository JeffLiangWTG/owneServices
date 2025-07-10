using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ElectronicFolderUpdateRequesterGridContextMenuItemComponent))]
sealed class ElectronicFolderUpdateRequesterGridContextMenuItemComponentTest : GridContextMenuItemComponentAbstractTest<ITEDIMessage>
{
	protected override string MenuItemText => "Resend EF Status Request";

	public void TestResendEFStatusRequestMenuItemVisibility()
	{
		using (var messagesGrid = new MessagesGridForTest(entryHeader))
		{
			new ElectronicFolderUpdateRequesterGridContextMenuItemComponent(messagesGrid).Initialize();

			var resendEFStatusRequestMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Resend EF Status Request");

			CombineAssertions("[Case 1]: no messages selected", () =>
			{
				messagesGrid.Messages.RemoveAndDeleteAll();
				AssertEquals("No selected elements", 0, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !resendEFStatusRequestMenuItem.Visible);
			});

			CombineAssertions("[Case 2]: 1 message selected", () =>
			{
				messagesGrid.Messages.AddNew();
				AssertEquals("1 element selected", 1, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item is visible", resendEFStatusRequestMenuItem.Visible);
			});

			CombineAssertions("[Case 3]: more than 1 message selected", () =>
			{
				messagesGrid.Messages.AddNew();
				AssertEquals("2 elements selected", 2, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item is not visible", !resendEFStatusRequestMenuItem.Visible);
			});

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			CombineAssertions("[Case 4]: Ucc6 Entry Header", () =>
			{
				messagesGrid.Messages.RemoveAndDeleteAll();
				messagesGrid.Messages.AddNew();
				AssertEquals("1 element selected", 1, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item is not visible", !resendEFStatusRequestMenuItem.Visible);
			});
		}
	}

	public void TestResendEFStatusRequestMenuItemClick_NoErrors()
	{
		using (var messagesGrid = new MessagesGridForTest(entryHeader))
		{
			new ElectronicFolderUpdateRequesterGridContextMenuItemComponent(messagesGrid).Initialize();

			Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-123456G", issueDate: ZDate.Today);
			messagesGrid.Messages.AddNew();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var resendEFStatusRequestMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Resend EF Status Request");
			resendEFStatusRequestMenuItem.PerformClick();
			AssertEquals("EF status update request has been sent to customs.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestResendEFStatusRequestMenuItemClick_WithError()
	{
		using (var messagesGrid = new MessagesGridForTest(entryHeader))
		{
			new ElectronicFolderUpdateRequesterGridContextMenuItemComponent(messagesGrid).Initialize();

			messagesGrid.Messages.AddNew();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var resendEFStatusRequestMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Resend EF Status Request");
			resendEFStatusRequestMenuItem.PerformClick();
			AssertEquals("This entry is not registered, the request cannot be performed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	protected override GridContextMenuItemComponent<ITEDIMessage> GetNewContextMenuItemComponent(ZGrid grid) =>
		new ElectronicFolderUpdateRequesterGridContextMenuItemComponent(grid);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
}
