using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SurplusOutturnLine))]
	sealed class SurplusOutturnLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			SurplusOutturnLine surplusOutturnLine = new SurplusOutturnLine(null, null, Factory);
			return surplusOutturnLine;
		}

		public void TestShipmentList()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "HouseBill3";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var cusHAWB1 = CusHAWB.CreateNew(consolCusMAWB, shipment1);
			var cusHAWB2 = CusHAWB.CreateNew(consolCusMAWB, shipment2);
			var cusHAWB3 = CusHAWB.CreateNew(consolCusMAWB, shipment3);
			cusHAWB1.CS_ConsignorName = "Consignor 1";
			cusHAWB1.CS_GoodsDescription = "Goods1";
			cusHAWB2.CS_ConsignorName = "Consignor 2";
			cusHAWB2.CS_GoodsDescription = "Goods2";
			cusHAWB2.CS_ConsignorName = "Consignor 3";
			cusHAWB2.CS_GoodsDescription = "Goods3";

			var shipmentSelectorLineCollection = new ShipmentSelectorLineCollection(Factory);
			var shipmentSelectorLine1 = new AirShipmentSelectorLine();
			shipmentSelectorLine1.CreateStandAloneLine(cusHAWB1);
			var shipmentSelectorLine2 = new AirShipmentSelectorLine();
			shipmentSelectorLine2.CreateStandAloneLine(cusHAWB2);
			var shipmentSelectorLine3 = new AirShipmentSelectorLine();
			shipmentSelectorLine3.CreateStandAloneLine(cusHAWB3);
			shipmentSelectorLineCollection.Add(shipmentSelectorLine1);
			shipmentSelectorLineCollection.Add(shipmentSelectorLine2);
			shipmentSelectorLineCollection.Add(shipmentSelectorLine3);
			shipmentSelectorLine1.IncludeInScan = true;
			shipmentSelectorLine2.IncludeInScan = false;
			shipmentSelectorLine3.IncludeInScan = true;

			var surplusLine = new SurplusOutturnLine(null, shipmentSelectorLineCollection, Factory);
			Assert(surplusLine.ShipmentList.ContainsCode("HouseBill1"));
			Assert(surplusLine.ShipmentList.ContainsCode("HouseBill3"));

			surplusLine.Shipment = shipment1.JS_HouseBill;
			AssertEquals("Consignor 1 - Goods1", surplusLine.Description);
		}

		public void TestValidateShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "HouseBill3";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var cusHAWB1 = CusHAWB.CreateNew(consolCusMAWB, shipment1);
			var cusHAWB2 = CusHAWB.CreateNew(consolCusMAWB, shipment2);
			var cusHAWB3 = CusHAWB.CreateNew(consolCusMAWB, shipment3);
			cusHAWB1.CS_ConsignorName = "Consignor 1";
			cusHAWB1.CS_GoodsDescription = "Goods1";
			cusHAWB2.CS_ConsignorName = "Consignor 2";
			cusHAWB2.CS_GoodsDescription = "Goods2";
			cusHAWB2.CS_ConsignorName = "Consignor 3";
			cusHAWB2.CS_GoodsDescription = "Goods3";

			var shipmentSelectorLineCollection = new ShipmentSelectorLineCollection(Factory);
			var shipmentSelectorLine1 = new AirShipmentSelectorLine();
			shipmentSelectorLine1.CreateStandAloneLine(cusHAWB1);
			var shipmentSelectorLine2 = new AirShipmentSelectorLine();
			shipmentSelectorLine2.CreateStandAloneLine(cusHAWB2);
			shipmentSelectorLineCollection.Add(shipmentSelectorLine1);
			shipmentSelectorLineCollection.Add(shipmentSelectorLine2);
			shipmentSelectorLine1.IncludeInScan = true;

			var surplusLine = new SurplusOutturnLine(null, shipmentSelectorLineCollection, Factory);
			surplusLine.Shipment = ZString.Empty;
			AssertHasMessageErrorContaining(surplusLine.ShipmentInfo, MandatoryValidation.YouHaveNotEntered);

			surplusLine.Shipment = "##";
			AssertHasMessageErrorContaining(surplusLine.ShipmentInfo, ListValidation.InvalidCodeMessageError);

			surplusLine.Shipment = shipment1.JS_HouseBill;
			AssertNoMessageErrorContaining(surplusLine.ShipmentInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
