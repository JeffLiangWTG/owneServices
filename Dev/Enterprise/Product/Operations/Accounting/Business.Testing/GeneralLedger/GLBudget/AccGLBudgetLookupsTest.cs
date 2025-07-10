using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget.Testing
{
	public class AccGLBudgetLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBalanceSheetAndPLFilter()
		{
			GLBudget gLBudget = Factory.New<GLBudget>();
			AccGLBudgetLookups testGLBudgetLookUps = new AccGLBudgetLookups(gLBudget);

			AccGLHeader[] gLHeaders = Factory.Load<AccGLHeader>(testGLBudgetLookUps.BalanceSheetAndPLFilter_ForTestOnly);
			int originalLength = gLHeaders.Length;
			gLBudget.Delete();

			AccGLHeader balanceSheetGL = Factory.NewWithValidTestData<AccGLHeader>();
			balanceSheetGL.AG_AccountType = AccountTypesList.Codes.BalanceSheet;

			AccGLHeader profitLossGL = Factory.NewWithValidTestData<AccGLHeader>();
			profitLossGL.AG_AccountType = AccountTypesList.Codes.ProfitLoss;

			AccGLHeader nonBSGL = Factory.NewWithValidTestData<AccGLHeader>();
			nonBSGL.AG_AccountType = AccountTypesList.Codes.Alternate;

			Factory.Save();

			gLHeaders = Factory.Load<AccGLHeader>(testGLBudgetLookUps.BalanceSheetAndPLFilter_ForTestOnly);
			AssertEquals(2, gLHeaders.Length - originalLength);
		}
	}
}
