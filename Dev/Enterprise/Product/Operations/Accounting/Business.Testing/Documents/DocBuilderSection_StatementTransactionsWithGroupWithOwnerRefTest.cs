using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderSection_StatementTransactionsWithGroupWithOwnerRefTest : TestCaseWithFactory
	{
		public void TestStatementTransactionsWithGroupWithOwnerRefSectionContains_DescriptionWithGovernmentAllocationNumber_Macros()
		{
			DocBuilderSectionStatementOfAccountDocStripsTestHelper.TestSectionNameContains_DescriptionWithGovernmentAllocationNumber_Macros(Factory, "Statement Transactions With Group (Owner Ref)", DocBuilderSectionStatementOfAccountDocStripsTestHelper.DescriptionContentForStatement);
		}
	}
}
