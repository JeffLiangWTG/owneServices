using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusStatementHeaderLookupTest : BusinessObjectLookupsTestCase
	{
		public void TestImportersList()
		{
			var header = Factory.New<CusStatementHeader>();
			var lookups = new CusStatementHeaderLookups(header);
			AssertEquals(typeof(ConsigneeCollection), lookups.ImportersList.GetType());
		}

		public void TestBillTypeList()
		{
			var header = Factory.New<CusStatementHeader>();
			var billTypeList = header.Lookups.BillTypeList;
			CombineAssertions(() =>
			{
				AssertEquals(6, billTypeList.Count);
				AssertEquals("Z, O, A, B, W, U", billTypeList.CodesAsString);
			});
		}
	}
}
