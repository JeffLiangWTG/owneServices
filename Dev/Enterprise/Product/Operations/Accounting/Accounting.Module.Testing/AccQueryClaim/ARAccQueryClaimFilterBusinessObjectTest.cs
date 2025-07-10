using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARAccQueryClaimFilterBusinessObject))]
	class ARAccQueryClaimFilterBusinessObjectTest : AccQueryClaimFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ARAccQueryClaimFilterBusinessObject();
		}

		protected override Invoice CreateNewInvoice(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<ARInvoice>();
		}

		protected override Invoice CreateNewInvoiceOfOtherLedger(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<APInvoice>();
		}

		protected override AccQueryClaim CreateNewAccQueryClaim(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<ARAccQueryClaim>();
		}

		public void TestLedger()
		{
			ARAccQueryClaimFilterBusinessObject filter = new ARAccQueryClaimFilterBusinessObject();
			AssertEquals("Ledger", LedgerTypes.AccountsReceivable, filter.Ledger);
		}
	}
}
