using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCommonContainerCollection))]
	sealed class DocCommonContainerCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCommonContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			return DocCommonContainer.New(container, cartage, Factory);
		}

		protected override DocCommonContainerCollection GetCollectionToTest()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			return new DocCommonContainerCollection(cartage);
		}
	}
}
