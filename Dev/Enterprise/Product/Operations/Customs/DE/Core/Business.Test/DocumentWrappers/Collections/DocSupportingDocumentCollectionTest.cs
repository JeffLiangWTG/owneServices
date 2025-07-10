using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocSupportingDocumentCollection))]
	sealed class DocSupportingDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocSupportingDocumentCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => DocSupportingDocument.New(Factory.New<SupportingDocument>(), Factory);

		protected override DocSupportingDocumentCollection GetCollectionToTest() => new DocSupportingDocumentCollection(Factory);
	}
}
