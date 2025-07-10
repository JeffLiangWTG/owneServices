using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusContainerLookups))]
sealed class CusContainerLookupsTest : Customs.Business.Testing.CusContainerLookupsTest
{
	protected override Customs.Business.CusContainerLookups GetCusContainerLookups() => new CusContainerLookups(Factory.New<CusContainer>());

	public void TestSealTypeList()
	{
		var container = Factory.New<CusContainer>();
		var cachedList = Factory.GetCachedValue<SealTypeList>();
		var lookedUpList = container.Lookups.SealTypeList;
		CombineAssertions(() =>
		{
			AssertSame("SealTypeList is cached", cachedList, lookedUpList);
			AssertEquals("SealTypeList values", "BOT, ESE, RFI", lookedUpList.CodesAsString);
		});
	}
}
