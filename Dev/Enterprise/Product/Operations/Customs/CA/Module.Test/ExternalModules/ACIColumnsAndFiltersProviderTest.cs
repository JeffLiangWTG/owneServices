using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants.Customs;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class ACIColumnsAndFiltersProviderTest : ColumnsAndFiltersProviderTest
	{
		public void TestACICargoStatus()
		{
			var filter = (ModuleTextFilter)filters["ACI Cargo Status"];
			AssertNotNull("ACI Cargo Status filter", filter);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, SupplementaryCargoReportJobStatusList.Codes.Clear, shipment1);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, SupplementaryCargoReportJobStatusList.Codes.Clear, shipment2, shipment3, shipment4, shipment5, shipment6, shipment7);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, SupplementaryCargoReportJobStatusList.Codes.Error, shipment2, shipment6);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, SupplementaryCargoReportJobStatusList.Codes.Error, shipment1, shipment3, shipment4, shipment5, shipment7);

			AssertStatusQuery(filter, SQLComparisonOperator.IsBlank, string.Empty, shipment3, shipment4, shipment5, shipment7);
			AssertStatusQuery(filter, SQLComparisonOperator.IsNotBlank, string.Empty, shipment1, shipment2, shipment6);
		}

		public void TestACIMessageStatus()
		{
			var filter = (ModuleTextFilter)filters["ACI Message Status"];
			AssertNotNull("ACI Message Status filter", filter);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, MessageStatusList.Codes.ClearOriginal, shipment1);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, MessageStatusList.Codes.ClearOriginal, shipment2, shipment3, shipment4, shipment5, shipment6, shipment7);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, MessageStatusList.Codes.ErrorOriginal, shipment2, shipment6);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, MessageStatusList.Codes.ErrorOriginal, shipment1, shipment3, shipment4, shipment5, shipment7);

			AssertStatusQuery(filter, SQLComparisonOperator.IsBlank, string.Empty, shipment3, shipment5, shipment7);
			AssertStatusQuery(filter, SQLComparisonOperator.IsNotBlank, string.Empty, shipment1, shipment2, shipment4, shipment6);
		}

		public void TestEManifestCargoStatus()
		{
			var filter = (ModuleTextFilter)filters["eManifest Cargo Status"];
			AssertNotNull("eManifest Cargo Status filter", filter);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, EManifestForwarderJobStatusList.Codes.Clear, shipment1, shipment6);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, SupplementaryCargoReportJobStatusList.Codes.Clear, shipment2, shipment3, shipment4, shipment5, shipment7);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, EManifestForwarderJobStatusList.Codes.Error, shipment5);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, SupplementaryCargoReportJobStatusList.Codes.Error, shipment1, shipment2, shipment3, shipment4, shipment6, shipment7);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, EManifestForwarderJobStatusList.Codes.NotMatched, shipment2);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, SupplementaryCargoReportJobStatusList.Codes.NotMatched, shipment1, shipment3, shipment4, shipment5, shipment6, shipment7);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, EManifestForwarderJobStatusList.Codes.Validated, shipment4);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, SupplementaryCargoReportJobStatusList.Codes.Validated, shipment1, shipment2, shipment3, shipment5, shipment6, shipment7);

			AssertStatusQuery(filter, SQLComparisonOperator.IsBlank, string.Empty, shipment3, shipment7);
			AssertStatusQuery(filter, SQLComparisonOperator.IsNotBlank, string.Empty, shipment1, shipment2, shipment4, shipment5, shipment6);
		}

		public void TestEManifestMessageStatus()
		{
			var filter = (ModuleTextFilter)filters["eManifest Message Status"];
			AssertNotNull("eManifest Message Status filter", filter);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, MessageStatusList.Codes.ClearOriginal, shipment1, shipment2, shipment6);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, MessageStatusList.Codes.ClearOriginal, shipment3, shipment4, shipment5, shipment7);

			AssertStatusQuery(filter, SQLComparisonOperator.Equal, MessageStatusList.Codes.ErrorOriginal, shipment5);
			AssertStatusQuery(filter, SQLComparisonOperator.NotEqual, MessageStatusList.Codes.ErrorOriginal, shipment1, shipment2, shipment3, shipment4, shipment6, shipment7);

			AssertStatusQuery(filter, SQLComparisonOperator.IsBlank, string.Empty, shipment3, shipment7);
			AssertStatusQuery(filter, SQLComparisonOperator.IsNotBlank, string.Empty, shipment1, shipment2, shipment4, shipment5, shipment6);
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var house1_1 = Factory.NewWithValidTestData<CusSCAHouse>();
			house1_1.CA_JS = shipment1.PK;
			house1_1.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Clear;
			house1_1.CA_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			var oceanBill1_1 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill1_1.CB_ApplicationCode = CusSCAOceanBillApplicationCodes.CanadaACISea;
			house1_1.CA_CB = oceanBill1_1.PK;
			var house1_2 = Factory.NewWithValidTestData<CusCAeMHHouse>();
			house1_2.BW_ParentID = shipment1.PK;
			house1_2.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			house1_2.BW_MessageStatus = MessageStatusList.Codes.ClearOriginal;

			shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var house2_1 = Factory.NewWithValidTestData<CusSCAHouse>();
			house2_1.CA_JS = shipment2.PK;
			house2_1.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Error;
			house2_1.CA_MessageStatus = MessageStatusList.Codes.ErrorOriginal;
			var oceanBill2_1 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill2_1.CB_ApplicationCode = CusSCAOceanBillApplicationCodes.CanadaACIRoad;
			house2_1.CA_CB = oceanBill2_1.PK;
			var house2_2 = Factory.NewWithValidTestData<CusCAeMHHouse>();
			house2_2.BW_ParentID = shipment2.PK;
			house2_2.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.NotMatched;
			house2_2.BW_MessageStatus = MessageStatusList.Codes.ClearOriginal;

			shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var house3_1 = Factory.NewWithValidTestData<CusSCAHouse>();
			house3_1.CA_JS = shipment3.PK;
			house3_1.CA_ShipmentStatus = "";
			house3_1.CA_MessageStatus = "";
			var oceanBill3_1 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill3_1.CB_ApplicationCode = CusSCAOceanBillApplicationCodes.CanadaACISea;
			house3_1.CA_CB = oceanBill3_1.PK;
			var house3_2 = Factory.NewWithValidTestData<CusCAeMHHouse>();
			house3_2.BW_ParentID = shipment3.PK;
			house3_2.BW_CustomsStatus = "";
			house3_2.BW_MessageStatus = "";

			shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			var house4_1 = Factory.NewWithValidTestData<CusSCAHouse>();
			house4_1.CA_JS = shipment4.PK;
			house4_1.CA_MessageStatus = MessageStatusList.Codes.ClearDelete;
			var oceanBill4_1 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill4_1.CB_ApplicationCode = CusSCAOceanBillApplicationCodes.CanadaACIRoad;
			house4_1.CA_CB = oceanBill4_1.PK;
			var house4_2 = Factory.NewWithValidTestData<CusCAeMHHouse>();
			house4_2.BW_ParentID = shipment4.PK;
			house4_2.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Validated;
			house4_2.BW_MessageStatus = MessageStatusList.Codes.ClearDelete;

			shipment5 = Factory.NewWithValidTestData<ForwardingShipment>();
			var house5_1 = Factory.NewWithValidTestData<CusSCAHouse>();
			house5_1.CA_JS = shipment5.PK;
			house5_1.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Clear;
			house5_1.CA_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			var house5_2 = Factory.NewWithValidTestData<CusCAeMHHouse>();
			house5_2.BW_ParentID = shipment5.PK;
			house5_2.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Error;
			house5_2.BW_MessageStatus = MessageStatusList.Codes.ErrorOriginal;

			shipment6 = Factory.NewWithValidTestData<ForwardingShipment>();
			var house6_1 = Factory.NewWithValidTestData<CusSCAHouse>();
			house6_1.CA_JS = shipment6.PK;
			house6_1.CA_ShipmentStatus = SupplementaryCargoReportJobStatusList.Codes.Error;
			house6_1.CA_MessageStatus = MessageStatusList.Codes.ErrorOriginal;
			var oceanBill6_1 = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill6_1.CB_ApplicationCode = CusSCAOceanBillApplicationCodes.CanadaACIAir;
			house6_1.CA_CB = oceanBill6_1.PK;
			var house6_2 = Factory.NewWithValidTestData<CusCAeMHHouse>();
			house6_2.BW_ParentID = shipment6.PK;
			house6_2.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			house6_2.BW_MessageStatus = MessageStatusList.Codes.ClearOriginal;

			shipment7 = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			filters = new ModuleFilterCollection();
			new CAShipmentModuleColumnsAndFiltersProvider().AddFilters(filters, Factory);
		}

		ForwardingShipment shipment1;
		ForwardingShipment shipment2;
		ForwardingShipment shipment3;
		ForwardingShipment shipment4;
		ForwardingShipment shipment5;
		ForwardingShipment shipment6;
		ForwardingShipment shipment7;
		ModuleFilterCollection filters;
	}
}
