using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class RatingDataRegistry : RegistryItemSet
	{
		#region Construction

		RatingDataRegistry()
		{
		}

		public static RatingDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new RatingDataRegistry();
				}
				return fInstance;
			}
		}
		[ThreadStatic]
		static RatingDataRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString AutoRating_Calculation_Unspecifiedratebehavior { get { return CombineCategories(AutoRating_Calculation, ResString.GetMultilingualString("7fe0eba2-fba6-413f-abaa-06a885a2e5fb", "Unspecified rate behavior")); } }
			public static MultilingualString AutoRating_Calculation_SlidingCalculator { get { return CombineCategories(AutoRating_Calculation, ResString.GetMultilingualString("0fd8765f-641d-47ab-9a99-108c45997df7", "Sliding Calculator")); } }
			public static MultilingualString AutoRating_Calculation_FreeTextNoteCalculator { get { return CombineCategories(AutoRating_Calculation, ResString.GetMultilingualString("6c02e8d6-2a8e-4dd7-a4d2-cfdf706285eb", "Free-Text Note Calculator")); } }
			public static MultilingualString AutoRating_Calculation_DisbursementInterestCalculator { get { return CombineCategories(AutoRating_Calculation, ResString.GetMultilingualString("202cab79-7416-44d3-b559-93ed0ab02c67", "Disbursement Interest Calculator")); } }
			public static MultilingualString AutoRating_Calculation_Rounding { get { return CombineCategories(AutoRating_Calculation, ResString.GetMultilingualString("8f4a4f36-fb62-4fcd-820f-20910744b74a", "Rounding")); } }
			public static MultilingualString AutoRating_Calculation_Warehouse { get { return CombineCategories(AutoRating_Calculation, ResString.GetMultilingualString("3c73bb4e-e82e-4f46-96f8-94c25239baab", "Warehouse")); } }
			public static MultilingualString AutoRating_RequiredFields { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("4c4a2dfd-43a0-42ab-9c44-62818a486207", "Required Fields")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs { get { return CombineCategories(AutoRating_ChargeCodes, ResString.GetMultilingualString("64bb1a2a-63d7-4fc3-8071-4c8fa33fec1e", "Customs")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_UnitedArabEmirates { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("ad87a8e5-add2-4ba4-9bf2-103c36a50feb", "United Arab Emirates")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_Australia { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("e4f70c1a-bf53-45e3-9096-c40a8d8f4b54", "Australia")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_NewZealand { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("60d9cb2f-3dd5-42e7-91d6-c1b0a258d749", "New Zealand")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_Singapore { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("2ea454c2-f92d-4921-9eb7-ccd032e8daae", "Singapore")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_UnitedStates { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("d1135f0b-4716-4eb6-85a0-f01fcf162f95", "United States")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_SouthAfrica { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("19ce874c-9211-4d2a-bbb2-66043e783053", "South Africa")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_Switzerland { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("A59A68C6-4965-4AEB-9B57-D3C48D7625DA", "Switzerland")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_Ireland { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("86743B3E-844E-4E4E-9072-EA074CBCD2BA", "Ireland")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_Netherlands { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("500C1EDD-3412-4989-BD5C-7302B43DBADB", "Netherlands")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_Belgium { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("9661660A-847C-4A7A-80EF-AB4C9DF821B8", "Belgium")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_Sweden { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("64B8E172-C902-46F2-AE3E-2251FD603797", "Sweden")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_Italy { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("D5EA8B28-AD4D-4CDD-8551-0CC082539AAE", "Italy")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_Germany { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("86b7a661-b831-4ce1-af10-9dc915f585a2", "Germany")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_France { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("A841DADA-EE4A-4506-B42D-D35D9B61B627", "France")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_Brazil { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("35CD73D5-6B86-4D18-9CE6-4B43B1A97C09", "Brazil")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_Taiwan { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("3949898A-97B0-4D30-ADC5-B826E7A5FEA0", "Taiwan")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_UnitedKingdom { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("70B1AC67-8664-4987-89CF-61DD6484A827", "United Kingdom")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_China { get { return CombineCategories(AutoRating_ChargeCodes_Customs, ResString.GetMultilingualString("9de0750b-4310-4efe-bfe0-f74663cde5da", "China")); } }
			public static MultilingualString AutoRating_ChargeCodes_Origin { get { return CombineCategories(AutoRating_ChargeCodes, ResString.GetMultilingualString("ec786c35-881a-4aea-af88-593aed0f5eca", "Origin")); } }
			public static MultilingualString AutoRating_ChargeCodes_Destination { get { return CombineCategories(AutoRating_ChargeCodes, ResString.GetMultilingualString("a3f7b118-9c99-4787-ad08-e5ec85659da3", "Destination")); } }
			public static MultilingualString AutoRating_ChargeCodeGroups_SellRatesPriorities { get { return CombineCategories(AutoRating_ChargeCodeGroups, ResString.GetMultilingualString("4efe3fad-62a8-401b-843d-3ce47903068d", "Sell Rates Priorities")); } }
			public static MultilingualString AutoRating_Quotations { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("418099fe-5a3e-4c92-ad82-7f82ded65e52", "Quotations")); } }
			public static MultilingualString AutoRating_RateCommodity { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("6dcdd4e8-125f-4624-93a6-843545c63256", "Rate Commodity")); } }
			public static MultilingualString AutoRating_RatesService { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("a5cafba4-71ea-4d07-9994-fcb0d02755a9", "Rates Service")); } }
			public static MultilingualString AutoRating_UniversalRatesService { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("df4d4362-e2a2-4916-ab68-9217b1a25497", "Universal Rates Service")); } }
			public static MultilingualString AutoRating_Quotations_SpotQuotes { get { return CombineCategories(AutoRating_Quotations, ResString.GetMultilingualString("20511b71-4d8f-4696-9f25-6abcbf5d3b86", "Spot Quotes")); } }
			public static MultilingualString AutoRating_GatewayBilling { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("a304f0a2-d602-4984-9a90-d5f209fbbfa2", "Gateway Billing")); } }
			public static MultilingualString AutoRating_MultimodalRating { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("f4abab9c-fb92-4bbc-a02a-f372a7aa53dc", "Multi-Route Auto-Costing")); } }
			public static MultilingualString AutoRating_RatesService_Providers { get { return CombineCategories(AutoRating_RatesService, ResString.GetMultilingualString("9a6ee7f0-6463-40fb-b73f-e562f540a697", "Third Party Rate Providers")); } }
			public static MultilingualString AutoRating_RatesService_Providers_CargoSphere { get { return CombineCategories(AutoRating_RatesService_Providers, ResString.GetMultilingualString("166b6269-8219-424c-977f-a50773cd1c9e", "CargoSphere")); } }
			public static MultilingualString AutoRating_RatesService_Providers_Cargoguide { get { return CombineCategories(AutoRating_RatesService_Providers, ResString.GetMultilingualString("6d563df6-a500-11e8-81d3-1c1b0d09faa1", "Cargoguide")); } }
			public static MultilingualString AutoRating_RatesService_DiagnosticSettings { get { return CombineCategories(AutoRating_RatesService, ResString.GetMultilingualString("D3DE1FAF-F760-4D8A-ACB8-043C27D1E188", "Diagnostic Settings")); } }
			public static MultilingualString AutoRating_GlobalRates { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("0ae19c78-f812-43db-9074-44ccdefea44b", "Global Rates")); } }
			public static MultilingualString AutoRating_RateSelector { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("d1f9c8ae-a06e-4fff-b50c-815242fde5e0", "Rate Selector")); } }
			public static MultilingualString AutoRating_RatingWebService { get { return CombineCategories(AutoRating, ResString.GetMultilingualString("00916238-B1CC-4864-A551-3674E3E39040", "Rating Web Services")); } }
		}

		#endregion

		#region Current Prime Rate

		public DecimalRegistryItem CurrentPrimeRate
		{
			get
			{
				return GetItem("CurrentPrimeRate", delegate
				{
					var result = new DecimalRegistryItem("CurrentPrimeRate",
						Categories.AutoRating_Calculation_DisbursementInterestCalculator,
						ResString.GetMultilingualString("caec4acf-9c41-4225-ae87-351d7c2b0cb1", "Current Prime Rate for Interest on Disbursement Rating"),
						ResString.GetMultilingualString("91048146-f5d8-43e0-8064-2a0b131ab468", "The rate set here will be used by the autorating module when calculating interest on disbursement charges.\r\nThis interest rate is usually set by a relevant government authority in your country/region."),
						RegistryStorageFlags.Company,
						0M);

					result.EditorInfo = new NumericRegistryEditorInfo(4);
					return result;
				});
			}
		}

		#endregion

		#region Number of lines displayed in Cost Comparison

		public IntRegistryItem CostComparisonLinesNumber
		{
			get
			{
				return GetItem("CostComparisonLinesNumber",
					() => new IntRegistryItem("CostComparisonLinesNumber",
						Categories.AutoRating,
						ResString.GetMultilingualString("93c5de4a-ca80-4099-984e-e4b5b02257e4", "Number of lines displayed in Cost Comparison"),
						ResString.GetMultilingualString("8a3f8007-76ec-4f79-a848-d02f59badd5e", "Specifies the number of records that will be displayed in Cost Comparison View. A maximum of 10000 records is allowed, however it is advised to keep the default value of 1000 and only change the number of lines to display when needed."),
						null,
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						1000,
						100,
						10000));
			}
		}

		#endregion

		#region Show Invalid Rates

		public BooleanRegistryItem ValidateOnlyLoadedRatesUponSaving
		{
			get
			{
				return GetItem("ValidateOnlyLoadedRatesUponSaving",
					() => new BooleanRegistryItem("ValidateOnlyLoadedRatesUponSaving",
						Categories.AutoRating,
						ResString.GetMultilingualString("bf897afc-0035-47d4-851e-c88210b4b1bd", "Only displayed rates on grid (UI) are validated upon saving"),
						ResString.GetMultilingualString("e9386343-fbf0-48d1-bca2-185b8b8b08ca", @"Setting this registry to Yes:
When a Tariffs & Rates record is saved, only the rates displayed on the grid are validated. This improves performance but may allow invalid rates to exist.

Setting this registry to No:
When a Tariffs & Rates record is saved, all rates under the same header are validated whether they are loaded in UI or not, and any errors must be corrected. This ensures all invalid rates are corrected but may impact performance."),
						RegistryStorageFlags.System,
						false));
			}
		}

		#endregion

		#region Declared sell and cost

		public BooleanRegistryItem UnspecifiedSellShouldForceZeroToBePulledThrough
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UnspecifiedSellShouldForceZeroToBePulledThrough", delegate
				{
					return new BooleanRegistryItem(
						"UnspecifiedSellShouldForceZeroToBePulledThrough",
						Categories.AutoRating_Calculation_Unspecifiedratebehavior,
						ResString.GetMultilingualString("311c4b22-74a3-46db-81be-ac8a8fd5116f", "SELL - Assume zero if agent declared SELL rate is not found"),
						ResString.GetMultilingualString("1d7bd830-66be-4766-b474-92351c845246", "Will force the AutoRating system to assume a zero value for unspecified agent declared SELL rates, rather than the use of actual rates if no agent declared rates exist."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem UnspecifiedCostShouldForceZeroToBePulledThrough
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UnspecifiedCostShouldForceZeroToBePulledThrough", delegate
				{
					return new BooleanRegistryItem(
						"UnspecifiedCostShouldForceZeroToBePulledThrough",
						Categories.AutoRating_Calculation_Unspecifiedratebehavior,
						ResString.GetMultilingualString("fc0c4cc7-1b49-4049-bbbd-5e069b4d7fc9", "COST - Assume zero if agent declared COST rate is not found"),
						ResString.GetMultilingualString("0759c1da-6f5a-402d-bf04-74f8d0f5fa9e", "Will force the AutoRating system to assume a zero value for unspecified agent declared COST rates, rather than the use of actual rates if no agent declared rates exist."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Required Fields

		public AutoRatingRequiredFieldsRegistryItem QuotationsRequiredFields
		{
			get
			{
				return GetItem<AutoRatingRequiredFieldsRegistryItem>("QuotationsRequiredFields", delegate
				{
					return new AutoRatingRequiredFieldsRegistryItem(
						AutoRatingRequiredFields.RatingHeaderTypes.Quotations,
						"QuotationsRequiredFields",
						Categories.AutoRating_RequiredFields,
						ResString.GetMultilingualString("418099fe-5a3e-4c92-ad82-7f82ded65e52", "Quotations"),
						ResString.GetMultilingualString("67c6bba8-29c8-4b79-8cd9-7baeb7b78661", "Required fields for Quotations."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public AutoRatingRequiredFieldsRegistryItem ClientRatesRequiredFields
		{
			get
			{
				return GetItem<AutoRatingRequiredFieldsRegistryItem>("ClientRatesRequiredFields", delegate
				{
					return new AutoRatingRequiredFieldsRegistryItem(
						AutoRatingRequiredFields.RatingHeaderTypes.ClientRates,
						"ClientRatesRequiredFields",
						Categories.AutoRating_RequiredFields,
						ResString.GetMultilingualString("8f1f98e5-2970-4c3a-938b-962db25b4ff7", "Client Rates"),
						ResString.GetMultilingualString("2a961a4e-5b5e-49e3-8317-0add2a811ee7", "Required fields for Client Rates."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public AutoRatingRequiredFieldsRegistryItem CostsRequiredFields
		{
			get
			{
				return GetItem<AutoRatingRequiredFieldsRegistryItem>("CostsRequiredFields", delegate
				{
					return new AutoRatingRequiredFieldsRegistryItem(
						AutoRatingRequiredFields.RatingHeaderTypes.Costs,
						"CostsRequiredFields",
						Categories.AutoRating_RequiredFields,
						ResString.GetMultilingualString("a4b04cc8-b698-49c2-b7cd-b287e66ee6a6", "Costs"),
						ResString.GetMultilingualString("aa6af161-7b24-4c7b-9332-cd0fe544ef66", "Required fields for Costs."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public AutoRatingRequiredFieldsRegistryItem CompanyTariffsRequiredFields
		{
			get
			{
				return GetItem<AutoRatingRequiredFieldsRegistryItem>("CompanyTariffsRequiredFields", delegate
				{
					return new AutoRatingRequiredFieldsRegistryItem(
						AutoRatingRequiredFields.RatingHeaderTypes.CompanyTariffs,
						"CompanyTariffsRequiredFields",
						Categories.AutoRating_RequiredFields,
						ResString.GetMultilingualString("1f26a8ed-6aa5-418a-a120-84891b40a7fc", "Company Tariffs"),
						ResString.GetMultilingualString("66ca49fa-230d-4c52-8a94-c65d298b69c9", "Required fields for Company Tariffs."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Show Costings

		public BooleanRegistryItem ShowCostingsDuringRating
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ShowCostingsDuringRating", delegate
				{
					return new BooleanRegistryItem(
						"ShowCostingsDuringRating",
						Categories.AutoRating_Calculation,
						ResString.GetMultilingualString("82b79477-99ab-4ad3-8a36-cad4175685c3", "Show Related Costings"),
						ResString.GetMultilingualString("336ef98b-758a-4392-b102-8a1e3af5cc71", "Specifies whether costings are to be shown when Quotations, Client Rates and Company Tariffs are being created. If turned off, the user can still choose to view them by ticking the Costings option."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region Charge Codes

		#region Customs Charge Codes

		public BooleanRegistryItem IncludeEntryHeaderReferenceInCustomsDisbursementCharges
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeEntryHeaderReferenceInCustomsDisbursementCharges", delegate
				{
					return new BooleanRegistryItem(
						"IncludeEntryHeaderReferenceInCustomsDisbursementCharges",
						Categories.AutoRating_ChargeCodes_Customs,
						ResString.GetMultilingualString("9d83b686-83fc-44cd-ae96-df417bf5cecc", "Incl. Entry Ref in Disbursement Charge Description"),
						ResString.GetMultilingualString("3662dcda-bba6-46fd-8cf1-0230039f944e", "If enabled, charges will not be merged if they are for different entry references, and these references will be included in the description of that charge."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public EntryChargeTypeSettingCollectionRegistryItem EntryChargeTypesAndCodes
		{
			get
			{
				return GetItem("EntryChargeTypesAndCodes", () => new EntryChargeTypeSettingCollectionRegistryItem(
					"EntryChargeTypesAndCodes",
					Categories.AutoRating_ChargeCodes_Customs,
					DisbursementChargeCodeOverride,
					SelectChargeCodes,
					RegistryStorageFlags.Company,
					RegistryOptions.PreserveTestValue));
			}
		}

		#region Strings

		MultilingualString DisbursementChargeCodeOverride
		{
			get { return ResString.GetMultilingualString("B8C3DB3D-2536-4D50-9FBD-A3EBF24E3158", "Disbursement Charge Code Override"); }
		}

		MultilingualString SelectChargeCodes
		{
			get { return ResString.GetMultilingualString("0DAD5E95-17A4-4D1D-9E3D-4A6C451E7680", "Select Charge Codes for each Entry Charge Type to be used when AutoRating.\r\n\r\nIf a Charge Code is specified for an Entry Charge Type, it will be used by AutoRating to generate your AR/AP Invoice. Where no Charge Code is specified, the Default Disbursement Charge Code will be used."); }
		}
		#endregion

		public GuidRegistryItem CustomsDisbursementCreditor
		{
			get
			{
				return GetItem<GuidRegistryItem>("CustomsDisbursementCreditor", delegate
				{
					return new GuidRegistryItem(
						"CustomsDisbursementCreditor",
						Categories.AutoRating_ChargeCodes_Customs,
						ResString.GetMultilingualString("4db3c3d5-0a18-415b-9aae-3b89f00818e8", "Disbursement Creditor"),
						ResString.GetMultilingualString("48c9a821-af6f-4ae7-abe6-0dbb08fb9126", "This is the Disbursement Creditor that will be used for auto generating AP Customs Invoices."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						Guid.Empty);
				});
			}
		}

		#region CustomsDisbursementChargeCode
		public ChargeCodeRegistryItem CustomsDisbursementChargeCode
		{
			get
			{
				return GetItem<ChargeCodeRegistryItem>("CustomsDisbursementChargeCode", delegate
				{
					ChargeCodeRegistryItem result = new ChargeCodeRegistryItem(
						"CustomsDisbursementChargeCode",
						Categories.AutoRating_ChargeCodes_Customs,
						ResString.GetMultilingualString("6201cde8-1063-4fb0-988a-91dbc4e3d5bb", "Default Disbursement Charge Code"),
						ResString.GetMultilingualString("a5728df3-d8c0-4e62-8a7b-863b95a8405a", "This is the Disbursement Charge Code that will be used when AutoRating."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory,
						"CUSDSB");

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.DisbursementChargeCode);
					return result;
				});
			}
		}
		#endregion

		public GuidRegistryItem CustomDeferredChargeCode
		{
			get
			{
				return GetItem<GuidRegistryItem>("CustomDeferredChargeCode", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"CustomDeferredChargeCode",
						Categories.AutoRating_ChargeCodes_Customs,
						ResString.GetMultilingualString("e777b86f-bd4a-4a77-9713-152d9a4004fd", "Deferred Charge Code"),
						ResString.GetMultilingualString("9d4016ba-9067-456d-8c77-a9b7b94c3b3f", "This is the Charge Code used for Deferred Customs Charges when AutoRating if 'Include Deferred Charges in Invoicing' is turned on."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.CustomDeferredChargeCode);
					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public CodePairRegistryItem PopulateEntryRefAsInvoiceNo
		{
			get
			{
				return GetItem("PopulateEntryRefAsInvoiceNo", delegate
				{
					var result = new CodePairRegistryItem(
						"PopulateEntryRefAsInvoiceNo",
						Categories.AutoRating_ChargeCodes_Customs,
						ResString.GetMultilingualString("E2B132DA-8B64-4442-A9A8-D737349D1038", "Populate Entry Ref as Invoice No."),
						ResString.GetMultilingualString("67FBEC73-D4B5-43B6-9333-B0190F7CEF75", @"If enabled, the Entry Reference will be populated as Invoice No. in the selected Format.
Full Reference populates the complete Entry Reference Number, shortened Reference cuts off the first three digits of the Entry Reference Number."),
						new CodeDescriptionPairListProvider(() => new RateEntryReferenceTypeList()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						"");

					return result;
				});
			}
		}

		public BooleanRegistryItem IncludeCustomDeferredChargeInInvoicing
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeCustomDeferredChargeInInvoicing", delegate
				{
					return new BooleanRegistryItem(
						"IncludeCustomDeferredChargeInInvoicing",
						Categories.AutoRating_ChargeCodes_Customs,
						ResString.GetMultilingualString("c5144a4f-8cd7-4ce2-a08d-9357766fc030", "Include Deferred Charges in Invoicing"),
						ResString.GetMultilingualString("357b0e84-6601-47c2-86f8-2605559cfd8d", "Include Customs Deferred Charges on the AR Invoice Lines for 'Information Only' when AutoRating."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem HideValueBreakdownInDescription
		{
			get
			{
				return GetItem<BooleanRegistryItem>("HideValueBreakdownInDescription", delegate
				{
					return new BooleanRegistryItem(
						"HideValueBreakdownInDescription",
						Categories.AutoRating_ChargeCodes_Customs,
						ResString.GetMultilingualString("4e97a13e-3a20-4c64-9e5c-6b852bf510c4", "Hide Value Breakdown in Description"),
						ResString.GetMultilingualString("67430157-82ae-48f7-8c83-dcc7d51568f2", "AutoRating will always use the description from the Accounting Charge Code. The total amount will be appended to the description when the charge code in use is the deferred charge code."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public ChargeCodeWithDateRegistryItem CustomsQuarantineChargeCode
		{
			get
			{
				return GetItem("CustomsQuarantineChargeCode", delegate
				{
					var result = new ChargeCodeWithDateRegistryItem(
						"CustomsQuarantineChargeCode",
						Categories.AutoRating_ChargeCodes_Customs_Australia,
						ResString.GetMultilingualString("EB716035-E06F-4D40-A460-56A207F664E5", "Default Quarantine Charge Code"),
						ResString.GetMultilingualString("068F5F43-2146-43DC-8B9C-FD430B94E084", "If a Disbursement charge code (DSB) is specified for the ASP Entry Charge Type, it will be used by Accounting Integration when a successful payment response message is returned from Customs."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.AUCustomsQuarantineChargeCode);
					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					result.CountryFilterPKs = new Guid[] { Core.Constants.CountryGuids.Australia };
					return result;
				});
			}
		}

		#endregion

		#region Freight Charge Codes

		public ChargeCodeListRegistryItem AIRFreightDefaultCodes
		{
			get
			{
				return GetItem<ChargeCodeListRegistryItem>("AIRFreightDefaultCodes", delegate
				{
					return new ChargeCodeListRegistryItem(
						"AIRFreightDefaultCodes",
						Categories.AutoRating_ChargeCodes_Freight,
						ResString.GetMultilingualString("0716eae1-e20e-4b2f-8bce-87f2f348390c", "AIR Freight Default Charge Codes"),
						ResString.GetMultilingualString("0716eae1-e20e-4b2f-8bce-87f2f348390c", "AIR Freight Default Charge Codes"),
						RegistryOptions.PreserveTestValue,
						RegistryConstants.Strings.DefaultFreightChargeCode,
						RegistryFindBoxFilter.FreightChargeCode);
				});
			}
		}

		public ChargeCodeListRegistryItem FCLFreightDefaultCodes
		{
			get
			{
				return GetItem<ChargeCodeListRegistryItem>("FCLFreightDefaultCodes", delegate
				{
					return new ChargeCodeListRegistryItem(
						"FCLFreightDefaultCodes",
						Categories.AutoRating_ChargeCodes_Freight,
						ResString.GetMultilingualString("99170fb9-1bbc-45f0-afc6-841be86e9253", "FCL Freight Default Charge Codes"),
						ResString.GetMultilingualString("99170fb9-1bbc-45f0-afc6-841be86e9253", "FCL Freight Default Charge Codes"),
						RegistryOptions.PreserveTestValue,
						RegistryConstants.Strings.DefaultFreightChargeCode,
						RegistryFindBoxFilter.FreightChargeCode);
				});
			}
		}

		public ChargeCodeListRegistryItem LCLFreightDefaultCodes
		{
			get
			{
				return GetItem<ChargeCodeListRegistryItem>("LCLFreightDefaultCodes", delegate
				{
					return new ChargeCodeListRegistryItem(
						"LCLFreightDefaultCodes",
						Categories.AutoRating_ChargeCodes_Freight,
						ResString.GetMultilingualString("63c42e6c-f1ed-4258-a14d-8cf18950f28e", "LCL Freight Default Charge Codes"),
						ResString.GetMultilingualString("63c42e6c-f1ed-4258-a14d-8cf18950f28e", "LCL Freight Default Charge Codes"),
						RegistryOptions.PreserveTestValue,
						RegistryConstants.Strings.DefaultFreightChargeCode,
						RegistryFindBoxFilter.FreightChargeCode);
				});
			}
		}

		public LocationsChargesRegistryItem AirOriginDefaultChargeCodes
		{
			get
			{
				return GetItem<LocationsChargesRegistryItem>("ORGDefaultCodes", delegate
				{
					return new LocationsChargesRegistryItem(
						"ORGDefaultCodes",
						Categories.AutoRating_ChargeCodes_Origin,
						ResString.GetMultilingualString("bbe6a0ea-70f4-400f-a8b8-ea105bb97397", "Air Origin Default Charge Codes"),
						ResString.GetMultilingualString("bbe6a0ea-70f4-400f-a8b8-ea105bb97397", "Air Origin Default Charge Codes"),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						new LocationsChargesCollection());
				});
			}
		}

		public LocationsChargesRegistryItem SeaOriginDefaultChargeCodes
		{
			get
			{
				return GetItem<LocationsChargesRegistryItem>("SEAORGDefaultCodes", delegate
				{
					return new LocationsChargesRegistryItem(
						"SEAORGDefaultCodes",
						Categories.AutoRating_ChargeCodes_Origin,
						ResString.GetMultilingualString("b29429ac-1626-4f11-8007-0cd9ecc2b9d3", "Sea Origin Default Charge Codes"),
						ResString.GetMultilingualString("b29429ac-1626-4f11-8007-0cd9ecc2b9d3", "Sea Origin Default Charge Codes"),
						RegistryStorageFlags.Company,
						new LocationsChargesCollection());
				});
			}
		}

		public LocationsChargesRegistryItem AirDestinationDefaultChargeCodes
		{
			get
			{
				return GetItem<LocationsChargesRegistryItem>("DSTDefaultCodes", delegate
				{
					return new LocationsChargesRegistryItem(
						"DSTDefaultCodes",
						Categories.AutoRating_ChargeCodes_Destination,
						ResString.GetMultilingualString("e24376c2-428d-4536-9b0e-42aa1a50095d", "Air Destination Default Charge Codes"),
						ResString.GetMultilingualString("e24376c2-428d-4536-9b0e-42aa1a50095d", "Air Destination Default Charge Codes"),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						new LocationsChargesCollection());
				});
			}
		}

		public LocationsChargesRegistryItem SeaDestinationDefaultChargeCodes
		{
			get
			{
				return GetItem<LocationsChargesRegistryItem>("SEADSTDefaultCodes", delegate
				{
					return new LocationsChargesRegistryItem(
						"SEADSTDefaultCodes",
						Categories.AutoRating_ChargeCodes_Destination,
						ResString.GetMultilingualString("56f15daf-3aef-41ff-b049-24e76858707c", "Sea Destination Default Charge Codes"),
						ResString.GetMultilingualString("56f15daf-3aef-41ff-b049-24e76858707c", "Sea Destination Default Charge Codes"),
						RegistryStorageFlags.Company,
						new LocationsChargesCollection());
				});
			}
		}

		public DecimalArrayRegistryItem AIRFreightWeightBreaks
		{
			get
			{
				return GetItem<DecimalArrayRegistryItem>("AIRFreightWeightBreaks", delegate
				{
					DecimalArrayRegistryItem result = new DecimalArrayRegistryItem(
						"AIRFreightWeightBreaks",
						Categories.AutoRating_ChargeCodes_Freight,
						ResString.GetMultilingualString("05f48a19-1fd1-4eae-b1f3-46c88bf4feeb", "Air Freight Weight Breaks"),
						ResString.GetMultilingualString("dd448ff5-e081-45e7-b1c4-0158dbab9bcd", "Specifies the default weight breaks to use when specifying sliding rates for Air freight."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						new decimal[] { 45m, 100m, 250m, 500m, 1000m });
					result.DataType.LowerBound = 0m;
					result.DataType.DecimalPlaces = 1;
					return result;
				});
			}
		}

		public ChargeCodeRegistryItem WarehouseCartageChargeCode
		{
			get
			{
				return GetItem<ChargeCodeRegistryItem>("WarehouseCartageChargeCode", delegate
				{
					ChargeCodeRegistryItem result = new ChargeCodeRegistryItem("WarehouseCartageChargeCode", Categories.AutoRating_ChargeCodes_Freight, ResString.GetMultilingualString("332ee36e-8ab2-4efc-8498-caa8e041c2a4", "Warehouse Port Transport Charge Code"), ResString.GetMultilingualString("332ee36e-8ab2-4efc-8498-caa8e041c2a4", "Warehouse Port Transport Charge Code"), RegistryConstants.Strings.DefaultFreightChargeCode);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.MrgDsbOrMjaChargeCode);
					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		#endregion

		#region Shipping Charge Codes

		public ChargeCodeListRegistryItem ShippingContainerisedDefaultCodes
		{
			get
			{
				return GetItem<ChargeCodeListRegistryItem>("ShippingContainerisedDefaultCodes", delegate
				{
					return new ChargeCodeListRegistryItem(
						"ShippingContainerisedDefaultCodes",
						Categories.AutoRating_ChargeCodes_Freight,
						ResString.GetMultilingualString("1580c3ee-7eab-47a7-b2d3-4fa8c9fe9feb", "Shipping Containerized Freight Default Charge Codes"),
						ResString.GetMultilingualString("1580c3ee-7eab-47a7-b2d3-4fa8c9fe9feb", "Shipping Containerized Freight Default Charge Codes"),
						RegistryConstants.Strings.DefaultFreightChargeCode,
						RegistryFindBoxFilter.FreightChargeCode);
				});
			}
		}

		public ChargeCodeListRegistryItem ShippingNonContainerisedtDefaultCodes
		{
			get
			{
				return GetItem<ChargeCodeListRegistryItem>("ShippingNonContainerisedtDefaultCodes", delegate
				{
					return new ChargeCodeListRegistryItem(
						"ShippingNonContainerisedtDefaultCodes",
						Categories.AutoRating_ChargeCodes_Freight,
						ResString.GetMultilingualString("12dbfcd1-e0fa-4093-9a8e-cbc5cd382634", "Shipping Non-Containerized Freight Default Charge Codes"),
						ResString.GetMultilingualString("12dbfcd1-e0fa-4093-9a8e-cbc5cd382634", "Shipping Non-Containerized Freight Default Charge Codes"),
						RegistryConstants.Strings.DefaultFreightChargeCode,
						RegistryFindBoxFilter.FreightChargeCode);
				});
			}
		}

		public LocationsChargesRegistryItem ShippingOriginDefaultChargeCodes
		{
			get
			{
				return GetItem<LocationsChargesRegistryItem>("ShippingOriginDefaultChargeCodes", delegate
				{
					return new LocationsChargesRegistryItem(
						"ShippingOriginDefaultChargeCodes",
						Categories.AutoRating_ChargeCodes_Origin,
						ResString.GetMultilingualString("cdb9f84e-f0ab-48f0-8794-25cdf78bd5ed", "Shipping Origin Default Charge Codes"),
						ResString.GetMultilingualString("cdb9f84e-f0ab-48f0-8794-25cdf78bd5ed", "Shipping Origin Default Charge Codes"),
						RegistryStorageFlags.Company,
						new LocationsChargesCollection());
				});
			}
		}

		public LocationsChargesRegistryItem ShippingDestinationDefaultChargeCodes
		{
			get
			{
				return GetItem<LocationsChargesRegistryItem>("ShippingDestinationDefaultChargeCodes", delegate
				{
					return new LocationsChargesRegistryItem(
						"ShippingDestinationDefaultChargeCodes",
						Categories.AutoRating_ChargeCodes_Destination,
						ResString.GetMultilingualString("1c73b2c9-9dcf-470e-9f70-3f7f175b391a", "Shipping Destination Default Charge Codes"),
						ResString.GetMultilingualString("1c73b2c9-9dcf-470e-9f70-3f7f175b391a", "Shipping Destination Default Charge Codes"),
						RegistryStorageFlags.Company,
						new LocationsChargesCollection());
				});
			}
		}

		#endregion

		#region Destination Charge Codes

		public ChargeCodeRegistryItem DestinationDemurrageServiceChargeCode
		{
			get
			{
				return GetItem("DestinationDemurrageServiceChargeCode", delegate
				{
					var caption = ResString.GetMultilingualString("e03c09af-cf20-4f16-893c-1058155b84dd", "Destination Truck Wait Time");
					var hint = ResString.GetMultilingualString("99ae4f30-b221-4b7c-b19b-98cf9e96055e", "The Charge Code is selected based on the charge group being DST and the sub group being DME. Please 'Override Default' to make the appropriate selection.");
					var result = new ChargeCodeRegistryItem("DestinationDemurrageServiceChargeCode", Categories.AutoRating_ChargeCodes_Destination, caption, hint, "DCDEM")
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None)
					};

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}
		public ChargeCodeRegistryItem DestinationMergedDemurrageDetentionChargeCode
		{
			get
			{
				return GetItem("DestinationMergedDemurrageDetentionChargeCode", delegate
				{
					var caption = ResString.GetMultilingualString("4AF9F955-765B-4C78-9D80-3D71BE35CBD5", "Destination Merged Demurrage & Detention");
					var hint = ResString.GetMultilingualString("E9A36A92-0F4C-4835-8959-42A2719C7A2F", "The Charge Code is selected based on the charge group being DST and the sub group being MDD. Please ‘Override Default’ to make the appropriate selection.");
					var result = new ChargeCodeRegistryItem("DestinationMergedDemurrageDetentionChargeCode", Categories.AutoRating_ChargeCodes_Destination, caption, hint, string.Empty)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None)
					};

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public ChargeCodeRegistryItem DestinationDetentionServiceChargeCode
		{
			get
			{
				return GetItem("DestinationDetentionServiceChargeCode", delegate
				{
					var caption = ResString.GetMultilingualString("770ac926-25c6-4ca1-8e46-39e57fd06499", "Destination Detention");
					var hint = ResString.GetMultilingualString("78660d66-ef75-40e4-b504-6f7a593eb30c", "The Charge Code is selected based on the charge group being DST and the sub group being DTN. Please 'Override Default' to make the appropriate selection.");
					var result = new ChargeCodeRegistryItem("DestinationDetentionServiceChargeCode", Categories.AutoRating_ChargeCodes_Destination, caption, hint, "DCDET")
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None)
					};

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public ChargeCodeRegistryItem DestinationLaborServiceChargeCode
		{
			get
			{
				return GetItem("DestinationLaborServiceChargeCode", delegate
				{
					var caption = ResString.GetMultilingualString("973a6b69-e93c-4981-ab24-3f02a6d84d31", "Destination Labor Service");
					var hint = ResString.GetMultilingualString("2f890080-40cd-4bb3-b489-c3d005e47f93", "The Charge Code is selected based on the charge group being DST and the sub group being LBR. Please 'Override Default' to make the appropriate selection.");
					var result = new ChargeCodeRegistryItem("DestinationLaborServiceChargeCode", Categories.AutoRating_ChargeCodes_Destination, caption, hint, "DLAB")
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None)
					};

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public ChargeCodeRegistryItem DestinationStorageServiceChargeCode
		{
			get
			{
				return GetItem("DestinationStorageServiceChargeCode", delegate
				{
					var caption = ResString.GetMultilingualString("3b1ad252-35b5-4a93-8c94-bdc132b41bc6", "Destination CTO Storage");
					var hint = ResString.GetMultilingualString("58438049-ba75-4205-8f1b-d96827a60a81", "The Charge Code is selected based on the charge group being DST and the sub group being STG. Please 'Override Default' to make the appropriate selection.");
					var result = new ChargeCodeRegistryItem("DestinationStorageServiceChargeCode", Categories.AutoRating_ChargeCodes_Destination, caption, hint, "DSTOR")
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None)
					};

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public ChargeCodeRegistryItem DestinationCarrierStorageChargeCode
		{
			get
			{
				return GetItem("DestinationCarrierStorageChargeCode", delegate
				{
					var caption = ResString.GetMultilingualString("ad40df50-ac78-4715-862f-52a550005d22", "Destination Carrier Storage/Demurrage");
					var hint = ResString.GetMultilingualString("1ec3e09a-27c7-4597-90ec-5955e56d055d", "The Charge Code is selected based on the charge group being DST and the sub group being STC. Please 'Override Default' to make the appropriate selection.");
					var result = new ChargeCodeRegistryItem("DestinationCarrierStorageChargeCode", Categories.AutoRating_ChargeCodes_Destination, caption, hint, string.Empty)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None)
					};

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		#endregion

		#region Origin Charge Codes

		public ChargeCodeRegistryItem OriginDetentionChargeCode
		{
			get
			{
				return GetItem("OriginDetentionChargeCode", delegate
				{
					var caption = ResString.GetMultilingualString("88FC10AF-EAB7-42D2-B7D2-981A8325EE00", "Origin Detention");
					var hint = ResString.GetMultilingualString("ED1BCA88-4E35-4A3E-8671-00AF9092726F", "The Charge Code is selected based on the charge group being ORG and the sub group being DTN. Please 'Override Default' to make the appropriate selection.");
					var result = new ChargeCodeRegistryItem("OriginDetentionChargeCode", Categories.AutoRating_ChargeCodes_Origin, caption, hint, "")
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None)
					};

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public ChargeCodeRegistryItem OriginStorageChargeCode
		{
			get
			{
				return GetItem("OriginStorageChargeCode", delegate
				{
					var caption = ResString.GetMultilingualString("23086FD5-5D0F-4096-BCB0-0EED01A34354", "Origin CTO Storage");
					var hint = ResString.GetMultilingualString("EE22CED0-4B23-4A39-826B-180EC4DB3085", "The Charge Code is selected based on the charge group being ORG and the sub group being STG. Please 'Override Default' to make the appropriate selection.");
					var result = new ChargeCodeRegistryItem("OriginStorageChargeCode", Categories.AutoRating_ChargeCodes_Origin, caption, hint, "")
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None)
					};

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public ChargeCodeRegistryItem OriginCarrierStorageChargeCode
		{
			get
			{
				return GetItem("OriginCarrierStorageChargeCode", delegate
				{
					var caption = ResString.GetMultilingualString("9b139ed4-10ea-4139-b69d-ed4c62dff04f", "Origin Carrier Storage/Demurrage");
					var hint = ResString.GetMultilingualString("a63e5888-f97d-42b6-b685-b9b5ccda0fe9", "The Charge Code is selected based on the charge group being ORG and the sub group being STC. Please 'Override Default' to make the appropriate selection.");
					var result = new ChargeCodeRegistryItem("OriginCarrierStorageChargeCode", Categories.AutoRating_ChargeCodes_Origin, caption, hint, string.Empty)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None)
					};

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public ChargeCodeRegistryItem OriginDemurrageServiceChargeCode
		{
			get
			{
				return GetItem("OriginDemurrageServiceChargeCode", delegate
				{
					var caption = ResString.GetMultilingualString("329fd07b-f027-4af2-83d4-8019c5c9cf54", "Origin Truck Wait Time");
					var hint = ResString.GetMultilingualString("4f697509-0f6b-4764-9d4c-9bfe67f7f7e4", "The Charge Code is selected based on the charge group being ORG and the sub group being DME. Please 'Override Default' to make the appropriate selection.");
					var result = new ChargeCodeRegistryItem("OriginDemurrageServiceChargeCode", Categories.AutoRating_ChargeCodes_Origin, caption, hint, "OCDEM")
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None)
					};

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public ChargeCodeRegistryItem OriginMergedDemurrageDetentionChargeCode
		{
			get
			{
				return GetItem("OriginMergedDemurrageDetentionChargeCode", delegate
				{
					var caption = ResString.GetMultilingualString("2A5BAADA-3258-4C71-B746-C4A30AE04420", "Origin Merged Demurrage & Detention");
					var hint = ResString.GetMultilingualString("4B909116-3696-4BD6-A329-85AACB7D10B8", "The Charge Code is selected based on the charge group being ORG and the sub group being MDD. Please ‘Override Default’ to make the appropriate selection.");
					var result = new ChargeCodeRegistryItem("OriginMergedDemurrageDetentionChargeCode", Categories.AutoRating_ChargeCodes_Origin, caption, hint, string.Empty)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None)
					};

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public ChargeCodeRegistryItem OriginLaborServiceChargeCode
		{
			get
			{
				return GetItem("OriginLaborServiceChargeCode", delegate
				{
					var caption = ResString.GetMultilingualString("b57eb729-024e-4cda-87eb-60d6c1a181ef", "Origin Labor Service");
					var hint = ResString.GetMultilingualString("92b91ec7-3fe0-4d89-a1cf-4259f6834c78", "The Charge Code is selected based on the charge group being ORG and the sub group being LBR. Please 'Override Default' to make the appropriate selection.");
					var result = new ChargeCodeRegistryItem("OriginLaborServiceChargeCode", Categories.AutoRating_ChargeCodes_Origin, caption, hint, "OLAB")
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None)
					};

					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		#endregion

		#region Rates Service/Universal Rates Service

		#region Universal Rates Service Url

		public RatesServiceUrlRegistryItem UniversalRatesServiceUrl
		{
			get
			{
				const string key = "UniversalRatesServiceUrl";
				return GetItem(key, () => new RatesServiceUrlRegistryItem(
						key,
						RatingDataRegistry.Categories.AutoRating_UniversalRatesService,
						(NoResString)"URS URL",
						(NoResString)@"Support Only Registry. A URL to the Universal Rates web service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"https://urs.wisegrid.net"));
			}
		}

		#endregion

		#region Rates Service Url

		public RatesServiceUrlRegistryItem RatesServiceUrl
		{
			get
			{
				const string key = "WiseRatesServiceUrl";
				return GetItem(key, () => new RatesServiceUrlRegistryItem(
						key,
						RatingDataRegistry.Categories.AutoRating_RatesService,
						(NoResString)"Rates Service URL",
						(NoResString)@"Support Only Registry. A URL to the Rates Service web service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"https://rates.wisegrid.net"));
			}
		}

		public IRegistryItem RatesServiceAdminInterfaceUrl
		{
			get
			{
				const string key = nameof(RatesServiceAdminInterfaceUrl);
				return GetItem(key, () => new StringRegistryItem(
					key,
					Categories.AutoRating_RatesService,
					(NoResString)"Rates Service Admin Interface URL",
					(NoResString)"Support Only Registry. A URL to the Rates Service Admin Interface",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					"http://wiseratesadmin.wtg.zone"));
			}
		}

		#endregion

		#region WTG Auth Service Url

		public StringRegistryItem WTGAuthServiceUrl
		{
			get
			{
				var key = nameof(WTGAuthServiceUrl);
				return GetItem(key, () => new StringRegistryItem(
					key,
					RatingDataRegistry.Categories.AutoRating_RatesService,
					(NoResString)"WTG Authentication Service URL",
					(NoResString)"Support Only Registry. An URL to the WTG Authentication Service.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					"https://auth.wisegrid.net"));
			}
		}

		#endregion

		#region System Code

		public StringRegistryItem EnterpriseCodeOverride
		{
			get
			{
				const string key = nameof(EnterpriseCodeOverride);
				return GetItem(key, () => new StringRegistryItem(
					key,
					Categories.AutoRating_RatesService,
					(NoResString)"Enterprise Code",
					(NoResString)"Overriding this Registry allows to modify the Enterprise Code value sent in the authentication token when sending requests to Rates Service / URS.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForCargoWise,
					ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode));
			}
		}

		#endregion

		#endregion

		#endregion

		#region Calculators

		public MultilingualStringRegistryItem BaseRateText
		{
			get
			{
				return GetItem<MultilingualStringRegistryItem>("BaseRateText", delegate
				{
					return new MultilingualStringRegistryItem(
						"BaseRateText",
						Categories.AutoRating_Calculation,
						ResString.GetMultilingualString("5b27b7b2-7c38-42c7-820a-f8b6914db5f2", "Base Rate Text"),
						ResString.GetMultilingualString("88015922-92ea-42f0-bfa7-2ef3ad22fbde", "Specifies the text to use for the Base/Flat Part of a Calculator rate"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						ResString.GetMultilingualString("77c44be6-e233-41bd-96af-83772f6d6f7c", "Base Rate"));
				});
			}
		}

		#region Agency

		public CodePairRegistryItem DefaultFeeType
		{
			get
			{
				return GetItem<CodePairRegistryItem>("DefaultFeeType", delegate
				{
					return new CodePairRegistryItem(
						"DefaultFeeType",
						Categories.AutoRating_Calculation_AgencyCalculator,
						ResString.GetMultilingualString("df2371bc-1190-4a2e-bf01-532875fc9023", "Default Fee Type"),
						ResString.GetMultilingualString("169a2e84-18aa-4704-9be4-40bd7be12d37", "Default Fee Type for Agency Calculator"),
						FeeTypeListProvider,
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						RateFeeTypeList.Codes.PerShipment);
				});
			}
		}

		public CodePairRegistryItem DefaultLineType
		{
			get
			{
				return GetItem<CodePairRegistryItem>("DefaultLineType", delegate
				{
					return new CodePairRegistryItem(
						"DefaultLineType",
						Categories.AutoRating_Calculation_AgencyCalculator,
						ResString.GetMultilingualString("68cabb67-3412-4ce3-8a02-29ac6b22f1a3", "Default Line Type"),
						ResString.GetMultilingualString("9041b2c8-9ecc-4bfb-8be7-16ceecbccba9", "Default Line Type for Agency Calculator"),
						LineTypeListProvider,
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						RateLineTypeList.Codes.FLAT);
				});
			}
		}

		#region TypeLists

		ICodeDescriptionPairListProvider FeeTypeListProvider => fFeeTypeListProvider ?? (fFeeTypeListProvider = new CodeDescriptionPairListProvider(() => new RateFeeTypeList()));
		ICodeDescriptionPairListProvider fFeeTypeListProvider;

		public CodeDescriptionPairList FeeTypeList => fFeeTypeList ?? (fFeeTypeList = FeeTypeListProvider.CodeDescriptionPairList);
		CodeDescriptionPairList fFeeTypeList;

		ICodeDescriptionPairListProvider LineTypeListProvider => fLineTypeListProvider ?? (fLineTypeListProvider = new CodeDescriptionPairListProvider(() => new RateLineTypeList()));
		ICodeDescriptionPairListProvider fLineTypeListProvider;

		public CodeDescriptionPairList LineTypeList => fLineTypeList ?? (fLineTypeList = LineTypeListProvider.CodeDescriptionPairList);
		CodeDescriptionPairList fLineTypeList;

		#endregion

		#endregion

		#region Sliding

		public BooleanRegistryItem UseHigherWeightOrUnitLowerRateRuleDefault
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UseHigherWeightOrUnitLowerRateRuleDefault", delegate
				{
					return new BooleanRegistryItem(
						"UseHigherWeightOrUnitLowerRateRuleDefault",
						Categories.AutoRating_Calculation_SlidingCalculator,
						ResString.GetMultilingualString("e355038e-13fc-4c1c-8215-00295b37a827", "Higher Weight/Lower Rate Rule Default"),
						ResString.GetMultilingualString("9ae7cef1-fe25-479b-8b0a-3efaa03c1c69", "Specifies the default value for the tick-box 'Higher Weight/Lower Rate' rule on the sliding calculator."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Note Text

		public BooleanRegistryItem UseShowOnBillingWithoutPrefixDefault
		{
			get
			{
				return GetItem<BooleanRegistryItem>("UseShowOnBillingWithoutPrefixDefault", delegate
				{
					return new BooleanRegistryItem(
						"UseShowOnBillingWithoutPrefixDefault",
						Categories.AutoRating_Calculation_FreeTextNoteCalculator,
						ResString.GetMultilingualString("bfdc09ef-7484-4ee4-b9d0-6fd0e49103c3", "Show On Billing Without Prefix Default"),
						ResString.GetMultilingualString("181cf711-957d-4a08-ae35-42bb47407d46", "Specifies the default value for the tick-box 'Show On Billing Without Prefix' on the Free-Text Note calculator."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region AutoRating / Email Notification

		public BooleanRegistryItem ClientRateGoingToExpireNotification
		{
			get
			{
				return GetItem("ClientRateGoingToExpireNotification", delegate
				{
					return new BooleanRegistryItem(
						"ClientRateGoingToExpireNotification",
						Categories.AutoRating_EmailNotification,
						ResString.GetMultilingualString("fad3ef64-5fb4-4c40-bcbb-d0ea1d4c671c", "Client Rate Going To Expire Email"),
						ResString.GetMultilingualString("d87a0b30-c91c-4643-b06c-9460117e19b4", "If this option is on, the customer service and sales representatives will receive email notifications if a client rate is going to expire when auto-rating a job for this client."),
						RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
						false);
				});
			}
		}

		public BooleanRegistryItem ClientRateJustExpiredNotification
		{
			get
			{
				return GetItem("ClientRateJustExpiredNotification", delegate
				{
					return new BooleanRegistryItem(
						"ClientRateJustExpiredNotification",
						Categories.AutoRating_EmailNotification,
						ResString.GetMultilingualString("79598be7-ab23-4d87-87d6-45b2f087cae4", "Client Rate Just Expired Email"),
						ResString.GetMultilingualString("411cef53-5787-4394-9068-c4fe8c2f5f1a", "If this option is on, the customer service and sales representatives will receive email notifications if a client rate is expired when auto-rating a job for this client."),
						RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
						true);
				});
			}
		}

		public BooleanRegistryItem ClientRateNotFoundNotification
		{
			get
			{
				return GetItem("ClientRateNotFoundNotification", delegate
				{
					return new BooleanRegistryItem(
						"ClientRateNotFoundNotification",
						Categories.AutoRating_EmailNotification,
						ResString.GetMultilingualString("4b64e97f-fb43-418e-8d9e-c24c63ccd921", "Client Rate Not Found Email"),
						ResString.GetMultilingualString("5a3f9aaa-8e0c-40c9-929e-57072a7da9e4", "If this option is on, the customer service and sales representatives will receive email notifications if a client rate is not found when auto-rating a job for this client."),
						RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
						true);
				});
			}
		}

		public BooleanRegistryItem OneOffQuoteUsedNotification
		{
			get
			{
				return GetItem("OneOffQuoteUsedNotification", delegate
				{
					return new BooleanRegistryItem(
						"OneOffQuoteUsedNotification",
						Categories.AutoRating_EmailNotification,
						ResString.GetMultilingualString("adcde05f-f077-4ee3-9243-17ff1410bea8", "One Off Quote Used Email"),
						ResString.GetMultilingualString("20238bae-add3-4358-9fe5-1e26b30004fd", "If this option is on, the customer service and sales representatives will receive email notifications if one off quote is used."),
						RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
						true);
				});
			}
		}

		public BooleanRegistryItem IncludeSalesRepresentative
		{
			get
			{
				return GetItem("IncludeSalesRepresentative", delegate
				{
					return new BooleanRegistryItem(
						"IncludeSalesRepresentative",
						Categories.AutoRating_EmailNotification,
						ResString.GetMultilingualString("8d392ca5-52c3-4c32-8e12-5e3dabae0bf0", "Include Sales Representative as Recipient"),
						ResString.GetMultilingualString("021c2082-582b-4b0e-a5c9-568bf65d6915", "Determines whether rating email notifications should be sent to the sales representative."),
						RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
						true);
				});
			}
		}

		public BooleanRegistryItem IncludeCustomerService
		{
			get
			{
				return GetItem("IncludeCustomerService", delegate
				{
					return new BooleanRegistryItem(
						"IncludeCustomerService",
						Categories.AutoRating_EmailNotification,
						ResString.GetMultilingualString("b3d6611a-5dd4-4076-83c6-445d0c1514b4", "Include Customer Service as Recipient"),
						ResString.GetMultilingualString("2b06ffb0-1de6-4e7f-86a2-1261c3dd0b6b", "Determines whether rating email notifications should be sent to the customer service."),
						RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
						true);
				});
			}
		}

		public BooleanRegistryItem CompanyTariffGoingToExpireNotification
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CompanyTariffGoingToExpireNotification", delegate
				{
					return new BooleanRegistryItem(
						"CompanyTariffGoingToExpireNotification",
						Categories.AutoRating_EmailNotification,
						ResString.GetMultilingualString("643fb95c-d99c-492c-b3f6-2fb38837e8b5", "Company Tariff Going To Expire Email"),
						ResString.GetMultilingualString("f494ed48-3c59-4075-b4de-bfa42161e34b", "If this option is on, the company tariff notification group will receive email notification if a company tariff is going to expire."),
						RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem CompanyTariffJustExpiredNotification
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CompanyTariffJustExpiredNotification", delegate
				{
					return new BooleanRegistryItem(
						"CompanyTariffJustExpiredNotification",
						Categories.AutoRating_EmailNotification,
						ResString.GetMultilingualString("3bbe6b2f-ecae-4eee-ac7a-c25d11daa446", "Company Tariff Just Expired Email"),
						ResString.GetMultilingualString("c84ab6a1-6aa0-478e-8a38-4fb67cd702f0", "If this option is on, the company tariff notification group will receive email notification if a company tariff is expired."),
						RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public GuidRegistryItem CompanyTariffNotificationEmailGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("CompanyTariffNotificationEmailGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"CompanyTariffNotificationEmailGroup",
						Categories.AutoRating_EmailNotification,
						ResString.GetMultilingualString("d00a3ae8-0ebc-4df6-9254-96a82925e765", "Company Tariff Expiring / Expired Notification Group"),
						ResString.GetMultilingualString("da3e2c2b-4667-4183-9b32-c9c01a0a55bb", "This group will receive an email during Auto-rating whenever a company tariff rate is expiring / has expired based on the Company Tariff expiry settings."),
						RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
						RegistryOptions.PreserveTestValue,
						Guid.Empty);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem DefaultSalesRepresentative
		{
			get
			{
				return GetItem<GuidRegistryItem>("DefaultSalesRepresentative", delegate
				{
					var result = new GuidRegistryItem(
						"DefaultSalesRepresentative",
						Categories.AutoRating_EmailNotification,
						ResString.GetMultilingualString("c4c2996c-0303-48e8-beb9-db15ae244b7e", "Default Sales Representative"),
						ResString.GetMultilingualString("09f296af-7854-4581-bcbc-5d825c8434a6", "The default Sales Representative to use on all Sales Organizations. Will be used for email notifications if no sales representative is found on the sales organization."),
						RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
						Guid.Empty);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbStaff);
					return result;
				});
			}
		}

		public GuidRegistryItem DefaultCustomerService
		{
			get
			{
				return GetItem<GuidRegistryItem>("DefaultCustomerService", delegate
				{
					var result = new GuidRegistryItem(
						"DefaultCustomerService",
						Categories.AutoRating_EmailNotification,
						ResString.GetMultilingualString("0c8d76fd-901d-4f30-9e7b-9fc02b95f89d", "Default Customer Service"),
						ResString.GetMultilingualString("7d94e6f8-92df-4df9-9935-b789ae113df0", "The default Customer Service Representative to use on all Sales Organizations. Will be used for email notifications if no customer service representative is found on the sales organization."),
						RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
						Guid.Empty);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbStaff);
					return result;
				});
			}
		}

		#endregion

		#region IncoTerm

		public IncoTermChargeCodesRegistryItem IncoTermDefinition
		{
			get
			{
				return GetItem<IncoTermChargeCodesRegistryItem>("IncoTermDefinition", delegate
				{
					return new IncoTermChargeCodesRegistryItem(
						"IncoTermDefinition",
						Categories.AutoRating_ChargeCodeGroups,
						ResString.GetMultilingualString("24374b8e-fdbb-76a7-423c-686ff86b3dba", "Incoterm Charge Code Group Configuration"),
						ResString.GetMultilingualString("f0eb34a3-303b-af82-4988-ce13c7b7f89d", "This registry item allows you to configure the behavior of Incoterms in respect to the different freight related charge code groups. Defaults based on the standard definition of each Incoterm are provided, but can be changed by you to change certain parts of an Incoterm."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						IncoTermChargeCodesCollection.GetDefault());
				});
			}
		}

		#endregion

		#region Rounding

		public BooleanRegistryItem RoundingUsesWeightVolumeMultiple
		{
			get
			{
				return GetItem<BooleanRegistryItem>("RoundingUsesWeightVolumeMultiple", delegate
				{
					return new BooleanRegistryItem(
						"RoundingUsesWeightVolumeMultiple",
						Categories.AutoRating_Calculation_Rounding,
						ResString.GetMultilingualString("80d37dcc-91e8-4aa7-8770-444c58897867", "Rounding Uses Weight/Vol Multiple"),
						ResString.GetMultilingualString("a21f6cab-1a58-4dd7-96f6-a36fa7b91312", "If this registry is turned on, the system will apply rounding to the chargeable amount taking into account the weight/volume multiple specified. For example, if the rate is specified per 100 Kg, Round Up will round 95.5 kg to 100 Kg. If this registry is off (default), it will round up to 96 Kg."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Priorities

		MultilingualString GetRatesPrioritiesHint(MultilingualString termAndDirection)
		{
			return ResString.GetMultilingualString(
				"37b7e2e1-82e2-46fa-84f2-313c2856f69b",
				"This registry item allows you to set priorities for {0} sell rates. Autorating will prioritize rates which are higher in the list in a case of charge code conflicts. Removing particular Organization Type from the list will make its rates not applicable.",
				termAndDirection);
		}

		RatesPrioritiesRegistryItem GetRatesPrioritiesRegistryItem(string registryName, MultilingualString termAndDirection, RegistryOptions options = RegistryOptions.Default)
		{
			return GetItem
			(
				registryName,
				() => new RatesPrioritiesRegistryItem
				(
					registryName,
					Categories.AutoRating_ChargeCodeGroups_SellRatesPriorities,
					termAndDirection,
					GetRatesPrioritiesHint(termAndDirection),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					options,
					RatesPrioritiesCollection.GetDefault(registryName)
				)
			);
		}

		public RatesPrioritiesRegistryItem ImportPrepaidPriorities
		{
			get { return GetRatesPrioritiesRegistryItem(ImportPrepaidPrioritiesRegistryName, ResString.GetMultilingualString("22200915-681d-4386-b863-1494bf5dfa8d", "Import Prepaid")); }
		}
		internal const string ImportPrepaidPrioritiesRegistryName = "ImportPrepaidPriorities";

		public RatesPrioritiesRegistryItem ExportPrepaidPriorities
		{
			get { return GetRatesPrioritiesRegistryItem(ExportPrepaidPrioritiesRegistryName, ResString.GetMultilingualString("41e3da9c-c278-4b11-9aad-7c15dd6e2b67", "Export Prepaid")); }
		}
		internal const string ExportPrepaidPrioritiesRegistryName = "ExportPrepaidPriorities";

		public RatesPrioritiesRegistryItem ImportCollectPriorities
		{
			get { return GetRatesPrioritiesRegistryItem(ImportCollectPrioritiesRegistryName, ResString.GetMultilingualString("0423f46b-5f8e-4a0a-86be-a222e580c1a7", "Import Collect")); }
		}
		internal const string ImportCollectPrioritiesRegistryName = "ImportCollectPriorities";

		public RatesPrioritiesRegistryItem ExportCollectPriorities
		{
			get { return GetRatesPrioritiesRegistryItem(ExportCollectPrioritiesRegistryName, ResString.GetMultilingualString("0096904b-9513-439f-a699-b46065166a1a", "Export Collect")); }
		}
		internal const string ExportCollectPrioritiesRegistryName = "ExportCollectPriorities";

		public RatesPrioritiesRegistryItem DomesticPrepaidPriorities
		{
			get { return GetRatesPrioritiesRegistryItem(DomesticPrepaidPrioritiesRegistryName, ResString.GetMultilingualString("b83b10ef-bcbd-4780-afc1-b639b590c18e", "Domestic Prepaid")); }
		}
		internal const string DomesticPrepaidPrioritiesRegistryName = "DomesticPrepaidPriorities";

		public RatesPrioritiesRegistryItem DomesticCollectPriorities
		{
			get { return GetRatesPrioritiesRegistryItem(DomesticCollectPrioritiesRegistryName, ResString.GetMultilingualString("477fe09d-6112-490a-ab22-5cec0eac7ee3", "Domestic Collect")); }
		}
		internal const string DomesticCollectPrioritiesRegistryName = "DomesticCollectPriorities";

		public RatesPrioritiesRegistryItem CrossTradePrepaidPriorities
		{
			get { return GetRatesPrioritiesRegistryItem(CrossTradePrepaidPrioritiesRegistryName, ResString.GetMultilingualString("afaf1781-053e-4ac7-87b5-a0abfb886462", "Cross Trade Prepaid")); }
		}
		internal const string CrossTradePrepaidPrioritiesRegistryName = "CrossTradePrepaidPriorities";

		public RatesPrioritiesRegistryItem CrossTradeCollectPriorities
		{
			get { return GetRatesPrioritiesRegistryItem(CrossTradeCollectPrioritiesRegistryName, ResString.GetMultilingualString("a543a55f-b860-42a0-b93d-f5a872aae744", "Cross Trade Collect")); }
		}
		internal const string CrossTradeCollectPrioritiesRegistryName = "CrossTradeCollectPriorities";

		public RatesPrioritiesRegistryItem GatewayPrepaidPriorities
		{
			get { return GetRatesPrioritiesRegistryItem(GatewayPrepaidPrioritiesRegistryName, ResString.GetMultilingualString("24480b2c-ac93-4f33-ae9f-53038050ade6", "Gateway Prepaid"), GetGetwayPriorityRegistryOptions()); }
		}
		internal const string GatewayPrepaidPrioritiesRegistryName = "GatewayPrepaidPriorities";

		public RatesPrioritiesRegistryItem GatewayCollectPriorities
		{
			get { return GetRatesPrioritiesRegistryItem(GatewayCollectPrioritiesRegistryName, ResString.GetMultilingualString("c1bfa598-5a4a-40db-a2c2-02de9e204f3f", "Gateway Collect"), GetGetwayPriorityRegistryOptions()); }
		}
		internal const string GatewayCollectPrioritiesRegistryName = "GatewayCollectPriorities";

		static RegistryOptions GetGetwayPriorityRegistryOptions()
		{
			return RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value
				? RegistryOptions.IsHidden
				: RegistryOptions.Default;
		}

		#endregion

		#region ShowAutoRatingNotRunWarning

		public BooleanRegistryItem ShouldShowAutoRatingNotRunWarning
		{
			get
			{
				return GetItem("ShouldShowAutoRatingNotRunWarning",
								 () => new BooleanRegistryItem(
										 "ShouldShowAutoRatingNotRunWarning",
										 Categories.AutoRating_Calculation,
										 ResString.GetMultilingualString("ede9bf8d-4040-4e4f-ba48-ba6a500d4678", "Display warning if AutoRating has not been run"),
										 ResString.GetMultilingualString("a45e280b-bbad-407a-8248-001d932cc416", "Should the user be warned if they attempt to save a job without running AutoRating?"),
										 RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
										 true));
			}
		}

		#endregion

		#region Multimodal Rating

		public BooleanRegistryItem MultiModalRatingCost
		{
			get
			{
				return GetItem("MultiModalRatingCost",
								() => new BooleanRegistryItem(
									"MultiModalRatingCost",
									Categories.AutoRating_MultimodalRating,
									ResString.GetMultilingualString("9dfe31e1-6021-4968-b8c1-123592730f31", "Enable Multi-Route Auto-Costing"),
									ResString.GetMultilingualString("559c91dc-9fac-44cf-86eb-2fd34bf45074", "When this registry is set to ‘Yes’, costs will be autorated per consolidated route set."),
									RegistryStorageFlags.System | RegistryStorageFlags.Company,
									false));
			}
		}

		public BooleanRegistryItem MultiModalRatingCostShipment
		{
			get
			{
				return GetItem("MultiModalRatingCostShipment",
					() => new BooleanRegistryItem(
						"MultiModalRatingCostShipment",
						Categories.AutoRating_MultimodalRating,
						ResString.GetMultilingualString("F1666C4F-66AF-4845-A50D-A7DE4E5E123D", "Enable Multi-Route Auto-Costing (Shipment)"),
						ResString.GetMultilingualString("03B09E46-5472-44BA-8991-CCF1EF16DF6E", "When this registry is set to ‘Yes’, costs will be Autorated per Shipment route set."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		#endregion

		#region Quotes

		public BooleanRegistryItem EnableQuotationDocumentsChargeGroupingSequencingAndRollup
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableQuotationDocumentsChargeGroupingSequencingAndRollup", delegate
				{
					return new BooleanRegistryItem(
						"EnableQuotationDocumentsChargeGroupingSequencingAndRollup",
						Categories.AutoRating_Quotations,
						(NoResString)"Enable Quotation Documents Charge Grouping, Sequencing and Rollup Enhancement",
						(NoResString)"Quotation Documents printing enhancements for Charge Grouping, Sequencing and Rollup will be enabled.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public IntRegistryItem QuoteFollowUpDays
		{
			get
			{
				return GetItem("QuoteFollowUpDays",
								() => new IntRegistryItem(
										"QuoteFollowUpDays",
										Categories.AutoRating_ValidityandNotificationPeriods,
										ResString.GetMultilingualString("3c54f5d1-9e51-43b5-9745-4e2e9c19c264", "Quotation Follow Up Days"),
										ResString.GetMultilingualString("80b8c53f-50d5-46d1-81df-ba108406b4f2", "Specifies the number of days to set a follow up date for when creating a new quotation."),
										null,
										RegistryStorageFlags.Company,
										RegistryOptions.Default,
										7,
										0,
										365));
			}
		}

		public BooleanRegistryItem OneOffQuoteImperialUnitsInchesOnly
		{
			get
			{
				return GetItem<BooleanRegistryItem>("OneOffQuoteImperialUnitsInchesOnly", delegate
				{
					return new BooleanRegistryItem(
						"OneOffQuoteImperialUnitsInchesOnly",
						Categories.AutoRating_Quotations_SpotQuotes,
						ResString.GetMultilingualString("d06655e5-9908-4f38-84a3-661e59605ef7", "Express Imperial Units in Inches Only"),
						ResString.GetMultilingualString("91fef3b0-dc28-4774-b031-3f69566558ef", "Specifies whether imperial units should be displayed in inches only. By default, they are expressed in feet and inches."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem IncludeBranchCodeInQuoteNumber
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IncludeBranchCodeInQuoteNumber", delegate
				{
					return new BooleanRegistryItem(
						"IncludeBranchCodeInQuoteNumber",
						Categories.AutoRating_Quotations,
						ResString.GetMultilingualString("418fe825-c621-4567-a396-d073883f5323", "Include Branch Code In Quote Number"),
						ResString.GetMultilingualString("028e3b23-ad2f-4a15-8ee5-064f3875820c", "Auto-Generate Quote Number using the format:\r\n  QBBBNNNNNNNN\r\nWhere\r\n  BBB = Registered (created by) Login Branch\r\n  NNNNNNNN = Sequential number WITHIN BBB"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public CodeDescriptionBoolRegistryItem QuoteCancellationReasonCodes
		{
			get
			{
				return GetItem<CodeDescriptionBoolRegistryItem>("QuoteCancellationReasonCodes", delegate
				{
					var caption = ResString.GetMultilingualString("33e7748c-3dde-4718-980d-b68afea4c048", "Cancellation Reason Codes");
					var defaultDescription = ResString.GetMultilingualString("79ea7aa8-a3ce-4fa2-925f-5253d0dee0d1", "Undefined - You can modify this in the System Registry, under {0}", RegistryConstants.GetCategory(Categories.AutoRating_Quotations, caption));

					CodeDescriptionBoolCollection defaultList = new CodeDescriptionBoolCollection(3);
					defaultList.Add("UDF", defaultDescription, true);

					return new CodeDescriptionBoolRegistryItem(
						"QuoteCancellationReasonCodes",
						Categories.AutoRating_Quotations,
						caption,
						ResString.GetMultilingualString("e354e89d-6c08-4414-9c2a-7f1daecba352", "Specifies reason codes for quotation cancellation."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("b85680ba-55f9-4e9d-8437-d9efe34d7fd0", "Enabled")),
						defaultList);
				});
			}
		}

		public BooleanRegistryItem IsQuoteCancellationReasonCodeRequired
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IsQuoteCancellationReasonCodeRequired", delegate
					{
						return new BooleanRegistryItem(
							"IsQuoteCancellationReasonCodeRequired",
							Categories.AutoRating_Quotations,
							ResString.GetMultilingualString("6b6d6117-2b11-423f-ac9e-594e89977090", "Cancellation Reason Code Is Required"),
							ResString.GetMultilingualString("17de22e5-5881-4eec-9394-43cf79bf42d2", "If this is on, the cancellation reason will be mandatory when canceling a quotation."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							false);
					});
			}
		}

		public BooleanRegistryItem EnableOverseasAgentInOneOffQuote
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableOverseasAgentInOneOffQuote", delegate
				{
					return new BooleanRegistryItem(
						"EnableOverseasAgentInOneOffQuote",
						Categories.AutoRating_Quotations,
						(NoResString)"Enable Overseas Agent Type in One Off Quote",
						(NoResString)"If this is on, the Overseas Agent Option would be enabled in One off Quote.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public ChargeCodeForPricingPageSectionsRegistryItem ChargeCodeForPricingPageSections
		{
			get
			{
				return GetItem<ChargeCodeForPricingPageSectionsRegistryItem>("ChargeCodeForPricingPageSections", delegate
				{
					return new ChargeCodeForPricingPageSectionsRegistryItem(
						"ChargeCodeForPricingPageSections",
						Categories.AutoRating_Quotations,
						ResString.GetMultilingualString("E026AD22-0B0B-48CC-8528-775CBE774022", "Charge Code for Pricing Page Sections"),
						ResString.GetMultilingualString("C82492BD-144E-45B5-8705-6A306F122A89", "Charges Codes to display under the specified section of the Quotations Pricing Page."),
						RegistryStorageFlags.Company,
						new ChargeCodeForPricingPageSectionsConfigurationCollection());
				});
			}
		}

		#endregion

		#region Gateway Billing

		public BooleanRegistryItem GatewayBillingUseTotalWeightForWeightBreak
		{
			get
			{
				return GetItem<BooleanRegistryItem>("GatewayBillingUseTotalWeightForWeightBreak", delegate
				{
					return new BooleanRegistryItem(
						"GatewayBillingUseTotalWeightForWeightBreak",
						Categories.AutoRating_GatewayBilling,
						ResString.GetMultilingualString("5d2b2ef2-129b-404a-83e2-d2dda27e88d8", "Use Consol Weight to Determine Rating Break"),
						ResString.GetMultilingualString("dd803010-ad69-45de-86e0-923cad6cc226", "If this is on, the consol's chargeable weight will be used to determine which rating weight break to use when rating a gateway consolidation."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem UseIntercompanyTariffsToAutorateGatewayBilling =>
			GetItem(nameof(UseIntercompanyTariffsToAutorateGatewayBilling), () =>
			{
				var registryItem = new BooleanRegistryItem(
					nameof(UseIntercompanyTariffsToAutorateGatewayBilling),
					RatingDataRegistry.Categories.AutoRating_GatewayBilling,
					ResString.GetMultilingualString("61AFFBCC-9BE6-4AD3-848E-CE6930CBD576", "Use Intercompany Tariffs to Autorate Gateway Billing"),
					ResString.GetMultilingualString("61425897-808A-4D73-A468-4346EB14A521", @"Set this registry to ‘Yes’ switches the system to use Intercompany Tariffs only to Autorate Gateway Billing.
Once this registry is set to ‘Yes’, it cannot be reverted."),
					RegistryStorageFlags.System,
					false);

				MakeReadonlyIfNecessary(registryItem.Value);

				registryItem.OnUpdateAction += (cPk, bPk, dPk, value) => MakeReadonlyIfNecessary((bool)value);

				return registryItem;

				void MakeReadonlyIfNecessary(bool value) =>
					registryItem.Options |= value && !Env.CurrentUser.IsSupportUser
						? RegistryOptions.IsReadOnly
						: RegistryOptions.Default;
			});

		#endregion

		#region Warehouse

		public BooleanRegistryItem ExcludeInwardsFromPeriodicAutoRating
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ExcludeInwardsFromPeriodicAutoRating", delegate
				{
					return new BooleanRegistryItem(
						"ExcludeInwardsFromPeriodicAutoRating",
						Categories.AutoRating_Calculation_Warehouse,
						ResString.GetMultilingualString("78de2832-4809-4a1d-b15b-3dbc8fc23e68", "Exclude Warehouse Receipts From Periodic Auto-Rating"),
						ResString.GetMultilingualString("cba0ba9b-bed7-4072-bc56-7c9dffa69dd7", "Specifies the default value for the 'Exclude From Periodic Auto-Rating' checkbox on the Warehouse Receive Billing tab."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem ExcludeOrdersFromPeriodicAutoRating
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ExcludeOrdersFromPeriodicAutoRating", delegate
				{
					return new BooleanRegistryItem(
						"ExcludeOrdersFromPeriodicAutoRating",
						Categories.AutoRating_Calculation_Warehouse,
						ResString.GetMultilingualString("e3bd7b9a-feef-4872-b7ac-3bb2bdd6bfa8", "Exclude Warehouse Orders From Periodic Auto-Rating"),
						ResString.GetMultilingualString("a61df988-62d3-464e-94ff-49da502cb61c", "Specifies the default value for the 'Exclude From Periodic Auto-Rating' checkbox on the Warehouse Orders Billing tab."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public IntRegistryItem WarehouseClientFreeStorageDays
		{
			get
			{
				return GetItem<IntRegistryItem>("WarehouseClientFreeStorageDays", delegate
				{
					return new IntRegistryItem(
						"WarehouseClientFreeStorageDays",
						Categories.AutoRating_Calculation_Warehouse,
						ResString.GetMultilingualString("ac4c3f96-053e-4ec4-ae29-682f10e79ef5", "Free Storage Days"),
						ResString.GetMultilingualString("dd8078d7-a484-4910-94dc-283bbc70d5a3", "Specifies the default number of days that goods can be in storage and not be chargeable."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						0);
				});
			}
		}

		#endregion

		#region AutoRating Log Note

		public BooleanRegistryItem EnableAutoRatingLogNoteForDebug
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableAutoRatingLogNoteDebugLog", delegate
				{
					return new BooleanRegistryItem(
						"EnableAutoRatingLogNoteDebugLog",
						Categories.AutoRating_Calculation,
						(NoResString)"Enable AutoRating Log Note for Debugging",
						(NoResString)"Specifies whether the Autorating Log note includes additional debug information.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Global Rates

		public BooleanRegistryItem GlobalSellRatesOverrideLocal
		{
			get
			{
				var key = nameof(GlobalSellRatesOverrideLocal);
				return GetItem(key, () => new BooleanRegistryItem(
					key,
					Categories.AutoRating_GlobalRates,
					ResString.GetMultilingualString("e0e6b152-1b8e-4c29-b833-10b1fe63c91c", "Global Sell Rates override Local"),
					ResString.GetMultilingualString("1e8b1587-0fd1-4e76-a701-83432167a114", "During the Autorating process, Global Sell Rates override Local Sell Rates"),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					false));
			}
		}

		#endregion

		#region EnableChargeCalculationDescription

		public BooleanRegistryItem EnableLongChargeCalculationDescription
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableLongChargeCalculationDescription", () => new BooleanRegistryItem(
					"EnableLongChargeCalculationDescription",
					RatingDataRegistry.Categories.AutoRating_Calculation,
					ResString.GetMultilingualString("8bc58b8e-cbd9-4324-9a85-c78f80698dcb", "Enable Long Charge Calculation Description"),
					ResString.GetMultilingualString("191e81c6-29d2-4845-9e61-fe9a539c0ff6", "Override this Registry to show the longer detailed formula in the autorated charge description instead of the shorter and simplified version showing only the applied operator of the calculator."),
					RegistryStorageFlags.System,
					false));
			}
		}

		#endregion

		#region AutorateByBBK_BLK_ROR_BCNContainerModes

		public BooleanRegistryItem AutorateByBBK_BLK_ROR_BCNContainerModes
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AutorateByBBK_BLK_ROR_BCNContainerModes", () => new BooleanRegistryItem(
					"AutorateByBBK_BLK_ROR_BCNContainerModes",
					RatingDataRegistry.Categories.AutoRating_Calculation,
					ResString.GetMultilingualString("5C6D85C3-7967-4B40-BF43-DC14096A1478", "Autorate by BBK/BLK/ROR/BCN Container Modes"),
					ResString.GetMultilingualString("B293C635-A1C4-4A68-93D1-B1EA59C657FD", "When registry is set to ‘Yes’, Autorating will match with BBK/BLK/ROR/BCN Container Modes."),
					RegistryStorageFlags.System,
					false));
			}
		}

		#endregion

		public BooleanRegistryItem AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup
		{
			get
			{
				return GetItem<BooleanRegistryItem>("AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup", () => new BooleanRegistryItem(
					"AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup",
					RatingDataRegistry.Categories.AutoRating_Calculation,
					ResString.GetMultilingualString("FDA57EF0-3314-4818-B755-66EC6EE72C06", "Autorate Stand-alone Customs Declaration Job with Own Rates Setup"),
					ResString.GetMultilingualString("705B0114-18C1-4871-A214-19CFD1C9EEAF", "When this registry is set to ‘Yes’, rates setup under Customs tab in Client Rates, Company Tariffs and Costings are used for Autorating Stand-alone Customs Declaration jobs."),
					RegistryStorageFlags.System,
					false));
			}
		}

		public BooleanRegistryItem AllowConsolAutoratingDateSynchronizedAcrossCompanies
		{
			get
			{
				return GetItem<BooleanRegistryItem>(nameof(AllowConsolAutoratingDateSynchronizedAcrossCompanies), () => new BooleanRegistryItem(
					nameof(AllowConsolAutoratingDateSynchronizedAcrossCompanies),
					RatingDataRegistry.Categories.AutoRating_Calculation,
					ResString.GetMultilingualString("5E3BF0C5-4A82-46CF-8D90-B4A52D9B6415", "Allow Consol Autorating Date Synchronized Across All Login Companies"),
					ResString.GetMultilingualString("439EC130-FADD-4A00-A924-1BD852D72127", "When registry is set to ‘Yes’, Consol > Details > Rates> Autorating Date is synchronized across all login companies."),
					RegistryStorageFlags.System,
					false));
			}
		}

		#region EnableWarehouseUnitFactorRatesDevelopment

		public BooleanRegistryItem EnableWarehouseUnitFactorRatesDevelopment
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableWarehouseUnitFactorRatesDevelopment", () => new BooleanRegistryItem(
					"EnableWarehouseUnitFactorRatesDevelopment",
					RatingDataRegistry.Categories.AutoRating_Calculation,
					(NoResString)"Enable Packs Weight and Product Line for Warehouse Orders and Receives",
					(NoResString)"When registry is set to ‘Yes’, product warehouse rate-line unit-factor will have 'Packs Weight' and 'Product Line' option for autorating calculation.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		#endregion

		#region Feature Toggles

		public BooleanRegistryItem EnableSpotRatingBehaviourFeature
		{
			get
			{
				return GetItem("SpotRatingBehavioursAreEnabled", delegate
				{
					return new BooleanRegistryItem(
						"SpotRatingBehavioursAreEnabled",
						Categories.AutoRating_Calculation,
						ResString.GetMultilingualString("0A029F53-94AE-4AC1-899C-85E5967C6823", "Enable Spot Rating Behavior Feature"),
						ResString.GetMultilingualString("483CAF20-D8D0-4E95-AA9B-BD7E872BF80D", "Specifies whether Spot Rating Behavior feature on Consol costs is enabled or not."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem AllowSameUNLOCOInRatingInternationalZones
		{
			get
			{
				return GetItem("AllowSameUNLOCOInRatingInternationalZones", () => new BooleanRegistryItem(
					"AllowSameUNLOCOInRatingInternationalZones",
					Categories.AutoRating,
					ResString.GetMultilingualString("6113a7b4-a4bc-4400-8202-c2039f77274d", "Allow the same UNLOCO for multiple Rating International Zones"),
					ResString.GetMultilingualString("b51aff0b-4aef-458d-b7af-3e00e45066b6", @"Allow the same UNLOCO to be added to multiple International Zones with Type of 'RAT', 'IMP' or 'EXP'.
Note: Setting this registry to Yes may result in overlapping Rates or Charges to be loaded to the relevant Job when Autorating."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					false));
			}
		}

		#endregion

		#region Rating Web Services
		public BooleanRegistryItem SupportJsonMediaType
		{
			get
			{
				return GetItem("SupportJsonMediaType", delegate
				{
					return new BooleanRegistryItem(
						"SupportJsonMediaType",
						Categories.AutoRating_RatingWebService,
						ResString.GetMultilingualString("83E19D47-3869-4F36-9006-AA161477685E", "Enable JSON media type"),
						ResString.GetMultilingualString("E8A52D5C-A92A-44EC-9441-EB82926B2E4A", "Specifies whether JSON Media Type should be supported by server or not."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem ShowExceptionDetailsInResponse
		{
			get
			{
				return GetItem("ShowExceptionDetailsInResponse", delegate
				{
					return new BooleanRegistryItem(
						"ShowExceptionDetailsInResponse",
						Categories.AutoRating_RatingWebService,
						ResString.GetMultilingualString("AF14BF65-0666-4631-A0BB-8357208683D9", "Show Exception Details in Response"),
						ResString.GetMultilingualString("DA751F80-B12E-4114-8F87-B81FD653E69D", "Specifies whether we should return the details of an unhandled exception in response or not."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public IntRegistryItem TokenExpiryDurationInSeconds
		{
			get
			{
				return GetItem("TokenExpiryDurationInSeconds",
					() => new IntRegistryItem("TokenExpiryDurationInSeconds",
						Categories.AutoRating_RatingWebService,
						ResString.GetMultilingualString("A579C336-D146-4284-8C5C-EF17C5387791", "Token Expiry Duration In Seconds"),
						ResString.GetMultilingualString("208B7A0B-A77E-4813-91FF-2E724777B718", "Token Expiry Duration In Seconds."),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						30));
			}
		}

		public RatingTokenAuthenticationRegistryItem RatingTokenAuthentication
		{
			get
			{
				return GetItem("RatingTokenAuthentication",
					() => new RatingTokenAuthenticationRegistryItem("RatingTokenAuthentication",
						Categories.AutoRating_RatingWebService,
						ResString.GetMultilingualString("1368266A-A286-4590-A3B5-E14A31F43DC7", "Token For Rating Authentication"),
						ResString.GetMultilingualString("85F7656E-FE25-43BE-B27B-ADA13A0C881A", "Token For Rating Authentication."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						new RatingTokenAuthenticationCollection()));
			}
		}
		#endregion

		#region Autorating Via Port

		public AutoratingViaPortRegistryItem AutoratingViaPort
		{
			get
			{
				return GetItem<AutoratingViaPortRegistryItem>("AutoratingViaPort", delegate
				{
					return new AutoratingViaPortRegistryItem(
					"AutoratingViaPort",
					Categories.AutoRating_Calculation,
					ResString.GetMultilingualString("23E5B048-A504-4355-AB01-5DFCA478B32F", "Autorating Via Port"),
					ResString.GetMultilingualString("96963FE0-467D-490E-BE37-22099B0D10F3", "This registry item allows you to define 'Via' ports for Autorating Cost/Revenue for Booking, Shipment and Consol."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					AutoratingViaPortConfigurationCollection.Default);
				});
			}
		}

		#endregion

		#region HBL Delivery Priority

		public HBLDeliveryPriorityRegistryItem HBLDeliveryPriority
		{
			get
			{
				return GetItem<HBLDeliveryPriorityRegistryItem>("HBLDeliveryPriority", delegate
				{
					return new HBLDeliveryPriorityRegistryItem(
					"HBLDeliveryPriority",
					Categories.AutoRating_Calculation,
					ResString.GetMultilingualString("3F9903A9-63A5-4BFA-9ABA-B4B0BDD79257", "HBL Delivery Mode Matching Values with Priorities"),
					ResString.GetMultilingualString("D2A00987-826C-4DFC-8B5F-AB65B2B21539", "HBL Delivery Mode on Forwarding Shipment can be matched with one or more HBL Delivery Modes when Autorating with priority for the same Charge Code found."),
					RegistryStorageFlags.System,
					RegistryOptions.Default);
				});
			}
		}

		#endregion

		#region CargoWise CarrierConnect For Job Autorating

		public BooleanRegistryItem CargoWiseCarrierConnectForJobAutorating
		{
			get
			{
				return GetItem("CargoWiseCarrierConnectForJobAutorating", () => new BooleanRegistryItem(
					"CargoWiseCarrierConnectForJobAutorating",
					Categories.AutoRating_RateSelector,
					ResString.GetMultilingualString("56f9d3ff-8947-44d6-bd25-34f1f552dbdc", "Enable CargoWise CarrierConnect for Job Autorating"),
					ResString.GetMultilingualString("852b6995-6bf7-45f6-b5a1-9aa3c6ff8da0", "Enables the usage of CargoWise CarrierConnect which replaces the existing Rate Selectors."),
					RegistryStorageFlags.System,
					RatingFeatureHelper.CarrierConnect.IsFeatureEnabled() ? RegistryOptions.Default : RegistryOptions.IsHidden,
					true
				));
			}
		}

		#endregion

		#region Enable URS Integration

		public BooleanRegistryItem EnableUrsIntegration
		{
			get
			{
				return GetItem("EnableUrsIntegration", delegate
				{
					return new BooleanRegistryItem(
						"EnableUrsIntegration",
						RatingDataRegistry.Categories.AutoRating_UniversalRatesService,
						(NoResString)"Enable URS Integration", // Support only registry
						(NoResString)@"Support Only Registry. By enabling this registry, rates external to CargoWise will only be retrieved through integration with Universal Rates Service but NOT through Rates Service. This is applied to both Autorating and Multimodal Rates searching.", // Support only registry
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Use URS For Legacy Rate Selector and MMS

		public BooleanRegistryItem UseUrsForLegacyRateSelectorAndMMS
		{
			get
			{
				return GetItem(nameof(UseUrsForLegacyRateSelectorAndMMS), delegate
				{
					return new BooleanRegistryItem(
						nameof(UseUrsForLegacyRateSelectorAndMMS),
						RatingDataRegistry.Categories.AutoRating_UniversalRatesService,
						ResString.GetMultilingualString("665A85BB-A25F-47E6-AE2A-56DBBEA010C5", "Use URS For Legacy Rate Selectors/MMS"), // Support only registry
						ResString.GetMultilingualString("BD429203-BFE9-41AE-82F0-DFDE47E8FF27", "Enables the usage of URS for the Legacy Rate Selectors and Multimodal Rates."),
						RegistryStorageFlags.System,
						RatingFeatureHelper.Urs.IsEnabled ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						false);
				});
			}
		}

		#endregion
	}
}
