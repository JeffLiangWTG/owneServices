using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderToPrint))]
	sealed class JobComInvoiceHeaderToPrintTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaults()
		{
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "Buyer";
			var helper = new DeclarationTestHelper();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "123";
			invoice.JZ_OH_Buyer = buyer.PK;
			var invoiceToPrint = new JobComInvoiceHeaderToPrint(invoice);
			AssertEquals("Organisation", "Buyer", invoiceToPrint.Organisation);
			AssertEquals("Identifier", "123", invoiceToPrint.Identifier);
			AssertEquals("LastPrintDate", ZDateTime.Empty, invoiceToPrint.LastPrintDate);
			AssertEquals("ShouldBePrinted", true, invoiceToPrint.ShouldBePrinted);
		}

		public void TestDefaultsWhenPreviouslyPrinted()
		{
			var testDate = ZDateTime.Now;
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "Buyer";
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "123";
			invoice.JZ_OH_Buyer = buyer.PK;
			invoice.CA_LVSLastPrintDate = testDate;
			var invoiceToPrint = new JobComInvoiceHeaderToPrint(invoice);
			AssertEquals("Organisation", "Buyer", invoiceToPrint.Organisation);
			AssertEquals("Identifier", "123", invoiceToPrint.Identifier);
			AssertEquals("LastPrintDate should default from Invoice", testDate, invoiceToPrint.LastPrintDate);
			AssertEquals("ShouldBePrinted should not be ticked", false, invoiceToPrint.ShouldBePrinted);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new JobComInvoiceHeaderToPrint(Factory.New<JobComInvoiceHeader>());
		}

		#endregion
	}
}
