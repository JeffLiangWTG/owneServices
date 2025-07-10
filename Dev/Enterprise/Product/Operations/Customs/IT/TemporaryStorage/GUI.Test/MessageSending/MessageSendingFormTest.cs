using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.Customs.IT.GUI.Testing;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingFormWithValidationDetailsAbstractTest
{
	public void TestConfirmationFormPopoutWhenHeaderIsNotAllowedToBeSend()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(header);
		CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser(certificateSerialNumber: "0123456789", pin: "123ABC");

		header.AMA_JobReference = "0001";

		var expectedWarningMessage = @"The following entries were already sent and are already registered or waiting for messages from Customs.
Resending these entries could result in duplicated declarations.

Entries:
0001: TPA";

		var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(header);
		using var form = new MessageSendingFormForTest(sendingObjectParent);
		form.Show();

		var sendButton = form.SendButtonExposed;
		sendButton.Enabled = true;

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
		header.AMA_MessageStatus = "SNT";
		header.CustomsStatus = "TPA";
		sendButton.PerformClick();
		AssertEquals("User confirmation message box text", expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);

		UnitTestUserNotification.Instance.ClearMessages();
		header.AMA_MessageStatus = "ACK";
		sendButton.PerformClick();
		AssertNull("User confirmation message box text", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	[RequiresSTA]
	public void TestXadesCertificateVerification_WhenLoggedUserDoesNotHaveItConfigured()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(header);

		var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(header);
		using var form = new MessageSendingFormForTest(sendingObjectParent);
		form.Show();

		sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
		form.SendButtonExposed.Enabled = true;
		form.SendButtonExposed.PerformClick();

		CombineAssertions(() =>
		{
			AssertEquals("Last message prompted to the user", "XADES certificate is missing for the current user. Please fill XADES Certificate information in Staff and Resources > Credentials.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Is form visible?", true, form.Visible);
		});
	}

	[RequiresSTA]
	public void TestXadesCertificateVerification_WhenLoggedUserHasItConfiguredButNotPlugged()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(header);
		CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser(certificateSerialNumber: "000000000");

		ShowFormAndAssertIfItChecksForCertificatePresence(AssertFormWhenCertificateNotFound);
	}

	[RequiresSTA]
	public void TestDoesNotCheckForCertificatePresenceIfPinAlreadyEntered()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(header);
		CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser(certificateSerialNumber: "000000000", pin: "123ABC");

		ShowFormAndAssertIfItChecksForCertificatePresence(AssertFormWhenCertificatePresenceNotChecked);
	}

	[RequiresSTA]
	public void TestUserIsAskedToEnterPinIfNotEnteredYet()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(header);
		var cryptokiCertificate = CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser();

		var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(header);
		using var form = new MessageSendingFormForTest(sendingObjectParent, mockLocatableSerialNumber: cryptokiCertificate.GP_CertificateSerialNumber);
		form.Show();

		sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
		form.SendButtonExposed.Enabled = true;
		form.SendButtonExposed.PerformClick();

		AssertType<EnterCryptokiCertificatePinForm>("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
	}

	[RequiresSTA]
	public void TestUserEnteredPinIsPersistedInMemoryCache()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(header);
		var cryptokiCertificate = CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser();

		var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(header);
		using var form = new MessageSendingFormForTest(sendingObjectParent, new UserEnterableTokenPin() { Pin = "123ABC" }, cryptokiCertificate.GP_CertificateSerialNumber);
		form.Show();

		sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
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

	public void TestUserIsNotAskedToEnterPinIfAlreadyEntered()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(header);
		CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser(pin: "123ABC");

		var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(header);
		using var form = new MessageSendingFormForTest(sendingObjectParent);
		form.Show();

		sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
		form.SendButtonExposed.Enabled = true;
		form.SendButtonExposed.PerformClick();

		CombineAssertions("POST-CONDITIONS", () =>
		{
			AssertNull("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Is form visible?", false, form.Visible);
			AssertEquals("Form DialogResult", DialogResult.OK, form.DialogResult);
		});
	}

	[RequiresSTA]
	public void TestDoesNotCheckForCertificatePresenceIfUserHasActiveAutomaticRemoteSignature()
	{
		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(header);
		var cryptokiCertificate = CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser();
		var staffWrapper = IT.Business.GlbStaffWrapper.Get(GlbStaff.CurrentUser);
		staffWrapper.AutomaticSignaturePasswordCollection.AddNew().IsConfigurationActive = true;

		var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(header);
		using var form = new MessageSendingFormForTest(sendingObjectParent, mockLocatableSerialNumber: cryptokiCertificate.GP_CertificateSerialNumber);
		form.Show();

		sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
		form.SendButtonExposed.Enabled = true;
		form.SendButtonExposed.PerformClick();

		CombineAssertions("POST-CONDITIONS", () =>
		{
			AssertNull("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Is form visible?", false, form.Visible);
			AssertEquals("Form DialogResult", DialogResult.OK, form.DialogResult);
		});
	}

	public void TestValidationErrorsContainsPlaceID()
	{
		var orgHeader = Factory.New<OrgHeader>();

		header.GoodsLocation.AdditionalIdentifier = "XXX";
		header.GoodsLocation.Address.IdentificationHolderPK = orgHeader.PK;
		header.TransportType = "10";

		var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(header);
		using var form = new MessageSendingFormForTest(sendingObjectParent);
		form.Show();

		var validationErrorsTextBox = form.FindSingle<ZTextBox>("ValidationErrorsTextBox");
		AssertContains("Place ID: The code you have selected is not in the list.", validationErrorsTextBox.Text);

		header.GoodsLocation.Address.IdentificationHolderPK = ZGuid.Empty;
		AssertCollectionNotContains("Place ID: The code you have selected is not in the list.", validationErrorsTextBox.Text);
	}

	public void TestMAUCertificateErrorDialog()
	{
		var expectedMessage = "The Node you selected has no MAU Certificate. Please add it in Company>Brokerage for your company.";

		CustomsCredentialAndCertificateTestHelper.SetUpRegistryAccountAndDeclarant(header);
		CustomsCredentialAndCertificateTestHelper.AddNewCryptoKiCertificateToCurrentUser(pin: "123ABC");

		var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(header);
		using var form = new MessageSendingFormForTest(sendingObjectParent);
		form.Show();
		form.SendButtonExposed.Enabled = true;

		header.AMA_CustomsProfile = "0000-DEC1";
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		form.SendButtonExposed.PerformClick();
		AssertContains("When AMA_CustomsProfile has no MAU certificate", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

		header.AMA_CustomsProfile = "1111-DEC1";
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		form.SendButtonExposed.PerformClick();
		AssertCollectionNotContains("When AMA_CustomsProfile has MAU certificate", UnitTestUserNotification.Instance.PreviousMessages, m => m.Text == expectedMessage);
	}

	protected override Form GetFormToBashCore() => new MessageSendingForm(new TemporaryStorageMessageSendingObjectParent(header));

	protected override void SetUp()
	{
		base.SetUp();
		AddFiscalCodeToStaff(GlbStaff.CurrentUser);
		header = Factory.New<TemporaryStorageHeader>();
		header.AMA_AgentType = "DIR";
	}

	TemporaryStorageHeader header;

	void ShowFormAndAssertIfItChecksForCertificatePresence(Action<Form> assertExpectedResult)
	{
		var sendingObjectParent = new TemporaryStorageMessageSendingObjectParent(header);
		using var form = new MessageSendingFormForTest(sendingObjectParent);
		form.Show();

		sendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
		form.SendButtonExposed.Enabled = true;
		form.SendButtonExposed.PerformClick();

		CombineAssertions(() => assertExpectedResult(form));
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

	sealed class MessageSendingFormForTest : MessageSendingForm
	{
		public MessageSendingFormForTest(TemporaryStorageMessageSendingObjectParent sendingObjectParent, UserEnterableTokenPin userEnterableTokenPin = null, string mockLocatableSerialNumber = null) : base(sendingObjectParent)
		{
			var certificatePinHandlerForTest = new XadesCertificatePinHandlerForTest(this, mockLocatableSerialNumber, userEnterableTokenPin);
			SetXadesCertificatePinHandler(certificatePinHandlerForTest);
		}

		public ZButton SendButtonExposed => SendButton;
	}
}
