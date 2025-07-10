using Enterprise.Build.Database.Script.Public.Accounting.Balances;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Test
{
	public class AccountingBalancesTestForTG_AccTransactionLines_InsertToAccOrgBalanceChanges : AccOrgBalancesTestCase
	{
		public void TestRecognizedTotalForReceivablesTransactions()
		{
			TestRecognizedTotal("AR");
		}

		public void TestRecognizedTotalForPayablesTransactions()
		{
			TestRecognizedTotal("AP");
		}
	}
}
