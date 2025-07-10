using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class EInvoicingAuthorizationBehaviourProviderTest : TestCaseWithFactory
	{
		public void TestResetAuthorisationRecordWhenResetPivotStatusToQueued()
		{
			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001001", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var pivot1 = testObjectCreator.CreateEInvoicingTransactionPivot(invoice1);
			var authRecord1 = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authRecord1.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			authRecord1.AHF_ParentId = invoice1.PK;

			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001002", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var pivot2 = testObjectCreator.CreateEInvoicingTransactionPivot(invoice2);
			var authRecord2 = Factory.NewWithValidTestData<AccTransactionHeaderAuthorisationRecord>();
			authRecord2.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			authRecord2.AHF_ParentId = invoice2.PK;

			Factory.Save();

			CreateProviderForTest().ResetAuthorisationRecordWhenResetPivotStatusToQueued(pivot1);

			AssertEquals("Pivot relative Authorisation record should be deleted.", true, authRecord1.IsDeleted);
			AssertEquals("Non pivot relative Authorisation record should NOT be deleted.", false, authRecord2.IsDeleted);
		}

		protected virtual IEInvoicingAuthorizationBehaviourProvider CreateProviderForTest() => new EInvoicingAuthorizationBehaviourProvider();

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
