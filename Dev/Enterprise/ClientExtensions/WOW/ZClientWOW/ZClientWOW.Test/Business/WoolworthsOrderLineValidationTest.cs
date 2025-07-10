using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Client.Wow.Testing
{
	public class WoolworthsOrderLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateTotalJO_Quantity()
		{
			Order order = Factory.New<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			WoolworthsOrderLineDelivery delivery1 = (WoolworthsOrderLineDelivery)orderLine.Deliveries.AddNew();
			WoolworthsOrderLineDelivery delivery2 = (WoolworthsOrderLineDelivery)orderLine.Deliveries.AddNew();
			delivery1.J4_QuantityOrdered = 1;
			delivery2.J4_QuantityOrdered = 2;
			AssertEquals("Should have a warning as the total of deliveries is incorrect", true, orderLine.JO_QuantityInfo.HasWarnings());
			orderLine.JO_Quantity = 3;
			AssertEquals("Should NOT have a warning as the total is now incorrect", false, orderLine.JO_QuantityInfo.HasWarnings());
		}

		public void TestValidateJO_Partno_WhenOrderLinesCollectionNotLoaded()
		{
			Order savedOrder = Factory.NewWithValidTestData<Order>();
			OrderLine savedOrderLine = savedOrder.OrderLines.AddNew();
			savedOrderLine.JO_Partno = "123";
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrderLine orderLineNotInLoadedCollection = newFactory.Load<OrderLine>(savedOrderLine.PK);
			AssertEquals("The OrderLine shouldnt be contained within a collection for the test", 0, ((IBusinessObjectInternals)orderLineNotInLoadedCollection).ParentCollections.Length);
			// expect no exception
			orderLineNotInLoadedCollection.JO_Partno = "1234";
		}
	}
}
