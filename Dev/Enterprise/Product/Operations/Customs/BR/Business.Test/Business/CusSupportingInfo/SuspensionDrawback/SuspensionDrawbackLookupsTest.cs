using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class SuspensionDrawbackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeSuspensionDrawbackList()
		{
			var parent = Factory.New<SuspensionDrawback>();
			var list = parent.Lookups.TypeSuspensionDrawbackList;
			AssertEquals(8, list.Count);
		}
	}
}
