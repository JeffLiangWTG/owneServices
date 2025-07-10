using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCGL_AdditionalIdentifier_Mandatory_AuthorisationMode()
	{
		CombineAssertions(() =>
		{
			cusGoodsLocation.CGL_AdditionalIdentifier = ZString.Empty;
			AssertHasMessageErrorContaining(cusGoodsLocation.CGL_AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);

			cusGoodsLocation.CGL_AdditionalIdentifier = "1234";
			AssertNoMessageErrorContaining(cusGoodsLocation.CGL_AdditionalIdentifierInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckCGL_AdditionalIdentifier_Comma_AuthorisationMode()
	{
		CombineAssertions(() =>
		{
			cusGoodsLocation.CGL_AdditionalIdentifier = "59,2";
			AssertHasMessageErrorContaining(cusGoodsLocation.CGL_AdditionalIdentifierInfo, "You cannot use comma's in the house number");

			cusGoodsLocation.CGL_AdditionalIdentifier = "1234";
			AssertNoMessageErrorContaining(cusGoodsLocation.CGL_AdditionalIdentifierInfo, "You cannot use comma's in the house number");
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		cusGoodsLocation = Factory.New<CusGoodsLocation>();
		cusGoodsLocation.CGL_ParentTableCode = CusGoodsLocationUseList.Codes.CustomsPermitRule;
	}

	CusGoodsLocation cusGoodsLocation;
}
