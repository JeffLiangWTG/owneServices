using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Customs.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Registry.Business.RatingFeatureHelper.CarrierConnect;
using static Enterprise.Registry.Business.RatingFeatureHelper.Urs;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RatingDataRegistry))]
	sealed class RatingDataRegistryTest : RegistryItemSetTestCaseWithFactory<RatingDataRegistry>
	{
		public void TestAllowSameUNLOCOInRatingInternationalZones()
		{
			TestRegistryItem(
				ItemSet.AllowSameUNLOCOInRatingInternationalZones,
				"AllowSameUNLOCOInRatingInternationalZones",
				"AutoRating",
				"Allow the same UNLOCO for multiple Rating International Zones",
				@"Allow the same UNLOCO to be added to multiple International Zones with Type of 'RAT', 'IMP' or 'EXP'.
Note: Setting this registry to Yes may result in overlapping Rates or Charges to be loaded to the relevant Job when Autorating.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				expectedDefaultValue: false);
		}

		public void TestEnableQuotationDocumentsChargeGroupingSequencingAndRollup()
		{
			TestRegistryItem
				(
					ItemSet.EnableQuotationDocumentsChargeGroupingSequencingAndRollup,
					"EnableQuotationDocumentsChargeGroupingSequencingAndRollup",
					"AutoRating/Quotations",
					"Enable Quotation Documents Charge Grouping, Sequencing and Rollup Enhancement",
					"Quotation Documents printing enhancements for Charge Grouping, Sequencing and Rollup will be enabled.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false
				);
		}
		#region Gateway Prepaid\Collect Priorities

		public void TestGatewayPrepaidPrioritiesRegistry_WHENUseIntercompanyTariffsToAutorateGatewayBillingIsFalse()
		{
			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertGatewayPriorities
				(
					registryItem: ItemSet.GatewayPrepaidPriorities,
					expectedName: "GatewayPrepaidPriorities",
					expectedTermAndDirection: "Gateway Prepaid",
					expectedOptions: RegistryOptions.Default
				);
			}
		}

		public void TestGatewayPrepaidPrioritiesRegistry_WHENUseIntercompanyTariffsToAutorateGatewayBillingIsTrue()
		{
			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertGatewayPriorities
				(
					registryItem: ItemSet.GatewayPrepaidPriorities,
					expectedName: "GatewayPrepaidPriorities",
					expectedTermAndDirection: "Gateway Prepaid",
					expectedOptions: RegistryOptions.IsHidden
				);
			}
		}

		public void TestGatewayCollectPrioritiesRegistry_WHENUseIntercompanyTariffsToAutorateGatewayBillingIsFalse()
		{
			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertGatewayPriorities
				(
					registryItem: ItemSet.GatewayCollectPriorities,
					expectedName: "GatewayCollectPriorities",
					expectedTermAndDirection: "Gateway Collect",
					expectedOptions: RegistryOptions.Default
				);
			}
		}

		public void TestGatewayCollectPrioritiesRegistry_WHENUseIntercompanyTariffsToAutorateGatewayBillingIsTrue()
		{
			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertGatewayPriorities
				(
					registryItem: ItemSet.GatewayCollectPriorities,
					expectedName: "GatewayCollectPriorities",
					expectedTermAndDirection: "Gateway Collect",
					expectedOptions: RegistryOptions.IsHidden
				);
			}
		}

		void AssertGatewayPriorities(RatesPrioritiesRegistryItem registryItem, string expectedName, string expectedTermAndDirection, RegistryOptions expectedOptions)
		{
			TestRatesPrioritiesRegistryItem
			(
				item: registryItem,
				expectedName: expectedName,
				expectedCategory: RatingDataRegistry.Categories.AutoRating_ChargeCodeGroups_SellRatesPriorities,
				expectedCaption: expectedTermAndDirection,
				expectedHint: $"This registry item allows you to set priorities for {expectedTermAndDirection} sell rates. Autorating will prioritize rates which are higher in the list in a case of charge code conflicts. Removing particular Organization Type from the list will make its rates not applicable.",
				expectedStorageFlags: RegistryStorageFlags.System | RegistryStorageFlags.Company,
				expectedOptions: expectedOptions
			);
		}

		void TestRatesPrioritiesRegistryItem(RatesPrioritiesRegistryItem item, string expectedName, string expectedCategory, string expectedCaption, string expectedHint, RegistryStorageFlags expectedStorageFlags, RegistryOptions expectedOptions)
		{
			TestGenericRegistryItem(item, expectedName, expectedCategory, expectedCaption, expectedHint, expectedStorageFlags, expectedOptions);
		}

		#endregion

		public void TestCostComparisonLinesNumber()
		{
			TestRegistryItem(ItemSet.CostComparisonLinesNumber,
				"CostComparisonLinesNumber",
				"AutoRating",
				"Number of lines displayed in Cost Comparison",
				"Specifies the number of records that will be displayed in Cost Comparison View. A maximum of 10000 records is allowed, however it is advised to keep the default value of 1000 and only change the number of lines to display when needed.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				1000,
				100,
				10000);
		}

		public void TestValidateOnlyLoadedRatesUponSaving()
		{
			TestRegistryItem(ItemSet.ValidateOnlyLoadedRatesUponSaving,
				"ValidateOnlyLoadedRatesUponSaving",
				"AutoRating",
				"Only displayed rates on grid (UI) are validated upon saving",
				@"Setting this registry to Yes:
When a Tariffs & Rates record is saved, only the rates displayed on the grid are validated. This improves performance but may allow invalid rates to exist.

Setting this registry to No:
When a Tariffs & Rates record is saved, all rates under the same header are validated whether they are loaded in UI or not, and any errors must be corrected. This ensures all invalid rates are corrected but may impact performance.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestEnableAutoRatingLogNoteDebugLog()
		{
			TestRegistryItem(ItemSet.EnableAutoRatingLogNoteForDebug,
				"EnableAutoRatingLogNoteDebugLog",
				"AutoRating/Calculation",
				"Enable AutoRating Log Note for Debugging",
				"Specifies whether the Autorating Log note includes additional debug information.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableLongChargeCalculationDescriptions()
		{
			TestGenericRegistryItem(
				ItemSet.EnableLongChargeCalculationDescription,
				"EnableLongChargeCalculationDescription",
				"AutoRating/Calculation",
				"Enable Long Charge Calculation Description",
				"Override this Registry to show the longer detailed formula in the autorated charge description instead of the shorter and simplified version showing only the applied operator of the calculator.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestAutorateByBBK_BLK_ROR_BCNContainerModes()
		{
			TestGenericRegistryItem(
				ItemSet.AutorateByBBK_BLK_ROR_BCNContainerModes,
				"AutorateByBBK_BLK_ROR_BCNContainerModes",
				"AutoRating/Calculation",
				"Autorate by BBK/BLK/ROR/BCN Container Modes",
				"When registry is set to ‘Yes’, Autorating will match with BBK/BLK/ROR/BCN Container Modes.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup()
		{
			TestGenericRegistryItem(
				ItemSet.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup,
				"AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup",
				"AutoRating/Calculation",
				"Autorate Stand-alone Customs Declaration Job with Own Rates Setup",
				"When this registry is set to ‘Yes’, rates setup under Customs tab in Client Rates, Company Tariffs and Costings are used for Autorating Stand-alone Customs Declaration jobs.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestAllowConsolAutoratingDateSynchronizedAcrossCompanies()
		{
			TestGenericRegistryItem(
				ItemSet.AllowConsolAutoratingDateSynchronizedAcrossCompanies,
				"AllowConsolAutoratingDateSynchronizedAcrossCompanies",
				"AutoRating/Calculation",
				"Allow Consol Autorating Date Synchronized Across All Login Companies",
				"When registry is set to ‘Yes’, Consol > Details > Rates> Autorating Date is synchronized across all login companies.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestEnableWarehouseUnitFactorRatesDevelopment()
		{
			TestGenericRegistryItem(
				ItemSet.EnableWarehouseUnitFactorRatesDevelopment,
				"EnableWarehouseUnitFactorRatesDevelopment",
				"AutoRating/Calculation",
				"Enable Packs Weight and Product Line for Warehouse Orders and Receives",
				"When registry is set to ‘Yes’, product warehouse rate-line unit-factor will have 'Packs Weight' and 'Product Line' option for autorating calculation.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#region Rates Service

		public void TestUniversalRatesServiceUrl()
		{
			TestGenericRegistryItem(
				ItemSet.UniversalRatesServiceUrl,
				"UniversalRatesServiceUrl",
				"AutoRating/Universal Rates Service",
				"URS URL",
				"Support Only Registry. A URL to the Universal Rates web service.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"https://urs.wisegrid.net");
		}

		public void TestRatesServiceUrl()
		{
			TestGenericRegistryItem(
				ItemSet.RatesServiceUrl,
				"WiseRatesServiceUrl",
				"AutoRating/Rates Service",
				"Rates Service URL",
				"Support Only Registry. A URL to the Rates Service web service.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"https://rates.wisegrid.net");
		}

		public void TestWTGAuthServiceUrl()
		{
			TestGenericRegistryItem(
				ItemSet.WTGAuthServiceUrl,
				nameof(RatingDataRegistry.WTGAuthServiceUrl),
				"AutoRating/Rates Service",
				"WTG Authentication Service URL",
				"Support Only Registry. An URL to the WTG Authentication Service.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				"https://auth.wisegrid.net");
		}

		public void TestEnterpriseCodeOverride()
		{
			TestGenericRegistryItem(
				ItemSet.EnterpriseCodeOverride,
				"EnterpriseCodeOverride",
				"AutoRating/Rates Service",
				"Enterprise Code",
				"Overriding this Registry allows to modify the Enterprise Code value sent in the authentication token when sending requests to Rates Service / URS.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForCargoWise);
		}

		#endregion

		#region Charge Codes

		public void TestShippingDefaultCodes()
		{
			TestGenericRegistryItem(
				ItemSet.ShippingContainerisedDefaultCodes,
				"ShippingContainerisedDefaultCodes",
				"AutoRating/Charge Codes/Freight",
				"Shipping Containerized Freight Default Charge Codes",
				"Shipping Containerized Freight Default Charge Codes",
				RegistryStorageFlags.Company,
				Env.Registry.FreightChargeCode.ToString());

			TestGenericRegistryItem(
				ItemSet.ShippingNonContainerisedtDefaultCodes,
				"ShippingNonContainerisedtDefaultCodes",
				"AutoRating/Charge Codes/Freight",
				"Shipping Non-Containerized Freight Default Charge Codes",
				"Shipping Non-Containerized Freight Default Charge Codes",
				RegistryStorageFlags.Company,
				Env.Registry.FreightChargeCode.ToString());

			TestGenericRegistryItem(
				ItemSet.ShippingOriginDefaultChargeCodes,
				"ShippingOriginDefaultChargeCodes",
				"AutoRating/Charge Codes/Origin",
				"Shipping Origin Default Charge Codes",
				"Shipping Origin Default Charge Codes",
				RegistryStorageFlags.Company);

			AssertEquals("No default origin charge codes", 0, ItemSet.ShippingOriginDefaultChargeCodes.Value.Count);

			TestGenericRegistryItem(
				ItemSet.ShippingDestinationDefaultChargeCodes,
				"ShippingDestinationDefaultChargeCodes",
				"AutoRating/Charge Codes/Destination",
				"Shipping Destination Default Charge Codes",
				"Shipping Destination Default Charge Codes",
				RegistryStorageFlags.Company);

			AssertEquals("No default destination charge codes", 0, ItemSet.ShippingDestinationDefaultChargeCodes.Value.Count);
		}

		public void TestDefaultAIRFreightWeightBreaks()
		{
			decimal[] defaultValue = ItemSet.AIRFreightWeightBreaks.DefaultValue;
			AssertEquals("DefaultValue.Length", 5, defaultValue.Length);
			AssertEquals("DefaultValue[0]", 45m, defaultValue[0]);
			AssertEquals("DefaultValue[1]", 100m, defaultValue[1]);
			AssertEquals("DefaultValue[2]", 250m, defaultValue[2]);
			AssertEquals("DefaultValue[3]", 500m, defaultValue[3]);
			AssertEquals("DefaultValue[4]", 1000m, defaultValue[4]);
			AssertEquals("DataType.LowerBound", 0m, ItemSet.AIRFreightWeightBreaks.DataType.LowerBound);
			AssertEquals("DataType.DecimalPlaces", 1, ItemSet.AIRFreightWeightBreaks.DataType.DecimalPlaces);
		}

		public void TestFreightDefaultCodes()
		{
			Guid[] codesAIR = ItemSet.AIRFreightDefaultCodes.GetAsGuidArray();
			Guid[] codesFCL = ItemSet.FCLFreightDefaultCodes.GetAsGuidArray();
			Guid[] codesLCL = ItemSet.LCLFreightDefaultCodes.GetAsGuidArray();

			AssertEquals("AIR", Env.Registry.FreightChargeCode, codesAIR[0]);
			AssertEquals("FCL", Env.Registry.FreightChargeCode, codesFCL[0]);
			AssertEquals("LCL", Env.Registry.FreightChargeCode, codesLCL[0]);

			Guid new1 = Guid.NewGuid();
			Guid new2 = Guid.NewGuid();
			ItemSet.AIRFreightDefaultCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new1 + "," + new2);
			codesAIR = ItemSet.AIRFreightDefaultCodes.GetAsGuidArray();
			AssertEquals("New AIR[0]", new1, codesAIR[0]);
			AssertEquals("New AIR[1]", new2, codesAIR[1]);
		}

		public void TestDefaultChargeCodes()
		{
			AssertEquals("AIRFreightDefaultCodes.DefaultChargeCode", "FRT", ItemSet.AIRFreightDefaultCodes.DefaultChargeCode);
			AssertEquals("FCLFreightDefaultCodes.DefaultChargeCode", "FRT", ItemSet.FCLFreightDefaultCodes.DefaultChargeCode);
			AssertEquals("LCLFreightDefaultCodes.DefaultChargeCode", "FRT", ItemSet.LCLFreightDefaultCodes.DefaultChargeCode);
		}

		public void TestCustomsDisbursementChargeCodeDoesNotAllowEmpty()
		{
			AssertExceptionThrown<RegistryValidationException>("No empty allowed", "Please select a valid selection.", () => ItemSet.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestCustomDeferredChargeCode()
		{
			AssertEquals("DefaultValue", Guid.Empty, ItemSet.CustomDeferredChargeCode.DefaultValue);

			Guid newValue = Guid.NewGuid();
			ItemSet.CustomDeferredChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Value", newValue, ItemSet.CustomDeferredChargeCode.Value);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.CustomDeferredChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
		}

		public void TestCustomsQuarantineChargeCode()
		{
			var registry = ItemSet.CustomsQuarantineChargeCode;
			AssertEquals("DefaultValue", Guid.Empty, registry.DefaultValue.ChargeCode);
			AssertContainsExactElementsInAnyOrder(new Guid[] { Core.Constants.CountryGuids.Australia }, registry.CountryFilterPKs);
			AssertType<GuidFindBoxRegistryEditorInfo>(registry.EditorInfo);

			GlbCompanyTestHelper.TemporarilySetCountry(Core.Constants.CountryCodes.Australia);
			Guid newValue = Guid.NewGuid();
			ItemSet.CustomsQuarantineChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ChargeCodeWithDate() { ChargeCode = newValue });
			AssertEquals("Value", newValue, ItemSet.CustomsQuarantineChargeCode.Value.ChargeCode);
		}

		public void TestRatingDataRegistry()
		{
			AssertChargeTypesAndCodes(RatingDataRegistry.Instance.EntryChargeTypesAndCodes, Core.Constants.CountryCodes.Switzerland, AccChargeCode.PK, "A00", "Switzerland", Core.Constants.CountryGuids.Switzerland, 1);
			AssertChargeTypesAndCodes(RatingDataRegistry.Instance.EntryChargeTypesAndCodes, Core.Constants.CountryCodes.Ireland, AccChargeCode.PK, "A00", "Ireland", Core.Constants.CountryGuids.Ireland, 1);
			AssertChargeTypesAndCodes(RatingDataRegistry.Instance.EntryChargeTypesAndCodes, Core.Constants.CountryCodes.Netherlands, AccChargeCode.PK, "A00", "Netherlands", Core.Constants.CountryGuids.Netherlands, 1);
			AssertChargeTypesAndCodes(RatingDataRegistry.Instance.EntryChargeTypesAndCodes, Core.Constants.CountryCodes.Belgium, AccChargeCode.PK, "A00", "Belgium", Core.Constants.CountryGuids.Belgium, 1);
			AssertChargeTypesAndCodes(RatingDataRegistry.Instance.EntryChargeTypesAndCodes, Core.Constants.CountryCodes.Germany, AccChargeCode.PK, "A00", "Germany", Core.Constants.CountryGuids.Germany, 1);
			AssertChargeTypesAndCodes(RatingDataRegistry.Instance.EntryChargeTypesAndCodes, Core.Constants.CountryCodes.Sweden, AccChargeCode.PK, "A00", "Sweden", Core.Constants.CountryGuids.Sweden, 1);
			AssertChargeTypesAndCodes(RatingDataRegistry.Instance.EntryChargeTypesAndCodes, Core.Constants.CountryCodes.Italy, AccChargeCode.PK, "A00", "Italy", Core.Constants.CountryGuids.Italy, 1);
		}

		void AssertChargeTypesAndCodes(EntryChargeTypeSettingCollectionRegistryItem registryItem, string countryCode, ZGuid chargePK, string chargeType, string countryName, Guid countryGuid, int count)
		{
			GlbCompanyTestHelper.TemporarilySetCountry(countryCode);
			var collection = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

			var charge = collection.AddNew();
			charge.AC_ChargeCode = chargePK;
			charge.ChargeType = chargeType;
			registryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			collection = registryItem.Value;
			AssertEquals(count, collection.Count);
			AssertEquals(charge.ChargeType, collection[0].ChargeType);
		}

		#region AccChargeCode
		BusinessObject AccChargeCode
		{
			get
			{
				if (fAccChargeCode == null)
				{
					System.Reflection.Assembly masterFilesAssembly = System.Reflection.Assembly.Load("Enterprise.MasterFiles.Business");
					Type accChargeCodeType = masterFilesAssembly.GetType("Enterprise.MasterFiles.Business.AccChargeCode");
					fAccChargeCode = Factory.New(accChargeCodeType);
					fAccChargeCode.FillWithValidTestData();
					Factory.Save();
				}
				return fAccChargeCode;
			}
		}
		BusinessObject fAccChargeCode;
		#endregion

		public void TestEntryChargeTypeRegistry()
		{
			var item = ItemSet.EntryChargeTypesAndCodes;
			AssertEquals("EntryChargeTypesAndCodes", item.Name);
			AssertEquals("AutoRating/Charge Codes/Customs", item.Category);
			AssertEquals("Disbursement Charge Code Override", item.Caption);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Disbursement Charge Code Override", item.Caption);
		}

		public void TestEntryReferenceTypeRegistry()
		{
			TestRegistryItem(ItemSet.PopulateEntryRefAsInvoiceNo,
				"PopulateEntryRefAsInvoiceNo",
				RatingDataRegistry.Categories.AutoRating_ChargeCodes_Customs,
				"Populate Entry Ref as Invoice No.",
				@"If enabled, the Entry Reference will be populated as Invoice No. in the selected Format.
Full Reference populates the complete Entry Reference Number, shortened Reference cuts off the first three digits of the Entry Reference Number.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new RateEntryReferenceTypeList(),
				"");
		}

		public void TestEntryReferenceTypeList()
		{
			var entryReferenceTypeList = new RateEntryReferenceTypeList();
			AssertEquals(2, entryReferenceTypeList.Count);
			Assert(entryReferenceTypeList.ContainsCode("FUL"));
			Assert(entryReferenceTypeList.ContainsCode("SHT"));
		}

		#region Services Charge Codes

		public void TestDestinationDemurrageServiceChargeCode()
		{
			var item = ItemSet.DestinationDemurrageServiceChargeCode;
			AssertEquals("DestinationDemurrageServiceChargeCode", item.Name);
			AssertEquals("AutoRating/Charge Codes/Destination", item.Category);
			AssertEquals("Destination Truck Wait Time", item.Caption);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.DestinationDemurrageServiceChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertChargeCodeExists(item.DefaultChargeCode, "DST");
		}

		public void TestDestinationMergedDemurrageDetentionChargeCode()
		{
			var item = ItemSet.DestinationMergedDemurrageDetentionChargeCode;
			AssertEquals("DestinationMergedDemurrageDetentionChargeCode", item.Name);
			AssertEquals("AutoRating/Charge Codes/Destination", item.Category);
			AssertEquals("Destination Merged Demurrage & Detention", item.Caption);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.DestinationMergedDemurrageDetentionChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(string.Empty, item.DefaultChargeCode);
		}

		public void TestDestinationDetentionServiceChargeCode()
		{
			var item = ItemSet.DestinationDetentionServiceChargeCode;
			AssertEquals("DestinationDetentionServiceChargeCode", item.Name);
			AssertEquals("AutoRating/Charge Codes/Destination", item.Category);
			AssertEquals("Destination Detention", item.Caption);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.DestinationDetentionServiceChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertChargeCodeExists(item.DefaultChargeCode, "DST");
		}

		public void TestDestinationLaborServiceChargeCode()
		{
			var item = ItemSet.DestinationLaborServiceChargeCode;
			AssertEquals("DestinationLaborServiceChargeCode", item.Name);
			AssertEquals("AutoRating/Charge Codes/Destination", item.Category);
			AssertEquals("Destination Labor Service", item.Caption);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.DestinationLaborServiceChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertChargeCodeExists(item.DefaultChargeCode, "DST", "LBR");
		}

		public void TestDestinationStorageServiceChargeCode()
		{
			var item = ItemSet.DestinationStorageServiceChargeCode;
			AssertEquals("DestinationStorageServiceChargeCode", item.Name);
			AssertEquals("AutoRating/Charge Codes/Destination", item.Category);
			AssertEquals("Destination CTO Storage", item.Caption);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.DestinationStorageServiceChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertChargeCodeExists(item.DefaultChargeCode, "DST", "STG");
		}

		public void TestDestinationCarrierStorageChargeCode()
		{
			var item = ItemSet.DestinationCarrierStorageChargeCode;
			AssertEquals("DestinationCarrierStorageChargeCode", item.Name);
			AssertEquals("AutoRating/Charge Codes/Destination", item.Category);
			AssertEquals("Destination Carrier Storage/Demurrage", item.Caption);
			AssertEquals("PrimaryKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.DestinationStorageServiceChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(string.Empty, item.DefaultChargeCode);
		}

		public void TestOriginDetentionChargeCode()
		{
			var item = ItemSet.OriginDetentionChargeCode;
			AssertEquals("OriginDetentionChargeCode", item.Name);
			AssertEquals("AutoRating/Charge Codes/Origin", item.Category);
			AssertEquals("Origin Detention", item.Caption);
			AssertEquals("PrimaryKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.OriginDemurrageServiceChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals("", item.DefaultChargeCode);
		}

		public void TestOriginStorageChargeCode()
		{
			var item = ItemSet.OriginStorageChargeCode;
			AssertEquals("OriginStorageChargeCode", item.Name);
			AssertEquals("AutoRating/Charge Codes/Origin", item.Category);
			AssertEquals("Origin CTO Storage", item.Caption);
			AssertEquals("PrimaryKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.OriginDemurrageServiceChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals("", item.DefaultChargeCode);
		}

		public void TestOriginCarrierStorageChargeCode()
		{
			var item = ItemSet.OriginCarrierStorageChargeCode;
			AssertEquals("OriginCarrierStorageChargeCode", item.Name);
			AssertEquals("AutoRating/Charge Codes/Origin", item.Category);
			AssertEquals("Origin Carrier Storage/Demurrage", item.Caption);
			AssertEquals("PrimaryKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.DestinationStorageServiceChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(string.Empty, item.DefaultChargeCode);
		}

		public void TestOriginDemurrageServiceChargeCode()
		{
			var item = ItemSet.OriginDemurrageServiceChargeCode;
			AssertEquals("OriginDemurrageServiceChargeCode", item.Name);
			AssertEquals("AutoRating/Charge Codes/Origin", item.Category);
			AssertEquals("Origin Truck Wait Time", item.Caption);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.OriginDemurrageServiceChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertChargeCodeExists(item.DefaultChargeCode, "ORG");
		}

		public void TestOriginMergedDemurrageDetentionChargeCode()
		{
			var item = ItemSet.OriginMergedDemurrageDetentionChargeCode;
			AssertEquals("OriginMergedDemurrageDetentionChargeCode", item.Name);
			AssertEquals("AutoRating/Charge Codes/Origin", item.Category);
			AssertEquals("Origin Merged Demurrage & Detention", item.Caption);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.OriginMergedDemurrageDetentionChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(string.Empty, item.DefaultChargeCode);
		}

		public void TestOriginLaborServiceChargeCode()
		{
			var item = ItemSet.OriginLaborServiceChargeCode;
			AssertEquals("OriginLaborServiceChargeCode", item.Name);
			AssertEquals("AutoRating/Charge Codes/Origin", item.Category);
			AssertEquals("Origin Labor Service", item.Caption);
			AssertEquals("PrimariKeyFromCodeRequired", ((GuidFindBoxRegistryEditorInfo)ItemSet.OriginLaborServiceChargeCode.EditorInfo).IsPrimaryKeyFromCodeRequired, true);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertChargeCodeExists(item.DefaultChargeCode, "ORG", "LBR");
		}

		void AssertChargeCodeExists(string defaultChargeCode, string expectedGroup, string expectedSubgroup = "")
		{
			var chargeCodes = Factory.Load<IAccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, defaultChargeCode));

			var chargeCode = chargeCodes[0] as BusinessObject;
			AssertNotNull("Expected charge code to exist", chargeCode);
			AssertEquals("Charge code should be active", true, chargeCode[AccChargeCodeSchema.AC_IsActive]);
			AssertEquals(expectedGroup, chargeCode[AccChargeCodeSchema.AC_ChargeGroup]);

			if (!string.IsNullOrEmpty(expectedSubgroup))
			{
				AssertEquals(expectedSubgroup, chargeCode[AccChargeCodeSchema.AC_ChargeSubGroup]);
			}
		}

		#endregion

		#endregion

		public void TestIncludeCustomDeferredChargeInInvoicing()
		{
			AssertEquals("DefaultValue", false, ItemSet.IncludeCustomDeferredChargeInInvoicing.DefaultValue);

			ItemSet.IncludeCustomDeferredChargeInInvoicing.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.IncludeCustomDeferredChargeInInvoicing.Value);
		}

		public void TestNZCodesAreValid()
		{
			GlbCompanyTestHelper.TemporarilySetCountry("NZ");

			var chargeTypes = ItemSet.EntryChargeTypesAndCodes.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).AddNew().ChargeType_List;
			bool hasWrongCharges = false;
			foreach (var charge in chargeTypes)
			{
				if (((EntryChargeType)charge).IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing)
				{
					hasWrongCharges = true;
					break;
				}
			}
			AssertEquals("Has Bad Charge Codes", false, hasWrongCharges);
		}

		public void TestShowAutoRatingNotRunWarning()
		{
			AssertEquals("DefaultValue", true, ItemSet.ShouldShowAutoRatingNotRunWarning.DefaultValue);
			ItemSet.ShouldShowAutoRatingNotRunWarning.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Value", false, ItemSet.ShouldShowAutoRatingNotRunWarning.Value);
		}

		public void TestEnableMultiModalRatingCost()
		{
			AssertEquals("DefaultValue", false, ItemSet.MultiModalRatingCost.DefaultValue);
			ItemSet.MultiModalRatingCost.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.MultiModalRatingCost.Value);
		}

		public void TestEnableMultiModalRatingCostShipment()
		{
			AssertEquals("DefaultValue", false, ItemSet.MultiModalRatingCostShipment.DefaultValue);
			ItemSet.MultiModalRatingCostShipment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.MultiModalRatingCostShipment.Value);
		}

		public void TestHideValueBreakdownInDescription()
		{
			AssertEquals("DefaultValue", false, ItemSet.HideValueBreakdownInDescription.DefaultValue);

			ItemSet.HideValueBreakdownInDescription.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value", true, ItemSet.HideValueBreakdownInDescription.Value);
		}

		public void TestShowCostingsDuringRating()
		{
			TestRegistryItem(ItemSet.ShowCostingsDuringRating, "ShowCostingsDuringRating", "AutoRating/Calculation",
		"Show Related Costings",
		"Specifies whether costings are to be shown when Quotations, Client Rates and Company Tariffs are being created. If turned off, the user can still choose to view them by ticking the Costings option.",
		RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, true);
		}

		public void TestExcludeInwardsFromPeriodicAutoRating()
		{
			TestRegistryItem(ItemSet.ExcludeInwardsFromPeriodicAutoRating, "ExcludeInwardsFromPeriodicAutoRating", "AutoRating/Calculation/Warehouse",
		"Exclude Warehouse Receipts From Periodic Auto-Rating",
		"Specifies the default value for the 'Exclude From Periodic Auto-Rating' checkbox on the Warehouse Receive Billing tab.",
		RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, false);
		}

		public void TestExcludeOrdersFromPeriodicAutoRating()
		{
			TestRegistryItem(ItemSet.ExcludeOrdersFromPeriodicAutoRating, "ExcludeOrdersFromPeriodicAutoRating", "AutoRating/Calculation/Warehouse",
				"Exclude Warehouse Orders From Periodic Auto-Rating",
				"Specifies the default value for the 'Exclude From Periodic Auto-Rating' checkbox on the Warehouse Orders Billing tab.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, false);
		}

		public void TestClientRateGoingToExpireNotification()
		{
			TestGenericRegistryItem(
				ItemSet.ClientRateGoingToExpireNotification,
				"ClientRateGoingToExpireNotification",
				"AutoRating/Email Notification",
				"Client Rate Going To Expire Email",
				"If this option is on, the customer service and sales representatives will receive email notifications if a client rate is going to expire when auto-rating a job for this client.",
				RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
				false);
		}

		public void TestClientRateJustExpiredNotification()
		{
			TestGenericRegistryItem(
				ItemSet.ClientRateJustExpiredNotification,
				"ClientRateJustExpiredNotification",
				"AutoRating/Email Notification",
				"Client Rate Just Expired Email",
				"If this option is on, the customer service and sales representatives will receive email notifications if a client rate is expired when auto-rating a job for this client.",
				RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
				true);
		}

		public void TestClientRateNotFoundNotification()
		{
			TestGenericRegistryItem(
				ItemSet.ClientRateNotFoundNotification,
				"ClientRateNotFoundNotification",
				"AutoRating/Email Notification",
				"Client Rate Not Found Email",
				"If this option is on, the customer service and sales representatives will receive email notifications if a client rate is not found when auto-rating a job for this client.",
				RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
				true);
		}

		public void TestOneOffQuoteUsedNotification()
		{
			TestGenericRegistryItem(
				ItemSet.OneOffQuoteUsedNotification,
				"OneOffQuoteUsedNotification",
				"AutoRating/Email Notification",
				"One Off Quote Used Email",
				"If this option is on, the customer service and sales representatives will receive email notifications if one off quote is used.",
				RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
				true);
		}

		public void TestIncludeSalesRepresentative()
		{
			TestGenericRegistryItem(
				ItemSet.IncludeSalesRepresentative,
				"IncludeSalesRepresentative",
				"AutoRating/Email Notification",
				"Include Sales Representative as Recipient",
				"Determines whether rating email notifications should be sent to the sales representative.",
				RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
				true);
		}

		public void TestIncludeCustomerService()
		{
			TestGenericRegistryItem(
				ItemSet.IncludeCustomerService,
				"IncludeCustomerService",
				"AutoRating/Email Notification",
				"Include Customer Service as Recipient",
				"Determines whether rating email notifications should be sent to the customer service.",
				RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
				true);
		}

		public void TestDefaultSalesRepresentative()
		{
			TestGenericRegistryItem(
				ItemSet.DefaultSalesRepresentative,
				"DefaultSalesRepresentative",
				"AutoRating/Email Notification",
				"Default Sales Representative",
				"The default Sales Representative to use on all Sales Organizations. Will be used for email notifications if no sales representative is found on the sales organization.",
				RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
				RegistryOptions.IsValueMandatory,
				Guid.Empty);
		}

		public void TestDefaultCustomerService()
		{
			TestGenericRegistryItem(
				ItemSet.DefaultCustomerService,
				"DefaultCustomerService",
				"AutoRating/Email Notification",
				"Default Customer Service",
				"The default Customer Service Representative to use on all Sales Organizations. Will be used for email notifications if no customer service representative is found on the sales organization.",
				RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
				RegistryOptions.IsValueMandatory,
				Guid.Empty);
		}

		public void TestCompanyTariffGoingToExpireNotification()
		{
			TestRegistryItem(ItemSet.CompanyTariffGoingToExpireNotification,
				"CompanyTariffGoingToExpireNotification",
				"AutoRating/Email Notification",
				"Company Tariff Going To Expire Email",
				"If this option is on, the company tariff notification group will receive email notification if a company tariff is going to expire.",
				RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
				RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestCompanyTariffJustExpiredNotification()
		{
			TestRegistryItem(ItemSet.CompanyTariffJustExpiredNotification,
				"CompanyTariffJustExpiredNotification",
				"AutoRating/Email Notification",
				"Company Tariff Just Expired Email",
				"If this option is on, the company tariff notification group will receive email notification if a company tariff is expired.",
				RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
				RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestCompanyTariffNotificationEmailGroup()
		{
			TestGenericRegistryItem(ItemSet.CompanyTariffNotificationEmailGroup,
				"CompanyTariffNotificationEmailGroup",
				"AutoRating/Email Notification",
				"Company Tariff Expiring / Expired Notification Group",
				"This group will receive an email during Auto-rating whenever a company tariff rate is expiring / has expired based on the Company Tariff expiry settings.",
				RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
				RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue,
				Guid.Empty);
		}

		#region Required Fields

		public void TestQuotationsRequiredFields()
		{
			TestRequiredFieldsRegistryItem(ItemSet.QuotationsRequiredFields, "QuotationsRequiredFields", "Quotations", AutoRatingRequiredFields.RatingHeaderTypes.Quotations, true);
		}

		public void TestClientRatesRequiredFields()
		{
			TestRequiredFieldsRegistryItem(ItemSet.ClientRatesRequiredFields, "ClientRatesRequiredFields", "Client Rates", AutoRatingRequiredFields.RatingHeaderTypes.ClientRates, false);
		}

		public void TestCostsRequiredFields()
		{
			TestRequiredFieldsRegistryItem(ItemSet.CostsRequiredFields, "CostsRequiredFields", "Costs", AutoRatingRequiredFields.RatingHeaderTypes.Costs, false);
		}

		public void TestCompanyTariffsRequiredFields()
		{
			TestRequiredFieldsRegistryItem(ItemSet.CompanyTariffsRequiredFields, "CompanyTariffsRequiredFields", "Company Tariffs", AutoRatingRequiredFields.RatingHeaderTypes.CompanyTariffs, false);
		}

		void TestRequiredFieldsRegistryItem(AutoRatingRequiredFieldsRegistryItem item, string expectedName, string expectedCaption, string expectedRatingHeaderType, bool expectedShowIncoterm)
		{
			TestGenericRegistryItem(item, expectedName, "AutoRating/Required Fields", expectedCaption, string.Format("Required fields for {0}.", expectedCaption), RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue);
			AssertEquals("RatingHeaderType", expectedRatingHeaderType, item.RatingHeaderType);
			AssertEquals("EditorInfo.ShowIncoterm", expectedShowIncoterm, ((AutoRatingRequiredFieldsRegistryEditorInfo)item.EditorInfo).ShowIncoterm);

			AutoRatingRequiredFields fields = new AutoRatingRequiredFields();
			fields.RequireServiceLevel = true;
			item.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, fields);
			AssertEquals("ValueWithHeader.RequireServiceLevel", true, item.TypedValue.RequireServiceLevel);
		}

		#endregion

		public void TestIncoTermDefinition()
		{
			TestGenericRegistryItem(ItemSet.IncoTermDefinition,
				"IncoTermDefinition",
				"AutoRating/Charge Code Groups",
				"Incoterm Charge Code Group Configuration",
				"This registry item allows you to configure the behavior of Incoterms in respect to the different freight related charge code groups. Defaults based on the standard definition of each Incoterm are provided, but can be changed by you to change certain parts of an Incoterm.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue);
		}

		#region Free-Text Note Calculator

		public void TestDefaultShowOnBillingWithoutPrefix()
		{
			TestGenericRegistryItem(ItemSet.UseShowOnBillingWithoutPrefixDefault,
				"UseShowOnBillingWithoutPrefixDefault",
				"AutoRating/Calculation/Free-Text Note Calculator",
				"Show On Billing Without Prefix Default",
				"Specifies the default value for the tick-box 'Show On Billing Without Prefix' on the Free-Text Note calculator.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				false);
		}

		#endregion

		#region Disbursement Interest Calculator

		public void TestCurrentPrimeRate()
		{
			var item = ItemSet.CurrentPrimeRate;
			AssertEquals("CurrentPrimeRate", item.Name);
			AssertEquals("AutoRating/Calculation/Disbursement Interest Calculator", item.Category);
			AssertEquals("Current Prime Rate for Interest on Disbursement Rating", item.Caption);
			AssertEquals("The rate set here will be used by the autorating module when calculating interest on disbursement charges.\r\nThis interest rate is usually set by a relevant government authority in your country/region.", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(0m, item.DefaultValue);
			AssertEquals(4, ((NumericRegistryEditorInfo)item.EditorInfo).DecimalPlaces);
		}

		#endregion

		public void TestQuoteCancellationReasonCodes()
		{
			var item = ItemSet.QuoteCancellationReasonCodes;
			AssertEquals("QuoteCancellationReasonCodes", item.Name);
			AssertEquals("AutoRating/Quotations", item.Category);
			AssertEquals("Cancellation Reason Codes", item.Caption);
			AssertEquals("Specifies reason codes for quotation cancellation.", item.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals(1, item.DefaultValue.Count);
			AssertEquals("UDF", item.DefaultValue[0].Code);
		}

		public void TestQuoteFollowUpDays()
		{
			var item = ItemSet.QuoteFollowUpDays;
			AssertEquals("QuoteFollowUpDays", item.Name);
			AssertEquals("AutoRating/Validity and Notification Periods", item.Category);
			AssertEquals("Quotation Follow Up Days", item.Caption);
			AssertEquals("Specifies the number of days to set a follow up date for when creating a new quotation.", item.Hint);
			AssertEquals(RegistryStorageFlags.Company, item.Storage);
			AssertEquals(7, item.DefaultValue);
			AssertEquals((double)0, ((IntRegistryDataType)item.Inner.DataType).LowerBound);
			AssertEquals((double)365, ((IntRegistryDataType)item.Inner.DataType).UpperBound);
		}

		public void TestIsQuoteCancellationReasonCodeRequired()
		{
			var item = ItemSet.IsQuoteCancellationReasonCodeRequired;
			AssertEquals("IsQuoteCancellationReasonCodeRequired", item.Name);
			AssertEquals("AutoRating/Quotations", item.Category);
			AssertEquals("Cancellation Reason Code Is Required", item.Caption);
			AssertEquals("If this is on, the cancellation reason will be mandatory when canceling a quotation.", item.Hint);
			AssertEquals(RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals(false, item.DefaultValue);
		}

		public void TestEnableOverseasAgentInOneOffQuote()
		{
			var item = ItemSet.EnableOverseasAgentInOneOffQuote;
			AssertEquals("EnableOverseasAgentInOneOffQuote", item.Name);
			AssertEquals("AutoRating/Quotations", item.Category);
			AssertEquals("Enable Overseas Agent Type in One Off Quote", item.Caption);
			AssertEquals("If this is on, the Overseas Agent Option would be enabled in One off Quote.", item.Hint);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(false, item.DefaultValue);
		}

		public void TestGlobalSellRatesOverrideLocal()
		{
			TestGenericRegistryItem(
				ItemSet.GlobalSellRatesOverrideLocal,
				nameof(RatingDataRegistry.GlobalSellRatesOverrideLocal),
				"AutoRating/Global Rates",
				"Global Sell Rates override Local",
				"During the Autorating process, Global Sell Rates override Local Sell Rates",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				false);
		}

		#region UseIntercompanyTariffsToAutorateGatewayBilling

		public void TestUseIntercompanyTariffsToAutorateGatewayBilling() =>
			TestGenericRegistryItem(
				ItemSet.UseIntercompanyTariffsToAutorateGatewayBilling,
				nameof(RatingDataRegistry.UseIntercompanyTariffsToAutorateGatewayBilling),
				"AutoRating/Gateway Billing",
				"Use Intercompany Tariffs to Autorate Gateway Billing",
				@"Set this registry to ‘Yes’ switches the system to use Intercompany Tariffs only to Autorate Gateway Billing.
Once this registry is set to ‘Yes’, it cannot be reverted.",
				RegistryStorageFlags.System,
				false);

		public void TestUseIntercompanyTariffsToAutorateGatewayBilling_DefaultValue()
		{
			AssertEquals(false, RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.DefaultValue);
		}

		public void TestUseIntercompanyTariffsToAutorateGatewayBilling_EditableAsDefault()
		{
			AssertEquals(
				RegistryOptions.Default,
				RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Options & RegistryOptions.IsReadOnly);
		}

		public void TestUseIntercompanyTariffsToAutorateGatewayBilling_EditableWhenNo()
		{
			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(
				Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(
					RegistryOptions.Default,
					RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Options & RegistryOptions.IsReadOnly);
			}
		}

		#endregion

		#region Rating Web Services
		public void TestSupportJsonMediaType()
		{
			TestGenericRegistryItem(
				ItemSet.SupportJsonMediaType,
				nameof(RatingDataRegistry.SupportJsonMediaType),
				"AutoRating/Rating Web Services",
				"Enable JSON media type",
				"Specifies whether JSON Media Type should be supported by server or not.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestShowExceptionDetailsInResponse()
		{
			TestGenericRegistryItem(
				ItemSet.ShowExceptionDetailsInResponse,
				nameof(RatingDataRegistry.ShowExceptionDetailsInResponse),
				"AutoRating/Rating Web Services",
				"Show Exception Details in Response",
				"Specifies whether we should return the details of an unhandled exception in response or not.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestTokenExpiryDurationInSeconds()
		{
			TestGenericRegistryItem(
				ItemSet.TokenExpiryDurationInSeconds,
				nameof(RatingDataRegistry.TokenExpiryDurationInSeconds),
				"AutoRating/Rating Web Services",
				"Token Expiry Duration In Seconds",
				"Token Expiry Duration In Seconds.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				30);
		}

		public void TestRatingTokenAuthentication()
		{
			TestGenericRegistryItem(
				ItemSet.RatingTokenAuthentication,
				nameof(RatingDataRegistry.RatingTokenAuthentication),
				"AutoRating/Rating Web Services",
				"Token For Rating Authentication",
				"Token For Rating Authentication.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue);
			var defaultValue = ItemSet.RatingTokenAuthentication.DefaultValue;
			AssertEquals(typeof(RatingTokenAuthenticationCollection), defaultValue.GetType());
			AssertEquals(0, defaultValue.Count);
		}
		#endregion

		#region AutoratingViaPort

		public void TestAutoratingViaPort()
		{
			TestGenericRegistryItem(
				ItemSet.AutoratingViaPort,
				"AutoratingViaPort",
				"AutoRating/Calculation",
				"Autorating Via Port",
				"This registry item allows you to define 'Via' ports for Autorating Cost/Revenue for Booking, Shipment and Consol.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company);
		}

		public void TestAutoratingViaPort_DefaultValue()
		{
			var collection = ItemSet.AutoratingViaPort.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			(string jobType, string mode, string direction, string origin, string destination, string via)[] defaults =
			{
				("QSH", "ALL", "ALL",  "VL", "NVD", "VD"),
				("QSH", "ALL", "ALL", "NVL",  "VD", "VL"),
				("QSH", "ALL", "EXP", "NVL", "NVD", "VL"),
				("QSH", "ALL", "IMP", "NVL", "NVD", "VD"),

				("SHP", "ALL", "ALL",  "1L", "NLD", "LD"),
				("SHP", "ALL", "ALL", "N1L",  "LD", "1L"),
				("SHP", "ALL", "EXP", "N1L", "NLD", "1L"),
				("SHP", "ALL", "IMP", "N1L", "NLD", "LD"),

				("FCN", "ALL", "ALL",  "VL", "NVD", "VD"),
				("FCN", "ALL", "ALL", "NVL",  "VD", "VL"),
				("FCN", "ALL", "EXP", "NVL", "NVD", "VL"),
				("FCN", "ALL", "IMP", "NVL", "NVD", "VD"),
			};

			var index = 0;
			foreach (AutoratingViaPortConfiguration config in collection)
			{
				AssertEquals(defaults[index].jobType, config.JobType);
				AssertEquals(defaults[index].mode, config.TransportMode);

				foreach (AutoratingViaPortSetting setting in config.Settings)
				{
					AssertEquals(defaults[index].direction, setting.Direction);
					AssertEquals(defaults[index].origin, setting.OriginSourceOption);
					AssertEquals(defaults[index].destination, setting.DestinationSourceOption);
					AssertEquals(defaults[index].via, setting.ViaSourceOption);

					index++;

					if (index >= defaults.Length || defaults[index].jobType != config.JobType || defaults[index].mode != config.TransportMode)
					{
						break;
					}
				}
			}
		}

		#endregion

		#region HBLDeliveryPriority

		public void TestHBLDeliveryPriority()
		{
			TestGenericRegistryItem(
				ItemSet.HBLDeliveryPriority,
				"HBLDeliveryPriority",
				"AutoRating/Calculation",
				"HBL Delivery Mode Matching Values with Priorities",
				"HBL Delivery Mode on Forwarding Shipment can be matched with one or more HBL Delivery Modes when Autorating with priority for the same Charge Code found.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);
		}

		#endregion

		#region CargoWise CarrierConnect For Job Autorating

		public void TestCargoWiseCarrierConnectForJobAutorating()
		{
			TestGenericRegistryItem(
				ItemSet.CargoWiseCarrierConnectForJobAutorating,
				"CargoWiseCarrierConnectForJobAutorating",
				"AutoRating/Rate Selector",
				"Enable CargoWise CarrierConnect for Job Autorating",
				"Enables the usage of CargoWise CarrierConnect which replaces the existing Rate Selectors.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden,
				true);
		}

		public void TestCargoWiseCarrierConnectForJobAutorating_RegistryOptions_WhenEnabledInFeatureControl()
		{
			var cWCarrierConnectFeatureRule = new CWCarrierConnectFeatureRule { Enabled = true };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals(RegistryOptions.Default, RatingDataRegistry.Instance.CargoWiseCarrierConnectForJobAutorating.Options);
		}

		public void TestCargoWiseCarrierConnectForJobAutorating_RegistryOptions_WhenDisabledInFeatureControl()
		{
			var cWCarrierConnectFeatureRule = new CWCarrierConnectFeatureRule { Enabled = false };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals(RegistryOptions.IsHidden, RatingDataRegistry.Instance.CargoWiseCarrierConnectForJobAutorating.Options);
		}

		#endregion

		#region Enable URS Integration

		public void TestEnableUrsIntegration()
		{
			TestGenericRegistryItem(
				ItemSet.EnableUrsIntegration,
				"EnableUrsIntegration",
				"AutoRating/Universal Rates Service",
				"Enable URS Integration",
				@"Support Only Registry. By enabling this registry, rates external to CargoWise will only be retrieved through integration with Universal Rates Service but NOT through Rates Service. This is applied to both Autorating and Multimodal Rates searching.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		#endregion

		#region Use URS For Legacy Rate Selector and MMS

		public void TestUseUrsForLegacyRateSelectorAndMMS()
		{
			TestGenericRegistryItem(
				ItemSet.UseUrsForLegacyRateSelectorAndMMS,
				"UseUrsForLegacyRateSelectorAndMMS",
				"AutoRating/Universal Rates Service",
				"Use URS For Legacy Rate Selectors/MMS",
				@"Enables the usage of URS for the Legacy Rate Selectors and Multimodal Rates.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden,
				false);
		}

		public void TestUseUrsForLegacyRateSelectorAndMMS_RegistryOptions_WhenEnabledInFeatureControl()
		{
			var ursRule = new UrsFeatureRule { Enabled = true, Url = "https://fcm.cargowise.com" };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out ursRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.UniversalRatesService, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals(RegistryOptions.IsOnlyForSupport, RatingDataRegistry.Instance.UseUrsForLegacyRateSelectorAndMMS.Options);
		}

		public void TestUseUrsForLegacyRateSelectorAndMMS_RegistryOptions_WhenDisabledInFeatureControl()
		{
			var ursRule = new UrsFeatureRule { Enabled = false, Url = "https://fcm.cargowise.com" };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out ursRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.UniversalRatesService, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));
			ObjectFactory.Substitute(featureControlMock.Object);

			AssertEquals(RegistryOptions.IsHidden, RatingDataRegistry.Instance.UseUrsForLegacyRateSelectorAndMMS.Options);
		}

		#endregion

		#region Implementation

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				var list = new List<string>(base.ConditionallyVisibleRegistryItems);
				list.Add("AutorateByBBK_BLK_ROR_BCNContainerModes");
				list.Add("SpotRatingBehavioursAreEnabled");
				list.Add("AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup");
				list.Add("ShowExceptionDetailsInResponse");
				list.Add("CargoWiseCarrierConnectForJobAutorating");
				list.Add("UseUrsForLegacyRateSelectorAndMMS");
				return list;
			}
		}

		#endregion
	}
}
