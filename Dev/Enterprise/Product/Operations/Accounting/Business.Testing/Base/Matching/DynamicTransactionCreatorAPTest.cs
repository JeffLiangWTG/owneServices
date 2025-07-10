using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(DynamicTransactionCreatorAP))]
	public class DynamicTransactionCreatorAPTest : DynamicTransactionCreatorSingleLedgerTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DynamicTransactionCreatorAP(new OrganizationSubBalanceCollection(Factory),
				ZGuid.Empty, Factory);
		}

		protected DynamicTransactionCreatorAP TestAPTransactionCreator;

		#region TestCreateTransactions

		public void TestCreateTransactions()
		{
			OrganizationSubBalanceCollection testSubBalances = new OrganizationSubBalanceCollection(Factory);

			OrganizationSubBalance subBalance1 = new OrganizationSubBalance(Org1.PK, 50M, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			OrganizationSubBalance subBalance2 = new OrganizationSubBalance(Org2.PK, 40M, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			OrganizationSubBalance subBalance3 = new OrganizationSubBalance(Org3.PK, -30M, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			OrganizationSubBalance primaryOrgSubBalance = new OrganizationSubBalance(TestPrimaryOrg.PK, -60M, ZArchitecture.Core.LedgerTypes.AccountsPayable);

			testSubBalances.Add(subBalance1);
			testSubBalances.Add(subBalance2);
			testSubBalances.Add(subBalance3);
			testSubBalances.Add(primaryOrgSubBalance);

			TestAPTransactionCreator = new DynamicTransactionCreatorAP(testSubBalances, TestPrimaryOrg.PK, Factory);

			IMatchingCollection testTransfers = TestAPTransactionCreator.CreateTransactions();

			TransferFilter org1Filter = new TransferFilter();
			org1Filter.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			org1Filter.TransferFromOrg = Org1.PK;
			org1Filter.TransferFromAmount = -50M;
			org1Filter.TransferToOrg = TestPrimaryOrg.PK;
			org1Filter.TransferToAmount = 50M;
			AssertNotNull(testTransfers.GetMatchingTransfer(org1Filter));

			TransferFilter org2Filter = new TransferFilter();
			org2Filter.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			org2Filter.TransferFromOrg = Org2.PK;
			org2Filter.TransferFromAmount = -40M;
			org2Filter.TransferToOrg = TestPrimaryOrg.PK;
			org2Filter.TransferToAmount = 40M;
			AssertNotNull(testTransfers.GetMatchingTransfer(org2Filter));

			TransferFilter org3Filter = new TransferFilter();
			org3Filter.TransferLedger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			org3Filter.TransferFromOrg = Org3.PK;
			org3Filter.TransferFromAmount = 30M;
			org3Filter.TransferToOrg = TestPrimaryOrg.PK;
			org3Filter.TransferToAmount = -30M;
			AssertNotNull(testTransfers.GetMatchingTransfer(org3Filter));
		}

		#endregion
	}
}
