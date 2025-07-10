using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderSection_StatementTransactionsWithGroupTest : TestCaseWithFactory
	{
		public void TestStatementTransactionsWithGroupSectionContains_DescriptionWithGovernmentAllocationNumber_Macros()
		{
			DocBuilderSectionStatementOfAccountDocStripsTestHelper.TestSectionNameContains_DescriptionWithGovernmentAllocationNumber_Macros(Factory, "Statement Transactions With Group", DocBuilderSectionStatementOfAccountDocStripsTestHelper.DescriptionContentForStatement);
		}
	}
}
