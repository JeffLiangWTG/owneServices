using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirShipmentSelectorLineGeneralTest : TestCaseWithFactory
	{
		public void TestShipmentSelectorLineContructor()
		{
			var cusHAWB = Factory.NewWithValidTestData<CusHAWB>();
			cusHAWB.CS_HAWB = "shipment1";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			cusHAWB.CS_JS = shipment.PK;
			cusHAWB.CS_ConsigneeName = "consignee";
			cusHAWB.CS_ConsignorName = "consignor";
			cusHAWB.CS_GoodsDescription = "GoodsDescription";

			var line = new AirShipmentSelectorLine();
			line.CreateStandAloneLine(cusHAWB);

			AssertEquals("shipment1", line.Shipment);
			AssertEquals("HLS", line.Type);
			AssertEquals("consignee", line.Consignee);
			AssertEquals("consignor", line.Consignor);
			AssertEquals("GoodsDescription", line.GoodsDescription);
			AssertEquals(1, line.CusHAWBs.Count());
			AssertEquals(cusHAWB.PK, line.CusHAWBs.First().PK);
		}

		public void TestShipmentSelectorLineStandartShipmentContructor()
		{
			var line = new AirShipmentSelectorLine();
			line.CreateStandardLine();
			AssertEquals("All Standards", line.Shipment);
			AssertEquals(Core.Constants.ShipmentTypes.StandardHouse, line.Type);
			AssertEquals("Various", line.Consignee);
			AssertEquals(0, line.CusHAWBs.Count());

			var cusHAWB1 = Factory.NewWithValidTestData<CusHAWB>();
			cusHAWB1.CS_HAWB = "shipment1";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			cusHAWB1.CS_JS = shipment.PK;
			cusHAWB1.CS_ConsigneeName = "consignee";
			line.AddStandardHouseBill(cusHAWB1);

			AssertEquals("All Standards", line.Shipment);
			AssertEquals(Core.Constants.ShipmentTypes.StandardHouse, line.Type);
			AssertEquals("Various", line.Consignee);

			AssertEquals(1, line.CusHAWBs.Count());
			AssertEquals(cusHAWB1.PK, line.CusHAWBs.First().PK);

			var cusHAWB2 = Factory.New<CusHAWB>();
			AssertExceptionThrown(typeof(InvalidOperationException), () => { line.AddStandardHouseBill(cusHAWB2); });

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			cusHAWB2.CS_JS = shipment.PK;
			AssertExceptionThrown(typeof(InvalidOperationException), () => { line.AddStandardHouseBill(cusHAWB2); });

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			line.AddStandardHouseBill(cusHAWB2);
			AssertEquals(2, line.CusHAWBs.Count());
			AssertEquals(1, line.CusHAWBs.Count(c => c.PK == cusHAWB2.PK));
		}

		public void TestUpdateStatusesStandardShipment()
		{
			var line = new AirShipmentSelectorLine();
			line.CreateStandardLine();
			AssertExceptionThrown(typeof(ArgumentNullException), () => line.UpdateStatuses(null));
			var selectedUnderbond = Factory.New<CusUnderbond>();
			line.UpdateStatuses(selectedUnderbond);

			var cusHAWB = Factory.NewWithValidTestData<CusHAWB>();
			cusHAWB.CS_HAWB = "shipment1";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			cusHAWB.CS_JS = shipment.PK;
			cusHAWB.CS_ConsigneeName = "consignee";
			line = new AirShipmentSelectorLine();
			line.CreateStandardLine();
			line.AddStandardHouseBill(cusHAWB);
			selectedUnderbond.OutturnStatus.Code = CMRBaseStatuses.Codes.NotSent;
			line.UpdateStatuses(selectedUnderbond);
			AssertEquals(nameof(OutturnStatus.ReadyForScanning), line.OutturnStatusText);
			AssertEquals(nameof(UnderbondStatus.NotSend), line.UnderbondStatusText);
		}

		public void TestUpdateStatusesHLSShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;
			var underbon1 = consolCusMAWB.Underbonds.AddNew();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBillStd1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HouseBillStd2";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "HouseBillHLS1";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_HouseBill = "HouseBillHLS2";
			shipment4.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var cusHAWB1 = CusHAWB.CreateNew(consolCusMAWB, shipment1);
			var cusHAWB2 = CusHAWB.CreateNew(consolCusMAWB, shipment2);
			var cusHAWB3 = CusHAWB.CreateNew(consolCusMAWB, shipment3);
			var cusHAWB4 = CusHAWB.CreateNew(consolCusMAWB, shipment4);

			consolCusMAWB.ChildBills.Add(cusHAWB1);
			consolCusMAWB.ChildBills.Add(cusHAWB2);
			consolCusMAWB.ChildBills.Add(cusHAWB3);
			consolCusMAWB.ChildBills.Add(cusHAWB4);

			var standAloneHouseBill2 = Factory.New<CusMAWB>();
			standAloneHouseBill2.CM_MAWB = "Master1";
			standAloneHouseBill2.CM_MasterHouseBill = "HouseBillHLS1";

			var standAloneHouseBill3 = Factory.New<CusMAWB>();
			standAloneHouseBill3.CM_MAWB = "Master1";
			standAloneHouseBill3.CM_MasterHouseBill = "HouseBillHLS2";

			var standAlone2Underbond = standAloneHouseBill2.Underbonds.AddNew();
			var standAlone3Underbond = standAloneHouseBill3.Underbonds.AddNew();

			standAlone2Underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			standAlone3Underbond.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;

			var line = new AirShipmentSelectorLine();
			line.CreateStandAloneLine(cusHAWB3);
			var selectedUnderbond = Factory.New<CusUnderbond>();
			selectedUnderbond.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			line.UpdateStatuses(underbon1);
			AssertEquals(nameof(OutturnStatus.Sent), line.OutturnStatusText);
			AssertEquals(nameof(UnderbondStatus.NotSend), line.UnderbondStatusText);
		}

		public void TestMaxLengthInShipmentSelectorLineIsTheSameAsInCusHAWB()
		{
			var line = new AirShipmentSelectorLine();
			line.CreateStandardLine();
			AssertEquals(CusHAWB.Schema.CS_HAWBMaxLength, line.ShipmentInfo.MaxLength);
			AssertEquals(CommonShipment.Schema.JS_ShipmentTypeMaxLength, line.TypeInfo.MaxLength);
			AssertEquals(CusHAWB.Schema.CS_ConsigneeNameMaxLength, line.ConsigneeInfo.MaxLength);
		}
	}
}
