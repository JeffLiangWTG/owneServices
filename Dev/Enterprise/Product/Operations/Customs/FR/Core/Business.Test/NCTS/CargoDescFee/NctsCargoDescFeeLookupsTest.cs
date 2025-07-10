using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class NctsCargoDescFeeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestChargeTypeListNotThrowing()
		{
			AssertNoExceptionThrown(() => Factory.New<NctsCargoDescFee>().Lookups.ChargeTypeList.GetAllCodes());
		}

		public void TestChargeTypeList()
		{
			AssertType<CodeDescriptionPairList>(lookups.ChargeTypeList);
			AssertContainsExactElementsInAnyOrder(new ZString[] { HarbourFeeCodes.Codes.V905, "DTY", "ADD", "CVD", "VAT", "EXC" }, lookups.ChargeTypeList.GetAllCodes());

			var nctsHeader2 = Factory.New<NctsHeader>();
			nctsHeader2.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader2.SetMovementType(NctsMovementType.Codes.Arrival);
			var line2 = nctsHeader2.ArrivalMovementHeader.GoodsItems.AddNew();
			var fee2 = line2.Fees.AddNew();
			var lookups2 = fee2.Lookups;
			AssertContainsExactElementsInAnyOrder(new ZString[] { HarbourFeeCodes.Codes.V905, "DTY", "ADD", "CVD" }, lookups2.ChargeTypeList.GetAllCodes());
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var line = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var fee = line.Fees.AddNew();
			lookups = fee.Lookups;
		}

		public void TestRateOverrideReasonCodeList()
		{
			var fee = Factory.New<NctsCargoDescFee>();
			AssertType<EU.Business.RateOverrideReasonList>(fee.Lookups.RateOverrideReasonCodeList);
		}

		NctsHeader nctsHeader;
		NctsCargoDescFeeLookups lookups;
	}
}
