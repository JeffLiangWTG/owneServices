using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Orders.Testing
{
	[TestedType(typeof(DocOrderLineDeliverContainerCollection))]
	sealed class DocOrderLineDeliverContainerCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocOrderLineDeliverContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orderLineDeliverContainer = Factory.New<OrderLineDeliverContainer>();
			return DocOrderLineDeliverContainer.New(orderLineDeliverContainer, Factory);
		}

		protected override DocOrderLineDeliverContainerCollection GetCollectionToTest()
		{
			return new DocOrderLineDeliverContainerCollection(Factory);
		}
	}
}
