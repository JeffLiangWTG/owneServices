using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(SearchCustomsMessagesUpdateGridContextMenuComponent))]
sealed class SearchCustomsMessagesUpdateGridContextMenuItemComponentTest : GridContextMenuItemComponentAbstractTest<ITEDIMessage>
{
	public void TestSearchCustomsMessagesUpdatesMenuItemVisibility()
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
			var searchCustomsMessagesUpdatesMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Search Customs Messages Updates");

			CombineAssertions("[Case 1]: no messages selected", () =>
			{
				messagesGrid.UnSelectAll();
				AssertEquals("No selected elements", 0, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !searchCustomsMessagesUpdatesMenuItem.Visible);
			});

			CombineAssertions("[Case 2]: more than 1 message selected", () =>
			{
				messagesGrid.SelectAllElements();
				AssertEquals("2 elements selected", 2, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !searchCustomsMessagesUpdatesMenuItem.Visible);
			});

			CombineAssertions("[Case 3]: the selected message has no interchange", () =>
			{
				messagesGrid.UnSelectAll();
				messagesGrid.Select(0);
				AssertEquals("1 element selected", 1, messagesGrid.SelectedElements.Length);
				AssertNull("Selected element has no interchange", (messagesGrid.SelectedElements[0] as EDIMessage).Interchange);

				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !searchCustomsMessagesUpdatesMenuItem.Visible);
			});

			var interchange = Factory.New<EDIInterchange>();
			interchange.ContainedMessages.Add(message1);

			CombineAssertions("[Case 4]: EM_MessageType NOT 'R'", () =>
			{
				messagesGrid.Select(0);
				AssertEquals("1 element selected", 1, messagesGrid.SelectedElements.Length);
				AssertNotNull("Selected element has interchange", (messagesGrid.SelectedElements[0] as EDIMessage).Interchange);

				message1.EM_MessageType = SADConstants.CustomsInterchangeType.IrispX;
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !searchCustomsMessagesUpdatesMenuItem.Visible);
			});

			CombineAssertions("[Case 5]: EM_MessageType = 'R'", () =>
			{
				messagesGrid.Select(0);
				AssertEquals("1 element selected", 1, messagesGrid.SelectedElements.Length);
				AssertNotNull("Selected element has interchange", (messagesGrid.SelectedElements[0] as EDIMessage).Interchange);

				message1.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item visible", searchCustomsMessagesUpdatesMenuItem.Visible);
			});
		}
	}

	public void TestSearchCustomsMessagesUpdatesMenuItemClick()
	{
		var sentInterchange = Factory.New<ITEDIInterchange>();
		sentInterchange.IsTransmitInterchange = true;
		sentInterchange.EI_From = "XXX";
		sentInterchange.EI_To = "YYY";
		sentInterchange.EI_HeaderText = @"
	<ITMessage>
		<Staff>BOB</Staff>
		<Node>845A</Node>
		<MessageType>R</MessageType>
		<AccountNumber>13149600150-003</AccountNumber>
		<Header>845A            845A0108.R01            137100    13149600150     003 00003</Header>
	</ITMessage>";

		var sentMessage = Factory.New<ITEDIMessage>();
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("000001");
		sentMessage.EM_EI = sentInterchange.PK;
		sentInterchange.ContainedMessages.Add(sentMessage);
		sentMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.Messages.Add(sentMessage);

		using (var form = new ZForm(declaration))
		using (var messageUserControl = new MessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var tabControl = messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl");
			tabControl.SelectedTab = messageUserControl.FindSingle<ZTabPage>("MessageTabPage");
			var userControlMessages = (MessagesTabUserControl)messageUserControl.BaseMessageUserControl.HostedControl;
			var messagesGrid = userControlMessages.FindSingle<ZGrid>("MessagesGrid");
			var searchCustomsMessagesUpdatesMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Search Customs Messages Updates");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			searchCustomsMessagesUpdatesMenuItem.PerformClick();
			AssertEquals("Customs updates have been requested for '845A0108.R01'.", UnitTestUserNotification.Instance.LastMessage.Text);

			sentMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IrispX;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			searchCustomsMessagesUpdatesMenuItem.PerformClick();
			AssertEquals("'X' is not a supported message type.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	protected override string MenuItemText => "Search Customs Messages Updates";

	protected override GridContextMenuItemComponent<ITEDIMessage> GetNewContextMenuItemComponent(ZGrid grid)
	{
		return new SearchCustomsMessagesUpdateGridContextMenuComponent(grid);
	}
}
