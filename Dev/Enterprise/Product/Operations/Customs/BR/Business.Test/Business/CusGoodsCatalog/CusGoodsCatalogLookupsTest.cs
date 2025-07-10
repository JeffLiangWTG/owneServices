using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusGoodsCatalogLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusTypeList()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();

			var lookups = goodsCatalog.Lookups;
			AssertEquals(3, lookups.StatusTypeList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "0", "1", "2" }, lookups.StatusTypeList.GetAllCodes());
		}

		public void TestMessageStatusList()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();

			var lookups = goodsCatalog.Lookups;

			AssertEquals(5, lookups.MessageStatusList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "ACC", "AWA", "", "REJ", "FAL" }, lookups.MessageStatusList.GetAllCodes());
		}
	}
}
