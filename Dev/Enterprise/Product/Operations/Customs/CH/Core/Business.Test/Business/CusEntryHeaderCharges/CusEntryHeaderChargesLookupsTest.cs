using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusEntryHeaderChargesLookups))]
sealed class CusEntryHeaderChargesLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestChargeTypeList()
	{
		RefCusCodeTestHelper.CreateChargeTypeCodeList(Factory);

		var list = Lookups.ChargeTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("Valid charge type code", true, list.ContainsCode(RefCusCodeTestHelper.ValidChargeTypeCode));
			AssertEquals("Valid VAT code", true, list.ContainsCode(Core.Constants.Customs.CusEntryFeeTypes.VAT));
			AssertEquals("Invalid charge type code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidChargeTypeCode));
		});
	}

	CusEntryHeaderChargesLookups Lookups => lookups ??= GetNewLookups();
	CusEntryHeaderChargesLookups lookups;

	CusEntryHeaderChargesLookups GetNewLookups()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var charge = entryHeader.Charges.AddNew();
		return charge.Lookups;
	}
}
