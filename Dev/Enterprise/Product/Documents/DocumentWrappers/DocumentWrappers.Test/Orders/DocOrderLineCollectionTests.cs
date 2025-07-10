using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Orders.Testing
{
	[TestedType(typeof(DocOrderLineCollection))]
	sealed class DocOrderLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocOrderLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orderLine = Factory.New<OrderLine>();
			return DocOrderLine.New(orderLine, Factory);
		}

		protected override DocOrderLineCollection GetCollectionToTest()
		{
			return new DocOrderLineCollection(Factory);
		}
	}
}
