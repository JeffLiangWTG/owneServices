using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS.Testing;

class DepartureGoodsItemWrapperTest : Customs.Business.Testing.DataProviderTestCase<DepartureGoodsItemWrapper>
{
	public void TestPackages()
	{
		CombineAssertions(() =>
		{
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_MarksAndNumbers = "PACK456";
			package1.B5_UnitType = "BG";
			package1.B5_UnitCount = 23;
			var package2 = goodsItem.Packages.AddNew();
			package2.B5_MarksAndNumbers = "PACK123";
			package2.B5_UnitType = "BX";
			package2.B5_UnitCount = 50;
			AssertContainsExactElementsInAnyOrder("When isVehicles is false (MarksAndNumbersOfPackages, KindOfPackages, NumberOfPackages)",
													new (ZString, ZString, ZLong)[]
													{
															("PACK456", "BG", 23),
															("PACK123", "BX", 50)
													}, wrapper.Packages.Select(x => (x.MarksAndNumbersOfPackages, x.KindOfPackages, x.NumberOfPackages)));

			goodsItem.IsVehicles = true;
			var vehicle1 = goodsItem.Packages.AddNew();
			vehicle1.B5_PackageID = "VINCODE1";
			vehicle1.B5_Brand = "BRAND1";
			vehicle1.B5_Model = "MODEL1";

			var vehicle2 = goodsItem.Packages.AddNew();
			vehicle2.B5_PackageID = ZString.Empty;
			vehicle2.B5_Brand = "BRAND2";
			vehicle2.B5_Model = "MODEL2";

			var vehicle3 = goodsItem.Packages.AddNew();
			vehicle3.B5_PackageID = "VINCODE3";
			vehicle3.B5_Brand = ZString.Empty;
			vehicle3.B5_Model = "MODEL3";

			var vehicle4 = goodsItem.Packages.AddNew();
			vehicle4.B5_PackageID = "VINCODE4";
			vehicle4.B5_Brand = "BRAND4";
			vehicle4.B5_Model = ZString.Empty;

			wrapper = new DepartureGoodsItemWrapper(goodsItem);
			AssertContainsExactElementsInAnyOrder("When isVehicles is false (MarksAndNumbersOfPackages, KindOfPackages, NumberOfPackages)",
													new (ZString, ZString, ZLong)[]
													{
															("VINCODE1:BRAND1:MODEL1", "FR", 1),
															(":BRAND2:MODEL2", "FR", 1),
															("VINCODE3::MODEL3", "FR", 1),
															("VINCODE4:BRAND4:", "FR", 1)
													}, wrapper.Packages.Select(x => (x.MarksAndNumbersOfPackages, x.KindOfPackages, x.NumberOfPackages)));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes.Departure);
		goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		wrapper = new DepartureGoodsItemWrapper(goodsItem);
	}
	NctsDepartureCargoDesc goodsItem;
	NctsHeader header;
	DepartureGoodsItemWrapper wrapper;

	protected override DepartureGoodsItemWrapper GetProvider()
	{
		return new DepartureGoodsItemWrapper(goodsItem);
	}
}
