using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceLine))]
	sealed class AddInfoJobComInvoiceLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var lookups = invoiceLine.AddInfoLookups;
			AssertType<ExportAddInfoJobComInvoiceLineLookups>(lookups);
			AssertNotSame("not cached", lookups, invoiceLine.AddInfoLookups);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			lookups = invoiceLine.AddInfoLookups;
			AssertType<AddInfoJobComInvoiceLineLookups>(lookups);
			AssertNotSame("not cached", lookups, invoiceLine.AddInfoLookups);
		}

		public void TestValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<ExportAddInfoJobComInvoiceLineValidation>(invoiceLine.AddInfoValidation);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<ImportAddInfoJobComInvoiceLineValidation>(invoiceLine.AddInfoValidation);
		}

		protected override BusinessObject GetNewBusinessObject() => new AddInfoJobComInvoiceLine(Factory.New<JobComInvoiceLine>().JI_AddInfoInfo);
	}
}
