using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SWConstituentLookups))]
sealed class SWConstituentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestStatusList()
	{
		var statusList = Factory.New<JobComInvoiceLine>().SWConstituents.AddNew().Lookups.StatusList;
		AssertSame(Factory.GetCachedValue<Universal.CodeDescriptionPairLists.YesNoList>(), statusList);
	}
}
