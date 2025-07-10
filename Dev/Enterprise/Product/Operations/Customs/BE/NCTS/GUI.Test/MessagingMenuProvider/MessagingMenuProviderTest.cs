using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NctsHeader = Enterprise.Customs.BE.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.BE.NCTS.GUI.Testing;

sealed class MessagingMenuProviderTest : TestCaseWithFactory
{
	public void TestCreateMenuItems()
	{
		AssertContainsExactElementsInExactOrder(
			new[] {
				"Send to Customs",
				"Make Arrival Notification for this Departure",
				"Undo Amended Data",
				"Request Customs to Resend All Existing Answers",
				"Request TAD from Customs",
				"-",
				"Inventory Management",
				"TS Register Management",
				"-",
				"Import Entry Lines",
				"Import Invoice Lines",
				"&Copy Previous Goods Item",
				"Lock Customs Declaration",
				"Unlock Customs Declaration"
			},
			menuItems.Select(x => x.Text));
	}

	public void TestRefreshMenu_Departure()
	{
		header.BH_HeaderType = NctsMovementType.Codes.Departure;
		header.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
		provider.RefreshMenu();
		AssertContainsExactElementsInExactOrder(new[] { "Send to Customs", "Request Customs to Resend All Existing Answers", "-", "Import Entry Lines", "Import Invoice Lines", "&Copy Previous Goods Item" },
			menuItems.Where(x => x.Visible).Select(x => x.Text));
	}

	public void TestRefreshMenu_Arrival()
	{
		header.BH_HeaderType = NctsMovementType.Codes.Arrival;
		header.EffectiveMessageStatus = LogicalStatusList.Codes.Acknowledged;
		provider.RefreshMenu();
		AssertContainsExactElementsInExactOrder(new[] { "Send to Customs", "Request Customs to Resend All Existing Answers", "-", "Import Invoice Lines" },
			menuItems.Where(x => x.Visible).Select(x => x.Text));
	}

	public void TestGetProvider()
	{
		AssertType<MessagingMenuProvider>(Phase5MessagingMenuProvider.GetProvider(Factory.New<NctsHeader>()));
	}

	public void TestCanSendToCustoms_Departure()
	{
		var menuItem = menuItems.FirstOrDefault(x => x.Caption.ToString() == "Send to Customs");
		foreach (var messageStatus in new NctsMessageStatusList().GetAllCodes())
		{
			TestCanSendToCustoms(ExpectSend(messageStatus), menuItem, NctsMovementType.Codes.Departure, messageStatus);
		}
	}

	public void TestCanSendToCustoms_Arrival()
	{
		var menuItem = menuItems.FirstOrDefault(x => x.Caption.ToString() == "Send to Customs");
		foreach (var messageStatus in new NctsMessageStatusList().GetAllCodes())
		{
			TestCanSendToCustoms(ExpectSend(messageStatus), menuItem, NctsMovementType.Codes.Arrival, messageStatus);
		}
	}

	bool ExpectSend(string messageStatus)
	{
		return messageStatus != NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit
			&& messageStatus != EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.NoFullReleaseOfGoodsMovementRemainsOpen
			&& messageStatus != NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed
			&& messageStatus != NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease
			&& messageStatus != NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease
			&& messageStatus != NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease
			&& messageStatus != NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
	}

	void TestCanSendToCustoms(bool expected, ZMenuItem menuItem, string headerType, string messageStatus)
	{
		header.BH_HeaderType = headerType;
		header.EffectiveMessageStatus = messageStatus;
		provider.RefreshMenu();
		AssertEquals($"headerType: {headerType}, messageStatus: {messageStatus}", expected, menuItem.Visible);
	}

	public void TestSendExportDeclaration()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		TestSendMessage(NctsMessageTypeList.Codes.Declaration, "CC015C");
	}

	public void TestSendRequestARelease()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		TestSendMessage(NctsMessageTypeList.Codes.RequestARelease, "CC054C");
	}

	public void TestSendInvalidationRequest()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		TestSendMessage(NctsMessageTypeList.Codes.InvalidationCancellation, "CC014C");
	}

	public void TestSendPresentationNotification()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		TestSendMessage(NctsMessageTypeList.Codes.PresentationNotification, "CC170C");
	}

	public void TestSendUnloadingRemarks()
	{
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		header.UnloadingRemark.G9_UnloadingDate = ZDateTime.Now;
		TestSendMessage(NctsMessageTypeList.Codes.UnloadingRemarks, "CC044C");
	}

	public void TestSendNonArrivedInformation()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		TestSendMessage(NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement, "CC141C");
	}

	public void TestSendAmendment()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		TestSendMessage(NctsMessageTypeList.Codes.Amendment, "CC013C");
	}

	public void TestSendArrivalNotification()
	{
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		TestSendMessage(NctsMessageTypeList.Codes.ArrivalNotification, "CC007C");
	}

	public void TestSendNonArrivedInformation_SaveDestinationCustomsOffice()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		header.MovementHeader.DestinationCustomsOfficeCodeForDeparture = "BE000000";
		using (var form = new Phase5DepartureMovementForm(header))
		{
			form.Show();

			var menu = (Phase5NctsMessagingMenuItem)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&NCTS");
			var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
			{
				var dialog = (MessageSendingForm)obj;
				var action = dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
				action.EntryType = NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement;
				action.QueryInformation = "filled";
				action.ActualOfficeOfDestination = "BE101000";
				action.ShouldSend = true;
			});

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			sendToCustomsMenu.PerformClick();
			AssertEquals("Destination Office has been saved.", "BE101000", header.MovementHeader.DestinationCustomsOfficeCode);
		}
	}

	public void TestSendNonArrivedInformation_SaveDestinationCustomsOfficeFailedSave()
	{
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var unusedOrgHeaderToFailFactorySave = Factory.New<OrgHeader>();
		header.MovementHeader.DestinationCustomsOfficeCodeForDeparture = "BE000000";
		using (var form = new Phase5DepartureMovementForm(header))
		{
			form.Show();

			var menu = (Phase5NctsMessagingMenuItem)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&NCTS");
			var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
			{
				var dialog = (MessageSendingForm)obj;
				var action = dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
				action.EntryType = NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement;
				action.QueryInformation = "filled";
				action.ActualOfficeOfDestination = "BE101000";
				action.ShouldSend = true;
			});

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			sendToCustomsMenu.PerformClick();
			AssertEquals("Destination Office should be set back because save failed.", "BE000000", header.MovementHeader.DestinationCustomsOfficeCode);
		}
	}

	public void TestDownloadTADMenuItem()
	{
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, "REL", ZDateTimeOffset.Now);

		Factory.Save();

		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		var menu = new MessagingMenuProvider(nctsHeader);
		var menuItems = menu.CreateMenuItems().ToArray();
		var downloadTADMenuItem = menuItems.FindByText("Request TAD from Customs");

		menu.RefreshMenu();
		downloadTADMenuItem.PerformClick();

		var query = new ZQuery();
		query.AddToFilter(new ZQuery(EDIMessageSchema.EM_MessageType, "NCT"));
		query.AddToFilter(new ZQuery(EDIMessageSchema.EM_MessageSubType, "TAD"));

		AssertNotNull(Factory.LoadTop1<EDIMessage>(query));
	}

	public void TestMenuItemVisibility_CanDownloadTADMenuItem()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

		using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
		{
			var menu = new MessagingMenuProvider(nctsHeader);
			var menuItems = menu.CreateMenuItems().ToArray();
			var downloadTADMenuItem = menuItems.FindByText("Request TAD from Customs");

			CombineAssertions(() =>
			{
				menu.RefreshMenu();
				AssertEquals("Request TAD from Customs MenuItem is not visible when there is no CES-REL event", false, downloadTADMenuItem.Visible);

				nctsHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, "REL", ZDateTimeOffset.Now);
				menu.RefreshMenu();
				AssertEquals("Request TAD from Customs  MenuItem is visible when there is a CES-REL event", true, downloadTADMenuItem.Visible);
			});
		}
	}

	void TestSendMessage(ZString entryType, string expectedMessageType)
	{
		switch (header.BH_HeaderType)
		{
			case NctsMovementType.Codes.Arrival:
				using (var form = new Phase5ArrivalMovementForm(header))
				{
					doSendAction(entryType, form, expectedMessageType);
				}
				break;
			case NctsMovementType.Codes.Departure:
				using (var form = new Phase5DepartureMovementForm(header))
				{
					doSendAction(entryType, form, expectedMessageType);
				}
				break;
		}
	}

	void doSendAction(ZString entryType, ZTemplateForm form, string expectedMessageType)
	{
		form.Show();

		var menu = (Phase5NctsMessagingMenuItem)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&NCTS");
		var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");

		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
		{
			var dialog = (MessageSendingForm)obj;
			var action = dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
			action.EntryType = entryType;
			action.ShouldSend = true;
		});

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		sendToCustomsMenu.PerformClick();
		CombineAssertions(() =>
		{
			AssertEquals("The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			EDIMessage sentMessage;
			if (header.IsDepartureMovement)
			{
				sentMessage = (EDIMessage)header.DepartureMovementHeaders[0].Messages.Single();
			}
			else
			{
				sentMessage = (EDIMessage)header.Messages.Single();
			}

			AssertContains($"<messageType>{expectedMessageType}</messageType>", sentMessage.EM_MessageText);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		provider = new MessagingMenuProvider(header);
		menuItems = provider.CreateMenuItems();
	}
	NctsHeader header;
	MessagingMenuProvider provider;
	IEnumerable<ZMenuItem> menuItems;
}
