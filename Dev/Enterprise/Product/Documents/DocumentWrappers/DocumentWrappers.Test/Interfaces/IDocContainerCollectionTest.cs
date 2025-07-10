using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(IDocContainerCollection))]
	sealed class IDocContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<IDocContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var container = Factory.New<CommonContainer>();
			return DocContainer.New(container, Factory);
		}

		protected override IDocContainerCollection GetCollectionToTest()
		{
			return new IDocContainerCollection(Factory);
		}
	}
}
