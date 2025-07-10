using CargoWise.EntityFramework.Testing;
using Enterprise.Client.Wow.CASSKIRKOrderIntegration;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Client.Wow.Testing
{
	class WowDocImportOrderLineTest : TestCaseWithFactory
	{
		public void TestPaddedLineID()
		{
			Order order = TestHelper.GetOrder();
			AssertEquals("order has 1 orderline", 1, order.OrderLines.Count);
			OrderLine orderLine = order.OrderLines[0];
			orderLine.JO_SubLineNo = 1;
			WowDocImportOrderLine wrapper = WowDocImportOrderLine.New(orderLine, Factory, false);
			AssertEquals("PaddedLineID", "0001", wrapper.PaddedLineID);
			OrderLine orderLineWithSameLineNo = order.OrderLines.AddNew();
			orderLineWithSameLineNo.JO_LineNo = orderLine.JO_LineNo;
			orderLineWithSameLineNo.JO_SubLineNo = 2;
			orderLineWithSameLineNo.JO_Partno = "rubber";
			AssertEquals("PaddedLineID", "0001-1", wrapper.PaddedLineID);
		}

		CASSKIRKTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new CASSKIRKTestHelper(Factory));
			}
		}

		CASSKIRKTestHelper testHelper;
		public void TestIsUpdated()
		{
			OrderLine orderLine = Factory.NewWithValidTestData<OrderLine>();
			WowDocImportOrderLine docOrdreLine = WowDocImportOrderLine.New(orderLine, Factory, true);
			AssertEquals("Is Updated", true, docOrdreLine.IsUpdated);
			docOrdreLine = WowDocImportOrderLine.New(orderLine, Factory, false);
			AssertEquals("Is Updated", false, docOrdreLine.IsUpdated);
		}
	}
}
