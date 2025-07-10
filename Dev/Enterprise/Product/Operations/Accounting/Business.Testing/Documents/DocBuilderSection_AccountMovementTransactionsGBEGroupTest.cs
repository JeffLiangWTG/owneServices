using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderSection_AccountMovementTransactionsGBEGroupTest : TestCaseWithFactory
	{
		public void TestAccountMovementTransactionsGBEGroupSectionContains_DescriptionWithGovernmentAllocationNumber_Macros()
		{
			DocBuilderSectionStatementOfAccountDocStripsTestHelper.TestSectionNameContains_DescriptionWithGovernmentAllocationNumber_Macros(Factory, "Account Movement Transactions GBE Group", DocBuilderSectionStatementOfAccountDocStripsTestHelper.DescriptionContentForAccountMovement);
		}
	}
}
