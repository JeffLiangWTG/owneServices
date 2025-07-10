using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.Customs.IN.Manifest.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using GlbStaffWrapper = Enterprise.Customs.IN.Business.GlbStaffWrapper;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(MenuBuilder))]
sealed class MenuBuilderTest : TestCaseWithFactory
{
	public void TestMenuCaption()
	{
		using var form = new ZForm(CGMHeader);
		AssertEquals("IN Manifest", new MenuBuilder(CGMHeader, form).MenuCaption.EnglishText);
	}

	public void TestBuildMenu()
	{
		CGMHeader.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
		Assert(!CGMHeader.AMA_RN_NKCountryInfo.HasMessageErrors());

		using var menu = new AsycudaMenuForTest(CGMHeader);
		using var form = new ZForm(CGMHeader);
		form.Menu.MenuItems.Add(menu);
		form.Show();
		menu.OnPopup(EventArgs.Empty);
		var menuItems = menu.MenuItems.Cast<MenuItem>();
		AssertEquals("Menuitems Count", 1, menuItems.Count());
		var sendOption = menuItems.ElementAt(0);
		CombineAssertions(() =>
		{
			AssertEquals("Menu item Text", "ICEGATE - Send Electronically", sendOption.Text);
			AssertEquals("Menu item visibility", expected: true, sendOption.Visible);
		});
	}

	public void TestPreMessageSendingErrors()
	{
		const string incompleteCertificateError = "DSC Token Profile is not setup for current user.";

		RefDataSetupTestHelper.SetupCertificateTokenData(Factory);
		GlbStaff.CurrentUser.GS_LoginName = "ABC";
		var staffWrapper = GlbStaffWrapper.GetWrapperForCurrentUser();
		var loginPassword = staffWrapper.LoginPassword;
		loginPassword.GP_UserID = "ABC";
		loginPassword.GP_MailBoxID = "abc@wtg.in";
		staffWrapper.Factory.Save();

		using var menu = new AsycudaMenuForTest(CGMHeader);
		using var form = new ZForm(CGMHeader);

		CombineAssertions(() =>
		{
			ClickSendManifest(form, menu, setupCertificate: false);
			var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertNull("Pre Message Sending Validation failed", lastFormShown);
			AssertContains("Pre Sending Validation error", incompleteCertificateError, UnitTestUserNotification.Instance.LastMessage.Text);

			ClickSendManifest(form, menu);
			lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<SendMessageForm>("Pre Message Sending Validation success", lastFormShown);
			lastFormShown.Dispose();
			AssertNotEquals("No Incomplete Certificate error", incompleteCertificateError, UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	public void TestSendManifestMenuItem()
	{
		const string confirmSaveMsg = "You need to save first. Would you like to save now and proceed?";
		const string messageNotSent = "Message has not been sent.";
		const string messageHasBeenSent = "Message sent successfully.";

		RefDataSetupTestHelper.SetupCertificateTokenData(Factory);

		CGMHeader.AMA_TransportMode = ZString.Empty;
		using var menu = new AsycudaMenuForTest(CGMHeader);
		using var form = new ZForm(CGMHeader);

		CombineAssertions(() =>
		{
			ClickSendManifest(form, menu, saveToDb: false);
			AssertEquals("Confirmation message to save first", confirmSaveMsg, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Changes not saved, data not is database", expected: false, CGMHeader.IsInDatabase);

			ClickSendManifest(form, menu);
			AssertEquals("Changes saved to database", expected: true, CGMHeader.IsInDatabase);
			var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<SendMessageForm>("Token pin entered, message sending started", lastFormShown);
			lastFormShown.Dispose();
			AssertContains("Message not sent due to missing manifest details", messageNotSent, UnitTestUserNotification.Instance.LastMessage.Text);

			CGMHeader.AMA_TransportMode = TransportTypeList.Codes.Air;
			ClickSendManifest(form, menu);
			AssertContains("Message sent successfully", messageHasBeenSent, UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	public void TestCustomsOfficeRequiredWhenSendingMessage()
	{
		const string customsOfficeRequiredMessage = "Please enter a Customs Office to proceed with CGM message sending.";
		using var menu = new AsycudaMenuForTest(CGMHeader);
		using var form = new ZForm(CGMHeader);

		CombineAssertions(() =>
		{
			ClickSendManifest(form, menu, actionBeforeSend: () =>
			{
				CGMHeader.AMA_TransportMode = TransportTypeList.Codes.Air;
				CGMHeader.AMA_CustomsOffice = ZString.Empty;
			});
			var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertNull("Empty Customs office", lastFormShown);
			AssertEquals("Error while sending", customsOfficeRequiredMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("No message for Manifest", 0, CGMHeader.Messages.Count);

			ClickSendManifest(form, menu);
			lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<SendMessageForm>("After entering Customs office", lastFormShown);
			lastFormShown.Dispose();
			AssertNotEquals("No Error while sending", customsOfficeRequiredMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	void ClickSendManifest(ZForm manifestForm, AsycudaMenuForTest menu,
		bool saveToDb = true, bool setupCertificate = true, Action actionBeforeSend = null)
	{
		CGMHeader.AMA_CustomsOffice = "TestOffice";
		CGMHeader.AMA_RL_NKPortOfLoading = "INBLR";
		CGMHeader.AMA_RL_NKPortOfDischarge = "INDEL";
		CGMHeader.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
		var bill = CGMHeader.Bills.AddNew();
		bill.ABL_BillNumber = "123";

		if (setupCertificate)
		{
			var certificatePassword = GlbStaffWrapper.GetWrapperForCurrentUser().CertificatePassword;
			certificatePassword.GP_Name = "WatchData";
			certificatePassword.GP_CertificateSerialNumber = "XYZ-123";
		}

		var tokenPinstore = CertificateTokenPinStore.Instance as ITokenPinStore;
		tokenPinstore.ResetPin();
		tokenPinstore.SetPin("1234");

		manifestForm.Menu.MenuItems.Add(menu);
		manifestForm.Show();
		menu.OnPopup(EventArgs.Empty);
		var menuItem = menu.MenuItems.FindByText("ICEGATE - Send Electronically", true);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		var dialogResult = saveToDb ? DialogResult.Yes : DialogResult.No;
		UnitTestUserNotification.Instance.AddAnswer(dialogResult);

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		actionBeforeSend?.Invoke();
		menuItem.PerformClick();
	}

	CGMAsycudaManifestHeader CGMHeader => cgmHeader ??= Factory.NewWithValidTestData<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader cgmHeader;
}
