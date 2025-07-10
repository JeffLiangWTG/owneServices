using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocOuterPackCollection))]
	sealed class DocOuterPackCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocOuterPackCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OuterPack pack = new OuterPack();
			return DocOuterPack.New(pack, Factory);
		}

		protected override DocOuterPackCollection GetCollectionToTest()
		{
			return new DocOuterPackCollection(Factory);
		}
	}
}
