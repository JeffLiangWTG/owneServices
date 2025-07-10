using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderSection_AccountMovementTransactionNoGroupTest : TestCaseWithFactory
	{
		public void TestAccountMovementTransactionNoGroupSectionContains_DescriptionWithGovernmentAllocationNumber_Macros()
		{
			DocBuilderSectionStatementOfAccountDocStripsTestHelper.TestSectionNameContains_DescriptionWithGovernmentAllocationNumber_Macros(Factory, "Account Movement Transactions No Group",DocBuilderSectionStatementOfAccountDocStripsTestHelper.DescriptionContentForAccountMovement);
		}
	}
}
