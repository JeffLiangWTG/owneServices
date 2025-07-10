using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CryptokiExternalPasswordLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCertificateAuthorityList()
	{
		var externalPassword = Factory.New<CryptokiExternalPassword>();
		var list = externalPassword.Lookups.CertificateAuthorityList;

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, list.Count);
			AssertEquals("Aruba code", CertificateAuthorityList.Codes.Aruba, list.GetCodeFromDescription("Aruba"));
			AssertEquals("InfoCert code", CertificateAuthorityList.Codes.Infocert, list.GetCodeFromDescription("InfoCert"));
		});
	}

	public void TestChipsetList()
	{
		var externalPassword = Factory.New<CryptokiExternalPassword>();
		var list = externalPassword.Lookups.ChipsetList;
		AssertEquals(1, list.Count);
		AssertEquals(ChipsetList.Descriptions.Bit4id, list.GetDescriptionFromCode("BIT4ID"));
	}
}
