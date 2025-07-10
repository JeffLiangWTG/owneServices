using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	public class InwardProcessingProductLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSubTypeList()
		{
			var list = lookups.SubTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "B, P, T", list.CodesAsString);
				AssertSame("Cached", list, lookups.SubTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new InwardProcessingProductLookups(Factory.New<InwardProcessingProduct>());
		}
		InwardProcessingProductLookups lookups;
	}
}
