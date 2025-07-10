using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IModuleFilterWithSelectedFilters : IModuleFilterWithModuleID
	{
		bool SupportsFiltersMatchComparisonOperator { get; }
		ZString ComparisonOperator { get; set; }
		ZString Description { get; }
		ZQuery Query { get; }
		bool HasComparisonOperator { get; }
		SchemaColumn FilterColumn { get; }
		bool IsActive { get; set; }
		FilterStripBusinessObject SelectedFilters { get; }
		int SelectedFilterCount { get; }
		ZString SelectedFiltersDescription { get; }
		IReadOnlyList<string> AllowedComparisonOperators { get; }
		bool IsComparisonOperatorAutomaticallySelected { get; }
		MultilingualString MultilingualDescription { get; set; }
		bool HasValidModuleInCurrentContext { get; }

		bool IsFilterCollectionComparisonOperatorSelected();
		event EventHandler ComparisonOperatorChanged;
		void UpdateSelectedFilters(FilterStripBusinessObject newSelectedFilters);
		ZQuery GetSubFilterQueryIncludingCollectionFilters();
		IDisposable SuspendSelectedFiltersChangedFiring();
		event EventHandler SelectedFiltersChanged;
	}
}
