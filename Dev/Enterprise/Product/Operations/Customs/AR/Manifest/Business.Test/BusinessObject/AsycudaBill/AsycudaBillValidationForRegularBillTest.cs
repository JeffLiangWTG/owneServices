using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class AsycudaBillValidationForRegularBillTest : BusinessObjectLookupsTestCase
	{
		public void TestMarksAndNumbers()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var bill = header.Bills.AddNew();

			bill.ABL_MarksAndNumbers = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_MarksAndNumbers = "BDGA1996";
			AssertNoNotifications(bill.ABL_MarksAndNumbersInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_MarksAndNumbers = ZString.Empty;
			AssertNoNotifications(bill.ABL_MarksAndNumbersInfo);
			bill.ABL_MarksAndNumbers = "BDGA1996";
			AssertNoNotifications(bill.ABL_MarksAndNumbersInfo);
		}

		#region Consignee

		public void TestConsigneeRegNo()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeRegNoType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL;
			bill.ABL_ConsigneeRegNo = "62318879";
			AssertNoNotifications(bill.ABL_ConsigneeRegNoInfo);
			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, "You have not entered a value.");
		}

		public void TestConsigneeRegNoType()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeRegNoType = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeRegNoTypeInfo, "You have not entered a value.");
			bill.ABL_ConsigneeRegNoType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL;
			AssertNoNotifications(bill.ABL_ConsigneeRegNoTypeInfo);
			bill.ABL_ConsigneeRegNoType = "VAT";
			AssertHasMessageError(bill.ABL_ConsigneeRegNoTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestConsigneeName()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			bill.ABL_ConsigneeName = ZString.Empty;
			AssertNoNotifications(bill.ABL_ConsigneeNameInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_ConsigneeName = "Consignee";
			AssertNoNotifications(bill.ABL_ConsigneeNameInfo);
			bill.ABL_ConsigneeName = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestConsigneeCity()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			bill.ABL_ConsigneeCity = ZString.Empty;
			AssertNoNotifications(bill.ABL_ConsigneeCityInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_ConsigneeCity = "Montevideo";
			AssertNoNotifications(bill.ABL_ConsigneeCityInfo);
			bill.ABL_ConsigneeCity = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeCityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestConsigneeCountry()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_RN_NKConsigneeCountry = "UY";
			AssertNoNotifications(bill.ABL_RN_NKConsigneeCountryInfo);

			bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKConsigneeCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#endregion

		#region Shipper

		public void TestShipperRegNo()
		{
			var bill2 = Factory.New<AsycudaBill>();
			bill2.ABL_ShipperRegNoType = ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI;
			bill2.ABL_ShipperRegNo = "216714840016";
			AssertNoNotifications(bill2.ABL_ShipperRegNoInfo);
			bill2.ABL_ShipperRegNo = ZString.Empty;
			AssertHasMessageError(bill2.ABL_ShipperRegNoInfo, "You have not entered a value.");
		}

		public void TestShipperRegNoType()
		{
			var bill2 = Factory.New<AsycudaBill>();
			bill2.ABL_ShipperRegNoType = ZString.Empty;
			AssertHasMessageError(bill2.ABL_ShipperRegNoTypeInfo, "You have not entered a value.");
			bill2.ABL_ShipperRegNoType = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIL;
			AssertNoNotifications(bill2.ABL_ShipperRegNoTypeInfo);
			bill2.ABL_ShipperRegNoType = "VAT";
			AssertHasMessageError(bill2.ABL_ShipperRegNoTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestShipperName()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			bill.ABL_ShipperName = ZString.Empty;
			AssertNoNotifications(bill.ABL_ShipperNameInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_ShipperName = "Shipper";
			AssertNoNotifications(bill.ABL_ShipperNameInfo);
			bill.ABL_ShipperName = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ShipperNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestShipperCity()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			bill.ABL_ShipperCity = ZString.Empty;
			AssertNoNotifications(bill.ABL_ShipperCityInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_ShipperCity = "Montevideo";
			AssertNoNotifications(bill.ABL_ShipperCityInfo);
			bill.ABL_ShipperCity = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ShipperCityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestShipperCountry()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_RN_NKShipperCountry = "UY";
			AssertNoNotifications(bill.ABL_RN_NKShipperCountryInfo);

			bill.ABL_RN_NKShipperCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKShipperCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#endregion

		#region Notify Party

		public void TestNotifyPartyName()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			bill.ABL_NotifyPartyName = ZString.Empty;
			AssertNoNotifications(bill.ABL_NotifyPartyNameInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_NotifyPartyName = "NotifyParty";
			AssertNoNotifications(bill.ABL_NotifyPartyNameInfo);
			bill.ABL_NotifyPartyName = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestNotifyPartyCity()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			bill.ABL_NotifyPartyCity = ZString.Empty;
			AssertNoNotifications(bill.ABL_NotifyPartyCityInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_NotifyPartyCity = "Montevideo";
			AssertNoNotifications(bill.ABL_NotifyPartyCityInfo);
			bill.ABL_NotifyPartyCity = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyCityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestNotifyPartyCountry()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_RN_NKNotifyPartyCountry = "UY";
			AssertNoNotifications(bill.ABL_RN_NKNotifyPartyCountryInfo);

			bill.ABL_RN_NKNotifyPartyCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKNotifyPartyCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#endregion

		public void TestFinalDestination()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			bill.ABL_RL_NKFinalDestination = ZString.Empty;
			AssertHasMessageError(bill.ABL_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered + " a Final Destination.");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_RL_NKFinalDestination = "ARBUE";
			AssertNoNotifications(bill.ABL_RL_NKFinalDestinationInfo);

			bill.ABL_RL_NKFinalDestination = ZString.Empty;
			AssertHasMessageError(bill.ABL_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered + " a Final Destination.");
		}

		public void TestGoodsDecription()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			bill.ABL_GoodsDescription = ZString.Empty;
			AssertHasMessageError(bill.ABL_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered + " a Goods' Description (on Bill).");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_GoodsDescription = "GOODS DESCRIPTION";
			AssertNoNotifications(bill.ABL_GoodsDescriptionInfo);

			bill.ABL_GoodsDescription = ZString.Empty;
			AssertHasMessageError(bill.ABL_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered + " a Goods' Description (on Bill).");
		}

		public void TestPrepaidCollect()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			AssertNoNotifications(bill.ABL_PrepaidCollectInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			AssertNoNotifications(bill.ABL_PrepaidCollectInfo);

			bill.ABL_PrepaidCollect = ZString.Empty;
			AssertHasMessageError(bill.ABL_PrepaidCollectInfo, MandatoryValidation.YouHaveNotEntered + " a Prepaid/Collect.");
		}

		public void TestVolume()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			bill.ABL_Volume = 0.00;
			AssertHasMessageError(bill.ABL_VolumeInfo, MandatoryValidation.YouHaveNotEntered + " a Volume (on Bill).");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_Volume = 1.10;
			AssertNoNotifications(bill.ABL_VolumeInfo);

			bill.ABL_Volume = 0.00;
			AssertHasMessageError(bill.ABL_VolumeInfo, MandatoryValidation.YouHaveNotEntered + " a Volume (on Bill).");
		}

		public void TestVolumeUQ()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill = header.Bills.AddNew();

			bill.ABL_VolumeUQ = ZString.Empty;
			AssertHasMessageError(bill.ABL_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered + " a Volume Unit (on Bill).");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
			AssertNoNotifications(bill.ABL_VolumeUQInfo);

			bill.ABL_VolumeUQ = ZString.Empty;
			AssertHasMessageError(bill.ABL_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered + " a Volume Unit (on Bill).");
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

		public void TestCheckABL_BillNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.Validation.ValidateABL_BillNumber();
			AssertHasMessageErrorContaining(bill.ABL_BillNumberInfo, "At least one Pack should be inserted");

			bill.Packs.AddNew();

			bill.Validation.ValidateABL_BillNumber();
			AssertNoMessageErrorContaining(bill.ABL_BillNumberInfo, "At least one Pack should be inserted");
		}
	}
}
