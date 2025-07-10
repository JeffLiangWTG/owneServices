using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class RomaniaEInvoicingAuthorizationBehaviourProviderTest : EInvoicingAuthorizationBehaviourProviderTest
	{
		public void TestResetAuthorisationRecordWhenResetPivotStatusToQueued_HeaderProperties()
		{
			var testDate = ZDate.Today;

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001001", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var pivot1 = testObjectCreator.CreateEInvoicingTransactionPivot(invoice1);
			invoice1.AH_GovernmentAllocatedID = "TEST01";
			invoice1.AH_ComplianceDocumentDate = testDate;

			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001002", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var pivot2 = testObjectCreator.CreateEInvoicingTransactionPivot(invoice2);
			invoice2.AH_GovernmentAllocatedID = "TEST02";
			invoice2.AH_ComplianceDocumentDate = testDate.AddDays(-1);

			CreateProviderForTest().ResetAuthorisationRecordWhenResetPivotStatusToQueued(pivot1);

			AssertEquals("Pivot relative AR Invocie should have Authorisation data reset.", ZString.Empty, invoice1.AH_GovernmentAllocatedID);
			AssertEquals(ZDate.Empty, invoice1.AH_ComplianceDocumentDate);

			AssertEquals("Non pivot relative AR Invocie should have Authorisation data unchanged.", "TEST02", invoice2.AH_GovernmentAllocatedID);
			AssertEquals(testDate.AddDays(-1), invoice2.AH_ComplianceDocumentDate);
		}

		protected override IEInvoicingAuthorizationBehaviourProvider CreateProviderForTest() => new RomaniaEInvoicingAuthorizationBehaviourProvider();

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
