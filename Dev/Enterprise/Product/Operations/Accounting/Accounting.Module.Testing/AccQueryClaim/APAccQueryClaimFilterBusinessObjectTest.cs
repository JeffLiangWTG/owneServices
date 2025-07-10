using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APAccQueryClaimFilterBusinessObject))]
	class APAccQueryClaimFilterBusinessObjectTest : AccQueryClaimFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new APAccQueryClaimFilterBusinessObject();
		}

		protected override Invoice CreateNewInvoice(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<APInvoice>();
		}

		protected override Invoice CreateNewInvoiceOfOtherLedger(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<ARInvoice>();
		}

		protected override AccQueryClaim CreateNewAccQueryClaim(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<APAccQueryClaim>();
		}

		public void TestLedger()
		{
			APAccQueryClaimFilterBusinessObject filter = new APAccQueryClaimFilterBusinessObject();
			AssertEquals("Ledger", LedgerTypes.AccountsPayable, filter.Ledger);
		}
	}
}
