using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	public class CusHawbToShipmentSynchroniserTests : TestCaseWithFactory
	{
		public void TestFieldsAreSynchedByDefault()
		{
			var bridge = new CusHawbToShipmentSynchroniser(hAWB);

			hAWB.CS_OH_Consignee = ZGuid.NewZGuid();
			hAWB.CS_OH_Consignor = ZGuid.NewZGuid();

			ZDecimal actualWeight = 55m;
			ZString destination = "AUSYD";
			ZString goodsCurrency = "AUD";
			ZString goodsDescription = "Cuckoo Squeaker";  // GB --> 15 char limit
			ZDecimal goodsValue = 62m;
			ZString houseBillNumber = "HAWB123$%#* ";
			ZString origin = "USLAX";
			ZInt outerPacks = 2;
			ZString unitOfWeight = "KG";

			((IUpdateFromShipment)bridge).ActualWeight = actualWeight;
			((IUpdateFromShipment)bridge).Destination = destination;
			((IUpdateFromShipment)bridge).Origin = origin;
			((IUpdateFromShipment)bridge).GoodsDescription = goodsDescription;
			((IUpdateFromShipment)bridge).GoodsCurrency = goodsCurrency;
			((IUpdateFromShipment)bridge).GoodsValue = goodsValue;
			((IUpdateFromShipment)bridge).HouseBillNumber = houseBillNumber;
			((IUpdateFromShipment)bridge).OuterPacks = outerPacks;
			((IUpdateFromShipment)bridge).UnitOfWeight = unitOfWeight;

			AssertEquals(actualWeight, hAWB.CS_Weight);
			AssertEquals(destination, hAWB.CS_RL_NKDestination);
			AssertEquals(goodsCurrency, hAWB.CS_RX_NKGoodsCurrency);
			AssertEquals("CUCKOO SQUEAKER", hAWB.CS_GoodsDescription);
			AssertEquals(goodsValue, hAWB.CS_GoodsValue);
			AssertEquals("HAWB123$%#* ", hAWB.CS_HAWB);
			AssertHasMessageErrorContaining(hAWB.CS_HAWBInfo, "8 characters");
			AssertEquals(origin, hAWB.CS_RL_NKOrigin);
			AssertEquals(outerPacks, hAWB.CS_PiecesManifested);
			AssertEquals(unitOfWeight, hAWB.CS_WeightUQ);

			((IUpdateFromShipment)bridge).HouseBillNumber = "123456789";
			AssertHasMessageErrorContaining(hAWB.CS_HAWBInfo, "8 characters");

			// Fields open
			hAWB.SetCustomsActionCode(CustomsStatusCodes.Codes.EntryOrRequestCancelled, ZDateTime.Now);  // CX
			((IUpdateFromShipment)bridge).HouseBillNumber = "12345678";
			AssertEquals("CusHAWB.CS_HAWB updated by changes to bridge when Hawb is open", "12345678", hAWB.CS_HAWB);
			((IUpdateFromShipment)bridge).ActualWeight = 345m;
			AssertEquals("CusHAWB.CS_Weight updated by changes to bridge when Hawb is open", 345m, hAWB.CS_Weight);
			((IUpdateFromShipment)bridge).Destination = "SGSIN";
			AssertEquals("CusHAWB.CS_RL_NKDestination updated by changes to bridge when Hawb is open", "SGSIN", hAWB.CS_RL_NKDestination);
			((IUpdateFromShipment)bridge).Origin = "GBLHR";
			AssertEquals("CusHAWB.CS_RL_NKOrigin updated by changes to bridge when Hawb is open", "GBLHR", hAWB.CS_RL_NKOrigin);
			((IUpdateFromShipment)bridge).GoodsDescription = "STUFF";
			AssertEquals("CusHAWB.CS_GoodsDescription updated by changes to bridge when Hawb is open", "STUFF", hAWB.CS_GoodsDescription);
			((IUpdateFromShipment)bridge).OuterPacks = 3;
			AssertEquals("CusHAWB.CS_PiecesManifested updated by changes to bridge when Hawb is open", 3, (int)hAWB.CS_PiecesManifested);
			((IUpdateFromShipment)bridge).UnitOfWeight = "LB";
			AssertEquals("CusHAWB.CS_HAWB updated by changes to bridge when Hawb is open", "LB", hAWB.CS_WeightUQ);
			((IUpdateFromShipment)bridge).GoodsValue = 69m;
			AssertEquals("CusHAWB.CS_GoodsValue updated by changes to bridge when Hawb is open", 69m, hAWB.CS_GoodsValue);
			((IUpdateFromShipment)bridge).GoodsCurrency = "CAD";
			AssertEquals("CusHAWB.CS_RX_NKGoodsCurrency updated by changes to bridge when Hawb is open", "CAD", hAWB.CS_RX_NKGoodsCurrency);

			// Fields locked
			hAWB.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval, ZDateTime.Now);  // CT
			((IUpdateFromShipment)bridge).HouseBillNumber = "new";
			AssertEquals("CusHAWB.CS_HAWB not updated by changes to bridge when Hawb is locked", "12345678", hAWB.CS_HAWB);
			((IUpdateFromShipment)bridge).Destination = "new";
			AssertEquals("CusHAWB.CS_RL_NKDestination not updated by changes to bridge when Hawb is locked", "SGSIN", hAWB.CS_RL_NKDestination);
			((IUpdateFromShipment)bridge).Origin = "new";
			AssertEquals("CusHAWB.CS_RL_NKOrigin not updated by changes to bridge when Hawb is locked", "GBLHR", hAWB.CS_RL_NKOrigin);
			((IUpdateFromShipment)bridge).GoodsDescription = "new";
			AssertEquals("CusHAWB.CS_GoodsDescription not updated by changes to bridge when Hawb is locked", "STUFF", hAWB.CS_GoodsDescription);
			((IUpdateFromShipment)bridge).OuterPacks = 999;
			AssertEquals("CusHAWB.CS_PiecesManifested not updated by changes to bridge when Hawb is locked", 3, (int)hAWB.CS_PiecesManifested);
			((IUpdateFromShipment)bridge).UnitOfWeight = "XX";
			AssertEquals("CusHAWB.CS_HAWB not updated by changes to bridge when Hawb is locked", "LB", hAWB.CS_WeightUQ);
			((IUpdateFromShipment)bridge).ActualWeight = 999m;
			AssertEquals("CusHAWB.CS_Weight not updated by changes to bridge when Hawb is locked", 345m, hAWB.CS_Weight);

			((IUpdateFromShipment)bridge).GoodsValue = 999m;
			AssertEquals("CusHAWB.CS_GoodsValue updated by changes to bridge when Hawb is locked, this field not controlled", 999m, hAWB.CS_GoodsValue);
			((IUpdateFromShipment)bridge).GoodsCurrency = "GBP";
			AssertEquals("CusHAWB.CS_RX_NKGoodsCurrency updated by changes to bridge when Hawb is locked, this field not controlled", "GBP", hAWB.CS_RX_NKGoodsCurrency);
		}

		[TestDate(2015, 8, 22)]
		public void TestDoNotSynchOnceMessagesExist()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "GBLHR";
			consol.JK_MasterBillNum = "1112222222";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "H1111111";
			shipment.JS_RL_NKDestination = "GBLHR";
			shipment.JS_OuterPacks = 10;
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			mawb.NumberOfPiecesExpected = 10;
			mawb.NumberOfPiecesReceived = 10;
			mawb.Status1Date = ZDateTime.BrettsBirthday.AddDays(1);
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			hawb.CS_PiecesManifested = 10;
			hawb.CS_PiecesLanded = 10;
			hawb.Status1Date = ZDateTime.BrettsBirthday;
			hawb.Messages.AddNew();
			hawb.SynchroniseFromShipment(hawb.Shipment);
			AssertEquals("St1 is not clobbered upon synch from Forwarding", ZDateTime.BrettsBirthday, hawb.Status1Date);
			mawb.SynchroniseData();
			AssertEquals("St1 is not clobbered upon synch from Forwarding", ZDateTime.BrettsBirthday.AddDays(1), mawb.Status1Date);
		}

		CusMAWB mAWB;
		CusHAWB hAWB;
		protected override void SetUp()
		{
			base.SetUp();
			mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08111111111";
			mAWB.CM_FlightNo = "QF123";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 11, 12);
			mAWB.AirportOfDestination = "AUSYD";
			mAWB.AirportOfOrigin = "USLAX";
			hAWB = mAWB.ChildBills.AddNew();
		}
	}
}
