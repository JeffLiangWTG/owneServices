using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(CryptokiExternalPasswordValidation))]
sealed class CryptokiExternalPasswordValidationTest : MasterFiles.Business.Testing.GlbExternalPasswordValidationTest<CryptokiExternalPassword, CryptokiExternalPasswordValidation>
{
	public void TestCheckGP_CertificateAuthorityMandatoryValidation()
	{
		const string expectedError = "Please enter a Certificate Authority or click 'Clear Certificate'.";

		GlbExternalPassword.GP_CertificateAuthority = "";
		AssertHasErrorContaining(GlbExternalPassword.GP_CertificateAuthorityInfo, expectedError);

		GlbExternalPassword.GP_CertificateAuthority = CertificateAuthorityList.Codes.Aruba;
		AssertNoErrorContaining(GlbExternalPassword.GP_CertificateAuthorityInfo, expectedError);
	}

	public void TestCheckGP_CertificateAuthorityListValidation()
	{
		GlbExternalPassword.GP_CertificateAuthority = "";
		AssertNoErrorContaining(GlbExternalPassword.GP_CertificateAuthorityInfo, ListValidation.InvalidCodeError);

		GlbExternalPassword.GP_CertificateAuthority = "NotListed";
		AssertHasErrorContaining(GlbExternalPassword.GP_CertificateAuthorityInfo, ListValidation.InvalidCodeError);

		GlbExternalPassword.GP_CertificateAuthority = CertificateAuthorityList.Codes.Aruba;
		AssertNoErrorContaining(GlbExternalPassword.GP_CertificateAuthorityInfo, ListValidation.InvalidCodeError);
	}

	public void TestCheckGP_NameMandatoryValidation()
	{
		const string expectedError = "Please enter a Chipset or click 'Clear Certificate'.";

		GlbExternalPassword.GP_Name = "";
		AssertHasErrorContaining(GlbExternalPassword.GP_NameInfo, expectedError);

		GlbExternalPassword.GP_Name = ChipsetList.Codes.Bit4id;
		AssertNoErrorContaining(GlbExternalPassword.GP_NameInfo, expectedError);
	}

	public void TestCheckGP_NameListValidation()
	{
		GlbExternalPassword.GP_Name = "";
		AssertNoErrorContaining(GlbExternalPassword.GP_NameInfo, ListValidation.InvalidCodeError);

		GlbExternalPassword.GP_Name = "InvalidCode";
		AssertHasErrorContaining(GlbExternalPassword.GP_NameInfo, ListValidation.InvalidCodeError);

		GlbExternalPassword.GP_Name = ChipsetList.Codes.Bit4id;
		AssertNoErrorContaining(GlbExternalPassword.GP_NameInfo, ListValidation.InvalidCodeError);
	}

	public void TestCheckGP_CertificateSerialNumberMandatoryValidation()
	{
		const string expectedError = "Please enter a Serial Number or click 'Clear Certificate'.";

		GlbExternalPassword.GP_CertificateSerialNumber = "";
		AssertHasErrorContaining(GlbExternalPassword.GP_CertificateSerialNumberInfo, expectedError);

		GlbExternalPassword.GP_CertificateSerialNumber = "abcdef";
		AssertNoErrorContaining(GlbExternalPassword.GP_CertificateSerialNumberInfo, expectedError);
	}
}
