namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class AddInfoCusEntryHeaderLookupsTest : EU.Business.Declaration.Testing.AddInfoCusEntryHeaderLookupsTest
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

		public void TestClearanceResultCodeList()
		{
			var entry = Factory.New<CusEntryHeader>();
			var lookups = entry.AddInfoLookups.ClearanceResultCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(ClearanceResultCodeList), entry.AddInfoLookups.ClearanceResultCodeList.GetType());
				AssertEquals("A1, A2", lookups.CodesAsString);
				AssertEquals("[A1] Satisfactory", entry.AddInfoLookups.ClearanceResultCodeList.GetDescriptionFromCode("A1"));
			});
		}
	}
}
