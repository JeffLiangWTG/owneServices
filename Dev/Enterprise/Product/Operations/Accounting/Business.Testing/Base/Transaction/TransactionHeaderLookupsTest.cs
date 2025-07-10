using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class TransactionHeaderLookupsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestHeadersLookup()
		{
			TransactionHeader parent = Factory.New<ARAP.Journal.APJournal>();
			TransactionHeaderLookups lookups = new TransactionHeaderLookups(parent);

			parent.AH_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals("For " + parent.AH_Ledger + " should be DebtorsList", lookups.DebtorsList, lookups.Headers);

			foreach (string ledger in new string[] { LedgerTypes.AccountsPayable, LedgerTypes.UnapprovedPayableTransactions, LedgerTypes.TransactionsPendingAllocation, LedgerTypes.IncompleteTransactions })
			{
				parent.AH_Ledger = ledger;
				AssertEquals("For " + parent.AH_Ledger + " should be CreditorsList", lookups.CreditorsList, lookups.Headers);
			}

			foreach (string ledger in new string[] { LedgerTypes.CashBook, LedgerTypes.General, LedgerTypes.JobCosting })
			{
				parent.AH_Ledger = ledger;
				Assert("For " + parent.AH_Ledger + " should be DebtorOrCreditorCollection", lookups.Headers is DebtorOrCreditorCollection);
			}
		}
	}
}