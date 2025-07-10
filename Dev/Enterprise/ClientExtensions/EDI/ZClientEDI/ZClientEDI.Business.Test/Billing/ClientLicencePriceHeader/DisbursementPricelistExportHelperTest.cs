using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class DisbursementPricelistExportHelperTest : TestCaseWithFactory
	{
		public void TestAddPriceItems()
		{
			var helper = new DisbursementPricelistExportHelperForTest();
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item1 = priceHeader.Items.AddNew();
			var item2 = priceHeader.Items.AddNew();
			var itemWithNoPrice = priceHeader.Items.AddNew();
			var nonCWNItem = priceHeader.Items.AddNew();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 15);
			priceHeader.L6_HasExchangeRates = true;
			item1.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item1.L7_Code = "BBB";
			item1.L7_Description = "Hellooooooooo";
			item1.L7_RX_NKCurrency = "AUD";
			item1.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			item1.L7_Price = 5.23;

			item2.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item2.L7_Code = "BBB";
			item2.L7_Description = "x";
			item2.L7_RN_NKDisbursementCountry = "NZ";
			item2.L7_RX_NKCurrency = "NZD";
			item2.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Import;
			item2.L7_Price = 23.52;

			itemWithNoPrice.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			itemWithNoPrice.L7_Code = "CCC";
			itemWithNoPrice.L7_Description = "Goodbye";
			itemWithNoPrice.L7_RN_NKDisbursementCountry = "AU";
			itemWithNoPrice.L7_RX_NKCurrency = "AUD";
			itemWithNoPrice.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			itemWithNoPrice.L7_Price = 0.0m;

			nonCWNItem.L7_Category = "XXX";
			nonCWNItem.L7_Code = "CCC";
			nonCWNItem.L7_Description = "z";
			nonCWNItem.L7_RX_NKCurrency = "AUD";
			nonCWNItem.L7_Price = 7.0m;
			Factory.Save();

			AssertEquals("Should be successful", true, string.IsNullOrEmpty(helper.AddPriceItems(priceHeader)));
			AssertEquals("Should only add items with a price and CargoWiseNext category", 2, helper.PriceItemsForSerializationExposed.Count);

			var item1Exported = helper.PriceItemsForSerializationExposed.First(x => x.Key == FormattableString.Invariant($"{BillingConstants.PriceHeaderType.CargoWiseNext}{BillingConstants.PriceHeaderType.CargoWiseNext}BBBALLAUD{priceHeader.L6_ValidFrom.ToDateTime()}"));
			AssertNotNull("Item 1 should be exported since it has a price", item1Exported);
			AssertEquals("Exported price should match item price", item1.L7_Price, item1Exported.Value.EPF_Price);
			AssertEquals("Exported price should match item price", item1.L7_Description, item1Exported.Value.EPF_Description);

			var item2Exported = helper.PriceItemsForSerializationExposed.First(x => x.Key == FormattableString.Invariant($"{BillingConstants.PriceHeaderType.CargoWiseNext}{BillingConstants.PriceHeaderType.CargoWiseNext}BBBNZIMPNZD{priceHeader.L6_ValidFrom.ToDateTime()}"));
			AssertNotNull("Item 2 should be exported since it has a price", item2Exported);
			AssertEquals("Exported price should match item price", item2.L7_Price, item2Exported.Value.EPF_Price);
			AssertEquals("Exported price should match item price", item2.L7_Description, item2Exported.Value.EPF_Description);
		}

		public void TestAddPriceItems_ShouldFailIfHasChanges()
		{
			var helper = new DisbursementPricelistExportHelper();
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 15);
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			AssertEquals("Precondition", string.Empty, item.L7_RN_NKDisbursementCountry);

			AssertEquals("Should prompt save before generation if it has changes", "Please save changes before generating the XML.", helper.AddPriceItems(priceHeader));

			Factory.Save();
			helper = new DisbursementPricelistExportHelper();
			AssertEquals("Should succeed since changes are saved", true, string.IsNullOrEmpty(helper.AddPriceItems(priceHeader)));

			item.L7_Description = "Change";
			helper = new DisbursementPricelistExportHelper();
			AssertEquals("Should prompt save before generation if it has changes", "Please save changes before generating the XML.", helper.AddPriceItems(priceHeader));

			Factory.Save();
			priceHeader.L6_DiscountCode = "V1";
			helper = new DisbursementPricelistExportHelper();
			AssertEquals("Should prompt save before generation if it has changes", "Please save changes before generating the XML.", helper.AddPriceItems(priceHeader));

			Factory.Save();
			helper = new DisbursementPricelistExportHelper();
			AssertEquals("Should succeed since changes are saved", true, string.IsNullOrEmpty(helper.AddPriceItems(priceHeader)));
		}

		public void TestAddPriceItems_ShouldFailIfNotCWN()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();

			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 15);
			priceHeader.L6_HasExchangeRates = true;
			item.L7_Category = "AAA";
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;

			Factory.Save();
			var helper = new DisbursementPricelistExportHelper();
			AssertEquals("Should show error since system code is ODM", "System Code must be CargoWise Next.", helper.AddPriceItems(priceHeader));

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			AssertEquals("Precondition", string.Empty, item.L7_RN_NKDisbursementCountry);
			Factory.Save();
			helper = new DisbursementPricelistExportHelper();
			AssertEquals("Should succeed since system code is CWN", true, string.IsNullOrEmpty(helper.AddPriceItems(priceHeader)));
		}

		public void TestAddPriceItems_ShouldFailIfNoAllDirectionFallback()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 15);
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Import;
			AssertEquals("Precondition", string.Empty, item.L7_RN_NKDisbursementCountry);

			Factory.Save();
			var helper = new DisbursementPricelistExportHelper();
			AssertEquals("Should show error since there's no ALL disbursement direction", "Directional Fallback Price Items must be specified (must set a base price and have blank Country).", helper.AddPriceItems(priceHeader));

			item.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			Factory.Save();
			AssertEquals("Should have no error since direction is ALL", true, string.IsNullOrEmpty(helper.AddPriceItems(priceHeader)));

			item.L7_RN_NKDisbursementCountry = "AU";
			Factory.Save();
			helper = new DisbursementPricelistExportHelper();
			AssertEquals("Should show error since the ALL disbursement direction country is not empty", "Directional Fallback Price Items must be specified (must set a base price and have blank Country).", helper.AddPriceItems(priceHeader));

			item.L7_RN_NKDisbursementCountry = string.Empty;
			item.L7_Price = 0.0m;
			Factory.Save();
			helper = new DisbursementPricelistExportHelper();
			AssertEquals("Should show error since the ALL disbursement direction price is not set", "Directional Fallback Price Items must be specified (must set a base price and have blank Country).", helper.AddPriceItems(priceHeader));
		}

		public void TestAddPriceItems_ShouldFailIfNotSpecifyingDirectionFallbacks()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var importItem = priceHeader.Items.AddNew();
			var exportItem = priceHeader.Items.AddNew();
			var domesticItem = priceHeader.Items.AddNew();
			var otherItem = priceHeader.Items.AddNew();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 15);
			importItem.L7_Category = exportItem.L7_Category = domesticItem.L7_Category = otherItem.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			importItem.L7_Code = exportItem.L7_Code = domesticItem.L7_Code = otherItem.L7_Code = "BBB";
			importItem.L7_Description = exportItem.L7_Description = domesticItem.L7_Description = otherItem.L7_Description = "Hellooooooooo";
			importItem.L7_Price = exportItem.L7_Price = domesticItem.L7_Price = otherItem.L7_Price = 5.23;
			importItem.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Import;
			exportItem.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Export;
			domesticItem.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Domestic;
			otherItem.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Other;
			AssertEquals("Precondition", string.Empty, importItem.L7_RN_NKDisbursementCountry);

			Factory.Save();
			var helper = new DisbursementPricelistExportHelper();
			AssertEquals("Should have no error since all 4 main directions are covered", true, string.IsNullOrEmpty(helper.AddPriceItems(priceHeader)));

			importItem.L7_RN_NKDisbursementCountry = "AU";
			Factory.Save();
			helper = new DisbursementPricelistExportHelper();
			AssertEquals("Should show error since one of the disbursement direction countries is not empty", "Directional Fallback Price Items must be specified (must set a base price and have blank Country).", helper.AddPriceItems(priceHeader));

			importItem.L7_RN_NKDisbursementCountry = string.Empty;
			exportItem.L7_Price = 0.0m;
			Factory.Save();
			helper = new DisbursementPricelistExportHelper();
			AssertEquals("Should show error since one of the disbursement direction prices is not set", "Directional Fallback Price Items must be specified (must set a base price and have blank Country).", helper.AddPriceItems(priceHeader));
		}

		public void TestRunValidation_ShouldFailIfHasChanges()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 15);
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			item.L7_RN_NKDisbursementCountry = string.Empty;

			AssertEquals("Should prompt save before generation if it has changes", "Please save changes before generating the XML.", DisbursementPricelistExportHelper.RunValidation(priceHeader));

			Factory.Save();
			AssertEquals("Should succeed since changes are saved", true, string.IsNullOrEmpty(DisbursementPricelistExportHelper.RunValidation(priceHeader)));

			item.L7_Description = "Change";
			AssertEquals("Should prompt save before generation if it has changes", "Please save changes before generating the XML.", DisbursementPricelistExportHelper.RunValidation(priceHeader));

			Factory.Save();
			priceHeader.L6_DiscountCode = "V1";
			AssertEquals("Should prompt save before generation if it has changes", "Please save changes before generating the XML.", DisbursementPricelistExportHelper.RunValidation(priceHeader));

			Factory.Save();
			AssertEquals("Should succeed since changes are saved", true, string.IsNullOrEmpty(DisbursementPricelistExportHelper.RunValidation(priceHeader)));
		}

		public void TestRunValidation_ShouldFailIfNoAllDirectionFallback()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 15);
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Import;
			AssertEquals("Precondition", string.Empty, item.L7_RN_NKDisbursementCountry);

			Factory.Save();
			AssertEquals("Should show error since there's no ALL disbursement direction", "Directional Fallback Price Items must be specified (must set a base price and have blank Country).", DisbursementPricelistExportHelper.RunValidation(priceHeader));

			item.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			Factory.Save();
			AssertEquals("Should have no error since direction is ALL", true, string.IsNullOrEmpty(DisbursementPricelistExportHelper.RunValidation(priceHeader)));

			item.L7_RN_NKDisbursementCountry = "AU";
			Factory.Save();
			AssertEquals("Should show error since the ALL disbursement direction country is not empty", "Directional Fallback Price Items must be specified (must set a base price and have blank Country).", DisbursementPricelistExportHelper.RunValidation(priceHeader));

			item.L7_RN_NKDisbursementCountry = string.Empty;
			item.L7_Price = 0.0m;
			Factory.Save();
			AssertEquals("Should show error since the ALL disbursement direction price is not set", "Directional Fallback Price Items must be specified (must set a base price and have blank Country).", DisbursementPricelistExportHelper.RunValidation(priceHeader));
		}

		public void TestRunValidation_ShouldFailIfNotSpecifyingDirectionFallbacks()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var importItem = priceHeader.Items.AddNew();
			var exportItem = priceHeader.Items.AddNew();
			var domesticItem = priceHeader.Items.AddNew();
			var otherItem = priceHeader.Items.AddNew();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 15);
			importItem.L7_Category = exportItem.L7_Category = domesticItem.L7_Category = otherItem.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			importItem.L7_Code = exportItem.L7_Code = domesticItem.L7_Code = otherItem.L7_Code = "BBB";
			importItem.L7_Description = exportItem.L7_Description = domesticItem.L7_Description = otherItem.L7_Description = "Hellooooooooo";
			importItem.L7_Price = exportItem.L7_Price = domesticItem.L7_Price = otherItem.L7_Price = 5.23;
			importItem.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Import;
			exportItem.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Export;
			domesticItem.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Domestic;
			otherItem.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.Other;
			AssertEquals("Precondition", string.Empty, importItem.L7_RN_NKDisbursementCountry);

			Factory.Save();
			AssertEquals("Should have no error since all 4 main directions are covered", true, string.IsNullOrEmpty(DisbursementPricelistExportHelper.RunValidation(priceHeader)));

			importItem.L7_RN_NKDisbursementCountry = "AU";
			Factory.Save();
			AssertEquals("Should show error since one of the disbursement direction countries is not empty", "Directional Fallback Price Items must be specified (must set a base price and have blank Country).", DisbursementPricelistExportHelper.RunValidation(priceHeader));

			importItem.L7_RN_NKDisbursementCountry = string.Empty;
			exportItem.L7_Price = 0.0m;
			Factory.Save();
			AssertEquals("Should show error since one of the disbursement direction prices is not set", "Directional Fallback Price Items must be specified (must set a base price and have blank Country).", DisbursementPricelistExportHelper.RunValidation(priceHeader));
		}

		public void TestSerializeData()
		{
			var helper = new DisbursementPricelistExportHelperForTest();
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item1 = priceHeader.Items.AddNew();
			var item2 = priceHeader.Items.AddNew();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 15);
			priceHeader.L6_HasExchangeRates = true;
			item1.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item1.L7_Code = "BBB";
			item1.L7_Description = "Hellooooooooo";
			item1.L7_RX_NKCurrency = "AUD";
			item1.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			item1.L7_Price = 5.23;

			item2.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item2.L7_Code = "CCC";
			item2.L7_Description = "Hellooooooooo";
			item2.L7_RN_NKDisbursementCountry = "AU";
			item2.L7_RX_NKCurrency = "NZD";
			item2.L7_DisbursementDirection = Enterprise.MasterFiles.Business.OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			item2.L7_Price = 23.52;
			Factory.Save();

			AssertEquals("Precondition", string.Empty, item1.L7_RN_NKDisbursementCountry);
			AssertEquals("Should be successful", true, string.IsNullOrEmpty(helper.AddPriceItems(priceHeader)));
			var result = helper.SerializePricelist();

			AssertEquals("Serialization should be successful", true, result.Success);
			AssertEquals("Error message should be empty", true, string.IsNullOrEmpty(result.ErrorMessage));
			AssertEquals("Rates should be serialized", false, string.IsNullOrEmpty(result.SeralizedValue.OuterXml));
		}

		public void TestSerializeData_NoPricelist()
		{
			var helper = new DisbursementPricelistExportHelper();
			var result = helper.SerializePricelist();
			AssertEquals("Should not succeed since there are no priceitems", false, result.Success);
			AssertEquals("Should not succeed since there are no priceitems", "No price items were available to be serialized.", result.ErrorMessage);
		}

		class DisbursementPricelistExportHelperForTest : DisbursementPricelistExportHelper
		{
			internal Dictionary<string, RefAccElectronicProcessingFee> PriceItemsForSerializationExposed => PriceItemsForSerialization;
		}
	}
}
