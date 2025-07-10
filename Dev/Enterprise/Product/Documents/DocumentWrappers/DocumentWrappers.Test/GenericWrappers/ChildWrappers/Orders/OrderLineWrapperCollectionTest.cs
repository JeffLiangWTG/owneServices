using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(OrderLineWrapperCollection))]
	sealed class OrderLineWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<OrderLineWrapperCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new OrderLineWrapper(null, Factory);
		}

		protected override OrderLineWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new OrderLineWrapperCollection(Factory.GetNull<Order>(), Factory);
		}

		public void TestWrapperFull()
		{
			Order order = Factory.New<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_Description = "THIS IS THE BEST DESCRIPTION EVER";

			OrderLineWrapperCollection wrapperCollection = new OrderLineWrapperCollection(order, Factory);
			AssertEquals("wrapperCollection.Count", 1, wrapperCollection.Count);
			AssertEquals("wrapperCollection[0].Description", "THIS IS THE BEST DESCRIPTION EVER", wrapperCollection[0].Description);
		}
	}
}
