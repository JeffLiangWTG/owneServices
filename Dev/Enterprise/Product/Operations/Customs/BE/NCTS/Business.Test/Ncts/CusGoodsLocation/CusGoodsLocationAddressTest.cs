using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(CusGoodsLocationAddress))]
sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
{
	public void TestCusGoodsLocation()
	{
		AssertType<CusGoodsLocation>(locationAddress.GoodsLocation);
	}

	protected override BusinessObject GetNewBusinessObject() => locationAddress;

	protected override void SetUp()
	{
		base.SetUp();
		var cusGoodsLocation = Factory.New<CusGoodsLocation>();
		cusGoodsLocation.CGL_LocationUse = "ARR";
		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var movementHeader = nctsHeader.ArrivalMovementHeader;
		cusGoodsLocation.Parent = movementHeader;
		locationAddress = cusGoodsLocation.Address;
	}

	CusGoodsLocationAddress locationAddress;
}
