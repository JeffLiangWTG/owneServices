using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(SupportingDocumentCollection))]
class SupportingDocumentCollectionInvoiceHeaderTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest() => new SupportingDocumentCollection(Factory.New<JobComInvoiceHeader>());
}
