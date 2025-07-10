using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	[TestedType(typeof(AccPayableOrderLineCollection))]
	public class AccPayableOrderLineCollectionTest : ActiveBusinessObjectCollectionTestCase<AccPayableOrderLineCollection>
	{
		protected override AccPayableOrderLineCollection GetCollectionToTest()
		{
			return new AccPayableOrderLineCollection(Factory);
		}

		public void TestIsOrderPartiallyComplete()
		{
			var order = Factory.NewWithValidTestData<AccPayableOrderHeader>();
			var line = order.OrderLines.AddNew();
			line.APL_Quantity = 10;
			line.APL_QtyInvoiced = 10;
			AssertEquals(order.OrderLines.IsOrderPartiallyComplete, false);
			line.APL_QtyInvoiced = 5;
			AssertEquals(order.OrderLines.IsOrderPartiallyComplete, true);
		}
	}
}
