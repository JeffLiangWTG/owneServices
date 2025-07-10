using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class GlbCertificateProviderTest : TestCaseWithFactory
{
	public void TestGetCryptokiCertificate()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var currentStaff = GlbStaff.CurrentUser;
			var staffWrapper = GlbStaffWrapper.Get(currentStaff);
			var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
			cryptokiCertificate.GP_Name = ChipsetList.Codes.Bit4id;
			Factory.Save();

			var certificateProvider = new GlbCertificateProvider();
			var certificate = certificateProvider.GetCryptokiCertificate();

			AssertNotNull("CryptokiCertificate", certificate);
			AssertEquals(nameof(certificate.PK), cryptokiCertificate.PK, certificate.PK);
		}
	}

	public void TestGetCryptokiCertificateThrowsExceptionWhenNotConfiguredForCurrentUser()
	{
		var certificateProvider = new GlbCertificateProvider();

		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			AssertExceptionThrown<InvalidOperationException>("Certificate Not Configured",
				expectedExceptionMessage: "Cannot get the Cryptoki certificate for the current user",
				codeToRun: () => certificateProvider.GetCryptokiCertificate());
		}
	}

	public void TestGetMauCertificate()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			SetUpMauCertificateForCompany(Factory.NewWithValidTestData<GlbCompany>());
			var mauPassword = SetUpMauCertificateForCompany(GlbCompany.CurrentCompany);

			var certificateProvider = new GlbCertificateProvider();
			var mauCertificatePassword = certificateProvider.GetMauCertificatePassword("1234");

			AssertNotNull("MauCertificate", mauCertificatePassword);
			AssertEquals(nameof(mauCertificatePassword.PK), mauPassword.PK, mauCertificatePassword.PK);
		}
	}

	public void TestHasValidAutomaticSignaturePassword()
	{
		var certificateProvider = new GlbCertificateProvider();
		AssertEquals("When there are no automaticSignature, HasValidAutomaticSignaturePassword", false, certificateProvider.HasValidAutomaticSignaturePassword);

		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var currentStaff = GlbStaff.CurrentUser;
			var staffWrapper = GlbStaffWrapper.Get(currentStaff);
			var automaticSignature = staffWrapper.AutomaticSignaturePasswordCollection.AddNew();

			certificateProvider = new GlbCertificateProvider();

			automaticSignature.IsConfigurationActive = true;
			AssertEquals("When IsConfigurationActive is true, HasValidAutomaticSignaturePassword", true, certificateProvider.HasValidAutomaticSignaturePassword);

			automaticSignature.IsConfigurationActive = false;
			AssertEquals("When IsConfigurationActive is false, HasValidAutomaticSignaturePassword", false, certificateProvider.HasValidAutomaticSignaturePassword);
		}
	}

	public void TestAutomaticSignaturePassword()
	{
		var certificateProvider = new GlbCertificateProvider();
		AssertNull("AutomaticSignaturePassword", certificateProvider.AutomaticSignaturePassword);

		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var currentStaff = GlbStaff.CurrentUser;
			var staffWrapper = GlbStaffWrapper.Get(currentStaff);
			var automaticSignature = staffWrapper.AutomaticSignaturePasswordCollection.AddNew();
			certificateProvider = new GlbCertificateProvider();
			var automaticSignaturePassword = certificateProvider.AutomaticSignaturePassword;
			AssertNotNull("AutomaticSignaturePassword", automaticSignaturePassword);
			AssertEquals(nameof(automaticSignaturePassword.PK), automaticSignature.PK, automaticSignaturePassword.PK);
		}
	}

	GlbMauExternalPassword SetUpMauCertificateForCompany(GlbCompany company)
	{
		var factory = company.Factory;
		var companyWrapper = GlbCompanyWrapper.Get(company);
		var mauPassword = companyWrapper.PasswordCollection.AddNew();
		mauPassword.GP_UserID = "1234";
		factory.Save();
		return mauPassword;
	}
}
