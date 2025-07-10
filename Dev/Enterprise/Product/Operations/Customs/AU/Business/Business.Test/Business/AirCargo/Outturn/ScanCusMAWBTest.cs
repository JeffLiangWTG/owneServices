using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ScanCusMAWBTest : ScanMasterBillTest
	{
		public void TestStandAloneMode()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new ScanCusMAWB(null));

			var cusMAWB = Factory.NewWithValidTestData<CusMAWB>();
			cusMAWB.CM_MAWB = "081223223232";

			var scanCusMAWB = new ScanCusMAWB(cusMAWB);
			AssertEquals(Factory, scanCusMAWB.Factory);
			AssertEquals("081223223232", scanCusMAWB.MasterBillNumber);
			AssertEquals(true, scanCusMAWB.IsStandAlone);
			AssertEquals(cusMAWB.CM_MAWB, scanCusMAWB.GetAirCargoToAddSurplusConsignment().MasterBillNumber);
			AssertEquals(cusMAWB.CM_MAWB, scanCusMAWB.GetAirCargoToAddSurplusConsignment().MasterBillNumber);
			AssertEquals(0, scanCusMAWB.GetChildBills().Count());

			var cusHAWB = cusMAWB.ChildBills.AddNew();
			cusHAWB.CS_MasterHouseBill = "HouseBill1";

			AssertEquals(1, scanCusMAWB.GetChildBills().Count());
			AssertEquals(cusHAWB, scanCusMAWB.GetChildBills().ElementAt(0));
			AssertNull(scanCusMAWB.Underbonds);
			AssertExceptionThrown(typeof(InvalidOperationException), () => scanCusMAWB.GetUnderbond());
			AssertNull(scanCusMAWB.GetStandAloneAirCargo(""));
			AssertEquals(cusMAWB.CM_MAWB, scanCusMAWB.GetAirCargoToAddSurplusConsignment().MasterBillNumber);
			AssertEquals("No Underbond Movement exists for Master '081223223232'. A Master Underbond Movement must exist for scanning. If this job was created after the underbond was approved then you will need to manually create a 'dummy' Master Underbond Movement with the same premise codes as in the original underbond. Note this record is for internal use only and, as the movement has already been reported to Customs, so you do not need to send the Underbond message for this dummy record.".Trim(), scanCusMAWB.Validate().Trim());

			var underbond = Factory.NewWithValidTestData<CusUnderbond>();
			cusMAWB.Underbonds.Add(underbond);
			AssertEquals(1, scanCusMAWB.Underbonds.Length);
			AssertEquals(underbond.PK, scanCusMAWB.Underbonds[0].PK);

			AssertEquals("No Underbond selected. Please select an underbond first.", scanCusMAWB.ValidateSelectedUnderbond().Trim());
			AssertExceptionThrown(typeof(InvalidOperationException), () => scanCusMAWB.ValidateStandAloneUnderbonds());
			scanCusMAWB.SelectedUnderbond = underbond;
			AssertEquals(underbond.PK, scanCusMAWB.GetUnderbond().PK);
			AssertEquals("", scanCusMAWB.Validate());
			AssertEquals("Flight Number for Underbond 'IOT36NGX2ARLS752OBR3' is empty. Please enter a Flight Number.", scanCusMAWB.ValidateSelectedUnderbond().Trim());

			underbond.C4_FlightNo = "QF100";
			AssertEquals("", scanCusMAWB.ValidateSelectedUnderbond().Trim());
			AssertEquals("", scanCusMAWB.ValidateStandAloneUnderbonds());
			AssertNull(scanCusMAWB.GetStandAloneAirCargo("HouseBill1"));
		}

		public void TestConsolMode()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

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

			consolCusMAWB.ChildBills.Add(cusHAWB1);
			consolCusMAWB.ChildBills.Add(cusHAWB2);
			consolCusMAWB.ChildBills.Add(cusHAWB3);

			var scanCusMAWB = new ScanCusMAWB(consolCusMAWB);

			AssertEquals(false, scanCusMAWB.IsStandAlone);
			AssertEquals(3, scanCusMAWB.GetChildBills().Count());
			AssertEquals(null, scanCusMAWB.Underbonds);

			AssertEquals("No Underbond Movement exists for Master 'Master1'. A Master Underbond Movement must exist for scanning. If this job was created after the underbond was approved then you will need to manually create a 'dummy' Master Underbond Movement with the same premise codes as in the original underbond. Note this record is for internal use only and, as the movement has already been reported to Customs, so you do not need to send the Underbond message for this dummy record.", scanCusMAWB.Validate().Trim());

			AssertEquals("No Underbond selected. Please select an underbond first.", scanCusMAWB.ValidateSelectedUnderbond().Trim());
			AssertExceptionThrown(typeof(InvalidOperationException), () => scanCusMAWB.ValidateStandAloneUnderbonds());
			AssertNull(scanCusMAWB.Underbonds);
			AssertExceptionThrown(typeof(InvalidOperationException), () => scanCusMAWB.GetUnderbond());
			AssertNull(scanCusMAWB.GetAirCargoToAddSurplusConsignment());
			AssertExceptionThrown(typeof(InvalidOperationException), () => scanCusMAWB.GetStandAloneAirCargo(""));

			var underbond1 = Factory.NewWithValidTestData<CusUnderbond>();
			consolCusMAWB.Underbonds.Add(underbond1);

			var underbond2 = Factory.NewWithValidTestData<CusUnderbond>();
			consolCusMAWB.Underbonds.Add(underbond2);

			scanCusMAWB.SelectedUnderbond = underbond2;
			AssertEquals("", scanCusMAWB.Validate());

			AssertEquals("Flight Number for Underbond '1JWQJASGGEZXUS8YX5T8' is empty. Please enter a Flight Number.", scanCusMAWB.ValidateSelectedUnderbond().Trim());
			AssertEquals("", scanCusMAWB.ValidateStandAloneUnderbonds());
			AssertEquals(2, scanCusMAWB.Underbonds.Length);
			AssertExceptionThrown(typeof(InvalidOperationException), () => scanCusMAWB.GetStandAloneAirCargo(""));

			underbond2.C4_FlightNo = "QF220";
			AssertEquals("", scanCusMAWB.ValidateSelectedUnderbond().Trim());

			var standAloneCusMAWB1 = Factory.New<CusMAWB>();
			standAloneCusMAWB1.CM_MAWB = "Master1";
			standAloneCusMAWB1.CM_MasterHouseBill = "HouseBill2";
			standAloneCusMAWB1.CM_ArrivalDate = ZDateTime.Now.AddDays(-3);

			var standAloneCusMAWB2 = Factory.New<CusMAWB>();
			standAloneCusMAWB2.CM_MAWB = "Master1";
			standAloneCusMAWB2.CM_MasterHouseBill = "HouseBill3";
			standAloneCusMAWB2.CM_ArrivalDate = ZDateTime.Now.AddDays(-3);

			scanCusMAWB.SelectedShipments = new CusHAWB[] { cusHAWB1, cusHAWB2 };

			AssertEquals("".Trim(), scanCusMAWB.Validate().Trim());
			AssertEquals("", scanCusMAWB.ValidateSelectedUnderbond().Trim());

			standAloneCusMAWB1.CM_MasterHouseBill = "HouseBill2X";
			AssertEquals("Standalone AirCargo with House Bill 'HOUSEBILL2' was not found, has it been renamed?,", scanCusMAWB.ValidateStandAloneUnderbonds().Trim());
			standAloneCusMAWB1.CM_MasterHouseBill = "HouseBill2";
			AssertEquals(2, scanCusMAWB.Underbonds.Length);
			AssertEquals("Create Underbond for Standalone AirCargo with House Bill 'HOUSEBILL2' and with Flight No 'QF220', Arrival Date ''. Origin Premise ID '' and Destination Premise ID ''.", scanCusMAWB.ValidateStandAloneUnderbonds().Trim());
			AssertEquals(underbond2.PK, scanCusMAWB.GetUnderbond().PK);
			AssertEquals("Master1", scanCusMAWB.GetAirCargoToAddSurplusConsignment().MasterBillNumber);
			AssertEquals("Master1", scanCusMAWB.GetStandAloneAirCargo("HouseBill2").MasterBillNumber);

			var standAloneUnderbond1 = Factory.NewWithValidTestData<CusUnderbond>();
			standAloneCusMAWB1.Underbonds.Add(standAloneUnderbond1);

			var standAloneUnderbond2 = Factory.NewWithValidTestData<CusUnderbond>();
			standAloneCusMAWB1.Underbonds.Add(standAloneUnderbond2);

			AssertEquals("", scanCusMAWB.Validate().Trim());
			AssertEquals("", scanCusMAWB.ValidateSelectedUnderbond().Trim());
			AssertEquals("Create Underbond for Standalone AirCargo with House Bill 'HOUSEBILL2' and with Flight No 'QF220', Arrival Date ''. Origin Premise ID '' and Destination Premise ID ''.", scanCusMAWB.ValidateStandAloneUnderbonds().Trim());

			standAloneUnderbond1.C4_FlightNo = "QF220";
			standAloneUnderbond2.C4_FlightNo = "QF110";

			AssertEquals("", scanCusMAWB.Validate().Trim());
			AssertEquals("", scanCusMAWB.ValidateSelectedUnderbond().Trim());
			AssertEquals("", scanCusMAWB.ValidateStandAloneUnderbonds().Trim());
			AssertEquals(underbond2.PK, scanCusMAWB.GetUnderbond().PK);
			AssertEquals("Master1", scanCusMAWB.GetAirCargoToAddSurplusConsignment().MasterBillNumber);
			AssertEquals("Master1", scanCusMAWB.GetStandAloneAirCargo("HouseBill2").MasterBillNumber);

			underbond2.OutturnStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals(1, scanCusMAWB.GetSentUnderbonds().Length);

			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AssertNoExceptionThrown(() => scanCusMAWB.GetStandAloneAirCargo("HouseBill2"));
		}

		protected override ScanMasterBill GetNewConsolScanMasterBill()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00000001";
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HouseBill1";
			shipment1.JS_UniqueConsignRef = "S00000001";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HouseBill2";
			shipment2.JS_UniqueConsignRef = "S00000002";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

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

		protected override ScanMasterBill GetNewStandAloneScanMasterBill()
		{
			var cusMAWB = Factory.NewWithValidTestData<CusMAWB>();
			cusMAWB.CM_MAWB = "Master1";
			cusMAWB.CM_MasterHouseBill = "S00000001";
			cusMAWB.ChildBills.AddNew();
			return new ScanCusMAWB(cusMAWB);
		}

		protected override ScanForOutturnManager GetNewScanForOutturnManager(ScanMasterBill scanObj) => new AirScanForOutturnManager((ScanCusMAWB)scanObj);
	}
}
