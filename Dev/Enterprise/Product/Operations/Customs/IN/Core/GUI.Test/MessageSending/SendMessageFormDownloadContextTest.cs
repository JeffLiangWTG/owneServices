using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SendMessageForm))]
sealed class SendMessageFormDownloadContextTest : ZFormBasherTest
{
	public void TestSetTokenPinIfNeeded_TokenPinNotSet()
	{
		CombineAssertions(() =>
		{
			GlbStaff.CurrentUser.GS_LoginName = "Enterprise User";
			SetupLoginPassword();
			using var form = GetNewMessageSendingFormForPinDialog();
			var tokenPinStore = CertificateTokenPinStore.Instance as ITokenPinStore;
			tokenPinStore.ResetPin();
			form.Show();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			AssertEquals("Pre-Condition: Pin empty", ZString.Empty, tokenPinStore.GetPin());
			form.DownloadButtonExposed.Enabled = true;
			form.DownloadButtonExposed.PerformClick();
			var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;
			AssertType<EnterCryptokiCertificatePinForm>("Enter token pin form displayed", lastFormShown);
			lastFormShown?.Dispose();
			AssertEquals("Message downloading canceled", DialogResult.None, form.DialogResult);
			AssertContains("Token pin not set", "Message downloading canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	protected override Form GetFormToBashCore() => GetNewMessageSendingForm();

	SendMessageForm GetNewMessageSendingForm()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
		return new SendMessageForm(sendingObjectParent);
	}

	SendMessageFormForTest GetNewMessageSendingFormForPinDialog()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
		sendingObjectParent.SendingObjectsCollection.Cast<DeclarationMessageSendingObject>().First().ShouldSend = ZBool.True;
		return new SendMessageFormForTest(sendingObjectParent);
	}

	void SetupLoginPassword()
	{
		var password = Business.GlbStaffWrapper.GetWrapperForCurrentUser().LoginPassword;
		password.GP_UserID = "ICEGATEUSER";
		password.GP_MailBoxID = "user@ice.in";
	}
}
