using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class FiscalReferenceLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList()
	{
		var list = fiscalReferenceLookups.CodeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Correct list", new[] { "FR1", "FR2", "FR3", "FR4", "FR5", "FR7" }, ((CodeDescriptionPairList)list).GetAllCodes());
			AssertSame("Cached", Factory.GetCachedValue<FiscalReferenceCodeList>(), list);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		fiscalReferenceLookups = new FiscalReferenceLookups(Factory.New<FiscalReference>());
	}
	FiscalReferenceLookups fiscalReferenceLookups;
}

