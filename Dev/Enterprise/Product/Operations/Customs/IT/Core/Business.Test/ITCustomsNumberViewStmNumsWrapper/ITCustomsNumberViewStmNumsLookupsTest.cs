using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITCustomsNumberViewStmNumsLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
{
	public void TestTypeList()
	{
		AssertEquals(true, object.ReferenceEquals(Factory.GetCachedValue<NumberRangeTypeList>(), new ITCustomsNumberViewStmNumsLookups(Factory.New<CustomsNumberViewStmNums>()).TypeList));
	}
}

