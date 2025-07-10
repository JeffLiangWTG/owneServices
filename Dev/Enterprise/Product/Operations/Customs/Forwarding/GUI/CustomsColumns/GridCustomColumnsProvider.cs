using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using IntegrationIGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Customs.Forwarding.GUI
{
	public abstract class GridCustomColumnsProvider : IGridCustomColumnsProvider
	{
		public void AddColumns(IntegrationIGridControl control)
		{
			var filterStripControl = control as ZFilterStripControl;

			if (filterStripControl != null)
			{
				AddColumns(filterStripControl.FilteredGrid, filterStripControl.GridCollection);
			}
			else
			{
				var grid = control.GetGridControl();

				if (grid != null)
				{
					AddColumns(grid, (IBusinessObjectCollection)grid.List);
				}
			}
		}

		void AddColumns(ZGrid filteredGrid, IBusinessObjectCollection gridCollection)
		{
			if (gridCollection != null)
			{
				var propertyContainer = GetCustomPropertyContainer();
				var customColumnsInitializer = GetCustomColumnsInitializer(filteredGrid, gridCollection, propertyContainer);
				customColumnsInitializer.AddCustomColumns(propertyContainer.CustomProperties);

				SetFetchForView(gridCollection);
			}
		}

		protected virtual ResourceStringData GroupName => ResourceStringData.Empty;

		protected abstract CustomPropertyContainer GetCustomPropertyContainer();
		protected abstract ZGridCustomColumnsInitializer GetCustomColumnsInitializer(ZGrid grid, IBusinessObjectCollection collection, ICustomPropertyContainer propertyContainer);
		protected abstract void AddFetchHintForView(BusinessObject[] businessObjects, TableColumn[] columns);

		void SetFetchForView(IBusinessObjectCollection collection)
		{
			if (collection != null && collection.FetchStrategy != null)
			{
				collection.FetchStrategy.AdditionalFetchForView +=
					(sender, eventArgs) =>
					{
						AddFetchHintForView(eventArgs.BusinessObjects, eventArgs.TableColumns);
					};
			}
		}
	}
}
