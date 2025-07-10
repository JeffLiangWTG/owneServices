using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI.PlugIn;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class Phase5MessagingMenuProviderTest : TestCaseWithFactory
{
	public void TestGetProvider()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		AssertType<Phase5MessagingMenuProvider>("Provider Type", EU.NCTS.GUI.Phase5MessagingMenuProvider.GetProvider(nctsHeader));
	}

	public void TestMessageSendingForm()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var provider = new Phase5MessagingMenuProviderForTest(nctsHeader);

		var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
		using (var form = provider.GetMessageSendingFormExposed(sendingObjectParent))
		{
			AssertType<MessageSendingForm>("Message Sending Form Type", form);
		}
	}

	public void TestSendToCustomsCatchingExceptionAndIsSentInASeparateBusinessObjectFactory()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();

		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(nctsHeader);
		CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser();

		MockCryptoKiApi();

		using (var form = new ZForm(nctsHeader))
		{
			var messagingMenuProvider = new Phase5MessagingMenuProvider(nctsHeader);
			var menuItems = messagingMenuProvider.CreateMenuItems().ToArray();

			form.Menu.MenuItems.AddRange(menuItems);

			var sendToCustomsMenuItem = menuItems.SingleOrDefault(x => x.Caption == "Send to Customs");
			AssertNotNull("Send to Customs menu item", sendToCustomsMenuItem);

			UnitTestUserNotification.Instance.AddYesAnswer();
			ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;

			AssertNoExceptionThrown("Sending to Customs without user certificate", sendToCustomsMenuItem.PerformClick);

			CombineAssertions(() =>
			{
				AssertEquals("Last message showed to user",
					"Method C_GetTokenInfo returned CKR_TOKEN_NOT_RECOGNIZED",
					UnitTestUserNotification.Instance.LastMessage.Text.TrimEnd('\r', '\n'));
				AssertEquals("Ncts Header Has changes?", false, nctsHeader.HasChanges);
			});
		}
	}

	public void TestSetEntryAsAmendmentMenuAvailability()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		var provider = new Phase5MessagingMenuProvider(nctsHeader);
		var menuItems = provider.CreateMenuItems();

		CombineAssertions(() =>
		{
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
			provider.RefreshMenu();
			var setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Text == "Set Entry as Amendment");
			AssertNull("Arrival Header, Set Entry as Amendment menu item should not be present", setEntryAsAmendmentMenuItem);

			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			provider.RefreshMenu();
			setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Text == "Set Entry as Amendment");
			AssertNotNull("Departure Header, Set Entry as Amendment menu item should be present", setEntryAsAmendmentMenuItem);
			AssertEquals("Departure Header, Set Entry as Amendment menu item should be visible always", true, setEntryAsAmendmentMenuItem.Visible);
		});
	}

	public void TestSetEntryAsAmendmentMenuCatchingException()
	{
		AssertSetEntryAsAmendmentMenuCatchingException("MRN", "ACC", "015", true);
		AssertSetEntryAsAmendmentMenuCatchingException("REL", "ACC", "015", true);
		AssertSetEntryAsAmendmentMenuCatchingException("NRL", "ACC", "015", true);
		AssertSetEntryAsAmendmentMenuCatchingException("CO0", "ACC", "015", true);
		AssertSetEntryAsAmendmentMenuCatchingException("DEP", "ACC", "015", true);
		AssertSetEntryAsAmendmentMenuCatchingException(ZString.Empty, "FAL", "014", true);
		AssertSetEntryAsAmendmentMenuCatchingException(ZString.Empty, "ERR", "014", true);
		AssertSetEntryAsAmendmentMenuCatchingException("MRN", "ERR", "014", true);
		AssertSetEntryAsAmendmentMenuCatchingException("AMR", "ACC", "013", true);
		AssertSetEntryAsAmendmentMenuCatchingException("WRO", "ACC", "015", true);

		AssertSetEntryAsAmendmentMenuCatchingException("MRN", "ACC", "014", false);
		AssertSetEntryAsAmendmentMenuCatchingException(ZString.Empty, "FAL", "015", false);
	}

	public void TestSetEntryAsAmendmentMenu_WhenNctsIsLocked()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.MovementHeader.BM_CustomsStatus = "MRN";
		nctsHeader.EffectiveMessageStatus = "ACC";
		nctsHeader.MovementHeader.BM_Phase = "015";
		nctsHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, "MRN");
		nctsHeader.LockFile("Declaration is locked before amendment");

		using var form = new ZForm(nctsHeader);
		form.SetDataBinding(nctsHeader, ".");
		form.Show();

		var messagingMenuProvider = new Phase5MessagingMenuProvider(nctsHeader);
		messagingMenuProvider.RefreshMenu();
		var menuItems = messagingMenuProvider.CreateMenuItems().ToArray();
		form.Menu.MenuItems.AddRange(menuItems);

		var setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Caption == "Set Entry as Amendment");
		var unlockCustomsDeclarationMenuItem = menuItems.SingleOrDefault(x => x.Caption == "Unlock Customs Declaration");
		var lockCustomsDeclarationMenuItem = menuItems.SingleOrDefault(x => x.Caption == "Lock Customs Declaration");

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		var mrnValue = "24ITQYG08AAB1956J4";
		ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
		{
			var entryAmendmentForm = (EntryAmendmentForm)form;
			entryAmendmentForm.BusinessEntity.MovementReferenceNumber = mrnValue;
		});

		CombineAssertions("NCTS header amendment", () =>
		{
			AssertEquals(true, setEntryAsAmendmentMenuItem?.Enabled);
			AssertNoExceptionThrown("Sending to Customs without user certificate", () => setEntryAsAmendmentMenuItem?.PerformClick());
			AssertEquals("Last message showed to user", "One NCTS Departure Declaration was set to Amendment", UnitTestUserNotification.Instance.LastMessage.Text);
		});

		CombineAssertions("NCTS header status", () =>
		{
			AssertEquals("Ncts Header Has changes?", true, nctsHeader.HasChanges);
			AssertEquals(ZString.Empty, nctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals(ZString.Empty, nctsHeader.EffectiveMessageStatus);
			AssertEquals(mrnValue, nctsHeader.MovementReferenceNumber);
			AssertEquals(EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Amendment, nctsHeader.MovementHeader.BM_Phase);
			AssertEquals(false, nctsHeader.IsLocked);
		});

		CombineAssertions("Nenu items status", () =>
		{
			AssertEquals(false, unlockCustomsDeclarationMenuItem?.Visible);
			AssertEquals(true, lockCustomsDeclarationMenuItem?.Visible);
			AssertEquals(false, setEntryAsAmendmentMenuItem?.Enabled);
		});
	}

	public void TestSetEntryAsAmendmentMenuAvailability_Departure()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var provider = new Phase5MessagingMenuProvider(nctsHeader);
		var menuItems = provider.CreateMenuItems();
		provider.RefreshMenu();

		CombineAssertions(() =>
		{
			var setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Text == "Set Entry as Amendment");
			AssertSetEntryAsAmendmentMenuItem(setEntryAsAmendmentMenuItem, visible: true, enabled: true);

			nctsHeader.MovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Accepted;
			provider.RefreshMenu();
			setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Text == "Set Entry as Amendment");
			AssertSetEntryAsAmendmentMenuItem(setEntryAsAmendmentMenuItem, visible: true, enabled: false);

			nctsHeader.MovementHeader.BM_MessageStatus = ZString.Empty;
			nctsHeader.MovementHeader.BM_Phase = EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Arrival;
			provider.RefreshMenu();
			setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Text == "Set Entry as Amendment");
			AssertSetEntryAsAmendmentMenuItem(setEntryAsAmendmentMenuItem, visible: true, enabled: false);

			nctsHeader.MovementHeader.BM_Phase = ZString.Empty;
			nctsHeader.MovementHeader.BM_CustomsStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;
			provider.RefreshMenu();
			setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Text == "Set Entry as Amendment");
			AssertSetEntryAsAmendmentMenuItem(setEntryAsAmendmentMenuItem, visible: true, enabled: false);

			nctsHeader.MovementHeader.BM_CustomsStatus = ZString.Empty;
			var shipment = Factory.New<ForwardingShipment>();
			nctsHeader.BH_ParentID = shipment.PK;
			provider.RefreshMenu();
			setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Text == "Set Entry as Amendment");
			AssertSetEntryAsAmendmentMenuItem(setEntryAsAmendmentMenuItem, visible: true, enabled: false);
		});
	}

	public void TestSetEntryAsAmendmentMenuEnabled_WhenDeparture()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var provider = new Phase5MessagingMenuProvider(nctsHeader);
		var menuItems = provider.CreateMenuItems();
		ZMenuItem menuItem;

		var states = new (string CustomsStatus, string MessageStatus, string Phase)[]
		{
			("MRN", "ACC", "015"),
			("REL", "ACC", "015"),
			("NRL", "ACC", "015"),
			("CO0", "", "015"),
			("DEP", "", "015"),
			("", "FAL", "014"),
			("", "ERR", "014"),
			("AMR", "ACC", "013"),
			("WRO", "ACC", "015"),
			("", "", ""),
		};

		CombineAssertions(() =>
		{
			foreach (var (customsStatus, messageStatus, phase) in states)
			{
				nctsHeader.MovementHeader.BM_CustomsStatus = customsStatus;
				nctsHeader.MovementHeader.BM_MessageStatus = messageStatus;
				nctsHeader.MovementHeader.BM_Phase = phase;
				provider.RefreshMenu();
				menuItem = menuItems.SingleOrDefault(x => x.Text == "Set Entry as Amendment");
				AssertNotNull($"Set Entry as Amendment menu item should be present: '{customsStatus}' '{messageStatus}' '{phase}'", menuItem);
				AssertEquals($"Set Entry as Amendment menu item should be visible: '{customsStatus}' '{messageStatus}' '{phase}'", expected: true, menuItem.Visible);
				AssertEquals($"Set Entry as Amendment menu item should be enabled: '{customsStatus}' '{messageStatus}' '{phase}'", expected: true, menuItem.Enabled);
			}
		});
	}

	public void TestSetEntryAsAmendmentMenu_CancelOperation()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();

		using var form = new ZForm(nctsHeader);
		form.SetDataBinding(nctsHeader, ".");
		form.Show();

		var messagingMenuProvider = new Phase5MessagingMenuProvider(nctsHeader);
		messagingMenuProvider.RefreshMenu();
		var menuItems = messagingMenuProvider.CreateMenuItems().ToArray();
		form.Menu.MenuItems.AddRange(menuItems);

		var setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Caption == "Set Entry as Amendment");

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		setEntryAsAmendmentMenuItem.PerformClick();

		AssertEquals(typeof(EntryAmendmentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		AssertNull("If user presses Cancel, no feedback message is shown to user", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestSetEntryAsAmendmentMenu_WhenNctsIsLocked_MRN()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var mrnValue = "24ITQYG08AAB1956J4";
		var mrn = CusEntryNumber.New(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.CountryCode);
		mrn.CE_EntryNum = mrnValue;
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.MovementHeader.BM_CustomsStatus = "MRN";
		nctsHeader.EffectiveMessageStatus = "ACC";
		nctsHeader.MovementHeader.BM_Phase = "015";
		nctsHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, "MRN");
		nctsHeader.LockFile("Declaration is locked before amendment");

		using var form = new ZForm(nctsHeader);
		var messagingMenuProvider = new Phase5MessagingMenuProvider(nctsHeader);
		messagingMenuProvider.RefreshMenu();
		var menuItems = messagingMenuProvider.CreateMenuItems().ToArray();
		form.Menu.MenuItems.AddRange(menuItems);

		var setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Caption == "Set Entry as Amendment");
		var unlockCustomsDeclarationMenuItem = menuItems.SingleOrDefault(x => x.Caption == "Unlock Customs Declaration");
		var lockCustomsDeclarationMenuItem = menuItems.SingleOrDefault(x => x.Caption == "Lock Customs Declaration");

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

		CombineAssertions("NCTS header amendment", () =>
		{
			AssertEquals(true, setEntryAsAmendmentMenuItem?.Enabled);
			AssertNoExceptionThrown("Sending to Customs without user certificate", () => setEntryAsAmendmentMenuItem?.PerformClick());
			AssertEquals("Last message showed to user", "One NCTS Departure Declaration was set to Amendment", UnitTestUserNotification.Instance.LastMessage.Text);
		});

		CombineAssertions("NCTS header status", () =>
		{
			AssertEquals("Ncts Header Has changes?", true, nctsHeader.HasChanges);
			AssertEquals(ZString.Empty, nctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals(ZString.Empty, nctsHeader.EffectiveMessageStatus);
			AssertEquals(mrnValue, nctsHeader.MovementReferenceNumber);
			AssertEquals(EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Amendment, nctsHeader.MovementHeader.BM_Phase);
			AssertEquals(false, nctsHeader.IsLocked);
		});

		CombineAssertions("Menu items status", () =>
		{
			AssertEquals(false, unlockCustomsDeclarationMenuItem?.Visible);
			AssertEquals(true, lockCustomsDeclarationMenuItem?.Visible);
			AssertEquals(false, setEntryAsAmendmentMenuItem?.Enabled);
		});
	}

	public void TestSetEntryAsAmendmentMenu_MRN_CancelOperation()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var mrnValue = "24ITQYG08AAB1956J4";
		var mrn = CusEntryNumber.New(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.CountryCode);
		mrn.CE_EntryNum = mrnValue;
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.MovementHeader.BM_CustomsStatus = "MRN";
		nctsHeader.EffectiveMessageStatus = "ACC";
		nctsHeader.MovementHeader.BM_Phase = "015";
		nctsHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, "MRN");
		nctsHeader.LockFile("Declaration is locked before amendment");

		using var form = new ZForm(nctsHeader);
		var messagingMenuProvider = new Phase5MessagingMenuProvider(nctsHeader);
		messagingMenuProvider.RefreshMenu();
		var menuItems = messagingMenuProvider.CreateMenuItems().ToArray();
		form.Menu.MenuItems.AddRange(menuItems);

		var setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Caption == "Set Entry as Amendment");
		var unlockCustomsDeclarationMenuItem = menuItems.SingleOrDefault(x => x.Caption == "Unlock Customs Declaration");
		var lockCustomsDeclarationMenuItem = menuItems.SingleOrDefault(x => x.Caption == "Lock Customs Declaration");

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

		AssertNull("If user presses Cancel, no feedback message is shown to user", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	void AssertSetEntryAsAmendmentMenuCatchingException(ZString customStatus, ZString messageStatus, ZString phase, bool warningMessageWindowExpected)
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.MovementHeader.BM_CustomsStatus = customStatus;
		nctsHeader.EffectiveMessageStatus = messageStatus;
		nctsHeader.MovementHeader.BM_Phase = phase;

		using var form = new ZForm(nctsHeader);
		form.SetDataBinding(nctsHeader, ".");
		form.Show();

		var messagingMenuProvider = new Phase5MessagingMenuProvider(nctsHeader);
		messagingMenuProvider.RefreshMenu();
		var menuItems = messagingMenuProvider.CreateMenuItems().ToArray();
		form.Menu.MenuItems.AddRange(menuItems);

		var setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Caption == "Set Entry as Amendment");
		AssertNotNull("Set Entry as Amendment menu item", setEntryAsAmendmentMenuItem);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		var mrnValue = "24ITQYG08AAB1956J4";
		ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
		ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
		{
			var entryAmendmentForm = (EntryAmendmentForm)form;
			entryAmendmentForm.BusinessEntity.MovementReferenceNumber = mrnValue;
		});

		CombineAssertions($"When BM_CustomsStatus: {customStatus}, EffectiveMessageStatus: {messageStatus}, BM_Phase: {phase}", () =>
		{
			if (warningMessageWindowExpected)
			{
				AssertEquals(true, setEntryAsAmendmentMenuItem.Enabled);
				AssertNoExceptionThrown("Sending to Customs without user certificate", () => setEntryAsAmendmentMenuItem.PerformClick());
				AssertEquals("Last message showed to user", "One NCTS Departure Declaration was set to Amendment", UnitTestUserNotification.Instance.LastMessage.Text.TrimEnd('\r', '\n'));
				AssertEquals("Ncts Header Has changes?", true, nctsHeader.HasChanges);
				AssertEquals(ZString.Empty, nctsHeader.MovementHeader.BM_CustomsStatus);
				AssertEquals(ZString.Empty, nctsHeader.EffectiveMessageStatus);
				AssertEquals(mrnValue, nctsHeader.MovementReferenceNumber);
				AssertEquals(EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Amendment, nctsHeader.MovementHeader.BM_Phase);
			}
			else
			{
				AssertEquals(false, setEntryAsAmendmentMenuItem.Enabled);
			}
		});
	}

	void AssertSetEntryAsAmendmentMenuItem(ZMenuItem menuItem, bool visible, bool enabled)
	{
		AssertNotNull("Set Entry as Amendment menu item should be present", menuItem);
		AssertEquals("Set Entry as Amendment menu item should be visible", expected: visible, menuItem.Visible);
		AssertEquals("Set Entry as Amendment menu item should be enabled", expected: enabled, menuItem.Enabled);
	}

	static void MockCryptoKiApi()
	{
		var cryptoApiMock = new Mock<ICryptoApi>();
		cryptoApiMock
			.Setup(x =>
				x.SignXadesWithToken(
					It.IsAny<byte[]>(),
					It.IsAny<Chipset>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<DateTime>())
				)
			.Throws(new AidaXmlSignerException("Method C_GetTokenInfo returned CKR_TOKEN_NOT_RECOGNIZED", new Exception()));

		ObjectFactory.Substitute(cryptoApiMock.Object);
	}

	class Phase5MessagingMenuProviderForTest : Phase5MessagingMenuProvider
	{
		public Phase5MessagingMenuProviderForTest(NctsHeader header) : base(header)
		{
		}

		public EU.NCTS.GUI.MessageSendingForm GetMessageSendingFormExposed(EU.NCTS.Business.NctsHeaderMessageSendingObjectParent messageSendingObjectParent)
			=> GetMessageSendingFormCore(messageSendingObjectParent);
	}
}
