using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderSection_AccountMovementTransactionsGGBGroupTest : TestCaseWithFactory
	{
		public void TestAccountMovementTransactionsGGBGroupSectionContains_DescriptionWithGovernmentAllocationNumber_Macros()
		{
			DocBuilderSectionStatementOfAccountDocStripsTestHelper.TestSectionNameContains_DescriptionWithGovernmentAllocationNumber_Macros(Factory, "Account Movement Transactions GGB Group", DocBuilderSectionStatementOfAccountDocStripsTestHelper.DescriptionContentForAccountMovement);
		}
	}
}
