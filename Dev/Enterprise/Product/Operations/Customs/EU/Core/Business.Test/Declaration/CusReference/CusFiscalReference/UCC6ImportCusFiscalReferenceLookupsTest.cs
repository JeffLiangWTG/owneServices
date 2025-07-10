using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class UCC6ImportCusFiscalReferenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFiscalReferenceCodeList()
		{
			var cusFiscalReference = Factory.New<CusFiscalReference>();
			var lookups = new UCC6ImportCusFiscalReferenceLookups(cusFiscalReference);
			var codeList = lookups.CodeList;
			AssertEquals("FiscalReferenceCodeList", "FR1, FR2, FR3, FR4, FR5, FR7", codeList.CodesAsString);
			AssertSame("Cached", Factory.GetCachedValue<UCC6IMPFiscalReferenceCodeList>(), codeList);
		}
	}
}
