using System.Security.Cryptography.X509Certificates;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class GlbExternalPasswordTest<T> : GlbExternalPasswordWithCertificateTest<T>
	where T : GlbExternalPassword
{
	public override void TestSetDefaultValues()
	{
		AssertEquals("GP_PasswordType", PasswordType, GlbExternalPassword.GP_PasswordType);
		AssertEquals("GP_PasswordStatus", PasswordStatusList.Codes.Invalid, GlbExternalPassword.GP_PasswordStatus);
	}

	public void TestPasswordStatus()
	{
		GlbExternalPassword.GP_PasswordStatus = "VAL";
		AssertEquals("Valid", GlbExternalPassword.PasswordStatus);

		GlbExternalPassword.GP_PasswordStatus = "INV";
		AssertEquals("Invalid", GlbExternalPassword.PasswordStatus);
	}

	public override void TestDataDefaultFromCertificate()
	{
		void SetValidCertificate()
		{
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			GlbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		}
		var validCertificate = new X509Certificate2(X509Certificate2TestHelper.ValidCertificate, X509Certificate2TestHelper.ValidPassword);
		SetValidCertificate();
		AssertEquals("Expiry Date has been defaulted from valid certificate", validCertificate.NotAfter, GlbExternalPassword.GP_ExpiryDate);

		SetValidCertificate();
		var invalidCertificate = new byte[] { 1, 2, 3, 4 };
		GlbExternalPassword.GP_Certificate = invalidCertificate;
		AssertEquals("Expiry Date has been cleared (invalid certificate data)", ZDate.Empty, GlbExternalPassword.GP_ExpiryDate);

		SetValidCertificate();
		var invalidPassword = "123456";
		GlbExternalPassword.CurrentDecryptedCertificatePassphrase = invalidPassword;
		AssertEquals("Expiry Date has been cleared (invalid certificate data - password)", ZDate.Empty, GlbExternalPassword.GP_ExpiryDate);
	}

	public override void TestReadOnly()
	{
		AssertEquals("GP_IssueDateInfo.ReadOnly", true, GlbExternalPassword.GP_IssueDateInfo.ReadOnly);
		AssertEquals("GP_ExpiryDateInfo.ReadOnly", true, GlbExternalPassword.GP_ExpiryDateInfo.ReadOnly);
		AssertEquals("GP_UserIDInfo.ReadOnly", false, GlbExternalPassword.GP_UserIDInfo.ReadOnly);
	}

	public void TestCurrentDecryptedPasswordMaxLength()
	{
		AssertEquals(32, GlbExternalPassword.CurrentDecryptedCertificatePassphraseInfo.MaxLength);
	}

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
	}

	protected abstract ZString PasswordType { get; }
}
