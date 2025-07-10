using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ValuationJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_InvoiceUQ()
		{
			invoiceLine.JI_InvoiceUQ = "KG";
			AssertNoMessageErrors(invoiceLine.JI_InvoiceUQInfo);

			invoiceLine.JI_InvoiceUQ = "";
			AssertNoMessageErrors(invoiceLine.JI_InvoiceUQInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._5SM;

			var invoice = declaration.Invoices[0];
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
		JobComInvoiceLine invoiceLine;
	}
}
