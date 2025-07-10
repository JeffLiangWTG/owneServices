using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.H7.Business.Testing
{
	sealed class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQualifierList()
		{
			var location = Factory.New<CusGoodsLocation>();
			var codeList = location.Lookups.QualifierList;

			CombineAssertions(() =>
			{
				AssertEquals("There is only one element in the list", 1, codeList.Count);
				AssertEquals("Qualifier Code", "Z", codeList[0].Code);
				AssertEquals("Qualifier Description", "Address", codeList[0].Description);
			});
		}
	}
}
