using System;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class LocationOfGoodsProviderTest : Customs.Business.Testing.DataProviderTestCase<LocationOfGoodsProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new LocationOfGoodsProvider(null));
	}

	public void TestTypeOfLocation()
	{
		AssertEquals("T", Provider.TypeOfLocation);
	}

	public void TestQualifierOfIdentification()
	{
		AssertEquals("Z", Provider.QualifierOfIdentification);
	}

	public void TestAuthorisationNumber()
	{
		var provider = GetProvider();
		goodsLocation.CGL_Qualifier = "Y";
		goodsLocation.CGL_AdditionalIdentifier = "authNumber";
		AssertEquals("authNumber", provider.AuthorisationNumber);
	}

	public void TestAuthorisationNumber_Ignore()
	{
		AssertNullOrEmpty(Provider.AuthorisationNumber);
	}

	public void TestAdditionalIdentifier()
	{
		AssertEquals("AdditionalID", Provider.AdditionalIdentifier);
	}

	public void TestAdditionalIdentifier_Ignore()
	{
		var provider = GetProvider();
		goodsLocation.CGL_Qualifier = "Y";
		goodsLocation.CGL_AdditionalIdentifier = "notempty";
		AssertNullOrEmpty(provider.AdditionalIdentifier);
	}

	public void TestUNLocode()
	{
		var provider = GetProvider();
		goodsLocation.CGL_Qualifier = "U";
		goodsLocation.CGL_AdditionalIdentifier = "UN";
		AssertEquals("UN", provider.UNLocode);
	}

	public void TestUNLocode_Ignore()
	{
		AssertNullOrEmpty(Provider.UNLocode);
	}

	public void TestCustomsOfficeReferenceNumber()
	{
		var provider = GetProvider();
		goodsLocation.CGL_Qualifier = "V";
		goodsLocation.CGL_CustomsOffice = "refNumber";
		AssertEquals("refNumber", provider.CustomsOfficeReferenceNumber);
	}

	public void TestCustomsOfficeReferenceNumber_Ignore()
	{
		AssertNullOrEmpty(Provider.CustomsOfficeReferenceNumber);
	}

	public void TestGNSSLatitute()
	{
		var provider = GetProvider();
		goodsLocation.CGL_Qualifier = "W";
		goodsLocation.Address.E2_Latitude = 10;
		AssertEquals("10", provider.GNSSLatitute);
	}

	public void TestGNSSLongitude()
	{
		var provider = GetProvider();
		goodsLocation.CGL_Qualifier = "W";
		goodsLocation.Address.E2_Longitude = 5;
		AssertEquals("5", provider.GNSSLongitude);
	}

	public void TestGNSSLatitute_Ignore()
	{
		AssertNull(Provider.GNSSLatitute);
	}

	public void TestGNSSLongitude_Ignore()
	{
		AssertNull(Provider.GNSSLongitude);
	}

	public void TestEconomicOperatorIdentificationNumber()
	{
		var provider = GetProvider();
		goodsLocation.CGL_Qualifier = "X";
		goodsLocation.Address.E2_GovRegNum = "econ";
		AssertEquals("econ", provider.EconomicOperatorIdentificationNumber);
	}

	public void TestEconomicOperatorIdentificationNumber_Ignore()
	{
		AssertNullOrEmpty(Provider.EconomicOperatorIdentificationNumber);
	}

	public void TestAddress()
	{
		AssertNotNull(Provider.Address);
	}

	public void TestAddress_Ignore()
	{
		var provider = GetProvider();
		goodsLocation.CGL_Qualifier = "Y";
		AssertNull(provider.Address);
	}

	public void TestPostCodeAddress()
	{
		var provider = GetProvider();
		goodsLocation.CGL_Qualifier = "T";
		AssertNotNull(provider.PostCodeAddress);
	}

	public void TestPostCodeAddress_Ignore()
	{
		AssertNull(Provider.PostCodeAddress);
	}

	public void TestContactPerson()
	{
		AssertNotNull(Provider.ContactPerson);
	}

	public void TestContactPerson_Ignore()
	{
		var provider = GetProvider();
		goodsLocation.CGL_Qualifier = "Y";
		AssertNull(provider.ContactPerson);
	}

	protected override LocationOfGoodsProvider GetProvider()
	{
		goodsLocation = Factory.New<CusGoodsLocation>();
		goodsLocation.CGL_Type = "T";
		goodsLocation.CGL_Qualifier = "Z";
		goodsLocation.CGL_AdditionalIdentifier = "AdditionalID";
		goodsLocation.Address.E2_Latitude = 10;
		goodsLocation.Address.E2_Longitude = 5;
		goodsLocation.Address.E2_GovRegNum = "econ";
		goodsLocation.Address.E2_Email = "test@email.com";
		goodsLocation.Address.E2_Phone = "1234567890";
		goodsLocation.Address.E2_Contact = "test";
		return new LocationOfGoodsProvider(goodsLocation);
	}
	CusGoodsLocation goodsLocation;
}
