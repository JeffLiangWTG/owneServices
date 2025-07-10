using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SendMessageForm))]
sealed class SendMessageFormTest : Customs.GUI.Testing.MessageSendingFormWithValidationDetailsAbstractTest
{
	protected override Form GetFormToBashCore() => GetNewMessageSendingForm();

	SendMessageForm GetNewMessageSendingForm()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
		return new SendMessageForm(sendingObjectParent);
	}

	public void TestSetTokenPinIfNeeded_TokenPinNotSet()
	{
		GlbStaff.CurrentUser.GS_LoginName = "Enterprise User";
		var password = IN.Business.GlbStaffWrapper.GetWrapperForCurrentUser()?.LoginPassword;
		password.GP_GS = GlbStaff.CurrentUser.PK;
		password.GP_UserID = "test";
		password.GP_MailBoxID = "test@test.com";

		using var form = new SendMessageFormForTest(MessageSendingObjectParent);
		var tokenPinStore = CertificateTokenPinStore.Instance as ITokenPinStore;
		tokenPinStore.ResetPin();
		form.Show();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		AssertEquals("Pre-Condition: Pin empty", ZString.Empty, tokenPinStore.GetPin());
		form.SendButtonExposed.Enabled = true;
		form.SendButtonExposed.PerformClick();
		var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
		AssertType<EnterCryptokiCertificatePinForm>("Enter token pin form displayed", lastFormShown);
		lastFormShown.Dispose();
		AssertEquals("Message sending canceled", DialogResult.None, form.DialogResult);
		AssertContains("Token pin not set", "Message sending canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestSetTokenPinIfNeeded_TokenPinSet()
	{
		GlbStaff.CurrentUser.GS_LoginName = "Enterprise User";
		var password = IN.Business.GlbStaffWrapper.GetWrapperForCurrentUser()?.LoginPassword;
		password.GP_GS = GlbStaff.CurrentUser.PK;
		password.GP_UserID = "test";
		password.GP_MailBoxID = "test@test.com";

		using var form = new SendMessageFormForTest(MessageSendingObjectParent);
		var tokenPinStore = CertificateTokenPinStore.Instance as ITokenPinStore;
		tokenPinStore.ResetPin();
		tokenPinStore.SetPin("1234");
		form.Show();
		form.SendButtonExposed.Enabled = true;
		form.SendButtonExposed.PerformClick();
		var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
		AssertNull("Enter token pin form not displayed", lastFormShown);
		AssertEquals("Message sending started", DialogResult.OK, form.DialogResult);
	}

	public void TestSetTokenPinIfNeeded_IsSupport()
	{
		using var form = new SendMessageFormForTest(MessageSendingObjectParent);
		form.Show();
		form.SendButtonExposed.Enabled = true;
		form.SendButtonExposed.PerformClick();
		var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
		AssertNull("Enter token pin form not displayed", lastFormShown);
		AssertEquals("Message sending started", DialogResult.OK, form.DialogResult);
	}

	public void TestContext()
	{
		using var form = new SendMessageFormForTest(MessageSendingObjectParent);
		AssertEquals("Default context", MessageSendingContext.EMAIL, form.Context);
		form.Show();
		form.DownloadButtonExposed.Enabled = true;
		form.DownloadButtonExposed.PerformClick();
		AssertEquals("Download Context", MessageSendingContext.DOWNLOAD, form.Context);

		form.SendButtonExposed.Enabled = true;
		form.SendButtonExposed.PerformClick();
		AssertEquals("Email Context", MessageSendingContext.EMAIL, form.Context);
	}

	new DeclarationMessageSendingObjectParent GetMessageSendingObjectParent()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		var messageSendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
		messageSendingObjectParent.SendingObjectsCollection.Cast<DeclarationMessageSendingObject>().First().ShouldSend = ZBool.True;
		return messageSendingObjectParent;
	}

	DeclarationMessageSendingObjectParent MessageSendingObjectParent => messageSendingObjectParent ??= GetMessageSendingObjectParent();
	DeclarationMessageSendingObjectParent messageSendingObjectParent;
}
