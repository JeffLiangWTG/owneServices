using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	public class MessageHostedServiceRequirementTest : TestCaseWithFactory
	{
		public void TestCheckARCompanyHasCertificate_True()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes.Argentina;

			var credential = Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(company).GlbExternalPassword;
			credential.GP_UserID = "ADMINVUCEM13";
			credential.GP_CurrentPassword = "9974567890";

			credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_PasswordType = PasswordTypesList.Codes.ARB;

			GlbCompany.CurrentCompany.Factory.Save();
			Factory.Save();
			CertificateRequirementChecker.ResetForTesting();

			AssertEquals(ZString.Empty, MessageHostedServiceRequirement.CheckARCompanyHasCertificate());
		}

		public void TestCheckARCompanyHasCertificate_False()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes.Argentina;

			Factory.Save();

			AssertEquals("There is no Valid Certificate in Argentina.", MessageHostedServiceRequirement.CheckARCompanyHasCertificate());
		}
	}
}
