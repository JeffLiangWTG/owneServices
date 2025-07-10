using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class CusEUEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEADPrintProcedureCodeList()
		{
			var entry = Factory.New<CusEUEntryHeader>();
			var lookups = entry.Lookups.EADPrintProcedureCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(EADPrintProcedureCodeList), entry.Lookups.EADPrintProcedureCodeList.GetType());
				AssertEquals("0, 1, 2", lookups.CodesAsString);
				AssertEquals("[0] No EAD Print", entry.Lookups.EADPrintProcedureCodeList.GetDescriptionFromCode("0"));
			});
		}
	}
}
