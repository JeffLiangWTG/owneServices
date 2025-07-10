using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusFiscalReferenceLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestFiscalReferenceCodeList()
	{
		CombineAssertions(() =>
		{
			AssertType<FiscalReferenceCodeList>("CodeList Type", lookups.CodeList);
			AssertSame("Cached", Factory.GetCachedValue<FiscalReferenceCodeList>(), lookups.CodeList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var cusFiscalReference = Factory.New<CusFiscalReference>();
		lookups = new CusFiscalReferenceLookups(cusFiscalReference);
	}

	CusFiscalReferenceLookups lookups;
}
