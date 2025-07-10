using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using GlbCompanyWrapper = Enterprise.Customs.IE.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IE.ServiceTasks.Testing
{
	class ServiceTaskHelperTest : TestCaseWithFactory
	{
		public void TestGetCertificateMessageError_Standard()
		{
			AssertGetCertificateMessageError(PasswordTypesList.Codes.IER, () => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(PrepareCompanyAndBranch()).GlbExternalPassword, ServiceTaskHelper.GetCertificateMessageError, "There is no Certificate configured in Ireland.");
		}

		public void TestGetCertificateMessageError_EMCS()
		{
			AssertGetCertificateMessageError(PasswordTypesList.Codes.IEM, () => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(PrepareCompanyAndBranch()).EMCSGlbExternalPasswordCollection.AddNew(), ServiceTaskHelper.GetCertificateMessageError, "There is no Certificate configured in Ireland.");
		}

		public void TestGetEMCSCertificateMessageError()
		{
			AssertGetCertificateMessageError(PasswordTypesList.Codes.IEM, () => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(PrepareCompanyAndBranch()).EMCSGlbExternalPasswordCollection.AddNew(), ServiceTaskHelper.GetEMCSCertificateMessageError, "There is no valid EMCS Certificate configured in Ireland.");
		}

		public void AssertGetCertificateMessageError(string passwordType, Func<GlbExternalPasswordWithCertificate> createPassword, Func<string> getCertificateMessageError, string errorText)
		{
			CertificateRequirementChecker.ResetForTesting();
			AssertEquals("No Certificate", errorText, getCertificateMessageError());

			var credential = createPassword();
			credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			Factory.Save();

			CertificateRequirementChecker.ResetForTesting();
			AssertEquals("No Valid Certificate", errorText, getCertificateMessageError());

			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			CertificateRequirementChecker.ResetForTesting();
			AssertEquals($"Has {passwordType} Certificate", string.Empty, getCertificateMessageError());
		}

		GlbCompany PrepareCompanyAndBranch()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			AssertEquals("Pre-condition, current company is set for Ireland", Core.Constants.CountryCodes.Ireland, company.GC_RN_NKCountryCode);
			Assert("Pre-condition, current company has active branch", company.HasActiveBranch);
			return company;
		}
	}
}
