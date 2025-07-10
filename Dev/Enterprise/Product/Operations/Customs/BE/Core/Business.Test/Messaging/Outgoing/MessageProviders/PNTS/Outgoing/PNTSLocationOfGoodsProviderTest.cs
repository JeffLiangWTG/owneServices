using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSLocationOfGoodsProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSLocationOfGoodsProvider>
{
	public void TestTypeOfLocation()
	{
		goodsLocation.CGL_Type = "T";
		AssertEquals("T", provider.TypeOfLocation);
	}

	public void TestQualifierOfIdentification()
	{
		goodsLocation.CGL_Qualifier = "Q";
		AssertEquals("Q", provider.QualifierOfIdentification);
	}

	public void TestUnLoCode() => CombineAssertions(() =>
	{
		goodsLocation.CGL_AdditionalIdentifier = "AI";
		AssertEquals("Qualifier not set", "", provider.UnLoCode);
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
		AssertEquals("Only Qualifer set", "", provider.UnLoCode);
		goodsLocation.CGL_AdditionalIdentifier = "AI";
		AssertEquals("Qualifier set", "AI", provider.UnLoCode);
	});

	public void TestAuthorisationNumber() => CombineAssertions(() =>
	{
		goodsLocation.Address.E2_GovRegNum = "GRN";
		AssertEquals("Qualifier not set", "", provider.AuthorisationNumber);
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		AssertEquals("Only Qualifer set", "", provider.AuthorisationNumber);
		goodsLocation.Address.E2_GovRegNum = "GRN";
		AssertEquals("Qualifier set", "GRN", provider.AuthorisationNumber);
	});

	public void TestAdditionalIdentifier() => CombineAssertions(() =>
	{
		goodsLocation.CGL_AdditionalIdentifier = "AI";
		AssertEquals("Qualifier not set", "", provider.AdditionalIdentifier);
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		AssertEquals("Only Qualifer set", "", provider.AdditionalIdentifier);
		goodsLocation.CGL_AdditionalIdentifier = "AI";
		AssertEquals("Qualifier set", "AI", provider.AdditionalIdentifier);
	});

	public void TestCustomsOfficeReferenceNumber() => CombineAssertions(() =>
	{
		goodsLocation.CGL_CustomsOffice = "CO";
		AssertEquals("Qualifier not set", "", provider.CustomsOfficeReferenceNumber);
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
		AssertEquals("Qualifier set", "CO", provider.CustomsOfficeReferenceNumber);
	});

	public void TestGNSSLongitude() => CombineAssertions(() =>
	{
		var geoLocation = new CargoWise.Types.ZGeography("20,10");
		goodsLocation.Address.E2_GeoLocation = geoLocation;
		AssertEquals("Qualifier not set", "", provider.GNSSLongitude);
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
		AssertEquals("Only Qualifer set", "0", provider.GNSSLongitude);
		goodsLocation.Address.E2_GeoLocation = geoLocation;
		AssertEquals("Qualifier set", "20", provider.GNSSLongitude);
	});

	public void TestGNSSLatitude() => CombineAssertions(() =>
	{
		var geoLocation = new CargoWise.Types.ZGeography("20,10");
		goodsLocation.Address.E2_GeoLocation = geoLocation;
		AssertEquals("Qualifier not set", "", provider.GNSSLatitude);
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
		AssertEquals("Only Qualifer set", "0", provider.GNSSLatitude);
		goodsLocation.Address.E2_GeoLocation = geoLocation;
		AssertEquals("Qualifier set", "10", provider.GNSSLatitude);
	});

	public void TestEconomicOperatorIdentificationNumber() => CombineAssertions(() =>
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_RL_NKClosestPort = "BEANR";
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "007", orgHeader.CountryCode);
		goodsLocation.Address.IdentificationHolderPK = orgHeader.PK;
		AssertEquals("Qualifier not set", "", provider.EconomicOperatorIdentificationNumber);
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
		AssertEquals("Qualifier set(1)", "", provider.EconomicOperatorIdentificationNumber);
		goodsLocation.Address.IdentificationHolderPK = orgHeader.PK;
		AssertEquals("Qualifier set(2)", "BE007", provider.EconomicOperatorIdentificationNumber);
		goodsLocation.Address.E2_GovRegNum = "007";
		AssertEquals("Qualifier set(3)", "BE007", provider.EconomicOperatorIdentificationNumber);
		goodsLocation.Address.E2_GovRegNum = "NL007";
		AssertEquals("Qualifier set(4)", "NL007", provider.EconomicOperatorIdentificationNumber);
		goodsLocation.Address.E2_GovRegNum = "666";
		AssertEquals("Qualifier set(5)", "BE666", provider.EconomicOperatorIdentificationNumber);
	});

	public void TestAddress()
	{
		AssertNull(provider.Address);
	}

	protected override PNTSLocationOfGoodsProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		goodsLocation = Factory.New<CusGoodsLocation>();
		provider = new PNTSLocationOfGoodsProvider(goodsLocation);
	}

	CusGoodsLocation goodsLocation;
	PNTSLocationOfGoodsProvider provider;
}
