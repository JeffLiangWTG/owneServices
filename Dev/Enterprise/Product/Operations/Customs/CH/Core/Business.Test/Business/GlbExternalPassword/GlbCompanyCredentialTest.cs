using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(GlbCompanyCredential))]
class GlbCompanyCredentialTest : GlbExternalPasswordWithCertificateTest<GlbCompanyCredential>
{
	protected override GlbCompanyCredential CreateNewGlbExternalPassword(BusinessObjectFactory factory) => factory.New<GlbCompanyCredential>();

	public override void TestSetDefaultValues()
	{
		var credential = Factory.New<GlbCompanyCredential>();

		AssertEquals(PasswordTypesList.Codes.CHC, credential.GP_PasswordType);
		AssertNotEquals(ZGuid.Empty, credential.GP_GC);
		AssertEquals(PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
		AssertEquals(PasswordStatusList.Descriptions.Invalid, credential.PasswordStatus);
	}

	public void TestPasswordStatus()
	{
		GlbExternalPassword.GP_PasswordStatus = "VAL";
		AssertContains("Valid", GlbExternalPassword.GP_PasswordStatusDescription);

		GlbExternalPassword.GP_PasswordStatus = "INV";
		AssertContains("Invalid", GlbExternalPassword.GP_PasswordStatusDescription);
	}

	public override void TestReadOnly()
	{
		AssertEquals("GP_PasswordStatus.ReadOnly", false, GlbExternalPassword.GP_PasswordStatusInfo.ReadOnly);
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
		AssertEquals("Issue Date has been defaulted from valid certificate", validCertificate.NotBefore, GlbExternalPassword.GP_IssueDate);
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

	public void TestGetMessageAttrDictionary()
	{
		var newCompany = Factory.New<GlbCompany>();
		newCompany.GC_Code = "SAG";
		var wrapper = MasterFiles.Business.GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(newCompany);
		var credential = wrapper.GlbExternalPassword;
		credential.GP_PasswordType = PasswordTypesList.Codes.CHC;
		credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
		credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;

		var value = string.Empty;
		var dictionary = credential.GetMessageAttrDictionary();

		CombineAssertions(() =>
		{
			dictionary.TryGetValue("cw1.key", out value);
			AssertEquals("key", X509Certificate2TestHelper.ValidCertificate_KeyPEM, value);

			dictionary.TryGetValue("cw1.certificate", out value);
			AssertEquals("certificate", X509Certificate2TestHelper.ValidCertificate_CertPEM, value);
		});
	}
}
