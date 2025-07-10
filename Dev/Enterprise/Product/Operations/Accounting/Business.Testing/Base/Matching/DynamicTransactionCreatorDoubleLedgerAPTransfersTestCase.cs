using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(DynamicTransactionCreatorPrimaryLedgerAP))]
	public class DynamicTransactionCreatorDoubleLedgerAPTransfersTestCase : DynamicTransactionCreatorDoubleLedgerTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DynamicTransactionCreatorPrimaryLedgerAP(null, null, ZGuid.Empty, Factory);
		}

		#endregion

		public void TestCreateTransactions()
		{
			OrganizationSubBalance aRSubBalance = new OrganizationSubBalance(Org1.PK, 54.32M, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			OrganizationSubBalance aPSubBalance = new OrganizationSubBalance(Org2.PK, 54.32M, ZArchitecture.Core.LedgerTypes.AccountsPayable);

			OrganizationSubBalanceCollection testARSubBalances = new OrganizationSubBalanceCollection(Factory);
			testARSubBalances.Add(aRSubBalance);

			OrganizationSubBalanceCollection testAPSubBalances = new OrganizationSubBalanceCollection(Factory);
			testAPSubBalances.Add(aPSubBalance);

			TestDoubleLedgerTransCreator = new DynamicTransactionCreatorPrimaryLedgerAP(testARSubBalances, testAPSubBalances, Org2.PK, Factory);

			IMatchingCollection dynamicTransactions = TestDoubleLedgerTransCreator.CreateTransactions();

			AssertEquals("One Contra should be created", 1, dynamicTransactions.Count);
		}
	}
}
