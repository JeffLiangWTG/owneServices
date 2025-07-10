using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

class CusEntryLineFeeLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestChargeTypeList()
	{
		RefCusCodeTestHelper.CreateChargeTypeCodeList(Factory);

		var list = lookups.ChargeTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("Valid charge type code", true, list.ContainsCode(RefCusCodeTestHelper.ValidChargeTypeCode));
			AssertEquals("Valid VAT code", true, list.ContainsCode(Core.Constants.Customs.CusEntryFeeTypes.VAT));
			AssertEquals("Invalid charge type code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidChargeTypeCode));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.NewWithValidTestData<CusEntryLineFee>();
		lookups = declaration.Lookups;
	}
	CusEntryLineFeeLookups lookups;
}
