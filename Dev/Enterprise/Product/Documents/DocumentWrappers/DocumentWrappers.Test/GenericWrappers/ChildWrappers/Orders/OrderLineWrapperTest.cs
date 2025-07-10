using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(OrderLineWrapper))]
	sealed class OrderLineWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			OrderLineWrapper emptywrapper = new OrderLineWrapper(Factory.New<OrderLine>(), Factory);
			AssertEquals("emptywrapper.Description", ZString.Empty, emptywrapper.Description);
			AssertEquals("emptywrapper.Status.Code", "PLC", emptywrapper.Status.Code);
			AssertEquals("emptywrapper.LineNumber", new ZInt(1), emptywrapper.LineNumber);
			AssertEquals("emptywrapper.Product.Code", ZString.Empty, emptywrapper.Product.Code);
			AssertEquals("emptywrapper.OuterPacks", ZDecimal.Zero, emptywrapper.OuterPacks);
			AssertEquals("emptywrapper.InnerPacks", ZDecimal.Zero, emptywrapper.InnerPacks);
			AssertEquals("emptywrapper.TotalInnerPacks", ZDecimal.Zero, emptywrapper.TotalInnerPacks);
			AssertEquals("emptywrapper.QuantityOrdered.ValueAndUnitCodeBlankIfZero", ZString.Empty, emptywrapper.QuantityOrdered.ValueAndUnitCodeBlankIfZero);
			AssertEquals("emptywrapper.QuantityInvoiced.ValueAndUnitCodeBlankIfZero", ZString.Empty, emptywrapper.QuantityInvoiced.ValueAndUnitCodeBlankIfZero);
			AssertEquals("emptywrapper.QuantityReceived.ValueAndUnitCodeBlankIfZero", ZString.Empty, emptywrapper.QuantityReceived.ValueAndUnitCodeBlankIfZero);
			AssertEquals("emptywrapper.ItemPrice.AmountAndCurrencyCode", ZString.Empty, emptywrapper.ItemPrice.AmountAndCurrencyCode);
			AssertEquals("emptywrapper.QuantityRemaining.ValueAndUnitCodeBlankIfZero", ZString.Empty, emptywrapper.QuantityRemaining.ValueAndUnitCodeBlankIfZero);
			AssertEquals("emptywrapper.TotalLinePrice.AmountAndCurrencyCode", ZString.Empty, emptywrapper.TotalLinePrice.AmountAndCurrencyCode);
			AssertEquals("emptywrapper.RequiredDate", ZDateTime.Empty, emptywrapper.RequiredDate);
		}

		public void TestWrapperMappingsFull()
		{
			Order order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = "AUD";
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = new ZInt(8);
			orderLine.JO_Description = "JUST A DESCRIPTION OF WHAT IS ON THE LINE";
			orderLine.JO_OuterPacks = 8324;
			orderLine.JO_InnerPacks = 100000;
			orderLine.JO_Quantity = 12.34m;
			orderLine.JO_QtyInvoiced = 15.68m;
			orderLine.JO_QtyReceived = 45.89m;
			orderLine.JO_ItemPrice = 0.01m;
			orderLine.JO_LineDropDate = new ZDateTime(2008, 12, 23);
			orderLine.JO_F3_NKPackType = "BOX";

			OrderLineWrapper fullWrapper = new OrderLineWrapper(orderLine, Factory);
			AssertEquals("fullWrapper.LineNumber", new ZInt(8), fullWrapper.LineNumber);
			AssertEquals("fullWrapper.Description", "JUST A DESCRIPTION OF WHAT IS ON THE LINE", fullWrapper.Description);
			AssertEquals("fullWrapper.OuterPacks", 8324m, fullWrapper.OuterPacks);
			AssertEquals("fullWrapper.InnerPacks", 100000m, fullWrapper.InnerPacks);
			AssertEquals("fullWrapper.TotalInnerPacks", 832400000m, fullWrapper.TotalInnerPacks);
			AssertEquals("fullWrapper.QuantityOrdered.ValueAndUnitCodeBlankIfZero", "12.34 BOX", fullWrapper.QuantityOrdered.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.QuantityInvoiced.ValueAndUnitCodeBlankIfZero", "15.68 BOX", fullWrapper.QuantityInvoiced.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.QuantityReceived.ValueAndUnitCodeBlankIfZero", "45.89 BOX", fullWrapper.QuantityReceived.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.ItemPrice.AmountAndCurrencyCode", "0.01 AUD", fullWrapper.ItemPrice.AmountAndCurrencyCode);
			AssertEquals("fullWrapper.QuantityRemaining.ValueAndUnitCodeBlankIfZero", ZString.Empty, fullWrapper.QuantityRemaining.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.TotalLinePrice.AmountAndCurrencyCode", "0.12 AUD", fullWrapper.TotalLinePrice.AmountAndCurrencyCode);
			AssertEquals("fullWrapper.RequiredDate", new ZDateTime(2008, 12, 23), fullWrapper.RequiredDate);
		}

		public void TestOrderNumber()
		{
			var order = Factory.New<Order>();
			order.JD_OrderNumber = "order";
			order.JD_OrderNumberSplit = 2;

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = new ZInt(8);

			var wrapper = new OrderLineWrapper(orderLine, Factory);
			AssertEquals("order-2", wrapper.OrderNumber);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
OrderLine                                 (Default Field: Description)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Product                                 CodeAndDescription
Status                                  CodeAndDescription
ItemPrice                               Money
TotalLinePrice                          Money
QuantityInvoiced                        ValueAndUnit
QuantityOrdered                         ValueAndUnit
QuantityReceived                        ValueAndUnit
QuantityRemaining                       ValueAndUnit
Description                             String
InnerPacks                              Decimal
LineNumber                              Int
OrderNumber                             String
OuterPacks                              Decimal
RequiredDate                            DateTime
TotalInnerPacks                         Decimal
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
ItemPrice : 0.01 AUD
Product : GOOBA12
QuantityInvoiced : 15.68 BOX
QuantityOrdered : 100.34 BOX
QuantityReceived : 45.89 BOX
QuantityRemaining : 54.45 BOX
Registry : (No Default Field Value Available on Registry)
Status : INC - " + new CodeDescriptionPairList(OLookUpEditType.OrderHeaderStatus).GetDescriptionFromCode("INC") + @"
TotalLinePrice : 1.00 AUD
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			using var advOrm = AdvOrmFeatureHelper.GetMockedDisposable(isEnabled: true);

			Order order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = "AUD";
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = new ZInt(8);
			orderLine.JO_Description = "JUST A DESCRIPTION OF WHAT IS ON THE LINE";
			orderLine.JO_OuterPacks = 8324;
			orderLine.JO_InnerPacks = 100000;
			orderLine.JO_Quantity = 100.34m;
			orderLine.JO_QtyInvoiced = 15.68m;
			orderLine.JO_QtyReceived = 45.89m;
			orderLine.JO_ItemPrice = 0.01m;
			orderLine.JO_LineDropDate = new ZDateTime(2008, 12, 23);
			orderLine.JO_F3_NKPackType = "BOX";
			orderLine.JO_Partno = "GOOBA12";

			return new OrderLineWrapper(orderLine, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new OrderLineWrapper(null, Factory);
		}
	}
}
