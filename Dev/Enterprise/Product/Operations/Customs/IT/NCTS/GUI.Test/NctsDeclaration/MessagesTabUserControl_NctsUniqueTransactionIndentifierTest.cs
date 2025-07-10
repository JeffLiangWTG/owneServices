using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class MessagesTabUserControl_NctsUniqueTransactionIdentifierRequestTest : TestCaseWithFactory
{
	[RequiresSTA]
	public void TestMenuItemVisibility_WhenNoMessageSelected()
	{
		using (var form = new ZForm(nctsHeader))
		using (var userControl = new MessagesTabUserControl())
		{
			SetBindingsAndShowForm(form, userControl);

			var messagesGrid = userControl.FindSingle<ZGrid>("MessageGrid");
			messagesGrid.UnSelectAll();

			AssertEquals("PRE-CONDITION: Selected Messages", 0, messagesGrid.SelectedElements.Length);
			messagesGrid.ContextMenu.DoPopup();
			AssertMenuItemVisibility(messagesGrid, expectedVisible: false);
		}
	}

	[RequiresSTA]
	public void TestMenuItemVisibility_WhenMoreThanOneMessageSelected()
	{
		var unknownMessage = movementHeader.Messages.AddNew();
		unknownMessage.EM_MessageType = "XXX";
		var acknowledgeMessage = movementHeader.Messages.AddNew();
		acknowledgeMessage.EM_MessageType = "ACK";

		using (var form = new ZForm(nctsHeader))
		using (var userControl = new MessagesTabUserControl())
		{
			SetBindingsAndShowForm(form, userControl);

			var messagesGrid = userControl.FindSingle<ZGrid>("MessageGrid");
			messagesGrid.SelectAllElements();

			AssertEquals("PRE-CONDITION: Selected Messages", 2, messagesGrid.SelectedElements.Length);
			messagesGrid.ContextMenu.DoPopup();
			AssertMenuItemVisibility(messagesGrid, expectedVisible: false);
		}
	}

	[RequiresSTA]
	public void TestMenuItemVisibility_WhenOnlyOneNotAcknowledgeMessageSelected()
	{
		var unknownMessage = movementHeader.Messages.AddNew();
		unknownMessage.EM_MessageType = "XXX";

		using (var form = new ZForm(nctsHeader))
		using (var userControl = new MessagesTabUserControl())
		{
			SetBindingsAndShowForm(form, userControl);

			var messagesGrid = userControl.FindSingle<ZGrid>("MessageGrid");
			messagesGrid.Select(0);

			AssertEquals("PRE-CONDITION: Selected Messages", 1, messagesGrid.SelectedElements.Length);
			messagesGrid.ContextMenu.DoPopup();
			AssertMenuItemVisibility(messagesGrid, expectedVisible: false);
		}
	}

	[RequiresSTA]
	public void TestMenuItemVisibility_WhenOnlyOneAcknowledgeMessageSelected()
	{
		var acknowledgeMessage = movementHeader.Messages.AddNew();
		acknowledgeMessage.EM_MessageType = "ACK";

		using (var form = new ZForm(nctsHeader))
		using (var userControl = new MessagesTabUserControl())
		{
			SetBindingsAndShowForm(form, userControl);

			var messagesGrid = userControl.FindSingle<ZGrid>("MessageGrid");
			messagesGrid.Select(0);

			AssertEquals("PRE-CONDITION: Selected Messages", 1, messagesGrid.SelectedElements.Length);
			messagesGrid.ContextMenu.DoPopup();
			AssertMenuItemVisibility(messagesGrid, expectedVisible: true);
		}
	}

	[RequiresSTA]
	public void TestMenuItemClick_WhenIUTCannotBeFound()
	{
		var acknowledgeMessage = movementHeader.Messages.AddNew();
		acknowledgeMessage.EM_MessageType = "ACK";
		acknowledgeMessage.IsTransmitMessage = false;

		using (var form = new ZForm(nctsHeader))
		using (var userControl = new MessagesTabUserControl())
		{
			SetBindingsAndShowForm(form, userControl);

			var messagesGrid = userControl.FindSingle<ZGrid>("MessageGrid");
			messagesGrid.Select(0);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var menuItem = GetMenuItem(messagesGrid);
			menuItem.PerformClick();

			AssertEquals(
				"Prompted Notification Message",
				"The request cannot be processed as the unique transaction identifier (IUT) cannot be found.",
				UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	[RequiresSTA]
	public void TestMenuItemClick_WhenIUTCanBeFoundAndUserConfirmOperation()
	{
		var acknowledgeMessage = movementHeader.Messages.AddNew();
		acknowledgeMessage.EM_MessageType = "ACK";
		acknowledgeMessage.EM_MessageText = "<ns2:IUT>20220307D11000328189</ns2:IUT>";
		acknowledgeMessage.EM_MessageNum = "123456";
		acknowledgeMessage.IsTransmitMessage = false;

		using (var form = new ZForm(nctsHeader))
		using (var userControl = new MessagesTabUserControl())
		{
			SetBindingsAndShowForm(form, userControl);

			var messagesGrid = userControl.FindSingle<ZGrid>("MessageGrid");
			messagesGrid.Select(0);
			var menuItem = GetMenuItem(messagesGrid);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			menuItem.PerformClick();

			AssertEquals(
				"[PRE-CONDITION] Number of previous messages should be 3",
				true,
				UnitTestUserNotification.Instance.PreviousMessages.Length > 2);

			CombineAssertions(() =>
			{
				AssertEquals(
					"Job pending changes, Prompted Notification Message",
					"The Job has not yet been saved. Do you want to save and proceed?",
					UnitTestUserNotification.Instance.PreviousMessages[2].Text);

				AssertEquals(
					"Confirm to request, Prompted Notification Message",
					"Do you confirm to request responses for message 123456?",
					UnitTestUserNotification.Instance.PreviousMessages[1].Text);

				AssertEquals(
					"Message created, Prompted Notification Message",
					"The request message has been created.",
					UnitTestUserNotification.Instance.LastMessage.Text);

				var requestResponseMessage = movementHeader.Messages.GetLastMessageByType("IUT");
				AssertNotNull("IUT - Request Response Message", requestResponseMessage);
			});
		}
	}

	[RequiresSTA]
	public void TestMenuItemClick_WhenIUTCanBeFoundAndUserAbortOperation()
	{
		var acknowledgeMessage = movementHeader.Messages.AddNew();
		acknowledgeMessage.EM_MessageType = "ACK";
		acknowledgeMessage.EM_MessageText = "<ns2:IUT>20220307D11000328189</ns2:IUT>";
		acknowledgeMessage.EM_MessageNum = "123456";
		acknowledgeMessage.IsTransmitMessage = false;

		using (var form = new ZForm(nctsHeader))
		using (var userControl = new MessagesTabUserControl())
		{
			SetBindingsAndShowForm(form, userControl);

			var messagesGrid = userControl.FindSingle<ZGrid>("MessageGrid");
			messagesGrid.Select(0);
			var menuItem = GetMenuItem(messagesGrid);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			menuItem.PerformClick();

			AssertEquals(
				"[PRE-CONDITION] Number of previous messages should be 2",
				true,
				UnitTestUserNotification.Instance.PreviousMessages.Length > 1);

			CombineAssertions(() =>
			{
				AssertEquals(
					"Job pending changes, Prompted Notification Message",
					"The Job has not yet been saved. Do you want to save and proceed?",
					UnitTestUserNotification.Instance.PreviousMessages[1].Text);

				AssertEquals(
					"Confirm to request, Prompted Notification Message",
					"Do you confirm to request responses for message 123456?",
					UnitTestUserNotification.Instance.LastMessage.Text);

				var requestResponseMessage = movementHeader.Messages.GetLastMessageByType("IUT");
				AssertNull("IUT - Request Response Message", requestResponseMessage);
			});
		}
	}

	[RequiresSTA]
	public void TestMenuItemClick_WhenIUTCanBeFoundAndUserAbortJobPendingChanges()
	{
		var acknowledgeMessage = movementHeader.Messages.AddNew();
		acknowledgeMessage.EM_MessageType = "ACK";
		acknowledgeMessage.EM_MessageText = "<ns2:IUT>20220307D11000328189</ns2:IUT>";
		acknowledgeMessage.EM_MessageNum = "123456";
		acknowledgeMessage.IsTransmitMessage = false;

		using (var form = new ZForm(nctsHeader))
		using (var userControl = new MessagesTabUserControl())
		{
			SetBindingsAndShowForm(form, userControl);

			var messagesGrid = userControl.FindSingle<ZGrid>("MessageGrid");
			messagesGrid.Select(0);
			var menuItem = GetMenuItem(messagesGrid);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			menuItem.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals(
					"Job pending changes, Prompted Notification Message",
					"The Job has not yet been saved. Do you want to save and proceed?",
					UnitTestUserNotification.Instance.LastMessage.Text);

				var requestResponseMessage = movementHeader.Messages.GetLastMessageByType("IUT");
				AssertNull("IUT - Request Response Message", requestResponseMessage);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_ApplicationCode = "NC5";
		movementHeader = nctsHeader.MovementHeader;
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;

	MenuItem GetMenuItem(ZGrid messagesGrid) => messagesGrid.ContextMenu.MenuItems.FindByName("RequestResponsesMenuItem");

	void AssertMenuItemVisibility(ZGrid messagesGrid, bool expectedVisible)
	{
		var menuItem = GetMenuItem(messagesGrid);
		AssertEquals("MenuItem Visible", expectedVisible, menuItem.Visible);
	}

	void SetBindingsAndShowForm(ZForm form, MessagesTabUserControl userControl)
	{
		userControl.SetDataBinding(nctsHeader, "MovementHeader.Messages");
		form.Controls.Add(userControl);
		form.Show();
	}
}
