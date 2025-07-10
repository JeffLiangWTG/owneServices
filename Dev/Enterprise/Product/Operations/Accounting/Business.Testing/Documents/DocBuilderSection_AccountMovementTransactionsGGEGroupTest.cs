using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderSection_AccountMovementTransactionsGGEGroupTest : TestCaseWithFactory
	{
		public void TestAccountMovementTransactionsGGEGroupSectionContains_DescriptionWithGovernmentAllocationNumber_Macros()
		{
			DocBuilderSectionStatementOfAccountDocStripsTestHelper.TestSectionNameContains_DescriptionWithGovernmentAllocationNumber_Macros(Factory, "Account Movement Transactions GGE Group", DocBuilderSectionStatementOfAccountDocStripsTestHelper.DescriptionContentForAccountMovement);
		}
	}
}
