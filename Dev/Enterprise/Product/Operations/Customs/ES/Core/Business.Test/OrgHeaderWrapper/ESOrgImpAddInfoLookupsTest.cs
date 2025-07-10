using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ESOrgImpAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMethodOfPaymentList()
		{
			var testObj = ESOrgImpAddInfo.Get(Factory.New<OrgHeader>());
			var testList = testObj.Lookups.MethodOfPaymentList;
			AssertEquals(typeof(MethodOfPaymentList), testList.GetType());
			Assert(testList.ContainsCode("A"));
			AssertEquals(4, testList.Count);
			AssertSame("Should be cached", testList, testObj.Lookups.MethodOfPaymentList);
		}
	}
}
