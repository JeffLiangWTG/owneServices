using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(OrderWrapperCollection))]
	sealed class OrderWrapperCollectionTest : Base.Testing.GenericWrapperCollectionTest<OrderWrapperCollection>
	{
		public void TestLoadFromShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.DocsAndCartage.JP_OrderItemsAsString = "1234";

			OrderWrapperCollection collection = new OrderWrapperCollection(shipment, Factory);
			AssertEquals("collection.Count", 1, collection.Count);

			shipment.AttachedOrders.AddNew().JD_OrderNumber = "7777";
			shipment.AttachedOrders.AddNew().JD_OrderNumber = "0001";
			shipment.AttachedOrders.AddNew().JD_OrderNumber = "6666";

			collection = new OrderWrapperCollection(shipment, Factory);
			AssertEquals("collection.Count", 4, collection.Count);
		}

		public void TestLoadFromDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DocsAndCartage.JP_OrderItemsAsString = "1234,5678,9101112,13141516";

			OrderWrapperCollection collection = new OrderWrapperCollection(declaration, Factory);
			AssertEquals("collection.Count", 4, collection.Count);

			declaration.AttachedOrders.AddNew();
			declaration.AttachedOrders.AddNew();

			collection = new OrderWrapperCollection(declaration, Factory);
			AssertEquals("collection.Count", 6, collection.Count);
		}

		protected override OrderWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new OrderWrapperCollection(null, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new OrderWrapper((Order)null, Factory);
		}
	}
}
