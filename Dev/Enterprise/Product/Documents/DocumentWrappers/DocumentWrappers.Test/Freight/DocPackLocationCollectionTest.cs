using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocPackLocationCollection))]
	sealed class DocPackLocationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocPackLocationCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocPackLocation.New(Factory.New<PackLocation>(), Factory);
		}

		protected override DocPackLocationCollection GetCollectionToTest()
		{
			return new DocPackLocationCollection(Factory);
		}
	}
}
