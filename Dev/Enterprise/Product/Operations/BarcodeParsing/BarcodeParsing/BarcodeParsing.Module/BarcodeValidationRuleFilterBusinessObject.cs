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
	public class BarcodeValidationRuleFilterBusinessObject : FilterStripBusinessObject
	{
		#region FilterNames

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Filter names")]
		public static class FilterNames
		{
			public const string Buyer = "Buyer";
			public const string OptionalBuyer = "OptionalBuyer";
			public const string Module = "Module";
			public const string Supplier = "Supplier";
			public const string Format = "Format";
			public const string TargetField = "TargetField";
		}

		#endregion

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddOrgFilters(filters);
			AddTextFilters(filters);

			return filters;
		}

		#region AddOrgFilters

		void AddOrgFilters(ModuleFilterCollection filters)
		{
			var buyerFilter = filters.AddGuidFilter(FilterNames.Buyer, ModuleIDs.Organisation, BarcodeRuleSetSchema.BRS_OH_Buyer, new OrgHeaderCollection(Factory));
			buyerFilter.SubGroup = BarcodeValidationRuleSubGroup;
			buyerFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeValidationRuleFilterBusinessObject|Buyer", "Buyer");

			var optionalBuyerFilter = filters.AddGuidFilter(FilterNames.OptionalBuyer, ModuleIDs.Organisation, GetOptionalBuyerQuery, new OrgHeaderCollection(Factory));
			optionalBuyerFilter.SubGroup = BarcodeValidationRuleSubGroup;
			optionalBuyerFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeValidationRuleFilterBusinessObject|OptionalBuyer", "Optional Buyer");

			var supplierFilter = filters.AddGuidFilter(FilterNames.Supplier, ModuleIDs.Organisation, BarcodeRuleSetSchema.BRS_OH_Supplier, new OrgHeaderCollection(Factory));
			supplierFilter.SubGroup = BarcodeValidationRuleSubGroup;
			supplierFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeValidationRuleFilterBusinessObject|Supplier", "Supplier");
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
			var selectedModuleFilter = filters.AddTextFilter(FilterNames.Module, BarcodeRuleSetSchema.BRS_Module, ModuleList);
			selectedModuleFilter.SubGroup = barcodeValidationRuleSubGroup;
			selectedModuleFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeValidationRuleFilterBusinessObject|Module", "Module");

			var formatFilter = filters.AddTextFilter(FilterNames.Format, BarcodeValidationRuleSchema.BVR_Format, FormatList);
			formatFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeValidationRuleFilterBusinessObject|Format", "Format");

			var targetFieldFilter = filters.AddTextFilter(FilterNames.TargetField, BarcodeValidationRuleSchema.BVR_TargetField, TargetFieldList);
			targetFieldFilter.PropertyValidation = TargetFieldValidation;
			targetFieldFilter.MultilingualDescription = ResString.GetMultilingualString("BarcodeValidationRuleFilterBusinessObject|TargetField", "Target Field");
		}

		void TargetFieldValidation(ZPropertyInfo info)
		{
			if (!TargetFieldModuleFilter.Property.IsEmpty && string.IsNullOrEmpty(SelectedModule))
			{
				info.AddError(Res.GetString("99bcb63c-69c1-4782-b029-060e4a2c6a7e", "Target Field cannot be entered without a module."));
			}
		}

		ZString SelectedModule => SelectedModuleFilter.IsActive ? SelectedModuleFilter.Property : ZString.Empty;
		ModuleTextFilter SelectedModuleFilter => (ModuleTextFilter)ModuleFilters[FilterNames.Module];
		ModuleTextFilter TargetFieldModuleFilter => (ModuleTextFilter)ModuleFilters[FilterNames.TargetField];

		#region ModuleList

		CodeDescriptionPairList ModuleList => BarcodeParsingLookupHelper.ModuleTypes(Factory);

		#endregion

		#region FormatList

		CodeDescriptionPairList FormatList => Factory.GetCachedValue("BarcodeValidationRuleFilterBusinessObject|FormatList",
			() => new CodeDescriptionPairList(BarcodeFormatHelper.GetFormatTypes(false)));

		#endregion

		#region TargetFieldList

		CodeDescriptionPairList TargetFieldList() => Factory.GetCachedValue($"BarcodeValidationRuleFilterBusinessObject|TargetFieldList|{SelectedModule}",
			() => string.IsNullOrEmpty(SelectedModule)
				? new CodeDescriptionPairList()
				: new CodeDescriptionPairList(Factory.GetBarcodeParsingConsumerFromModuleCode(SelectedModule).TargetFields));

		#endregion

		#region BarcodeValidationRuleSubGroup

		ModuleFilterSubGroup BarcodeValidationRuleSubGroup => barcodeValidationRuleSubGroup ?? (barcodeValidationRuleSubGroup = new BarcodeValidationRuleFilterSubGroup());

		ModuleFilterSubGroup barcodeValidationRuleSubGroup;

		class BarcodeValidationRuleFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var ruleQuery = new ZDBOnlyQuery(typeof(BarcodeValidationRule));
				var subQuery = new ZDBOnlySubQuery(typeof(BarcodeRuleSet), BarcodeValidationRuleSchema.BVR_BRS_RuleSet);
				subQuery.AddToFilter(filter);
				ruleQuery.AddSubQuery(subQuery, JoinCondition.And);
				return ruleQuery;
			}
		}

		#endregion

		#endregion

		#endregion
	}
}
