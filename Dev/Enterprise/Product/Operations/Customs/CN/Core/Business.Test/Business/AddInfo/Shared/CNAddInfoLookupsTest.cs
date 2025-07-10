using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	abstract class CNAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			var parent = GetNewAddInfo();
			AssertEquals(parent.Lookups.Parent, parent);
		}

		protected abstract AddInfo GetNewAddInfo();
	}
}
