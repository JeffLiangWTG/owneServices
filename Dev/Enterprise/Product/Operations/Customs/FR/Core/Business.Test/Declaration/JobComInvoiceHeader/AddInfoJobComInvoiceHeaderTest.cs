using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceHeader))]
	sealed class AddInfoJobComInvoiceHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestZG_ValuationMethod()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.ZG_ValuationMethod = "2";
			CombineAssertions(() =>
			{
				AssertEquals("Line 1 Validation Code", "2", invoiceLine1.JI_ValuationCode);
				AssertEquals("Line 2 Validation Code", "2", invoiceLine2.JI_ValuationCode);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			return new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
		}
	}
}
