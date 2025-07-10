using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.KR.Business.Testing
{
	public class CertificateTestHelper : TestCaseWithFactory
	{
		public GlbCompanyCredential GetGlbExternalPassword(string fileName, string passStr)
		{
			var password = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password.GP_Name = "readykorea";
			password.GP_PasswordType = "KRB";
			password.GP_UserID = "ABC";
			password.GP_GC = Env.CurrentCompany.PK;
			password.GP_IssueDate = new ZDateTime(2022, 1, 1);
			password.GP_ExpiryDate = ZDateTime.MaxSmallDateTime;
			password.GP_Certificate = new TestFileReader(typeof(CertificateTestHelper)).GetEmbeddedFileData(TestFilesPath, fileName);
			password.CurrentDecryptedCertificatePassphrase = passStr;
			Factory.Save();
			return password;
		}

		public static void SetCustomsPublicKey()
		{
			var embeddedData = new TestFileReader(typeof(CertificateTestHelper)).GetEmbeddedFileData(CertificateTestHelper.TestFilesPath, "x509_Server.der");
			KRCustomsRegistry.Instance.CustomsCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, embeddedData);
		}

		public static string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Certificate";
	}
}
