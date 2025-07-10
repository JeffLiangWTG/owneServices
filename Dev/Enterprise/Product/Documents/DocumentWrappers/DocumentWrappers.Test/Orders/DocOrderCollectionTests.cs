using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Orders.Testing
{
	[TestedType(typeof(DocOrderCollection))]
	sealed class DocOrderCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocOrderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var order = Factory.New<Order>();
			return DocOrder.New(order, Factory);
		}

		protected override DocOrderCollection GetCollectionToTest()
		{
			return new DocOrderCollection(Factory);
		}
	}
}
