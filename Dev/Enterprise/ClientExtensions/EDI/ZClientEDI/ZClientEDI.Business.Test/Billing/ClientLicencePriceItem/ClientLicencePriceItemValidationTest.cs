using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicencePriceItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckL7_Code_Negative()
		{
			var priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.Validation.ValidateAll();
			AssertNoErrors(priceItem.L7_CodeInfo);

			priceItem.L7_Category = BillingConstants.BillingSystem.ODM;
			priceItem.L7_Code = BillingConstants.CoreModuleCode;
			priceItem.L7_Price = 5;
			AssertNoErrors(priceItem.L7_PriceInfo);

			priceItem.L7_Price = -5;
			AssertHasErrors(priceItem.L7_PriceInfo);

			priceItem.L7_Code = "XXX";
			AssertHasErrors(priceItem.L7_PriceInfo);

			priceItem.L7_Code = "DS1";
			AssertNoErrors(priceItem.L7_PriceInfo);

			priceItem.L7_Code = "DS2";
			AssertNoErrors(priceItem.L7_PriceInfo);

			priceItem.L7_Price = 5;
			AssertHasErrors(priceItem.L7_PriceInfo);
		}

		public void TestCheckL7_Code()
		{
			ClientLicencePriceItem priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.Validation.ValidateAll();
			AssertNoErrors(priceItem.L7_CodeInfo);

			priceItem.L7_Code = "XXX";
			AssertNoErrors("Not validating against the list, because there can be normal codes outside the list like Hosting, TestCore etc.", priceItem.L7_CodeInfo);

			priceItem.L7_Code = BillingConstants.CoreModuleCode;
			AssertNoErrors(priceItem.L7_CodeInfo);

			ClientLicencePriceItemCollection collection = new ClientLicencePriceItemCollection(Factory.New<ClientLicencePriceHeader>());
			ClientLicencePriceItem anotherPriceItem = collection.AddNew();
			anotherPriceItem.L7_Code = "AAA";

			priceItem = collection.AddNew();
			priceItem.L7_Code = "XXX";
			AssertNoErrors(priceItem.L7_CodeInfo);

			anotherPriceItem.L7_Code = "XXX";
			anotherPriceItem.L7_UnitBreak = 5;
			priceItem.Validation.ValidateL7_Code();
			AssertNoErrors(priceItem.L7_CodeInfo);

			anotherPriceItem.L7_UnitBreak = 0;
			priceItem.Validation.ValidateL7_Code();
			AssertHasErrors(priceItem.L7_CodeInfo);

			priceItem.L7_Ref4 = "AAAAA";
			anotherPriceItem.L7_Ref4 = "BBBBB";
			priceItem.Validation.ValidateL7_Code();
			AssertNoErrors(priceItem.L7_CodeInfo);
		}

		public void TestCheckL7_Code_NoDuplicateCode()
		{
			var registryValue = new CodeDescriptionPairList();
			registryValue.AddPairIfNotExist("#AA", "DESC_#AA");
			EDIDataRegistry.Instance.UnregisteredDevicePremiumTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "HST");
			var licence2 = BillingTestHelper.CreateLicence(Factory, "EEE", "DEF", "HST");

			var priceHeaderODM1 = licence1.Company.PriceHeaders.AddNew();
			priceHeaderODM1.L6_SystemCode = "ODM";
			var priceItemODM1 = priceHeaderODM1.Items.AddNew();
			priceItemODM1.L7_Code = "A01";

			var priceHeaderODM2 = licence2.Company.PriceHeaders.AddNew();
			priceHeaderODM2.L6_SystemCode = "ODM";
			var priceItemODM2 = priceHeaderODM2.Items.AddNew();
			priceItemODM2.L7_Code = "A01";
			var priceItemOdmCmp = priceHeaderODM2.Items.AddNew();

			var priceHeaderSTL = licence2.Company.PriceHeaders.AddNew();
			priceHeaderSTL.L6_SystemCode = "STL";
			var priceItemSTL1 = priceHeaderSTL.Items.AddNew();
			var priceItemSTL2 = priceHeaderSTL.Items.AddNew();
			priceItemSTL2.L7_Code = "A02";

			var priceHeaderLDS = licence2.Company.PriceHeaders.AddNew();
			priceHeaderLDS.L6_SystemCode = "LDS";
			var priceItem4 = priceHeaderLDS.Items.AddNew();
			priceItem4.L7_Code = "A00";

			var priceHeaderBorderWise = licence2.Company.PriceHeaders.AddNew();
			priceHeaderBorderWise.L6_SystemCode = PriceHeaderType.BorderWise;
			var priceItemBW = priceHeaderBorderWise.Items.AddNew();

			var priceHeaderEHUB = licence2.Company.PriceHeaders.AddNew();
			priceHeaderEHUB.L6_SystemCode = PriceHeaderType.EHub;
			var priceItemCMP = priceHeaderEHUB.Items.AddNew();

			Factory.Save();

			priceItemCMP.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			AssertNoErrors("when CMP only on eHUB prices", priceItemCMP.L7_CodeInfo);
			Factory.Save();

			priceItemSTL1.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			AssertNoErrors("CMP can also be on STL prices", priceItemSTL1.L7_CodeInfo);
			Factory.Save();

			priceItemOdmCmp.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			AssertNoErrors("CMP can also be on ODM prices", priceItemOdmCmp.L7_CodeInfo);
			Factory.Save();

			priceItemCMP.L7_Code = ZString.Empty;
			Factory.Save();
			priceItemCMP.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			AssertNoErrors("CMP can be on eHUB, STL and ODM prices all at once", priceItemCMP.L7_CodeInfo);
			Factory.Save();

			priceItemSTL1.L7_Code = "A01";
			AssertNoErrors("Main price item code A01", priceItemSTL1.L7_CodeInfo);
			Factory.Save();

			priceItem4.L7_Code = "A01";
			AssertHasErrors("A01 is used as ODM price item code", priceItem4.L7_CodeInfo);

			priceItem4.L7_Code = "B01";
			AssertNoNotifications("B01 is not used yet", priceItem4.L7_CodeInfo);

			var priceItem4c = priceHeaderSTL.Items.AddNew();
			priceItem4c.L7_Code = "B01";
			AssertHasWarnings("B01 is used as LDS price item code", priceItem4c.L7_CodeInfo);
			AssertNoErrors("B01 is used as LDS price item code", priceItem4c.L7_CodeInfo);

			priceItem4.Validation.ValidateL7_Code();
			AssertHasWarnings("B01 is used as STL price item code", priceItem4.L7_CodeInfo);
			AssertNoErrors("B01 is used as STL price item code", priceItem4.L7_CodeInfo);

			priceItemBW.L7_Code = "A01";
			AssertNoNotifications("BorderWise can have the same codes as STL", priceItemBW.L7_CodeInfo);

			priceItemBW.L7_Code = "#AA";
			AssertHasError("UnregisteredDevicePremiumTypes", priceItemBW.L7_CodeInfo, "Code -#AA has already been used by Registry. WiseTech Global Client Extensions -> Licence Billing -> STL -> Un-registered Device Premium Types");

			var priceHeaderCWN = licence2.Company.PriceHeaders.AddNew();
			priceHeaderCWN.L6_SystemCode = PriceHeaderType.CargoWiseNext;
			var priceItemSHD_1 = priceHeaderCWN.Items.AddNew();
			priceItemSHD_1.L7_Code = "SHD";
			priceItemSHD_1.L7_Category = PriceHeaderType.CargoWiseNext;
			var priceItemSHD_2 = priceHeaderCWN.Items.AddNew();
			priceItemSHD_2.L7_Code = "SHD";
			AssertHasError(priceItemSHD_2.L7_CodeInfo, "You can't have two price items with the same codes and same sub codes and same unit break.");
			priceItemSHD_2.L7_Category = PriceHeaderType.CargoWiseNext;
			priceItemSHD_2.RunPreSaveValidation();
			AssertNoErrors(priceItemSHD_2.L7_CodeInfo);
		}

		public void TestCheckL7_Code_NoDuplicateCode_UsedByOtherSystems()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "HST");

			var priceHeaderINV1 = licence1.Company.PriceHeaders.AddNew();
			priceHeaderINV1.L6_SystemCode = "INV";
			var priceItemINV1 = priceHeaderINV1.Items.AddNew();
			priceItemINV1.L7_Code = "IV1";

			var priceHeaderINV2 = licence1.Company.PriceHeaders.AddNew();
			priceHeaderINV2.L6_SystemCode = "INV";
			var priceItemINV2 = priceHeaderINV2.Items.AddNew();
			priceItemINV2.L7_Code = "IV1";
			AssertNoNotifications(priceItemINV2.L7_CodeInfo);

			var priceHeaderSTL = licence1.Company.PriceHeaders.AddNew();
			priceHeaderSTL.L6_SystemCode = "STL";
			var priceItemSTL = priceHeaderSTL.Items.AddNew();
			priceItemSTL.L7_Code = "IV1";

			priceHeaderINV2.RunPreSaveValidation();
			AssertHasWarning(priceItemINV2.L7_CodeInfo, "Code -IV1 is also on some STL price lists. For those STL price lists, the STL price will be used instead of this item.");

			var priceHeaderCMP = licence1.Company.PriceHeaders.AddNew();
			priceHeaderCMP.L6_SystemCode = "CMP";
			var priceItemCMP = priceHeaderCMP.Items.AddNew();
			priceItemCMP.L7_Code = "IV1";

			priceHeaderINV2.RunPreSaveValidation();
			AssertHasError(priceItemINV2.L7_CodeInfo, "Code -IV1 has already been used on one or more other price lists.");

			priceItemINV2.L7_Code = "IV2";
			AssertNoNotifications(priceItemINV2.L7_CodeInfo);
		}

		public void TestCheckL7_Code_MatchRegistryItemWhenUseMinimumUsageCode()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			CreateStandardPricesCompany();

			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var categories = new CodeDescriptionPairList(EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value);
			categories.AddPair("SMF", "SMF Category");
			EDIDataRegistry.Instance.BillingUsageCategoryCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categories);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SMF";
			priceList1.PriceListCode = "AXT";
			priceList1.Description = "ABC-SMF-AXT";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var minimumFeeSettings = new UsageMinimumFeeSettings();
			var minimumFeeUsage1 = minimumFeeSettings.MinimumFeeList.AddNew();
			minimumFeeUsage1.ProductCode = "ABC";
			minimumFeeUsage1.PriceListCode = "AXT";
			minimumFeeUsage1.MinimumFeeCode = "TTT";
			EDIDataRegistry.Instance.UsageMinimumFeeSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, minimumFeeSettings);

			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "HST");
			licence1.Company.Header.OH_Code = "WISGLOSYD2";
			var priceHeaderAXT = licence1.Company.PriceHeaders.AddNew();
			priceHeaderAXT.L6_SystemCode = "AXT";
			priceHeaderAXT.L6_LC = LicenceCompany.StandardPricesCompany.PK;
			var priceItem1 = priceHeaderAXT.Items.AddNew();
			priceItem1.L7_Code = "AXT";
			priceItem1.L7_FeeType = BillingConstants.FeeType.MinimumFee;
			priceItem1.Validation.ValidateL7_Code();

			AssertHasWarning(priceItem1.L7_CodeInfo, "Code doesn't match with usage code in WiseTech Global Client Extensions > Licence Billing > Products (non-CW1) Enabled for Minimum Fee usage.");
		}

		public void TestCheckL7_UnitBreak()
		{
			ClientLicencePriceItem priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.L7_FeeType = BillingConstants.FeeType.NamedUser;
			priceItem.Validation.ValidateAll();
			AssertNoErrors(priceItem.L7_UnitBreakInfo);

			priceItem.L7_UnitBreak = -5;
			AssertHasErrors("Please enter a valid break value.", priceItem.L7_UnitBreakInfo);

			priceItem.L7_UnitBreak = 5;
			AssertNoErrors(priceItem.L7_UnitBreakInfo);

			priceItem.L7_FeeType = BillingConstants.FeeType.Transactional;
			priceItem.Validation.ValidateL7_UnitBreak();
			AssertNoErrors(priceItem.L7_UnitBreakInfo);

			priceItem.L7_FeeType = BillingConstants.FeeType.Database;
			priceItem.Validation.ValidateL7_UnitBreak();
			AssertHasErrors("Fee type can only have zero break value: " + BillingConstants.FeeType.Database, priceItem.L7_UnitBreakInfo);

			priceItem.L7_FeeType = BillingConstants.FeeType.Licence;
			priceItem.Validation.ValidateL7_UnitBreak();
			AssertHasErrors("Fee type can only have zero break value: " + BillingConstants.FeeType.Licence, priceItem.L7_UnitBreakInfo);

			priceItem.L7_UnitBreak = 0;
			AssertNoErrors(priceItem.L7_UnitBreakInfo);
		}

		public void TestCheckL6_RX_NKCurrency()
		{
			ClientLicencePriceItem priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.Validation.ValidateAll();
			AssertNoErrors(priceItem.L7_RX_NKCurrencyInfo);

			priceItem.L7_RX_NKCurrency = "XXX";
			AssertHasErrors(priceItem.L7_RX_NKCurrencyInfo);

			priceItem.L7_RX_NKCurrency = "AUD";
			AssertNoErrors(priceItem.L7_RX_NKCurrencyInfo);
		}

		public void TestCheckL7_UnitBreakParentCode()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var itemParent = priceHeader.Items.AddNew();
			itemParent.L7_Category = BillingConstants.BillingSystem.STL;
			itemParent.L7_Code = "AAA";

			var item = priceHeader.Items.AddNew();
			item.L7_Category = BillingConstants.BillingSystem.STL;
			item.L7_Code = "BBB";
			item.L7_UnitBreakParentCode = "AAA";
			AssertHasError(item.L7_UnitBreakParentCodeInfo, "A value here requires a fee type of " + BillingConstants.FeeType.TransactionalOneVolumeBreak + " - " + BillingConstants.FeeTypeDescriptions.TransactionalOneVolumeBreak);

			item.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			item.Validation.ValidateL7_UnitBreakParentCode();
			AssertNoErrors(item.L7_UnitBreakParentCodeInfo);

			item.L7_Category = BillingConstants.BillingSystem.DeniedPartyScreening;
			item.Validation.ValidateL7_UnitBreakParentCode();
			AssertHasError(item.L7_UnitBreakParentCodeInfo, "Value must match an existing price code in the same category as this: " + item.L7_Category);
		}

		public void TestCheckL7_Language()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			item.L7_FeeType = "DAL";
			item.L7_Language = Core.SharedConstants.Languages.French;
			AssertNoErrors(item.L7_LanguageInfo);
			item.L7_Language = "1@";
			AssertHasError(item.L7_LanguageInfo, "Enter a valid selection.");

			item.L7_FeeType = "DAZ";
			item.L7_Language = Core.SharedConstants.Languages.ChineseSimplified;
			AssertNoErrors(item.L7_LanguageInfo);
			item.L7_Language = "2#";
			AssertHasError(item.L7_LanguageInfo, "Enter a valid selection.");

			item.L7_FeeType = "CC1";
			item.L7_Language = Core.SharedConstants.Languages.French;
			AssertHasError(item.L7_LanguageInfo, "Please do not enter a value.");
			item.L7_Language = "";
			AssertNoErrors(item.L7_LanguageInfo);
		}

		public void TestCheckL7_WebParentCode()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			item.L7_FeeType = "MUM";
			item.L7_WebParentCode = "FOR";
			AssertNoErrors(item.L7_WebParentCodeInfo);

			item.L7_WebParentCode = "";
			AssertHasError(item.L7_WebParentCodeInfo, "Please enter a value.");

			item.L7_WebParentCode = "FOR";
			AssertNoErrors(item.L7_WebParentCodeInfo);

			item.L7_FeeType = "DAT";
			item.L7_WebParentCode = "";
			AssertNoErrors(item.L7_WebParentCodeInfo);
		}

		public void TestCheckL7_ExchangeRateGroupCode()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_SystemCode = "STL";
			priceHeader.L6_HasExchangeRates = false;
			var item = priceHeader.Items.AddNew();
			item.L7_FeeType = "MUM";
			item.L7_WebParentCode = "FOR";
			item.L7_ExchangeRateGroupCode = "STL";
			AssertHasError(item.L7_ExchangeRateGroupCodeInfo, "Exchange Rate Group Code is only valid where the Price Header has Exchange Rates.");

			priceHeader.L6_HasExchangeRates = true;
			item.RunPreSaveValidation();
			AssertNoError(item.L7_ExchangeRateGroupCodeInfo, "Exchange Rate Group Code is only valid where the Price Header has Exchange Rates.");
		}

		public void TestCheckL7_ParentCode()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany);
			var itemA = BillingTestHelper.AddPriceItem(stlPrices, "AAA", FeeType.Transactional, "", 1m);
			var itemB = BillingTestHelper.AddPriceItem(stlPrices, "BBB", FeeType.Transactional, "", 5m);
			var minFeeItem = BillingTestHelper.AddPriceItem(stlPrices, new UsageCodeKey("CW1", "#MF"), FeeType.MinimumFee, 100m);
			itemB.L7_ParentCategory = BillingConstants.BillingSystem.DeniedPartyScreening;
			itemB.L7_ParentCode = "AAA";

			var ctrPriceList = stdLicCompany.PriceHeaders.AddNew();
			ctrPriceList.L6_PricelistVersion = "CTR V1";
			ctrPriceList.L6_SystemCode = BillingConstants.BillingSystem.GlobalContainerTracking;
			ctrPriceList.L6_RX_NKCurrency = "USD";
			ctrPriceList.L6_ValidFrom = new ZDateTime(2015, 1, 1);

			var ctrPriceItem1 = BillingTestHelper.AddPriceItem(ctrPriceList, new UsageCodeKey("CTR", "CTR"), FeeType.Transactional, 0.95m);
			ctrPriceItem1.L7_Description = "Container Tracking";

			itemB.Validation.ValidateL7_ParentCode();
			AssertListValidationInvalidCodeError(itemB.L7_ParentCodeInfo, true);

			itemB.L7_ParentCategory = BillingConstants.BillingSystem.STL;
			itemB.Validation.ValidateL7_ParentCode();
			AssertNoErrors(itemB.L7_ParentCodeInfo);

			itemB.L7_ParentCategory = ZString.Empty;
			itemB.Validation.ValidateL7_ParentCategory();
			AssertHasErrors(itemB.L7_ParentCategoryInfo);

			Factory.Save();
			itemB.Validation.ValidateL7_ParentCategory();
			AssertNoErrors(itemB.L7_ParentCategoryInfo);

			itemB.L7_ParentCategory = BillingConstants.BillingSystem.STL;
			itemA.L7_Category = BillingConstants.BillingSystem.Service;
			itemB.Validation.ValidateL7_ParentCode();
			AssertListValidationInvalidCodeError(itemB.L7_ParentCodeInfo, true);

			itemB.L7_ParentCategory = BillingConstants.BillingSystem.Service;
			itemB.Validation.ValidateL7_ParentCode();
			AssertNoErrors(itemB.L7_ParentCodeInfo);

			itemB.L7_ParentCode = "";
			AssertHasError(itemB.L7_ParentCodeInfo, "Parent Code is mandatory if Parent Category is entered.");

			ctrPriceItem1.L7_ParentCategory = "CW1";
			ctrPriceItem1.L7_ParentCode = "#MF";
			AssertNoErrors("min fee item from STL pricelist can be entered on universal pricelist", ctrPriceItem1.L7_ParentCodeInfo);
			AssertNoErrors("min fee item from STL pricelist can be entered on universal pricelist", ctrPriceItem1.L7_ParentCategoryInfo);
		}

		public void TestCheckProductProperties()
		{
			var priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.Validation.ValidateAll();
			AssertEquals("", priceItem.L7_ProductAvailability);
			AssertEquals("", priceItem.L7_ProductDisplayCategory);
			AssertNoErrors(priceItem.L7_ProductAvailabilityInfo);
			AssertNoErrors(priceItem.L7_ProductDisplayCategoryInfo);

			priceItem.L7_ProductAvailability = "Y";
			priceItem.L7_ProductDisplayCategory = "ACC";
			AssertNoErrors(priceItem.L7_ProductAvailabilityInfo);
			AssertNoErrors(priceItem.L7_ProductDisplayCategoryInfo);

			priceItem.L7_ProductAvailability = "@";
			priceItem.L7_ProductDisplayCategory = "@@@";
			AssertHasError(priceItem.L7_ProductAvailabilityInfo, "Enter a valid selection.");
			AssertHasError(priceItem.L7_ProductDisplayCategoryInfo, "Enter a valid selection.");

			priceItem.L7_ProductAvailability = "N";
			priceItem.L7_ProductDisplayCategory = "BWP";
			AssertNoErrors(priceItem.L7_ProductAvailabilityInfo);
			AssertNoErrors(priceItem.L7_ProductDisplayCategoryInfo);

			priceItem.L7_ProductAvailability = "";
			priceItem.L7_ProductDisplayCategory = "";
			AssertNoErrors(priceItem.L7_ProductAvailabilityInfo);
			AssertNoErrors(priceItem.L7_ProductDisplayCategoryInfo);
		}

		public void TestCheckL7_CountryTierCode_FeeType()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var priceItem = priceHeader.Items.AddNew();
			priceItem.Validation.ValidateAll();
			AssertNoErrors(priceItem.L7_CountryTierCodeInfo);

			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = "STL";
			mapping1.PriceCode = "FBL";
			mappingCollection.Add(mapping1);
			mapping1.MappingLines.AddNew("AU", "C01");

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);

			priceHeader.L6_SystemCode = "STL";
			priceItem.L7_Code = "FBL";

			priceItem.L7_CountryTierCode = "C01";
			AssertHasError(priceItem.L7_CountryTierCodeInfo, "Country Tier Code is only valid when the Price Item has a Country Tier Fee Type.");

			priceItem.L7_FeeType = FeeType.Transactional;
			AssertHasError(priceItem.L7_CountryTierCodeInfo, "Country Tier Code is only valid when the Price Item has a Country Tier Fee Type.");

			priceItem.L7_FeeType = FeeType.CountryTier;
			AssertNoErrors(priceItem.L7_CountryTierCodeInfo);

			priceItem.L7_CountryTierCode = string.Empty;
			AssertHasError(priceItem.L7_CountryTierCodeInfo, "Country Tier Code must be entered when the Price Item has a Country Tier Fee Type.");

			priceItem.L7_FeeType = FeeType.Transactional;
			AssertNoErrors(priceItem.L7_CountryTierCodeInfo);
		}

		public void TestCheckL7_CountryTierCode_Lookup()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var priceItem = priceHeader.Items.AddNew();
			priceItem.Validation.ValidateAll();
			AssertNoErrors(priceItem.L7_CountryTierCodeInfo);

			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = "STL";
			mapping1.PriceCode = "FBL";
			mapping1.MappingLines.AddNew("AU", "C01");

			var mapping2 = new CountryTierPriceCodeMapping();
			mapping2.SystemCode = "XXX";
			mapping2.PriceCode = "YYY";
			mapping2.MappingLines.AddNew("NZ", "C02");

			mappingCollection.Add(mapping1);
			mappingCollection.Add(mapping2);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);

			priceHeader.L6_SystemCode = "XXX";
			priceItem.L7_Code = "FBL";
			priceItem.L7_FeeType = FeeType.CountryTier;

			priceItem.L7_CountryTierCode = "C01";
			AssertHasError(priceItem.L7_CountryTierCodeInfo, "Enter a valid selection.");

			priceItem.L7_CountryTierCode = "C02";
			AssertHasError(priceItem.L7_CountryTierCodeInfo, "Enter a valid selection.");

			priceItem.L7_Code = "YYY";
			priceItem.Validation.ValidateL7_CountryTierCode();
			AssertNoErrors(priceItem.L7_CountryTierCodeInfo);

			priceItem.L7_CountryTierCode = "C01";
			AssertHasError(priceItem.L7_CountryTierCodeInfo, "Enter a valid selection.");

			priceHeader.L6_SystemCode = "STL";
			priceItem.L7_Code = "FBL";
			priceItem.Validation.ValidateL7_CountryTierCode();
			AssertNoErrors(priceItem.L7_CountryTierCodeInfo);
		}

		public void TestCheck_L7_Code_DuplicateCode_CountryTierCode()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var priceItem1 = priceHeader.Items.AddNew();
			var priceItem2 = priceHeader.Items.AddNew();

			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = "STL";
			mapping1.PriceCode = "FBL";
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");

			mappingCollection.Add(mapping1);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);

			priceHeader.L6_SystemCode = "STL";
			priceItem1.L7_Code = "FBL";
			priceItem2.L7_Code = "FBL";
			priceItem1.L7_FeeType = FeeType.CountryTier;
			priceItem2.L7_FeeType = FeeType.CountryTier;

			AssertHasError(priceItem2.L7_CodeInfo, "You can't have two price items with the same codes and same sub codes and same unit break.");

			priceItem1.L7_CountryTierCode = "C01";
			AssertNoErrors(priceItem1.L7_CodeInfo);
		}

		public void TestCheck_L7_CountryTierCode_DuplicateCode()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var priceItem1 = priceHeader.Items.AddNew();
			var priceItem2 = priceHeader.Items.AddNew();

			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = "STL";
			mapping1.PriceCode = "FBL";
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");

			mappingCollection.Add(mapping1);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);

			priceHeader.L6_SystemCode = "STL";
			priceItem1.L7_Code = "FBL";
			priceItem2.L7_Code = "FBL";
			priceItem1.L7_FeeType = FeeType.CountryTier;
			priceItem2.L7_FeeType = FeeType.CountryTier;

			priceItem1.L7_CountryTierCode = "C01";
			priceItem2.L7_CountryTierCode = "C01";
			AssertHasError(priceItem2.L7_CountryTierCodeInfo, "You can't have two price items with the same codes and country tier codes.");

			priceItem2.L7_CountryTierCode = "C02";
			priceItem1.Validation.ValidateL7_CountryTierCode();
			AssertNoErrors(priceItem1.L7_CountryTierCodeInfo);
			AssertNoErrors(priceItem2.L7_CountryTierCodeInfo);
		}

		public void TestCheck_L7_CountryTierCode_MissingCode()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var priceItem1 = priceHeader.Items.AddNew();
			var priceItem2 = priceHeader.Items.AddNew();
			var priceItem3 = priceHeader.Items.AddNew();

			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = "STL";
			mapping1.PriceCode = "FBL";
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");
			mapping1.MappingLines.AddNew("US", "C03");

			mappingCollection.Add(mapping1);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);

			priceHeader.L6_SystemCode = "STL";
			priceItem1.L7_Code = priceItem2.L7_Code = priceItem3.L7_Code = "FBL";
			AssertNoErrors(priceItem1.L7_CountryTierCodeInfo);
			priceItem1.L7_FeeType = priceItem2.L7_FeeType = priceItem3.L7_FeeType = FeeType.CountryTier;

			priceItem1.L7_CountryTierCode = "C01";
			AssertHasError(priceItem1.L7_CountryTierCodeInfo, "You must also add a country tier price item for country tier code: C02");
			AssertHasError(priceItem1.L7_CountryTierCodeInfo, "You must also add a country tier price item for country tier code: C03");

			priceItem2.L7_CountryTierCode = "C02";
			priceItem1.Validation.ValidateL7_CountryTierCode();
			AssertNoError(priceItem1.L7_CountryTierCodeInfo, "You must also add a country tier price item for country tier code: C02");
			AssertHasError(priceItem1.L7_CountryTierCodeInfo, "You must also add a country tier price item for country tier code: C03");

			priceItem3.L7_CountryTierCode = "C03";
			priceItem1.Validation.ValidateL7_CountryTierCode();
			priceItem2.Validation.ValidateL7_CountryTierCode();
			AssertNoErrors(priceItem1.L7_CountryTierCodeInfo);
			AssertNoErrors(priceItem2.L7_CountryTierCodeInfo);
			AssertNoErrors(priceItem3.L7_CountryTierCodeInfo);
		}

		public void TestCheck_L7_CountryTierCode_DuplicatePrice()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var priceItem1 = priceHeader.Items.AddNew();
			var priceItem2 = priceHeader.Items.AddNew();

			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = "STL";
			mapping1.PriceCode = "FBL";
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");

			mappingCollection.Add(mapping1);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);

			priceHeader.L6_SystemCode = "STL";
			priceItem1.L7_Code = priceItem2.L7_Code = "FBL";
			AssertNoErrors(priceItem1.L7_CountryTierCodeInfo);
			priceItem1.L7_FeeType = priceItem2.L7_FeeType = FeeType.CountryTier;
			priceItem1.L7_CountryTierCode = "C01";
			priceItem2.L7_CountryTierCode = "C02";
			priceItem1.L7_Description = "Tier 1";
			priceItem2.L7_Description = "Tier 2";

			priceItem1.L7_Price = priceItem2.L7_Price = 1;

			priceItem2.Validation.ValidateL7_CountryTierCode();
			AssertHasWarning(priceItem1.L7_CountryTierCodeInfo, "Potential duplicate (multiple country tiers with the same price).");
			AssertHasWarning(priceItem2.L7_CountryTierCodeInfo, "Potential duplicate (multiple country tiers with the same price).");

			priceItem2.L7_Price = 2;
			priceItem1.Validation.ValidateL7_CountryTierCode();
			AssertNoWarning(priceItem1.L7_CountryTierCodeInfo, "Potential duplicate (multiple country tiers with the same price).");
			AssertNoWarning(priceItem2.L7_CountryTierCodeInfo, "Potential duplicate (multiple country tiers with the same price).");
		}

		public void TestCheck_L7_CountryTierCode_DuplicateDescription()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var priceItem1 = priceHeader.Items.AddNew();
			var priceItem2 = priceHeader.Items.AddNew();

			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = "STL";
			mapping1.PriceCode = "FBL";
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");

			mappingCollection.Add(mapping1);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);

			priceHeader.L6_SystemCode = "STL";
			priceItem1.L7_Code = priceItem2.L7_Code = "FBL";
			AssertNoErrors(priceItem1.L7_CountryTierCodeInfo);
			priceItem1.L7_FeeType = priceItem2.L7_FeeType = FeeType.CountryTier;
			priceItem1.L7_CountryTierCode = "C01";
			priceItem2.L7_CountryTierCode = "C02";
			priceItem1.L7_Price = 1;
			priceItem2.L7_Price = 2;
			priceItem1.L7_Description = priceItem2.L7_Description = "Tier 1";

			priceItem2.Validation.ValidateL7_CountryTierCode();
			AssertHasWarning(priceItem1.L7_CountryTierCodeInfo, "Potential duplicate (multiple country tiers with the same description).");
			AssertHasWarning(priceItem2.L7_CountryTierCodeInfo, "Potential duplicate (multiple country tiers with the same description).");

			priceItem2.L7_Description = "Tier 2";
			priceItem1.Validation.ValidateL7_CountryTierCode();
			AssertNoWarning(priceItem1.L7_CountryTierCodeInfo, "Potential duplicate (multiple country tiers with the same description).");
			AssertNoWarning(priceItem2.L7_CountryTierCodeInfo, "Potential duplicate (multiple country tiers with the same description).");
		}

		public void TestValidateDisbursementProperties()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "HST");

			var priceHeaderCWN = licence1.Company.PriceHeaders.AddNew();
			priceHeaderCWN.L6_SystemCode = PriceHeaderType.CargoWiseNext;
			var priceItem1 = priceHeaderCWN.Items.AddNew();
			priceItem1.L7_Code = "SHD";
			priceItem1.L7_DisbursementDirection = "EXP";
			priceItem1.L7_RN_NKDisbursementCountry = "AU";
			AssertHasError(priceItem1.L7_DisbursementDirectionInfo, "Disbursement Direction is only valid for Category CWN.");
			AssertHasError(priceItem1.L7_RN_NKDisbursementCountryInfo, "Disbursement Country is only valid for Category CWN.");
			priceItem1.L7_DisbursementDirection = "";
			priceItem1.L7_RN_NKDisbursementCountry = "";
			AssertNoErrors(priceItem1.L7_DisbursementDirectionInfo);
			AssertNoErrors(priceItem1.L7_RN_NKDisbursementCountryInfo);

			priceItem1.L7_Category = PriceHeaderType.CargoWiseNext;
			priceItem1.L7_DisbursementDirection = "!~~";
			priceItem1.L7_RN_NKDisbursementCountry = "!~";
			AssertHasError(priceItem1.L7_DisbursementDirectionInfo, "Enter a valid selection.");
			AssertHasError(priceItem1.L7_RN_NKDisbursementCountryInfo, "Enter a valid selection.");
			priceItem1.L7_DisbursementDirection = "";
			priceItem1.L7_RN_NKDisbursementCountry = "";
			AssertNoErrors(priceItem1.L7_DisbursementDirectionInfo);
			AssertNoErrors(priceItem1.L7_RN_NKDisbursementCountryInfo);
			priceItem1.L7_DisbursementDirection = "EXP";
			priceItem1.L7_RN_NKDisbursementCountry = "AU";
			AssertNoErrors(priceItem1.L7_DisbursementDirectionInfo);
			AssertNoErrors(priceItem1.L7_RN_NKDisbursementCountryInfo);

			var priceItem2 = priceHeaderCWN.Items.AddNew();
			priceItem2.L7_Code = "SHD";
			priceItem2.L7_Category = PriceHeaderType.CargoWiseNext;
			priceItem2.L7_DisbursementDirection = "EXP";
			priceItem2.L7_RN_NKDisbursementCountry = "AU";
			AssertHasError(priceItem2.L7_DisbursementDirectionInfo, "AU - EXP has already been used.");
			AssertHasError(priceItem2.L7_RN_NKDisbursementCountryInfo, "AU - EXP has already been used.");
			priceItem2.L7_DisbursementDirection = "IMP";
			AssertNoErrors(priceItem2.L7_DisbursementDirectionInfo);
			AssertNoErrors(priceItem2.L7_RN_NKDisbursementCountryInfo);
			priceItem2.L7_DisbursementDirection = "EXP";
			priceItem2.L7_RN_NKDisbursementCountry = "US";
			AssertNoErrors(priceItem2.L7_DisbursementDirectionInfo);
			AssertNoErrors(priceItem2.L7_RN_NKDisbursementCountryInfo);
		}

		public void TestCheckHostingUsageCode()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "HST");
			var priceHeader = licence1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = PriceHeaderType.STL;
			var priceItem1 = priceHeader.Items.AddNew();
			priceItem1.L7_Code = BillingConstants.Hosting.DataAccessCode;

			var priceItem2 = priceHeader.Items.AddNew();
			priceItem2.L7_Code = BillingConstants.Hosting.DataAccessByGBCode;

			priceHeader.RunPreSaveValidation();
			AssertHasError(priceItem1.L7_CodeInfo, "Only one Read-Only Access hosting price code can be added to a pricelist.");
			AssertHasError(priceItem2.L7_CodeInfo, "Only one Read-Only Access hosting price code can be added to a pricelist.");

			priceItem1.L7_Code = BillingConstants.Hosting.DataAccessCode;
			priceItem2.L7_Code = "ABC";
			priceHeader.RunPreSaveValidation();
			AssertNoErrors(priceItem1.L7_CodeInfo);
			AssertNoErrors(priceItem2.L7_CodeInfo);

			priceItem1.L7_Code = "ABC";
			priceItem2.L7_Code = BillingConstants.Hosting.DataAccessByGBCode;
			priceHeader.RunPreSaveValidation();
			AssertNoErrors(priceItem1.L7_CodeInfo);
			AssertNoErrors(priceItem2.L7_CodeInfo);

			priceItem2.L7_Code = BillingConstants.Hosting.DataAccessByGBCode;
			priceItem2.L7_FeeType = BillingConstants.FeeType.PerMBPerMonthMin1GB;
			priceHeader.RunPreSaveValidation();
			AssertHasError(priceItem2.L7_CodeInfo, "Fee Code M1G is not valid for this Price Code.");
			priceItem2.L7_FeeType = BillingConstants.FeeType.PerGBPerMonthMin1GB;
			priceHeader.RunPreSaveValidation();
			AssertNoErrors(priceItem2.L7_CodeInfo);

			priceItem2.L7_Code = BillingConstants.Hosting.DataAccessCode;
			priceItem2.L7_FeeType = BillingConstants.FeeType.PerGBPerMonthMin1GB;
			priceHeader.RunPreSaveValidation();
			AssertHasError(priceItem2.L7_CodeInfo, "Fee Code G1G is not valid for this Price Code.");
			priceItem2.L7_FeeType = BillingConstants.FeeType.PerMBPerMonthMin1GB;
			priceHeader.RunPreSaveValidation();
			AssertNoErrors(priceItem2.L7_CodeInfo);
		}

		void CreateStandardPricesCompany()
		{
			ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
		}
	}
}
