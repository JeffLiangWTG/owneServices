using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class FijiEInvoicingPivotStatusProviderTest : EInvoicingPivotStatusProviderTestBase<FijiEInvoicingPivotStatusProvider>
	{
		protected override string CountryCode => CountryCodes.Fiji;

		public override void TestCanCreateNotEligibleForEInvoicingPivot()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
				AssertNotEligiblePivotCreation(arInvoice, true, true);
				AssertNotEligiblePivotCreation(arInvoice, false, false);

				var apInvoice = Factory.NewWithValidTestData<APInvoice>();
				AssertNotEligiblePivotCreation(apInvoice, true, false);
			}

			void AssertNotEligiblePivotCreation(AccTransactionHeader transaction, bool eInvoicingIsEnabled, bool expectedEligibility)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, eInvoicingIsEnabled))
				{
					AssertCanCreateNotEligibleForEInvoicingPivot(transaction, expectedEligibility);
				}
			}
		}

		public void TestGetInitialPivotStatus_ReturnsDefaultStatus_WhenTransactionIsEligible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, 1m, 100m, 10m, 0m, taxRate: TestObjectCreator.GSTFREE1);
				TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, 1m, 200m, 20m, 0m, taxRate: TestObjectCreator.IntegratedGST);

				AssertEquals("Precondition: Invoice is Eligible for EInvoicing", true, invoice.EInvoicingProxy.IsEligibleToCreatePivot());
				AssertInitialPivotStatusEquals(invoice, string.Empty, "Initial pivot status will be returned as empty.");

				var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
				TestObjectCreator.CreateInvoiceLine(arCreditNote, arCreditNote.TransactionCurrency, 1m, 100m, 10m, 0m, taxRate: TestObjectCreator.GSTFREE1);
				TestObjectCreator.CreateInvoiceLine(arCreditNote, arCreditNote.TransactionCurrency, 1m, 200m, 20m, 0m, taxRate: TestObjectCreator.IntegratedGST);

				AssertEquals("Precondition: transaction is Eligible for EInvoicing", true, arCreditNote.EInvoicingProxy.IsEligibleToCreatePivot());
				AssertInitialPivotStatusEquals(arCreditNote, string.Empty, "Initial pivot status for eligible transaction will be returned as empty.");
			}
		}
	}
}
