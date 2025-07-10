using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackedItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPI_CustomsQty()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			packedItem.API_CustomsQty = -1;
			AssertHasError(packedItem.API_CustomsQtyInfo, AsycudaPackedItemValidation.NegativeAmountNotAllowed);

			packedItem.API_CustomsQty = 1;
			AssertNoError(packedItem.API_CustomsQtyInfo, AsycudaPackedItemValidation.NegativeAmountNotAllowed);
		}

		public void TestCheckAPI_Tariff()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();

			packedItem.API_Tariff = "010101";
			AssertHasMessageErrorContaining(packedItem.API_TariffInfo, "The code you have selected is not in the list");
			packedItem.API_Tariff = "900110";
			AssertNoMessageErrors(packedItem.API_TariffInfo);
			packedItem.API_Tariff = "01010";
			AssertHasMessageErrorContaining(packedItem.API_TariffInfo, "Only 4, 6, 8, 12 digits are allowed");
		}

		public void TestAPI_CustomsUQ()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			packedItem.API_CustomsQty = 1;
			packedItem.API_CustomsUQ = "KG";
			AssertNoErrorContaining(packedItem.API_CustomsUQInfo, MandatoryValidation.MustBeEntered);

			packedItem.API_CustomsUQ = "";
			AssertHasErrorContaining(packedItem.API_CustomsUQInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckAPI_GrossWeightAndUQ()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			packedItem.API_GrossWeight = -1;
			AssertHasError(packedItem.API_GrossWeightInfo, AsycudaPackedItemValidation.NegativeAmountNotAllowed);

			packedItem.API_GrossWeight = 1;
			AssertNoError(packedItem.API_GrossWeightInfo, AsycudaPackedItemValidation.NegativeAmountNotAllowed);

			packedItem.API_GrossWeightUQ = "KG";
			AssertNoErrorContaining(packedItem.API_GrossWeightUQInfo, MandatoryValidation.MustBeEntered);

			packedItem.API_GrossWeightUQ = "";
			AssertHasErrorContaining(packedItem.API_GrossWeightUQInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckAPI_NetWeightAndUQ()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			packedItem.API_NetWeight = -1;
			AssertHasError(packedItem.API_NetWeightInfo, AsycudaPackedItemValidation.NegativeAmountNotAllowed);

			packedItem.API_NetWeight = 1;
			AssertNoError(packedItem.API_NetWeightInfo, AsycudaPackedItemValidation.NegativeAmountNotAllowed);

			packedItem.API_NetWeightUQ = "KG";
			AssertNoErrorContaining(packedItem.API_NetWeightUQInfo, MandatoryValidation.MustBeEntered);

			packedItem.API_NetWeightUQ = "";
			AssertHasErrorContaining(packedItem.API_NetWeightUQInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckAPI_GoodsValueAndCurrency()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			packedItem.API_GoodsValue = -1;
			AssertHasError(packedItem.API_GoodsValueInfo, AsycudaPackedItemValidation.NegativeAmountNotAllowed);

			packedItem.API_GoodsValue = 1;
			AssertNoError(packedItem.API_GoodsValueInfo, AsycudaPackedItemValidation.NegativeAmountNotAllowed);

			packedItem.API_RX_NKGoodsValueCurrency = "USD";
			AssertNoErrorContaining(packedItem.API_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.MustBeEntered);

			packedItem.API_RX_NKGoodsValueCurrency = "";
			AssertHasErrorContaining(packedItem.API_RX_NKGoodsValueCurrencyInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckAPI_TaxAmount()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			packedItem.API_TaxAmount = -1;
			AssertHasError(packedItem.API_TaxAmountInfo, AsycudaPackedItemValidation.NegativeAmountNotAllowed);

			packedItem.API_TaxAmount = 1;
			AssertNoError(packedItem.API_TaxAmountInfo, AsycudaPackedItemValidation.NegativeAmountNotAllowed);
		}

		public void TestCheckGoodsType()
		{
			var helper = new ZZDataTestHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsType, "GoodsType");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, "DT", "Dutiable Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, "CT", "Controlled Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.GoodsType, "KD", "KD DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.GetSGPackedItemForTesting();
			var info = ((BusinessObject)packedItem).ZPropertyInfoHash.GetPropertySafe("GoodsType");
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			packedItem.GoodsType = "DT";
			AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			packedItem.GoodsType = "CT";
			AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
		}

		[TestDate(2018, 06, 01)]
		public void TestCheckGoodsTypeWithTariff()
		{
			var consignee = CreateOrg("SNOWSILL, SONYA", "CGNTEST1", "8AU0495926");

			var helper = new ZZDataTestHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsType, "GoodsType");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, "DT", "Dutiable Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, "NT", "Normal Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, "ME", "Major Exporter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.GoodsType, "KD", "KD DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_E_DEP = new ZDateTime(2017, 12, 2);

			var bill = header.Bills.AddNew();

			var pack = bill.Packs.AddNew();
			var packedItem = pack.GetSGPackedItemForTesting();

			header.AMA_ManifestType = "MGE";
			packedItem.API_Tariff = "25161100";
			packedItem.GoodsType = "NT";
			var info = ((BusinessObject)packedItem).ZPropertyInfoHash.GetPropertySafe("GoodsType");
			AssertHasMessageError(info, "'CT' must be used when the Tariff is not Blank and is a controlled type.");

			packedItem.API_Tariff = "93063019";
			packedItem.GoodsType = "NT";
			AssertHasMessageError(info, "'CT' must be used when the Tariff is not Blank and is a controlled type.");

			packedItem.API_Tariff = "01029010";
			packedItem.GoodsType = "NT";
			AssertNoMessageError(info, "'CT' must be used when the Tariff is not Blank and is a controlled type.");

			header.AMA_ManifestType = "MGI";
			packedItem.API_DutyAmount = 1;
			packedItem.GoodsType = "DT";
			AssertNoMessageError(info, "'DT' must be used when the Duty amount is not 0");
			packedItem.API_DutyAmount = 1;
			packedItem.GoodsType = "NT";
			AssertHasMessageError(info, "'DT' must be used when the Duty amount is not 0");
			packedItem.API_DutyAmount = 0;
			packedItem.GoodsType = "NT";
			AssertNoMessageError(info, "'DT' must be used when the Duty amount is not 0");

			bill.ABL_OA_Consignee = consignee.MainAddress.PK;
			packedItem.API_Tariff = "01029010";
			packedItem.GoodsType = "NT";
			AssertHasMessageError(info, "'ME' must be used when the Tariff is not Blank and is a controlled type.");
			packedItem.GoodsType = "ME";
			AssertNoMessageError(info, "'ME' must be used when the Tariff is not Blank and is a controlled type.");
		}

		public void TestMandatoryFieldsWhenManifestShowPackItemAttributeApplied()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.PackageTypes, "PK1", "PACKS1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();

			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();

			packedItem.API_TaxAmount = 1.0; //Must change to not the same value

			packedItem.API_GoodsDescription = "";
			packedItem.API_CustomsQty = 0.0;
			packedItem.API_CustomsUQ = "";
			packedItem.API_TaxAmount = 0.0;
			packedItem.API_RN_NKGoodsOrigin = "";

			AssertHasMessageErrorContaining(packedItem.API_GoodsDescriptionInfo, "Goods Description is a required field for Singapore");
			AssertHasMessageErrorContaining(packedItem.API_CustomsQtyInfo, "Customs Qty is a required field for Singapore");
			AssertHasMessageErrorContaining(packedItem.API_CustomsUQInfo, "You have not entered a Customs UQ");
			AssertHasMessageErrorContaining(packedItem.API_TaxAmountInfo, "Tax Amount is a required field for Singapore");
			AssertHasMessageErrorContaining(packedItem.API_RN_NKGoodsOriginInfo, "Goods Origin is a required field for Singapore");

			packedItem.API_GoodsDescription = "Description";
			packedItem.API_CustomsQty = 5.0;
			packedItem.API_CustomsUQ = "PK1";
			packedItem.API_TaxAmount = 5.0;
			packedItem.API_RN_NKGoodsOrigin = "AU";

			AssertNoMessageErrorContaining(packedItem.API_GoodsDescriptionInfo, "Goods Description is a required field for Singapore");
			AssertNoMessageErrorContaining(packedItem.API_CustomsQtyInfo, "Customs Qty is a required field for Singapore");
			AssertNoMessageErrorContaining(packedItem.API_CustomsUQInfo, "You have not entered a Customs UQ");
			AssertNoMessageErrorContaining(packedItem.API_CustomsUQInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(packedItem.API_TaxAmountInfo, "Tax Amount is a required field for Singapore");
			AssertNoMessageErrorContaining(packedItem.API_RN_NKGoodsOriginInfo, "Goods Origin is a required field for Singapore");

			packedItem.API_CustomsUQ = "KG";
			AssertHasMessageErrorContaining(packedItem.API_CustomsUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestGoodsTypeForSG()
		{
			var helper = new ZZDataTestHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsType, "GoodsType");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, "DT", "Dutiable Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, "CT", "Controlled Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, "NT", "Normal Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, "ME", "Major Exporter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.GoodsType, "KD", "KD DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var billSG = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;
			var pack = bill.Packs.AddNew();
			var packedItem = pack.GetSGPackedItemForTesting();
			packedItem.GoodsType = "KD";
			var info = ((BusinessObject)packedItem).ZPropertyInfoHash.GetPropertySafe("GoodsType");
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			packedItem.GoodsType = "DT";
			AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			packedItem.GoodsType = "CT";
			AssertNoMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);

			packedItem.GoodsType = ZString.Empty;
			AssertNoMessageErrors("For SG Manifest Goods type will always default back to 'NT' or 'ME' when blanked out", info);
			AssertEquals("NT", packedItem.GoodsType);

			billSG.SG_PartyStatus = "Y";
			packedItem.GoodsType = ZString.Empty;
			AssertNoMessageErrors("For SG Manifest Goods type will always default back to 'NT' or 'ME' when blanked out", info);
			AssertEquals("ME", packedItem.GoodsType);
		}

		public void TestCheckAPI_RN_NKGoodsOrigin()
		{
			var helper = new ZZDataTestHelper(Factory);
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.CreatePackedItemForTesting();
			packedItem.API_RN_NKGoodsOrigin = "~~";
			AssertHasMessageErrorContaining(packedItem.API_RN_NKGoodsOriginInfo, ListValidation.InvalidCodeMessageError);
			packedItem.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.SouthAfrica;
			AssertNoMessageErrors(packedItem.API_RN_NKGoodsOriginInfo);
		}

		public void TestCheckAPI_CustomsQtyAndUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.PackageTypes, "PKG", "PACKAGES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "SGA";

			var bill = header.Bills.AddNew();

			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();

			Factory.Save();

			packedItem.API_CustomsQty = 10;
			packedItem.API_CustomsUQ = "PKG";
			AssertNoMessageErrorContaining(packedItem.API_CustomsQtyInfo, "enter");
			AssertNoMessageErrorContaining(packedItem.API_CustomsUQInfo, "enter");
			AssertNoMessageErrorContaining(packedItem.API_CustomsUQInfo, "list");

			packedItem.API_CustomsQty = 0;
			packedItem.API_CustomsUQ = "";
			AssertHasMessageErrorContaining(packedItem.API_CustomsUQInfo, "enter");

			packedItem.API_CustomsUQ = "X";
			AssertHasMessageErrorContaining(packedItem.API_CustomsUQInfo, "list");
		}

		public void TestCheckAPI_CustomsQtyAndUQNotApplicableForFJ()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			packedItem.API_CustomsUQ = ZString.Empty;
			AssertNoMessageErrors(packedItem.API_CustomsUQInfo);
			packedItem.API_CustomsQty = ZDecimal.Zero;
			AssertNoMessageErrors(packedItem.API_CustomsQtyInfo);
		}

		[TestDate(2015, 1, 1)]
		public void TestCheckAPI_DutyAmount()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			header.AMA_ManifestType = "MGI";

			packedItem.API_Tariff = "22030090";
			packedItem.API_CustomsValue = 401m;
			packedItem.API_DutyAmount = 0m;
			AssertHasMessageError(packedItem.API_DutyAmountInfo, "Duty is applicable and cannot be calculated. Please supply the duty amount");
			packedItem.API_DutyAmount = 12m;
			AssertNoMessageErrors(packedItem.API_DutyAmountInfo);

			packedItem.API_Tariff = "22030090";
			packedItem.API_CustomsValue = 399m;
			packedItem.API_DutyAmount = 0m;
			AssertNoMessageErrors(packedItem.API_DutyAmountInfo);
			packedItem.API_DutyAmount = 12m;
			AssertNoMessageErrors(packedItem.API_DutyAmountInfo);

			packedItem.Header.AMA_ManifestType = "MGE";
			packedItem.API_DutyAmount = 0m;
			AssertNoMessageErrors(packedItem.API_DutyAmountInfo);
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariffType2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "22030090", new ZDateTime(2011, 11, 26), new ZDateTime(2018, 6, 23));
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType2.PK, "900110", ZDateTime.BrettsBirthday, ZDateTime.Now.AddDays(1));

			helper.CreateNomenclatureGroup("TR", "900110", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Optik lifler, optik Iif demetleri ve kabloları:", compositeKey: "18.90.01.10.01.01", nomenclatureGroupType: "TR");

			helper.CreateTariffUOM(tariff1, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "LTR");
			var com1 = helper.CreateCommodity(tariff1, "ZBP0AAF00CS", Core.Constants.CountryCodes.Singapore);
			helper.CreateTariffAttribute("ISIMPORTCONTROL", "Y", com1);
			helper.CreateTariffAttribute("COMMODITYTYPE", "ALC", tariff1);

			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "25161100", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff2, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "TNE");
			var com2 = helper.CreateCommodity(tariff2, "BCAOTHERG1", Core.Constants.CountryCodes.Singapore);
			helper.CreateTariffAttribute("ISIMPORTCONTROL", "Y", com2);
			helper.CreateTariffAttribute("ISTRANSHIPMENTCONTROL", "Y", com2);

			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "22030090", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff3, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "LTR");
			var com3 = helper.CreateCommodity(tariff3, "ZBP0AAF00CS", Core.Constants.CountryCodes.Singapore);
			helper.CreateTariffAttribute("ISIMPORTCONTROL", "Y", com3);
			helper.CreateTariffAttribute("COMMODITYTYPE", "ALC", tariff3);

			var tariff4 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "93063019", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff4, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "VAL");
			var com4 = helper.CreateCommodity(tariff4, "ANEXPL018", Core.Constants.CountryCodes.Singapore);
			helper.CreateTariffAttribute("ISIMPORTCONTROL", "Y", com4);
			helper.CreateTariffAttribute("ISEXPORTCONTROL", "Y", com4);
			helper.CreateTariffAttribute("ISTRANSHIPMENTCONTROL", "Y", com4);

			var tariff5 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "01029010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff5, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");
			var com5 = helper.CreateCommodity(tariff5, "VMA0OX", Core.Constants.CountryCodes.Singapore);

			Factory.Save();
		}

		OrgHeader CreateOrg(ZString companyName, ZString orgCode, ZString accountNumber)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = companyName;
			org.OH_Code = orgCode;
			org.MainAddress.OA_Address1 = companyName + " STR 1";
			org.MainAddress.OA_Address2 = companyName + " STR 2";
			org.MainAddress.OA_City = companyName + " CITY";
			org.MainAddress.OA_State = companyName + " STATE";
			org.MainAddress.OA_PostCode = accountNumber.Left(6) + "POST";
			org.CustomsCodes.AddNew("UAN", accountNumber, Core.Constants.CountryCodes.Singapore);
			org.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.PartyStatusType, "Y", Core.Constants.CountryCodes.Singapore);
			return org;
		}
	}
}
