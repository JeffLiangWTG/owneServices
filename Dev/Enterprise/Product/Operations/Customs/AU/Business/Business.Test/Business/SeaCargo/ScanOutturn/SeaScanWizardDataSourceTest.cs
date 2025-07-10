using System.Linq;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaScanWizardDataSource))]
	sealed class SeaScanWizardDataSourceTest : ScanWizardDataSourceTest
	{
		public void TestShipmentSelectorCollection()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HB1";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HLS1";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;

			var consolOceanBill = Factory.New<CusSCAOceanBill>();
			consolOceanBill.CB_OceanBill = "OC123";
			consolOceanBill.CB_ParentId = Factory.New<ForwardingConsol>().PK;
			consolOceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var container1 = consolOceanBill.Containers.AddNew();
			container1.CN_ContainerNumber = "CN123";

			var house1 = consolOceanBill.HouseBills.AddNew();
			house1.CA_HouseBill = "HB1";
			house1.CA_JS = shipment1.PK;
			var pivot1 = container1.Pivots.AddNew();
			pivot1.CV_CA = house1.PK;

			var house2 = consolOceanBill.HouseBills.AddNew();
			house2.CA_HouseBill = "HLS1";
			house2.CA_JS = shipment2.PK;
			var pivot2 = container1.Pivots.AddNew();
			pivot2.CV_CA = house2.PK;

			var standAloneOceanBill = Factory.New<CusSCAOceanBill>();
			standAloneOceanBill.CB_MasterHouseBill = "HLS1";
			standAloneOceanBill.CB_OceanBill = "OC123";
			var container2 = standAloneOceanBill.Containers.AddNew();
			container2.CN_ContainerNumber = "CN123";
			var houseBill2 = standAloneOceanBill.HouseBills.AddNew();
			var standAlonepivot2 = container2.Pivots.AddNew();
			standAlonepivot2.CV_CA = houseBill2.PK;

			var underbond = container1.Underbonds.AddNew();

			Factory.Save();

			var scanObj = new ScanCusSCAOceanBill(consolOceanBill);
			scanObj.SelectedUnderbond = underbond;

			var dataSource = new SeaScanWizardDataSource(scanObj);
			AssertEquals(2, dataSource.ShipmentSelectorLineCollection.Count);
			Assert(dataSource.ShipmentSelectorLineCollection.Cast<ShipmentSelectorLine>().Any(x => x.Shipment == "All Standards"));
			Assert(dataSource.ShipmentSelectorLineCollection.Cast<ShipmentSelectorLine>().Any(x => x.Shipment == "HLS1"));
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var consol = Factory.New<ForwardingConsol>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var container = oceanBill.Containers.AddNew();
			var underbond = container.Underbonds.AddNew();
			var scanObj = new ScanCusSCAOceanBill(oceanBill);
			scanObj.SelectedUnderbond = underbond;
			return new SeaScanWizardDataSource(scanObj);
		}
	}
}
