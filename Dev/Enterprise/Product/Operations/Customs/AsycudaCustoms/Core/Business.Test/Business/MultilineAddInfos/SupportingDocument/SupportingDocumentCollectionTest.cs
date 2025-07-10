using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(SupportingDocumentCollection))]
	class SupportingDocumentCollectionTest : CusSupportingInfoCollectionTest<SupportingDocument>
	{
		protected override CusSupportingInfoCollection<SupportingDocument> GetCusSupportingInfoCollection() => ((ISupportingDocumentsProvider)Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew()).SupportingDocuments;

		protected void TestConstructors()
		{
			var invoiceHeader = Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew();
			AssertNoExceptionThrown(() => _ = new SupportingDocumentCollection(invoiceHeader));
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			AssertNoExceptionThrown(() => _ = new SupportingDocumentCollection(invoiceLine));
		}
	}
}
