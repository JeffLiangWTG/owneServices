using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCGL_CustomsOfficeEmpty_HasCN0394CodeInErrorMessage()
	{
		cusGoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
		cusGoodsLocation.CGL_CustomsOffice = string.Empty;

		AssertHasMessageErrorContaining("C0394 rule error, should have CN0394 for IT", cusGoodsLocation.CGL_CustomsOfficeInfo, IT.Business.ValidationRuleCodeConstants.CN0394);
	}

	protected override void SetUp()
	{
		base.SetUp();
		cusGoodsLocation = Factory.New<CusGoodsLocation>();
		cusGoodsLocation.CGL_LocationUse = "ARR";
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
		cusGoodsLocation.Parent = arrivalMovementHeader;
	}

	CusGoodsLocation cusGoodsLocation;
	NctsArrivalMovementHeader arrivalMovementHeader;
}
