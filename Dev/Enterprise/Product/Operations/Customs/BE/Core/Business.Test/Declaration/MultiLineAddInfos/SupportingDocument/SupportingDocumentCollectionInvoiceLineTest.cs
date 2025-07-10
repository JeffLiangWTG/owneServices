using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(SupportingDocumentCollection))]
class SupportingDocumentCollectionInvoiceLineTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest() => new SupportingDocumentCollection(Factory.New<JobComInvoiceLine>());
}
