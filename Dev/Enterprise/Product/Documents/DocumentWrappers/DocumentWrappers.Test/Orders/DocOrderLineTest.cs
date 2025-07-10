using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Orders.Testing
{
	[TestedType(typeof(DocOrderLine))]
	sealed class DocOrderLineTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrderLine.New(Line, Factory),
				DocOrderLine.New(Line.Factory, Line.PK)
			};
		}

		#region Overrides

		public void TestToString()
		{
			AssertEquals("ToString()", ZString.Empty, OrderLineWrapper.ToString());
		}

		#endregion

		#region ZString Fields

		public void TestPrimaryKey()
		{
			AssertEquals("PrimaryKey", Line.PK.ToString(), OrderLineWrapper.PrimaryKey);
		}

		public void TestCustomAttrib1()
		{
			var customAttrib1 = new ZString("CustomAttrib1");
			Line.JO_CustomAttrib1 = customAttrib1;
			AssertEquals("CustomAttrib1", customAttrib1, OrderLineWrapper.CustomAttrib1);
		}

		public void TestCustomAttrib2()
		{
			var customAttrib2 = new ZString("CustomAttrib2");
			Line.JO_CustomAttrib2 = customAttrib2;
			AssertEquals("CustomAttrib2", customAttrib2, OrderLineWrapper.CustomAttrib2);
		}

		public void TestCustomAttrib3()
		{
			var customAttrib3 = new ZString("CustomAttrib3");
			Line.JO_CustomAttrib3 = customAttrib3;
			AssertEquals("CustomAttrib3", customAttrib3, OrderLineWrapper.CustomAttrib3);
		}

		public void TestCustomAttrib4()
		{
			var customAttrib4 = new ZString("CustomAttrib4");
			Line.JO_CustomAttrib4 = customAttrib4;
			AssertEquals("CustomAttrib4", customAttrib4, OrderLineWrapper.CustomAttrib4);
		}

		public void TestCustomAttrib5()
		{
			var customAttrib5 = new ZString("CustomAttrib5");
			Line.JO_CustomAttrib5 = customAttrib5;
			AssertEquals("CustomAttrib5", customAttrib5, OrderLineWrapper.CustomAttrib5);
		}

		public void TestCustomAttrib6()
		{
			var customAttrib6 = new ZString("CustomAttrib6");
			Line.JO_CustomAttrib6 = customAttrib6;
			AssertEquals("CustomAttrib6", customAttrib6, OrderLineWrapper.CustomAttrib6);
		}

		public void TestPartAttrib1()
		{
			var partAttrib1 = new ZString("PartAttrib1");
			Line.JO_PartAttrib1 = partAttrib1;
			AssertEquals("PartAttrib1", partAttrib1, OrderLineWrapper.PartAttrib1);
		}

		public void TestPartAttrib2()
		{
			var partAttrib2 = new ZString("PartAttrib2");
			Line.JO_PartAttrib2 = partAttrib2;
			AssertEquals("PartAttrib2", partAttrib2, OrderLineWrapper.PartAttrib2);
		}

		public void TestPartAttrib3()
		{
			var partAttrib3 = new ZString("PartAttrib3");
			Line.JO_PartAttrib3 = partAttrib3;
			AssertEquals("PartAttrib3", partAttrib3, OrderLineWrapper.PartAttrib3);
		}

		public void TestSerialNumber()
		{
			var serialNumber = new ZString("SerialNumber");
			Line.JO_SerialNumber = serialNumber;
			AssertEquals("SerialNumber", serialNumber, OrderLineWrapper.SerialNumber);
		}

		public void TestDescription()
		{
			var description = new ZString("Description");
			Line.JO_Description = description;
			AssertEquals("Description", description, OrderLineWrapper.Description);
		}

		public void TestLineStatus()
		{
			var lineStatus = new ZString("LLL");
			Line.JO_LineStatus = lineStatus;
			AssertEquals("LineStatus", lineStatus, OrderLineWrapper.LineStatus);
		}

		public void TestOrderUnitOfQty()
		{
			var orderUnitOfQty = new ZString("QTY");
			Line.JO_F3_NKPackType = orderUnitOfQty;
			AssertEquals("OrderUnitOfQty", orderUnitOfQty, OrderLineWrapper.OrderUnitOfQty);
		}

		public void TestPartno()
		{
			var partno = new ZString("Partno");
			Line.JO_Partno = partno;
			AssertEquals("Partno", partno, OrderLineWrapper.Partno);
		}

		public void TestLineID()
		{
			var order1 = Factory.New<Order>();
			var line1 = order1.OrderLines.AddNew();
			var lineWrapper = DocOrderLine.New(line1, Factory);

			AssertEquals("Order Line ID should be '1'", "1", lineWrapper.LineID);

			var line2 = order1.OrderLines.AddNew();
			line2.JO_LineNo = 1;

			AssertEquals("Order Line ID 1 should be '1-1'", "1-1", lineWrapper.LineID);
		}

		#endregion

		#region Collections

		public void TestDeliveries()
		{
			AssertEquals("Deliveries", 0, OrderLineWrapper.Deliveries.Count);

			Line.Deliveries.AddNew();
			AssertEquals("Deliveries", 1, OrderLineWrapper.Deliveries.Count);
			AssertEquals("Deliveries is of type DocOrderLineDelivery", typeof(DocOrderLineDelivery), OrderLineWrapper.Deliveries[0].GetType());

			Line.Deliveries.AddNew();
			AssertEquals("Deliveries", 2, OrderLineWrapper.Deliveries.Count);
			AssertEquals("Deliveries is of type DocOrderLineDelivery", typeof(DocOrderLineDelivery), OrderLineWrapper.Deliveries[0].GetType());
			AssertEquals("Deliveries is of type DocOrderLineDelivery", typeof(DocOrderLineDelivery), OrderLineWrapper.Deliveries[1].GetType());
		}

		#endregion

		#region Wrapper Fields

		public void TestOrder()
		{
			AssertNull("Order", OrderLineWrapper.Order);

			Line.JO_JD = Factory.New(typeof(Order)).PK;
			AssertNotNull("Order", OrderLineWrapper.Order);
			AssertEquals("Order", typeof(DocOrder), OrderLineWrapper.Order.GetType());
		}

		public void TestProduct()
		{
			AssertNull("Product", OrderLineWrapper.Product);
		}

		#endregion

		#region ZDecimal Fields

		public void TestQuantityRemaining()
		{
			AssertEquals("QuantityRemaining", Line.JO_QuantityRemaining, OrderLineWrapper.QuantityRemaining);
		}

		public void TestTotalInnerPacks()
		{
			AssertEquals("TotalInnerPacks", Line.JO_TotalInnerPacks, OrderLineWrapper.TotalInnerPacks);
		}

		//		public void TestTotalQuantityInvoiced()
		//		{
		//			AssertEquals("TotalQuantityInvoiced", Line.JO_Calc_TotalQtyReceived, OrderLineWrapper.TotalQuantityInvoiced);
		//		}

		public void TestCustomDecimal1()
		{
			ZDecimal customDecimal1 = new ZDecimal(1);
			Line.JO_CustomDecimal1 = customDecimal1;
			AssertEquals("CustomDecimal1", customDecimal1, OrderLineWrapper.CustomDecimal1);
		}

		public void TestCustomDecimal2()
		{
			ZDecimal customDecimal2 = new ZDecimal(1);
			Line.JO_CustomDecimal2 = customDecimal2;
			AssertEquals("CustomDecimal2", customDecimal2, OrderLineWrapper.CustomDecimal2);
		}

		public void TestCustomDecimal3()
		{
			ZDecimal customDecimal3 = new ZDecimal(1);
			Line.JO_CustomDecimal3 = customDecimal3;
			AssertEquals("CustomDecimal3", customDecimal3, OrderLineWrapper.CustomDecimal3);
		}

		public void TestCustomDecimal4()
		{
			ZDecimal customDecimal4 = new ZDecimal(1);
			Line.JO_CustomDecimal4 = customDecimal4;
			AssertEquals("CustomDecimal4", customDecimal4, OrderLineWrapper.CustomDecimal4);
		}

		public void TestCustomDecimal5()
		{
			ZDecimal customDecimal5 = new ZDecimal(1);
			Line.JO_CustomDecimal5 = customDecimal5;
			AssertEquals("CustomDecimal5", customDecimal5, OrderLineWrapper.CustomDecimal5);
		}

		public void TestInnerPacks()
		{
			ZDecimal innerPacks = new ZDecimal(1);
			Line.JO_InnerPacks = innerPacks;
			AssertEquals("InnerPacks", innerPacks, OrderLineWrapper.InnerPacks);
		}

		public void TestItemPrice()
		{
			ZDecimal itemPrice = new ZDecimal(1);
			Line.JO_ItemPrice = itemPrice;
			AssertEquals("ItemPrice", itemPrice, OrderLineWrapper.ItemPrice);
		}

		public void TestLinePrice()
		{
			ZDecimal linePrice = new ZDecimal(1);
			Line.JO_LinePrice = linePrice;
			AssertEquals("LinePrice", linePrice, OrderLineWrapper.LinePrice);
		}

		public void TestOuterPacks()
		{
			ZDecimal outerPacks = new ZDecimal(1);
			Line.JO_OuterPacks = outerPacks;
			AssertEquals("OuterPacks", outerPacks, OrderLineWrapper.OuterPacks);
		}

		public void TestQtyInvoiced()
		{
			ZDecimal qtyInvoiced = new ZDecimal(1);
			Line.JO_QtyInvoiced = qtyInvoiced;
			AssertEquals("QtyInvoiced", qtyInvoiced, OrderLineWrapper.QtyInvoiced);
		}

		public void TestQtyReceived()
		{
			ZDecimal qtyReceived = new ZDecimal(1);
			Line.JO_QtyReceived = qtyReceived;
			AssertEquals("QtyReceived", qtyReceived, OrderLineWrapper.QtyReceived);
		}

		public void TestQuantity()
		{
			ZDecimal quantity = new ZDecimal(1);
			Line.JO_Quantity = quantity;
			AssertEquals("Quantity", quantity, OrderLineWrapper.Quantity);
		}

		public void TestOpenQuantity()
		{
			var openQuantity = new ZDecimal(1.2);
			Line.JO_OpenQuantity = openQuantity;
			AssertEquals("OpenQuantity", openQuantity, OrderLineWrapper.OpenQuantity);
		}

		public void TestPackedQuantity()
		{
			var packedQuantity = new ZDecimal(1.7);
			Line.JO_QtyPacked = packedQuantity;
			AssertEquals("PackedQuantity", packedQuantity, OrderLineWrapper.PackedQuantity);
		}

		#endregion

		#region ZBool Fields

		public void TestContainersVisible()
		{
			Line.JO_ContainersVisible = ZBool.False;
			Assert("!ContainersVisible", !OrderLineWrapper.ContainersVisible);
		}

		public void TestCustomFlag1()
		{
			Line.JO_CustomFlag1 = ZBool.False;
			Assert("!CustomFlag1", !OrderLineWrapper.CustomFlag1);

			Line.JO_CustomFlag1 = ZBool.True;
			Assert("CustomFlag1", OrderLineWrapper.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			Line.JO_CustomFlag2 = ZBool.False;
			Assert("!CustomFlag2", !OrderLineWrapper.CustomFlag2);

			Line.JO_CustomFlag2 = ZBool.True;
			Assert("CustomFlag2", OrderLineWrapper.CustomFlag2);
		}

		public void TestCustomFlag3()
		{
			Line.JO_CustomFlag3 = ZBool.False;
			Assert("!CustomFlag3", !OrderLineWrapper.CustomFlag3);

			Line.JO_CustomFlag3 = ZBool.True;
			Assert("CustomFlag3", OrderLineWrapper.CustomFlag3);
		}

		public void TestCustomFlag4()
		{
			Line.JO_CustomFlag4 = ZBool.False;
			Assert("!CustomFlag4", !OrderLineWrapper.CustomFlag4);

			Line.JO_CustomFlag4 = ZBool.True;
			Assert("CustomFlag4", OrderLineWrapper.CustomFlag4);
		}

		public void TestCustomFlag5()
		{
			Line.JO_CustomFlag5 = ZBool.False;
			Assert("!CustomFlag5", !OrderLineWrapper.CustomFlag5);

			Line.JO_CustomFlag5 = ZBool.True;
			Assert("CustomFlag5", OrderLineWrapper.CustomFlag5);
		}

		#endregion

		#region ZDateTime Fields

		public void TestCustomDate1()
		{
			ZDateTime customDate1 = new ZDateTime(2004, 04, 04);
			Line.JO_CustomDate1 = customDate1;
			AssertEquals("CustomDate1", customDate1, OrderLineWrapper.CustomDate1);
		}

		public void TestCustomDate2()
		{
			ZDateTime customDate2 = new ZDateTime(2004, 04, 04);
			Line.JO_CustomDate2 = customDate2;
			AssertEquals("CustomDate2", customDate2, OrderLineWrapper.CustomDate2);
		}

		public void TestCustomDate3()
		{
			ZDateTime customDate3 = new ZDateTime(2004, 04, 04);
			Line.JO_CustomDate3 = customDate3;
			AssertEquals("CustomDate3", customDate3, OrderLineWrapper.CustomDate3);
		}

		public void TestCustomDate4()
		{
			ZDateTime customDate4 = new ZDateTime(2004, 04, 04);
			Line.JO_CustomDate4 = customDate4;
			AssertEquals("CustomDate4", customDate4, OrderLineWrapper.CustomDate4);
		}

		public void TestCustomDate5()
		{
			ZDateTime customDate5 = new ZDateTime(2004, 04, 04);
			Line.JO_CustomDate5 = customDate5;
			AssertEquals("CustomDate5", customDate5, OrderLineWrapper.CustomDate5);
		}

		public void TestLineDropDate()
		{
			ZDateTime lineDropDate = new ZDateTime(2004, 04, 04);
			Line.JO_LineDropDate = lineDropDate;
			AssertEquals("LineDropDate", lineDropDate, OrderLineWrapper.LineDropDate);
		}

		public void TestShipmentWindowStart()
		{
			var shipmentWindowStart = new ZDate(2004, 04, 04);
			Line.JO_ShipmentWindowStart = shipmentWindowStart;
			AssertEquals("ShipmentWindowStart", shipmentWindowStart, OrderLineWrapper.ShipmentWindowStart);
		}

		public void TestShipmentWindowEnd()
		{
			var shipmentWindowEnd = new ZDate(2004, 04, 04);
			Line.JO_ShipmentWindowEnd = shipmentWindowEnd;
			AssertEquals("ShipmentWindowEnd", shipmentWindowEnd, OrderLineWrapper.ShipmentWindowEnd);
		}

		#endregion

		#region ZInt Fields

		public void TestLineNo()
		{
			Line.JO_LineNo = 1;
			AssertEquals("LineNo", 1, OrderLineWrapper.LineNo);
		}

		#endregion

		#region Implementation

		OrderLine Line;
		DocOrderLine OrderLineWrapper;

		protected override void SetUp()
		{
			Line = Factory.New<OrderLine>();
			OrderLineWrapper = DocOrderLine.New(Line, Factory);
			base.SetUp();
		}

		#endregion
	}
}
