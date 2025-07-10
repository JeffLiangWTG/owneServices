using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderSection_StatementTransactionsWithNoGroupTest : TestCaseWithFactory
	{
		public void TestStatementTransactionsWithNoGroupSectionContains_DescriptionWithGovernmentAllocationNumber_Macros()
		{
			DocBuilderSectionStatementOfAccountDocStripsTestHelper.TestSectionNameContains_DescriptionWithGovernmentAllocationNumber_Macros(Factory, "Statement Transactions No Group", DocBuilderSectionStatementOfAccountDocStripsTestHelper.DescriptionContentForStatement);
		}
	}
}
