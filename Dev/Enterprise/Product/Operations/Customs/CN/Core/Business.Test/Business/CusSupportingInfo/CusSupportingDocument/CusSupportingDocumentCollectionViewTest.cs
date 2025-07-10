using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusSupportingDocumentCollectionView))]
	class CusSupportingDocumentCollectionViewTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<CusSupportingDocumentCollectionView>
	{
		public void TestCollectionFiltering()
		{
			var cusSupportingDocuments1 = InvoiceLine.CusSupportingDocuments.AddNew();
			cusSupportingDocuments1.CSI_Code = "01";
			var cusSupportingDocuments2 = InvoiceLine.CusSupportingDocuments.AddNew();
			cusSupportingDocuments2.CSI_Code = "1Y";
			var collection = GetCollectionToTest();
			AssertEquals(1, collection.Count);
			var cusSupportingDocuments = collection.Cast<CusSupportingDocument>();
			AssertNull(cusSupportingDocuments.FirstOrDefault(x => x.CSI_Code == "1Y"));
			AssertNotNull(cusSupportingDocuments.FirstOrDefault(x => x.CSI_Code == "01"));
		}

		protected override CusSupportingDocumentCollectionView GetCollectionToTest() => new CusSupportingDocumentCollectionView(InvoiceLine.CusSupportingDocuments);

		protected override BusinessObject GetNewElementToAddToTheCollection() => InvoiceLine.CusSupportingDocuments.AddNew();

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		JobComInvoiceHeader InvoiceHeader => invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
		JobComInvoiceHeader invoiceHeader;

		JobComInvoiceLine InvoiceLine => invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew());
		JobComInvoiceLine invoiceLine;
	}
}
