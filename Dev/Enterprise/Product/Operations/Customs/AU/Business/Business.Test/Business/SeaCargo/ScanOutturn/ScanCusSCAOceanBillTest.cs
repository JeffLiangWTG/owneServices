using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ScanCusSCAOceanBillTest : ScanMasterBillTest
	{
		public void TestNoOceanBill()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var underbond = container.Underbonds.AddNew();
			var scanObj = new ScanCusSCAOceanBill(oceanBill);
			scanObj.SelectedUnderbond = underbond;
			AssertContains(ScanCusSCAOceanBill.OceanBillMissing, scanObj.ValidateSelectedUnderbond());
			oceanBill.CB_OceanBill = "OC";
			Assert(!scanObj.ValidateSelectedUnderbond().Contains(ScanCusSCAOceanBill.OceanBillMissing));
		}

		public void TestNoUnderbondSelected()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var scanObj = new ScanCusSCAOceanBill(oceanBill);
			AssertContains(ScanMasterBill.NoUnderbondSelected, scanObj.ValidateSelectedUnderbond());

			var container = oceanBill.Containers.AddNew();
			var underbond = container.Underbonds.AddNew();
			scanObj.SelectedUnderbond = underbond;
			Assert(!scanObj.ValidateSelectedUnderbond().Contains(ScanMasterBill.NoUnderbondSelected));
		}

		public void TestDestinationPremiseIDMissing()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var underbond = container.Underbonds.AddNew();
			var scanObj = new ScanCusSCAOceanBill(oceanBill);
			scanObj.SelectedUnderbond = underbond;
			AssertContains(ScanCusSCAOceanBill.DestinationPremiseIDMissing, scanObj.ValidateSelectedUnderbond());
			underbond.C4_DestinationPremiseID = "193K";
			Assert(!scanObj.ValidateSelectedUnderbond().Contains(ScanCusSCAOceanBill.DestinationPremiseIDMissing));
		}

		public void TestContainerNumberIsMissingError()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var underbond = container.Underbonds.AddNew();
			underbond.C4_SendersMessageReference = "U1234";
			var scanObj = new ScanCusSCAOceanBill(oceanBill);
			scanObj.SelectedUnderbond = underbond;
			AssertContains(ScanCusSCAOceanBill.GetContainerNumberIsMissingError("U1234"), scanObj.ValidateSelectedUnderbond());
			container.CN_ContainerNumber = "CN1234";
			Assert(!scanObj.ValidateSelectedUnderbond().Contains(ScanCusSCAOceanBill.GetContainerNumberIsMissingError("U1234")));
		}

		public void TestLloydMissing()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var underbond = container.Underbonds.AddNew();
			var scanObj = new ScanCusSCAOceanBill(oceanBill);
			scanObj.SelectedUnderbond = underbond;
			AssertContains(ScanCusSCAOceanBill.LloydMissing, scanObj.ValidateSelectedUnderbond());
			oceanBill.CB_LloydsIMO = "12345";
			Assert(!scanObj.ValidateSelectedUnderbond().Contains(ScanCusSCAOceanBill.LloydMissing));
		}

		public void TestVoyageMissing()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var underbond = container.Underbonds.AddNew();
			var scanObj = new ScanCusSCAOceanBill(oceanBill);
			scanObj.SelectedUnderbond = underbond;
			AssertContains(ScanCusSCAOceanBill.VoyageMissing, scanObj.ValidateSelectedUnderbond());
			oceanBill.CB_Voyage = "123";
			Assert(!scanObj.ValidateSelectedUnderbond().Contains(ScanCusSCAOceanBill.VoyageMissing));
		}

		public void TestContainerArrivalHasNotBeenPostedError()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_Voyage = "123";
			oceanBill.CB_LloydsIMO = "12345";
			oceanBill.CB_OceanBill = "OC";
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "CN123";
			var underbond = container.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "193K";
			var scanObj = new ScanCusSCAOceanBill(oceanBill);
			scanObj.SelectedUnderbond = underbond;
			AssertContains(ScanCusSCAOceanBill.GetContainerArrivalHasNotBeenPostedError("CN123", "12345", "123", "193K"), scanObj.ValidateSelectedUnderbond());

			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_LloydsIMO = "12345";
			outturnHeader.C6_VoyageNum = "123";
			outturnHeader.C6_OutturningPremiseID = "193K";
			var line = outturnHeader.Outturns.AddNew();
			line.C5_ContainerNumber = "CN123";
			line.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			line.C5_CargoReceiptDate = ZDateTime.Today;
			Assert(!scanObj.ValidateSelectedUnderbond().Contains(ScanCusSCAOceanBill.GetContainerArrivalHasNotBeenPostedError("CN123", "12345", "123", "193K")));
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

			var oceanBill1 = Factory.New<CusSCAOceanBill>();
			oceanBill1.CB_OceanBill = "Master1";
			oceanBill1.CB_ParentId = consol.PK;
			oceanBill1.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var hb1 = oceanBill1.HouseBills.AddNew();
			hb1.CA_HouseBill = "HOUSEBILL1";
			hb1.CA_JS = shipment1.PK;

			hb1.CA_ShipmentStatus = "HLD";

			var hb3 = oceanBill1.HouseBills.AddNew();
			hb3.CA_HouseBill = "HOUSEBILL2";
			hb3.CA_JS = shipment2.PK;
			hb3.CA_ShipmentStatus = "CLR";

			var container1 = oceanBill1.Containers.AddNew();
			container1.CN_ContainerNumber = "CN1";
			container1.Pivots.AddNew().CV_CA = hb1.PK;
			container1.Pivots.AddNew().CV_CA = hb3.PK;

			var underbond = Factory.NewWithValidTestData<CusUnderbond>();
			container1.Underbonds.Add(underbond);

			var oceanBill2 = Factory.New<CusSCAOceanBill>();
			oceanBill2.CB_OceanBill = "Master1";
			oceanBill2.CB_MasterHouseBill = "HOUSEBILL1";
			var hb2 = oceanBill2.HouseBills.AddNew();
			hb2.CA_HouseBill = "hb1";
			var container2 = oceanBill2.Containers.AddNew();
			container2.CN_ContainerNumber = "CN1";
			container2.Pivots.AddNew().CV_CA = hb2.PK;

			Factory.Save();

			return new ScanCusSCAOceanBill(oceanBill1);
		}

		protected override ScanMasterBill GetNewStandAloneScanMasterBill()
		{
			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "Master1";
			oceanBill.CB_MasterHouseBill = "S00000001";

			var houseBill = oceanBill.HouseBills.AddNew();

			return new ScanCusSCAOceanBill(oceanBill);
		}

		protected override ScanForOutturnManager GetNewScanForOutturnManager(ScanMasterBill scanObj)
		{
			return new SeaScanForOutturnManager((ScanCusSCAOceanBill)scanObj);
		}
	}
}
