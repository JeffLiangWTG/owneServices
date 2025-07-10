using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(GlbCompanyCredentialValidation))]
public class GlbCompanyCredentialValidationTest : GlbExternalPasswordWithCertificateValidationTest<GlbCompanyCredential, GlbCompanyCredentialValidation>
{
	public void TestCheckCurrentDecryptedCertificatePassphrase()
	{
		var glbExternalPassword = GlbExternalPassword;
		var currentDecryptedCertificatePassphraseInfo = glbExternalPassword.CurrentDecryptedCertificatePassphraseInfo;

		glbExternalPassword.CurrentDecryptedCertificatePassphrase = ZString.Empty;
		AssertNoError(currentDecryptedCertificatePassphraseInfo, "Please enter a Certificate Password.");

		glbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		glbExternalPassword.Validation.ValidateCurrentDecryptedCertificatePassphrase();
		AssertHasError(currentDecryptedCertificatePassphraseInfo, "Please enter a Certificate Password.");

		glbExternalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
		AssertNoError(currentDecryptedCertificatePassphraseInfo, "Please enter a Certificate Password.");
	}

	public void TestCheckGP_Certificate_Mandatory()
	{
		var glbExternalPassword = GlbExternalPassword;
		var certificateInfo = glbExternalPassword.GP_CertificateInfo;

		glbExternalPassword.CurrentDecryptedCertificatePassphrase = ZString.Empty;
		AssertNoError(certificateInfo, "Please enter a Certificate.");

		glbExternalPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
		glbExternalPassword.Validation.ValidateGP_Certificate();
		AssertHasError(certificateInfo, "Please enter a Certificate.");

		glbExternalPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		AssertNoError(certificateInfo, "Please enter a Certificate.");
	}

	public void TestCheckGP_ExpiryDate()
	{
		var glbExternalPassword = Factory.New<GlbCompanyCredential>();
		var errorCertiticateExpired = "The certificate has expired.";
		var warningCertiticateExpiringShortly = "The certificate will shortly expire.";
		var errorCertiticateNotValidYet = "The certificate is not valid yet.";

		glbExternalPassword.GP_ExpiryDate = ZDateTime.Now.AddDays(-1);
		AssertHasWarning(glbExternalPassword.GP_ExpiryDateInfo, errorCertiticateExpired);
		AssertNoWarning(glbExternalPassword.GP_ExpiryDateInfo, warningCertiticateExpiringShortly);
		AssertNoWarning(glbExternalPassword.GP_ExpiryDateInfo, errorCertiticateNotValidYet);

		glbExternalPassword.GP_ExpiryDate = ZDateTime.Now.AddDays(1);
		AssertNoWarning(glbExternalPassword.GP_ExpiryDateInfo, errorCertiticateExpired);
		AssertHasWarning(glbExternalPassword.GP_ExpiryDateInfo, warningCertiticateExpiringShortly);

		glbExternalPassword.GP_IssueDate = ZDateTime.Now.AddDays(1);
		glbExternalPassword.GP_ExpiryDate = ZDateTime.Now.AddDays(31);
		AssertHasWarning(glbExternalPassword.GP_ExpiryDateInfo, errorCertiticateNotValidYet);

		glbExternalPassword.GP_IssueDate = ZDateTime.Now.AddDays(-1);
		glbExternalPassword.Validation.ValidateGP_ExpiryDate();
		AssertNoWarning(glbExternalPassword.GP_ExpiryDateInfo, warningCertiticateExpiringShortly);
		AssertNoWarning(glbExternalPassword.GP_ExpiryDateInfo, errorCertiticateExpired);
		AssertNoWarning(glbExternalPassword.GP_ExpiryDateInfo, errorCertiticateNotValidYet);
	}

	public void TestCheckGP_ExpiryDateIsValidZDateTimeRange()
	{
		var glbExternalPassword = GlbExternalPassword;
		var currentYear = ZDateTime.Today.Year;
		glbExternalPassword.GP_ExpiryDate = new ZDateTime(currentYear + 2, 01, 01);
		AssertNoWarnings(glbExternalPassword.GP_ExpiryDateInfo);
	}

	protected override bool IsCertificateMandatory => false;
}
