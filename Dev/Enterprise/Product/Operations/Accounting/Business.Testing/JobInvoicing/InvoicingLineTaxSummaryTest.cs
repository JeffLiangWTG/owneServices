using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	[TestedType(typeof(InvoicingLineTaxSummary))]
	public class InvoicingLineTaxSummaryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEvaluateData()
		{
			var objectCreater = new TestObjectCreator(Factory);
			var invoicingLineTaxSummary = new InvoicingLineTaxSummary() {
				LocalExTaxAmount = 10,
				LocalTaxAmount = 2,
				LocalTotalAmount = 12,
				Message = "Test",
				TaxID = "Tax",
				TaxIDDescription = "Des",
				TaxRate = 20m
			};

			TaxSummaryTestHelper.AssertTaxSummaryResult(invoicingLineTaxSummary, "Tax", "Test", 10, 2, 12, "Des");

			invoicingLineTaxSummary.LocalExTaxAmount = 12;
			invoicingLineTaxSummary.LocalTaxAmount = 6;
			invoicingLineTaxSummary.LocalTotalAmount = 18;
			invoicingLineTaxSummary.Message = "Test2";
			invoicingLineTaxSummary.TaxID = "Tax2";
			invoicingLineTaxSummary.TaxIDDescription = "Des2";
			invoicingLineTaxSummary.TaxRate = 50m;

			TaxSummaryTestHelper.AssertTaxSummaryResult(invoicingLineTaxSummary, "Tax2", "Test2", 12, 6, 18, "Des2");
		}
	}
}
