using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(OrganizationSubBalance))]
	public class OrganizationSubBalanceTestCase : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrganizationSubBalance();
		}

		public void TestProperties()
		{
			OrganizationSubBalance testOrgSubBal = new OrganizationSubBalance(ZGuid.NewZGuid(), 10M, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			AssertEquals("Amount should be set correctly", 10M, testOrgSubBal.Amount);
			AssertEquals("Ledger should be set correctly", ZArchitecture.Core.LedgerTypes.AccountsPayable, testOrgSubBal.Ledger);
		}
	}
}
