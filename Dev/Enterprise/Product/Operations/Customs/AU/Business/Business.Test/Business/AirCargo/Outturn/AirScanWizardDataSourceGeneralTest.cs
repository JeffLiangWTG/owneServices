using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirScanWizardDataSourceGeneralTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new AirScanWizardDataSource(null));
			var source = new AirScanWizardDataSource(new ScanCusMAWB(Factory.New<CusMAWB>()));
		}

		public void TestProperties()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new AirScanWizardDataSource(null));
			var source = new AirScanWizardDataSource(new ScanCusMAWB(Factory.New<CusMAWB>()));
			Assert(!source.IsShipmentSelected);
		}

		public void TestUnderbondSelectorLineCollection()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var underbond1 = consolCusMAWB.Underbonds.AddNew();
			underbond1.C4_SendersMessageReference = "U1";
			underbond1.OutturnStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			underbond1.Outturns.AddNew();

			var underbond2 = consolCusMAWB.Underbonds.AddNew();
			underbond2.C4_SendersMessageReference = "U2";
			underbond2.OutturnStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
			underbond2.Outturns.AddNew();

			var underbond3 = consolCusMAWB.Underbonds.AddNew();
			underbond3.C4_SendersMessageReference = "U3";
			underbond3.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalRejected;
			underbond3.Outturns.AddNew();

			var source = new AirScanWizardDataSource(new ScanCusMAWB(consolCusMAWB));
			var collection = source.UnderbondSelectorLineCollection;
			AssertEquals(3, collection.Count);
			var line = collection.Cast<AirUnderbondSelectorLine>().ToList().First(l => l.Reference == "U1");
			AssertEquals(UnderbondStatus.FullySent, line.UnderbondStatus);

			line = collection.Cast<AirUnderbondSelectorLine>().ToList().First(l => l.Reference == "U2");
			AssertEquals(UnderbondStatus.WaitingCustomsResponse, line.UnderbondStatus);

			line = collection.Cast<AirUnderbondSelectorLine>().ToList().First(l => l.Reference == "U3");
			AssertEquals(UnderbondStatus.NotSend, line.UnderbondStatus);

			underbond3.OutturnStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			source.RefreshUnderbondStatuses();

			AssertEquals(UnderbondStatus.WaitingCustomsResponse, line.UnderbondStatus);
		}

		public void TestUnderbondSelectorLineCollectionStandAlone()
		{
			var cusMAWB = Factory.NewWithValidTestData<CusMAWB>();
			cusMAWB.CM_MAWB = "081223223232";
			var scanCusMAWB = new ScanCusMAWB(cusMAWB);
			var source = new AirScanWizardDataSource(scanCusMAWB);
			AssertNotNull(source.UnderbondSelectorLineCollection);
			AssertEquals(0, source.UnderbondSelectorLineCollection.Count);
		}

		public void TestUnderbondSelectorLineCollectionConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var consolCusMAWB = Factory.New<CusMAWB>();
			consolCusMAWB.CM_MAWB = "Master1";
			consolCusMAWB.CM_JK = consol.PK;

			var underbond1 = consolCusMAWB.Underbonds.AddNew();

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

			var scanCusMAWB = new ScanCusMAWB(consolCusMAWB);
			var source = new AirScanWizardDataSource(scanCusMAWB);
			scanCusMAWB.SelectedUnderbond = underbond1;
			AssertEquals(3, source.ShipmentSelectorLineCollection.Count);
			var shipmentLines = source.ShipmentSelectorLineCollection.Cast<AirShipmentSelectorLine>().ToList();
			var line = shipmentLines.First(l => l.Shipment == "All Standards");
			line.IncludeInScan = true;
			Assert(source.IsShipmentSelected);
			AssertEquals(2, line.CusHAWBs.Count());
			AssertEquals(nameof(OutturnStatus.ReadyForScanning), line.OutturnStatusText);
			AssertEquals(nameof(UnderbondStatus.NotSend), line.UnderbondStatusText);

			line = shipmentLines.First(l => l.Shipment == "HOUSEBILLHLS2");
			AssertEquals(1, line.CusHAWBs.Count());
			AssertEquals(nameof(OutturnStatus.Sent), line.OutturnStatusText);
			AssertEquals(nameof(UnderbondStatus.NotSend), line.UnderbondStatusText);

			var cusHAWB = standAloneHouseBill3.ChildBills.AddNew();
			cusHAWB.CS_PiecesManifested = 1;
			var outturn = standAlone3Underbond.Outturns.AddNew();
			outturn.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;
			outturn.C5_ParentID = cusHAWB.PK;
			standAloneHouseBill3.ResetDCLUnderbondCacheForTest();
			source.RefreshShipmentStatuses();
			line = source.ShipmentSelectorLineCollection.Cast<AirShipmentSelectorLine>().First(l => l.Shipment == "HOUSEBILLHLS2");
			AssertEquals(nameof(UnderbondStatus.FullySent), line.UnderbondStatusText);
		}
	}
}
