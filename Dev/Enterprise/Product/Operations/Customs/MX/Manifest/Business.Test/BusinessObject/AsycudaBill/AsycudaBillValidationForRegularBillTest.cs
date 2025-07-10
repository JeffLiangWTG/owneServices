using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	class AsycudaBillValidationForRegularBillTest : BusinessObjectLookupsTestCase
	{
		public void TestConsigneeMandatoryData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var bill = header.Bills.AddNew();

			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ConsigneeRegNo = "123456789";
			AssertNoNotifications(bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKConsigneeCountryInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ConsigneePostcode = ZString.Empty;
			AssertNoNotifications(bill.ABL_ConsigneePostcodeInfo);
			bill.ABL_RN_NKConsigneeCountry = "MX";
			bill.ABL_ConsigneePostcode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ConsigneePostcodeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ConsigneePostcode = "12345";
			AssertNoNotifications(bill.ABL_ConsigneePostcodeInfo);
			bill.ABL_ConsigneeState = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeStateInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ConsigneeState = "ABC";
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeStateInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_RN_NKConsigneeCountry = "UY";
			AssertNoNotifications(bill.ABL_ConsigneePostcodeInfo);
			bill.ABL_ConsigneeState = ZString.Empty;
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeStateInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_ConsigneeName = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeNameInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ConsigneeName = "ConsigneeName";
			AssertNoNotifications(bill.ABL_ConsigneeNameInfo);
			bill.ABL_ConsigneeCity = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeCityInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ConsigneeCity = "ConsigneeCity";
			AssertNoNotifications(bill.ABL_ConsigneeCityInfo);
			bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKConsigneeCountryInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ConsigneeState = ZString.Empty;
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeStateInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_RN_NKConsigneeCountry = "MX";
			AssertNoNotifications(bill.ABL_RN_NKConsigneeCountryInfo);
			bill.ABL_ConsigneeState = ZString.Empty;
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeStateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestNotifyPartyMandatoryData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var bill = header.Bills.AddNew();

			bill.ABL_NotifyPartyName = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyNameInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_NotifyPartyName = "Notify Party Name";
			AssertNoNotifications(bill.ABL_NotifyPartyNameInfo);

			bill.ABL_NotifyPartyStreet1 = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyStreet1Info, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_NotifyPartyStreet1 = "Notify Party Street";
			AssertNoNotifications(bill.ABL_NotifyPartyStreet1Info);

			bill.ABL_RN_NKNotifyPartyCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKNotifyPartyCountryInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_NotifyPartyPostcode = ZString.Empty;
			AssertNoNotifications(bill.ABL_NotifyPartyPostcodeInfo);
			bill.ABL_RN_NKNotifyPartyCountry = "MX";
			bill.ABL_NotifyPartyPostcode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyPostcodeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_NotifyPartyPostcode = "12345";
			AssertNoNotifications(bill.ABL_NotifyPartyPostcodeInfo);
			bill.ABL_NotifyPartyState = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyStateInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_NotifyPartyState = "ABC";
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyStateInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_RN_NKNotifyPartyCountry = "UY";
			AssertNoNotifications(bill.ABL_NotifyPartyPostcodeInfo);
			bill.ABL_NotifyPartyState = ZString.Empty;
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyStateInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_NotifyPartyName = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyNameInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_NotifyPartyName = "NotifyPartyName";
			AssertNoNotifications(bill.ABL_NotifyPartyNameInfo);
			bill.ABL_NotifyPartyCity = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyCityInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_NotifyPartyCity = "NotifyPartyCity";
			AssertNoNotifications(bill.ABL_NotifyPartyCityInfo);
			bill.ABL_RN_NKNotifyPartyCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKNotifyPartyCountryInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_NotifyPartyState = ZString.Empty;
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyStateInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_RN_NKNotifyPartyCountry = "MX";
			AssertNoNotifications(bill.ABL_RN_NKNotifyPartyCountryInfo);
			bill.ABL_NotifyPartyState = ZString.Empty;
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyStateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestShipperMandatoryData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var bill = header.Bills.AddNew();

			bill.ABL_ShipperRegNo = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ShipperRegNoInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ShipperRegNo = "123456789";
			AssertNoNotifications(bill.ABL_ShipperRegNoInfo);

			bill.ABL_RN_NKShipperCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKShipperCountryInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ShipperPostcode = ZString.Empty;
			AssertNoNotifications(bill.ABL_ShipperPostcodeInfo);
			bill.ABL_RN_NKShipperCountry = "MX";
			bill.ABL_ShipperPostcode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ShipperPostcodeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ShipperPostcode = "12345";
			AssertNoNotifications(bill.ABL_ShipperPostcodeInfo);
			bill.ABL_ShipperState = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ShipperStateInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ShipperState = "ABC";
			AssertNoMessageErrorContaining(bill.ABL_ShipperStateInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_RN_NKShipperCountry = "UY";
			AssertNoNotifications(bill.ABL_ShipperPostcodeInfo);
			bill.ABL_ShipperState = ZString.Empty;
			AssertNoMessageErrorContaining(bill.ABL_ShipperStateInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			bill.ABL_ShipperName = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ShipperNameInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ShipperName = "ShipperName";
			AssertNoNotifications(bill.ABL_ShipperNameInfo);
			bill.ABL_ShipperCity = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ShipperCityInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ShipperCity = "ShipperCity";
			AssertNoNotifications(bill.ABL_ShipperCityInfo);
			bill.ABL_RN_NKShipperCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKShipperCountryInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_ShipperState = ZString.Empty;
			AssertNoMessageErrorContaining(bill.ABL_ShipperStateInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ABL_RN_NKShipperCountry = "MX";
			AssertNoNotifications(bill.ABL_RN_NKShipperCountryInfo);
			bill.ABL_ShipperState = ZString.Empty;
			AssertNoMessageErrorContaining(bill.ABL_ShipperStateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCountryCodesValidData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var bill = header.Bills.AddNew();

			bill.ABL_RN_NKConsigneeCountry = "CL";
			AssertNoNotifications(bill.ABL_RN_NKConsigneeCountryInfo);
			bill.ABL_RN_NKConsigneeCountry = "VV";
			AssertHasMessageError(bill.ABL_RN_NKConsigneeCountryInfo, "The code you have selected is not in the list.");

			bill.ABL_RN_NKNotifyPartyCountry = "MX";
			AssertNoNotifications(bill.ABL_RN_NKNotifyPartyCountryInfo);
			bill.ABL_RN_NKNotifyPartyCountry = "XV";
			AssertHasMessageError(bill.ABL_RN_NKNotifyPartyCountryInfo, "The code you have selected is not in the list.");

			bill.ABL_RN_NKShipperCountry = "UY";
			AssertNoNotifications(bill.ABL_RN_NKShipperCountryInfo);
			bill.ABL_RN_NKShipperCountry = "ZS";
			AssertHasMessageError(bill.ABL_RN_NKShipperCountryInfo, "The code you have selected is not in the list.");
		}

		public void TestPrepaidCollect()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = header.Bills.AddNew();

			bill.ABL_PrepaidCollect = ZString.Empty;

			AssertHasMessageError(bill.ABL_PrepaidCollectInfo, MandatoryValidation.YouHaveNotEntered + " a Prepaid/Collect.");
		}

		public void TestVolume()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_Volume = 0.00;

			AssertHasMessageError(bill.ABL_VolumeInfo, MandatoryValidation.YouHaveNotEntered + " a Volume (on Bill).");
		}

		public void TestVolumeUQ()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_Volume = 5.0;
			bill.ABL_VolumeUQ = ZString.Empty;

			AssertHasMessageError(bill.ABL_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered + " a Volume Unit (on Bill).");
		}

		public void TestGoodsDecription()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_GoodsDescription = ZString.Empty;

			AssertHasMessageError(bill.ABL_GoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered + " a Goods' Description (on Bill).");
		}

		public void TestFinalDestination()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.ABL_RL_NKFinalDestination = "MXACA";
			AssertNoNotifications(bill.ABL_RN_NKNotifyPartyCountryInfo);

			bill.ABL_RL_NKFinalDestination = ZString.Empty;

			AssertHasMessageError(bill.ABL_RL_NKFinalDestinationInfo, MandatoryValidation.YouHaveNotEntered + " a Final Destination.");
		}

		public void TestGrossWeightIsEqualBetweenBillAndPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Weight = 4;
			pack1.APA_WeightUQ = Core.Constants.Weight.Kilograms;

			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_GrossWeight = 5;

			AssertHasMessageError("If the weight declared in the Bill does not match the sum of the weights of the Packs, a Message Error should be displayed.", bill.ABL_GrossWeightInfo, "Weight of packages (4) is not equal to manifest Gross Weight (5) in (KG)");

			var pack2 = bill.Packs.AddNew();
			pack2.APA_Weight = 3000;
			pack2.APA_WeightUQ = Core.Constants.Weight.Grams;

			bill.ABL_GrossWeight = 7;

			AssertNoMessageErrors("If the weight declared in the Bill matches the sum of the weights of the Packs, a Message Error should not be displayed.", bill.ABL_GrossWeightInfo);

			bill.Packs.RemoveAndDelete(pack2);
			bill.Validation.ValidateAll();

			AssertHasMessageError("If the weight declared in the Bill does not match the sum of the weights of the Packs, a Message Error should be displayed.", bill.ABL_GrossWeightInfo, "Weight of packages (4) is not equal to manifest Gross Weight (7) in (KG)");
		}

		public void TestVolumeIsEqualBetweenBillAndPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Volume = 4;
			pack1.APA_VolumeUQ = Core.Constants.Volume.Litre;

			bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
			bill.ABL_Volume = 5;

			AssertHasMessageError("If the volume declared in the Bill does not match the sum of the volume of the Packs, a Message Error should be displayed.", bill.ABL_VolumeInfo, "Volume of packages (4) is not equal to manifest Volume (5) in (L)");

			var pack2 = bill.Packs.AddNew();
			pack2.APA_Volume = 4000;
			pack2.APA_VolumeUQ = Core.Constants.Volume.CubicCentimeters;

			bill.ABL_Volume = 8;

			AssertNoMessageErrors("If the volume declared in the Bill matches the sum of the volume of the Packs, a Message Error should not be displayed.", bill.ABL_VolumeInfo);

			bill.Packs.RemoveAndDelete(pack2);
			bill.Validation.ValidateAll();

			AssertHasMessageError("If the volume declared in the Bill does not match the sum of the volume of the Packs, a Message Error should be displayed.", bill.ABL_VolumeInfo, "Volume of packages (4) is not equal to manifest Volume (8) in (L)");
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
