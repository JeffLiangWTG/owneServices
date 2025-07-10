using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using ITGlbCompanyWrapper = Enterprise.Customs.IT.Business.GlbCompanyWrapper;
using ITGlbStaffWrapper = Enterprise.Customs.IT.Business.GlbStaffWrapper;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

static class CustomsCredentialAndCertificateTestHelper
{
	public static void SetUpRegistryAccountAndDeclarant(TemporaryStorageHeader header)
	{
		var factory = header.Factory;

		factory.New<OrgHeader>().OH_Code = "DEC1";
		factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
		.AppendAccount("11111111111-001", "1111", "00", "01")
			.AppendAccountDetail("1111-DEC1", "DEC1")
			.Build();

		var company = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var companyWrapper = ITGlbCompanyWrapper.Get(company);
		var mauPassword = companyWrapper.PasswordCollection.AddNew();
		mauPassword.GP_UserID = "1111-DEC1";
		factory.Save();

		header.AMA_CustomsProfile = "1111-DEC1";
	}

	public static CryptokiExternalPassword AddNewCryptoKiCertificateToCurrentUser(string certificateSerialNumber = null, string pin = null)
	{
		var staffWrapper = ITGlbStaffWrapper.Get(GlbStaff.CurrentUser);
		var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
		cryptokiCertificate.GP_Name = "BIT4ID";
		cryptokiCertificate.GP_CertificateSerialNumber = certificateSerialNumber ?? "0123456789";
		if (!string.IsNullOrEmpty(pin))
		{
			cryptokiCertificate.TokenPinStore.SetPin(pin);
		}

		return cryptokiCertificate;
	}
}
