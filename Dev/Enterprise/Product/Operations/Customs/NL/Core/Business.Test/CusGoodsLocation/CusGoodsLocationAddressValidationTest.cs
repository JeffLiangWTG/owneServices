using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CusGoodsLocationAddressValidation))]
sealed class CusGoodsLocationAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckE2_Postcode_Mandatory_AuthorisationMode()
	{
		CombineAssertions(() =>
		{
			locationAddress.E2_Postcode = ZString.Empty;
			locationAddress.Validation.ValidateE2_Postcode();
			AssertHasMessageErrorContaining(locationAddress.E2_PostcodeInfo, MandatoryValidation.YouHaveNotEntered);

			locationAddress.E2_Postcode = "1234 AB";
			locationAddress.Validation.ValidateE2_Postcode();
			AssertNoMessageErrorContaining(locationAddress.E2_PostcodeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckE2_Postcode_WrongFormat_AuthorisationMode()
	{
		CombineAssertions(() =>
		{
			locationAddress.E2_Postcode = "2940";
			locationAddress.Validation.ValidateE2_Postcode();
			AssertHasWarningContaining(locationAddress.E2_PostcodeInfo, "The entered postcode does not comply with the postcode format rules of the country");

			locationAddress.E2_Postcode = "1234 AB";
			locationAddress.Validation.ValidateE2_Postcode();
			AssertNoWarningContaining(locationAddress.E2_PostcodeInfo, "The entered postcode does not comply with the postcode format rules of the country");
		});
	}

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
