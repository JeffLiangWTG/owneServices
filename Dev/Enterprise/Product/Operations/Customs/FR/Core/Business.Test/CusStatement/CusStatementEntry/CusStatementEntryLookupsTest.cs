using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	class CusStatementEntryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			var cusStatementEntryLookups = Factory.New<CusStatementEntry>().Lookups;
			AssertEquals("EntryType code list lookup content should match ClosingDeclarationDirectionList code pair description list", Factory.GetCachedValue<StatementEntryTypeList>().CodesAsString, cusStatementEntryLookups.EntryTypeList.CodesAsString);
		}
	}
}
