using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsCargoDescFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeTypeListNotThrowing()
		{
			AssertNoExceptionThrown(() => Factory.New<NctsCargoDescFee>().Lookups.ChargeTypeList.GetAllCodes());
		}

		public void TestChargeTypeList()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var line = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var fee = line.Fees.AddNew();
			var lookups = fee.Lookups;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "DTY", "ADD", "CVD", "VAT", "EXC" }, lookups.ChargeTypeList.GetAllCodes());

			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader2.SetMovementType(NctsMovementType.Codes.Arrival);
			var line2 = nctsHeader2.ArrivalMovementHeader.GoodsItems.AddNew();
			var fee2 = line2.Fees.AddNew();
			var lookups2 = fee2.Lookups;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "DTY", "ADD", "CVD" }, lookups2.ChargeTypeList.GetAllCodes());
		}
	}
}
