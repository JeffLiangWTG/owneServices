using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class BLArgentinaWrapperManifestTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestBLArgentinaWrapperManifest()
		{
			CreateAndPopulateManifestHeader();

			Factory.Save();

			ISeaManifest wrapper = new BLArgentinaWrapperManifest(header.Bills[0]);

			CombineAssertions(() =>
			{
				AssertEquals("2021080000000010", wrapper.VoyageID);
				AssertEquals("UYMVD", wrapper.LoadingPort);
				AssertEquals("UYMVDMAN0001", wrapper.MasterNumber);
				AssertEquals("10/10/2021 12:00:00 AM", wrapper.DepartureDate.ToString());
				AssertEquals("Montevideo", wrapper.OriginName);
				AssertEquals("10/10/2021 12:00:00 AM", wrapper.HouseLoadingDate.ToString());
				AssertEquals("225", wrapper.OriginCountry);
				AssertEquals("ARBUE", wrapper.DischargePort);
				AssertEquals("200", wrapper.ArrivalCountryCode);
				AssertEquals("(H)QRHW20050055C", wrapper.HouseNumber);
				AssertEquals("S/I", wrapper.MarksAndNumbers);
				AssertEquals("Consignee", wrapper.ConsigneeName);
				AssertEquals("NotifyParty", wrapper.NotifyName);
				AssertEquals("S", wrapper.ConsolidatedIndicator);
				AssertEquals("S", wrapper.TransshipmentIndicator);
				AssertEquals("CUI", wrapper.ConsigneeRegistrationNumberType);
				AssertEquals(100, wrapper.ConsigneeRegistrationNumber);
				AssertEquals("200", wrapper.ConsigneeCountry);
				AssertEquals("", wrapper.TariffPosition);
				AssertEquals("S", wrapper.SecureLogisticalOperatorIndicator);
				AssertEquals("N", wrapper.MonitoredTransitIndicator);
				AssertEquals("S", wrapper.RenarIndicator);
				AssertEquals("Shipper", wrapper.ShipperName);
				AssertEquals("200", wrapper.ShipperRegistrationNumber);
				AssertEquals("200", wrapper.ShipperCountry);
				AssertEquals("123", wrapper.CustomsOffice);
				AssertEquals("1X20´DRY PART CONTAINER STC.: 1 PAQUETE CONTENIENDO SECADORA DE HIELO NOVADRYER-HF400", wrapper.Description);

				AssertEquals(1, wrapper.Packs.Count);
				AssertEquals(1, wrapper.Containers.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Argentina);
			helper.CreateCusMapType(ARMessageConstants.CountryMapType, "OUT", ARMessageConstants.CountryMapType, true);
			helper.CreateCusMap(ARMessageConstants.CountryMapType, Core.Constants.CountryCodes.Argentina, "200", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Argentina);
			helper.CreateCusMap(ARMessageConstants.CountryMapType, Core.Constants.CountryCodes.Uruguay, "225", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Argentina);
		}

		AsycudaManifestHeader CreateAndPopulateManifestHeader()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_CustomsOffice = "123";
			header.AMA_E_DEP = new ZDateTime(2021, 10, 10);
			header.AMA_MasterBill = "MAN0001";
			header.AMA_RL_NKPortOfDischarge = "ARBUE";
			header.AMA_RL_NKPortOfLoading = "UYMVD";
			header.RegistrationNumber = "2021080000000010";
			header.AMA_CustomsDischargePort = "ARBUE";
			header.AMA_CustomsLoadPort = "UYMVD";

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "22G0";

			var container = header.Containers.AddNew();
			container.ACN_RC_ContainerType = refContainer.PK;

			CreateAndPopulateHouseBill();

			return header;
		}

		void CreateAndPopulateHouseBill()
		{
			var bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_CargoStatus = CargoStatusList.Codes.FullShipment;
			bill.ABL_CustomsDischargePort = "ARBUE";
			bill.ABL_CustomsLoadPort = "UYMVD";
			bill.ABL_GoodsDescription = "1X20´DRY PART CONTAINER STC.: 1 PAQUETE CONTENIENDO SECADORA DE HIELO NOVADRYER-HF400";
			bill.ABL_GrossWeight = 170;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_IsInformedToRenar = ZBool.True;
			bill.ABL_IsMonitoredTransit = ZBool.False;
			bill.ABL_ManifestQty = 1;
			bill.ABL_MarksAndNumbers = "S/I";
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_RL_NKFinalDestination = "CLSCL";
			bill.ABL_RL_NKOrigin = "UYMVD";
			bill.ABL_RL_NKPortOfDischarge = "CLSCL";
			bill.ABL_Volume = 0.76;
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "96";
			orgHeader.OH_FullName = "orgHeader";
			orgHeader.CustomsCodes.AddNew(ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT, "211110890017", Core.Constants.CountryCodes.Argentina);

			bill.ABL_OA_Consignee = FillOrgAddress(orgHeader, "Consignee").PK;
			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Argentina;
			bill.ABL_ConsigneeRegNoType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;
			bill.ABL_ConsigneeRegNo = "100";
			bill.ABL_OA_Shipper = FillOrgAddress(orgHeader, "Shipper").PK;
			bill.ABL_ShipperRegNo = "200";
			bill.ABL_OA_NotifyParty = FillOrgAddress(orgHeader, "NotifyParty").PK;

			var pack = bill.Packs.AddNew();
			pack.ContainerPK = header.Containers[0].PK;
		}

		OrgAddress FillOrgAddress(OrgHeader orgHeader, ZString orgType)
		{
			OrgAddress orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.CompanyName = orgType;
			orgAddress.OA_Address1 = string.Concat(orgType, "Address1");
			return orgAddress;
		}
	}
}
