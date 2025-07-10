using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class StlPriceListFilters
	{
		public StlPriceListFilters()
		{
			Factory = new BusinessObjectFactory();
			Factory.Saving += (x) => throw new InvalidOperationException("Attempt to save the temporary StlPriceListFilters factory.");
			StlFilterCategory = new FilterCategory((NoResString)"STL");
		}

		readonly BusinessObjectFactory Factory;
		readonly FilterCategory StlFilterCategory;

		public void AddPriceListFilters(ModuleFilterCollection filters, ModuleFilterSubGroup priceSubGroup, ModuleFilterSubGroup settingSubGroup)
		{
			AddPriceFilters(filters, priceSubGroup);
			AddSettingFilters(filters, settingSubGroup);
		}

		#region Price Filters

		void AddPriceFilters(ModuleFilterCollection filters, ModuleFilterSubGroup priceSubGroup)
		{
			var existingFilters = new HashSet<ModuleFilter>(filters);

			filters.AddDateFilter("STL Prices Valid From", EdiPriceHeaderLinkSchema.PHL_ValidFrom);
			filters.AddDateFilter("STL Prices Valid To", EdiPriceHeaderLinkSchema.PHL_ValidTo);

			var textFilter = filters.AddTextFilter("STL Prices Version", GetPricesVersionQuery, EdiPriceHeaderLinkLookups.GetPriceHeaderVersions(Factory));
			textFilter.MaxLength = ClientLicencePriceHeaderSchema.L6_PricelistVersion.MaxLength;
			filters.AddNkFilter("STL Prices Currency", EdiPriceHeaderLinkSchema.PHL_RX_NKCurrency, ModuleIDs.RefCurrency, new RefCurrencyCollection(Factory));

			filters.AddTextFilter("STL Prices Volume", (x) => new ZQuery(EdiPriceHeaderLinkSchema.PHL_VolumeCode, x), new EdiPriceHeaderLinkVolumeCodeList());
			filters.AddTextFilter("STL Prices Core Pack", (x) => new ZQuery(EdiPriceHeaderLinkSchema.PHL_CorePackCode, x), new EdiPriceHeaderLinkCorePackCodeList());

			filters.AddNkFilter("STL Prices Created By", EdiPriceHeaderLinkSchema.PHL_SystemCreateUser, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			filters.AddDateFilter("STL Prices Created Date", EdiPriceHeaderLinkSchema.PHL_SystemCreateTimeUtc, true);

			foreach (var filter in filters.Where(x => !existingFilters.Contains(x)).ToArray())
			{
				filter.Category = StlFilterCategory;
				filter.SubGroup = priceSubGroup;
			}
		}

		ZQuery GetPricesVersionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(ClientLicencePriceHeader), ClientLicencePriceHeaderSchema.PK);
			subQuery.AddToFilter(ClientLicencePriceHeaderSchema.L6_PricelistVersion, comparisonOperator, value);

			var query = new ZDBOnlyQuery(typeof(EdiPriceHeaderLink));
			query.AddSubQuery(EdiPriceHeaderLinkSchema.PHL_L6, subQuery, JoinCondition.And);

			return query;
		}

		#endregion Price Filters

		#region Setting Filters

		void AddSettingFilters(ModuleFilterCollection filters, ModuleFilterSubGroup settingSubGroup)
		{
			var existingFilters = new HashSet<ModuleFilter>(filters);
			var lookup = new EdiLicenceSettingLookups(Factory.New<EdiLicenceSetting>());

			filters.AddDateFilter("STL Setting Valid From", EdiLicenceSettingSchema.LS9_ValidFrom);
			filters.AddDateFilter("STL Setting Valid To", EdiLicenceSettingSchema.LS9_ValidTo);

			filters.AddTextFilter("STL Setting Discount Name", GetDiscountNameQuery, lookup.DiscountNames);
			filters.AddNumberRangeFilter("STL Setting Discount Percent", GetDiscountPercentQuery, EdiLicenceSettingSchema.LS9_Percent.Precision, EdiLicenceSettingSchema.LS9_Percent.Scale);

			var buyingGroups = CodesToList(Factory.Load<EdiLicenceSetting>(new ZQuery(EdiLicenceSettingSchema.LS9_Type, LicenceSetting.BuyingGroup)).Select(x => x.LS9_Name).Distinct().OrderBy(x => x));
			var buyingGroupFilter = filters.AddTextFilter("STL Setting Buying Group Name", GetBuyingGroupQuery, buyingGroups);
			buyingGroupFilter.MaxLength = EdiLicenceSettingSchema.LS9_Name.MaxLength;
			RemoveCodesExcept(buyingGroupFilter.ComparisonOperator_List, new[] { ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.IsNotBlank });

			var commitmentGroups = CodesToList(Factory.Load<EdiLicenceSetting>(new ZQuery(EdiLicenceSettingSchema.LS9_Type, LicenceSetting.Commitment)).Select(x => x.LS9_Name).Distinct().OrderBy(x => x));
			var commitmentGroupFilter = filters.AddTextFilter("STL Setting Commitment Group Name", GetCommitmentGroupQuery, commitmentGroups);
			commitmentGroupFilter.MaxLength = EdiLicenceSettingSchema.LS9_Name.MaxLength;
			RemoveCodesExcept(commitmentGroupFilter.ComparisonOperator_List, new[] { ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.IsNotBlank });

			filters.AddNumberRangeFilter("STL Setting Commitment Units", GetCommitmentUnitsQuery);

			filters.AddTextFilter("STL Setting Price Code", GetPriceCodeQuery, () => GetPriceCodeList());
			filters.AddNumberRangeFilter("STL Setting Price", GetPriceQuery);

			filters.AddTextFilter("STL Setting Comment", EdiLicenceSettingSchema.LS9_Comment);

			filters.AddTextFilter("STL Setting High Volume Feature", GetHighVolumeFeaturePriceCodeQuery, () => GetPriceCodeList());

			filters.AddTextFilter("STL Setting Min. Spend Feature", GetMinSpendPriceCodeQuery, () => GetPriceCodeList());
			filters.AddNumberRangeFilter("STL Setting Min. Spend", GetMinSpendQuery);

			foreach (var filter in filters.Where(x => !existingFilters.Contains(x)).ToArray())
			{
				filter.Category = StlFilterCategory;
				filter.SubGroup = settingSubGroup;
			}
		}

		ZQuery GetDiscountNameQuery(ZString value)
		{
			return new ZQuery(EdiLicenceSettingSchema.LS9_Type, LicenceSetting.Discount).AddToFilter(EdiLicenceSettingSchema.LS9_Name, value);
		}

		ZQuery GetDiscountPercentQuery(INumericZType value1, INumericZType value2)
		{
			return new ZQuery(EdiLicenceSettingSchema.LS9_Type, LicenceSetting.Discount)
				.AddToFilter(EdiLicenceSettingSchema.LS9_Percent, SQLComparisonOperator.GreaterThanOrEqualTo, value1)
				.AddToFilter(EdiLicenceSettingSchema.LS9_Percent, SQLComparisonOperator.LessThanOrEqualTo, value2);
		}

		ZQuery GetBuyingGroupQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(EdiLicenceSettingSchema.LS9_Type, LicenceSetting.BuyingGroup).AddToFilter(EdiLicenceSettingSchema.LS9_Name, comparisonOperator, value);
		}

		ZQuery GetCommitmentGroupQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(EdiLicenceSettingSchema.LS9_Type, LicenceSetting.Commitment).AddToFilter(EdiLicenceSettingSchema.LS9_Name, comparisonOperator, value);
		}

		ZQuery GetCommitmentUnitsQuery(INumericZType value1, INumericZType value2)
		{
			return new ZQuery(EdiLicenceSettingSchema.LS9_Type, LicenceSetting.Commitment)
				.AddToFilter(EdiLicenceSettingSchema.LS9_Price, SQLComparisonOperator.GreaterThanOrEqualTo, value1)
				.AddToFilter(EdiLicenceSettingSchema.LS9_Price, SQLComparisonOperator.LessThanOrEqualTo, value2);
		}

		ZQuery GetPriceCodeQuery(ZString value)
		{
			return new ZQuery(EdiLicenceSettingSchema.LS9_Type, LicenceSetting.Price)
				.AddToFilter(EdiLicenceSettingSchema.LS9_Name, SQLComparisonOperator.EndsWith, value);
		}

		ZQuery GetHighVolumeFeaturePriceCodeQuery(ZString value)
		{
			return new ZQuery(EdiLicenceSettingSchema.LS9_Type, LicenceSetting.HighVolumeFeature)
				.AddToFilter(EdiLicenceSettingSchema.LS9_Name, SQLComparisonOperator.EndsWith, value);
		}

		ZQuery GetMinSpendPriceCodeQuery(ZString value)
		{
			return new ZQuery(EdiLicenceSettingSchema.LS9_Type, LicenceSetting.MinSpend)
				.AddToFilter(EdiLicenceSettingSchema.LS9_Name, SQLComparisonOperator.EndsWith, value);
		}

		ZQuery GetPriceQuery(INumericZType value1, INumericZType value2)
		{
			return new ZQuery(EdiLicenceSettingSchema.LS9_Type, LicenceSetting.Price)
				.AddToFilter(EdiLicenceSettingSchema.LS9_Price, SQLComparisonOperator.GreaterThanOrEqualTo, value1)
				.AddToFilter(EdiLicenceSettingSchema.LS9_Price, SQLComparisonOperator.LessThanOrEqualTo, value2);
		}

		ZQuery GetMinSpendQuery(INumericZType value1, INumericZType value2)
		{
			return new ZQuery(EdiLicenceSettingSchema.LS9_Type, LicenceSetting.MinSpend)
				.AddToFilter(EdiLicenceSettingSchema.LS9_Price, SQLComparisonOperator.GreaterThanOrEqualTo, value1)
				.AddToFilter(EdiLicenceSettingSchema.LS9_Price, SQLComparisonOperator.LessThanOrEqualTo, value2);
		}

		CodeDescriptionPairList GetPriceCodeList()
		{
			var dynamicCollection = new DynamicBusinessObjectCollection(Factory);
			dynamicCollection.Load(
@"SELECT L7_Code, MAX(LTRIM(L7_Description)) L7_Description
FROM dbo.ClientLicencePriceItem
JOIN dbo.ClientLicencePriceHeader ON L7_L6 = L6_PK
WHERE L6_PricelistVersion LIKE 'STL%' AND L7_Code != ''
GROUP BY L7_Code ORDER BY L7_Code;");

			var list = new CodeDescriptionPairList();
			foreach (DynamicBusinessObject code in dynamicCollection)
			{
				list.AddPair(code[ClientLicencePriceItemSchema.L7_Code].ToString(), code[ClientLicencePriceItemSchema.L7_Description].ToString());
			}

			return list;
		}

		#endregion Setting Filters

		void RemoveCodesExcept(CodeDescriptionPairList list, IEnumerable<string> codes)
		{
			foreach (var code in list.GetAllCodes().Except(codes))
			{
				list.RemoveCode(code);
			}
		}

		CodeDescriptionPairList CodesToList(IEnumerable<ZString> codes)
		{
			var list = new CodeDescriptionPairList();
			foreach (var code in codes)
			{
				list.AddPair(code);
			}

			return list;
		}
	}
}
