using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirScanForOutturnManagerTest : ScanForOutturnManagerTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new AirScanForOutturnManager(null); });
		}

		public void TestAutomaticAndManualScanning()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "HouseBill3";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_HouseBill = "HouseBill4";
			shipment4.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var cusHAWB1 = CusHAWB.CreateNew(consolCusMAWB, shipment1);
			var cusHAWB2 = CusHAWB.CreateNew(consolCusMAWB, shipment2);
			var cusHAWB3 = CusHAWB.CreateNew(consolCusMAWB, shipment3);
			var cusHAWB4 = CusHAWB.CreateNew(consolCusMAWB, shipment4);

			consolCusMAWB.ChildBills.Add(cusHAWB1);
			consolCusMAWB.ChildBills.Add(cusHAWB2);
			consolCusMAWB.ChildBills.Add(cusHAWB3);
			consolCusMAWB.ChildBills.Add(cusHAWB4);

			cusHAWB1.CS_PiecesManifested = 2;
			cusHAWB1.CS_GoodsDescription = "HouseBill1 Goods";
			cusHAWB1.CS_CustomsStatus = "HLD";

			cusHAWB2.CS_PiecesManifested = 4;
			cusHAWB2.CS_GoodsDescription = "HouseBill2 Goods";
			cusHAWB2.CS_CustomsStatus = "CLR";

			var underbond1Master1 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond1Master1.C4_FlightNo = "QF101";
			consolCusMAWB.Underbonds.Add(underbond1Master1);

			var underbond2Master1 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond2Master1.C4_FlightNo = "QF102";
			consolCusMAWB.Underbonds.Add(underbond2Master1);

			var standAloneHouseBill3 = Factory.New<CusMAWB>();
			standAloneHouseBill3.CM_MAWB = "Master1";
			standAloneHouseBill3.CM_MasterHouseBill = "HouseBill3";

			var houseBill3Child1 = standAloneHouseBill3.ChildBills.AddNew();
			houseBill3Child1.CS_HAWB = "HLV1HouseBill1";
			houseBill3Child1.CS_PiecesManifested = 3;
			houseBill3Child1.CS_GoodsDescription = "HLV1HouseBill1 Goods";
			houseBill3Child1.CS_CustomsStatus = "CLR";

			var houseBill3Child2 = standAloneHouseBill3.ChildBills.AddNew();
			houseBill3Child2.CS_HAWB = "HLV1HouseBill2";
			houseBill3Child2.CS_PiecesManifested = 5;
			houseBill3Child2.CS_GoodsDescription = "HLV1HouseBill2 Goods";
			houseBill3Child2.CS_CustomsStatus = "HLD";

			var houseBill3Child3 = standAloneHouseBill3.ChildBills.AddNew();
			houseBill3Child3.CS_HAWB = "HLV1HouseBill3";
			houseBill3Child3.CS_PiecesManifested = 2;
			houseBill3Child3.CS_GoodsDescription = "HLV1HouseBill3 Goods";
			houseBill3Child3.CS_CustomsStatus = "CLR";

			var standAloneHouseBill3Underbond1 = Factory.NewWithValidTestData<CusUnderbond>();
			standAloneHouseBill3Underbond1.C4_FlightNo = "QF101";
			standAloneHouseBill3.Underbonds.Add(standAloneHouseBill3Underbond1);

			var standAloneHouseBill4 = Factory.New<CusMAWB>();
			standAloneHouseBill4.CM_MAWB = "Master1";
			standAloneHouseBill4.CM_MasterHouseBill = "HouseBill4";

			var houseBill4Child1 = standAloneHouseBill4.ChildBills.AddNew();
			houseBill4Child1.CS_HAWB = "HLV2HouseBill1";
			houseBill3Child1.CS_PiecesManifested = 4;
			var houseBill4Child2 = standAloneHouseBill4.ChildBills.AddNew();
			houseBill4Child2.CS_HAWB = "HLV2HouseBill2";
			houseBill3Child1.CS_PiecesManifested = 1;
			var houseBill4Child3 = standAloneHouseBill4.ChildBills.AddNew();
			houseBill4Child3.CS_HAWB = "HLV2HouseBill3";
			houseBill3Child1.CS_PiecesManifested = 5;

			var standAloneHouseBill4Underbond1 = Factory.NewWithValidTestData<CusUnderbond>();
			standAloneHouseBill4Underbond1.C4_FlightNo = "QF101";
			standAloneHouseBill4.Underbonds.Add(standAloneHouseBill4Underbond1);

			var manager = new AirScanForOutturnManager(new ScanCusMAWB(consolCusMAWB));
			Assert(!manager.IsStandaloneShipment);

			AssertEquals(2, manager.ScanWizardDataSource.UnderbondSelectorLineCollection.Count);
			AssertEquals("QF101", manager.ScanWizardDataSource.UnderbondSelectorLineCollection[0].FlightNumber);
			AssertEquals("QF102", manager.ScanWizardDataSource.UnderbondSelectorLineCollection[1].FlightNumber);

			manager.SelectedUnderbond = manager.ScanWizardDataSource.UnderbondSelectorLineCollection[0];
			manager.ValidateSelectedUnderbond();

			AssertEquals(3, manager.ScanWizardDataSource.ShipmentSelectorLineCollection.Count);

			var selectedShipments = manager.ScanWizardDataSource.ShipmentSelectorLineCollection.Cast<AirShipmentSelectorLine>().OrderBy(s => s.Shipment).ToArray();
			AssertEquals("All Standards", selectedShipments[0].Shipment);
			AssertEquals("HOUSEBILL3", selectedShipments[1].Shipment);
			AssertEquals("HOUSEBILL4", selectedShipments[2].Shipment);

			manager.ScanWizardDataSource.ShipmentSelectorLineCollection[0].IncludeInScan = true;
			manager.ScanWizardDataSource.ShipmentSelectorLineCollection[2].IncludeInScan = true;

			AssertEquals("", manager.ValidateStandAloneUnderbond());

			var standAloneHouseBill3Underbond2 = Factory.NewWithValidTestData<CusUnderbond>();
			standAloneHouseBill3Underbond2.C4_FlightNo = "QF102";
			standAloneHouseBill3.Underbonds.Add(standAloneHouseBill3Underbond2);

			var standAloneHouseBill4Underbond2 = Factory.NewWithValidTestData<CusUnderbond>();
			standAloneHouseBill4Underbond2.C4_FlightNo = "QF102";
			standAloneHouseBill4.Underbonds.Add(standAloneHouseBill4Underbond2);

			var manifestedCollection = manager.ManifestCollection;
			AssertEquals(5, manifestedCollection.Count);
			var manifests = manifestedCollection.Cast<AirOutturnLine>().OrderBy(l => l.ConsignmentRef).ToArray();
			AssertEquals("HLV1HouseBill1", manifests[0].ConsignmentRef);
			AssertEquals("HLV1HouseBill2", manifests[1].ConsignmentRef);
			AssertEquals("HLV1HouseBill3", manifests[2].ConsignmentRef);
			AssertEquals("HOUSEBILL1", manifests[3].ConsignmentRef);
			AssertEquals("HOUSEBILL2", manifests[4].ConsignmentRef);

			manifests[0].Count = 3;
			manifests[1].Count = 5;

			var outturnCollection = manager.OutturnCollection;
			AssertEquals(5, outturnCollection.Count);
			var outturns = outturnCollection.Cast<AirOutturnLine>().OrderBy(l => l.ConsignmentRef).ToArray();
			AssertEquals("HLV1HouseBill1", outturns[0].ConsignmentRef);
			AssertEquals("HLV1HouseBill2", outturns[1].ConsignmentRef);
			AssertEquals("HLV1HouseBill3", outturns[2].ConsignmentRef);
			AssertEquals("HOUSEBILL1", outturns[3].ConsignmentRef);
			AssertEquals("HOUSEBILL2", outturns[4].ConsignmentRef);

			AssertEquals(0, outturns[0].Count);
			AssertEquals(0, outturns[1].Count);

			var scannedCollection = manager.ManifestCollection.DeepCopy();
			((AirOutturnLine)scannedCollection[0]).Count = 3;
			((AirOutturnLine)scannedCollection[1]).Count = 4;
			((AirOutturnLine)scannedCollection[2]).Count = 3;
			((AirOutturnLine)scannedCollection[3]).Count = 1;
			((AirOutturnLine)scannedCollection[4]).Count = 1;

			var surplusLine = scannedCollection.AddNew();
			surplusLine.ConsignmentRef = "Surplus";
			surplusLine.Count = 2;

			manager.MergeAutomaticScanResults(scannedCollection);

			outturnCollection = manager.OutturnCollection;
			AssertEquals(6, outturnCollection.Count);
			outturns = outturnCollection.Cast<AirOutturnLine>().OrderBy(l => l.ConsignmentRef).ToArray();
			AssertEquals("HLV1HouseBill1", outturns[0].ConsignmentRef);
			AssertEquals(3, outturns[0].Count);
			AssertEquals("HLV1HouseBill2", outturns[1].ConsignmentRef);
			AssertEquals(4, outturns[1].Count);
			AssertEquals("HLV1HouseBill3", outturns[2].ConsignmentRef);
			AssertEquals(3, outturns[2].Count);
			AssertEquals("HOUSEBILL1", outturns[3].ConsignmentRef);
			AssertEquals(1, outturns[3].Count);
			AssertEquals("HOUSEBILL2", outturns[4].ConsignmentRef);
			AssertEquals(1, outturns[4].Count);
			AssertEquals("Surplus", outturns[5].ConsignmentRef);
			AssertEquals(2, outturns[5].Count);

			var surplusOutturnCollection = manager.ScanWizardDataSource.SurplusOutturnCollection;
			AssertEquals(1, surplusOutturnCollection.Count);

			AssertNotNull(manager.ManualScanHistory);
			ManualScanLine manualScanLine1 = manager.ManualScanHistory.AddNew();
			manualScanLine1.Barcode = "HLV1HouseBill2";

			ManualScanLine manualScanLine2 = manager.ManualScanHistory.AddNew();
			manualScanLine2.Barcode = "HLV1HouseBill2";

			ManualScanLine manualScanLine3 = manager.ManualScanHistory.AddNew();
			manualScanLine3.Barcode = "HOUSEBILL2";

			ManualScanLine manualScanLine4 = manager.ManualScanHistory.AddNew();
			manualScanLine4.Barcode = "Surplus1";

			ManualScanLine manualScanLine5 = manager.ManualScanHistory.AddNew();
			manualScanLine5.Barcode = "Surplus1";

			manager.MergeManualScanResults();

			AssertEquals(5, manager.NumberOfManualParcelScanned);

			outturnCollection = manager.OutturnCollection;
			AssertEquals(7, outturnCollection.Count);
			outturns = outturnCollection.Cast<AirOutturnLine>().OrderBy(l => l.ConsignmentRef).ToArray();
			AssertEquals("HLV1HouseBill1", outturns[0].ConsignmentRef);
			AssertEquals(3, outturns[0].Count);
			AssertEquals("HLV1HouseBill2", outturns[1].ConsignmentRef);
			AssertEquals(6, outturns[1].Count);
			AssertEquals("HLV1HouseBill3", outturns[2].ConsignmentRef);
			AssertEquals(3, outturns[2].Count);
			AssertEquals("HOUSEBILL1", outturns[3].ConsignmentRef);
			AssertEquals(1, outturns[3].Count);
			AssertEquals("HOUSEBILL2", outturns[4].ConsignmentRef);
			AssertEquals(2, outturns[4].Count);
			AssertEquals("Surplus", outturns[5].ConsignmentRef);
			AssertEquals(2, outturns[5].Count);
			AssertEquals("Surplus1", outturns[6].ConsignmentRef);
			AssertEquals(2, outturns[6].Count);

			surplusOutturnCollection = manager.ScanWizardDataSource.SurplusOutturnCollection;
			AssertEquals(2, surplusOutturnCollection.Count);

			surplusOutturnCollection[0].Shipment = "HouseBill3";
			surplusOutturnCollection[1].Shipment = "HouseBill3";

			manager.SetSelectedShipment();

			manager.SaveOutturnResult(null);

			AssertEquals(2, underbond1Master1.Outturns.Count);
			var underbondOutturns = underbond1Master1.Outturns.Cast<CusOutturn>().OrderBy(o => o.ParentStringRepresentation).ToArray();

			AssertEquals("HouseBill HOUSEBILL1", underbondOutturns[0].ParentStringRepresentation);
			AssertEquals(2, underbondOutturns[0].C5_OuterPacks);
			AssertEquals(1, underbondOutturns[0].C5_PackagesOutturned);
			AssertEquals(cusHAWB1.PK, underbondOutturns[0].C5_ParentID);
			AssertEquals("CS", underbondOutturns[0].C5_ParentTableCode);
			AssertEquals(underbond1Master1.PK, underbondOutturns[0].C5_C4_Underbond);
			AssertEquals("HELD - Cargo is held under Customs control", underbondOutturns[0].C5_CustomsStatus);
			AssertEquals("HouseBill1 Goods", underbondOutturns[0].C5_GoodsDescription);

			AssertEquals("HouseBill HOUSEBILL2", underbondOutturns[1].ParentStringRepresentation);
			AssertEquals(4, underbondOutturns[1].C5_OuterPacks);
			AssertEquals(2, underbondOutturns[1].C5_PackagesOutturned);
			AssertEquals(cusHAWB2.PK, underbondOutturns[1].C5_ParentID);
			AssertEquals("CS", underbondOutturns[1].C5_ParentTableCode);
			AssertEquals(underbond1Master1.PK, underbondOutturns[1].C5_C4_Underbond);
			AssertEquals("CLEAR - Cargo is free of any impediments and may be released.", underbondOutturns[1].C5_CustomsStatus);
			AssertEquals("HouseBill2 Goods", underbondOutturns[1].C5_GoodsDescription);

			AssertEquals(5, standAloneHouseBill3.ChildBills.Count);

			var houseBills = standAloneHouseBill3.ChildBills.Cast<CusHAWB>().OrderBy(b => b.CS_HAWB).ToArray();
			AssertEquals("HLV1HouseBill1", houseBills[0].CS_HAWB);
			AssertEquals("HLV1HouseBill2", houseBills[1].CS_HAWB);
			AssertEquals("HLV1HouseBill3", houseBills[2].CS_HAWB);
			var surplusHAWB = houseBills[3];
			var surplus1HAWB = houseBills[4];
			AssertEquals("Surplus", surplusHAWB.CS_HAWB);
			AssertEquals("Surplus1", surplus1HAWB.CS_HAWB);

			AssertEquals(5, standAloneHouseBill3Underbond1.Outturns.Count);
			var standAloneOutturns = standAloneHouseBill3Underbond1.Outturns.Cast<CusOutturn>().OrderBy(o => o.ParentStringRepresentation).ToArray();

			AssertEquals("HouseBill HLV1HouseBill1", standAloneOutturns[0].ParentStringRepresentation);
			AssertEquals(5, standAloneOutturns[0].C5_OuterPacks);
			AssertEquals(3, standAloneOutturns[0].C5_PackagesOutturned);
			AssertEquals(houseBill3Child1.PK, standAloneOutturns[0].C5_ParentID);
			AssertEquals("CS", standAloneOutturns[0].C5_ParentTableCode);
			AssertEquals(standAloneHouseBill3Underbond1.PK, standAloneOutturns[0].C5_C4_Underbond);
			AssertEquals("CLEAR - Cargo is free of any impediments and may be released.", standAloneOutturns[0].C5_CustomsStatus);
			AssertEquals("HLV1HouseBill1 Goods", standAloneOutturns[0].C5_GoodsDescription);

			AssertEquals("HouseBill HLV1HouseBill2", standAloneOutturns[1].ParentStringRepresentation);
			AssertEquals(5, standAloneOutturns[1].C5_OuterPacks);
			AssertEquals(6, standAloneOutturns[1].C5_PackagesOutturned);
			AssertEquals(houseBill3Child2.PK, standAloneOutturns[1].C5_ParentID);
			AssertEquals("CS", standAloneOutturns[1].C5_ParentTableCode);
			AssertEquals(standAloneHouseBill3Underbond1.PK, standAloneOutturns[1].C5_C4_Underbond);
			AssertEquals("HELD - Cargo is held under Customs control", standAloneOutturns[1].C5_CustomsStatus);
			AssertEquals("HLV1HouseBill2 Goods", standAloneOutturns[1].C5_GoodsDescription);

			AssertEquals("HouseBill HLV1HouseBill3", standAloneOutturns[2].ParentStringRepresentation);
			AssertEquals(2, standAloneOutturns[2].C5_OuterPacks);
			AssertEquals(3, standAloneOutturns[2].C5_PackagesOutturned);
			AssertEquals(houseBill3Child3.PK, standAloneOutturns[2].C5_ParentID);
			AssertEquals("CS", standAloneOutturns[2].C5_ParentTableCode);
			AssertEquals(standAloneHouseBill3Underbond1.PK, standAloneOutturns[2].C5_C4_Underbond);
			AssertEquals("CLEAR - Cargo is free of any impediments and may be released.", standAloneOutturns[2].C5_CustomsStatus);
			AssertEquals("HLV1HouseBill3 Goods", standAloneOutturns[2].C5_GoodsDescription);

			AssertEquals("HouseBill Surplus", standAloneOutturns[3].ParentStringRepresentation);
			AssertEquals(0, standAloneOutturns[3].C5_OuterPacks);
			AssertEquals(2, standAloneOutturns[3].C5_PackagesOutturned);
			AssertEquals(surplusHAWB.PK, standAloneOutturns[3].C5_ParentID);
			AssertEquals("CS", standAloneOutturns[3].C5_ParentTableCode);
			AssertEquals(standAloneHouseBill3Underbond1.PK, standAloneOutturns[3].C5_C4_Underbond);
			AssertEquals("HELD - Cargo is held under Customs control", standAloneOutturns[3].C5_CustomsStatus);
			AssertEquals("SURPLUS GOODS", standAloneOutturns[3].C5_GoodsDescription);

			AssertEquals("HouseBill Surplus1", standAloneOutturns[4].ParentStringRepresentation);
			AssertEquals(0, standAloneOutturns[4].C5_OuterPacks);
			AssertEquals(2, standAloneOutturns[4].C5_PackagesOutturned);
			AssertEquals(surplus1HAWB.PK, standAloneOutturns[4].C5_ParentID);
			AssertEquals("CS", standAloneOutturns[4].C5_ParentTableCode);
			AssertEquals(standAloneHouseBill3Underbond1.PK, standAloneOutturns[4].C5_C4_Underbond);
			AssertEquals("HELD - Cargo is held under Customs control", standAloneOutturns[4].C5_CustomsStatus);
			AssertEquals("SURPLUS GOODS", standAloneOutturns[4].C5_GoodsDescription);
		}

		public void TestAutomaticAndManualScanningForStandAloneShipment()
		{
			var standAloneHouseCusMAWB = Factory.New<CusMAWB>();
			standAloneHouseCusMAWB.CM_MAWB = "Master1";

			var cusHAWB1 = standAloneHouseCusMAWB.ChildBills.AddNew();
			var cusHAWB2 = standAloneHouseCusMAWB.ChildBills.AddNew();

			cusHAWB1.CS_HAWB = "HAWB1_12345678901234567890123456789";
			cusHAWB1.CS_PiecesManifested = 2;
			cusHAWB1.CS_GoodsDescription = "HAWB1 Goods";
			cusHAWB1.CS_CustomsStatus = "HLD";

			cusHAWB2.CS_HAWB = "HAWB2_12345678901234567890123456789";
			cusHAWB2.CS_PiecesManifested = 4;
			cusHAWB2.CS_GoodsDescription = "HAWB2 Goods";
			cusHAWB2.CS_CustomsStatus = "CLR";

			var underbond1 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond1.C4_FlightNo = "QF101";
			standAloneHouseCusMAWB.Underbonds.Add(underbond1);

			var manager = new AirScanForOutturnManager(new ScanCusMAWB(standAloneHouseCusMAWB));
			Assert(manager.IsStandaloneShipment);

			AssertEquals(1, manager.ScanWizardDataSource.UnderbondSelectorLineCollection.Count);
			AssertEquals("QF101", manager.ScanWizardDataSource.UnderbondSelectorLineCollection[0].FlightNumber);

			manager.SelectedUnderbond = manager.ScanWizardDataSource.UnderbondSelectorLineCollection[0];
			AssertEquals("", manager.ValidateSelectedUnderbond());
			AssertEquals("", manager.ValidateStandAloneUnderbond());

			var manifestedCollection = manager.ManifestCollection;
			AssertEquals(2, manifestedCollection.Count);
			var manifests = manifestedCollection.Cast<AirOutturnLine>().OrderBy(l => l.ConsignmentRef).ToArray();
			AssertEquals("HAWB1_12345678901234567890123456789", manifests[0].ConsignmentRef);
			AssertEquals("HAWB2_12345678901234567890123456789", manifests[1].ConsignmentRef);

			var outturnCollection = manager.OutturnCollection;
			AssertEquals(2, outturnCollection.Count);
			var outturns = outturnCollection.Cast<AirOutturnLine>().OrderBy(l => l.ConsignmentRef).ToArray();
			AssertEquals("HAWB1_12345678901234567890123456789", outturns[0].ConsignmentRef);
			AssertEquals("HAWB2_12345678901234567890123456789", outturns[1].ConsignmentRef);

			AssertEquals(0, outturns[0].Count);
			AssertEquals(0, outturns[1].Count);

			var scannedCollection = manager.ManifestCollection.DeepCopy();
			((AirOutturnLine)scannedCollection[0]).Count = 3;
			((AirOutturnLine)scannedCollection[1]).Count = 4;

			var surplusLine = scannedCollection.AddNew();
			surplusLine.ConsignmentRef = "Surplus1_45678901234567890123456789";
			surplusLine.Count = 2;

			manager.MergeAutomaticScanResults(scannedCollection);

			outturnCollection = manager.OutturnCollection;
			AssertEquals(3, outturnCollection.Count);
			outturns = outturnCollection.Cast<AirOutturnLine>().OrderBy(l => l.ConsignmentRef).ToArray();
			AssertEquals("HAWB1_12345678901234567890123456789", outturns[0].ConsignmentRef);
			AssertEquals(3, outturns[0].Count);
			AssertEquals("HAWB2_12345678901234567890123456789", outturns[1].ConsignmentRef);
			AssertEquals(4, outturns[1].Count);
			AssertEquals("Surplus1_45678901234567890123456789", outturns[2].ConsignmentRef);
			AssertEquals(2, outturns[2].Count);

			AssertNotNull(manager.ManualScanHistory);
			ManualScanLine manualScanLine1 = manager.ManualScanHistory.AddNew();
			manualScanLine1.Barcode = "HAWB2_12345678901234567890123456789";

			ManualScanLine manualScanLine2 = manager.ManualScanHistory.AddNew();
			manualScanLine2.Barcode = "Surplus2_45678901234567890123456789";

			manager.MergeManualScanResults();

			AssertEquals(2, manager.NumberOfManualParcelScanned);
			AssertEquals(4, outturnCollection.Count);

			outturnCollection = manager.OutturnCollection;
			outturns = outturnCollection.Cast<AirOutturnLine>().OrderBy(l => l.ConsignmentRef).ToArray();
			AssertEquals("HAWB1_12345678901234567890123456789", outturns[0].ConsignmentRef);
			AssertEquals(3, outturns[0].Count);
			AssertEquals("HAWB2_12345678901234567890123456789", outturns[1].ConsignmentRef);
			AssertEquals(5, outturns[1].Count);
			AssertEquals("Surplus1_45678901234567890123456789", outturns[2].ConsignmentRef);
			AssertEquals(2, outturns[2].Count);
			AssertEquals("Surplus2_45678901234567890123456789", outturns[3].ConsignmentRef);
			AssertEquals(1, outturns[3].Count);

			manager.SaveOutturnResult(null);
			AssertEquals(4, underbond1.Outturns.Count);
			var underbondOutturns = underbond1.Outturns.Cast<CusOutturn>().OrderBy(o => o.ParentStringRepresentation).ToArray();

			var houseBills = standAloneHouseCusMAWB.ChildBills.Cast<CusHAWB>().OrderBy(b => b.CS_HAWB).ToArray();
			AssertEquals("HAWB1_12345678901234567890123456789", houseBills[0].CS_HAWB);
			AssertEquals("HAWB2_12345678901234567890123456789", houseBills[1].CS_HAWB);
			var surplus1HAWB = houseBills[2];
			var surplus2HAWB = houseBills[3];
			AssertEquals("Surplus1_45678901234567890123456789", surplus1HAWB.CS_HAWB);
			AssertEquals("Surplus2_45678901234567890123456789", surplus2HAWB.CS_HAWB);

			AssertEquals("HouseBill HAWB1_12345678901234567890123456789", underbondOutturns[0].ParentStringRepresentation);
			AssertEquals(2, underbondOutturns[0].C5_OuterPacks);
			AssertEquals(3, underbondOutturns[0].C5_PackagesOutturned);
			AssertEquals(cusHAWB1.PK, underbondOutturns[0].C5_ParentID);
			AssertEquals("CS", underbondOutturns[0].C5_ParentTableCode);
			AssertEquals(underbond1.PK, underbondOutturns[0].C5_C4_Underbond);
			AssertEquals("HELD - Cargo is held under Customs control", underbondOutturns[0].C5_CustomsStatus);
			AssertEquals("HAWB1 Goods", underbondOutturns[0].C5_GoodsDescription);

			AssertEquals("HouseBill HAWB2_12345678901234567890123456789", underbondOutturns[1].ParentStringRepresentation);
			AssertEquals(4, underbondOutturns[1].C5_OuterPacks);
			AssertEquals(5, underbondOutturns[1].C5_PackagesOutturned);
			AssertEquals(cusHAWB2.PK, underbondOutturns[1].C5_ParentID);
			AssertEquals("CS", underbondOutturns[1].C5_ParentTableCode);
			AssertEquals(underbond1.PK, underbondOutturns[1].C5_C4_Underbond);
			AssertEquals("CLEAR - Cargo is free of any impediments and may be released.", underbondOutturns[1].C5_CustomsStatus);
			AssertEquals("HAWB2 Goods", underbondOutturns[1].C5_GoodsDescription);

			AssertEquals("HouseBill Surplus1_45678901234567890123456789", underbondOutturns[2].ParentStringRepresentation);
			AssertEquals(0, underbondOutturns[2].C5_OuterPacks);
			AssertEquals(2, underbondOutturns[2].C5_PackagesOutturned);
			AssertEquals(surplus1HAWB.PK, underbondOutturns[2].C5_ParentID);
			AssertEquals("CS", underbondOutturns[2].C5_ParentTableCode);
			AssertEquals(underbond1.PK, underbondOutturns[2].C5_C4_Underbond);
			AssertEquals("HELD - Cargo is held under Customs control", underbondOutturns[2].C5_CustomsStatus);
			AssertEquals("SURPLUS GOODS", underbondOutturns[2].C5_GoodsDescription);

			AssertEquals("HouseBill Surplus2_45678901234567890123456789", underbondOutturns[3].ParentStringRepresentation);
			AssertEquals(0, underbondOutturns[3].C5_OuterPacks);
			AssertEquals(1, underbondOutturns[3].C5_PackagesOutturned);
			AssertEquals(surplus2HAWB.PK, underbondOutturns[3].C5_ParentID);
			AssertEquals("CS", underbondOutturns[3].C5_ParentTableCode);
			AssertEquals(underbond1.PK, underbondOutturns[3].C5_C4_Underbond);
			AssertEquals("HELD - Cargo is held under Customs control", underbondOutturns[3].C5_CustomsStatus);
			AssertEquals("SURPLUS GOODS", underbondOutturns[3].C5_GoodsDescription);
		}

		public void TestCountTotalNumberOfManualScansByBarcode()
		{
			#region Prepare Test Data

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var cusHAWB1 = CusHAWB.CreateNew(consolCusMAWB, shipment1);
			consolCusMAWB.ChildBills.Add(cusHAWB1);

			cusHAWB1.CS_PiecesManifested = 2;
			cusHAWB1.CS_GoodsDescription = "HouseBill1 Goods";
			cusHAWB1.CS_CustomsStatus = "HLD";

			var underbond1Master1 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond1Master1.C4_FlightNo = "QF101";
			consolCusMAWB.Underbonds.Add(underbond1Master1);

			var standAloneMAWB = Factory.New<CusMAWB>();
			standAloneMAWB.CM_MAWB = "Master1";
			standAloneMAWB.CM_MasterHouseBill = "HouseBill1";

			var standAloneMAWBChild1 = standAloneMAWB.ChildBills.AddNew();
			standAloneMAWBChild1.CS_HAWB = "HLV1HouseBill1";
			standAloneMAWBChild1.CS_PiecesManifested = 3;
			standAloneMAWBChild1.CS_GoodsDescription = "HLV1HouseBill1 Goods";
			standAloneMAWBChild1.CS_CustomsStatus = "CLR";

			#endregion

			var manager = new AirScanForOutturnManager(new ScanCusMAWB(consolCusMAWB));

			manager.SelectedUnderbond = manager.ScanWizardDataSource.UnderbondSelectorLineCollection[0];
			manager.ValidateSelectedUnderbond();
			manager.ScanWizardDataSource.ShipmentSelectorLineCollection[0].IncludeInScan = true;

			var manifestedCollection = manager.ManifestCollection;
			AssertEquals(1, manifestedCollection.Count);
			var manifests = manifestedCollection.Cast<AirOutturnLine>().OrderBy(l => l.ConsignmentRef).ToArray();
			AssertEquals("HLV1HouseBill1", manifests[0].ConsignmentRef);

			var sampleBarcode = "HLV1HouseBill1";
			AssertEquals(0, manager.CountTotalNumberOfManualScansByBarcode(sampleBarcode));

			AssertNotNull(manager.ManualScanHistory);
			ManualScanLine manualScanLine1 = manager.ManualScanHistory.AddNew();
			manualScanLine1.Barcode = sampleBarcode;
			manager.MergeManualScanResults();

			AssertEquals(1, manager.CountTotalNumberOfManualScansByBarcode(sampleBarcode));

			ManualScanLine manualScanLine2 = manager.ManualScanHistory.AddNew();
			manualScanLine2.Barcode = sampleBarcode;

			AssertEquals(2, manager.CountTotalNumberOfManualScansByBarcode(sampleBarcode));
		}

		public void TestNoScanError()
		{
			#region Prepare Test Data

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var cusHAWB1 = CusHAWB.CreateNew(consolCusMAWB, shipment1);
			consolCusMAWB.ChildBills.Add(cusHAWB1);

			cusHAWB1.CS_PiecesManifested = 2;
			cusHAWB1.CS_GoodsDescription = "HouseBill1 Goods";
			cusHAWB1.CS_CustomsStatus = "HLD";

			var underbond1Master1 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond1Master1.C4_FlightNo = "QF101";
			consolCusMAWB.Underbonds.Add(underbond1Master1);

			var standAloneMAWB = Factory.New<CusMAWB>();
			standAloneMAWB.CM_MAWB = "Master1";
			standAloneMAWB.CM_MasterHouseBill = "HouseBill1";

			var standAloneMAWBChild1 = standAloneMAWB.ChildBills.AddNew();
			standAloneMAWBChild1.CS_HAWB = "HLV1HouseBill1";
			standAloneMAWBChild1.CS_PiecesManifested = 3;
			standAloneMAWBChild1.CS_GoodsDescription = "HLV1HouseBill1 Goods";
			standAloneMAWBChild1.CS_CustomsStatus = "CLR";

			#endregion

			var manager = new AirScanForOutturnManager(new ScanCusMAWB(consolCusMAWB));
			var manager2 = new AirScanForOutturnManager(new ScanCusMAWB(consolCusMAWB));

			manager.SelectedUnderbond = manager.ScanWizardDataSource.UnderbondSelectorLineCollection[0];
			manager.ValidateSelectedUnderbond();
			manager.ScanWizardDataSource.ShipmentSelectorLineCollection[0].IncludeInScan = true;

			Assert("No scans should have been loaded", !manager.HasScanHappened);
			ManualScanLine manualScanLine1 = manager.ManualScanHistory.AddNew();
			var sampleBarcode = "HLV1HouseBill1";
			manualScanLine1.Barcode = sampleBarcode;
			manager.MergeManualScanResults();
			Assert("Scan should have been loaded", manager.HasScanHappened);

			manager2.SelectedUnderbond = manager.ScanWizardDataSource.UnderbondSelectorLineCollection[0];
			manager2.ValidateSelectedUnderbond();
			manager2.ScanWizardDataSource.ShipmentSelectorLineCollection[0].IncludeInScan = true;

			Assert("No scans should have been loaded", !manager2.HasScanHappened);
			manager2.MergeManualScanResults();
			Assert("Empty scan should not cause load", !manager2.HasScanHappened);
		}

		protected override ScanMasterBill GetNewConsolScanMasterBill()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var cusHAWB1 = CusHAWB.CreateNew(consolCusMAWB, shipment1);
			var cusHAWB2 = CusHAWB.CreateNew(consolCusMAWB, shipment2);
			consolCusMAWB.ChildBills.Add(cusHAWB1);
			consolCusMAWB.ChildBills.Add(cusHAWB2);

			cusHAWB1.CS_PiecesManifested = 2;
			cusHAWB1.CS_GoodsDescription = "HouseBill1 Goods";
			cusHAWB1.CS_CustomsStatus = "HLD";

			cusHAWB2.CS_PiecesManifested = 4;
			cusHAWB2.CS_GoodsDescription = "HouseBill2 Goods";
			cusHAWB2.CS_CustomsStatus = "CLR";

			var underbond1Master1 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond1Master1.C4_FlightNo = "QF101";
			consolCusMAWB.Underbonds.Add(underbond1Master1);

			var underbond2Master1 = Factory.NewWithValidTestData<CusUnderbond>();
			underbond2Master1.C4_FlightNo = "QF102";
			consolCusMAWB.Underbonds.Add(underbond2Master1);

			return new ScanCusMAWB(consolCusMAWB);
		}

		protected override ScanForOutturnManager GetNewScanForOutturnManager(ScanMasterBill scanObj) => new AirScanForOutturnManager((ScanCusMAWB)scanObj);

		protected override ScanMasterBill GetNewStandaloneScanMasterBill()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "021212";
			return new ScanCusMAWB(cusMAWB);
		}

		protected override OutturnLine GetNewOutturnLine() => new AirOutturnLine();

		protected override void SetMasterHouseBillNumber(IScanMasterBillProvider masterBillProvider)
		{
			((CusMAWB)masterBillProvider).CM_MasterHouseBill = "123456";
		}
	}
}
