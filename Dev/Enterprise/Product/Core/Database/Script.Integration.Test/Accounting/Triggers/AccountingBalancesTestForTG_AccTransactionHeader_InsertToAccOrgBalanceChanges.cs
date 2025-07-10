using Enterprise.Build.Database.Script.Public.Accounting.Balances;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Test
{
	public class AccountingBalancesTestForTG_AccTransactionHeader_InsertToAccOrgBalanceChanges : AccOrgBalancesTestCase
	{
		public void TestBalanceTotalForReceivablesTransactions()
		{
			TestBalanceTotal("AR");
		}

		public void TestBalanceTotalForPayablesTransactions()
		{
			TestBalanceTotal("AP");
		}
	}
}

