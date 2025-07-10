using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI.PlugIn;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using TemporaryStorageHeader = Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(TemporaryStorageMessagesMenu))]
sealed class TemporaryStorageMessagesMenuTest : TestCaseWithFactory
{
	public void TestMessageSendingForm()
	{
		using var form = new TemporaryStorageForm(header);

		var sendMessageMenuItem = form.Menu.MenuItems.FindByText("Send To Customs", true);
		sendMessageMenuItem.PerformClick();
		AssertType<MessageSendingForm>("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
	}

	public void TestSendToCustoms()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(header);
		CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser();
		Factory.Save();

		MockCryptokiApiWithResult();

		using var form = new TemporaryStorageForm(header);
		var sendMessageMenuItem = form.Menu.MenuItems.FindByText("Send To Customs", true);

		UnitTestUserNotification.Instance.AddYesAnswer();
		ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;

		sendMessageMenuItem.PerformClick();
		AssertEquals("Last message showed to user", "The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestSendToCustomsCatchingExceptionAndIsSentInASeparateBusinessObjectFactory()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(header);
		CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser();

		MockCryptokiApiWithException();

		using var form = new TemporaryStorageForm(header);
		var sendMessageMenuItem = form.Menu.MenuItems.FindByText("Send To Customs", true);

		UnitTestUserNotification.Instance.AddYesAnswer();
		ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;

		AssertNoExceptionThrown("Sending to Customs without user certificate", sendMessageMenuItem.PerformClick);

		CombineAssertions(() =>
		{
			AssertEquals("Last message showed to user",
				"Method C_GetTokenInfo returned CKR_TOKEN_NOT_RECOGNIZED",
				UnitTestUserNotification.Instance.LastMessage.Text.TrimEnd('\r', '\n'));
			AssertEquals("Header Has changes?", false, header.HasChanges);
		});
	}

	public void TestSetEntryAsAmendmentMenu_Enabled()
	{
		using var form = new TemporaryStorageForm(header);
		var provider = new TemporaryStorageMessagesMenuTestForTest(form);

		var menuItems = provider.CreateMenuItems_Exposed();
		provider.RefreshMenuItems_Exposed();

		CombineAssertions(() =>
		{
			var setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Text == SetAmendmentMenuCaption);
			AssertSetEntryAsAmendmentMenuItem(setEntryAsAmendmentMenuItem, visible: true, enabled: false);

			header.CustomsStatus = PNTSCustomsStatusList.Codes.PartialActivated;
			provider.RefreshMenuItems_Exposed();
			setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Text == SetAmendmentMenuCaption);
			AssertSetEntryAsAmendmentMenuItem(setEntryAsAmendmentMenuItem, visible: true, enabled: false);

			header.CustomsStatus = PNTSCustomsStatusList.Codes.FullyActivated;
			provider.RefreshMenuItems_Exposed();
			setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Text == SetAmendmentMenuCaption);
			AssertSetEntryAsAmendmentMenuItem(setEntryAsAmendmentMenuItem, visible: true, enabled: true);
		});
	}

	void AssertSetEntryAsAmendmentMenuItem(ZMenuItem menuItem, bool visible, bool enabled)
	{
		AssertNotNull("Set Entry as Amendment menu item should be present", menuItem);
		AssertEquals("Set Entry as Amendment menu item should be visible", expected: visible, menuItem.Visible);
		AssertEquals("Set Entry as Amendment menu item should be enabled", expected: enabled, menuItem.Enabled);
	}

	public void TestSetEntryAsAmendmentMenu_OkOperation()
	{
		var bill = header.MasterBill;
		var mrn = CusEntryNumber.New(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber, bill.Header.CountryCode);
		mrn.CE_EntryNum = "MRN123";
		header.CustomsStatus = PNTSCustomsStatusList.Codes.FullyActivated;

		using var form = new ZForm(header);
		var messagingMenuProvider = new TemporaryStorageMessagesMenuTestForTest(form);
		messagingMenuProvider.RefreshMenuItems_Exposed();
		var menuItems = messagingMenuProvider.CreateMenuItems_Exposed().ToArray();
		form.Menu.MenuItems.AddRange(menuItems);

		var setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Caption == SetAmendmentMenuCaption);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

		CombineAssertions("Temporary storage header amendment", () =>
		{
			AssertEquals(true, setEntryAsAmendmentMenuItem?.Enabled);
			AssertNoExceptionThrown("Sending to Customs without user certificate", () => setEntryAsAmendmentMenuItem?.PerformClick());
			AssertEquals("Last message showed to user", "One Entry was set to Amending", UnitTestUserNotification.Instance.LastMessage.Text);
		});

		CombineAssertions("Temporary storage header status", () =>
		{
			AssertEquals("Temporary storage header has changes?", true, header.HasChanges);
			AssertEquals("AMG", header.CustomsStatus);
			AssertEquals(ZString.Empty, header.AMA_MessageStatus);
		});

		messagingMenuProvider.RefreshMenuItems_Exposed();
		AssertEquals("Menu item status", false, setEntryAsAmendmentMenuItem?.Enabled);
	}

	public void TestSetEntryAsAmendmentMenu_CancelOperation()
	{
		using var form = new ZForm(header);
		form.SetDataBinding(header, ".");
		form.Show();

		var provider = new TemporaryStorageMessagesMenuTestForTest(form);
		provider.RefreshMenuItems_Exposed();
		var menuItems = provider.CreateMenuItems_Exposed().ToArray();
		form.Menu.MenuItems.AddRange(menuItems);

		var setEntryAsAmendmentMenuItem = menuItems.SingleOrDefault(x => x.Caption == SetAmendmentMenuCaption);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		setEntryAsAmendmentMenuItem.PerformClick();

		AssertEquals(typeof(EntryAmendmentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		AssertNull("If user presses Cancel, no feedback message is shown to user", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<TemporaryStorageHeader>();
	}

	TemporaryStorageHeader header;

	const string SetAmendmentMenuCaption = "Set Entry as Amendment";

	static void MockCryptokiApiWithException()
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

	static void MockCryptokiApiWithResult()
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
			.Returns(Array.Empty<byte>());

		ObjectFactory.Substitute(cryptoApiMock.Object);
	}

	class TemporaryStorageMessagesMenuTestForTest : TemporaryStorageMessagesMenu
	{
		public TemporaryStorageMessagesMenuTestForTest(ZForm parentForm) : base(parentForm)
		{
		}

		public IEnumerable<ZMenuItem> CreateMenuItems_Exposed() => base.CreateMenuItems();

		public void RefreshMenuItems_Exposed() => base.RefreshMenuItems();
	}
}
