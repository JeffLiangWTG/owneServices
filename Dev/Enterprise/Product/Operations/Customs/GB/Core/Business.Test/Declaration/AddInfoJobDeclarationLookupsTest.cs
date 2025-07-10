using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Test.Declaration
{
	public class AddInfoJobDeclarationLookupsTest : TestCaseWithFactory
	{
		public void TestLookupLists()
		{
			var dec = GetDeclaration();
			Assert(dec.AddInfoLookups.GatewayList.ContainsCode("CCSUK"));

			Assert(dec.Lookups.GbNIMode.ContainsCode("NII"));
		}

		public void TestImportClearanceStatusICSList()
		{
			var dec = GetDeclaration();
			AssertEquals(23, dec.AddInfoLookups.ImportClearanceStatusICSList.Count);
		}

		public void TestRouteOfEntryList()
		{
			var dec = GetDeclaration();
			AssertEquals(11, dec.AddInfoLookups.RouteOfEntryList.Count);
		}

		public void TestSpecificCircumstanceIndicatorList()
		{
			var dec = GetDeclaration();
			AssertEquals(1, dec.AddInfoLookups.SpecificCircumstanceIndicatorList.Count);
			AssertEquals("A", dec.AddInfoLookups.SpecificCircumstanceIndicatorList[0].Code);
		}

		public void TestGatewayList()
		{
			var dec = GetDeclaration();
			AssertEquals(6, dec.AddInfoLookups.GatewayList.Count);
		}

		JobDeclaration GetDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			return dec;
		}
	}
}
