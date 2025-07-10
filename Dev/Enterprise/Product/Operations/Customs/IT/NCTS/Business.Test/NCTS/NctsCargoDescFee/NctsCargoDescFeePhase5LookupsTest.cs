using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsCargoDescFeePhase5LookupsTest : BusinessObjectLookupsTestCase
{
	public void TestChargeTypeList()
	{
		AssertEquals("CodesAsString", "", fee.Lookups.ChargeTypeList.CodesAsString);
	}

	public void TestRateOverrideReasonList()
	{
		AssertEquals("CodesAsString", "", fee.ITLookups.RateOverrideReasonList.CodesAsString);
	}

	public void TestMethodOfPaymentList()
	{
		AssertEquals("CodesAsString", "", fee.ITLookups.MethodOfPaymentList.CodesAsString);
	}

	public void TestMethodOfCalculationList()
	{
		AssertEquals("CodesAsString", "", fee.ITLookups.MethodOfCalculationList.CodesAsString);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		fee = goodsItem.Fees.AddNew();
	}

	NctsCargoDescFee fee;
}
