using System.IO;
using System.Reflection;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordValidation_CCT))]
	public class GlbExternalPasswordValidation_CCTTest : GlbExternalPasswordWithCertificateValidationTest<GlbExternalPassword_CCT, GlbExternalPasswordValidation_CCT>
	{
		public void TestCheckGP_ExpiryDate()
		{
			var staff = Factory.New<GlbStaff>();
			var wrapper = BRGlbStaffWrapper.Get(staff);
			var password = wrapper.CCTPassword;
			password.GP_ExpiryDate = ZDateTime.Now.AddDays(-3);
			AssertHasError(password.GP_ExpiryDateInfo, "This certificate is not valid – the expiry date is in the past.");
			AssertNoWarning(password.GP_ExpiryDateInfo, "This certificate will expire soon – the expiry date is within one month.");
			password.GP_ExpiryDate = ZDateTime.Now.AddDays(3);
			AssertNoError(password.GP_ExpiryDateInfo, "This certificate is not valid – the expiry date is in the past.");
			AssertHasWarning(password.GP_ExpiryDateInfo, "This certificate will expire soon – the expiry date is within one month.");
			password.GP_ExpiryDate = ZDateTime.Now.AddDays(36);
			AssertNoError(password.GP_ExpiryDateInfo, "This certificate is not valid – the expiry date is in the past.");
			AssertNoWarning(password.GP_ExpiryDateInfo, "This certificate will expire soon – the expiry date is within one month.");
		}

		public void TestCheckGP_Certificate_ChainErrors()
		{
			var staff = Factory.New<GlbStaff>();
			var wrapper = BRGlbStaffWrapper.Get(staff);
			var password = wrapper.CCTPassword;
			var chainError = """
1. The certificate is not valid due to an untrusted root certification.
2. The certificate has expired. Please create a new certificate.
""";
			password.Validation.ValidateGP_Certificate();
			AssertNoError(password.GP_CertificateInfo, chainError);
			AssertNoError(password.GP_CertificateInfo, "Please enter a Certificate.");

			wrapper.EventSubscriptions.AddNew();
			password.Validation.ValidateGP_Certificate();
			AssertHasError(password.GP_CertificateInfo, "Please enter a Certificate.");

			password.CurrentDecryptedCertificatePassphrase = "1234";
			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = assembly.GetManifestResourceStream("Enterprise.Customs.BR.Business.Testing.Resources.Certificate_HasExpiredDate.pfx"))
			using (var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				password.GP_Certificate = memoryStream.ToArray();
				AssertHasError(password.GP_CertificateInfo, chainError);
			}
		}

		public void TestCheckGP_CertificateWaitingForResponse()
		{
			var staff = Factory.New<GlbStaff>();
			var wrapper = BRGlbStaffWrapper.Get(staff);
			var password = wrapper.CCTPassword;

			password.CurrentDecryptedCertificatePassphrase = "1234";
			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = assembly.GetManifestResourceStream("Enterprise.Customs.BR.Business.Testing.Resources.Certificate_HasExpiredDate.pfx"))
			using (var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				password.GP_Certificate = memoryStream.ToArray();
				var subscription = password.EventSubscriptions.AddNew();
				subscription.GP_MailBoxID = "123456789";
				Factory.Save();
				password.Validation.ValidateGP_Certificate();
				AssertNoError(password.GP_CertificateInfo, "Please cancel the active Subscription(s) before replacing the Certificate.");

				password.GP_ExpiryDate = ZDateTime.Now.AddDays(+1);
				Factory.Save();
				password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
				password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				password.Validation.ValidateGP_Certificate();
				AssertHasError(password.GP_CertificateInfo, "Please cancel the active Subscription(s) before replacing the Certificate.");

				subscription.GP_MailBoxID = ZString.Empty;
				password.Validation.ValidateGP_Certificate();
				AssertNoError(password.GP_CertificateInfo, "Please cancel the active Subscription(s) before replacing the Certificate.");

				password.GP_Certificate = (ZBlob)password.GP_CertificateInfo.OriginalValue;
				password.CurrentDecryptedCertificatePassphrase = (ZString)password.CurrentDecryptedCertificatePassphraseInfo.OriginalValue;
				password.GP_ExpiryDate = ZDateTime.Now.AddDays(-1);
				Factory.Save();
				subscription.GP_MailBoxID = "123456789";
				password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
				password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				password.Validation.ValidateGP_Certificate();
				AssertNoError(password.GP_CertificateInfo, "Please cancel the active Subscription(s) before replacing the Certificate.");
			}
		}

		protected override bool IsCertificateMandatory => false;
	}
}
