using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.Billing.Module
{
	class BillingPricesFilterBusinessObject : FilterStripBusinessObject, IRelatedModuleFilterBusinessObject
	{
		public BillingPricesFilterBusinessObject()
		{
			QueryObjectType = typeof(ClientLicencePriceItem);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddTextFilters(filters);
			AddNumberRangeFilters(filters);
			AddPriceHeaderFilters(filters);

			return filters;
		}

		#region Add Filters

		void AddNumberRangeFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberRangeFilter("Price", ClientLicencePriceItemSchema.L7_Price);
			filters.AddNumberRangeFilter("Licence Units", ClientLicencePriceItemSchema.L7_LicenceUnits);
			filters.AddNumberRangeFilter("Unit Break", ClientLicencePriceItemSchema.L7_UnitBreak);
			filters.AddNumberRangeFilter("Order", ClientLicencePriceItemSchema.L7_Order);
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Fee Basis", ClientLicencePriceItemSchema.L7_FeeType);
			filters.AddTextFilter("Discount Group", ClientLicencePriceItemSchema.L7_PGM_DiscountGroupCode);
			filters.AddFiltersForTranslatableText("Charge Basis", ClientLicencePriceItemSchema.L7_ChargeBasis, typeof(ClientLicencePriceItem), ResString.GetMultilingualString("BillingPricesFilterBusinessObject|ChargeBasis", "Charge Basis"));

			filters.AddFilter(new ModuleTextFilter("Category", ClientLicencePriceItemSchema.L7_Category, EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value));
			filters.AddTextFilter("Code", ClientLicencePriceItemSchema.L7_Code);
			filters.AddFilter(new ModuleTextFilter("Parent Category", ClientLicencePriceItemSchema.L7_ParentCategory, EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value));
			filters.AddTextFilter("Parent Code", ClientLicencePriceItemSchema.L7_ParentCode);
			filters.AddTextFilter("Web Parent Code", ClientLicencePriceItemSchema.L7_WebParentCode);
			filters.AddTextFilter("Charge Code", ClientLicencePriceItemSchema.L7_ChargeCode);
			filters.AddTextFilter("Deposit Charge Code", ClientLicencePriceItemSchema.L7_DepositChargeCode);
			filters.AddTextFilter("Discount Charge Code", ClientLicencePriceItemSchema.L7_DiscountChargeCode);

			filters.AddFiltersForTranslatableText("Description", ClientLicencePriceItemSchema.L7_Description, typeof(ClientLicencePriceItem), ResString.GetMultilingualString("BillingPricesFilterBusinessObject|Description", "Description"));
			filters.AddTextFilter("Unit Break Parent", ClientLicencePriceItemSchema.L7_UnitBreakParentCode);
			filters.AddTextFilter("Language",
				(value) => value.IsEmpty ? new ZQuery() : new ZQuery(ClientLicencePriceItemSchema.L7_Language, value),
				() => new CodeDescriptionPairList(OLookUpEditType.Language)).Category = FilterCategories.StatusAndFlags;
			filters.AddFilter(new ModuleTextFilter("Disbursement Country", ClientLicencePriceItemSchema.L7_RN_NKDisbursementCountry, Country));
			filters.AddFilter(new ModuleTextFilter("Disbursement Direction", ClientLicencePriceItemSchema.L7_DisbursementDirection, JobConfigurationSelectorLookups.GetBaseDirectionList()));
		}

		#endregion

		#region Filter Collections

		RefCurrencyCollection currency;
		RefCurrencyCollection Currency => currency ?? (currency = new RefCurrencyCollection(Factory));

		RefCountryCollection country;
		RefCountryCollection Country => country ?? (country = new RefCountryCollection(Factory));

		#endregion

		void AddPriceHeaderFilters(ModuleFilterCollection filters)
		{
			PriceItemFilterSubGroup subGroup = new PriceItemFilterSubGroup();
			FilterCategory priceListCategory = new FilterCategory((NoResString)"Price List");

			#region Organisation Filters

			ModuleNkFilter headerFilter = new ModuleNkFilter("Header Created By", ClientLicencePriceHeaderSchema.L6_SystemCreateUser, ModuleIDs.GlbStaff, StaffList)
			{
				SubGroup = subGroup,
				Category = FilterCategories.Organisations
			};
			headerFilter.MultilingualDescription = ResString.GetMultilingualString("BillingPricesFilterBusinessObject|HeaderCreatedBy", "Header Created By");
			filters.AddCustomFilter(headerFilter);
			filters.AddGuidFilter("Organization", ModuleIDs.Organisation, GetOrganisationQuery, Organisations);

			#endregion

			#region Text Filters

			var versionFilter = new ModuleTextFilter("Version", ClientLicencePriceHeaderSchema.L6_PricelistVersion)
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(versionFilter);

			var editionFilter = new ModuleTextFilter("Edition", ClientLicencePriceHeaderSchema.L6_LicenceEdition)
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(editionFilter);

			var discountCodeFilter = new ModuleTextFilter("Discount Code", ClientLicencePriceHeaderSchema.L6_DiscountCode)
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(discountCodeFilter);

			var testDbCodeFilter = new ModuleTextFilter("Test Db Code", ClientLicencePriceHeaderSchema.L6_TestDbPriceCode)
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(testDbCodeFilter);

			#endregion

			#region Flags Filters

			var isStandardFilter = new ModuleFlagsFilter("Is Standard", new string[] { "Is Standard" }, new SchemaBoolColumn[] { ClientLicencePriceHeaderSchema.L6_IsStandard })
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(isStandardFilter);

			var stdDiscountFilter = new ModuleFlagsFilter("Use Std Discount", new string[] { "Use Std Discount" }, new SchemaBoolColumn[] { ClientLicencePriceHeaderSchema.L6_UseStandardDiscount })
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(stdDiscountFilter);

			#endregion

			#region Date Filters

			var validFromFilter = new ModuleDateFilter("Valid From", ClientLicencePriceHeaderSchema.L6_ValidFrom, true)
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(validFromFilter);

			var validToFilter = new ModuleDateFilter("Valid To", ClientLicencePriceHeaderSchema.L6_ValidTo, true)
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(validToFilter);

			var headerCreatedFilter = new ModuleDateFilter("Header Created", ClientLicencePriceHeaderSchema.L6_SystemCreateTimeUtc, true)
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(headerCreatedFilter);

			#endregion

			#region Number Range Filters

			var unitRateFilter = new ModuleNumberRangeFilter("Unit Rate", ClientLicencePriceHeaderSchema.L6_LicenceUnitRate)
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(unitRateFilter);

			#endregion

			#region Module / List Filters

			var currencyFilter = new ModuleNkFilter("Header Currency", ClientLicencePriceHeaderSchema.L6_RX_NKCurrency, ModuleIDs.RefCurrency, Currency)
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(currencyFilter);

			var systemFilter = new ModuleTextFilter("System", ClientLicencePriceHeaderSchema.L6_SystemCode, BillingConstants.PriceHeaderType.GetPriceHeaderTypeList())
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(systemFilter);

			var countryFilter = new ModuleTextFilter("Country", ClientLicencePriceHeaderSchema.L6_RN_NKCountry, Country)
			{
				SubGroup = subGroup,
				Category = priceListCategory
			};
			filters.AddFilter(countryFilter);

			var priceCurrencyFilter = new ModuleTextFilter("Price Currency", ClientLicencePriceItemSchema.L7_RX_NKCurrency, Currency)
			{
				Category = priceListCategory
			};
			filters.AddFilter(priceCurrencyFilter);

			#endregion

		}

		ZQuery GetOrganisationQuery(ZGuid value)
		{
			if (value.IsValid)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ClientLicencePriceItem));

				ZDBOnlySubQuery subQueryL7 = new ZDBOnlySubQuery(typeof(ClientLicencePriceHeader), ClientLicencePriceItemSchema.L7_L6);
				ZDBOnlySubQuery subQueryLC = new ZDBOnlySubQuery(typeof(LicenceCompany), ClientLicencePriceHeaderSchema.L6_LC);
				subQueryLC.AddToFilter(LicenceCompanySchema.LC_OH, value);

				subQueryL7.AddSubQuery(subQueryLC, JoinCondition.And);
				query.AddSubQuery(subQueryL7, JoinCondition.And);

				return query;
			}

			return null;
		}

		#region Collections
		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public GlbStaffCollection StaffList
		{
			get { return new GlbStaffCollection(Factory); }
		}

		#endregion

		#region Sub Group

		class PriceItemFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ClientLicencePriceItem));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ClientLicencePriceHeader), ClientLicencePriceItemSchema.L7_L6);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion

	}
}
