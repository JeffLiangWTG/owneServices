using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	sealed class AsycudaBillValidationForRegularBillTest : BusinessObjectLookupsTestCase
	{
		public void TestCheckCargoDisposition()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			bill.CargoDisposition = "10";
			AssertNoNotifications(bill.CargoDispositionInfo);

			bill.CargoDisposition = ZString.Empty;
			AssertHasMessageError(bill.CargoDispositionInfo, "You have not entered a Cargo Disposition.");

			bill.CargoDisposition = "30";
			AssertHasMessageError(bill.CargoDispositionInfo, "The code you have selected is not in the list.");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.CargoDisposition = ZString.Empty;
			AssertNoNotifications(bill.CargoDispositionInfo);
		}

		public void TestCheckTravelDocumentType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			bill.TravelDocumentType = "0";
			AssertNoNotifications(bill.TravelDocumentTypeInfo);

			bill.TravelDocumentType = ZString.Empty;
			AssertHasMessageError(bill.TravelDocumentTypeInfo, "You have not entered a Travel Document Type.");

			bill.TravelDocumentType = "5";
			AssertHasMessageError(bill.TravelDocumentTypeInfo, "The code you have selected is not in the list.");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.TravelDocumentType = ZString.Empty;
			AssertNoNotifications(bill.TravelDocumentTypeInfo);
		}

		public void TestCheckABL_OA_Shipper()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_OA_Shipper = ZGuid.Empty;

			AssertHasMessageError(bill.ABL_OA_ShipperInfo, "You have not entered a Shipper.");

			var org = Factory.New<OrgHeader>();

			var orgAddress = org.Addresses.AddNew();
			bill.ABL_RN_NKShipperCountry = Core.Constants.CountryCodes.Colombia;
			bill.ABL_OA_Shipper = orgAddress.PK;

			AssertHasMessageError(bill.ABL_OA_ShipperInfo, "The selected Shipper should have NIT");

			var orgCusCode = orgAddress.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
			orgCusCode.OK_CustomsRegNo = "12345";

			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertNoNotifications(bill.ABL_OA_ShipperInfo);
		}

		public void TestABL_ShipperRegNo()
		{
			var bill2 = Factory.New<AsycudaBill>();
			bill2.ABL_ShipperRegNoType = ColombiaOrgCusCodeInfo.OrgCusCodes.CID;

			bill2.ABL_ShipperRegNo = "216714840016";
			AssertNoNotifications(bill2.ABL_ShipperRegNoInfo);

			bill2.ABL_ShipperRegNo = ZString.Empty;
			AssertNoNotifications(bill2.ABL_ShipperRegNoInfo);

			bill2.ABL_RN_NKShipperCountry = Core.Constants.CountryCodes.Colombia;
			bill2.ABL_ShipperRegNo = ZString.Empty;
			AssertHasMessageError(bill2.ABL_ShipperRegNoInfo, "You have not entered a value.");
		}

		public void TestABL_ShipperRegNoType()
		{
			var bill2 = Factory.New<AsycudaBill>();

			bill2.ABL_ShipperRegNoType = ZString.Empty;
			AssertNoNotifications(bill2.ABL_ShipperRegNoTypeInfo);

			bill2.ABL_RN_NKShipperCountry = Core.Constants.CountryCodes.Colombia;
			bill2.ABL_ShipperRegNoType = ZString.Empty;
			AssertHasMessageError(bill2.ABL_ShipperRegNoTypeInfo, "You have not entered a value.");

			bill2.ABL_ShipperRegNoType = OrgCusCode.CodeTypes.PassportID;
			AssertNoNotifications(bill2.ABL_ShipperRegNoTypeInfo);

			bill2.ABL_ShipperRegNoType = "VAT";
			AssertHasMessageError(bill2.ABL_ShipperRegNoTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckABL_OA_Consignee()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_OA_Consignee = ZGuid.Empty;

			AssertHasMessageError(bill.ABL_OA_ConsigneeInfo, "You have not entered a Consignee.");

			var org = Factory.New<OrgHeader>();

			var orgAddress = org.Addresses.AddNew();
			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Colombia;
			bill.ABL_OA_Consignee = orgAddress.PK;

			AssertHasMessageError(bill.ABL_OA_ConsigneeInfo, "The selected Consignee should have NIT");

			var orgCusCode = orgAddress.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
			orgCusCode.OK_CustomsRegNo = "12345";

			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertNoNotifications(bill.ABL_OA_ConsigneeInfo);
		}

		public void TestABL_ConsigneeRegNo()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeRegNoType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;

			bill.ABL_ConsigneeRegNo = "62318879";
			AssertNoNotifications(bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertNoNotifications(bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Colombia;
			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, "You have not entered a value.");
		}

		public void TestABL_ConsigneeRegNoType()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.ABL_ConsigneeRegNoType = ZString.Empty;
			AssertNoNotifications(bill.ABL_ConsigneeRegNoTypeInfo);

			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Colombia;
			bill.ABL_ConsigneeRegNoType = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoTypeInfo, "You have not entered a value.");

			bill.ABL_ConsigneeRegNoType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
			AssertNoNotifications(bill.ABL_ConsigneeRegNoTypeInfo);

			bill.ABL_ConsigneeRegNoType = "VAT";
			AssertHasMessageError(bill.ABL_ConsigneeRegNoTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckABL_OA_GoodsLocation()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "CO";
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "BILL1";
			bill.ABL_OA_GoodsLocation = ZGuid.Empty;
			Factory.Save();
			AssertHasMessageError(bill.ABL_OA_GoodsLocationInfo, "A Warehouse is required");
			bill.ABL_OA_GoodsLocation = orgAddress.PK;
			AssertHasMessageError(bill.ABL_OA_GoodsLocationInfo, "The Warehouse should have a CCP assigned number.");
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1996");
			bill.ABL_OA_GoodsLocation = orgAddress.PK;
			AssertNoNotifications(bill.ABL_OA_GoodsLocationInfo);
		}

		public void TestCheckABL_RX_NKGoodsValueCurrency()
		{
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "EUR";
			var rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			rate.RE_SellRate = 0.923m;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_MasterBillIssueDate = ZDate.Today;

			var bill = header.Bills.AddNew();
			bill.ABL_GoodsValue = 20;
			bill.ABL_RX_NKGoodsValueCurrency = "EUR";
			AssertNoMessageErrorContaining(bill.ABL_RX_NKGoodsValueCurrencyInfo, "There is no valid exchange rate for this currency for");

			bill.ABL_RX_NKGoodsValueCurrency = "XYX";
			AssertHasMessageErrorContaining(bill.ABL_RX_NKGoodsValueCurrencyInfo, "There is no valid exchange rate for this currency for");
		}

		public void TestCheckABL_RX_NKFreightValueCurrency()
		{
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "EUR";
			var rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			rate.RE_SellRate = 0.923m;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_MasterBillIssueDate = ZDate.Today;

			var bill = header.Bills.AddNew();
			bill.ABL_FreightValue = 20;
			bill.ABL_RX_NKFreightValueCurrency = "EUR";
			AssertNoMessageErrorContaining(bill.ABL_RX_NKFreightValueCurrencyInfo, "There is no valid exchange rate for this currency for");

			bill.ABL_RX_NKFreightValueCurrency = "XYX";
			AssertHasMessageErrorContaining(bill.ABL_RX_NKFreightValueCurrencyInfo, "There is no valid exchange rate for this currency for");
		}

		public void TestBillIssueDate()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_BillIssueDate = ZDate.Today;
			AssertNoMessageErrors(bill.ABL_BillIssueDateInfo);

			bill.ABL_BillIssueDate = ZDate.Empty;
			AssertHasMessageError(bill.ABL_BillIssueDateInfo, MandatoryValidation.YouHaveNotEntered + " an Issue Date.");
		}

		public void TestGoodsDecription()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_GoodsDescription = "GOODS DESCRIPTION";
			AssertNoMessageErrors(bill.ABL_GoodsDescriptionInfo);

			bill.ABL_GoodsDescription = ZString.Empty;
			AssertHasMessageError(bill.ABL_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered + " a Goods' Description (on Bill).");
		}

		public void TestVolume()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_Volume = 1.10;
			AssertNoMessageErrors(bill.ABL_VolumeInfo);

			bill.ABL_Volume = 0.00;
			AssertHasMessageError(bill.ABL_VolumeInfo, MandatoryValidation.YouHaveNotEntered + " a Volume (on Bill).");
		}

		public void TestVolumeUQ()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
			AssertNoMessageErrors(bill.ABL_VolumeUQInfo);

			bill.ABL_VolumeUQ = ZString.Empty;
			AssertHasMessageError(bill.ABL_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered + " a Volume Unit (on Bill).");
		}

		public void TestCheckABL_ManifestQtyMatchSumOfPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_ManifestQty = 11;

			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 6;

			pack = bill.Packs.AddNew();
			pack.APA_PackQty = 5;

			bill.Validation.ValidateABL_ManifestQty();
			AssertNoMessageErrors(bill.ABL_ManifestQtyInfo);

			pack = bill.Packs.AddNew();
			pack.APA_PackQty = 1;

			bill.Validation.ValidateABL_ManifestQty();
			AssertHasMessageError(bill.ABL_ManifestQtyInfo, "Sum of packages' package counts (12) is not equal to manifest quantity");
		}

		public void TestGrossWeightIsEqualBetweenBillAndPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Weight = 4;
			pack1.APA_WeightUQ = Core.Constants.Weight.Kilograms;

			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_GrossWeight = 5;

			AssertHasMessageError(bill.ABL_GrossWeightInfo, "Weight of packages (4) is not equal to manifest Gross Weight (5) in (KG)");

			var pack2 = bill.Packs.AddNew();
			pack2.APA_Weight = 3000;
			pack2.APA_WeightUQ = Core.Constants.Weight.Grams;

			bill.ABL_GrossWeight = 7;

			AssertNoMessageErrors(bill.ABL_GrossWeightInfo);
		}

		public void TestVolumeIsEqualBetweenBillAndPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Volume = 4;
			pack1.APA_VolumeUQ = Core.Constants.Volume.Litre;

			bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
			bill.ABL_Volume = 5;

			AssertHasMessageError(bill.ABL_VolumeInfo, "Volume of packages (4) is not equal to manifest Volume (5) in (L)");

			var pack2 = bill.Packs.AddNew();
			pack2.APA_Volume = 4000;
			pack2.APA_VolumeUQ = Core.Constants.Volume.CubicCentimeters;

			bill.ABL_Volume = 8;

			AssertNoMessageErrors(bill.ABL_VolumeInfo);
		}

		public void TestCheckABL_RL_NKFinalDestination()
		{
			var state = Factory.New<RefCountryStates>();
			state.RW_Code = "CUN";
			state.RW_RN_NKCountryCode = Core.Constants.CountryCodes.Colombia;

			var unloco = new RefUNLOCO.Loader(Factory).Load("COBOG");
			unloco.RL_RW = state.PK;

			var bill = Factory.New<AsycudaBill>();

			bill.ABL_RL_NKFinalDestination = unloco.Code;
			AssertNoMessageErrors(bill.ABL_RL_NKFinalDestinationInfo);

			bill.ABL_RL_NKFinalDestination = "COCAL";
			AssertHasMessageError(bill.ABL_RL_NKFinalDestinationInfo, "The selected Final Destination should have State entered");

			bill.ABL_RL_NKFinalDestination = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_RL_NKFinalDestinationInfo);
		}

		public void TestCheckContainerMode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(bill.ABL_ContainerModeInfo, "2", "1");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_ContainerMode = ZString.Empty;
			AssertNoNotifications(bill.ABL_ContainerModeInfo);
		}
	}
}
