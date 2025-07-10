using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.Customs.IT.GUI.Testing;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;
using NctsHeaderMessageSendingObjectParent = Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.NctsHeaderMessageSendingObjectParent;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingFormWithValidationDetailsAbstractTest
{
	public void TestConfirmationFormPopoutWhenHeaderIsNotAllowedToBeSend()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(nctsHeader);
		CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser(certificateSerialNumber: "0123456789", pin: "123ABC");

		nctsHeader.BH_JobReference = "0001";

		var expectedWarningMessage = @"The following entries were already sent and are already registered or waiting for messages from Customs.
Resending these entries could result in duplicated declarations.

Entries:
0001: MDS";

		using (var form = new MessageSendingFormForTest(NctsMessageSendingObjectParent))
		{
			form.Show();

			var sendButton = form.SendButtonExposed;
			sendButton.Enabled = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			nctsHeader.BH_MessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationSent;
			sendButton.PerformClick();
			AssertEquals("User confirmation message box text", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			nctsHeader.BH_MessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
			sendButton.PerformClick();
			AssertNull("User confirmation message box text", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	[RequiresSTA]
	public void TestSendWithValidationErrorsCheckBoxAvailability()
	{
		nctsHeader.BH_JobReference = "0001";

		using (var form = new MessageSendingForm(NctsMessageSendingObjectParent))
		{
			form.Show();

			var messageSendingObject = NctsMessageSendingObjectParent.SendingObjectsCollection[0];
			var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");

			messageSendingObject.MessageType = "CAN";

			CombineAssertions(() =>
			{
				AssertEquals("SendWithValidationErrorsCheckBox invisible when MessageType is CAN", false, sendWithValidationErrorsCheckBox.Visible);

				messageSendingObject.MessageType = "NEW";

				AssertEquals("SendWithValidationErrorsCheckBox visible when MessageType is NEW", true, sendWithValidationErrorsCheckBox.Visible);
			});
		}
	}

	public void TestSendButtonAvailability()
	{
		nctsHeader.BH_JobReference = "0001";
		var messageSendingObject = NctsMessageSendingObjectParent.SendingObjectsCollection[0];

		using (var form = new MessageSendingFormForTest(NctsMessageSendingObjectParent))
		{
			form.Show();
			var sendButton = form.SendButtonExposed;
			messageSendingObject.MessageType = "CAN";

			CombineAssertions(() =>
			{
				AssertEquals("SendButton enabled when MessageType is CAN", true, sendButton.Enabled);

				messageSendingObject.MessageType = "NEW";

				AssertEquals("SendButton disabled when MessageType is NEW", false, sendButton.Enabled);
			});
		}
	}

	[RequiresSTA]
	public void TestEventsAreUnhookedAfterDispose()
	{
		nctsHeader.BH_JobReference = "0001";
		ZButton sendButton;
		ZCheckBox sendWithValidationErrorsCheckBox;
		var messageSendingObject = NctsMessageSendingObjectParent.SendingObjectsCollection[0];

		using (var form = new MessageSendingForm(NctsMessageSendingObjectParent))
		{
			form.Show();
			sendButton = form.FindSingle<ZButton>("SendButton");
			sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");

			messageSendingObject.MessageType = "CAN";

			CombineAssertions("Before Dispose", () =>
			{
				AssertEquals("SendWithValidationErrorsCheckBox invisible when MessageType is CAN", false, sendWithValidationErrorsCheckBox.Visible);
				AssertEquals("SendButton enabled when MessageType is CAN", true, sendButton.Enabled);

				messageSendingObject.MessageType = "NEW";

				AssertEquals("SendWithValidationErrorsCheckBox visible when MessageType is NEW", true, sendWithValidationErrorsCheckBox.Visible);
				AssertEquals("SendButton disabled when MessageType is NEW", false, sendButton.Enabled);
			});
		}

		messageSendingObject.MessageType = "CAN";

		CombineAssertions("After Dispose", () =>
		{
			AssertEquals("SendWithValidationErrorsCheckBox visible when MessageType is CAN", true, sendWithValidationErrorsCheckBox.Visible);
			AssertEquals("SendButton disabled when MessageType is CAN", false, sendButton.Enabled);

			messageSendingObject.MessageType = "NEW";

			AssertEquals("SendWithValidationErrorsCheckBox visible when MessageType is NEW", true, sendWithValidationErrorsCheckBox.Visible);
			AssertEquals("SendButton disabled when MessageType is NEW", false, sendButton.Enabled);
		});
	}

	[RequiresSTA]
	public void TestXadesCertificateVerification_WhenLoggedUserDoesNotHaveItConfigured()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(nctsHeader);

		using (var form = new MessageSendingFormForTest(NctsMessageSendingObjectParent))
		{
			form.Show();

			NctsMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			form.SendButtonExposed.Enabled = true;
			form.SendButtonExposed.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Last message prompted to the user", "XADES certificate is missing for the current user. Please fill XADES Certificate information in Staff and Resources > Credentials.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Is form visible?", true, form.Visible);
			});
		}
	}

	[RequiresSTA]
	public void TestXadesCertificateVerification_WhenLoggedUserHasItConfiguredButNotPlugged()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(nctsHeader);
		CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser(certificateSerialNumber: "000000000");

		ShowFormAndAssertIfItChecksForCertificatePresence(AssertFormWhenCertificateNotFound);
	}

	public void TestDoesNotCheckForCertificatePresenceIfPinAlreadyEntered()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(nctsHeader);
		CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser(certificateSerialNumber: "000000000", pin: "123ABC");

		ShowFormAndAssertIfItChecksForCertificatePresence(AssertFormWhenCertificatePresenceNotChecked);
	}

	public void TestUserIsAskedToEnterPinIfNotEnteredYet()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(nctsHeader);
		var cryptokiCertificate = CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser();

		using (var form = new MessageSendingFormForTest(NctsMessageSendingObjectParent, mockLocatableSerialNumber: cryptokiCertificate.GP_CertificateSerialNumber))
		{
			form.Show();

			NctsMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			form.SendButtonExposed.Enabled = true;
			form.SendButtonExposed.PerformClick();

			AssertType<EnterCryptokiCertificatePinForm>("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
		}
	}

	public void TestUserEnteredPinIsPersistedInMemoryCache()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(nctsHeader);
		var cryptokiCertificate = CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser();

		using (var form = new MessageSendingFormForTest(NctsMessageSendingObjectParent, new UserEnterableTokenPin() { Pin = "123ABC" }, cryptokiCertificate.GP_CertificateSerialNumber))
		{
			form.Show();

			NctsMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			form.SendButtonExposed.Enabled = true;

			AssertEquals("PRE-CONDITION: Pin", "", cryptokiCertificate.TokenPinStore.GetPin());
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			form.SendButtonExposed.PerformClick();
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
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(nctsHeader);
		CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser(pin: "123ABC");

		using (var form = new MessageSendingFormForTest(NctsMessageSendingObjectParent))
		{
			form.Show();

			NctsMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			form.SendButtonExposed.Enabled = true;
			form.SendButtonExposed.PerformClick();

			CombineAssertions("POST-CONDITIONS", () =>
			{
				AssertNull("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Is form visible?", false, form.Visible);
				AssertEquals("Form DialogResult", DialogResult.OK, form.DialogResult);
			});
		}
	}

	public void TestErrorDialogForNodeWithMAUCertificate()
	{
		var expectedMessage = "The Node you selected has no MAU Certificate. Please add it in Company>Brokerage for your company.";

		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(nctsHeader);

		NctsMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
		using (var form = new MessageSendingFormForTest(NctsMessageSendingObjectParent))
		{
			form.Show();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			form.SendButtonExposed.Enabled = true;

			nctsHeader.BH_CustomsProfile = "CC-YYYY";
			form.SendButtonExposed.PerformClick();
			AssertContains("When CustomsProfile not present in company allowed list", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			nctsHeader.BH_CustomsProfile = "1111-DEC1";
			form.SendButtonExposed.PerformClick();
			AssertNotContains("When CustomsProfile present in company allowed list", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertCollectionNotContains("All previous messages", UnitTestUserNotification.Instance.PreviousMessages, eachMessage => eachMessage.Text == expectedMessage);
		}
	}

	public void TestErrorDialogForCurrentUserHasFiscalCode()
	{
		var temporaryStaff = Factory.NewWithValidTestData<GlbStaff>();
		Factory.Save();
		using (Env.Instance.SetTemporaryUserContext(temporaryStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			const string expectedMessage = "The Current User (Subscriber) has no Fiscal Code. Go to Staff and Resources > Human Resources > Certificates, ID and Training and add a 'COD' code in Type field and Fiscal code in Certificate Number.";

			CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(nctsHeader);
			CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser(pin: "123ABC");
			nctsHeader.BH_CustomsProfile = "1111-DEC1";

			var messageSendingObject = NctsMessageSendingObjectParent.SendingObjectsCollection[0];
			messageSendingObject.ShouldSend = true;

			using (var form = new MessageSendingFormForTest(NctsMessageSendingObjectParent))
			{
				form.Show();
				var sendButton = form.SendButtonExposed;
				sendButton.Enabled = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				sendButton.PerformClick();
				AssertEquals("Before adding Fiscal Code", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Before adding Fiscal Code", expected: true, UnitTestUserNotification.Instance.LastMessage.WasError);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AddFiscalCodeToStaff(temporaryStaff);
				Factory.Save();

				sendButton.PerformClick();
				AssertEquals("After adding Fiscal Code", expected: true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}
	}

	[RequiresSTA]
	public void TestNctsPhase5FormIsReadOnlyAfterSending()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		NctsHeaderMessageSendingObjectParentTest.SetupStaffCryptoKiCertificate();

		using var nctsForm = new EU.NCTS.GUI.Phase5DepartureMovementForm(nctsHeader);
		nctsForm.Show();

		Factory.Save();

		var customerReferenceNumberTextBox = nctsForm.FindSingle<ZTextBox>("CustomerReferenceNumberTextBox");
		AssertEquals("[PRE-CONDITION] CustomerReferenceNumberTextBox readonly", false, customerReferenceNumberTextBox.ReadOnly);

		var sendingMenuItem = nctsForm.Menu.MenuItems.FindByText("&NCTS").MenuItems.FindByText("Send to Customs");

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		sendingMenuItem.PerformClick();

		AssertEquals("BM_MessageStatus", "SNT", nctsHeader.MovementHeader.BM_MessageStatus);
		AssertEquals("After sending declaration, CustomerReferenceNumberTextBox readonly", true, customerReferenceNumberTextBox.ReadOnly);
	}

	[RequiresSTA]
	public void TestDoesNotCheckForCertificatePresenceIfUserHasActiveAutomaticRemoteSignature()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(nctsHeader);
		var cryptokiCertificate = CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser();
		var staffWrapper = Customs.IT.Business.GlbStaffWrapper.Get(GlbStaff.CurrentUser);
		staffWrapper.AutomaticSignaturePasswordCollection.AddNew().IsConfigurationActive = true;

		using (var form = new MessageSendingFormForTest(NctsMessageSendingObjectParent, mockLocatableSerialNumber: cryptokiCertificate.GP_CertificateSerialNumber))
		{
			form.Show();

			NctsMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			form.SendButtonExposed.Enabled = true;
			form.SendButtonExposed.PerformClick();

			CombineAssertions("POST-CONDITIONS", () =>
			{
				AssertNull("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Is form visible?", false, form.Visible);
				AssertEquals("Form DialogResult", DialogResult.OK, form.DialogResult);
			});
		}
	}

	protected override Form GetFormToBashCore() => new MessageSendingForm(NctsMessageSendingObjectParent);

	protected override void SetUp()
	{
		base.SetUp();
		AddFiscalCodeToStaff(GlbStaff.CurrentUser);
		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_MessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
	}

	void ShowFormAndAssertIfItChecksForCertificatePresence(Action<Form> assertExpectedResult)
	{
		using (var form = new MessageSendingFormForTest(NctsMessageSendingObjectParent))
		{
			form.Show();

			NctsMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			form.SendButtonExposed.Enabled = true;
			form.SendButtonExposed.PerformClick();

			CombineAssertions(() => assertExpectedResult(form));
		}
	}

	void AssertFormWhenCertificateNotFound(Form form)
	{
		AssertEquals("Last message prompted to the user", "Cannot locate XADES certificate for the current user. Please insert the certificate in your computer and retry the operation.", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals("Is form visible?", true, form.Visible);
	}

	void AssertFormWhenCertificatePresenceNotChecked(Form form)
	{
		AssertNull("Last message prompted to the user", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals("Is form visible?", false, form.Visible);
	}

	void AddFiscalCodeToStaff(GlbStaff staff)
	{
		var codCertificate = staff.Certificates.AddNew();
		codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
		codCertificate.XZ_RefNumber = "00891230153";
	}

	NctsHeader nctsHeader;

	NctsHeaderMessageSendingObjectParent NctsMessageSendingObjectParent => nctsMessageSendingObjectParent ?? (nctsMessageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader));
	NctsHeaderMessageSendingObjectParent nctsMessageSendingObjectParent;

	sealed class MessageSendingFormForTest : MessageSendingForm
	{
		public MessageSendingFormForTest(NctsHeaderMessageSendingObjectParent sendingObjectParent, UserEnterableTokenPin userEnterableTokenPin = null, string mockLocatableSerialNumber = null) : base(sendingObjectParent)
		{
			var certificatePinHandlerForTest = new XadesCertificatePinHandlerForTest(this, mockLocatableSerialNumber, userEnterableTokenPin);
			SetXadesCertificatePinHandler(certificatePinHandlerForTest);
		}

		public ZUserControl GetBottomSectionUserControlExposed() => GetBottomSectionUserControl();

		public ZButton SendButtonExposed => SendButton;
	}
}
