using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderSection_StatementTransactionsWith_GroupPlusTermPaymentMethodTest : TestCaseWithFactory
	{
		public void TestStatementTransactionsWithGroupPlusTermPaymentMethodSectionContains_Description_Macros()
		{
			DocBuilderSectionStatementOfAccountDocStripsTestHelper.TestSectionNameContains_DescriptionWithGovernmentAllocationNumber_Macros(Factory, "Statement Transactions With Group (+ Term Payment Method)", DocBuilderSectionStatementOfAccountDocStripsTestHelper.DescriptionContentForStatement);
		}
	}
}
