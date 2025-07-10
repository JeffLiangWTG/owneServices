using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction;
using ITGlbCompanyWrapper = Enterprise.Customs.IT.Business.GlbCompanyWrapper;
using ITGlbStaffWrapper = Enterprise.Customs.IT.Business.GlbStaffWrapper;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(MessageSendingFormUcc6))]
sealed class MessageSendingFormUcc6Test : Customs.GUI.Testing.MessageSendingObjectFormTest
{
	protected override Form GetFormToBashCore() => new MessageSendingFormUcc6(DeclarationWrapper);

	public void TestXadesCertificateVerification_WhenLoggedUserDoesNotHaveItConfigured()
	{
		SetUpRegistryAccountAndDeclarant();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var form = new MessageSendingFormUcc6ForTest(DeclarationWrapper))
		{
			form.Show();

			DeclarationWrapper.SendingObjectsCollection[0].ShouldSend = true;
			form.SendSplitButton.Enabled = true;
			form.SendSplitButton.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Last message prompted to the user", "XADES certificate is missing for the current user. Please fill XADES Certificate information in Staff and Resources > Credentials.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Is form visible?", false, form.Visible);
				AssertEquals("Form DialogResult", DialogResult.Cancel, form.DialogResult);
			});
		}
	}

	public void TestXadesCertificateVerification_WhenLoggedUserHasItConfiguredButNotPlugged()
	{
		SetUpRegistryAccountAndDeclarant();

		var staffWrapper = ITGlbStaffWrapper.Get(GlbStaff.CurrentUser);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = "BIT4ID";
		cryptokiCertificate.GP_CertificateSerialNumber = "000000000";

		ShowFormAndAssertIfItChecksForCertificatePresence(AssertFormWhenCertificateNotFound);
	}

	public void TestDoesNotCheckForCertificatePresenceIfPinAlreadyEntered()
	{
		SetUpRegistryAccountAndDeclarant();

		var staffWrapper = ITGlbStaffWrapper.Get(GlbStaff.CurrentUser);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = "BIT4ID";
		cryptokiCertificate.GP_CertificateSerialNumber = "000000000";
		cryptokiCertificate.TokenPinStore.SetPin("123ABC");

		ShowFormAndAssertIfItChecksForCertificatePresence(AssertFormWhenCertificatePresenceNotChecked);
	}

	public void TestDoesNotCheckForCertificatePresenceIfUserHasActiveAutomaticRemoteSignature()
	{
		SetUpRegistryAccountAndDeclarant();
		var staffWrapper = ITGlbStaffWrapper.Get(GlbStaff.CurrentUser);
		staffWrapper.AutomaticSignaturePasswordCollection.AddNew().IsConfigurationActive = true;

		ShowFormAndAssertIfItChecksForCertificatePresence(AssertFormWhenCertificatePresenceNotChecked);
	}

	void ShowFormAndAssertIfItChecksForCertificatePresence(Action<Form> assertExpectedResult)
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var form = new MessageSendingFormUcc6ForTest(DeclarationWrapper))
		{
			form.Show();

			DeclarationWrapper.SendingObjectsCollection[0].ShouldSend = true;
			form.SendSplitButton.Enabled = true;
			form.SendSplitButton.PerformClick();

			CombineAssertions(() => assertExpectedResult(form));
		}
	}

	void AssertFormWhenCertificateNotFound(Form form)
	{
		AssertEquals("Last message prompted to the user", "Cannot locate XADES certificate for the current user. Please insert the certificate in your computer and retry the operation.", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals("Is form visible?", true, form.Visible);
		AssertEquals("Form DialogResult", DialogResult.None, form.DialogResult);
	}

	void AssertFormWhenCertificatePresenceNotChecked(Form form)
	{
		AssertNull("Last message prompted to the user", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals("Is form visible?", false, form.Visible);
		AssertEquals("Form DialogResult", DialogResult.OK, form.DialogResult);
	}

	public void TestUserIsAskedToEnterPinIfNotEnteredYet()
	{
		SetUpRegistryAccountAndDeclarant();

		var staffWrapper = ITGlbStaffWrapper.Get(GlbStaff.CurrentUser);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = "BIT4ID";
		cryptokiCertificate.GP_CertificateSerialNumber = "0123456789";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var form = new MessageSendingFormUcc6ForTest(DeclarationWrapper, mockLocatableSerialNumber: cryptokiCertificate.GP_CertificateSerialNumber))
		{
			form.Show();

			DeclarationWrapper.SendingObjectsCollection[0].ShouldSend = true;
			form.SendSplitButton.Enabled = true;
			form.SendSplitButton.PerformClick();

			AssertType<EnterCryptokiCertificatePinForm>("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
		}
	}

	public void TestUserEnteredPinIsPersistedInMemoryCache()
	{
		SetUpRegistryAccountAndDeclarant();

		var staffWrapper = ITGlbStaffWrapper.Get(GlbStaff.CurrentUser);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = "BIT4ID";
		cryptokiCertificate.GP_CertificateSerialNumber = "0123456789";

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var form = new MessageSendingFormUcc6ForTest(DeclarationWrapper, new UserEnterableTokenPin() { Pin = "123ABC" }, cryptokiCertificate.GP_CertificateSerialNumber))
		{
			form.Show();

			DeclarationWrapper.SendingObjectsCollection[0].ShouldSend = true;
			form.SendSplitButton.Enabled = true;

			AssertEquals("PRE-CONDITION: Pin", "", cryptokiCertificate.TokenPinStore.GetPin());
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			form.SendSplitButton.PerformClick();
			CombineAssertions("POST-CONDITIONS", () =>
			{
				AssertEquals("Pin", "123ABC", cryptokiCertificate.TokenPinStore.GetPin());
				AssertEquals("Is form visible?", false, form.Visible);
				AssertEquals("Form DialogResult", DialogResult.OK, form.DialogResult);
			});
		}
	}

	public void TestUserIsNotAskedToEnterPinIfAlreadyEntered()
	{
		SetUpRegistryAccountAndDeclarant();

		var staffWrapper = ITGlbStaffWrapper.Get(GlbStaff.CurrentUser);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = "BIT4ID";
		cryptokiCertificate.GP_CertificateSerialNumber = "0123456789";
		cryptokiCertificate.TokenPinStore.SetPin("123ABC");

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var form = new MessageSendingFormUcc6ForTest(DeclarationWrapper))
		{
			form.Show();

			DeclarationWrapper.SendingObjectsCollection[0].ShouldSend = true;
			form.SendSplitButton.Enabled = true;
			form.SendSplitButton.PerformClick();

			CombineAssertions("POST-CONDITIONS", () =>
			{
				AssertNull("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Is form visible?", false, form.Visible);
				AssertEquals("Form DialogResult", DialogResult.OK, form.DialogResult);
			});
		}
	}

	public void TestSendSplitButtonStyle()
	{
		using (var form = new MessageSendingFormUcc6ForTest(DeclarationWrapper))
		{
			form.Show();

			AssertEquals("SendSplitButton SendButtonStyle", form.SendSplitButton.SendButtonStyle, SendingModeSplitButton.ButtonStyle.AutomaticAction);
		}
	}

	public void TestSendSplitButtonStyleIsNotAffectedByShouldSendChangedEvent()
	{
		using (var form = new MessageSendingFormUcc6ForTest(DeclarationWrapper))
		{
			form.Show();

			DeclarationWrapper.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals("SendSplitButton SendButtonStyle", form.SendSplitButton.SendButtonStyle, SendingModeSplitButton.ButtonStyle.AutomaticAction);
		}
	}

	public void TestUserIsAskedForConfirmationWhenSendingCancellationMessage()
	{
		SetUpRegistryAccountAndDeclarant();

		using (var form = new MessageSendingFormUcc6(DeclarationWrapper))
		{
			form.Show();

			var sendingObject = DeclarationWrapper.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = "CAN";
			sendingObject.VOCReason = "A";
			sendingObject.CancellationAndAmendmentLegislativeReference = "1";
			form.SendSplitButton.Enabled = true;

			var entryHeader = sendingObject.Header;
			entryHeader.MovementReferenceNumberSetter("23CH00000302568360");
			entryHeader.CH_EntryStatus = "REG";

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			form.SendSplitButton.PerformClick();
			AssertEquals("Last message prompted to the user", "You are canceling a registered customs declaration. Do you confirm?", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestUserIsNotAskedForConfirmationWhenSendingNonCancellationMessage()
	{
		SetUpRegistryAccountAndDeclarant();

		using (var form = new MessageSendingFormUcc6(DeclarationWrapper))
		{
			form.Show();

			var sendingObject = DeclarationWrapper.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = "NEW";
			form.SendSplitButton.Enabled = true;

			form.SendSplitButton.PerformClick();
			AssertNotEquals("Last message prompted to the user", "You are canceling a registered customs declaration. Do you confirm?", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestDuplicatedDeclarationWarningPopupIsNotPromptedForCancellationMessages()
	{
		const string unexpectedWarningMessagePrefix = @"The following entries were already sent and are already registered or waiting for messages from Customs.
Resending these entries could result in duplicated declarations.";

		SetUpRegistryAccountAndDeclarant();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_Status = "AWO";

		var messageSendingObject = DeclarationWrapper.SendingObjectsCollection[0];
		messageSendingObject.ShouldSend = true;

		using (var form = new MessageSendingFormUcc6(DeclarationWrapper))
		{
			form.Show();
			var sendButton = form.SendSplitButton;
			sendButton.Enabled = true;

			messageSendingObject.MessageType = "CAN";
			messageSendingObject.VOCReason = "A";
			messageSendingObject.CancellationAndAmendmentLegislativeReference = "1";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			sendButton.PerformClick();
			AssertNotContains("Last message prompted to the user", unexpectedWarningMessagePrefix, UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestDuplicatedDeclarationWarningPopupIsPromptedForNonCancellationMessages()
	{
		const string unexpectedWarningMessagePrefix = @"The following entries were already sent and are already registered or waiting for messages from Customs.
Resending these entries could result in duplicated declarations.";

		SetUpRegistryAccountAndDeclarant();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_Status = "AWO";

		var messageSendingObject = DeclarationWrapper.SendingObjectsCollection[0];
		messageSendingObject.ShouldSend = true;

		using (var form = new MessageSendingFormUcc6(DeclarationWrapper))
		{
			form.Show();
			var sendButton = form.SendSplitButton;
			sendButton.Enabled = true;

			messageSendingObject.MessageType = "NEW";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			sendButton.PerformClick();
			AssertContains("Last message prompted to the user", unexpectedWarningMessagePrefix, UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestErrorDialogForNodeWithMAUCertificate()
	{
		var expectedMessage = "The Node you selected has no MAU Certificate. Please add it in Company>Brokerage for your company.";

		var declarantWithTwoNodes = Factory.NewWithValidTestData<OrgHeader>();
		declarantWithTwoNodes.OH_Code = "CC";
		Factory.Save();

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK.ToGuid())
			.AppendAccount("22222222222-001", "YYYY").AppendAccountDetail("CC-YYYY", "CC")
			.AppendAccount("33333333333-001", "ZZZZ").AppendAccountDetail("CC-ZZZZ", "CC")
		.Build();

		declaration.JE_OA_DeclarantAddress = declarantWithTwoNodes.MainAddress.PK;

		var companyWrapper = ITGlbCompanyWrapper.Get(company);
		var mauPassword = companyWrapper.PasswordCollection.AddNew();
		mauPassword.GP_UserID = "CC-ZZZZ";
		Factory.Save();

		var sendingObject = DeclarationWrapper.SendingObjectsCollection[0];
		sendingObject.ShouldSend = true;
		using (var form = new MessageSendingFormUcc6ForTest(DeclarationWrapper))
		{
			form.Show();
			var sendButton = form.SendSplitButton;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			sendButton.Enabled = true;

			declaration.JE_CustomsProfile = "CC-YYYY";
			sendButton.PerformClick();
			AssertContains("When CustomsProfile not present in compnay allowed list", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			declaration.JE_CustomsProfile = "CC-ZZZZ";
			sendButton.PerformClick();
			AssertNotContains("When CustomsProfile present in compnay allowed list", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertCollectionNotContains("All previous messages", UnitTestUserNotification.Instance.PreviousMessages, eachMessage => eachMessage.Text == expectedMessage);
		}
	}

	public void TestErrorDialogForSubsciberWithFiscalCode()
	{
		var temporaryStaff = Factory.NewWithValidTestData<GlbStaff>();
		Factory.Save();
		using (Env.Instance.SetTemporaryUserContext(temporaryStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			const string expectedMessage = "The Current User (Subscriber) has no Fiscal Code. Go to Staff and Resources > Human Resources > Certificates, ID and Training and add a 'COD' code in Type field and Fiscal code in Certificate Number.";

			SetUpRegistryAccountAndDeclarant();

			var messageSendingObject = DeclarationWrapper.SendingObjectsCollection[0];
			messageSendingObject.ShouldSend = true;

			using (var form = new MessageSendingFormUcc6(DeclarationWrapper))
			{
				form.Show();
				var sendButton = form.SendSplitButton;
				sendButton.Enabled = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				sendButton.PerformClick();
				AssertContains("Before adding Fiscal Code", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AddFiscalCodeToStaff(temporaryStaff);
				Factory.Save();

				sendButton.PerformClick();
				AssertNotContains("After adding Fiscal Code", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}

	public void TestFileNameNumberRangeValidationIsDisabled()
	{
		var messageSendingObject = DeclarationWrapper.SendingObjectsCollection[0];
		messageSendingObject.ShouldSend = true;

		using (var form = new MessageSendingFormUcc6(declarationWrapper))
		{
			form.Show();
			var sendSplitButton = form.SendSplitButton;
			sendSplitButton.Enabled = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			sendSplitButton.PerformClick();

			AssertNotEquals("Error dialog not expected", "No Account could be found for the selected Node.Please check that the selected Node in Misc.Tab is valid.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Form has been closed as error dialog was not expected", DialogResult.None, form.DialogResult);
		}
	}

	void SetUpRegistryAccountAndDeclarant()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1111", "00", "01")
			.AppendAccountDetail("1111-DEC1", "DEC1")
			.Build();

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var companyWrapper = ITGlbCompanyWrapper.Get(company);
		var mauPassword = companyWrapper.PasswordCollection.AddNew();
		mauPassword.GP_UserID = "1111-DEC1";
		Factory.Save();

		declaration.JE_CustomsProfile = "1111-DEC1";
	}

	void AddFiscalCodeToStaff(GlbStaff staff)
	{
		var codCertificate = staff.Certificates.AddNew();
		codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		codCertificate.XZ_RefNumber = "00891230153";
	}

	Ucc6JobDeclarationMessageSendingObjectParent DeclarationWrapper => declarationWrapper ?? (declarationWrapper = new Ucc6JobDeclarationMessageSendingObjectParent(declaration));
	Ucc6JobDeclarationMessageSendingObjectParent declarationWrapper;

	protected override void SetUp()
	{
		base.SetUp();

		AddFiscalCodeToStaff(GlbStaff.CurrentUser);
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_OA_DeclarantAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_JZ = invoiceHeader.PK;
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceLine invoiceLine;
}

class MessageSendingFormUcc6ForTest : MessageSendingFormUcc6
{
	public MessageSendingFormUcc6ForTest(Ucc6JobDeclarationMessageSendingObjectParent declarationWrapper, UserEnterableTokenPin userEnterableTokenPin = null, string mockLocatableSerialNumber = null) : base(declarationWrapper)
	{
		var certificatePinHandlerForTest = new XadesCertificatePinHandlerForTest(this, mockLocatableSerialNumber, userEnterableTokenPin);
		base.SetXadesCertificatePinHandler(certificatePinHandlerForTest);
	}

	public ZUserControl GetBottomSectionUserControlExposed() => GetBottomSectionUserControl();
}
