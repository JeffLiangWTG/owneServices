using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineViewCollection))]
	sealed class JobComInvoiceLineCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
	{
		public void TestReCalculatePageNumber_WhenSuspendAdditionallyForImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			var invoice1Line2 = invoiceHeader1.JobComInvoiceLines.AddNew();

			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoice2Line1 = invoiceHeader2.JobComInvoiceLines.AddNew();
			var invoice2Line2 = invoiceHeader2.JobComInvoiceLines.AddNew();

			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				using (invoiceLine.SuspendRecalculatePageNumbers())
				{
					invoiceLine.CA_PageNumber = 0;
				}
			}
			var viewCollection = new InvoiceLineViewCollection(declaration);

			using (viewCollection.SuspendAdditionallyForImport())
			{
				// Do nothing but call the finalization
			}

			AssertContainsExactElementsInExactOrder(new ZInt[] { 1, 2, 3, 4 }, new[] { invoice1Line1.CA_PageNumber, invoice1Line2.CA_PageNumber, invoice2Line1.CA_PageNumber, invoice2Line2.CA_PageNumber });
		}

		public void TestNoRowNotInTableExceptionThrownWhenAddingLineForDeletedInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.Delete();

			AssertNoExceptionThrown(() => invoice.JobComInvoiceLines.AddNew());
		}

		public void TestSetDefaultsForNewChild()
		{
			TestDec.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice = TestDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			var lineCollection = new InvoiceLineCompleteCollection(TestDec);
			var collection = new JobComInvoiceLineViewCollection(invoice, lineCollection);
			var invoiceLine = collection.AddNew();
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.DeliveredDutyPaid, invoiceLine.CA_CalculationMethod);

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceLine = collection.AddNew();
			AssertEquals("CA_CalculationMethod", CalculationMethods.Codes.NoRemission, invoiceLine.CA_CalculationMethod);
		}

		public void TestTypedIndexer()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			InvoiceLineCompleteCollection lineCollection = new InvoiceLineCompleteCollection(declaration);
			JobComInvoiceLineViewCollection collection = new JobComInvoiceLineViewCollection(header, lineCollection);
			JobComInvoiceLine invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		protected override JobComInvoiceLineViewCollection GetCollectionToTest()
		{
			return Invoice.JobComInvoiceLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_JZ = Invoice.PK;
			if (Invoice.JobComInvoiceLines.Contains(invoiceLine))
			{
				Invoice.JobComInvoiceLines.Remove(invoiceLine);
			}
			return invoiceLine;
		}

		#region TestDec
		JobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = Factory.New<JobDeclaration>();
				}
				return fTestDec;
			}
		}
		JobDeclaration fTestDec;
		#endregion

		#region Invoice
		JobComInvoiceHeader Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = TestDec.Invoices.AddNew();
				}
				return fInvoice;
			}
		}
		JobComInvoiceHeader fInvoice;
		#endregion
	}
}
