using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(GlbBrokerExternalPasswordValidation))]
sealed class GlbBrokerExternalPasswordValidationTest : GlbExternalPasswordValidationTest<GlbBrokerExternalPassword, GlbBrokerExternalPasswordValidation>
{
	public void TestCheckGP_UserIDMandatory()
	{
		var externalPassword = GetExternalPassword();

		externalPassword.GP_UserID = "";
		externalPassword.Validation.ValidateGP_UserID();
		AssertHasErrorContaining(externalPassword.GP_UserIDInfo, ValidationCaptions.GlbBrokerExternalPassword.NodeMustHaveValue);

		externalPassword.GP_UserID = "0000";
		AssertNoErrorContaining(externalPassword.GP_UserIDInfo, ValidationCaptions.GlbBrokerExternalPassword.NodeMustHaveValue);
	}

	public void TestCheckGP_UserIDBelongsToAccount()
	{
		var externalPassword = GetExternalPassword();

		externalPassword.GP_UserID = "0000";
		AssertHasErrorContaining(externalPassword.GP_UserIDInfo, ValidationCaptions.GlbBrokerExternalPassword.TheNodeMustBelongToValidAccountInRegistry);

		externalPassword.GP_UserID = "1234";
		AssertNoErrorContaining(externalPassword.GP_UserIDInfo, ValidationCaptions.GlbBrokerExternalPassword.TheNodeMustBelongToValidAccountInRegistry);
	}

	public void TestCheckGP_UserIDUnique()
	{
		var staff = Factory.New<GlbStaff>();
		var staffWrapper = GlbStaffWrapper.Get(staff);
		var externalPassword = staffWrapper.PasswordCollection.AddNew();

		externalPassword.GP_UserID = "1234";

		var externalPassword1 = staffWrapper.PasswordCollection.AddNew();
		externalPassword1.GP_UserID = "1234";
		externalPassword.Validation.ValidateGP_UserID();
		externalPassword1.Validation.ValidateGP_UserID();
		AssertHasErrorContaining(externalPassword.GP_UserIDInfo, ValidationCaptions.GlbBrokerExternalPassword.NodeMustBeUnique);
		AssertHasErrorContaining(externalPassword1.GP_UserIDInfo, ValidationCaptions.GlbBrokerExternalPassword.NodeMustBeUnique);

		externalPassword1.GP_UserID = "5678";
		externalPassword.Validation.ValidateGP_UserID();
		externalPassword1.Validation.ValidateGP_UserID();
		AssertNoErrorContaining(externalPassword.GP_UserIDInfo, ValidationCaptions.GlbBrokerExternalPassword.NodeMustBeUnique);
		AssertNoErrorContaining(externalPassword1.GP_UserIDInfo, ValidationCaptions.GlbBrokerExternalPassword.NodeMustBeUnique);
	}

	public void TestIsCertificateMandatory()
	{
		var validation = new GlbBrokerExternalPasswordValidationForTest(GetExternalPassword());
		AssertEquals("IsCertificateMandatory", false, validation.IsCertificateMandatoryExposed);
	}

	public void TestIsCurrentDecryptedCertificatePassphraseMandatory()
	{
		var validation = new GlbBrokerExternalPasswordValidationForTest(GetExternalPassword());
		AssertEquals("IsCurrentDecryptedCertificatePassphraseMandatory", false, validation.IsCurrentDecryptedCertificatePassphraseMandatoryExposed);
	}

	public void TestCheckCurrentDecryptedCertificatePassphrase()
	{
		const string expectedError = "When a Certificate is loaded, Certificate Password is mandatory";

		GlbExternalPassword.GP_Certificate = ZBlob.Empty;
		GlbExternalPassword.CurrentDecryptedCertificatePassphrase = "";
		AssertNoErrorContaining(GlbExternalPassword.CurrentDecryptedCertificatePassphraseInfo, expectedError);

		GlbExternalPassword.GP_Certificate = new byte[] { 1, 2, 3 };
		GlbExternalPassword.CurrentDecryptedCertificatePassphrase = "";
		AssertHasErrorContaining(GlbExternalPassword.CurrentDecryptedCertificatePassphraseInfo, expectedError);

		GlbExternalPassword.GP_Certificate = new byte[] { 1, 2, 3 };
		GlbExternalPassword.CurrentDecryptedCertificatePassphrase = "test";
		AssertNoErrorContaining(GlbExternalPassword.CurrentDecryptedCertificatePassphraseInfo, expectedError);
	}

	public void TestValidateAllCertificateOrXadesCertificateRequired()
	{
		const string expectedError = "Please enter a Certificate or a XADES Certificate.";

		var staffWrapper = GlbStaffWrapper.Get(Staff);
		GlbExternalPassword.Validation.ValidateAll();
		AssertHasRowError("When both Certificate and XADES Certificate are empty", GlbExternalPassword, expectedError);

		GlbExternalPassword.GP_Certificate = new byte[] { 1, 2, 3 };
		GlbExternalPassword.Validation.ValidateAll();
		AssertNoRowError("When Certificate is not empty and XADES Certificate is empty", GlbExternalPassword, expectedError);

		GlbExternalPassword.GP_Certificate = ZBlob.Empty;
		staffWrapper.CryptokiCertificateCollection.AddNew();
		GlbExternalPassword.Validation.ValidateAll();
		AssertNoRowError("When Certificate is empty and XADES Certificate is not empty", GlbExternalPassword, expectedError);

		GlbExternalPassword.GP_Certificate = new byte[] { 1, 2, 3 };
		GlbExternalPassword.Validation.ValidateAll();
		AssertNoRowError("When both Certificate and XADES Certificate are not empty", GlbExternalPassword, expectedError);
	}

	protected override GlbBrokerExternalPassword GetExternalPassword()
	{
		var staff = Factory.New<GlbStaff>();
		var staffWrapper = GlbStaffWrapper.Get(staff);
		return staffWrapper.PasswordCollection.AddNew();
	}

	protected override bool IsCertificateMandatory => false;
}

class GlbBrokerExternalPasswordValidationForTest : GlbBrokerExternalPasswordValidation
{
	public GlbBrokerExternalPasswordValidationForTest(GlbBrokerExternalPassword parent) : base(parent)
	{
	}

	public bool IsCertificateMandatoryExposed => IsCertificateMandatory;

	public bool IsCurrentDecryptedCertificatePassphraseMandatoryExposed => IsCurrentDecryptedCertificatePassphraseMandatory;
}
