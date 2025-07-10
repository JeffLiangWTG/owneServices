using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(IDocSimpleContainerCollection))]
	sealed class IDocSimpleContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<IDocSimpleContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var container = Factory.New<CommonContainer>();
			return DocContainer.New(container, Factory);
		}

		protected override IDocSimpleContainerCollection GetCollectionToTest()
		{
			return new IDocSimpleContainerCollection(Factory);
		}
	}
}
