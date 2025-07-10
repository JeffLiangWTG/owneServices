using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(OrderWrapper))]
	sealed class OrderWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			OrderWrapper wrapperEmpty = new OrderWrapper((Order)null, Factory);
			AssertEquals("wrapperEmpty.ToString()", ZString.Empty, wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.OrderNo", ZString.Empty, wrapperEmpty.OrderNo);
			AssertEquals("wrapperEmpty.OrderDate", ZDateTime.Empty, wrapperEmpty.OrderDate);
			AssertEquals("wrapperEmpty.Lines.Count", 0, wrapperEmpty.Lines.Count);
		}

		public void TestWrapperMappingFullFromOrder()
		{
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "1234567";
			order.JD_OrderDate = new ZDateTime(2006, 5, 6);
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_Quantity = 123m;

			OrderWrapper wrapperFull = new OrderWrapper(order, Factory);
			AssertEquals("wrapperFull.ToString()", "1234567", wrapperFull.ToString());
			AssertEquals("wrapperFull.OrderNo", "1234567", wrapperFull.OrderNo);
			AssertEquals("wrapperFull.OrderDate", new ZDateTime(2006, 5, 6), wrapperFull.OrderDate);
			AssertEquals("wrapperFull.Lines.Count", 1, wrapperFull.Lines.Count);
			OrderLineWrapper lineWrapper = wrapperFull.Lines[0];
			AssertEquals("lineWrapper.QuantityOrdered.ValueAndUnitCodeBlankIfZero", "123.00 UNT", lineWrapper.QuantityOrdered.ValueAndUnitCodeBlankIfZero);
		}

		public void TestWrapperMappingFullFromOrderItem()
		{
			OrderItem orderItem = Factory.New<OrderItem>();
			orderItem.JT_OrderReference = "234309889";
			OrderWrapper wrapperFull = new OrderWrapper(orderItem, Factory);
			AssertEquals("wrapperFull.ToString()", "234309889", wrapperFull.ToString());
			AssertEquals("wrapperFull.OrderNo", "234309889", wrapperFull.OrderNo);
			AssertEquals("wrapperFull.OrderDate", ZDateTime.Empty, wrapperFull.OrderDate);
		}

		public void TestWrapperOrderNoShowsSplits()
		{
			var order = Factory.New<Order>();
			order.JD_OrderNumber = "1234567";
			order.JD_OrderNumberSplit = 1;

			var wrapper = new OrderWrapper(order, Factory);
			AssertEquals("wrapper.ToString()", "1234567-1", wrapper.ToString());
			AssertEquals("wrapper.OrderNo", "1234567-1", wrapper.OrderNo);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Order                                         (Default Field: OrderNo)
======================================================================
Name                                    Type
----------------------------------------------------------------------
OrderDate                               DateTime
OrderNo                                 String

Lines                                   OrderLine Collection
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			Order order = Factory.New<Order>();
			return new OrderWrapper(order, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new OrderWrapper((Order)null, Factory);
		}
	}
}
