using System;
using CargoWise.Common;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class SaudiArabiaEInvoicingPivotStatusProviderTest : EInvoicingPivotStatusProviderTestBase<SaudiArabiaEInvoicingPivotStatusProvider>
	{
		protected override string CountryCode => CountryCodes.SaudiArabia;

		public void TestGetInitialPivotStatus_ManyLines_VaryTaxTypes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var notReportableRate = Factory.NewWithValidTestData<AccTaxRate>();
				notReportableRate.AT_Type = AccTaxRate.Types.NotReportable;
				var invoice = Factory.NewWithValidTestData<APInvoice>();
				TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, 1.0m, 100m, 10m, 0m, taxRate: TestObjectCreator.GSTFREE1);
				TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.IntegratedGST);

				AssertInitialPivotStatusEquals(invoice, EInvoicingPivotState.Queued, "The status should be 'Queued' when invoice contains Taxable Lines.");

				TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: notReportableRate);

				AssertInitialPivotStatusEquals(invoice, EInvoicingPivotState.Queued, "The status should be 'Queued' when invoice contains Taxable Lines.");

				invoice.Lines.ForEach(l => ((InvoicingLineBase)l).AL_AT = notReportableRate.PK);

				AssertInitialPivotStatusEquals(invoice, EInvoicingPivotState.Discarded, "The status should be 'Discarded' when all lines are non-taxable.");
			}
		}

		public void TestGetInitialPivotStatus_WithNoLines()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var invoice = Factory.NewWithValidTestData<APInvoice>();
				AssertEquals("Precondition: Invoice contains no lines", 0, invoice.Lines.Count);

				AssertInitialPivotStatusEquals(invoice, EInvoicingPivotState.Discarded);
			}
		}
	}
}
