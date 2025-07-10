using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Client.Wow.Testing
{
	public class WoolworthsOrderLineDeliveryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateQuantityOrdered()
		{
			WoolworthsOrderLineDelivery delivery = (WoolworthsOrderLineDelivery)Factory.NewWithValidTestData(typeof(OrderLineDelivery));
			OrderLineDeliverContainer container1 = delivery.Containers.AddNew();
			OrderLineDeliverContainer container2 = delivery.Containers.AddNew();
			container1.J5_QuantityInvoiced = 10m;
			container2.J5_QuantityInvoiced = 20m;
			delivery.J4_QuantityOrdered = 25m;
			AssertEquals("Error because qty ordered less than the total quantity invoiced on the container lines", true, delivery.J4_CustomDecimal5Info.HasErrors());
			delivery.J4_QuantityOrdered = 35m;
			AssertEquals("There should no longer be an error", false, delivery.J4_CustomDecimal5Info.HasErrors());
		}
	}
}
