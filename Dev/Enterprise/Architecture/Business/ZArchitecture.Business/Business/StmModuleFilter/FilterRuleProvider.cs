using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public class FilterRuleProvider
	{
		public ModuleIdentifier ModuleId { get; }
		readonly IRelatedModuleFilterSupportable parent;
		readonly string filterName;

		protected StmModuleFilter CachedFilter { get; private set; }

		public bool ReloadExistingRowsOnFilterLoad { get; set; }

		public FilterRuleProvider(ModuleIdentifier moduleId, IRelatedModuleFilterSupportable parent, string filterName = null)
		{
			this.ModuleId = moduleId;
			this.parent = parent;
			this.filterName = filterName;
		}

		public ZQuery GetLoadQuery()
		{
			return RelatedModuleFiltersHelper.GetLoadQuery(parent, filterName, ReloadExistingRowsOnFilterLoad);
		}

		public StmModuleFilter GetOrCreateAndCacheFilter()
		{
			CacheFilter();

			return CachedFilter;
		}

		protected virtual void CacheFilter()
		{
			CachedFilter = RelatedModuleFiltersHelper.GetOrCreateFilter(parent, filterName, CachedFilter, ModuleId, ReloadExistingRowsOnFilterLoad);
		}

		public bool IsFilterCached => CachedFilter != null;

		public void DeleteFilter()
		{
			var filter = CachedFilter ?? RelatedModuleFiltersHelper.LoadFilter(parent, filterName);

			filter?.DeleteWithUserData();
		}

		public bool HasFilterStrips => IsFilterCached && RelatedModuleFiltersHelper.HasFilterStrips(CachedFilter, ModuleId);

		public void CopyFilterStrips(Func<StmModuleFilter> toFilterGetter)
		{
			if (parent.IsInDatabase || IsFilterCached)
			{
				RelatedModuleFiltersHelper.CopyFilterStrips(GetOrCreateAndCacheFilter(), toFilterGetter());
			}
		}
	}
}
