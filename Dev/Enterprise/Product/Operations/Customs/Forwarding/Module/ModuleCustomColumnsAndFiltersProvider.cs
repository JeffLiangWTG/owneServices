using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Forwarding.GUI;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using IGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Customs.Forwarding.Module
{
	public abstract class ModuleCustomColumnsAndFiltersProvider
	{
		#region Add Filters

		public void AddFilters(IModuleFilterCollection filterCollection, BusinessObjectFactory factory, IBusiness filterBusinessObject = null)
		{
			var filterProviders = GetFiltersProviders(factory, (FilterStripBusinessObject)filterBusinessObject);
			foreach (var provider in filterProviders)
			{
				provider.AddFilters(filterCollection);
			}
		}

		protected abstract IEnumerable<IModuleCustomFiltersProvider> GetFiltersProviders(BusinessObjectFactory factory, FilterStripBusinessObject filterBusinessObject = null);

		#endregion

		#region Add Columns

		public void AddColumns(IGridControl control)
		{
			var columnsProviders = GetColumnsProviders();
			foreach (var provider in columnsProviders)
			{
				provider.AddColumns(control);
			}

			var grid = control.GetGridControl();
			grid?.LoadUserLayoutSettings();
		}

		protected abstract IEnumerable<IGridCustomColumnsProvider> GetColumnsProviders();

		#endregion
	}
}
