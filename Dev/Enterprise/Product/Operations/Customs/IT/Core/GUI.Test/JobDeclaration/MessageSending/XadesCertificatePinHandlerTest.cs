using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI.Certificates;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using GlbStaffWrapper = Enterprise.Customs.IT.Business.GlbStaffWrapper;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class XadesCertificatePinHandlerTest : TestCaseForAttachGUI
{
	public void TestUserShownErrorIfNoCertificateIsConfigured()
	{
		SetUpRegistryAccountAndDeclarant();
		var handler = new XadesCertificatePinHandler();
		var handlerResult = handler.HandleTokenPin();

		AssertEquals(nameof(handlerResult), XadesCertificatePinHandler.XadesCertificatePinHandlerResult.Cancelled, handlerResult);
		AssertEquals("Last message prompted to the user", "XADES certificate is missing for the current user. Please fill XADES Certificate information in Staff and Resources > Credentials.", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestUserIsAskedToEnterPinIfNotEnteredYet()
	{
		SetUpRegistryAccountAndDeclarant();

		var staffWrapper = GlbStaffWrapper.Get(GlbStaff.CurrentUser);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = "BIT4ID";
		cryptokiCertificate.GP_CertificateSerialNumber = "0123456789";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var form = new ZForm(declaration))
		{
			form.Show();
			var handler = new XadesCertificatePinHandlerForTest(form, cryptokiCertificate.GP_CertificateSerialNumber, new UserEnterableTokenPin());
			var handlerResult = handler.HandleTokenPin();

			AssertEquals(nameof(handlerResult), XadesCertificatePinHandler.XadesCertificatePinHandlerResult.UserCancelled, handlerResult);
			AssertType<EnterCryptokiCertificatePinForm>("Last Form Shown", ZFormModaliser.LastFormShownDialogForTest);
		}
	}

	public void TestUserEnteredPinIsPersistedInMemoryCache()
	{
		SetUpRegistryAccountAndDeclarant();
		var staffWrapper = GlbStaffWrapper.Get(GlbStaff.CurrentUser);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = "BIT4ID";
		cryptokiCertificate.GP_CertificateSerialNumber = "0123456789";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var form = new ZForm(declaration))
		{
			form.Show();
			AssertEquals("PRE-CONDITION: Pin", "", cryptokiCertificate.TokenPinStore.GetPin());

			var handler = new XadesCertificatePinHandlerForTest(form, cryptokiCertificate.GP_CertificateSerialNumber, new UserEnterableTokenPin { Pin = "PQR1234" });
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var handlerResult = handler.HandleTokenPin();

			AssertEquals(nameof(handlerResult), XadesCertificatePinHandler.XadesCertificatePinHandlerResult.Completed, handlerResult);
			AssertEquals("Pin", "PQR1234", cryptokiCertificate.TokenPinStore.GetPin());
		}
	}

	public void TestUserNotAbleToLocateCertificate()
	{
		SetUpRegistryAccountAndDeclarant();
		var staffWrapper = GlbStaffWrapper.Get(GlbStaff.CurrentUser);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = "BIT4ID";
		cryptokiCertificate.GP_CertificateSerialNumber = "0123456789";
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var form = new ZForm(declaration))
		{
			form.Show();
			AssertEquals("PRE-CONDITION: Pin", "", cryptokiCertificate.TokenPinStore.GetPin());

			var handler = new XadesCertificatePinHandlerForTest(form, "TESTCERT_SERIAL", new UserEnterableTokenPin());
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var handlerResult = handler.HandleTokenPin();

			AssertEquals(nameof(handlerResult), XadesCertificatePinHandler.XadesCertificatePinHandlerResult.ErrorReported, handlerResult);
			AssertEquals("Last message prompted to the user", "Cannot locate XADES certificate for the current user. Please insert the certificate in your computer and retry the operation.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	void SetUpRegistryAccountAndDeclarant()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1111", "00", "01")
			.AppendAccountDetail("1111-DEC1", "DEC1")
			.Build();

		declaration.JE_CustomsProfile = "1111-DEC1";
	}

	JobDeclaration declaration;
}
