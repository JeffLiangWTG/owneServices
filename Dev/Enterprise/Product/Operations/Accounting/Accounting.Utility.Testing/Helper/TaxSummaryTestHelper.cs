using System.Reflection;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Utility.Testing
{
	public static class TaxSummaryTestHelper
	{
		public static void AssertTaxSummaryResult(InvoicingLineTaxSummary invoicingLineTaxSummary, ZString taxID, ZString message, ZDecimal localExTaxAmount, ZDecimal localTaxAmount, ZDecimal localTotalAmount, ZString taxIDDescription)
		{
			var taxRate = localExTaxAmount == 0M ? 0M : localTaxAmount / localExTaxAmount * 100;

			Assertion.AssertEquals("Tax ID shoule be equal", taxID, invoicingLineTaxSummary.TaxID);
			Assertion.AssertEquals("Tax Message shoule be equal", message, invoicingLineTaxSummary.Message);
			Assertion.AssertEquals("Ex Tax Amount should be equal", localExTaxAmount, invoicingLineTaxSummary.LocalExTaxAmount);
			Assertion.AssertEquals("Tax Amount should be equal", localTaxAmount, invoicingLineTaxSummary.LocalTaxAmount);
			Assertion.AssertEquals("Total Amount should be equal", localTotalAmount, invoicingLineTaxSummary.LocalTotalAmount);
			Assertion.AssertEquals("Tax Rate shoule be equal", taxRate, invoicingLineTaxSummary.TaxRate);
			Assertion.AssertEquals("Tax ID Description shoule be equal", taxIDDescription, invoicingLineTaxSummary.TaxIDDescription);
		}

		public static void AssertInvoiceLineTaxSummariesBeNull(InvoicingBase invoicingBase)
		{
			AssertInvoiceLineTaxSummariesCore(invoicingBase, true, 0);
		}

		public static void AssertInvoiceLineTaxSummariesCount(InvoicingBase invoicingBase, int expectCount)
		{
			AssertInvoiceLineTaxSummariesCore(invoicingBase, false, expectCount);
		}

		static void AssertInvoiceLineTaxSummariesCore(InvoicingBase invoicingBase, bool isNull, int expectCount)
		{
			var fieldTaxSummaries = typeof(InvoicingBase).GetField("fInvoiceLineTaxSummaries", BindingFlags.NonPublic | BindingFlags.Instance);

			Assertion.Assert("field fInvoiceLineTaxSummaries should exist", fieldTaxSummaries != null);

			var fInvoiceLineTaxSummaries = (InvoicingLineTaxSummaryCollection)fieldTaxSummaries.GetValue(invoicingBase);

			if (isNull)
			{
				Assertion.AssertNull("fInvoiceLineTaxSummaries should be null", fInvoiceLineTaxSummaries);
			}
			else
			{
				Assertion.AssertNotNull("fInvoiceLineTaxSummaries should not be null", fInvoiceLineTaxSummaries);
				Assertion.AssertEquals("field fInvoiceLineTaxSummaries should contain", expectCount, fInvoiceLineTaxSummaries.Count);
			}
		}
	}
}
