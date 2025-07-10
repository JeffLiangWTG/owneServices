namespace Enterprise.Customs.FR.Business.CusStatement.Testing
{
	class CusStatementEntryValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCheckB3_EntryType()
		{
			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_BranchDesignation = StatementEntryTypeList.Codes.Import;

			var entry = statementHeader.Entries.AddNew();
			AssertEquals(StatementEntryTypeList.Codes.Import, entry.B3_EntryType);
			AssertNoMessageError(entry.B3_EntryTypeInfo, CusStatementEntryValidation.InconsistentEntryType);

			entry.B3_EntryType = StatementEntryTypeList.Codes.Export;
			AssertHasMessageError(entry.B3_EntryTypeInfo, CusStatementEntryValidation.InconsistentEntryType);
		}
	}
}
