using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class ScanForOutturnHeldShipmentManagerTest<TMaster, THouse> : NonPersistentBusinessObjectTestCase
			where TMaster : IScanMasterBillProvider
			where THouse : EnterpriseBusinessObject, IScanHouseBillProvider
	{
		public void CountTotalNumberOfManualScansByBarcode()
		{
			var master1 = CreateMasterBill("MB1");
			var houseWithCAD = CreateHouseBill(master1, "B1", 2);
			houseWithCAD.Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-5));

			var master2 = CreateMasterBill("MB2");
			var houseWithCADOld = CreateHouseBill(master2, "B2", 1);
			houseWithCADOld.Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-15));

			Factory.Save();

			var manager = GetScanForOutturnHeldShipmentManager();
			var line1 = manager.ManualScanHistory.AddNew();
			line1.Barcode = "B1";
			var line2 = manager.ManualScanHistory.AddNew();
			line2.Barcode = "B2";
			var line3 = manager.ManualScanHistory.AddNew();
			line3.Barcode = "B3";
			var line4 = manager.ManualScanHistory.AddNew();
			line4.Barcode = "B1";

			AssertEquals(2, manager.CountTotalNumberOfManualScansByBarcode("B1"));
			AssertEquals(1, manager.CountTotalNumberOfManualScansByBarcode("B2"));
			AssertEquals(1, manager.CountTotalNumberOfManualScansByBarcode("B3"));
			AssertEquals(0, manager.CountTotalNumberOfManualScansByBarcode("B4"));
		}

		public void TestMergeManualScanResults()
		{
			var master1 = CreateMasterBill("MB1");
			var hawb1WithCAD = CreateHouseBill(master1, "B1", 2, CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased);
			hawb1WithCAD.Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-5));
			var hawb6WithCADClear = CreateHouseBill(master1, "B6", 1, CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased);
			((EnterpriseBusinessObject)hawb6WithCADClear.GetManifestInformation(null)).Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-14));

			var master2 = CreateMasterBill("MB2");
			var hawb2WithCADOld = CreateHouseBill(master2, "B2", 1, CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement);
			hawb2WithCADOld.Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-15));

			var master3 = CreateMasterBill("MB3");
			var hawb3WithCAD = CreateHouseBill(master3, "B3", 2, CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation);
			hawb3WithCAD.Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-5));
			var hawb6WithCADHeld = CreateHouseBill(master3, "B6", 1, CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased);
			((EnterpriseBusinessObject)hawb6WithCADHeld.GetManifestInformation(null)).Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-15));

			Factory.Save();

			var manager = GetScanForOutturnHeldShipmentManager();
			AssertEquals(false, manager.HasScanHappened);
			AssertEquals("", manager.MergeManualScanResults());
			AssertEquals(false, manager.HasScanHappened);

			manager = GetScanForOutturnHeldShipmentManager();
			var line1 = manager.ManualScanHistory.AddNew();
			line1.Barcode = "B1";
			line1.Instruction = "Release";
			var line2 = manager.ManualScanHistory.AddNew();
			line2.Barcode = "B2";
			line2.Instruction = "Release";
			var line3 = manager.ManualScanHistory.AddNew();
			line3.Barcode = "B3";
			line3.Instruction = "Held";
			var line4 = manager.ManualScanHistory.AddNew();
			line4.Barcode = "B1";
			line4.Instruction = "Release";
			var line5 = manager.ManualScanHistory.AddNew();
			line5.Barcode = "B5";
			line5.Instruction = "Release";
			var line6 = manager.ManualScanHistory.AddNew();
			line6.Barcode = "B6";
			line6.Instruction = "Release";

			AssertEquals("The following consignments could not be matched.\r\nEither they were not outturned in the selectecd establishment,\r\nor they do not have a 'CAD' event (not outturned),\r\nor might already have an 'RLD' event (ready for delivery).\r\n\r\nB5", manager.MergeManualScanResults());
			AssertEquals(6, manager.NumberOfManualParcelScanned);
			AssertEquals(0, manager.ManualScanHistory.Count);
			AssertEquals(true, manager.HasScanHappened);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ReadyForLocalDeliveryCode);
			query.AddToFilter(StmALogSchema.SL_Parent, hawb1WithCAD.GetManifestInformation(null).PK);
			AssertEquals("One new ReadyForLocalDelivery Event added", 1, Factory.Load<StmALog>(query).Length);

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ReadyForLocalDeliveryCode);
			query.AddToFilter(StmALogSchema.SL_Parent, hawb2WithCADOld.GetManifestInformation(null).PK);
			AssertEquals("New ReadyForLocalDelivery Event added even though it is old", 1, Factory.Load<StmALog>(query).Length);

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ReadyForLocalDeliveryCode);
			query.AddToFilter(StmALogSchema.SL_Parent, hawb3WithCAD.GetManifestInformation(null).PK);
			AssertEquals("No new ReadyForLocalDelivery Event added as it is held", 0, Factory.Load<StmALog>(query).Length);

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ReadyForLocalDeliveryCode);
			query.AddToFilter(StmALogSchema.SL_Parent, hawb6WithCADClear.GetManifestInformation(null).PK);
			AssertEquals("Should update latter CusHAWB", 1, Factory.Load<StmALog>(query).Length);
		}

		[TestDate(2013, 5, 2, 10, 30, 45)]
		public void TestExportFileName()
		{
			var manager = GetScanForOutturnHeldShipmentManager();
			AssertEquals("eManifest_HeldShipments_201305021030.csv", manager.ExportFileName);
		}

		[TestDate(2013, 5, 2, 10, 30, 45)]
		public void TestMergeAutomaticScanResults()
		{
			var master1 = CreateMasterBill("MB1");
			var hawb1WithCAD = CreateHouseBill(master1, "B1", 2, CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, true);
			hawb1WithCAD.Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-5));
			var hawb6WithCADbutHeldOnInputFile = CreateHouseBill(master1, "B6", 1, CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement, true);
			hawb6WithCADbutHeldOnInputFile.Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-5));
			var hawb7WithCADbutHeld = CreateHouseBill(master1, "B7", 1, CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms, true);
			hawb7WithCADbutHeld.Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-5));
			var hawb8WithCADbutHeld = CreateHouseBill(master1, "B8", 1, CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine, true);
			hawb8WithCADbutHeld.Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-5));

			var master2 = CreateMasterBill("MB2");
			var hawb2WithCADOld = CreateHouseBill(master2, "B2", 1, CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement);
			hawb2WithCADOld.Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-15));

			var master3 = CreateMasterBill("MB3");
			var hawb3WithCAD = CreateHouseBill(master2, "B3", 2, CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, true);
			hawb3WithCAD.Logs.AddNew(Events.CargoReceivedAtDepot, ZDateTimeOffset.Now.AddDays(-5));
			Factory.Save();

			var manager = GetScanForOutturnHeldShipmentManager();
			AssertEquals(false, manager.HasScanHappened);
			AssertEquals("", manager.MergeAutomaticScanResults(null));
			AssertEquals(false, manager.HasScanHappened);
			var collection = manager.GetNewOutturnLineCollection();
			AssertEquals("", manager.MergeAutomaticScanResults(collection));
			AssertEquals(false, manager.HasScanHappened);

			AssertEquals("collection contains ? selected CusHAWBs", 5, manager.ManifestCollection.Count);

			var bill = manager.ManifestCollection.FindByConsignmentRef("B1");
			AssertNotNull(bill);
			AssertEquals("b1", "B1", bill.ConsignmentRef);
			AssertEquals("b1 count", 0, bill.Count);
			AssertEquals("b1 status", "Clear", bill.Status);
			bill = manager.ManifestCollection.FindByConsignmentRef("B6");
			AssertNotNull(bill);
			AssertEquals("b6", "B6", bill.ConsignmentRef);
			AssertEquals("b6 count", 0, bill.Count);
			AssertEquals("b6 status", "Clear", bill.Status);
			bill = manager.ManifestCollection.FindByConsignmentRef("B7");
			AssertNotNull(bill);
			AssertEquals("b7", "B7", bill.ConsignmentRef);
			AssertEquals("b7 count", 0, bill.Count);
			AssertEquals("b7 status", "Held", bill.Status);
			bill = manager.ManifestCollection.FindByConsignmentRef("B8");
			AssertNotNull(bill);
			AssertEquals("b8", "B8", bill.ConsignmentRef);
			AssertEquals("b8 count", 0, bill.Count);
			AssertEquals("b8 status", "Held", bill.Status);
			bill = manager.ManifestCollection.FindByConsignmentRef("B3");
			AssertNotNull(bill);
			AssertEquals("b3", "B3", bill.ConsignmentRef);
			AssertEquals("b3 count", 0, bill.Count);
			AssertEquals("b3 status", "Clear", bill.Status);

			var line1 = collection.AddNew();
			line1.ConsignmentRef = "B1";
			line1.Count = 1;
			line1.Status = "Clear";
			var line2 = collection.AddNew();
			line2.ConsignmentRef = "B2";
			line2.Count = 1;
			line2.Status = "Held";
			var line3 = collection.AddNew();
			line3.ConsignmentRef = "B3";
			line3.Count = 0;
			line3.Status = "Clear";
			var line4 = collection.AddNew();
			line4.ConsignmentRef = "B1";
			line4.Count = 1;
			line4.Status = "Clear";
			var line5 = collection.AddNew();
			line5.ConsignmentRef = "B5";
			line5.Count = 1;
			line5.Status = "Held";
			var line6 = collection.AddNew();
			line6.ConsignmentRef = "B4";
			line6.Count = 1;
			line6.Status = "Held";
			var line7 = collection.AddNew();
			line7.ConsignmentRef = "B6";
			line7.Count = 1;
			line7.Status = "Held";

			AssertEquals("The following consignments could not be matched.\r\nEither they were not outturned in the selectecd establishment,\r\nor they do not have a 'CAD' event (not outturned),\r\nor might already have an 'RLD' event (ready for delivery).\r\n\r\nB5\r\nB4", manager.MergeAutomaticScanResults(collection));
			AssertEquals(true, manager.HasScanHappened);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ReadyForLocalDeliveryCode);
			query.AddToFilter(StmALogSchema.SL_Parent, hawb1WithCAD.GetManifestInformation(null).PK);
			AssertEquals("One new ReadyForLocalDelivery Event added", 1, Factory.Load<StmALog>(query).Length);

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ReadyForLocalDeliveryCode);
			query.AddToFilter(StmALogSchema.SL_Parent, hawb6WithCADbutHeldOnInputFile.GetManifestInformation(null).PK);
			AssertEquals("No new ReadyForLocalDelivery Event added as status was Held when file produced", 0, Factory.Load<StmALog>(query).Length);

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ReadyForLocalDeliveryCode);
			query.AddToFilter(StmALogSchema.SL_Parent, hawb2WithCADOld.GetManifestInformation(null).PK);
			AssertEquals("No new ReadyForLocalDelivery Event added as it's old", 0, Factory.Load<StmALog>(query).Length);

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CargoReceivedAtDepotCode);
			query.AddToFilter(StmALogSchema.SL_Parent, hawb2WithCADOld.GetManifestInformation(null).PK);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			var b2CADEvents = Factory.Load<StmALog>(query);
			AssertEquals("Only one active CAD on B2", 1, b2CADEvents.Length);
			AssertEquals("Date has been reset to today", ZDate.Today, b2CADEvents[0].SL_EventTime.Date);
			AssertEquals("Reference has been set", "Reset by HELD scan", b2CADEvents[0].SL_Reference);
			AssertIsHeldAtOutturn(hawb2WithCADOld);

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ReadyForLocalDeliveryCode);
			query.AddToFilter(StmALogSchema.SL_Parent, hawb3WithCAD.GetManifestInformation(null).PK);
			AssertEquals("No new ReadyForLocalDelivery Event added as count is 0", 0, Factory.Load<StmALog>(query).Length);
		}

		public void TestIsClearStatusForScanOutturn()
		{
			var master1 = CreateMasterBill("MB1");
			var hawb1 = CreateHouseBill(master1, "HB1", 2, CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, true);
			var hawb2 = CreateHouseBill(master1, "HB2", 2, CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation, true);
			Factory.Save();

			CONClearReleaseStatusRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.AUCustoms.CondClearReleaseStatus.Clear);
			var manager = GetScanForOutturnHeldShipmentManager();
			AssertEquals(2, manager.ManifestCollection.Count);
			AssertEquals(nameof(ScanForOutturnManager.ManifestStatuses.Clear), manager.ManifestCollection[0].Status);
			AssertEquals(nameof(ScanForOutturnManager.ManifestStatuses.Clear), manager.ManifestCollection[1].Status);

			CONClearReleaseStatusRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.AUCustoms.CondClearReleaseStatus.Held);
			manager = GetScanForOutturnHeldShipmentManager();
			AssertEquals(2, manager.ManifestCollection.Count);
			AssertEquals(nameof(ScanForOutturnManager.ManifestStatuses.Clear), manager.ManifestCollection[0].Status);
			AssertEquals(nameof(ScanForOutturnManager.ManifestStatuses.Held), manager.ManifestCollection[1].Status);
		}

		THouse CreateHouseBill(TMaster master, string billNo, int manifestQuanity)
		{
			return CreateHouseBill(master, billNo, manifestQuanity, string.Empty, false);
		}

		THouse CreateHouseBill(TMaster master, string billNo, int manifestQuanity, string customsStatus)
		{
			return CreateHouseBill(master, billNo, manifestQuanity, customsStatus, false);
		}

		protected abstract ScanForOutturnHeldShipmentManager GetScanForOutturnHeldShipmentManager();
		protected abstract THouse CreateHouseBill(TMaster master, string billNo, int manifestQuantity, string customsStatus, bool isHeld);
		protected abstract TMaster CreateMasterBill(string masterBillNo);
		protected abstract void AssertIsHeldAtOutturn(THouse house);
		protected abstract CodePairRegistryItem CONClearReleaseStatusRegistry { get; }
	}
}
