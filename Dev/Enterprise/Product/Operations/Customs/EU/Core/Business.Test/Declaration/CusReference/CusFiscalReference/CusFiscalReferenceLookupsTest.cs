using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class CusFiscalReferenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFiscalReferenceCodeList()
		{
			var cusFiscalReference = Factory.New<CusFiscalReference>();
			var lookups = new CusFiscalReferenceLookups(cusFiscalReference);
			var codeList = lookups.CodeList;
			AssertEquals("FiscalReferenceCodeList", "FR1, FR2, FR3, FR4, FR5", codeList.CodesAsString);
			AssertSame("Cached", Factory.GetCachedValue<FiscalReferenceCodeList>(), lookups.CodeList);
		}
	}
}
