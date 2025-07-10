using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class CusLPCOHeaderLookupTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList()
		{
			var lpcoHeader = Factory.New<CusLPCOHeader>();

			var lookups = lpcoHeader.Lookups;
			AssertEquals(5, lookups.MessageStatusList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "ACC", "AWA", "FAL", "REJ", "" }, lookups.MessageStatusList.GetAllCodes());
		}
	}
}
