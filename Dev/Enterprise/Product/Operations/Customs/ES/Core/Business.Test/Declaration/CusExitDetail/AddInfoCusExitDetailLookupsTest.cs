using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class AddInfoCusExitDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCircuitcodeList()
		{
			var entry = Factory.New<CusEntryHeader>();
			var lookups = entry.AddInfoLookups.CircuitCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(CircuitCodeList), entry.AddInfoLookups.CircuitCodeList.GetType());
				AssertEquals("4, 6, 5, 7", lookups.CodesAsString);
				AssertEquals("RED", entry.AddInfoLookups.CircuitCodeList.GetDescriptionFromCode("5"));
			});
		}
	}
}
