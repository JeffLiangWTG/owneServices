using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business;

public abstract class InflatableFilterStripBusinessObject : FilterStripBusinessObject
{
	protected InflatableFilterStripBusinessObject(string nameForDebugging) : base(nameForDebugging)
	{
	}

	protected InflatableFilterStripBusinessObject()
	{
	}

	protected InflatableFilterStripBusinessObject(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected sealed override ModuleFilterCollection GetModuleFiltersCore() => InflateFilters();

	protected abstract List<IFilterInflator> GetFilterInflators();

	ModuleFilterCollection InflateFilters()
	{
		var filterCollection = new ModuleFilterCollection();
		var filterInflators = GetFilterInflators();
		filterInflators?.ForEach(inflator => inflator?.InflateFilter(filterCollection));
		return filterCollection;
	}
}
