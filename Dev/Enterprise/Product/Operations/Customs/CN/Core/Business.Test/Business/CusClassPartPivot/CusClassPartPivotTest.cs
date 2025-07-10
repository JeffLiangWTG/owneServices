using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestWrapperProperties()
		{
			var lookup = Factory.NewWithValidTestData<CusClassification>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_CC = lookup.PK;
			pivot.CI_OP = product.PK;
			pivot.CNC_CIQTariff = "TARIFF";
			pivot.CNC_ExpiryDate = new ZDateTime(2020, 02, 18);
			pivot.CNC_QualityGuaranteePeriod = 1;
			pivot.CNC_Brand = "Brand";
			pivot.CNC_Model = "Model";
			pivot.CNC_EndUse = "TT";
			pivot.CNC_OriginState = "CN";
			pivot.CNC_DestinationDistrict = "AU";
			pivot.CNC_OriginDistrict = "BJS";
			pivot.CNC_DestinationRegion = "SYD";
			pivot.CNC_OriginRegion = "BJ";
			pivot.CNC_TradeUnitQty = "KG";
			pivot.CNC_UNPackageMarking = "UNDG";
			pivot.CNC_NonDangerousChemicalFlag = false;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			pivot.CNC_OA_ManufacturerAddress = org.MainAddress.PK;
			Factory.Save();
			AssertEquals("No fields are saved in CI_AddInfo", ZString.Empty, pivot.CI_AddInfo);
			var newFactory = new BusinessObjectFactory();
			var cnClassification = newFactory.LoadTop1<CusCNClassification>(new ZQuery(CusCNClassificationSchema.CNC_CI, pivot.PK));
			AssertNotNull(cnClassification);
			AssertEquals("TARIFF", cnClassification.CNC_CIQTariff);
			AssertEquals(new ZDateTime(2020, 02, 18), cnClassification.CNC_ExpiryDate);
			AssertEquals(1, cnClassification.CNC_QualityGuaranteePeriod);
			AssertEquals("Brand", cnClassification.CNC_Brand);
			AssertEquals("Model", cnClassification.CNC_Model);
			AssertEquals("TT", cnClassification.CNC_EndUse);
			AssertEquals("CN", cnClassification.CNC_OriginState);
			AssertEquals("AU", cnClassification.CNC_DestinationDistrict);
			AssertEquals("BJS", cnClassification.CNC_OriginDistrict);
			AssertEquals("SYD", cnClassification.CNC_DestinationRegion);
			AssertEquals("BJ", cnClassification.CNC_OriginRegion);
			AssertEquals("KG", cnClassification.CNC_TradeUnitQty);
			AssertEquals("UNDG", cnClassification.CNC_UNPackageMarking);
			AssertEquals(false, cnClassification.CNC_NonDangerousChemicalFlag);
			AssertEquals(org.MainAddress.PK, cnClassification.CNC_OA_ManufacturerAddress);
		}

		public void TestCusCNClassificationNotCreatedAfterProductIsDeleted()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partPivot = product.PivotsForBinding.AddNew();
			var details = partPivot.Details;
			partPivot.Delete();
			AssertNoExceptionThrown(() => details = partPivot.Details);
			AssertNull("Should not return a CusCNClassification", details);
		}

		public void TestCusCNClassificationDeletedAfterPivotDeleted()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partPivot = product.PivotsForBinding.AddNew();
			AssertNotNull(partPivot.Details);
			var classPK = partPivot.Details.PK;
			partPivot.Delete();
			Factory.Save();
			AssertNull(new BusinessObjectFactory().Load<CusCNClassification>(classPK));
		}

		public void TestIsDangerousChemical()
		{
			var anotherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(anotherFactory);
			helper.CreateNewOrGetExistingDataGrouping("CN", "test country CN");
			anotherFactory.Save();
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNDangerousChemical, "China Dangerous Chemical");
			var codeList = helper.CreateNewOrGetExistingCusCodeList("CN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNDangerousChemical, "7664-41-7", "氨", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList.PK, "Alias", "液氨");
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			anotherFactory.Save();
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNAdditionalElements, "China Customs Tariff Additional Elements");
			helper.CreateNewOrGetExistingCusCodeList("CN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNAdditionalElements, "00005", "CAS", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, hsnTariffType.PK, "8476900001", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffAttribute("AdditionalInfo1", "00005", tariff);
			anotherFactory.Save();
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "8476900001";
			pivot.CNC_NameOfGoods = "氨";
			Assert(pivot.IsDangerousChemical);
			pivot.CNC_NameOfGoods = "test";
			Assert(!pivot.IsDangerousChemical);
			pivot.CNC_NameOfGoods = "液氨";
			Assert(pivot.IsDangerousChemical);
			pivot.CNC_NameOfGoods = "test";
			Assert(!pivot.IsDangerousChemical);
			pivot.CI_TariffNum = tariff.ZZ1_TariffCode;
			pivot.CNC_GoodsSpecModel = "7664-41-7";
			Assert(pivot.IsDangerousChemical);
		}

		public void TestCI_CC_ReadOnly()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "111111111";
			Assert("Should not able to edit CI_CC", pivot.CI_CCInfo.ReadOnly);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			ICusCodeDataTypeSupporter supporter = Factory.New<CusClassPartPivot>();
			AssertEquals(typeof(AdditionalInformation), supporter.GetCusCodeDataTypes()[Constants.CusCodeDataTypes.Codes.AdditionalInformation]);
		}

		public void TestIsEnteringOrExiting()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ZString.Empty;
			AssertEquals("Child Type is empty", EnteringOrExiting.Both, pivot.IsEnteringOrExiting);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("Child Type is HTI", EnteringOrExiting.Entering, pivot.IsEnteringOrExiting);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Child Type is HTE", EnteringOrExiting.Exiting, pivot.IsEnteringOrExiting);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			AssertEquals("Child Type is HTB", EnteringOrExiting.Both, pivot.IsEnteringOrExiting);
		}

		public void TestNameOfGoodsAndGoodsSpecModel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariff = helper.CreateCustomsTariff("2713200000", "00000", "00423", "00352", "99999");
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "2713200000";
			pivot.CNC_NameOfGoods = "XXX";
			pivot.CNC_GoodsSpecModel = "AAA|BBB|CCC";
			AssertEquals(4, pivot.AdditionalInformationCodes.Count);
			AssertEquals("AdditionalInformation 00000", "XXX", pivot.AdditionalInformationCodes["00000"].CY_Data);
			AssertEquals("AdditionalInformation 00423", "AAA", pivot.AdditionalInformationCodes["00423"].CY_Data);
			AssertEquals("AdditionalInformation 00352", "BBB", pivot.AdditionalInformationCodes["00352"].CY_Data);
			AssertEquals("AdditionalInformation 99999", "CCC", pivot.AdditionalInformationCodes["99999"].CY_Data);
			pivot.AdditionalInformationCodes["00000"].CY_Data = "111";
			pivot.AdditionalInformationCodes["00423"].CY_Data = "222";
			pivot.AdditionalInformationCodes["00352"].CY_Data = "333";
			pivot.AdditionalInformationCodes["99999"].CY_Data = "";
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			pivot.CI_OP = product.PK;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var newPivot = newFactory.Load<CusClassPartPivot>(pivot.PK);
			AssertEquals("NameOfGoods", "111", newPivot.CNC_NameOfGoods);
			AssertEquals("GoodsSpecModel", "222|333", newPivot.CNC_GoodsSpecModel);
			pivot.CNC_NameOfGoods = "";
			pivot.CNC_GoodsSpecModel = "";
			pivot.AdditionalInformationCodes.SetNameOfGoods("XXX");
			pivot.AdditionalInformationCodes.SetGoodsSpecModel(tariff, pivot.IsEnteringOrExiting, "AAA|BBB|CCC");
			AssertEquals("NameOfGoods", "", pivot.CNC_NameOfGoods);
			AssertEquals("GoodsSpecModel", "", pivot.CNC_GoodsSpecModel);
			AssertEquals(4, pivot.AdditionalInformationCodes.Count);
			AssertEquals("AdditionalInformation 00000", "XXX", pivot.AdditionalInformationCodes["00000"].CY_Data);
			AssertEquals("AdditionalInformation 00423", "AAA", pivot.AdditionalInformationCodes["00423"].CY_Data);
			AssertEquals("AdditionalInformation 00352", "BBB", pivot.AdditionalInformationCodes["00352"].CY_Data);
			AssertEquals("AdditionalInformation 99999", "CCC", pivot.AdditionalInformationCodes["99999"].CY_Data);
			Factory.Save();
			var anotherFactory = new BusinessObjectFactory();
			var anotherPivot = anotherFactory.Load<CusClassPartPivot>(pivot.PK);
			AssertEquals("NameOfGoods", "XXX", anotherPivot.CNC_NameOfGoods);
			AssertEquals("GoodsSpecModel", "AAA|BBB|CCC", anotherPivot.CNC_GoodsSpecModel);
		}

		public void TestOnSaving_CleanByTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200010", "00000", "00423", "99999");
			helper.CreateCustomsTariff("2713200090", "00000", "00352", "99999");
			Factory.Save();
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var code1 = pivot.AdditionalInformationCodes.AddNew("00000", "111");
			var code2 = pivot.AdditionalInformationCodes.AddNew("00423", "222");
			var code3 = pivot.AdditionalInformationCodes.AddNew("00352", "333");
			var code4 = pivot.AdditionalInformationCodes.AddNew("99999", "444");
			pivot.CI_TariffNum = "2713200010";
			Factory.Save();
			AssertEquals(3, pivot.AdditionalInformationCodes.Count);
			Assert("00352 should be deleted", code3.IsDeleted);
			pivot.CI_TariffNum = "2713200090";
			pivot.AdditionalInformationCodes["99999"].CY_Data = "";
			Factory.Save();
			AssertEquals(1, pivot.AdditionalInformationCodes.Count);
			Assert("00423 should be deleted", code2.IsDeleted);
			Assert("99999 should be deleted", code4.IsDeleted);
			pivot.CI_TariffNum = "";
			Factory.Save();
			AssertEquals(1, pivot.AdditionalInformationCodes.Count);
			Assert("00000 should NOT be deleted", !code1.IsDeleted);
		}

		public void TestCI_ChildType()
		{
			var testItem = Factory.New<CusClassPartPivot>();
			testItem.CNC_OriginDistrict = "10001";
			testItem.CNC_OriginRegion = "20001";
			testItem.CNC_DestinationDistrict = "10002";
			testItem.CNC_DestinationRegion = "20002";
			testItem.CI_RW_NKOriginState = "US1";
			testItem.CNC_OriginState = "30001";
			testItem.CI_ChildType = "HTB";
			AssertEquals("CNC_OriginDistrict should NOT be cleared.", "10001", testItem.CNC_OriginDistrict);
			AssertEquals("CNC_OriginRegion should NOT be cleared.", "20001", testItem.CNC_OriginRegion);
			AssertEquals("CNC_DestinationDistrict should NOT be cleared.", "10002", testItem.CNC_DestinationDistrict);
			AssertEquals("CNC_DestinationRegion should NOT be cleared.", "20002", testItem.CNC_DestinationRegion);
			AssertEquals("CI_RW_NKOriginState should NOT be cleared.", "US1", testItem.CI_RW_NKOriginState);
			AssertEquals("CNC_OriginState should NOT be cleared.", "30001", testItem.CNC_OriginState);
			testItem.CI_ChildType = "HTI";
			Assert("CNC_OriginDistrict should be cleared.", testItem.CNC_OriginDistrict.IsEmpty);
			Assert("CNC_OriginRegion should be cleared.", testItem.CNC_OriginRegion.IsEmpty);
			AssertEquals("CNC_DestinationDistrict should NOT be cleared.", "10002", testItem.CNC_DestinationDistrict);
			AssertEquals("CNC_DestinationRegion should NOT be cleared.", "20002", testItem.CNC_DestinationRegion);
			AssertEquals("CI_RW_NKOriginState should NOT be cleared.", "US1", testItem.CI_RW_NKOriginState);
			AssertEquals("CNC_OriginState should NOT be cleared.", "30001", testItem.CNC_OriginState);
			testItem.CI_ChildType = "HTE";
			Assert("CNC_DestinationDistrict should be cleared.", testItem.CNC_DestinationDistrict.IsEmpty);
			Assert("CNC_DestinationRegion should be cleared.", testItem.CNC_DestinationRegion.IsEmpty);
			Assert("CI_RW_NKOriginState should be cleared.", testItem.CI_RW_NKOriginState.IsEmpty);
			Assert("CNC_OriginState should be cleared.", testItem.CNC_OriginState.IsEmpty);
		}

		[TestDate(2018, 12, 12)]
		public void TestDefaultCIQOriginState()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNCIQStates, "OUT", "CIQ State Mapping", true);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNCIQStates, "TN22", "788067", new ZDateTime(2018, 12, 01), new ZDateTime(2018, 12, 30), Core.Constants.CountryCodes.China);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CNCIQStates, "UYAR", "858001", new ZDateTime(2018, 12, 01), new ZDateTime(2018, 12, 30), Core.Constants.CountryCodes.China);
			Factory.Save();
			var testItem = Factory.New<CusClassPartPivot>();
			testItem.CI_RN_NKCountryOfOrigin = "TN";
			testItem.CI_RW_NKOriginState = "22";
			AssertEquals("CIQ Origin State should have been set.", "788067", testItem.CNC_OriginState);
			testItem.CI_RN_NKCountryOfOrigin = "UY";
			testItem.CI_RW_NKOriginState = "AR";
			AssertEquals("CIQ Origin State should have been set.", "858001", testItem.CNC_OriginState);
			testItem.CI_RW_NKOriginState = "AU";
			AssertEquals("Origin State is defaulted with the ISO code of Goods Origin.", "858", testItem.CNC_OriginState);
		}

		public void TestDelete()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000", "00423", "00352", "99999");
			Factory.Save();
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_TariffNum = "2713200000";
			pivot.CNC_NameOfGoods = "XXX";
			pivot.CNC_GoodsSpecModel = "AAA|BBB|CCC";
			pivot.CargoAttributes.AddNew();
			Factory.Save();
			AssertEquals(5, Factory.Load<CusCodeData>(new ZQuery(CusCodeDataSchema.CY_ParentID, pivot.PK)).Length);
			var anotherFactory = new BusinessObjectFactory();
			anotherFactory.Load<CusClassPartPivot>(pivot.PK).Delete();
			AssertEquals(0, anotherFactory.Load<CusCodeData>(new ZQuery(CusCodeDataSchema.CY_ParentID, pivot.PK)).Length);
		}

		public void TestCloneCusCNSpecificData()
		{
			var lookup = Factory.NewWithValidTestData<CusClassification>();
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "123";
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_CC = lookup.PK;
			pivot.CI_OP = product.PK;
			pivot.CIQIngredient = "CIQIngredient";
			pivot.AdditionalInformationCodes.SetNameOfGoods("Name Of Goods");
			var attribute = pivot.CargoAttributes.AddNew();
			attribute.CY_Code = "COD";
			attribute.CY_Data = "Test Data";
			pivot.CNC_CIQTariff = "TARIFF";
			pivot.CNC_ExpiryDate = new ZDateTime(2020, 03, 23);
			pivot.CNC_QualityGuaranteePeriod = 10;
			pivot.CNC_Brand = "Brand";
			pivot.CNC_Model = "Model";
			pivot.CNC_EndUse = "TT";
			pivot.CNC_OriginState = "CN";
			pivot.CNC_DestinationDistrict = "AU";
			pivot.CNC_OriginDistrict = "BJS";
			pivot.CNC_DestinationRegion = "SYD";
			pivot.CNC_OriginRegion = "BJ";
			pivot.CNC_TradeUnitQty = "KG";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			pivot.CNC_OA_ManufacturerAddress = org.MainAddress.PK;
			var clonedPivot = pivot.Clone();
			AssertEquals("CIQIngredient", clonedPivot.CIQIngredient);
			AssertEquals("Name Of Goods", clonedPivot.AdditionalInformationCodes.GetNameOfGoods());
			AssertEquals("COD", clonedPivot.CargoAttributes[0].CY_Code);
			AssertEquals("Test Data", clonedPivot.CargoAttributes[0].CY_Data);
			var cnClassification = clonedPivot.Details;
			AssertNotNull(cnClassification);
			AssertEquals("TARIFF", clonedPivot.CNC_CIQTariff);
			AssertEquals(new ZDateTime(2020, 03, 23), clonedPivot.CNC_ExpiryDate);
			AssertEquals(10, clonedPivot.CNC_QualityGuaranteePeriod);
			AssertEquals("Brand", clonedPivot.CNC_Brand);
			AssertEquals("Model", clonedPivot.CNC_Model);
			AssertEquals("TT", clonedPivot.CNC_EndUse);
			AssertEquals("CN", clonedPivot.CNC_OriginState);
			AssertEquals("AU", clonedPivot.CNC_DestinationDistrict);
			AssertEquals("BJS", clonedPivot.CNC_OriginDistrict);
			AssertEquals("SYD", clonedPivot.CNC_DestinationRegion);
			AssertEquals("BJ", clonedPivot.CNC_OriginRegion);
			AssertEquals("KG", clonedPivot.CNC_TradeUnitQty);
			AssertEquals(org.MainAddress.PK, clonedPivot.CNC_OA_ManufacturerAddress);
		}

		public void TestNonDangerousChemicalFlagVisible()
		{
			var part = Factory.New<OrgSupplierPart>();
			var partPivot = part.PivotsForBinding.AddNew();
			var testCollection = partPivot.CargoAttributes as ICodeDescriptionOptionStorage;
			testCollection.AddNew(CargoAttributeList.Codes._31);
			AssertEquals("NonDangerousChemicalFlagVisible should be true for UNDG has value.", true, partPivot.NonDangerousChemicalFlagVisible);
			testCollection.RemoveAndDelete(testCollection.FindByCode(CargoAttributeList.Codes._31));
			AssertEquals("NonDangerousChemicalFlagVisible should be false for empty UNDG.", false, partPivot.NonDangerousChemicalFlagVisible);
			var undg = part.UNDGs.AddNew();
			undg.DI_IMOClass = "tt";
			partPivot.DangerousGoodsDGSubs = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals("NonDangerousChemicalFlagVisible should be true for UNDG has value.", true, partPivot.NonDangerousChemicalFlagVisible);
		}
	}
}
