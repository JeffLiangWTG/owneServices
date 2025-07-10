namespace Enterprise.Customs.CH.Business.Testing;

public class CusPermitHeaderLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
{
	public void TestUnitOfQuantityList()
	{
		RefCusCodeTestHelper.CreateCustomsUnitOfQuantityCodeList(Factory);

		var list = lookups.UnitOfQuantityList;
		CombineAssertions(() =>
		{
			AssertEquals("valid customs quantity code", true, list.ContainsCode(RefCusCodeTestHelper.ValidCustomsUnitOfQuantityCode));
			AssertEquals("invalid customs quantity code", false, list.ContainsCode(RefCusCodeTestHelper.InvalidCustomsUnitOfQuantityCode));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.NewWithValidTestData<CusPermitHeader>();
		lookups = declaration.Lookups;
	}
	CusPermitHeaderLookups lookups;
}
