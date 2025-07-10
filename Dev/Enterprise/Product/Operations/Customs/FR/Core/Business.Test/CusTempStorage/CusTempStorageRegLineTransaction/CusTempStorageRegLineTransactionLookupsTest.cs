using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	class CusTempStorageRegLineTransactionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReferenceTypeList()
		{
			var lookups = new CusTempStorageRegLineTransactionLookups(Factory.New<CusTempStorageRegLineTransaction>());
			var list = lookups.ReferenceTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "DEC, IST, ADJ, MRN", list.CodesAsString);
				AssertSame("Cached", list, lookups.ReferenceTypeList);
			});
		}
	}
}
