using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class CusGoodsLocationWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsPhase5CusGoodsLocationWrapper(goodsLocation: null));
		AssertNoExceptionThrown(() => new NctsPhase5CusGoodsLocationWrapper(goodsLocation));
	}

	public void TestAddress()
	{
		AssertType<CusGoodsLocationAddress>(goodsLocationWrapper.Address);
		AssertSame("Address", goodsLocation.Address, goodsLocationWrapper.Address);
	}

	public void TestGetCustomOfficeCodes()
	{
		var customsOfficeCodes = goodsLocationWrapper.GetCustomOfficeCodes();
		AssertNotNull(customsOfficeCodes);
		AssertEquals("When CustomsOffice is empty", 0, customsOfficeCodes.Count);

		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.CustomsOffices.AddNew("DEP", "IT12343");
		movementHeader.CustomsOffices.AddNew("DEP", "DE43222");
		movementHeader.CustomsOffices.AddNew("TRA", "TR12332");

		customsOfficeCodes = goodsLocationWrapper.GetCustomOfficeCodes();
		AssertNotNull(customsOfficeCodes);
		AssertEquals("When CustomsOffice with Purpose DEP is present", 2, customsOfficeCodes.Count);
		AssertContainsExactElementsInAnyOrder("When Customs Offices with Purpose DEP is present", new[] { "IT12343", "DE43222" }, customsOfficeCodes);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		goodsLocation = nctsHeader.MovementHeader.GoodsLocation;
		goodsLocationWrapper = new NctsPhase5CusGoodsLocationWrapper(goodsLocation);
	}

	NctsHeader nctsHeader;
	CusGoodsLocation goodsLocation;
	ICusGoodsLocationWrapper goodsLocationWrapper;
}
