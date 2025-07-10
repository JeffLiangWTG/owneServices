using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.CH.Business.CusEntryHeader;
using SupportingDocSendingObject = Enterprise.Customs.CH.Business.SupportingDocSendingObject;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(EDIMenu))]
sealed class EDIMenuTest : TestCaseForAttachGUI
{
	public void TestSendCustomsMessagesMenuItemVisible() => CombineAssertions(() =>
	{
		CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface() { RecipientID = "RecipientID", SubmissionType = "BTH", });
		using (var formForTest = new JobDeclarationFormForTesting(Declaration))
		{
			formForTest.Show();
			var menu = formForTest.EDIMenu;

			var mergeBaseMenuItem = menu.MenuItems.FindByText("Generate Entries (Merge)");

			AssertNotNull("Merge base menu item", mergeBaseMenuItem);

			Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			menu.RefreshMenu();

			menu.OnPopup(EventArgs.Empty);
			AssertEquals($"({Declaration.JE_ApplicationCode}) - Merge base menu item visible state", true, mergeBaseMenuItem.Visible);

			Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			menu.RefreshMenu();
			menu.OnPopup(EventArgs.Empty);
			AssertEquals($"({Declaration.JE_ApplicationCode}) - Merge base menu item visible state", false, mergeBaseMenuItem.Visible);
		}
	});

	public void TestDisplayGenerateEntriesMenuOption() => CombineAssertions(() =>
	{
		using (var formForTest = new JobDeclarationFormForTesting(Declaration))
		{
			formForTest.Show();
			var menu = formForTest.EDIMenu;

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
			AssertEquals("MessageType=IMP", true, menu.DisplayGenerateEntriesMenuOption);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
			AssertEquals("MessageType=EXP", true, menu.DisplayGenerateEntriesMenuOption);

			Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			AssertEquals("MessageType=EDA", false, menu.DisplayGenerateEntriesMenuOption);
		}
	});

	public void TestImportSendToCustomsMenuItemVisible() => AssertSendToCustomsMenuItemVisible(Common.Shared.SharedJobMessageTypeList.Codes.Import);

	public void TestExportSendToCustomsMenuItemVisible() => AssertSendToCustomsMenuItemVisible(Common.Shared.SharedJobMessageTypeList.Codes.Export);

	void AssertSendToCustomsMenuItemVisible(string messageType)
	{
		CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface() { RecipientID = "RecipientID", SubmissionType = "BTH", });
		Declaration.JE_MessageType = messageType;

		using (var formForTest = new JobDeclarationFormForTesting(Declaration))
		{
			formForTest.Show();
			var menu = formForTest.EDIMenu;
			var sendCustomsMenuItem = menu.sendToCustomsMenuItem;

			CombineAssertions(() =>
			{
				AssertNotNull("Send to Customs menu item", sendCustomsMenuItem);

				Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				menu.RefreshMenu();

				menu.OnPopup(EventArgs.Empty);
				AssertEquals($"({Declaration.JE_ApplicationCode}) - Send To Customs menu item visible state", true, sendCustomsMenuItem.Visible);

				Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				menu.RefreshMenu();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals($"({Declaration.JE_ApplicationCode}) - Send To Customs menu item visible state", false, sendCustomsMenuItem.Visible);
			});
		}
	}

	public void TestImportSendToCustomsMenuItem() => AssertSendToCustomsMenuItem(Common.Shared.SharedJobMessageTypeList.Codes.Import, () => CredentialsTestHelper.CreateCurrentCompanyCertificateCredential(), typeof(MessageSendingForm), MessageSubTypeCodeList.Codes.ImportDeclaration);

	public void TestExportSendToCustomsMenuItem() => AssertSendToCustomsMenuItem(Common.Shared.SharedJobMessageTypeList.Codes.Export, () => CredentialsTestHelper.CreateCurrentCompanyTokenCredential(OrgCusCode.SwissCodeTypes.BID, "123"), typeof(MessageSendingForm), "015");

	void AssertSendToCustomsMenuItem(string messageType, Action createCurrentCompanyCredential, Type expectedMessageSendingForm, string expectedMessageSubType)
	{
		var declarationReference = "B00001000";
		Declaration.JE_MessageType = messageType;
		Declaration.JE_DeclarationReference = declarationReference;

		createCurrentCompanyCredential();

		using (var formForTest = new JobDeclarationFormForTesting(Declaration))
		{
			formForTest.Show();
			var testMenu = formForTest.EDIMenu;

			CombineAssertions(() =>
			{
				Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				testMenu.RefreshMenu();
				AssertEquals(false, testMenu.sendToCustomsMenuItem.Visible);

				Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				testMenu.RefreshMenu();
				AssertEquals(true, testMenu.sendToCustomsMenuItem.Visible);

				Assert("Pre-condition", Declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertEquals(PreSaveMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				Declaration.Factory.Save();
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertEquals("You can't merge this entry because there are no invoice headers.", UnitTestUserNotification.Instance.LastMessage.Text);

				var testInst = Declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "11";
				var testInvHeader = Declaration.Invoices.AddNew();
				var testInvLine = testInvHeader.InvoiceLines.AddNew();
				testInvLine.JI_CEI = testInst.PK;
				testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";
				Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				Factory.Save();

				AssertEquals("Merged Entries Count before sending", 0, Declaration.CustomsEntryHeaders.Count);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testMenu.sendToCustomsMenuItem.PerformClick();

				AssertType("MessageSendingForm type", expectedMessageSendingForm, ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("1 message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("1 message has been sent.", 1, Declaration.CustomsEntryHeaders[0].Messages.Count);
				AssertEquals("Merged Entries Count after sending", 1, Declaration.CustomsEntryHeaders.Count);
				AssertNotNullOrEmpty("EntryHeader.CH_BGMReference after merge", Declaration.CustomsEntryHeaders[0].CH_BGMReference);

				var declarationInAnotherFactory = new BusinessObjectFactory().Load<JobDeclaration>(Declaration.PK);
				AssertEquals($"1 {expectedMessageSubType} message has been sent and saved.", expectedMessageSubType, declarationInAnotherFactory?.CustomsEntryHeaders[0]?.Messages[0]?.EM_MessageSubType);
			});
		}
	}

	public void TestSendToCustoms_CreditLimitCheck() => CombineAssertions(() =>
	{
		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();
		CredentialsTestHelper.CreateCurrentCompanyTokenCredential(OrgCusCode.SwissCodeTypes.BID, "123");
		using var temporaryCreditControllerOverride = CreditCheckTestHelper.DisposableCreditControllerOverride();

		Declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
		Declaration.Invoices.AddNew().InvoiceLines.AddNew();
		Declaration.Importer.MiscServ.OM_ARCreditLimit = -1;

		using var form = new JobDeclarationFormForTesting(Declaration);
		form.Show();

		AssertCreditLimitCheck(true, CHJobMessageTypeList.Codes.Import, creditLimitCheckFails: true);
		AssertCreditLimitCheck(false, CHJobMessageTypeList.Codes.Import, creditLimitCheckFails: false);
		AssertCreditLimitCheck(true, CHJobMessageTypeList.Codes.Export, creditLimitCheckFails: true);
		AssertCreditLimitCheck(false, CHJobMessageTypeList.Codes.Export, creditLimitCheckFails: false);
		AssertCreditLimitCheck(false, CHJobMessageTypeList.Codes.ExportDeclarationActivation, creditLimitCheckFails: true);
		AssertCreditLimitCheck(false, CHJobMessageTypeList.Codes.ExportDeclarationActivation, creditLimitCheckFails: false);

		void AssertCreditLimitCheck(bool creditCheckMessageBoxExpected, string jobMessageType, bool creditLimitCheckFails, [CallerLineNumber] int line = 0)
		{
			var assertionMessage = $"[{line}] {jobMessageType}: LimitExceeded={creditLimitCheckFails}";

			using var temporaryCreditCheckOnMessageSendOverride = CreditCheckTestHelper.DisposableCreditCheckOnSendOverride(creditLimitCheckFails);

			Declaration.JE_MessageType = jobMessageType;
			Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();
			form.EDIMenu.sendToCustomsMenuItem.PerformClick();

			if (creditCheckMessageBoxExpected)
			{
				AssertEquals($"{assertionMessage} - Credit limit check message box expected", CreditCheckMessageText, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				AssertNull($"{assertionMessage} - No credit limit check message box expected", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertType($"{assertionMessage} - MessageSendingForm expected", typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest);
			}
		}
	});

	public void TestECMMessage_SendMessages()
	{
		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.ECM;

		var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
		entryHeader.EntryNumber = "NO1";
		entryHeader.CH_BGMReference = "BGM1";
		entryHeader.MovementReferenceNumberSetter("MRN1");
		CHGlbStaffWrapper.Get(GlbStaff.CurrentUser).CHDPassword.GP_UserID = "CHUSER";

		Factory.Save();

		using (var formForTest = new JobDeclarationFormForTesting(Declaration))
		{
			formForTest.Show();

			CombineAssertions(() =>
			{
				var testMenu = formForTest.EDIMenu;
				var eComplaintsMenuItem = formForTest.EDIMenu.EComplaintMenuItem;
				var entryMenuItem = eComplaintsMenuItem.MenuItems[0];
				var sendMenuItem = entryMenuItem.MenuItems[0];
				ZFormModaliser.ShowDialogsInTest = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendMenuItem.PerformClick();
				AssertType<EComplaintMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("1 message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("1 message has been sent.", 1, Declaration.CustomsEntryHeaders[0].Messages.Count);
			});
		}
	}

	public void TestImportSendToCustomsMenuItemZSaveException() => AssertSendToCustomsMenuItemZSaveException(CHJobMessageTypeList.Codes.Import, () => CredentialsTestHelper.CreateCurrentCompanyCertificateCredential());

	public void TestExportSendToCustomsMenuItemZSaveException() => AssertSendToCustomsMenuItemZSaveException(CHJobMessageTypeList.Codes.Export, () => CredentialsTestHelper.CreateCurrentCompanyTokenCredential(OrgCusCode.SwissCodeTypes.BID, "123"));

	void AssertSendToCustomsMenuItemZSaveException(string messageType, Action createCurrentCompanyCredential)
	{
		Declaration.JE_MessageType = messageType;

		createCurrentCompanyCredential();

		using (var formForTest = new JobDeclarationFormForTesting(Declaration))
		{
			formForTest.Show();
			var testMenu = formForTest.EDIMenu;

			CombineAssertions(() =>
			{
				Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

				var testInst = Declaration.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "11";
				var testInvHeader = Declaration.Invoices.AddNew();
				var testInvLine = testInvHeader.InvoiceLines.AddNew();
				testInvLine.JI_CEI = testInst.PK;
				testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";
				Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				Declaration.DoMerge();

				Factory.Save();
				AssertEquals("Merged Entries Count", 1, Declaration.CustomsEntryHeaders.Count);

				ThrowZSaveException(ZSaveExceptionMessage);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testMenu.sendToCustomsMenuItem.PerformClick();
				AssertContains(ZSaveExceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("0 message has been sent.", 0, Declaration.CustomsEntryHeaders[0].Messages.Count);
				AssertEquals("0 logs has been added on Entries.", 0, Declaration.CustomsEntryHeaders[0].Logs.LogsNotInDB.Length);
				AssertEquals("0 logs has been added on Declaration.", 0, Declaration.Logs.LogsNotInDB.Length);
			});
		}
	}

	public void TestCreateNewMessageSendingObjectParentNull()
	{
		Declaration.JE_MessageType = string.Empty;
		var ediMenu = new EDIMenuForTesting();
		AssertNull(ediMenu.CreateNewMessageSendingObjectParent(Declaration));
	}

	public void TestCreateNewMessageSendingObjectParentImport() => AssertCreateNewMessageSendingObjectParent(CHJobMessageTypeList.Codes.Import, typeof(ImportDeclarationMessageSendingObjectParent));

	public void TestCreateNewMessageSendingObjectParentExport() => AssertCreateNewMessageSendingObjectParent(CHJobMessageTypeList.Codes.Export, typeof(ExportDeclarationMessageSendingObjectParent));

	void AssertCreateNewMessageSendingObjectParent(string messageType, Type expectedType)
	{
		Declaration.JE_MessageType = messageType;
		var ediMenu = new EDIMenuForTesting();
		AssertType(expectedType, ediMenu.CreateNewMessageSendingObjectParent(Declaration));
	}

	public void TestMessageBoxIfCanSendMessageFalse()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var invoice = Declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var instruction = Declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		var entryLine = entryHeader.MergedLines.AddNew();

		invoiceLine.JI_CEI = instruction.PK;
		invoiceLine.JI_CL = entryLine.PK;

		Factory.Save();

		using (var form = new JobDeclarationForm(Declaration))
		{
			form.Show();

			var menu = (EDIMenu)form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "&Brokerage");
			var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");
			ZFormModaliser.ShowDialogsInTest = true;

			CombineAssertions(() =>
			{
				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				sendToCustomsMenu.PerformClick();
				UserNotificationTestHelper.AssertLastMessage("No customs regno", UnableToSendMessageCaption, NoCustomsRegNoMessageText, expectedWasError: true);

				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123456";
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendToCustomsMenu.PerformClick();
				AssertType<MessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Can send message", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			});
		}
	}

	public void TestEComplaintMenuItem()
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryHeader1 = Declaration.ActiveEntryHeaders.AddNew();
		var entryHeader2 = Declaration.ActiveEntryHeaders.AddNew();
		var entryHeader3WithoutMRN = Declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.MovementReferenceNumberSetter("MRN1");
		entryHeader2.MovementReferenceNumberSetter("MRN2");
		Factory.Save();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();
			var ediMenu = form.EDIMenu;

			CombineAssertions(() =>
			{
				var eComplaintsMenuItem = ediMenu.EComplaintMenuItem;
				AssertNotNull("ECom menu should exist", eComplaintsMenuItem);
				AssertEquals("Visible when BLT", true, eComplaintsMenuItem.Visible);

				var sendToCustomsMenu = ediMenu.SendToCustomsMenuItem;
				Assert("EComp menu below SendToCustoms", sendToCustomsMenu.Index < eComplaintsMenuItem.Index);

				var mrnMenuItems = eComplaintsMenuItem.MenuItems;
				AssertContainsExactElementsInAnyOrder("MRN menu items", new[] { "MRN1", "MRN2" }, mrnMenuItems.Cast<MenuItem>().Select(m => m.Text));

				foreach (var bgmMenuItem in mrnMenuItems.Cast<MenuItem>())
				{
					AssertEquals($"{bgmMenuItem.Text} sub items count", 2, bgmMenuItem.MenuItems.Count);
					AssertEquals($"{bgmMenuItem.Text} sub item text", "Send", bgmMenuItem.MenuItems[0].Text);
					AssertEquals($"{bgmMenuItem.Text} sub item text", "Close", bgmMenuItem.MenuItems[1].Text);
				}

				foreach (var messageType in new CHJobMessageTypeList().GetAllCodes().Except(CHJobMessageTypeList.Codes.Import))
				{
					Declaration.JE_MessageType = messageType;
					eComplaintsMenuItem = ediMenu.EComplaintMenuItem;
					ediMenu.RefreshMenu();
					AssertEquals($"menuItem should not be Visible with the message {messageType}", false, eComplaintsMenuItem.Visible);
				}

				Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				ediMenu.RefreshMenu();
				ediMenu.OnPopup(EventArgs.Empty);
				AssertEquals("Visible when not BLT", false, eComplaintsMenuItem.Visible);
			});
		}
	}

	public void TestSendEComplaintMenuItem()
	{
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "BGM1";
		entryHeader.MovementReferenceNumberSetter("MRN1");
		CHGlbStaffWrapper.Get(GlbStaff.CurrentUser).CHDPassword.GP_UserID = "123456";
		Factory.Save();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();

			CombineAssertions(() =>
			{
				var eComplaintsMenuItem = form.EDIMenu.EComplaintMenuItem;
				var entryMenuItem = eComplaintsMenuItem.MenuItems[0];
				var sendMenuItem = entryMenuItem.MenuItems[0];
				ZFormModaliser.ShowDialogsInTest = true;

				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123456";
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				sendMenuItem.PerformClick();
				AssertType<EComplaintMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
			});
		}
	}

	public void TestSendEComplaintMenuItemCantSendWithoutCompanyRegNo()
	{
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "BGM1";
		entryHeader.MovementReferenceNumberSetter("MRN1");
		CHGlbStaffWrapper.Get(GlbStaff.CurrentUser).CHDPassword.GP_UserID = "123456";
		Factory.Save();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();

			CombineAssertions(() =>
			{
				var eComplaintsMenuItem = form.EDIMenu.EComplaintMenuItem;
				var entryMenuItem = eComplaintsMenuItem.MenuItems[0];
				var sendMenuItem = entryMenuItem.MenuItems[0];
				ZFormModaliser.ShowDialogsInTest = true;

				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				sendMenuItem.PerformClick();
				UserNotificationTestHelper.AssertLastMessage("No customs regno", UnableToSendMessageCaption, NoCustomsRegNoMessageText, expectedWasError: true);

				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123456";
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertType<EComplaintMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Can send message", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			});
		}
	}

	public void TestSendEComplaintMenuItemCantSendWithoutDeclarantNo()
	{
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "BGM1";
		entryHeader.MovementReferenceNumberSetter("MRN1");
		Factory.Save();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();

			CombineAssertions(() =>
			{
				var eComplaintsMenuItem = form.EDIMenu.EComplaintMenuItem;
				var entryMenuItem = eComplaintsMenuItem.MenuItems[0];
				var sendMenuItem = entryMenuItem.MenuItems[0];
				ZFormModaliser.ShowDialogsInTest = true;

				var staffWrapper = CHGlbStaffWrapper.Get(GlbStaff.CurrentUser);

				staffWrapper.CHDPassword.GP_UserID = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				sendMenuItem.PerformClick();
				UserNotificationTestHelper.AssertLastMessage("No declarant no", UnableToSendMessageCaption, NoDeclarantNumberMessageText, expectedWasError: true);

				staffWrapper.CHDPassword.GP_UserID = "123456";
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertType<EComplaintMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Can send message", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			});
		}
	}

	public void TestEvvMenuItem()
	{
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryHeader1 = Declaration.ActiveEntryHeaders.AddNew();
		var entryHeader2 = Declaration.ActiveEntryHeaders.AddNew();
		var entryHeader3WithoutMRN = Declaration.ActiveEntryHeaders.AddNew();
		entryHeader1.MovementReferenceNumberSetter("MRN1");
		entryHeader2.MovementReferenceNumberSetter("MRN2");
		Factory.Save();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();

			CombineAssertions(() =>
			{
				var evvMenuItem = form.EDIMenu.EvvMenuItem;
				AssertNotNull("eVV menu should exist", evvMenuItem);

				Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
				form.EDIMenu.RefreshMenu();
				AssertEquals("Not Visible for EXP", false, evvMenuItem.Visible);

				Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
				form.EDIMenu.RefreshMenu();
				AssertEquals("Visible when BLT and IMP", true, evvMenuItem.Visible);

				var sendToCustomsMenu = form.EDIMenu.SendToCustomsMenuItem;
				Assert("eVV menu below SendToCustoms", sendToCustomsMenu.Index < evvMenuItem.Index);

				var mrnMenuItems = evvMenuItem.MenuItems;
				AssertContainsExactElementsInAnyOrder("MRN menu items", new[] { "MRN1", "MRN2" }, mrnMenuItems.Cast<MenuItem>().Select(m => m.Text));

				Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				form.EDIMenu.RefreshMenu();
				form.EDIMenu.OnPopup(EventArgs.Empty);
				AssertEquals("Visible when not BLT", false, evvMenuItem.Visible);
			});
		}
	}

	public void TestSendEvvRequestMenuItem()
	{
		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "BGM1";
		entryHeader.MovementReferenceNumberSetter("MRN1");
		CHGlbStaffWrapper.Get(GlbStaff.CurrentUser).CHDPassword.GP_UserID = "123456";
		Factory.Save();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();

			CombineAssertions(() =>
			{
				var evvMenuItem = form.EDIMenu.EvvMenuItem;
				var entryMenuItem = evvMenuItem.MenuItems[0];
				ZFormModaliser.ShowDialogsInTest = true;

				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				entryMenuItem.PerformClick();
				UserNotificationTestHelper.AssertLastMessage("No customs regno", UnableToSendMessageCaption, NoCustomsRegNoMessageText, expectedWasError: true);

				GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123456";
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull("Pre-condition: No request yet sent", entryHeader.Logs.MostRecentLogByEventTime(Events.ElectronicAssessmentDecisionStatus));
				entryMenuItem.PerformClick();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					if (dialog is EvvManualRequestForm sendingForm)
					{
						var sendingObjectParent = (EvvRequestSendingObjectParent)sendingForm.DataSource;
						sendingObjectParent.MrnVersion = 2;
						sendingObjectParent.SendCustomsDuties = true;
						sendingObjectParent.SendVat = true;
						sendingObjectParent.SendReimbursementCustomsDuties = true;
						sendingObjectParent.SendReimbursementVat = true;
					}
				});
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				entryMenuItem.PerformClick();

				UserNotificationTestHelper.AssertLastMessage("", null, "4 message(s) have been sent.");

				AssertNotNull("Request has been sent", entryHeader.Logs.MostRecentLogByEventTime(Events.ElectronicAssessmentDecisionStatus));
			});
		}
	}

	public void TestAccompanyingDocumentsMenuItem_Interfaced()
	{
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		RefCusCodeTestHelper.CreateDocumentTypeList(Factory);
		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();
			var ediMenu = form.EDIMenu;

			AssertEquals("Send Accompanying Documents should be hidden", false, ediMenu.SendDocumentsMenuItem.Visible);
		}
	}

	public void TestAccompanyingDocumentsMenuItem_Builtin()
	{
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		RefCusCodeTestHelper.CreateDocumentTypeList(Factory);
		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();
			var ediMenu = form.EDIMenu;
			var menuItem = ediMenu.SendDocumentsMenuItem;

			CombineAssertions(() =>
			{
				AssertNotNull("menuItem", menuItem);
				AssertEquals("Caption", "Send Accompanying Documents (eBD)", menuItem.Text);
				AssertEquals("Visible", true, menuItem.Visible);
				AssertEquals("Index", ediMenu.SendToCustomsMenuItem.Index + 1, menuItem.Index);

				foreach (var messageType in new CHJobMessageTypeList().GetAllCodes().Except(CHJobMessageTypeList.Codes.Import))
				{
					Declaration.JE_MessageType = messageType;
					ediMenu.RefreshMenu();
					menuItem = ediMenu.SendDocumentsMenuItem;
					AssertEquals($"menuItem should not be Visible with the message {messageType}", false, menuItem.Visible);
				}
			});
		}
	}

	public void TestAccompanyingDocumentsMenuItem_DeclarationShouldBeSaved()
	{
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		RefCusCodeTestHelper.CreateDocumentTypeList(Factory);
		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();
			var ediMenu = form.EDIMenu;
			var menuItem = ediMenu.SendDocumentsMenuItem;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			Factory.Save();
			menuItem.PerformClick();

			CombineAssertions(() =>
			{
				Assert("Declaration should be saved", !Declaration.HasChanges);
				AssertEquals($"Declaration '{Declaration.JE_DeclarationReference}' has no entry. Please generate entries before attempting to send a message.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestAccompanyingDocumentsMenuItem()
	{
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		RefCusCodeTestHelper.CreateDocumentTypeList(Factory);
		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();
			var ediMenu = form.EDIMenu;
			var menuItem = ediMenu.SendDocumentsMenuItem;

			var eDoc = Declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("MRN100");
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				if (dialog is SupportingDocSendingForm sendingForm)
				{
					var sendingObjectParent = (JobDeclarationSupportingDocSendingObjectParent)sendingForm.DataSource;
					var sendingObject = (SupportingDocSendingObject)sendingObjectParent.SendingObjectsCollection.AddNew();
					sendingObject.EDoc = eDoc.UniqueKey;
					sendingObject.DocumentType = RefCusCodeTestHelper.ValidDocumentTypeCode;
					sendingObject.LocalReferenceNumber = entryHeader.MovementReferenceNumber;
				}
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			menuItem.PerformClick();

			CombineAssertions(() =>
			{
				AssertType<SupportingDocSendingForm>("Should show a SupportingDocSendingForm", ZFormModaliser.LastFormShownDialogForTest);

				AssertEquals("1 message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("1 message has been sent.", 1, Declaration.CustomsEntryHeaders[0].Messages.Count);

				var declarationInAnotherFactory = new BusinessObjectFactory().Load<JobDeclaration>(Declaration.PK);
				AssertEquals($"1 EBD message has been sent and saved.", MessageTypeCodeList.Codes.EBD, declarationInAnotherFactory?.CustomsEntryHeaders[0]?.Messages[0]?.EM_MessageType);
			});
		}
	}

	public void TestAccompanyingDocumentsMenuItemZSaveException()
	{
		RefCusCodeTestHelper.CreateDocumentTypeList(Factory);
		CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();
			var ediMenu = form.EDIMenu;
			var menuItem = ediMenu.SendDocumentsMenuItem;

			Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var eDoc = Declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("MRN100");
			Factory.Save();

			ThrowZSaveException(ZSaveExceptionMessage);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				if (dialog is SupportingDocSendingForm sendingForm)
				{
					var sendingObjectParent = (JobDeclarationSupportingDocSendingObjectParent)sendingForm.DataSource;
					var sendingObject = (SupportingDocSendingObject)sendingObjectParent.SendingObjectsCollection.AddNew();
					sendingObject.EDoc = eDoc.UniqueKey;
					sendingObject.DocumentType = RefCusCodeTestHelper.ValidDocumentTypeCode;
					sendingObject.LocalReferenceNumber = entryHeader.MovementReferenceNumber;
				}
			});
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			menuItem.PerformClick();

			CombineAssertions(() =>
			{
				AssertContains(ZSaveExceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("0 message has been sent.", 0, Declaration.CustomsEntryHeaders[0].Messages.Count);
				AssertEquals("0 logs has been added on Entries.", 0, Declaration.CustomsEntryHeaders[0].Logs.LogsNotInDB.Length);
				AssertEquals("0 logs has been added on Declaration.", 0, Declaration.Logs.LogsNotInDB.Length);
			});
		}
	}

	public void TestCloseEComplaintMenuItem()
	{
		var testHelper = new SendingObjectsTestHelper();

		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "BGM1";
		entryHeader.MovementReferenceNumberSetter("MRN1");
		entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Received;
		Factory.Save();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();
			var eComplaintsMenuItem = form.EDIMenu.EComplaintMenuItem;
			var entryMenuItem = eComplaintsMenuItem.MenuItems[0];
			var closeMenuItem = entryMenuItem.MenuItems[1];

			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				closeMenuItem.PerformClick();
				Assert("No message expected", UnitTestUserNotification.Instance.LastMessage.WasNone);
				entryHeader = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
				AssertEquals("New LastEComplaintStatus", EComplaintStatusList.Codes.Closed, entryHeader.CH_LastEComplaintStatus);
				testHelper.AssertEvent(entryHeader.Logs.MostRecentLogByEventTime(Events.EComStatusChange), entryHeader, $"|NEW=CLS|OLD={EComplaintStatusList.Codes.Received}");
			});
		}
	}

	public void TestCloseEComplaintMenuItemPreSave()
	{
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "BGM1";
		entryHeader.MovementReferenceNumberSetter("MRN1");
		entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Received;

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();
			var eComplaintsMenuItem = form.EDIMenu.EComplaintMenuItem;
			var entryMenuItem = eComplaintsMenuItem.MenuItems[0];
			var closeMenuItem = entryMenuItem.MenuItems[1];

			CombineAssertions(() =>
			{
				Assert("Pre-condition", Declaration.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				closeMenuItem.PerformClick();
				AssertEquals(PreSaveMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No saved (HasChanges)", true, Declaration.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				closeMenuItem.PerformClick();
				AssertEquals("Saved (HasChanges)", false, Declaration.HasChanges);

				entryHeader = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
				AssertEquals("LastEComplaintStatus updated", EComplaintStatusList.Codes.Closed, entryHeader.CH_LastEComplaintStatus);
			});
		}
	}

	public void TestCloseEComplaintMenuItemInvalidStatus()
	{
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "BGM1";
		entryHeader.MovementReferenceNumberSetter("MRN1");

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();
			var eComplaintsMenuItem = form.EDIMenu.EComplaintMenuItem;
			var entryMenuItem = eComplaintsMenuItem.MenuItems[0];
			var closeMenuItem = entryMenuItem.MenuItems[1];

			CombineAssertions(() =>
			{
				var eventCount = entryHeader.Logs.DatabaseCount;

				entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.NotSent;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				closeMenuItem.PerformClick();
				UserNotificationTestHelper.AssertLastMessage("When NotSent", "Close ECom", "There is no ECom open.", expectedWasError: true);

				entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Sent;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				closeMenuItem.PerformClick();
				UserNotificationTestHelper.AssertLastMessage("When Sent", "Close ECom", "Cannot close, a response is pending.", expectedWasError: true);

				entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Closed;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				closeMenuItem.PerformClick();
				UserNotificationTestHelper.AssertLastMessage("When Closed", "Close ECom", "The ECom has already been closed.", expectedWasError: true);

				AssertEquals("No event should have been added.", 0, entryHeader.Logs.LogsNotInDB.Length);
				entryHeader = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
				AssertEquals("No event should have been written.", eventCount, entryHeader.Logs.DatabaseCount);
			});
		}
	}

	public void TestCloseEComplaintMenuItemZSaveException()
	{
		Declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "BGM1";
		entryHeader.MovementReferenceNumberSetter("MRN1");
		entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Received;
		Factory.Save();

		using (var form = new JobDeclarationFormForTesting(Declaration))
		{
			form.Show();
			var eComplaintsMenuItem = form.EDIMenu.EComplaintMenuItem;
			var entryMenuItem = eComplaintsMenuItem.MenuItems[0];
			var closeMenuItem = entryMenuItem.MenuItems[1];

			CombineAssertions(() =>
			{
				var otherFactory = new BusinessObjectFactory();
				otherFactory.RefreshEnabled = false;
				otherFactory.Load<CusEntryHeader>(entryHeader.PK).CH_LastEComplaintStatus = "XXX";
				otherFactory.Save();

				var eventCount = entryHeader.Logs.DatabaseCount;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				closeMenuItem.PerformClick();
				AssertContains(SaveErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				entryHeader = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
				AssertEquals("LastEComplaintStatus unchanged", "XXX", entryHeader.CH_LastEComplaintStatus);
				AssertEquals("No event should have been written.", eventCount, entryHeader.Logs.DatabaseCount);
			});
		}
	}

	public void TestSendToCustoms_EnabledExport() => AssertSendToCustoms_EnabledWithMessageType(Common.Shared.SharedJobMessageTypeList.Codes.Export);

	public void TestSendToCustoms_EnabledImport() => AssertSendToCustoms_EnabledWithMessageType(Common.Shared.SharedJobMessageTypeList.Codes.Import);

	public void TestSendToCustoms_EnabledExportDeclarationActivaiton() => AssertSendToCustoms_EnabledWithMessageType(CHJobMessageTypeList.Codes.ExportDeclarationActivation);

	void AssertSendToCustoms_EnabledWithMessageType(string messageType) => CombineAssertions(() =>
	{
		CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface() { RecipientID = "RecipientID", SubmissionType = "BTH", });
		Declaration.JE_MessageType = messageType;

		Declaration.ActiveEntryHeaders.RemoveAll();
		var entryHeader1 = Declaration.ActiveEntryHeaders.AddNew();
		var entryHeader2 = Declaration.ActiveEntryHeaders.AddNew();

		using var formForTest = new JobDeclarationFormForTesting(Declaration);
		formForTest.Show();
		var menu = formForTest.EDIMenu;
		var sendCustomsMenuItem = menu.sendToCustomsMenuItem;

		AssertEnabled(true, string.Empty, string.Empty, false);
		AssertEnabled(true, string.Empty, CHLogicalStatusList.Codes.Sent, false);
		AssertEnabled(false, CHLogicalStatusList.Codes.Acknowledged, CHLogicalStatusList.Codes.Sent, false);

		AssertEnabled(true, string.Empty, string.Empty, true);
		AssertEnabled(true, string.Empty, CHLogicalStatusList.Codes.Sent, true);
		AssertEnabled(true, CHLogicalStatusList.Codes.Acknowledged, CHLogicalStatusList.Codes.Sent, true);

		void AssertEnabled(bool expectedEnabled, string message1Status, string message2Status, bool isResendAllowed)
		{
			Env.Security.CHCustomsDeclarationAllowResendToCustoms.IsAllowed = isResendAllowed;
			entryHeader1.CH_Status = message1Status;
			entryHeader2.CH_Status = message2Status;
			menu.RefreshMenu();
			AssertEquals($"message1Status={message1Status} message2Status={message2Status} IsAllowed={isResendAllowed}", expectedEnabled, sendCustomsMenuItem?.Enabled);
		}
	});

	public void TestMenuWithNoDeclaration()
	{
		var shipment = Factory.New<ForwardingShipment>();
		ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

		using (var shipmentForm = new Freight.Forwarding.GUI.ShipmentForm(shipment))
		{
			var ediMenu = new EDIMenu();
			shipmentForm.Menu.MenuItems.Add(ediMenu);
			shipmentForm.Show();
			ediMenu.RefreshMenu();

			AssertEquals("sendToCustomsMenuItem.Enabled", false, ediMenu.sendToCustomsMenuItem.Enabled);
		}
	}

	void ThrowZSaveException(string exceptionMessage) => Factory.Saving += (f) => throw new ZSaveException(new ZDataExceptionForTesting(exceptionMessage), f);

	JobDeclaration Declaration => declaration ?? (declaration = Factory.NewWithValidTestData<JobDeclaration>());
	JobDeclaration declaration;

	const string PreSaveMessage = "The Job has not yet been saved. Do you want to save and proceed?";
	const string SaveErrorMessage = "While you have been working with this form, another user has made changes.";
	const string ZSaveExceptionMessage = "*** TEST ERROR ***";
	const string UnableToSendMessageCaption = "Unable to send to customs";
	const string NoCustomsRegNoMessageText = "Customs Registration Number is not configured for the current company. Please contact your system administrator.";
	const string NoDeclarantNumberMessageText = "Declarant Number is not configured for the current user. Please contact your system administrator.";
	const string CreditCheckMessageText = "Submit message with credit restriction canceled.";
}
