using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Orders.Testing
{
	[TestedType(typeof(DocOrderLineDelivery))]
	sealed class DocOrderLineDeliveryTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrderLineDelivery.New(Delivery, Factory)
			};
		}

		#region Overrides

		public void TestToString()
		{
			AssertEquals("ToString()", ZString.Empty, DeliveryWrapper.ToString());
		}

		#endregion

		#region ZString Fields

		public void TestPrimaryKey()
		{
			AssertEquals("PrimaryKey", Delivery.PK.ToString(), DeliveryWrapper.PrimaryKey);
		}

		public void TestDeliverPointAndPrimaryKey()
		{
			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			Delivery.J4_OA_NKDeliveryPoint = address.OA_Code;
			AssertEquals("DeliverPointAndPrimaryKey", address.OA_Code + Delivery.PK.ToString(), DeliveryWrapper.DeliverPointAndPrimaryKey);
		}

		public void TestDeliverPointCode()
		{
			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			Delivery.J4_OA_NKDeliveryPoint = address.OA_Code;
			AssertEquals("DeliverPointCode", address.OA_Code, DeliveryWrapper.DeliverPointCode);
		}

		public void TestDeliverPointAddress1()
		{
			ZString deliverPointAddress1 = new ZString("DeliverPointAddress1");
			AssertEquals("DeliverPointAddress1", Delivery.J4_DeliverPointAddress1, DeliveryWrapper.DeliverPointAddress1);
		}

		public void TestDeliverPointAddress2()
		{
			ZString deliverPointAddress2 = new ZString("DeliverPointAddress2");
			AssertEquals("DeliverPointAddress2", Delivery.J4_DeliverPointAddress2, DeliveryWrapper.DeliverPointAddress2);
		}

		public void TestCustomAttribute1()
		{
			ZString customAttribute1 = new ZString("CustomAtt1");
			Delivery.J4_CustomAttribute1 = customAttribute1;
			AssertEquals("CustomAttribute1", customAttribute1, DeliveryWrapper.CustomAttribute1);
		}

		public void TestCustomAttribute2()
		{
			ZString customAttribute2 = new ZString("CustomAtt2");
			Delivery.J4_CustomAttribute2 = customAttribute2;
			AssertEquals("CustomAttribute2", customAttribute2, DeliveryWrapper.CustomAttribute2);
		}

		public void TestCustomAttribute3()
		{
			ZString customAttribute3 = new ZString("CustomAtt3");
			Delivery.J4_CustomAttribute3 = customAttribute3;
			AssertEquals("CustomAttribute3", customAttribute3, DeliveryWrapper.CustomAttribute3);
		}

		public void TestCustomAttribute4()
		{
			ZString customAttribute4 = new ZString("CustomAtt4");
			Delivery.J4_CustomAttribute4 = customAttribute4;
			AssertEquals("CustomAttribute4", customAttribute4, DeliveryWrapper.CustomAttribute4);
		}

		public void TestCustomAttribute5()
		{
			ZString customAttribute5 = new ZString("CustomAtt5");
			Delivery.J4_CustomAttribute5 = customAttribute5;
			AssertEquals("CustomAttribute5", customAttribute5, DeliveryWrapper.CustomAttribute5);
		}

		#endregion

		#region Collections

		public void TestContainers()
		{
			AssertEquals("Containers", 0, DeliveryWrapper.Containers.Count);

			Delivery.Containers.AddNew();
			AssertEquals("Containers", 1, DeliveryWrapper.Containers.Count);

			Delivery.Containers.AddNew();
			AssertEquals("Containers", 2, DeliveryWrapper.Containers.Count);
		}

		#endregion

		#region Wrapper Fields

		public void TestOrderLine()
		{
			AssertNotNull("OrderLine", DeliveryWrapper.OrderLine);
			AssertEquals("OrderLine is of type DocOrderLine", typeof(DocOrderLine), DeliveryWrapper.OrderLine.GetType());
		}

		public void TestDeliverPoint()
		{
			AssertNull("DeliverPoint", DeliveryWrapper.DeliverPoint);

			var theHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var theAddress = theHeader.Addresses.AddNew();
			theAddress.OA_Code = "CCC";
			TheOrder.BuyerPK = theHeader.PK;
			Delivery.J4_OA_NKDeliveryPoint = theAddress.OA_Code;

			DeliveryWrapper = DocOrderLineDelivery.New(Delivery, Factory);

			AssertNotNull("DeliverPoint", DeliveryWrapper.DeliverPoint);
			AssertEquals("DeliverPoint is of type DocAddress", typeof(DocAddress), DeliveryWrapper.DeliverPoint.GetType());
		}

		public void TestDestinationPort()
		{
			AssertNull("DestinationPort", DeliveryWrapper.DestinationPort);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Delivery.J4_RL_NKDestinationPort = uNLOCO.RL_Code;
			AssertNotNull("DestinationPort", DeliveryWrapper.DestinationPort);
			AssertEquals("DestinationPort is of type DocUNLOCO", typeof(DocUNLOCO), DeliveryWrapper.DestinationPort.GetType());
		}

		#endregion

		#region ZDecimal Fields

		public void TestTotalQuantity()
		{
			AssertEquals("TotalQuantity", Delivery.J4_Calc_TotalQuantityAllocated, DeliveryWrapper.TotalQuantity);
		}

		public void TestAllocated()
		{
			ZDecimal allocated = new ZDecimal(1);
			Delivery.J4_Allocated = allocated;
			AssertEquals("Allocated", allocated, DeliveryWrapper.Allocated);
		}

		public void TestCustomDecimal1()
		{
			ZDecimal customDecimal1 = new ZDecimal(1);
			Delivery.J4_CustomDecimal1 = customDecimal1;
			AssertEquals("CustomDecimal1", customDecimal1, DeliveryWrapper.CustomDecimal1);
		}

		public void TestCustomDecimal2()
		{
			ZDecimal customDecimal2 = new ZDecimal(1);
			Delivery.J4_CustomDecimal2 = customDecimal2;
			AssertEquals("CustomDecimal2", customDecimal2, DeliveryWrapper.CustomDecimal2);
		}

		public void TestCustomDecimal3()
		{
			ZDecimal customDecimal3 = new ZDecimal(1);
			Delivery.J4_CustomDecimal3 = customDecimal3;
			AssertEquals("CustomDecimal3", customDecimal3, DeliveryWrapper.CustomDecimal3);
		}

		public void TestCustomDecimal4()
		{
			ZDecimal customDecimal4 = new ZDecimal(1);
			Delivery.J4_CustomDecimal4 = customDecimal4;
			AssertEquals("CustomDecimal4", customDecimal4, DeliveryWrapper.CustomDecimal4);
		}

		public void TestCustomDecimal5()
		{
			ZDecimal customDecimal5 = new ZDecimal(1);
			Delivery.J4_CustomDecimal5 = customDecimal5;
			AssertEquals("CustomDecimal5", customDecimal5, DeliveryWrapper.CustomDecimal5);
		}

		#endregion

		#region ZDateTime Fields

		public void TestCustomDate1()
		{
			ZDateTime customDate1 = new ZDateTime(2004, 04, 04);
			Delivery.J4_CustomDate1 = customDate1;
			AssertEquals("CustomDate1", customDate1, DeliveryWrapper.CustomDate1);
		}

		public void TestCustomDate2()
		{
			ZDateTime customDate2 = new ZDateTime(2004, 04, 04);
			Delivery.J4_CustomDate2 = customDate2;
			AssertEquals("CustomDate2", customDate2, DeliveryWrapper.CustomDate2);
		}

		public void TestCustomDate3()
		{
			ZDateTime customDate3 = new ZDateTime(2004, 04, 04);
			Delivery.J4_CustomDate3 = customDate3;
			AssertEquals("CustomDate3", customDate3, DeliveryWrapper.CustomDate3);
		}

		public void TestCustomDate4()
		{
			ZDateTime customDate4 = new ZDateTime(2004, 04, 04);
			Delivery.J4_CustomDate4 = customDate4;
			AssertEquals("CustomDate4", customDate4, DeliveryWrapper.CustomDate4);
		}

		public void TestCustomDate5()
		{
			ZDateTime customDate5 = new ZDateTime(2004, 04, 04);
			Delivery.J4_CustomDate5 = customDate5;
			AssertEquals("CustomDate5", customDate5, DeliveryWrapper.CustomDate5);
		}

		#endregion

		#region ZBool Fields

		public void TestCustomFlag1()
		{
			Delivery.J4_CustomFlag1 = ZBool.False;
			Assert("!CustomFlag1", !DeliveryWrapper.CustomFlag1);

			Delivery.J4_CustomFlag1 = ZBool.True;
			Assert("CustomFlag1", DeliveryWrapper.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			Delivery.J4_CustomFlag2 = ZBool.False;
			Assert("!CustomFlag2", !DeliveryWrapper.CustomFlag2);

			Delivery.J4_CustomFlag2 = ZBool.True;
			Assert("CustomFlag2", DeliveryWrapper.CustomFlag2);
		}

		public void TestCustomFlag3()
		{
			Delivery.J4_CustomFlag3 = ZBool.False;
			Assert("!CustomFlag3", !DeliveryWrapper.CustomFlag3);

			Delivery.J4_CustomFlag3 = ZBool.True;
			Assert("CustomFlag3", DeliveryWrapper.CustomFlag3);
		}

		public void TestCustomFlag4()
		{
			Delivery.J4_CustomFlag4 = ZBool.False;
			Assert("!CustomFlag4", !DeliveryWrapper.CustomFlag4);

			Delivery.J4_CustomFlag4 = ZBool.True;
			Assert("CustomFlag4", DeliveryWrapper.CustomFlag4);
		}

		public void TestCustomFlag5()
		{
			Delivery.J4_CustomFlag5 = ZBool.False;
			Assert("!CustomFlag5", !DeliveryWrapper.CustomFlag5);

			Delivery.J4_CustomFlag5 = ZBool.True;
			Assert("CustomFlag5", DeliveryWrapper.CustomFlag5);
		}

		#endregion

		#region Implementation

		Order TheOrder;
		OrderLineDelivery Delivery;
		DocOrderLineDelivery DeliveryWrapper;

		protected override void SetUp()
		{
			TheOrder = Factory.New<Order>();
			var theLine = TheOrder.OrderLines.AddNew();
			Delivery = theLine.Deliveries.AddNew();
			DeliveryWrapper = DocOrderLineDelivery.New(Delivery, Factory);
			base.SetUp();
		}

		#endregion
	}
}
