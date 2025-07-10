using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(TSCustomsNumberViewStmNumsLookups))]
sealed class TSCustomsNumberViewStmNumsLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTypeList()
	{
		var lookups = new TSCustomsNumberViewStmNumsLookups(Factory.New<CustomsNumberViewStmNums>());
		AssertSame(Factory.GetCachedValue<NumberRangeTypeList>(), lookups.TypeList);
	}
}
