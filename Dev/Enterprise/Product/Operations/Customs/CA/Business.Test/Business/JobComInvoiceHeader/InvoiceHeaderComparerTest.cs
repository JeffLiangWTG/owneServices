using System.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceHeaderComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoices = declaration.B2AsAccountedForInvoices;

			CreateInvoice(invoices, "1");
			CreateInvoice(invoices, "11");
			CreateInvoice(invoices, "2");
			CreateInvoice(invoices, "22");
			CreateInvoice(invoices, "A01");
			CreateInvoice(invoices, "B02");
			CreateInvoice(invoices, "NS");
			CreateInvoice(invoices, "NS");

			invoices.ApplySort(JobComInvoiceHeader.Schema.JZ_InvoiceNumber, ListSortDirection.Ascending);
			AssertEquals("1", invoices[0].JZ_InvoiceNumber);
			AssertEquals("01", invoices[1].JZ_InvoiceNumber);
			AssertEquals("2", invoices[2].JZ_InvoiceNumber);
			AssertEquals("02", invoices[3].JZ_InvoiceNumber);
			AssertEquals("11", invoices[4].JZ_InvoiceNumber);
			AssertEquals("22", invoices[5].JZ_InvoiceNumber);
			AssertEquals("NS1", invoices[6].JZ_InvoiceNumber);
			AssertEquals("NS2", invoices[7].JZ_InvoiceNumber);

			invoices.ApplySort(JobComInvoiceHeader.Schema.JZ_InvoiceNumber, ListSortDirection.Descending);
			AssertEquals("NS2", invoices[0].JZ_InvoiceNumber);
			AssertEquals("NS1", invoices[1].JZ_InvoiceNumber);
			AssertEquals("22", invoices[2].JZ_InvoiceNumber);
			AssertEquals("11", invoices[3].JZ_InvoiceNumber);
			AssertEquals("02", invoices[4].JZ_InvoiceNumber);
			AssertEquals("2", invoices[5].JZ_InvoiceNumber);
			AssertEquals("01", invoices[6].JZ_InvoiceNumber);
			AssertEquals("1", invoices[7].JZ_InvoiceNumber);

			invoices = declaration.B2AsClaimedForInvoices;
			invoices.ApplySort(JobComInvoiceHeader.Schema.JZ_InvoiceNumber, ListSortDirection.Ascending);
			AssertEquals("1", invoices[0].JZ_InvoiceNumber);
			AssertEquals("01", invoices[1].JZ_InvoiceNumber);
			AssertEquals("2", invoices[2].JZ_InvoiceNumber);
			AssertEquals("02", invoices[3].JZ_InvoiceNumber);
			AssertEquals("11", invoices[4].JZ_InvoiceNumber);
			AssertEquals("22", invoices[5].JZ_InvoiceNumber);
			AssertEquals("NS1", invoices[6].JZ_InvoiceNumber);
			AssertEquals("NS2", invoices[7].JZ_InvoiceNumber);

			invoices.ApplySort(JobComInvoiceHeader.Schema.JZ_InvoiceNumber, ListSortDirection.Descending);
			AssertEquals("NS2", invoices[0].JZ_InvoiceNumber);
			AssertEquals("NS1", invoices[1].JZ_InvoiceNumber);
			AssertEquals("22", invoices[2].JZ_InvoiceNumber);
			AssertEquals("11", invoices[3].JZ_InvoiceNumber);
			AssertEquals("02", invoices[4].JZ_InvoiceNumber);
			AssertEquals("2", invoices[5].JZ_InvoiceNumber);
			AssertEquals("01", invoices[6].JZ_InvoiceNumber);
			AssertEquals("1", invoices[7].JZ_InvoiceNumber);
		}

		void CreateInvoice(B2JobComInvoiceHeaderCollection invoiceHeaders, string invoiceNumber)
		{
			var invoiceHeader = invoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = invoiceNumber;
		}
	}
}
