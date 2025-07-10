using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public sealed class DummyModuleFilterCollection : ModuleFilterCollection
	{
		protected override ZQuery GetFilterQueryCore(IEnumerable<ModuleFilter> activeModuleFiltersForQuery, FilterGroupQueryMapAction forGroups, IEnumerable<BusinessObject> bizosToApplyFiltersTo)
		{
			GetFilterQueryCoreCount++;
			return base.GetFilterQueryCore(activeModuleFiltersForQuery, forGroups, bizosToApplyFiltersTo);
		}

		public int GetFilterQueryCoreCount { get; private set; }

		public ZQuery CachedQuery_ForTest
		{
			get => CachedQuery;
			set => CachedQuery = value;
		}
	}
}
