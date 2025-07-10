using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(EDIMessageExporterGridContextMenuItemComponent))]
sealed class MessageTabUserControl_EDIMessageExporterGridContextMenuItemComponentTest : EDIMessageExporterGridContextMenuItemComponentTest
{
	public void TestSaveEdiFileMenuItemVisibility()
	{
		using (var form = GetNewZForm())
		using (var messageUserControl = GetNewMessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var messagesGrid = GetMessageGrid(messageUserControl);
			var saveEdiFileMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Save EDI file");

			CombineAssertions("[Case 1]: no messages selected", () =>
			{
				messagesGrid.UnSelectAll();
				AssertEquals("No selected elements", 0, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !saveEdiFileMenuItem.Visible);
			});

			CombineAssertions("[Case 2]: 1 message selected", () =>
			{
				messagesGrid.Select(0);
				AssertEquals("1 element selected", 1, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item visible", saveEdiFileMenuItem.Visible);
			});

			CombineAssertions("[Case 3]: more than 1 message selected", () =>
			{
				messagesGrid.SelectAllElements();
				AssertEquals("2 elements selected", 2, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item visible", saveEdiFileMenuItem.Visible);
			});
		}
	}

	public void TestSaveEdiFileMenuItemVisibility_Ucc6()
	{
		using (var form = GetNewZForm())
		using (var messageUserControl = GetNewMessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var messagesGrid = GetMessageGrid(messageUserControl);
			var saveEdiFileMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Save EDI file");

			CombineAssertions("Menu item visibility", () =>
			{
				declaration.JE_MessageType = "IMP";
				messagesGrid.Select(0);
				messagesGrid.ContextMenu.DoPopup();
				AssertEquals("For Import", false, saveEdiFileMenuItem.Visible);

				declaration.JE_MessageType = "EXP";
				messagesGrid.Select(0);
				messagesGrid.ContextMenu.DoPopup();
				AssertEquals("For Non Ucc6 Export", true, saveEdiFileMenuItem.Visible);

				using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
				{
					messagesGrid.Select(0);
					messagesGrid.ContextMenu.DoPopup();
					AssertEquals("For Ucc6 Export", false, saveEdiFileMenuItem.Visible);
				}
			});
		}
	}

	protected override ZForm GetNewZForm() => new ZForm(declaration);

	protected override ZUserControl GetNewMessageUserControl() => new MessageUserControl();

	protected override ZGrid GetMessageGrid(object parentUserControl)
	{
		var messageUserControl = ((MessageUserControl)parentUserControl);
		var tabControl = messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl");
		tabControl.SelectedTab = messageUserControl.FindSingle<ZTabPage>("MessageTabPage");
		var userControlMessages = (MessagesTabUserControl)messageUserControl.BaseMessageUserControl.HostedControl;
		return userControlMessages.FindSingle<ZGrid>("MessagesGrid");
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		sentMessage = entryHeader.Messages.AddNew();
		sentMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		sentMessage.EM_MessageText = idocMessageText;
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("000001");

		sentInterchange = Factory.New<ITEDIInterchange>();
		sentInterchange.ContainedMessages.Add(sentMessage);
		sentInterchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.ITCustoms;
		sentInterchange.EI_From = "FROM";
		sentInterchange.EI_To = "TO";
		sentInterchange.EI_SessionGUID = new ZGuid("8C2900CD-E6F1-482E-9D4A-0D4C60BA1292");
		sentInterchange.EI_Status = EDIInterchange.Status.Sent;
		sentInterchange.IsTransmitInterchange = true;
		sentInterchange.EI_HeaderText = idocInterchangeHeaderText;
		sentInterchange.EI_InterchangeType = SADConstants.CustomsInterchangeType.IdocR;

		receivedMessage = entryHeader.Messages.AddNew();
		receivedMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IrispX;
		receivedMessage.EM_MessageText = irispMessageText;
		receivedMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("000002");

		receivedInterchange = Factory.New<ITEDIInterchange>();
		receivedInterchange.ContainedMessages.Add(receivedMessage);
		receivedInterchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.ITCustoms;
		receivedInterchange.EI_From = "FROM";
		receivedInterchange.EI_To = "TO";
		receivedInterchange.EI_SessionGUID = new ZGuid("9C99B9D0-7B80-412A-91B5-83875DC7B659");
		receivedInterchange.EI_Status = EDIInterchange.Status.Sent;
		receivedInterchange.IsTransmitInterchange = true;
		receivedInterchange.EI_HeaderText = irispInterchangeHeaderText;
		receivedInterchange.EI_InterchangeType = SADConstants.CustomsInterchangeType.IrispX;

		Factory.Save();
	}

	JobDeclaration declaration;
	EDIInterchange sentInterchange;
	EDIInterchange receivedInterchange;
	ITEDIMessage sentMessage;
	ITEDIMessage receivedMessage;
}
