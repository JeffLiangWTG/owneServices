using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	public abstract class KRAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			var parent = GetNewAddInfo();
			AssertEquals(parent.Lookups.Parent, parent);
		}

		protected abstract KRAddInfo GetNewAddInfo();
	}
}

