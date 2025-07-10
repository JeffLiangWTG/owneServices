using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaScanForOutturnManagerTest : ScanForOutturnManagerTest
	{
		[TestDate(2013, 12, 12, 1, 1, 1)]
		public void TestSaveOutturnResult()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HB2";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "HLS1";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var consolOceanBill = Factory.New<CusSCAOceanBill>();
			consolOceanBill.CB_ParentId = consol.PK;
			consolOceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			consolOceanBill.CB_LloydsIMO = "12345";
			consolOceanBill.CB_Voyage = "123";
			consolOceanBill.CB_OceanBill = "OC123";

			var container1 = consolOceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "CN123";
			var house1 = consolOceanBill.HouseBills.AddNew();
			house1.CA_JS = shipment1.PK;
			house1.CA_HouseBill = "HB1";
			var pivot1 = container1.Pivots.AddNew();
			pivot1.CV_CA = house1.PK;
			pivot1.CV_PackageCount = 10;
			pivot1.CV_PackageType = "XX";
			pivot1.CV_GoodsDescription = "GOODS DESCRIPTION 1";
			pivot1.CV_MarksAndNumbers = "MARKS AND NUMBERS 1";
			var house2 = consolOceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "HB2";
			house2.CA_JS = shipment2.PK;
			var pivot2 = container1.Pivots.AddNew();
			pivot2.CV_CA = house2.PK;
			pivot2.CV_PackageCount = 10;
			pivot2.CV_PackageType = "XY";
			pivot2.CV_GoodsDescription = "GOODS DESCRIPTION 2";
			pivot2.CV_MarksAndNumbers = "MARK AND NUMBERS 2";

			var hlsShipment1 = consolOceanBill.HouseBills.AddNew();
			hlsShipment1.CA_HouseBill = "HLS1";
			hlsShipment1.CA_JS = shipment3.PK;
			var hlsShipment1pivot3 = container1.Pivots.AddNew();
			hlsShipment1pivot3.CV_CA = hlsShipment1.PK;
			hlsShipment1pivot3.CV_PackageCount = 10;
			hlsShipment1pivot3.CV_PackageType = "XY";
			hlsShipment1pivot3.CV_GoodsDescription = "GOODS DESCRIPTION 3";
			hlsShipment1pivot3.CV_MarksAndNumbers = "MARK AND NUMBERS 3";

			var underbond = container1.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "193K";
			var underbondLine = new SeaUnderbondSelectorLine(underbond);

			Factory.Save();

			var manager = new SeaScanForOutturnManager(new ScanCusSCAOceanBill(consolOceanBill));
			manager.SelectedUnderbond = underbondLine;
			manager.ScanWizardDataSource.ShipmentSelectorLineCollection.Cast<ShipmentSelectorLine>().First(x => x.Shipment == "All Standards").IncludeInScan = true;

			var sender = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals(SeaScanForOutturnManager.OutturnCollectionIsEmpty, manager.SaveOutturnResult(sender));

			var scan = new ManualScanLine();
			scan.Barcode = "HBS";
			manager.ManualScanHistory.Add(scan);
			manager.MergeManualScanResults();

			AssertEquals(SeaScanForOutturnManager.NoOutturnHeader, manager.SaveOutturnResult(sender));

			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_LloydsIMO = "12345";
			outturnHeader.C6_VoyageNum = "123";
			outturnHeader.C6_OutturningPremiseID = "193K";

			AssertEquals(SeaScanForOutturnManager.NoStandAloneSeaCargoForSurplusConsignment, manager.SaveOutturnResult(sender));

			var standaloneOceanBill = Factory.New<CusSCAOceanBill>();
			standaloneOceanBill.CB_Voyage = "123";
			standaloneOceanBill.CB_LloydsIMO = "12345";
			standaloneOceanBill.CB_OceanBill = "OC123";
			standaloneOceanBill.CB_MasterHouseBill = "HLS1";

			var container2 = standaloneOceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = "CN123";
			var house3 = standaloneOceanBill.HouseBills.AddNew();
			house3.CA_HouseBill = "HB3";
			var pivot3 = container2.Pivots.AddNew();
			pivot3.CV_CA = house3.PK;
			pivot3.CV_PackageCount = 10;
			pivot3.CV_PackageType = "XZ";
			pivot3.CV_GoodsDescription = "GOODS DESCRIPTION 3";
			pivot3.CV_MarksAndNumbers = "MARK AND NUMBERS 3";

			Factory.Save();

			manager = new SeaScanForOutturnManager(new ScanCusSCAOceanBill(consolOceanBill));
			manager.SelectedUnderbond = underbondLine;
			manager.ScanWizardDataSource.ShipmentSelectorLineCollection.Cast<ShipmentSelectorLine>().First(x => x.Shipment == "All Standards").IncludeInScan = true;
			manager.ScanWizardDataSource.ShipmentSelectorLineCollection.Cast<ShipmentSelectorLine>().First(x => x.Shipment == "HLS1").IncludeInScan = true;
			manager.SetSelectedShipment();

			scan = new ManualScanLine();
			scan.Barcode = "HBS";
			manager.ManualScanHistory.Add(scan);
			manager.MergeManualScanResults();

			var automatic = new SeaOutturnLineCollection(Factory);
			automatic.Add(new SeaOutturnLine() { ConsignmentRef = "HB1", Count = 9 });
			automatic.Add(new SeaOutturnLine() { ConsignmentRef = "HB2", Count = 10 });
			automatic.Add(new SeaOutturnLine() { ConsignmentRef = "HB3", Count = 11 });
			manager.MergeAutomaticScanResults(automatic);

			var surplusOutturnCollection = manager.ScanWizardDataSource.SurplusOutturnCollection;
			AssertEquals(1, surplusOutturnCollection.Count);
			AssertEquals("HBS", surplusOutturnCollection[0].ConsignmentRef);

			manager.SaveOutturnResult(sender);

			var surplusConsignment = standaloneOceanBill.HouseBills.Cast<CusSCAHouse>().FirstOrDefault(x => x.CA_HouseBill == "HBS");
			AssertNotNull(surplusConsignment);
			AssertEquals(CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, surplusConsignment.CA_ShipmentStatus);
			var surplusPivot = surplusConsignment.Pivot.Cast<CusSCAPivot>().FirstOrDefault(x => x.CN_ContainerNumber == "CN123");
			AssertNotNull(surplusPivot);
			AssertEquals("SURPLUS GOODS", surplusPivot.CV_GoodsDescription);
			AssertEquals(1, surplusPivot.CV_PackageCount);
			AssertEquals(CMRPackageTypes.Codes.Package, surplusPivot.CV_PackageType);

			AssertEquals(4, outturnHeader.Outturns.Count);
			AssertOutturnLine(outturnHeader.Outturns, "HB1", "OC123", "CN123", 10, "XX", pivot1.PK, CMROutturnResultType.Codes.ShortLanded, 9, ZString.Empty);
			AssertOutturnLine(outturnHeader.Outturns, "HB2", "OC123", "CN123", 10, "XY", pivot2.PK, CMROutturnResultType.Codes.NilDiscrepancy, 10, ZString.Empty);
			AssertOutturnLine(outturnHeader.Outturns, "HB3", "OC123", "CN123", 10, "XZ", pivot3.PK, CMROutturnResultType.Codes.SurplusPackages, 11, "MARK AND NUMBERS 3");
			AssertOutturnLine(outturnHeader.Outturns, "HBS", "OC123", "CN123", 1, CMRPackageTypes.Codes.Package, surplusPivot.PK, CMROutturnResultType.Codes.SurplusConsignment, 1, "SURPLUS GOODS");
		}

		[TestDate(2013, 12, 12, 1, 1, 1)]
		public void TestSaveOutturnResultWithMultipleHLSShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HB2";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "HLS1";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_HouseBill = "HLS2";
			shipment4.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var consolOceanBill = Factory.New<CusSCAOceanBill>();
			consolOceanBill.CB_ParentId = consol.PK;
			consolOceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			consolOceanBill.CB_LloydsIMO = "12345";
			consolOceanBill.CB_Voyage = "123";
			consolOceanBill.CB_OceanBill = "OC123";

			var container1 = consolOceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "CN123";
			var house1 = consolOceanBill.HouseBills.AddNew();
			house1.CA_JS = shipment1.PK;
			house1.CA_HouseBill = "HB1";
			var pivot1 = container1.Pivots.AddNew();
			pivot1.CV_CA = house1.PK;
			pivot1.CV_PackageCount = 10;
			pivot1.CV_PackageType = "XX";
			pivot1.CV_GoodsDescription = "GOODS DESCRIPTION 1";
			pivot1.CV_MarksAndNumbers = "MARKS AND NUMBERS 1";
			var house2 = consolOceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "HB2";
			house2.CA_JS = shipment2.PK;
			var pivot2 = container1.Pivots.AddNew();
			pivot2.CV_CA = house2.PK;
			pivot2.CV_PackageCount = 10;
			pivot2.CV_PackageType = "XY";
			pivot2.CV_GoodsDescription = "GOODS DESCRIPTION 2";
			pivot2.CV_MarksAndNumbers = "MARK AND NUMBERS 2";

			var hlsShipment1 = consolOceanBill.HouseBills.AddNew();
			hlsShipment1.CA_HouseBill = "HLS1";
			hlsShipment1.CA_JS = shipment3.PK;
			var hlsShipment1pivot3 = container1.Pivots.AddNew();
			hlsShipment1pivot3.CV_CA = hlsShipment1.PK;
			hlsShipment1pivot3.CV_PackageCount = 10;
			hlsShipment1pivot3.CV_PackageType = "XY";
			hlsShipment1pivot3.CV_GoodsDescription = "GOODS DESCRIPTION 3";
			hlsShipment1pivot3.CV_MarksAndNumbers = "MARK AND NUMBERS 3";

			var hlsShipment2 = consolOceanBill.HouseBills.AddNew();
			hlsShipment2.CA_HouseBill = "HLS2";
			hlsShipment2.CA_JS = shipment4.PK;
			var hlsShipment2pivot4 = container1.Pivots.AddNew();
			hlsShipment2pivot4.CV_CA = hlsShipment2.PK;
			hlsShipment2pivot4.CV_PackageCount = 10;
			hlsShipment2pivot4.CV_PackageType = "XY";
			hlsShipment2pivot4.CV_GoodsDescription = "GOODS DESCRIPTION 4";
			hlsShipment2pivot4.CV_MarksAndNumbers = "MARK AND NUMBERS 4";

			var underbond = container1.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "193K";
			var underbondLine = new SeaUnderbondSelectorLine(underbond);

			Factory.Save();

			var standaloneOceanBill = Factory.New<CusSCAOceanBill>();
			standaloneOceanBill.CB_Voyage = "123";
			standaloneOceanBill.CB_LloydsIMO = "12345";
			standaloneOceanBill.CB_OceanBill = "OC123";
			standaloneOceanBill.CB_MasterHouseBill = "HLS1";

			var container2 = standaloneOceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = "CN123";
			var house3 = standaloneOceanBill.HouseBills.AddNew();
			house3.CA_HouseBill = "HB3";
			var pivot3 = container2.Pivots.AddNew();
			pivot3.CV_CA = house3.PK;
			pivot3.CV_PackageCount = 10;
			pivot3.CV_PackageType = "XZ";
			pivot3.CV_GoodsDescription = "GOODS DESCRIPTION 3";
			pivot3.CV_MarksAndNumbers = "MARK AND NUMBERS 3";

			var standaloneOceanBill2 = Factory.New<CusSCAOceanBill>();
			standaloneOceanBill2.CB_Voyage = "123";
			standaloneOceanBill2.CB_LloydsIMO = "12345";
			standaloneOceanBill2.CB_OceanBill = "OC123";
			standaloneOceanBill2.CB_MasterHouseBill = "HLS2";

			var container3 = standaloneOceanBill2.Containers.AddNew();
			container3.CN_ContainerNumber = "CN123";
			var house4 = standaloneOceanBill2.HouseBills.AddNew();
			house4.CA_HouseBill = "HB4";
			var pivot4 = container3.Pivots.AddNew();
			pivot4.CV_CA = house4.PK;
			pivot4.CV_PackageCount = 10;
			pivot4.CV_PackageType = "XZ";
			pivot4.CV_GoodsDescription = "GOODS DESCRIPTION 4";
			pivot4.CV_MarksAndNumbers = "MARK AND NUMBERS 4";

			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_LloydsIMO = "12345";
			outturnHeader.C6_VoyageNum = "123";
			outturnHeader.C6_OutturningPremiseID = "193K";

			Factory.Save();

			var manager = new SeaScanForOutturnManager(new ScanCusSCAOceanBill(consolOceanBill));
			manager.SelectedUnderbond = underbondLine;
			manager.ScanWizardDataSource.ShipmentSelectorLineCollection.Cast<ShipmentSelectorLine>().First(x => x.Shipment == "All Standards").IncludeInScan = true;
			manager.ScanWizardDataSource.ShipmentSelectorLineCollection.Cast<ShipmentSelectorLine>().First(x => x.Shipment == "HLS1").IncludeInScan = true;
			manager.ScanWizardDataSource.ShipmentSelectorLineCollection.Cast<ShipmentSelectorLine>().First(x => x.Shipment == "HLS2").IncludeInScan = true;
			manager.SetSelectedShipment();
			var sender = new SendsMessagesToCustomsShutterUpperer(false);

			var scan = new ManualScanLine();
			scan.Barcode = "HBS";
			manager.ManualScanHistory.Add(scan);
			manager.MergeManualScanResults();

			var automatic = new SeaOutturnLineCollection(Factory);
			automatic.Add(new SeaOutturnLine() { ConsignmentRef = "HB1", Count = 9 });
			automatic.Add(new SeaOutturnLine() { ConsignmentRef = "HB2", Count = 10 });
			automatic.Add(new SeaOutturnLine() { ConsignmentRef = "HB3", Count = 11 });
			automatic.Add(new SeaOutturnLine() { ConsignmentRef = "HB4", Count = 11 });
			manager.MergeAutomaticScanResults(automatic);

			var surplusOutturnCollection = manager.ScanWizardDataSource.SurplusOutturnCollection;
			AssertEquals(1, surplusOutturnCollection.Count);
			AssertEquals("HBS", surplusOutturnCollection[0].ConsignmentRef);
			surplusOutturnCollection[0].Shipment = "HLS2";

			manager.SaveOutturnResult(sender);

			var surplusConsignment = standaloneOceanBill2.HouseBills.Cast<CusSCAHouse>().FirstOrDefault(x => x.CA_HouseBill == "HBS");
			AssertNotNull(surplusConsignment);
			AssertEquals(CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, surplusConsignment.CA_ShipmentStatus);
			var surplusPivot = surplusConsignment.Pivot.Cast<CusSCAPivot>().FirstOrDefault(x => x.CN_ContainerNumber == "CN123");
			AssertNotNull(surplusPivot);
			AssertEquals("SURPLUS GOODS", surplusPivot.CV_GoodsDescription);
			AssertEquals(1, surplusPivot.CV_PackageCount);
			AssertEquals(CMRPackageTypes.Codes.Package, surplusPivot.CV_PackageType);

			AssertEquals(5, outturnHeader.Outturns.Count);
			AssertOutturnLine(outturnHeader.Outturns, "HB1", "OC123", "CN123", 10, "XX", pivot1.PK, CMROutturnResultType.Codes.ShortLanded, 9, ZString.Empty);
			AssertOutturnLine(outturnHeader.Outturns, "HB2", "OC123", "CN123", 10, "XY", pivot2.PK, CMROutturnResultType.Codes.NilDiscrepancy, 10, ZString.Empty);
			AssertOutturnLine(outturnHeader.Outturns, "HB3", "OC123", "CN123", 10, "XZ", pivot3.PK, CMROutturnResultType.Codes.SurplusPackages, 11, "MARK AND NUMBERS 3");
			AssertOutturnLine(outturnHeader.Outturns, "HB4", "OC123", "CN123", 10, "XZ", pivot4.PK, CMROutturnResultType.Codes.SurplusPackages, 11, "MARK AND NUMBERS 4");
			AssertOutturnLine(outturnHeader.Outturns, "HBS", "OC123", "CN123", 1, CMRPackageTypes.Codes.Package, surplusPivot.PK, CMROutturnResultType.Codes.SurplusConsignment, 1, "SURPLUS GOODS");
		}

		void AssertOutturnLine(Customs.Business.CusOutturnHeaderCusOutturnCollection outturns, ZString housebill, ZString oceanBill,
			ZString containerNo, ZInt outterPack, ZString outterPackUnit, ZGuid parentID, ZString otturnResultType, ZInt outturned, ZString markAndNumbers)
		{
			var outturn = outturns.Cast<CusOutturn>().FirstOrDefault(x => x.C5_CargoType == CMRImportCargoTypes.Codes.LessThanContainerLoad && x.C5_HouseBill == housebill
				&& x.C5_MasterBill == oceanBill && x.C5_ContainerNumber == containerNo);
			AssertNotNull(outturn);
			AssertEquals(outterPack, outturn.C5_OuterPacks);
			AssertEquals(outterPackUnit, outturn.C5_OuterPackUnits);
			AssertEquals(parentID, outturn.C5_ParentID);
			AssertEquals(otturnResultType, outturn.C5_OutturnResultType);
			AssertEquals(outturned, outturn.C5_PackagesOutturned);
			AssertEquals(ZDateTime.Now, outturn.C5_CargoUnpackDate);
			AssertEquals(CusSCAPivotSchema.Constants.Prefix, outturn.C5_ParentTableCode);
			AssertEquals(markAndNumbers, outturn.C5_MarksAndNumbers);
		}

		public void TestMergeManualScanResults()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OC123";

			var container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "CN1234";
			var house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "HB1";
			var pivot1 = container1.Pivots.AddNew();
			pivot1.CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			pivot1.CV_CA = house1.PK;
			pivot1.CV_PackageCount = 10;
			var house2 = oceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "HB2";
			var pivot2 = container1.Pivots.AddNew();
			pivot2.CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			pivot2.CV_CA = house2.PK;
			pivot2.CV_PackageCount = 10;

			var underbond1 = container1.Underbonds.AddNew();
			var underbondSelectorLine1 = new SeaUnderbondSelectorLine(underbond1);

			var manager = new SeaScanForOutturnManager(new ScanCusSCAOceanBill(oceanBill));
			manager.SelectedUnderbond = underbondSelectorLine1;

			var scan1 = new ManualScanLine();
			scan1.Barcode = "HB1";
			var scan2 = new ManualScanLine();
			scan2.Barcode = "HB3";
			manager.ManualScanHistory.Add(scan1);
			manager.ManualScanHistory.Add(scan2);

			manager.MergeManualScanResults();
			Assert(manager.HasScanHappened);
			AssertEquals(0, manager.ManualScanHistory.Count);
			AssertEquals(3, manager.OutturnCollection.Count);
			AssertOutturnLine(manager.OutturnCollection, "HB1", ScanForOutturnManager.ManifestStatuses.Clear, house1, underbond1, 10, 1);
			AssertOutturnLine(manager.OutturnCollection, "HB2", ScanForOutturnManager.ManifestStatuses.Held, house2, underbond1, 10, 0);
			AssertOutturnLine(manager.OutturnCollection, "HB3", ScanForOutturnManager.ManifestStatuses.Held, null, null, 0, 1);
		}

		public void TestMergeAutomanticScanResult()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OC123";

			var container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "CN1234";
			var house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "HB1";
			var pivot1 = container1.Pivots.AddNew();
			pivot1.CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			pivot1.CV_CA = house1.PK;
			pivot1.CV_PackageCount = 10;
			var house2 = oceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "HB2";
			var pivot2 = container1.Pivots.AddNew();
			pivot2.CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			pivot2.CV_CA = house2.PK;
			pivot2.CV_PackageCount = 10;

			var underbond1 = container1.Underbonds.AddNew();
			var underbondSelectorLine1 = new SeaUnderbondSelectorLine(underbond1);

			var manager = new SeaScanForOutturnManager(new ScanCusSCAOceanBill(oceanBill));
			manager.SelectedUnderbond = underbondSelectorLine1;

			var scanResults = new SeaOutturnLineCollection(Factory);
			var scan1 = new SeaOutturnLine();
			scan1.Count = 3;
			scan1.ConsignmentRef = "HB1";
			var scan2 = new SeaOutturnLine();
			scan2.Count = 4;
			scan2.ConsignmentRef = "HB3";
			scanResults.Add(scan1);
			scanResults.Add(scan2);

			manager.MergeAutomaticScanResults(scanResults);
			Assert(manager.HasScanHappened);
			AssertEquals(3, manager.OutturnCollection.Count);
			AssertOutturnLine(manager.OutturnCollection, "HB1", ScanForOutturnManager.ManifestStatuses.Clear, house1, underbond1, 10, 3);
			AssertOutturnLine(manager.OutturnCollection, "HB2", ScanForOutturnManager.ManifestStatuses.Held, house2, underbond1, 10, 0);
			AssertOutturnLine(manager.OutturnCollection, "HB3", ScanForOutturnManager.ManifestStatuses.Held, null, null, 0, 4);
		}

		public void TestManifestCollection()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OC123";
			oceanBill.CB_MasterHouseBill = "HLS1";
			var container1 = oceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "CN1234";
			var house1 = oceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "HB1";
			var pivot1 = container1.Pivots.AddNew();
			pivot1.CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			pivot1.CV_CA = house1.PK;
			pivot1.CV_PackageCount = 1;
			var house2 = oceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "HB2";
			var pivot2 = container1.Pivots.AddNew();
			pivot2.CV_CargoStatus = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			pivot2.CV_CA = house2.PK;
			pivot2.CV_PackageCount = 2;

			var underbond1 = container1.Underbonds.AddNew();
			var underbondSelectorLine1 = new SeaUnderbondSelectorLine(underbond1);

			var manager = new SeaScanForOutturnManager(new ScanCusSCAOceanBill(oceanBill));
			manager.SelectedUnderbond = underbondSelectorLine1;

			AssertEquals(2, manager.ManifestCollection.Count);
			AssertOutturnLine(manager.ManifestCollection, "HB1", ScanForOutturnManager.ManifestStatuses.Clear, house1, underbond1, 1, 0);
			AssertOutturnLine(manager.ManifestCollection, "HB2", ScanForOutturnManager.ManifestStatuses.Held, house2, underbond1, 2, 0);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB3";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HB4";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "HLS1";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var consolOceanBill = Factory.New<CusSCAOceanBill>();
			consolOceanBill.CB_ParentId = Factory.New<ForwardingConsol>().PK;
			consolOceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			consolOceanBill.CB_OceanBill = "OC123";

			var container2 = consolOceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = "CN1234";
			var house3 = consolOceanBill.HouseBills.AddNew();
			house3.CA_HouseBill = "HB3";
			house3.CA_JS = shipment1.PK;
			var pivot3 = container2.Pivots.AddNew();
			pivot3.CV_CA = house3.PK;
			pivot3.CV_PackageCount = 3;
			var house4 = consolOceanBill.HouseBills.AddNew();
			house4.CA_HouseBill = "HB4";
			house4.CA_JS = shipment2.PK;
			var pivot4 = container2.Pivots.AddNew();
			pivot4.CV_CA = house4.PK;
			pivot4.CV_PackageCount = 4;

			var house5 = consolOceanBill.HouseBills.AddNew();
			house5.CA_HouseBill = "HLS1";
			house5.CA_JS = shipment3.PK;
			var pivot5 = container2.Pivots.AddNew();
			pivot5.CV_CA = house5.PK;
			pivot5.CV_PackageCount = 4;

			Factory.Save();

			var underbond2 = container2.Underbonds.AddNew();
			var underbondSelectorLine2 = new SeaUnderbondSelectorLine(underbond2);

			manager = new SeaScanForOutturnManager(new ScanCusSCAOceanBill(consolOceanBill));
			manager.SelectedUnderbond = underbondSelectorLine2;
			foreach (SeaShipmentSelectorLine shipmentSelectorLine in manager.ScanWizardDataSource.ShipmentSelectorLineCollection)
			{
				shipmentSelectorLine.IncludeInScan = true;
			}
			manager.SetSelectedShipment();
			AssertEquals(4, manager.ManifestCollection.Count);
			AssertOutturnLine(manager.ManifestCollection, "HB1", ScanForOutturnManager.ManifestStatuses.Clear, house1, underbond2, 1, 0);
			AssertOutturnLine(manager.ManifestCollection, "HB2", ScanForOutturnManager.ManifestStatuses.Held, house2, underbond2, 2, 0);
			AssertOutturnLine(manager.ManifestCollection, "HB3", ScanForOutturnManager.ManifestStatuses.Held, house3, underbond2, 3, 0);
			AssertOutturnLine(manager.ManifestCollection, "HB4", ScanForOutturnManager.ManifestStatuses.Held, house4, underbond2, 4, 0);
		}

		void AssertOutturnLine(OutturnLineCollection coll, ZString consignementRef, ScanForOutturnManager.ManifestStatuses status, CusSCAHouse house, CusUnderbond underbond, ZInt manifestQuan, ZInt count)
		{
			var line = coll.Cast<OutturnLine>().FirstOrDefault(x => x.ConsignmentRef == consignementRef);
			AssertNotNull(line);
			AssertEquals(status.ToString(), line.Status);
			AssertEquals(house, line.HouseBill);
			AssertEquals(underbond, line.Underbond);
			if (house != null)
			{
				AssertEquals(manifestQuan, line.ManifestInfo.Quantity);
			}
			AssertEquals(count, line.Count);
		}

		public void TestWatingResponseFromCusotmsError()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OC";
			oceanBill.CB_LloydsIMO = "12345";
			oceanBill.CB_Voyage = "123";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "CN123";
			var underbond = container.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "193K";

			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_LloydsIMO = "12345";
			outturnHeader.C6_VoyageNum = "123";
			outturnHeader.C6_OutturningPremiseID = "193K";
			var line = outturnHeader.Outturns.AddNew();
			line.C5_ContainerNumber = "CN123";
			line.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			line.C5_CargoReceiptDate = ZDateTime.Today;
			outturnHeader.C6_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			var underbondSelectorLine = new SeaUnderbondSelectorLine(underbond);
			underbondSelectorLine.UpdateStatuses();
			var manager = new SeaScanForOutturnManager(new ScanCusSCAOceanBill(oceanBill));
			manager.SelectedUnderbond = underbondSelectorLine;
			AssertContains(SeaScanForOutturnManager.WatingResponseFromCusotmsError, manager.ValidateSelectedUnderbond());

			outturnHeader.C6_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			underbondSelectorLine.UpdateStatuses();
			Assert(!manager.ValidateSelectedUnderbond().Contains(SeaScanForOutturnManager.WatingResponseFromCusotmsError));
		}

		#region Test ScanningForOutturnMutex

		protected override ScanForOutturnManager GetNewScanForOutturnManager(ScanMasterBill scanObj)
		{
			return new SeaScanForOutturnManager((ScanCusSCAOceanBill)scanObj);
		}

		protected override ScanMasterBill GetNewConsolScanMasterBill()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var oceanBill1 = Factory.New<CusSCAOceanBill>();
			oceanBill1.CB_OceanBill = "OC1";
			oceanBill1.CB_ParentId = consol.PK;
			oceanBill1.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var houseBill1 = oceanBill1.HouseBills.AddNew();
			houseBill1.CA_JS = shipment1.PK;
			houseBill1.CA_ShipmentStatus = "HLD";

			var houseBill2 = oceanBill1.HouseBills.AddNew();
			houseBill2.CA_JS = shipment2.PK;
			houseBill2.CA_ShipmentStatus = "CLR";

			var container = oceanBill1.Containers.AddNew();
			container.CN_ContainerNumber = "CN1";
			container.Pivots.AddNew().CV_CA = houseBill1.PK;
			container.Pivots.AddNew().CV_CA = houseBill2.PK;

			var underbond = Factory.NewWithValidTestData<CusUnderbond>();
			container.Underbonds.Add(underbond);

			var oceanBill2 = Factory.New<CusSCAOceanBill>();
			oceanBill2.CB_OceanBill = "OC1";
			oceanBill2.CB_MasterHouseBill = "HouseBill1";
			var houseBill3 = oceanBill2.HouseBills.AddNew();
			var container2 = oceanBill2.Containers.AddNew();
			container2.CN_ContainerNumber = "CN1";
			container2.Pivots.AddNew().CV_CA = houseBill3.PK;

			Factory.Save();

			return new ScanCusSCAOceanBill(oceanBill1);
		}

		#endregion

		protected override ScanMasterBill GetNewStandaloneScanMasterBill()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OC";
			return new ScanCusSCAOceanBill(oceanBill);
		}

		protected override OutturnLine GetNewOutturnLine()
		{
			return new SeaOutturnLine();
		}

		protected override void SetMasterHouseBillNumber(IScanMasterBillProvider masterBillProvider)
		{
			((CusSCAOceanBill)masterBillProvider).CB_MasterHouseBill = "123456";
		}
	}
}
