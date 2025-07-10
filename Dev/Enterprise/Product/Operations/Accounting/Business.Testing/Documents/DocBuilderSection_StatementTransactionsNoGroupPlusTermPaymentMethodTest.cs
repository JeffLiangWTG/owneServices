using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderSection_StatementTransactionsNoGroupPlusTermPaymentMethodTest : TestCaseWithFactory
	{
		public void TestStatementTransactionsWith_GroupPlusTermPaymentMethodSectionContains_Description_Macros()
		{
			DocBuilderSectionStatementOfAccountDocStripsTestHelper.TestSectionNameContains_DescriptionWithGovernmentAllocationNumber_Macros(Factory, "Statement Transactions No Group (+ Term Payment Method)", DocBuilderSectionStatementOfAccountDocStripsTestHelper.DescriptionContentForStatement);
		}
	}
}
