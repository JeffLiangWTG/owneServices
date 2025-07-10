using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CusGoodsLocationAddress))]
sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
{
	public void TestValidation()
	{
		AssertType<CusGoodsLocationAddressValidation>(locationAddress.Validation);
	}

	public void TestCusGoodsLocation()
	{
		AssertType<CusGoodsLocation>(locationAddress.GoodsLocation);
	}

	public void TestE2_RN_NKCountryCodeReadonly()
	{
		AssertEquals("Country Code should be read only in authorisation mode", true, locationAddress.E2_RN_NKCountryCodeInfo.ReadOnly);
	}

	public override bool EnableAllowSpatialTypesAttributeTest => false;

	protected override BusinessObject GetNewBusinessObject() => locationAddress;

	protected override void SetUp()
	{
		base.SetUp();
		var cusAuthorisationRule = Factory.New<CusAuthorisationRule>();
		cusGoodsLocation = (CusGoodsLocation)cusAuthorisationRule.GoodsLocation;
		locationAddress = cusGoodsLocation.Address;
	}

	CusGoodsLocation cusGoodsLocation;
	CusGoodsLocationAddress locationAddress;
}
