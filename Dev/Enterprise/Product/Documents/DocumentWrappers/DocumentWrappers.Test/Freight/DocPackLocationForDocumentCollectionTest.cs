using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocPackLocationForDocumentCollection))]
	sealed class DocPackLocationForDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocPackLocationForDocumentCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			PackLocationForDocument location = new PackLocationForDocument("S00002031", "AUSYD", "Shipping Inc", 23, "PKG", "Warehouse");
			return DocPackLocationForDocument.New(location, Factory);
		}

		protected override DocPackLocationForDocumentCollection GetCollectionToTest()
		{
			return new DocPackLocationForDocumentCollection(Factory);
		}
	}
}
