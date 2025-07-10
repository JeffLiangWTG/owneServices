using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	sealed class InvoiceHeaderCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
		public void TestRecalculatePageNumberWhenDeleteItem()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceDisplaySequence = 1;
			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.CA_PageNumber = 1;

			var line2 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line2.CA_PageNumber = 2;

			JobComInvoiceHeader invoice2 = jobDeclaration.Invoices.AddNew();
			invoice2.JZ_InvoiceDisplaySequence = 2;
			var line3 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			line3.CA_PageNumber = 3;
			var line4 = invoice2.InvoiceLines.AddNew() as JobComInvoiceLine;
			line4.CA_PageNumber = 3;

			JobComInvoiceHeader invoice3 = jobDeclaration.Invoices.AddNew();
			invoice3.JZ_InvoiceDisplaySequence = 3;
			var line5 = invoice3.InvoiceLines.AddNew() as JobComInvoiceLine;
			line5.CA_PageNumber = 4;
			var line6 = invoice3.InvoiceLines.AddNew() as JobComInvoiceLine;
			line6.CA_PageNumber = 5;

			jobDeclaration.Invoices.Delete(invoice2);

			AssertEquals(1, line1.CA_PageNumber);
			AssertEquals(2, line2.CA_PageNumber);
			AssertEquals(3, line5.CA_PageNumber);
			AssertEquals(4, line6.CA_PageNumber);

			jobDeclaration.Invoices.Delete(invoice);
			AssertEquals(1, line5.CA_PageNumber);
			AssertEquals(2, line6.CA_PageNumber);
		}

		public void TestUpdateExchangeRateOnExportDateChanged()
		{
			var today = ZDateTime.Today;
			var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			SetExchangeRate(today.AddDays(-10), today.AddDays(-8), 0.70m, uSDCurrency);
			SetExchangeRate(today.AddDays(-7), today.AddDays(-5), 0.69m, uSDCurrency);

			var testDec = JobDeclaration.New(Factory);
			testDec.JE_ExportDate = today.AddDays(-9);

			var invoice = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;

			AssertEquals("PreCondition:Exchange rate", 0.70m, invoice.JZ_InvoiceCurrLandedCostExRate);
			invoice.JZ_ValuationDateOverride = today.AddDays(-6);
			AssertEquals("Exchange rate updated", 0.69m, invoice.JZ_InvoiceCurrLandedCostExRate);
		}

		public void TestSuspendRecalculatePageNumbers()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var headerCollection = jobDeclaration.Invoices;
			var header1 = headerCollection.AddNew();
			var line1 = header1.JobComInvoiceLines.AddNew();
			var header2 = headerCollection.AddNew();
			var line2 = header2.JobComInvoiceLines.AddNew();

			using (headerCollection.SuspendRecalculatePageNumbers())
			{
				using (headerCollection.SuspendRecalculatePageNumbers())
				{
					Assert(headerCollection.IsRecalculatePageNumbersSuspended);

					line1.CA_PageNumber = 10;
					line2.CA_PageNumber = 9;

					AssertEquals("line1 page number:", 10, line1.CA_PageNumber);
					AssertEquals("line2 page number", 9, line2.CA_PageNumber);
				}

				Assert("Should still be suspended", headerCollection.IsRecalculatePageNumbersSuspended);
			}

			Assert("Not suspended", !headerCollection.IsRecalculatePageNumbersSuspended);
		}

		public void TestRecalculateAllPageNumbers()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var headerCollection = jobDeclaration.Invoices;
			var header1 = headerCollection.AddNew();
			var line1 = header1.JobComInvoiceLines.AddNew();
			var header2 = headerCollection.AddNew();
			var line2 = header2.JobComInvoiceLines.AddNew();

			using (headerCollection.SuspendRecalculatePageNumbers())
			{
				Assert(headerCollection.IsRecalculatePageNumbersSuspended);

				line1.CA_PageNumber = 10;
				line2.CA_PageNumber = 9;

				AssertEquals("Precondition: line1", 10, line1.CA_PageNumber);
				AssertEquals("Precondition: line2", 9, line2.CA_PageNumber);
			}

			headerCollection.RecalculateAllPageNumbers();
			AssertEquals("line1 was recalculated", 1, line1.CA_PageNumber);
			AssertEquals("line2 was recalculated", 2, line2.CA_PageNumber);
		}
	}
}
