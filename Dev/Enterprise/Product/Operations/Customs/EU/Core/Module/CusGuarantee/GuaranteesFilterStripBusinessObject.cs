using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Module
{
	public class GuaranteesFilterStripBusinessObject : Customs.Module.GuaranteesFilterStripBusinessObject
	{
		public new class FilterConstants : Customs.Module.GuaranteesFilterStripBusinessObject.FilterConstants
		{
			public const string GuaranteeRule = CusGuaranteeHeaderCollectionFiltered.FilterConstants.GuaranteeRule;
		}

		public new GuaranteesFilterLookups Lookups => (GuaranteesFilterLookups)base.Lookups;

		protected override Customs.Module.GuaranteesFilterLookups GetNewLookups() => new GuaranteesFilterLookups(this);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddGuaranteeRuleFilter(filters);
			return filters;
		}

		void AddGuaranteeRuleFilter(ModuleFilterCollection filters)
		{
			var ruleDetailsFilter = new CusAuthorisationsRuleModuleFilter(FilterConstants.GuaranteeRule, GetRuleDetailsQuery, Lookups.GuaranteeRuleCodeList)
			{
				Category = FilterCategories.ModesAndTypes,
				MultilingualDescription = ResString.GetMultilingualString("Customs|GuaranteesFilter|GuaranteeRule", FilterConstants.GuaranteeRule),
				ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact,
				Visibility = FilterVisibility.AlwaysVisible,
			};
			filters.AddFilter(ruleDetailsFilter);
		}

		ZDBOnlyQuery GetRuleDetailsQuery(ZString ruleCode, SQLComparisonOperator comparisonOperator, ZString ruleValue)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusGuaranteeRule), CusPermitRuleSchema.CPR_CPH_PermitHeader);
			if (!ruleCode.IsEmpty)
			{
				_ = subQuery.AddToFilter(CusPermitRuleSchema.CPR_RuleCode, ruleCode);
			}
			if (!ruleValue.IsEmpty)
			{
				_ = subQuery.AddToFilter(CusPermitRuleSchema.CPR_ValueFrom, comparisonOperator, ruleValue);
			}

			var result = new ZDBOnlyQuery(typeof(BaseCusGuaranteeHeader));
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}
	}
}
