using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Orders.Testing
{
	[TestedType(typeof(DocOrderLineDeliveryCollection))]
	sealed class DocOrderLineDeliveryCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocOrderLineDeliveryCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orderLineDelivery = Factory.New<OrderLineDelivery>();
			return DocOrderLineDelivery.New(orderLineDelivery, Factory);
		}

		protected override DocOrderLineDeliveryCollection GetCollectionToTest()
		{
			return new DocOrderLineDeliveryCollection(Factory);
		}
	}
}
