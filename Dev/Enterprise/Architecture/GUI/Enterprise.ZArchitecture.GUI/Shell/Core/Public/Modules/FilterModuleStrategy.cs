using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public abstract class FilterModuleStrategy : IFilterModuleStrategy
	{
		public const string UniqueSuffix = "_55a594ef-d39f-4d99-81ae-e575ad4b13c4";

		protected abstract IEnumerable<ModuleFilter> FiltersToAdd { get; }

		protected Type BizoType { get; set; }
		protected BusinessObjectFactory Factory { get; set; }

#if DEBUG
		protected ModuleFilterCollection AllFiltersForTest;
#endif

		public abstract void RunOnFilterControlInitialisation(IFilterControl control, IBusinessObjectCollection gridCollection);

		public void RunOnModuleFiltersCreated(IModuleFilterCollection filters, Type bizObjType, BusinessObjectFactory factory)
		{
			if (filters is ModuleFilterCollection filterCollection)
			{
				BizoType = bizObjType;
				Factory = factory;

#if DEBUG

				AllFiltersForTest = filterCollection;
#endif

				foreach (var filter in FiltersToAdd)
				{
					filter.MultilingualDescription = GetUniqueMultilingualDescription(filter, filterCollection);
					((IModuleFilterForStrategyInternal)filter).Description = filter.Description + UniqueSuffix;
					filterCollection.AddFilter(filter);
				}
			}
		}

		protected virtual MultilingualString GetUniqueMultilingualDescription(ModuleFilter filter, ModuleFilterCollection filters)
		{
			var result = filter.MultilingualDescription
				?? (NoResString)filter.Description;

			result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("bfe1786b-c529-465f-8da7-278d92f445c0", "(System)"));

			return result;
		}
	}
}
