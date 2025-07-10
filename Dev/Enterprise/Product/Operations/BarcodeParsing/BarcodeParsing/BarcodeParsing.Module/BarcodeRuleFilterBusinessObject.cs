using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Module
{
	public class BarcodeRuleFilterBusinessObject : FilterStripBusinessObject
	{
		#region FilterNames

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Filter name")]
		public static class FilterNames
		{
			public const string Buyer = "Buyer";
			public const string OptionalBuyer = "OptionalBuyer";
			public const string IsPartial = "Is Partial Rule";
			public const string IsSystem = "Is System";
			public const string Module = "Module";
			public const string RuleName = "Rule Name";
			public const string Supplier = "Supplier";
			public const string TerminatorType = "Terminator Type";
		}
		#endregion

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddFlagFilters(filters);
			AddOrgFilters(filters);
			AddTextFilters(filters);

			return filters;
		}

		#region AddFlagFilters

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var partialFilter = filters.AddTextFilter(FilterNames.IsPartial, GetIsPartialFilter, new IsPartialStatuses());
			partialFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeRuleFilterBusinessObject|IsPartial", "Is Partial Rule");

			var systemFilter = filters.AddTextFilter(FilterNames.IsSystem, GetIsSystemFilter, new IsSystemStatuses());
			systemFilter.SubGroup = BarcodeRuleSubGroup;
			systemFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeRuleFilterBusinessObject|IsSystem", "Is System");
		}

		#region GetIsPartialFilter

		ZQuery GetIsPartialFilter(ZString isPartialStatus)
		{
			var result = new ZQuery();
			if (isPartialStatus.EqualsIgnoringCase(IsPartialStatuses.Codes.All))
			{
				result = new ZQuery();
			}
			else
			{
				var comparisonOperator = isPartialStatus.EqualsIgnoringCase(IsPartialStatuses.Codes.FullRules)
					? SQLComparisonOperator.GreaterThan
					: SQLComparisonOperator.Equal;

				var subQuery = new ZDBOnlySubQuery(typeof(BarcodeRuleComponent), BarcodeRuleComponentSchema.BRC_BRU_Rule);
				subQuery.AddToFilter(BarcodeRuleComponentSchema.BRC_Sequence, comparisonOperator, (short)0);

				var query = new ZDBOnlyQuery(typeof(BarcodeRule));
				query.AddSubQuery(subQuery, JoinCondition.And);

				result = query;
			}
			return result;
		}

		#endregion

		#region GetIsSystemFilter

		ZQuery GetIsSystemFilter(ZString isSystemStatus)
		{
			ZQuery result;

			if (isSystemStatus.EqualsIgnoringCase(IsSystemStatuses.Codes.All))
			{
				result = new ZQuery();
			}
			else
			{
				var comparisonOperator = isSystemStatus.EqualsIgnoringCase(IsSystemStatuses.Codes.SystemRules)
					? SQLComparisonOperator.Equal
					: SQLComparisonOperator.NotEqual;

				var query = new ZDBOnlyQuery(typeof(BarcodeRuleSet));
				query.AddToFilter(BarcodeRuleSetSchema.BRS_IsSystem, comparisonOperator, ZBool.True);

				result = query;
			}

			return result;
		}

		#endregion

		#endregion

		#region AddOrgFilters

		void AddOrgFilters(ModuleFilterCollection filters)
		{
			var buyerFilter = filters.AddGuidFilter(FilterNames.Buyer, ModuleIDs.Organisation, BarcodeRuleSetSchema.BRS_OH_Buyer, new OrgHeaderCollection(Factory));
			buyerFilter.SubGroup = BarcodeRuleSubGroup;
			buyerFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeRuleFilterBusinessObject|Buyer", "Buyer");

			// This for LinkLabel from Product form. There was no way to setup 
			// statement (buyer = X OR buyer is NULL) using FilterBusinessObjectDefault
			var optionalBuyerFilter = filters.AddGuidFilter(FilterNames.OptionalBuyer, ModuleIDs.Organisation, GetOptionalBuyerQuery, new OrgHeaderCollection(Factory));
			optionalBuyerFilter.SubGroup = BarcodeRuleSubGroup;
			optionalBuyerFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeRuleFilterBusinessObject|OptionalBuyer", "Optional Buyer");

			var supplierFilter = filters.AddGuidFilter(FilterNames.Supplier, ModuleIDs.Organisation, BarcodeRuleSetSchema.BRS_OH_Supplier, new OrgHeaderCollection(Factory));
			supplierFilter.SubGroup = BarcodeRuleSubGroup;
			supplierFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeRuleFilterBusinessObject|Supplier", "Supplier");
		}

		ZQuery GetOptionalBuyerQuery(ZGuid buyerPK)
		{
			var query = new ZQuery();
			query.AddToFilter(BarcodeRuleSetSchema.BRS_OH_Buyer, buyerPK);
			query.AddToFilter(JoinCondition.Or, BarcodeRuleSetSchema.BRS_OH_Buyer, null);
			return query;
		}

		#endregion

		#region AddTextFilters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var moduleFilter = filters.AddTextFilter(FilterNames.Module, BarcodeRuleSetSchema.BRS_Module, BarcodeModuleList);
			moduleFilter.SubGroup = BarcodeRuleSubGroup;
			moduleFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeRuleFilterBusinessObject|Module", "Module");

			var nameFilter = filters.AddTextFilter(FilterNames.RuleName, BarcodeRuleSchema.BRU_Name);
			nameFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeRuleFilterBusinessObject|RuleName", "Rule Name");

			var typeFilter = filters.AddTextFilter(FilterNames.TerminatorType, GetTerminatorTypeFilter, TerminatorTypeList);
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeRuleFilterBusinessObject|TerminatorType", "Terminator Type");
		}

		#region GetTerminatorFilter

		ZQuery GetTerminatorTypeFilter(SQLComparisonOperator comparisonOperator, ZString terminator)
		{
			var query = new ZDBOnlyQuery(typeof(BarcodeRule));
			if (terminator == TerminatorTypes.Codes.GS1)
			{
				query.AddToFilter(BarcodeRuleSchema.BRU_Terminator, SQLComparisonOperator.Equal, BarcodeRule.GS1Terminator);
			}
			else
			{
				query.AddToFilter(BarcodeRuleSchema.BRU_Terminator, SQLComparisonOperator.NotEqual, BarcodeRule.GS1Terminator);
			}

			return query;
		}

		#endregion

		#endregion

		#region Lists

		#region BarcodeModuleList

		CodeDescriptionPairList BarcodeModuleList => BarcodeParsingLookupHelper.ModuleTypes(Factory);

		#endregion

		#region TerminatorTypeList

		CodeDescriptionPairList TerminatorTypeList
		{
			get { return Factory.GetCachedValue("BarcodeRuleFilterBusinessObject|TerminatorTypeList", () => new TerminatorTypes()); }
		}

		#endregion

		#endregion

		#region BarcodeRuleSubGroup

		ModuleFilterSubGroup BarcodeRuleSubGroup
		{
			get { return barcodeRuleSubGroup ?? (barcodeRuleSubGroup = new BarcodeRuleFilterSubGroup()); }
		}
		ModuleFilterSubGroup barcodeRuleSubGroup;

		class BarcodeRuleFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var ruleQuery = new ZDBOnlyQuery(typeof(BarcodeRule));
				var subQuery = new ZDBOnlySubQuery(typeof(BarcodeRuleSet), BarcodeRuleSchema.BRU_BRS_RuleSet);
				subQuery.AddToFilter(filter);
				ruleQuery.AddSubQuery(subQuery, JoinCondition.And);
				return ruleQuery;
			}
		}

		#endregion

		#endregion
	}
}
