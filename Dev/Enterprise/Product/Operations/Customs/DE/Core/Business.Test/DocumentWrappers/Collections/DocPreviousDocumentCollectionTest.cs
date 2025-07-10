using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocPreviousDocumentCollection))]
	sealed class DocPreviousDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocPreviousDocumentCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => DocPreviousDocument.New(Factory.New<PreviousDocument>(), Factory);

		protected override DocPreviousDocumentCollection GetCollectionToTest() => new DocPreviousDocumentCollection(Factory);
	}
}
