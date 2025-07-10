using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(PreviousDocumentCollection))]
sealed class PreviousDocumentCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.PreviousDocumentCollectionTest
{
	protected override CusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> GetCusSupportingInfoCollection()
	{
		var decl = Factory.New<JobDeclaration>();
		var invoiceHeader = decl.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		return new PreviousDocumentCollection(invoiceLine);
	}

	public void TestPreviousDocumentType()
	{
		var collection = GetCusSupportingInfoCollection();
		AssertType<PreviousDocument>(collection.AddNew());
	}
}

