using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(EDIMenu))]
sealed class EDIMenuTest : TestCaseWithFactory
{
	public void TestMenuItem_GenerateEntries()
	{
		using var form = GetForm();
		var menu = form.EDIMenu;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		menu.Declaration = declaration;
		var generateEntriesMenuItem = menu.MenuItems.FindByText("Generate Entries");
		Assert("Menu Item to generate entries is visible", generateEntriesMenuItem.Visible);
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		menu.RefreshMenu();
		generateEntriesMenuItem = menu.MenuItems.FindByText("Generate Entries");
		Assert("Menu Item to generate entries is visible", generateEntriesMenuItem.Visible);
	}

	public void TestMenuItems()
	{
		using var menu = new EDIMenu();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		declaration.JE_MessageType = "";
		menu.Declaration = declaration;

		var sendMessageMenuItem = menu.MenuItems.FindByText(SendMessageText);
		CombineAssertions(() =>
		{
			AssertEquals("Menu Item to send message is hidden", expected: false, sendMessageMenuItem.Visible);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			menu.RefreshMenu();
			AssertEquals("Menu Item to send message is visible", expected: true, sendMessageMenuItem.Visible);
		});
	}

	public void TestSendMessageMenuItem_SupportUser()
	{
		using var form = GetForm();
		var declaration = form.Declaration;
		declaration.JE_DeclarationReference = "TestSend";
		Factory.Save();

		CombineAssertions(() =>
		{
			ClickSendToCustoms(form, SendMessageText);
			var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertNull("Message Sending Form not displayed", lastFormShown);
			AssertEquals("Declaration TestSend has no entry.", UnitTestUserNotification.Instance.LastMessage.Text);

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			ClickSendToCustoms(form, SendMessageText);
			lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertNull("Message Sending Form not displayed", lastFormShown);
			AssertEquals("Please select Customs House to send the message electronically to ICEGate.", UnitTestUserNotification.Instance.LastMessage.Text);

			declaration.JE_CustomsOffice = "PQR123";
			ClickSendToCustoms(form, SendMessageText);
			lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<SendMessageForm>("Message Sending Form displayed", lastFormShown);
		});
	}

	public void TestSendMessageMenuItem_MessageSending()
	{
		const string incompleteCertificateError = "DSC Token Profile is not setup for current user.";
		const string sendDeclarationMessageError = "Message has not been sent. This may be caused by a failure or because sending has not been implemented.";
		const string sendDeclarationMessageSuccess = "Message sent successfully.";
		const string declarationHasNoEntry = "Declaration B00001000 has no entry.";
		const string declarationHasNoCustomsHouse = "Please select Customs House to send the message electronically to ICEGate.";

		RefDataSetupTestHelper.SetupCertificateTokenData(Factory);
		GlbStaff.CurrentUser.GS_LoginName = "ABC";
		var staffWrapper = Business.GlbStaffWrapper.GetWrapperForCurrentUser();
		var loginPassword = staffWrapper.LoginPassword;
		loginPassword.GP_UserID = "ABC";
		loginPassword.GP_MailBoxID = "abc@wtg.in";
		staffWrapper.Factory.Save();

		using var form = GetForm();
		var declaration = form.Declaration;
		declaration.JE_DeclarationReference = "B00001000";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Factory.Save();

		CombineAssertions(() =>
		{
			ClickSendToCustoms(form, SendMessageText);
			var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertNull("Pre Message Sending Validation failed", lastFormShown);
			AssertContains("Pre Sending Validation error", declarationHasNoEntry, UnitTestUserNotification.Instance.LastMessage.Text);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			ClickSendToCustoms(form, SendMessageText);
			lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertNull("Pre Message Sending Validation failed", lastFormShown);
			AssertEquals("Pre Sending Validation error", incompleteCertificateError, UnitTestUserNotification.Instance.LastMessage.Text);

			var certificatePassword = staffWrapper.CertificatePassword;
			certificatePassword.GP_Name = "WatchData";
			certificatePassword.GP_CertificateSerialNumber = "XYZ-123";
			ClickSendToCustoms(form, SendMessageText);
			lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertNull("Pre Message Sending Validation failed", lastFormShown);
			AssertContains("Pre Sending Validation error", declarationHasNoCustomsHouse, UnitTestUserNotification.Instance.LastMessage.Text);

			declaration.JE_CustomsOffice = "PQR123";
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ClickSendToCustoms(form, SendMessageText);
			lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<SendMessageForm>("Pre Message Sending Validation success", lastFormShown);
			lastFormShown.Dispose();
			AssertEquals(sendDeclarationMessageError, UnitTestUserNotification.Instance.LastMessage.Text);
			ZFormModaliser.LastFormShownDialogForTest = null;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			ClickSendToCustoms(form, SendMessageText);
			lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<SendMessageForm>("Pre Message Sending Validation success", lastFormShown);
			lastFormShown.Dispose();
			AssertEquals(sendDeclarationMessageSuccess, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Message Sent", EDIMessageStatusList.Codes.Queued, entryHeader.Messages.LastMessage?.EM_Status);
		});
	}

	static void ClickSendToCustoms(JobDeclarationFormTestClass form, string menuItemName)
	{
		var menu = form.EDIMenu;
		var menuItemToClick = menu.MenuItems.FindByText(menuItemName);
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
		ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
		{
			if (obj is SendMessageForm form)
			{
				form.MessageSendingObjectParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().ForEach(x => x.ShouldSend = true);
			}
		});
		menuItemToClick.PerformClick();
	}

	JobDeclarationFormTestClass GetForm(string shipmentType = JobMessageTypeList.Codes.Export)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = shipmentType;

		var form = new JobDeclarationFormTestClass(declaration);
		var menu = form.EDIMenu;
		menu.Declaration = declaration;
		return form;
	}

	const string SendMessageText = "ICEGATE - Send Electronically";

	class JobDeclarationFormTestClass : Customs.GUI.Testing.BaseJobDeclarationFormTestClass
	{
		public JobDeclarationFormTestClass(JobDeclaration dec) : base(dec) { }

		protected override Customs.GUI.IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();
	}
}
