using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	public class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDocumentType()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			CombineAssertions(() =>
			{
				bill.DocumentType = ZString.Empty;
				AssertHasMessageErrorContaining(bill.DocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);

				bill.DocumentType = "XXX";
				AssertHasMessageError(bill.DocumentTypeInfo, ListValidation.InvalidCodeMessageError);

				bill.DocumentType = "DUE";
				AssertNoMessageErrorContaining(bill.DocumentTypeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckLocationOfGoods()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = "MUCR";

			var bill = header.Bills.AddNew();
			bill.ABL_GoodsLocation = ZString.Empty;
			AssertNoNotifications(bill.ABL_GoodsLocationInfo);

			header.AMA_ManifestType = "MER";
			bill.ABL_GoodsLocation = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckFRTMode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = "MER";
			var bill = header.Bills.AddNew();

			bill.BL_Service = ZBool.False;

			bill.FRTMode = "";
			AssertNoNotifications(bill.FRTModeInfo);

			bill.FRTMode = "PP";
			AssertNoMessageErrorContaining(bill.FRTModeInfo, "Leave blank if BL Service is selected.");
			AssertNoMessageErrorContaining(bill.FRTModeInfo, "Leave blank if Container Mode is Containerized.");
			AssertNoMessageErrorContaining(bill.FRTModeInfo, ListValidation.InvalidCodeMessageError);

			bill.FRTMode = "BG";
			AssertNoMessageErrorContaining(bill.FRTModeInfo, "Leave blank if Container Mode is Containerized.");
			AssertHasMessageErrorContaining(bill.FRTModeInfo, ListValidation.InvalidCodeMessageError);

			header.AMA_ContainerMode = "CNT";

			bill.FRTMode = "";
			AssertNoNotifications(bill.FRTModeInfo);

			bill.FRTMode = "PP";
			AssertHasMessageErrorContaining(bill.FRTModeInfo, "Leave blank if Container Mode is Containerized.");
			AssertNoMessageErrorContaining(bill.FRTModeInfo, ListValidation.InvalidCodeMessageError);

			bill.FRTMode = "BG";
			AssertHasMessageErrorContaining(bill.FRTModeInfo, "Leave blank if Container Mode is Containerized.");
			AssertHasMessageErrorContaining(bill.FRTModeInfo, ListValidation.InvalidCodeMessageError);

			bill.BL_Service = ZBool.True;

			bill.FRTMode = "";
			AssertNoNotifications(bill.FRTModeInfo);

			bill.FRTMode = "PP";
			AssertHasMessageErrorContaining(bill.FRTModeInfo, "Leave blank if BL Service is selected.");
		}

		public void TestCheckABL_RN_NKSellerCountry()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = "MUCR";

			var bill = header.Bills.AddNew();
			bill.ABL_RN_NKSellerCountry = ZString.Empty;
			AssertNoNotifications(bill.ABL_RN_NKSellerCountryInfo);

			bill.ABL_RN_NKSellerCountry = "XX";
			AssertHasMessageErrorContaining(bill.ABL_RN_NKSellerCountryInfo, ListValidation.InvalidCodeMessageError);

			header.AMA_ManifestType = "MER";

			bill.ABL_RN_NKSellerCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKSellerCountryInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_RN_NKSellerCountry = "XX";
			AssertHasMessageErrorContaining(bill.ABL_RN_NKSellerCountryInfo, ListValidation.InvalidCodeMessageError);

			bill.ABL_RN_NKSellerCountry = "BR";
			AssertNoNotifications(bill.ABL_RN_NKSellerCountryInfo);
		}

		public void TestCheckABL_ManifestQtyMatchSumOfPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_ManifestQty = 10;

			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 5;

			pack = bill.Packs.AddNew();
			pack.APA_PackQty = 5;

			bill.Validation.ValidateABL_ManifestQty();
			AssertNoMessageErrors(bill.ABL_ManifestQtyInfo);

			pack = bill.Packs.AddNew();
			pack.APA_PackQty = 1;

			bill.Validation.ValidateABL_ManifestQty();
			AssertHasMessageError(bill.ABL_ManifestQtyInfo, "Sum of packages' package counts (11) is not equal to manifest quantity");
		}

		public void TestGrossWeightIsEqualBetweenBillAndPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Weight = 3;
			pack1.APA_WeightUQ = Core.Constants.Weight.Kilograms;

			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_GrossWeight = 5;

			AssertHasMessageError(bill.ABL_GrossWeightInfo, "Weight of packages (3) is not equal to manifest Gross Weight (5) in (KG)");

			var pack2 = bill.Packs.AddNew();
			pack2.APA_Weight = 3000;
			pack2.APA_WeightUQ = Core.Constants.Weight.Grams;

			bill.ABL_GrossWeight = 6;

			AssertNoMessageErrors(bill.ABL_GrossWeightInfo);
		}

		public void TestVolumeIsEqualBetweenBillAndPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Volume = 3;
			pack1.APA_VolumeUQ = Core.Constants.Volume.Litre;

			bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
			bill.ABL_Volume = 5;

			AssertHasMessageError(bill.ABL_VolumeInfo, "Volume of packages (3) is not equal to manifest Volume (5) in (L)");

			var pack2 = bill.Packs.AddNew();
			pack2.APA_Volume = 4000;
			pack2.APA_VolumeUQ = Core.Constants.Volume.CubicCentimeters;

			bill.ABL_Volume = 7;

			AssertNoMessageErrors(bill.ABL_VolumeInfo);
		}

		public void TestCheckABL_ConsigneeRegNo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;

			var bill = header.Bills.AddNew();

			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Uruguay;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Brazil;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_ConsigneeRegNo = "1996";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_ManifestType = BRManifestTypes.Codes.MUCR;

			bill.ABL_ConsigneeRegNo = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeRegNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckABL_ConsigneeRegNoType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;

			var bill = header.Bills.AddNew();

			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Uruguay;
			bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeRegNoTypeInfo, "CJN or CPF type should be selected.");

			bill.ABL_RN_NKConsigneeCountry = Core.Constants.CountryCodes.Brazil;
			bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeRegNoTypeInfo, "CJN or CPF type should be selected.");

			bill.ABL_ConsigneeRegNoType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeRegNoTypeInfo, "CJN or CPF type should be selected.");

			bill.ABL_ConsigneeRegNoType = OrgCusCode.CodeTypes.PassportID;
			bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeRegNoTypeInfo, "CJN or CPF type should be selected.");

			header.AMA_ManifestType = BRManifestTypes.Codes.MUCR;

			bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeRegNoTypeInfo, "CJN or CPF type should be selected.");
		}

		public void TestCheckABL_NotifyPartyRegNo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;

			var bill = header.Bills.AddNew();

			bill.ABL_RN_NKNotifyPartyCountry = Core.Constants.CountryCodes.Uruguay;
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_RN_NKNotifyPartyCountry = Core.Constants.CountryCodes.Brazil;
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			bill.ABL_NotifyPartyRegNo = "1996";
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyRegNoInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_ManifestType = BRManifestTypes.Codes.MUCR;

			bill.ABL_NotifyPartyRegNo = ZString.Empty;
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyRegNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckABL_NotifyPartyRegNoType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;

			var bill = header.Bills.AddNew();

			bill.ABL_RN_NKNotifyPartyCountry = Core.Constants.CountryCodes.Uruguay;
			bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyRegNoTypeInfo, "CJN or CPF type should be selected.");

			bill.ABL_RN_NKNotifyPartyCountry = Core.Constants.CountryCodes.Brazil;
			bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyRegNoTypeInfo, "CJN or CPF type should be selected.");

			bill.ABL_NotifyPartyRegNoType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyRegNoTypeInfo, "CJN or CPF type should be selected.");

			bill.ABL_NotifyPartyRegNoType = OrgCusCode.CodeTypes.PassportID;
			bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyRegNoTypeInfo, "CJN or CPF type should be selected.");

			header.AMA_ManifestType = BRManifestTypes.Codes.MUCR;

			bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyRegNoTypeInfo, "CJN or CPF type should be selected.");
		}

		public void TestCheckCEMercante()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;

			var bill = header.Bills.AddNew();
			var targetInfo = bill.CustomsOwnNumberInfo;

			bill.CustomsOwnNumber = "";
			bill.RunPreSaveValidation();
			AssertNoMessageError(targetInfo, "You have not entered a CE Merchant.");

			bill.BL_Service = ZBool.True;

			bill.RunPreSaveValidation();
			AssertHasMessageErrorContaining(targetInfo, "You have not entered a CE Merchant.");

			bill.CustomsOwnNumber = "1BR11111111255555555555555555554444";
			bill.RunPreSaveValidation();
			AssertNoMessageError(targetInfo, "You have not entered a CE Merchant.");
		}
	}
}
