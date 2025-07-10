using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	class TagRuleFilterRuleProvider : BMFilterRuleProvider
	{
		readonly TagRule tagRule;

		internal TagRuleFilterRuleProvider(TagRule tagRule)
			: base(tagRule)
		{
			this.tagRule = tagRule;
		}

		protected override void CacheFilter()
		{
			var oldPk = CachedFilter?.PK ?? ZGuid.Empty;
			base.CacheFilter();

			if (oldPk != CachedFilter.PK)
			{
				CachedFilter.ReadOnly = tagRule.TGR_IsSystem;
				if (!CachedFilter.S9_IsIndexSearch)
				{
					CachedFilter.ModuleFilterComparisonStrategy = new TagRuleModuleFilterComparisonStrategy();
				}
			}
		}
	}
}
