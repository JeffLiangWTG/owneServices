using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem;
using static Enterprise.Customs.EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStoragePackedItem))]
	sealed class TemporaryStoragePackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Storage Packed Item", Factory.New<TemporaryStoragePackedItem>().HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				var packedItem = Factory.New<TemporaryStoragePackedItem>();
				AssertEquals("API_RX_NKGoodsValueCurrency Default", "EUR", packedItem.API_RX_NKGoodsValueCurrency);
				AssertEquals("API_GrossWeightUQ Default", "KG", packedItem.API_GrossWeightUQ);
			});
		}

		public void TestCalculateAndRefreshAllDutyAmountFromTariffRates()
		{
			AssertEquals("CalculateAndRefreshAllDutyAmountFromTariffRates is false", false, packedItem.CalculateAndRefreshAllDutyAmountFromTariffRates);
		}

		public void TestCusCodeDataTypes()
		{
			var supporter = packedItem as Integration.Customs.ICusCodeDataTypeSupporter;
			AssertContainsExactElementsInAnyOrder(new Type[] { typeof(SupplementaryCode) }, supporter.GetCusCodeDataTypes().Values);
		}

		public void TestReadOnlyPropertiesUnderTSDStatus()
		{
			header.ENSReuse = 0;

			foreach (var status in TemporaryStorageHeaderTest.GetNoEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				var propertyInfos = packedItem.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			return packedItem;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		public void TestAPI_ChemicalSubstanceCodeCaptions()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringData(packedItem.API_ChemicalSubstanceCodeInfo, "Customs Union and Statistics (CUS) Code", "CUS Code", "CUS", "The Customs Union and Statistics (CUS) Code is the identifier assigned within the European Customs Inventory of Chemical Substances (ECICS) to mainly chemical substances and preparations.");
			});
		}

		public void TestAPI_TariffIsUnformatted()
		{
			const string formattedTariff = "4901.99.90";
			const string unformattedTariff = "49019990";

			CombineAssertions(() =>
			{
				packedItem.API_Tariff = formattedTariff;
				AssertEquals(unformattedTariff, packedItem.API_Tariff);
				AssertEquals(formattedTariff, packedItem.API_FormattedTariff);
				packedItem.API_Tariff = unformattedTariff;
				AssertEquals(unformattedTariff, packedItem.API_Tariff);
				AssertEquals(formattedTariff, packedItem.API_FormattedTariff);
				packedItem.API_Tariff = "12 34.56. 78";
				AssertEquals("12345678", packedItem.API_Tariff);
				AssertEquals("1234.56.78", packedItem.API_FormattedTariff);
			});
		}

		public void TestSetTariffDoesNotDefaultGoodsDescriptionWhenAlreadyFilledWithUnofficialDescription()
		{
			TemporaryStorageTestHelper.SetUpTariff(Factory);
			packedItem.API_GoodsDescription = "I'M NOT EMPTY SO I STAY HERE!";
			packedItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
			AssertEquals("When setting API_Tariff and API_GoodsDescription is already filled with an unofficial description, API_GoodsDescription", "I'M NOT EMPTY SO I STAY HERE!", packedItem.API_GoodsDescription);
		}

		public void TestSetTariffDefaultsGoodsDescriptionWhenEmpty()
		{
			TemporaryStorageTestHelper.SetUpTariff(Factory);
			TemporaryStorageTestHelper.AssertCargoDescSetting_BY_HarmonisedTariff_Synchronizes_BY_Description(packedItem);
		}

		public void TestAPI_LineNo()
		{
			bill.PackedItems.Delete(packedItem);
			var packedItem1 = bill.PackedItems.AddNew();
			var packedItem2 = bill.PackedItems.AddNew();
			var packedItem3 = bill.PackedItems.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Line number 1", (ZShort)1, packedItem1.API_LineNo);
				AssertEquals("Line number 2", (ZShort)2, packedItem2.API_LineNo);
				AssertEquals("Line number 3", (ZShort)3, packedItem3.API_LineNo);

				bill.PackedItems.RemoveAndDelete(packedItem2);
				AssertEquals("Line number 1 stay same", (ZShort)1, packedItem1.API_LineNo);
				AssertEquals("Line number 3 change to 2", (ZShort)2, packedItem3.API_LineNo);
			});
		}

		public void TestAPI_RN_NKGoodsOrigin()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringData(packedItem.API_RN_NKGoodsOriginInfo, "Origin Country/Region", "Origin Ctry./Rgn.", "Origin", "Country/Region of Origin");
			});
		}

		public void TestAPI_Supplements()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringData(packedItem.API_SupplementsInfo, "Supplementary Codes", "Sup. Codes", "Sup. Codes", "Additional Supplementary Codes");
			});
		}

		public void TestAPI_GoodsValue()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringData(packedItem.API_GoodsValueInfo, "Monetary Value", "Mon. Value", "Mon. Value", "Item's Monetary Value");
			});
		}

		public void TestAPI_CustomsQty()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringData(packedItem.API_CustomsQtyInfo, "Supplementary Quantity", "Supplementary Qty", "Sup. Qty", "Supplementary Additional Quantity for Liability Amount Calculation");
			});
		}

		public void TestAPI_CustomsUQ()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringData(packedItem.API_CustomsUQInfo, "Supplementary Unit Quantity", "Supplementary Unit Qty", "Sup. Unit Qty", "Supplementary Additional Unit for Liability Amount Calculation");
			});
		}

		public void TestAPI_CustomsQty2()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringData(packedItem.API_CustomsQty2Info, "Second Quantity", "Second Qty", "Second Qty", "Second Additional Quantity for Liability Amount Calculation");
			});
		}

		public void TestAPI_CustomsQty3()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringData(packedItem.API_CustomsQty3Info, "Third Quantity", "Third Qty", "Third Qty", "Third Additional Quantity for Liability Amount Calculation");
			});
		}

		public void TestLiabilityAmountCaption()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringData(packedItem.LiabilityAmountInfo, "Liability Amount", "Liability Amount", "Lia. Amount", "Calculated Liability Amount");
			});
		}

		public void TestAdditionalSupplementaryCodesList()
		{
			var loader = new BaseSupplementaryCode.Loader(Factory);
			var code = loader.LoadOrCreate<SupplementaryCode, TemporaryStoragePackedItem>(packedItem, 1);
			code.CY_Code = "SUP";
			TemporaryStorageTestHelper.SetupTariffAndRateForAdditionalSupplementaryCodes(Factory);
			packedItem.API_Tariff = "1234512345";
			packedItem.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;
			var list = code.Lookups.CY_CodeList;
			CombineAssertions(() =>
			{
				AssertEquals("list count", 1, list.Count);
				Assert("list has code", list.ContainsCode("additionalcode"));
				AssertEquals("Description from Code", "additionalcode", list.GetDescriptionFromCode("additionalcode"));
			});
		}

		public void TestSupplementaryCodes()
		{
			CombineAssertions(() =>
			{
				var collection = packedItem.AdditionalSupplementaryCodes;
				AssertEquals("No Supplementary code captured.", 0, packedItem.SupplementaryCodes.Count());

				collection.AddNew("ABCD");
				collection.AddNew("EFGH");
				AssertEquals("Two supplementary codes captured.", 2, packedItem.SupplementaryCodes.Count());
				AssertContainsExactElementsInAnyOrder("", new ZString[] { "ABCD", "EFGH" }, packedItem.SupplementaryCodes.Select(x => x.CY_Code).ToArray());
			});
		}

		public void TestTemporaryStorageLinkPackages()
		{
			AssertType<TemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage>>(packedItem.TemporaryStorageLinkPackages);
		}

		public void TestPackagesPivot()
		{
			AssertType<AsycudaPackPackedItemPivotCollection<TemporaryStoragePackedItem, TemporaryStoragePack>>(packedItem.PackagesPivot);
		}

		public void TestValidationDecider()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var packedItem = header.Bills.AddNew().PackedItems.AddNew();
			AssertType<TemporaryStoragePackedItemValidationDecider>(packedItem.ValidationDecider);
		}

		public void TestToggleLinkageWithPackage()
		{
			AssertEquals("0 pivot should have been created.", 0, packedItem.PackagesPivot.Count);

			var temporaryStoragePack = Factory.New<TemporaryStoragePack>();
			var temporaryStoragePack2 = Factory.New<TemporaryStoragePack>();

			var pivot = packedItem.ToggleLinkageWithPackage(temporaryStoragePack, true);
			var pivot2 = packedItem.ToggleLinkageWithPackage(temporaryStoragePack2, true);

			CombineAssertions("pivots should have been created.", () =>
			{
				AssertEquals(2, packedItem.PackagesPivot.Count);
				AssertEquals("APP_APA_Pack should equals pivot temporaryStoragePack.PK", temporaryStoragePack.PK, pivot.APP_APA_Pack);
				AssertEquals("APP_APA_Pack should equals pivot temporaryStoragePack.PK", temporaryStoragePack2.PK, pivot2.APP_APA_Pack);
			});

			pivot = packedItem.ToggleLinkageWithPackage(temporaryStoragePack, false);
			CombineAssertions("A pivot should have been deleted.", () =>
			{
				packedItem.PackagesPivot.Reload(true);
				AssertEquals(1, packedItem.PackagesPivot.Count);
				AssertNull("APP_APA_Pack should equals pivot temporaryStoragePack.PK", pivot);
				AssertEquals("APP_APA_Pack should equals pivot temporaryStoragePack.PK", temporaryStoragePack2.PK, pivot2.APP_APA_Pack);
				AssertEquals("only pivot2 should have been in the PackagesPivot.", temporaryStoragePack2.PK, packedItem.PackagesPivot.Cast<AsycudaPackPackedItemPivot>().FirstOrDefault().APP_APA_Pack);
			});
		}

		public void TestSupplyChainActors()
		{
			var packedItem = (TemporaryStoragePackedItem)GetNewBusinessObject();
			var supplyChainActors = packedItem.SupplyChainActors;
			AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>(supplyChainActors);
			AssertEquals("Empty additionalInfo collection", 0, supplyChainActors.Count);
		}

		public void TestSupportingDocument()
		{
			var supportingDocuments = packedItem.SupportingDocuments;
			AssertType<TemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument>>(supportingDocuments);
			AssertEquals("Empty supportingDocument collection", 0, supportingDocuments.Count);

			var supportingDocument1 = supportingDocuments.AddNew();
			supportingDocument1.CSI_ReferenceNumber = "Reference1";
			CombineAssertions("Default value for new supportingDocument", () =>
			{
				AssertEquals("CSI_ParentTableCode", "API", supportingDocument1.CSI_ParentTableCode);
				AssertEquals("CSI_ParentID", packedItem.PK, supportingDocument1.CSI_ParentID);
				AssertEquals("CSI_Type", "SUP", supportingDocument1.CSI_Type);
			});

			var supportingDocument2 = supportingDocuments.AddNew();
			supportingDocument2.CSI_ReferenceNumber = "Reference2";
			Factory.Save();

			var reloadedBillPackedItem = Factory.Load<TemporaryStoragePackedItem>(packedItem.PK);
			var reloadedSupportingDocuments = reloadedBillPackedItem.SupportingDocuments;
			CombineAssertions("Reloaded supportingDocument collection", () =>
			{
				AssertEquals("Count", 2, reloadedSupportingDocuments.Count);
				AssertEquals("Element", "Reference1,Reference2", string.Join(",", reloadedSupportingDocuments.Select(x => x.CSI_ReferenceNumber)));
			});
		}

		public void TestPreviousDocument()
		{
			var document = packedItem.PreviousDocuments.AddNew();
			CombineAssertions("Default value for new previous document", () =>
			{
				AssertType<TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>>(packedItem.PreviousDocuments);
				AssertEquals("Parent of the previous document should be correct", packedItem, document.Parent);
				AssertEquals("CSI_ParentTableCode", "API", document.CSI_ParentTableCode);
				AssertEquals("CSI_ParentID", packedItem.PK, document.CSI_ParentID);
				AssertEquals("CSI_Type", "PRE", document.CSI_Type);
			});
		}

		public void TestAPI_GrossWeight()
		{
			var packedItem = Factory.New<TemporaryStoragePackedItem>();

			CombineAssertions("API_GrossWeight", () =>
			{
				AssertResourceStringData(packedItem.API_GrossWeightInfo, "Gross Weight");
				AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(TemporaryStoragePackedItem), nameof(TemporaryStoragePackedItem.API_GrossWeight), false, x => x.DecimalPlacesMember == "API_GrossWeightDecimalPlaces");
			});
		}

		public void TestAPI_GrossWeightUQ()
		{
			var packedItem = Factory.New<TemporaryStoragePackedItem>();
			var grossWeightUQ = typeof(TemporaryStoragePackedItem).GetProperty(nameof(packedItem.API_GrossWeightUQ));

			CombineAssertions("API_GrossWeightUQ", () =>
			{
				AssertResourceStringData(packedItem.API_GrossWeightUQInfo, "Gross Weight UQ", null, "UG");
				AssertEquals("List data source", "Lookups.GrossWeightUQList", grossWeightUQ.GetCustomAttribute<ListAttribute>().ListDataSourceMember);
			});
		}

		public void TestAPI_NetWeight()
		{
			var packedItem = Factory.New<TemporaryStoragePackedItem>();

			CombineAssertions("API_NetWeight", () =>
			{
				AssertResourceStringData(packedItem.API_NetWeightInfo, "Net Weight");
				AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(TemporaryStoragePackedItem), nameof(TemporaryStoragePackedItem.API_NetWeight), false, x => x.DecimalPlaces == 3);
			});
		}

		public void TestAPI_NetWeightUQ()
		{
			var packedItem = Factory.New<TemporaryStoragePackedItem>();
			var netWeightUQ = typeof(TemporaryStoragePackedItem).GetProperty(nameof(packedItem.API_NetWeightUQ));

			CombineAssertions("API_NetWeightUQ", () =>
			{
				AssertResourceStringData(packedItem.API_NetWeightUQInfo, "Net Weight UQ", null, "UN");
				AssertEquals("List data source", "Lookups.NetWeightUQList", netWeightUQ.GetCustomAttribute<ListAttribute>().ListDataSourceMember);
			});
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var supportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)packedItem).GetCusSupportingInfoTypes();

			CombineAssertions(() =>
			{
				AssertEquals("SupportingInfoTypes Count", 3, supportingInfoTypes.Count);

				AssertEquals("Expected type for AdditionalInfo", typeof(TemporaryStorageAdditionalInfo), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
				AssertEquals("Expected type for SupportingDocument", typeof(TemporaryStorageSupportingDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
				AssertEquals("Expected type for PreviousDocument", typeof(TemporaryStoragePreviousDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
			});
		}

		void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption = null, string shortCaption = null, string fullDescription = null)
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("Caption", caption, captionResourceString.Caption);

			if (mediumCaption is not null)
			{
				AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
			}

			if (shortCaption is not null)
			{
				AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
			}

			if (fullDescription is not null)
			{
				AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
			}
		}

		public void TestCustomsFirstUnitQtyKilograms()
		{
			CombineAssertions(() =>
			{
				packedItem.API_CustomsUQ = ZString.Empty;
				AssertEquals("CustomsFirstUnitQtyKilograms is KGM when API_CustomsUQ is empty", "KGM", packedItem.CustomsFirstUnitQtyKilograms);
				packedItem.API_CustomsUQ = "AA";
				AssertEquals("CustomsFirstUnitQtyKilograms is AA when API_CustomsUQ is set to AA", "AA", packedItem.CustomsFirstUnitQtyKilograms);
			});
		}

		public void TestCVDAdditionalCodesWithOutTariff()
		{
			AssertContainsExactElementsInAnyOrder(Array.Empty<ZString>(), packedItem.CVDAdditionalCodes);
		}

		public void TestTariffUnits()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Country", parentDataGrouping);

			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, impTariffType.PK, "12345678", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff, "CU2", "SET", Core.Constants.CountryCodes.Latvia);

			var frTradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.France, Core.Constants.CountryCodes.France, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(frTradeGroup, Core.Constants.CountryCodes.France);
			var itTradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.Italy, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(itTradeGroup, Core.Constants.CountryCodes.Italy);

			SetupRateFormula(helper, tariff, frTradeGroup, "DTY", "5.86*[LI]", Core.Constants.CountryCodes.France);
			SetupRateFormula(helper, tariff, frTradeGroup, "CVD", "6.330*[LPA]", Core.Constants.CountryCodes.France);
			SetupRateFormula(helper, tariff, itTradeGroup, "DTY", "7.86*[HLT]", Core.Constants.CountryCodes.Italy);
			SetupRateFormula(helper, tariff, itTradeGroup, "ADD", "8.330*[DTN]", Core.Constants.CountryCodes.Italy);

			Factory.Save();

			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_Tariff = "12345678";

			AssertEquals("API_CustomsUQ", packedItem.API_CustomsUQ, "SET");

			packedItem.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.France;
			AssertEquals("API_CustomsUQ2 FR", packedItem.API_CustomsUQ2, "LI");
			AssertEquals("API_CustomsUQ3 FR", packedItem.API_CustomsUQ3, "LPA");

			packedItem.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Italy;
			AssertEquals("API_CustomsUQ2 IT", packedItem.API_CustomsUQ2, "HLT");
			AssertEquals("API_CustomsUQ3 IT", packedItem.API_CustomsUQ3, "DTN");
		}

		public void TestTariffUnits_DutyRateFormulaWithTwoUOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Country", parentDataGrouping);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, impTariffType.PK, "12345678", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffUOM(tariff, "CU1", "KGM", Core.Constants.CountryCodes.Latvia);
			helper.CreateTariffUOM(tariff, "CU2", "LPA", Core.Constants.CountryCodes.Latvia);
			var tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Latvia, Core.Constants.CountryCodes.Latvia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Latvia);
			var ratePK = SetupRateFormula(helper, tariff, tradeGroup, "DTY", "0.600*[ASVX]+3.200*[HLT]", Core.Constants.CountryCodes.Latvia);
			helper.CreateRateUOM(ratePK, "ASVX");
			helper.CreateRateUOM(ratePK, "HLT");

			Factory.Save();

			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Latvia;
			packedItem.API_Tariff = "12345678";

			AssertEquals("API_CustomsUQ", packedItem.API_CustomsUQ, "LPA");
			AssertEquals("API_CustomsUQ2", packedItem.API_CustomsUQ2, "HLT");
			AssertEquals("API_CustomsUQ3", packedItem.API_CustomsUQ3, ZString.Empty);
		}

		public void TestAPI_CustomsUQMaxLength()
		{
			var maxLength = 4;

			CombineAssertions(() =>
			{
				AssertEntity<TemporaryStoragePackedItem>()
					.HasProperty(x => x.API_CustomsUQ)
					.WithMaxLength(maxLength);

				AssertEntity<TemporaryStoragePackedItem>()
					.HasProperty(x => x.API_CustomsUQ2)
					.WithMaxLength(maxLength);

				AssertEntity<TemporaryStoragePackedItem>()
					.HasProperty(x => x.API_CustomsUQ3)
					.WithMaxLength(maxLength);
			});
		}

		#region Duty Amount

		public void TestDutiableAmount() => CombineAssertions(() =>
		{
			TemporaryStorageTestHelper.SetUpTariff(Factory, countryCode);

			AssertDutiableAmount("Good Item", SetUpPackedItemForTest());

			void AssertDutiableAmount(ZString assertionText, TemporaryStoragePackedItemForTest goodsItem)
			{
				goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
				goodsItem.API_GoodsValue = 1_000m;
				goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = false;
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);
				AssertDutiesAndTaxes($"{assertionText}: Duty Amount Calculation when CalculateAndRefreshAllDutyAmountFromTariffRates is false", goodsItem, ChargeType.Duty);
				goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);
				AssertDutiesAndTaxes($"{assertionText}: Duty Amount Calculation when CalculateAndRefreshAllDutyAmountFromTariffRates is true", goodsItem, ChargeType.Duty, 120m);
			}
		});

		public void TestDutiableAmountWithSecondUnitNARxValue()
		{
			TemporaryStorageTestHelper.SetUpTariffSecondUnit(Factory, countryCode);

			AssertDutiableAmountWithSecondUnitNARxValue("Good Item", SetUpPackedItemForTest());

			void AssertDutiableAmountWithSecondUnitNARxValue(ZString assertionText, TemporaryStoragePackedItemForTest goodsItem)
			{
				goodsItem.API_Tariff = "9111100000";
				goodsItem.API_CustomsUQ2 = "NAR";
				goodsItem.API_GoodsValue = 200m;
				goodsItem.API_CustomsQty2 = 15.00m;
				goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);

				AssertDutiesAndTaxes($"{assertionText}: Calculate by NAR of the second unit and apply by price", goodsItem, ChargeType.Duty, 7.50m);
			}
		}

		public void TestDutiableAmountWithSecondUnitNARMax()
		{
			TemporaryStorageTestHelper.SetUpTariffSecondUnit(Factory, countryCode);

			AssertDutiableAmountWithSecondUnitNARMax("Good Item", SetUpPackedItemForTest());

			void AssertDutiableAmountWithSecondUnitNARMax(ZString assertionText, TemporaryStoragePackedItemForTest goodsItem)
			{
				goodsItem.API_Tariff = "9111100000";
				goodsItem.API_CustomsUQ2 = "NAR";
				goodsItem.API_GoodsValue = 200m;
				goodsItem.API_CustomsQty2 = 50.00m;
				goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);

				AssertDutiesAndTaxes($"{assertionText}: Calculate by NAR of the second unit and apply by MAX", goodsItem, ChargeType.Duty, 9.20m);
			}
		}

		public void TestDutiableAmountWithSecondUnitNARMin()
		{
			TemporaryStorageTestHelper.SetUpTariffSecondUnit(Factory, countryCode);

			AssertDutiableAmountWithSecondUnitNARMin("Good Item", SetUpPackedItemForTest());

			void AssertDutiableAmountWithSecondUnitNARMin(ZString assertionText, TemporaryStoragePackedItemForTest goodsItem)
			{
				goodsItem.API_Tariff = "9111100000";
				goodsItem.API_CustomsUQ2 = "NAR";
				goodsItem.API_GoodsValue = 200m;
				goodsItem.API_CustomsQty2 = 1.00m;
				goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);

				AssertDutiesAndTaxes($"{assertionText}: Calculate by NAR of the second unit and apply by MIN", goodsItem, ChargeType.Duty, 5.40m);
			}
		}

		public void TestDutiableAmountWithCustomsUnit()
		{
			TemporaryStorageTestHelper.SetUpTariffSecondUnit(Factory, countryCode);

			AssertDutiableAmountWithCustomsUnit("Good Item", SetUpPackedItemForTest());

			void AssertDutiableAmountWithCustomsUnit(ZString assertionText, TemporaryStoragePackedItemForTest goodsItem)
			{
				goodsItem.API_Tariff = "9111100000";
				goodsItem.API_CustomsUQ = "NAR";
				goodsItem.API_GoodsValue = 200m;
				goodsItem.API_CustomsQty = 15.00m;
				goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);

				AssertDutiesAndTaxes($"{assertionText}: Calculate by NAR of the customs unit and apply by price", goodsItem, ChargeType.Duty, 7.50m);
			}
		}

		public void TestDutiableAmountWithThirdUnit()
		{
			packedItem = header.Bills.AddNew().PackedItems.AddNew();

			TemporaryStorageTestHelper.SetUpTariffSecondUnit(Factory, countryCode);
			AssertDutiableAmountWithThirdUnit("Good Item", SetUpPackedItemForTest());

			void AssertDutiableAmountWithThirdUnit(ZString assertionText, TemporaryStoragePackedItemForTest goodsItem)
			{
				goodsItem.API_Tariff = "9111100000";
				goodsItem.API_CustomsUQ3 = "NAR";
				goodsItem.API_GoodsValue = 200m;
				goodsItem.API_CustomsQty3 = 15.00m;
				goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);

				AssertDutiesAndTaxes($"{assertionText}: Calculate by NAR of the third unit and apply by price", goodsItem, ChargeType.Duty, 7.50m);
			}
		}

		public void TestDutiableAmountWithAllUnits()
		{
			packedItem = header.Bills.AddNew().PackedItems.AddNew();

			TemporaryStorageTestHelper.SetUpTariffAllUnits(Factory, countryCode);

			AssertDutiableAmountWithAllUnits("Good Item", SetUpPackedItemForTest());

			void AssertDutiableAmountWithAllUnits(ZString assertionText, TemporaryStoragePackedItemForTest goodsItem)
			{
				goodsItem.API_Tariff = "9111200000";
				goodsItem.API_GoodsValue = 1000m;
				goodsItem.API_CustomsUQ3 = "NAR";
				goodsItem.API_CustomsQty3 = 15.00m;
				goodsItem.API_CustomsUQ = "KGM";
				goodsItem.API_CustomsQty = 15.00m;
				goodsItem.API_CustomsUQ2 = "DTN";
				goodsItem.API_CustomsQty2 = 15.00m;
				goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);

				AssertDutiesAndTaxes($"{assertionText}: Calculate DutyAmount with the three Customs fields", goodsItem, ChargeType.Duty, 20m, 7.50m, 7.50m, 7.50m, 0m);
			}
		}

		public void TestRecalculateDutyAmount()
		{
			packedItem = header.Bills.AddNew().PackedItems.AddNew();

			TemporaryStorageTestHelper.SetUpTariffAllUnits(Factory, countryCode);

			AssertRecalculateDutyAmount("Good Item", SetUpPackedItemForTest());

			void AssertRecalculateDutyAmount(ZString assertionText, TemporaryStoragePackedItemForTest goodsItem)
			{
				goodsItem.API_Tariff = "9111200000";
				goodsItem.API_GoodsValue = 1000m;
				goodsItem.API_CustomsUQ3 = "NAR";
				goodsItem.API_CustomsQty3 = 15.00m;
				goodsItem.API_CustomsUQ = "KGM";
				goodsItem.API_CustomsQty = 15.00m;
				goodsItem.API_CustomsUQ2 = "DTN";
				goodsItem.API_CustomsQty2 = 15.00m;
				goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);

				CombineAssertions(assertionText, () =>
				{
					AssertDutiesAndTaxes("[Precondition] DutyAmount value", goodsItem, ChargeType.Duty, 0m, 20m, 7.5m, 7.5m, 7.5m);

					goodsItem.API_CustomsQty = 10.00m;
					goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);
					AssertDutiesAndTaxes("DutyAmount is recalculated when Customs value change", goodsItem, ChargeType.Duty, 0m, 20m, 5m, 7.5m, 7.5m);

					goodsItem.API_CustomsQty2 = 10.00m;
					goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);
					AssertDutiesAndTaxes("DutyAmount is recalculated when Customs Second value change", goodsItem, ChargeType.Duty, 0m, 20m, 5m, 5m, 7.5m);

					goodsItem.API_CustomsQty3 = 10.00m;
					goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);
					AssertDutiesAndTaxes("DutyAmount is recalculated when Customs Third value change", goodsItem, ChargeType.Duty, 0m, 20m, 5m, 5m, 5m);
				});
			}
		}

		public void TestDutiableAmountWithSupplementaryCodes()
		{
			TemporaryStorageTestHelper.SetUpTariff(Factory, countryCode);

			AssertDutiableAmountWithSupplementaryCodes("Good Item", SetUpPackedItemForTest());

			void AssertDutiableAmountWithSupplementaryCodes(ZString assertionText, TemporaryStoragePackedItemForTest goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;

					goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
					goodsItem.API_GoodsValue = 1_000m;
					goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);
					AssertDutiesAndTaxes("No suplementary code", goodsItem, ChargeType.Duty, 120m);

					var supCode = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supCode.CY_Code = "ABCD";
					goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);
					AssertDutiesAndTaxes("With suplementary code", goodsItem, ChargeType.Duty, 80m);
				});
			}
		}

		public void TestHighestDutyIfAdditionalCodesAndNoMatchingRate()
		{
			TemporaryStorageTestHelper.SetUpTariff(Factory, countryCode);

			AssertHighestDutyIfAdditionalCodesAndNoMatchingRate("Good Item", SetUpPackedItemForTest());

			void AssertHighestDutyIfAdditionalCodesAndNoMatchingRate(ZString assertionText, TemporaryStoragePackedItemForTest goodsItem)
			{
				goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;

				goodsItem.API_RN_NKGoodsOrigin = "EU";
				goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
				goodsItem.AdditionalSupplementaryCodes.AddNew("EFGH");
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);
				AssertEquals(string.Format("{0}: In case no rate matches the additional code, default rate is selected.", assertionText), "VFD * 0.12", goodsItem.HighestDuty.ZZ2_RateFormula);
			}
		}

		public void TestHighestDutyIfAdditionalCodesAndMatchingRate()
		{
			TemporaryStorageTestHelper.SetUpTariff(Factory, countryCode);

			AssertHighestDutyIfAdditionalCodesAndMatchingRate("Good Item", packedItem);

			void AssertHighestDutyIfAdditionalCodesAndMatchingRate(ZString assertionText, TemporaryStoragePackedItem goodsItem)
			{
				goodsItem.API_RN_NKGoodsOrigin = "EU";
				goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
				goodsItem.AdditionalSupplementaryCodes.AddNew("ABCD");
				AssertEquals(string.Format("{0}: Rate matching the additional code should always be selected over default rate.", assertionText), "VFD * 0.08", goodsItem.HighestDuty.ZZ2_RateFormula);
			}
		}

		public void TestHighestDutyIfNoAdditionalCode()
		{
			TemporaryStorageTestHelper.SetUpTariff(Factory, countryCode);

			AssertHighestDutyIfNoAdditionalCode("Good Item", packedItem);

			void AssertHighestDutyIfNoAdditionalCode(ZString assertionText, TemporaryStoragePackedItem goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.API_RN_NKGoodsOrigin = "EU";
					goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
					AssertEquals("VFD * 0.12", goodsItem.HighestDuty.ZZ2_RateFormula);

					goodsItem.API_Tariff = "0304798001";
					AssertNull(goodsItem.HighestDuty);
				});
			}
		}

		public void TestHighestDutyWithTwoEmptyAdditionalCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "test country", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();

			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: new string('0', Customs.Business.AutoCusInBondCargoDesc.Schema.BY_DescriptionMaxLength + 1));
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, TemporaryStorageTestHelper.TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(Factory, "A00", rateType1.PK);
			var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preference1.PK);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", preference1.PK);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate2.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(1000));
			Factory.Save();

			AssertHighestDutyWithTwoEmptyAdditionalCode("Good Item", packedItem);

			void AssertHighestDutyWithTwoEmptyAdditionalCode(ZString assertionText, TemporaryStoragePackedItem goodsItem)
			{
				goodsItem.API_RN_NKGoodsOrigin = "EU";
				goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
				goodsItem.API_GoodsValue = 100;

				AssertNoExceptionThrown(assertionText, () =>
				{
					var highestDuty = goodsItem.HighestDuty;
					AssertEquals("The highest duty should return the rate2. ", "VFD * 0.12", highestDuty.ZZ2_RateFormula);
				});
			}
		}

		public void TestHighestDutyWithOneNotEmptyAdditionalCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "test country", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();

			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: new string('0', Customs.Business.AutoCusInBondCargoDesc.Schema.BY_DescriptionMaxLength + 1));
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, TemporaryStorageTestHelper.TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(Factory, "A00", rateType1.PK);
			var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preference1.PK);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", preference1.PK);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "2501");
			helper.CreateCusApplicability(rate2.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(1000));
			Factory.Save();

			AssertHighestDutyWithOneNotEmptyAdditionalCode("Good Item", packedItem);

			void AssertHighestDutyWithOneNotEmptyAdditionalCode(ZString assertionText, TemporaryStoragePackedItem goodsItem)
			{
				goodsItem.API_RN_NKGoodsOrigin = "EU";
				goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
				goodsItem.API_GoodsValue = 100;

				AssertNoExceptionThrown(assertionText, () =>
				{
					var highestDuty = goodsItem.HighestDuty;
					AssertEquals("The highest duty should return the rate2 since it is the one with empty additional code. ", "VFD * 0.12", highestDuty.ZZ2_RateFormula);
				});
			}
		}

		public void TestHighestDutyWithTwoAdditionalCodesButNoSupplementaryCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "test country", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();

			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9999999999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: new string('0', Customs.Business.AutoCusInBondCargoDesc.Schema.BY_DescriptionMaxLength + 1));
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, TemporaryStorageTestHelper.TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(Factory, "A00", rateType1.PK);
			var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preference1.PK);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", preference1.PK);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "2501");
			helper.CreateCusApplicability(rate2.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(1000), "2500");

			var preference2 = helper.CreatePreferenceForCountryAndGrouping("200", "200", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var rate3 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.06", preference2.PK);
			helper.CreateCusApplicability(rate3.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "2505");
			Factory.Save();

			AssertHighestDutyWithTwoAdditionalCodesButNoSupplementaryCodes("Good Item", packedItem);

			void AssertHighestDutyWithTwoAdditionalCodesButNoSupplementaryCodes(ZString assertionText, TemporaryStoragePackedItem goodsItem)
			{
				goodsItem.API_RN_NKGoodsOrigin = "EU";
				goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
				goodsItem.API_GoodsValue = 100;

				AssertNoExceptionThrown(assertionText, () =>
				{
					var highestDuty = goodsItem.HighestDuty;
					AssertEquals("The highest duty should return the rate1 since it is the one with the highest additional code for preference 100. ", "VFD * 0.08", highestDuty.ZZ2_RateFormula);
				});
			}
		}

		public void TestHighestDutyWithIncompleteHarmonisedTariff()
		{
			TemporaryStorageTestHelper.SetUpTariff(Factory, countryCode);

			AssertDutiableAmount("Good Item", packedItem);

			void AssertDutiableAmount(ZString assertionText, TemporaryStoragePackedItem goodsItem)
			{
				goodsItem.API_Tariff = "0304";
				goodsItem.API_GoodsValue = 1_000m;
				goodsItem.RefreshDutyAmountsFromTariffRates();
				AssertNull(string.Format("{0}: Highest Duty Amount Calculation with incomplete harmonised tariff", assertionText), goodsItem.HighestDuty);

				goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
				goodsItem.RefreshDutyAmountsFromTariffRates();
				AssertNotNull(string.Format("{0}: Highest Duty Amount Calculation with complete harmonised tariff", assertionText), goodsItem.HighestDuty);
			}
		}

		#endregion

		#region Antidumping

		public void TestAntiDumpingDutyAmountCalculate()
		{
			SetupDataForGetAdditionalCodesForRateType(Customs.Universal.Constants.RateTypes.AntiDumping);
			AssertAntiDumpingDutyAmountWithNoRateMatched("Good Item", SetUpPackedItemForTest());

			void AssertAntiDumpingDutyAmountWithNoRateMatched(ZString assertionText, TemporaryStoragePackedItemForTest goodsItem)
			{
				goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
				goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
				goodsItem.API_RN_NKGoodsOrigin = countryCode;

				var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
				supplementaryCode1.CY_Code = "AC01";
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshADD: true);
				CombineAssertions(assertionText, () =>
				{
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC01" }, goodsItem.ADDAdditionalCodes);
					AssertDutiesAndTaxes("AC01 matched, amount should be 24.3", goodsItem, ChargeType.AntiDumpingDuty, 24.3m);

					supplementaryCode1.CY_Code = "AC02";
					goodsItem.RefreshDutyAmountsFromTariffRates(refreshADD: true);
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC02" }, goodsItem.ADDAdditionalCodes);
					AssertDutiesAndTaxes("AC02 matched, amount should be 4.3", goodsItem, ChargeType.AntiDumpingDuty, 4.3m);

					goodsItem.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					goodsItem.RefreshDutyAmountsFromTariffRates(refreshADD: true);
					AssertEquals("Empty ADDAdditionalCodes collection", 0, goodsItem.ADDAdditionalCodes.Count());
					AssertDutiesAndTaxes("AntiDumpingDutyAmount should be 0", goodsItem, ChargeType.AntiDumpingDuty);
				});
			}
		}

		public void TestAntiDumpingDutyAmountWithNoRateMatched()
		{
			SetupDataForGetAdditionalCodesForRateTypeWithNoRateMached(Customs.Universal.Constants.RateTypes.AntiDumping);
			AssertAntiDumpingDutyAmountWithNoRateMatched("Good Item", packedItem);

			void AssertAntiDumpingDutyAmountWithNoRateMatched(ZString assertionText, TemporaryStoragePackedItem goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
					goodsItem.API_RN_NKGoodsOrigin = countryCode;

					var supplementaryCode = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supplementaryCode.CY_Code = "AC00";
					goodsItem.RefreshDutyAmountsFromTariffRates();

					AssertContainsExactElementsInAnyOrder(Array.Empty<ZString>(), goodsItem.ADDAdditionalCodes);
					AssertDutiesAndTaxes("No matched ,amount should be 0", goodsItem, ChargeType.AntiDumpingDuty);
				});
			}
		}

		public void TestAntiDumpingWithIncompleteHarmonisedTariff()
		{
			SetupDataForGetAdditionalCodesForRateType(Customs.Universal.Constants.RateTypes.AntiDumping);
			AssertAntiDumpingDutyAmountWithNoRateMatched("Good Item", packedItem);

			void AssertAntiDumpingDutyAmountWithNoRateMatched(ZString assertionText, TemporaryStoragePackedItem goodsItem)
			{
				goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
				goodsItem.API_RN_NKGoodsOrigin = countryCode;

				var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
				supplementaryCode1.CY_Code = "AC02";
				CombineAssertions(assertionText, () =>
				{
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC02" }, goodsItem.ADDAdditionalCodes);

					goodsItem.API_Tariff = "12345";
					AssertEquals("When Harmonised Tariff is incomplete, there should be no AntiDumping Additional Codes", 0, goodsItem.ADDAdditionalCodes.Count());
				});
			}
		}

		#endregion

		#region Countervailing

		public void TestCountervailingDutyAmountCalculate()
		{
			SetupDataForGetAdditionalCodesForRateType(Customs.Universal.Constants.RateTypes.Countervailing);
			AssertCountervailingDutyAmount("Good Item", SetUpPackedItemForTest());

			void AssertCountervailingDutyAmount(ZString assertionText, TemporaryStoragePackedItemForTest goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
					goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
					goodsItem.API_RN_NKGoodsOrigin = countryCode;

					var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supplementaryCode1.CY_Code = "AC01";
					goodsItem.RefreshDutyAmountsFromTariffRates(refreshCVD: true);
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC01" }, goodsItem.CVDAdditionalCodes);
					AssertDutiesAndTaxes("AC01 matched, amount should be 24.3", goodsItem, ChargeType.CountervailingDuty, 24.3m);

					supplementaryCode1.CY_Code = "AC02";
					goodsItem.RefreshDutyAmountsFromTariffRates(refreshCVD: true);
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC02" }, goodsItem.CVDAdditionalCodes);
					AssertDutiesAndTaxes("AC02 matched, amount should be 4.3", goodsItem, ChargeType.CountervailingDuty, 4.3m);

					goodsItem.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					goodsItem.RefreshDutyAmountsFromTariffRates(refreshCVD: true);
					AssertEquals("Empty CVDAdditionalCodes collection", 0, goodsItem.CVDAdditionalCodes.Count());
					AssertDutiesAndTaxes("CountervailingDutyAmount should be 0", goodsItem, ChargeType.CountervailingDuty);
				});
			}
		}

		public void TestCountervailingDutyAmountWithNoRateMatched()
		{
			SetupDataForGetAdditionalCodesForRateTypeWithNoRateMached(Customs.Universal.Constants.RateTypes.Countervailing);
			AssertCountervailingDutyAmountWithNoRateMatched("Good Item", packedItem);

			void AssertCountervailingDutyAmountWithNoRateMatched(ZString assertionText, TemporaryStoragePackedItem goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
					goodsItem.API_RN_NKGoodsOrigin = countryCode;

					var supplementaryCode = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supplementaryCode.CY_Code = "AC00";
					goodsItem.RefreshDutyAmountsFromTariffRates();

					AssertContainsExactElementsInAnyOrder(Array.Empty<ZString>(), goodsItem.CVDAdditionalCodes);
					AssertDutiesAndTaxes("No matched ,amount should be 0", goodsItem, ChargeType.CountervailingDuty);
				});
			}
		}

		public void TestCountervailingWithIncompleteHarmonisedTariff()
		{
			SetupDataForGetAdditionalCodesForRateType(Customs.Universal.Constants.RateTypes.Countervailing);
			AssertCountervailingDutyAmount("Good Item", packedItem);

			void AssertCountervailingDutyAmount(ZString assertionText, TemporaryStoragePackedItem goodsItem)
			{
				CombineAssertions(assertionText, () =>
				{
					goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
					goodsItem.API_RN_NKGoodsOrigin = countryCode;

					var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
					supplementaryCode1.CY_Code = "AC02";
					AssertContainsExactElementsInAnyOrder(new ZString[] { "AC02" }, goodsItem.CVDAdditionalCodes);

					goodsItem.API_Tariff = "12345";
					AssertEquals("When Harmonised Tariff is incomplete, there should be no Countervailing additional codes", 0, goodsItem.CVDAdditionalCodes.Count());
				});
			}
		}

		#endregion

		#region Liability

		public void TestLiabilityAmountCalculate()
		{
			_ = SetupLiabilityRates();

			var goodsItem = SetUpPackedItemForTest();

			CombineAssertions(() =>
			{
				goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
				goodsItem.API_GoodsValue = 1_000m;
				goodsItem.API_RX_NKGoodsValueCurrency = "EUR";
				goodsItem.API_RN_NKGoodsOrigin = countryCode;
				goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshDuty: true);
				AssertEquals("[PRE-CONDITION] DutyAmount", 420m, goodsItem.DutiesAndTaxes.Cast<TemporaryStorageDutyAndTax>().Where(x => x.AET_ChargeType == ChargeType.Duty).Sum(x => x.AET_ChargeAmount));

				var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
				supplementaryCode1.CY_Code = "AC01";
				goodsItem.RefreshDutyAmountsFromTariffRates(refreshADD: true, refreshCVD: true);
				AssertEquals("[PRE-CONDITION] AntiDumpingDutyAmount", 60_000m, goodsItem.DutiesAndTaxes.Cast<TemporaryStorageDutyAndTax>().Where(x => x.AET_ChargeType == ChargeType.CountervailingDuty).Sum(x => x.AET_ChargeAmount));
				AssertEquals("[PRE-CONDITION] CountervailingDutyAmount", 9001m, goodsItem.DutiesAndTaxes.Cast<TemporaryStorageDutyAndTax>().Where(x => x.AET_ChargeType == ChargeType.AntiDumpingDuty).Sum(x => x.AET_ChargeAmount));

				goodsItem.API_TaxAmount = 10;

				AssertEquals("LiabilityAmount", 69_431m, goodsItem.LiabilityAmount);
			});
		}

		public void TestLiabilityAmountIsCalculatedAndSetInGuarantee()
		{
			TemporaryStorageTestHelper.SetUpTariff(Factory, Core.Constants.CountryCodes.Latvia);

			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.AMA_GB = branch.PK;
			var bill = header.Bills.AddNew();
			var guarantee = header.Guarantee;
			guarantee.PW_BondNumber = "GUA1";
			guarantee.PW_Override = false;

			var goodsItem = Factory.New<TemporaryStoragePackedItemForTest>();
			bill.PackedItems.Add(goodsItem);
			goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
			goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
			goodsItem.API_GoodsValue = 1_000m;
			goodsItem.API_RX_NKGoodsValueCurrency = "EUR";
			goodsItem.API_RN_NKGoodsOrigin = "EU";

			CombineAssertions(() =>
			{
				AssertEquals("With all data set, LiabilityAmount", 120m, goodsItem.LiabilityAmount);
				AssertEquals("With all data set, PW_BondAmount", 120m, guarantee.PW_BondAmount);

				goodsItem.API_GoodsValue = 500.5m;
				AssertEquals("When changing API_GoodsValue, LiabilityAmount", 60.06m, goodsItem.LiabilityAmount);
				AssertEquals("When changing API_GoodsValue, PW_BondAmount", 60.06m, guarantee.PW_BondAmount);

				goodsItem.API_Tariff = "8001100000";
				AssertEquals("When changing API_Tariff, LiabilityAmount", 20.02m, goodsItem.LiabilityAmount);
				AssertEquals("When changing API_Tariff, PW_BondAmount", 20.02m, guarantee.PW_BondAmount);

				goodsItem.API_RN_NKGoodsOrigin = "CN";
				AssertEquals("When changing API_RN_NKGoodsOrigin, LiabilityAmount", 0m, goodsItem.LiabilityAmount);
				AssertEquals("When changing API_RN_NKGoodsOrigin, PW_BondAmount", 0m, guarantee.PW_BondAmount);
			});
		}

		#endregion

		public void TestDutiesAndTaxes()
		{
			AssertType<TemporaryStorageDutyAndTax>(packedItem.DutiesAndTaxes.AddNew());
			AssertType<TemporaryStorageDutyAndTax>(packedItem.AsycudaTaxes.AddNew());
		}

		public void TestGetValueSetStrategy()
		{
			var packedItem = SetUpPackedItemForTest();
			AssertType<TemporaryStoragePackedItemValueSetStrategy>(packedItem.GetValueSetStrategy_Exposed());
		}

		void AssertDutiesAndTaxes(string assertionMessage, TemporaryStoragePackedItem packedItem, string chargeType, params ZDecimal[] expectedChargeAmounts)
		{
			var actualChargeAmounts = packedItem.DutiesAndTaxes.Where(x => x.AET_ChargeType == chargeType).Select(x => x.AET_ChargeAmount);
			AssertContainsExactElementsInAnyOrder(assertionMessage, expectedChargeAmounts, actualChargeAmounts);
		}

		(RateView dutyRate, RateView antiDumpingRate, RateView countervailingRate) SetupLiabilityRates()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			_ = helper.CreateRefCusTaxOrFeeType("VAT");
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");

			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", euGrouping);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.TariffTypes.Import, nomenclatureGroupType: "LV", ensureDataGroupingExists: false);
			Factory.Save();

			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Latvia, tariffType.PK, TemporaryStorageTestHelper.TestTariffCode, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "EU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, ensureDataGroupingExists: false);
			_ = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Latvia);
			var rateType1 = helper.CreateCusRateType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.RateTypes.Duty, ensureDataGroupingExists: false);
			var rateType2 = helper.CreateCusRateType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.RateTypes.AntiDumping, ensureDataGroupingExists: false);
			var rateType3 = helper.CreateCusRateType(Core.Constants.CountryCodes.Latvia, Customs.Universal.Constants.RateTypes.Countervailing, ensureDataGroupingExists: false);
			var rateCode1 = helper.CreateCusRateCode(Factory, "RC1", rateType1.PK);
			var rateCode2 = helper.CreateCusRateCode(Factory, "RC2", rateType2.PK);
			var rateCode3 = helper.CreateCusRateCode(Factory, "RC3", rateType3.PK);

			var testRate1 = helper.CreateRate(cusTariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "  420.0 * [FLAT]");
			var testRate2 = helper.CreateRate(cusTariff, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, " 9001.0 * [FLAT]");
			var testRate3 = helper.CreateRate(cusTariff, rateCode3.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "60000.0 * [FLAT]");
			_ = helper.CreateCusApplicability(testRate1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateCusApplicability(testRate2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "AC01");
			_ = helper.CreateCusApplicability(testRate3, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "AC01");
			Factory.Save();

			return (testRate1, testRate2, testRate3);
		}

		void SetupDataForGetAdditionalCodesForRateType(string rateTypeCode)
		{
			(var helper, var testRate1, var testRate2, var tradeGroup) = SetupRatesForGetAdditionalCodes(rateTypeCode);

			helper.CreateCusApplicability(testRate1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC01");
			helper.CreateCusApplicability(testRate2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC02");

			TemporaryStorageTestHelper.SetupCusCodeForGetAdditionalCodes(helper, Factory);
		}

		(UniversalReferenceTestDataHelper, RateView, RateView, CusRefTradeGroupView) SetupRatesForGetAdditionalCodes(string rateTypeCode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateRefCusTaxOrFeeType("VAT");
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", euGrouping);
			var s1p1TariffType = helper.CreateNewOrGetExistingTariffType(countryCode, Customs.Universal.Constants.TariffTypes.Import, nomenclatureGroupType: "LV");
			Factory.Save();

			var cusTariff = helper.LoadOrCreateNewTariff(countryCode, s1p1TariffType.PK, TemporaryStorageTestHelper.TestTariffCode, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");

			var tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.CountryCodes.Turkey, "EU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);
			var rateType = helper.CreateCusRateType(countryCode, rateTypeCode, ensureDataGroupingExists: false);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "RC1", rateType.PK);
			var testRate1 = helper.CreateRate(cusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "24.3 * [FLAT]");
			var testRate2 = helper.CreateRate(cusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "4.3 * [FLAT]");

			return (helper, testRate1, testRate2, tradeGroup);
		}

		void SetupDataForGetAdditionalCodesForRateTypeWithNoRateMached(string rateTypeCode)
		{
			(var helper, var testRate1, var testRate2, var tradeGroup) = SetupRatesForGetAdditionalCodes(rateTypeCode);

			helper.CreateCusApplicability(testRate1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC02");
			helper.CreateCusApplicability(testRate2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC04");

			TemporaryStorageTestHelper.SetupCusCodeForGetAdditionalCodes(helper, Factory);
		}

		ZGuid SetupRateFormula(UniversalReferenceTestDataHelper helper, TariffView tariff, CusRefTradeGroupView tradeGroup, string rateTypeCode, string rateFormula, string countryCode)
		{
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, rateTypeCode);
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "A00", rateType.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var rate = helper.CreateRefCusRate(tariff.PK, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula, preference.PK);
			helper.CreateCusApplicability(rate.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			return rate.PK;
		}

		protected override void SetUp()
		{
			base.SetUp();

			company = Factory.New<GlbCompany>();
			branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.AMA_GB = branch.PK;
			bill = header.Bills.AddNew();
			packedItem = bill.PackedItems.AddNew();
		}

		GlbCompany company;
		GlbBranch branch;
		TemporaryStorageHeader header;
		TemporaryStorageBill bill;
		TemporaryStoragePackedItem packedItem;

		readonly string countryCode = Core.Constants.CountryCodes.Latvia;

		TemporaryStoragePackedItemForTest SetUpPackedItemForTest()
		{
			var item = Factory.New<TemporaryStoragePackedItemForTest>();
			bill.PackedItems.Add(item);
			return item;
		}
	}
}
