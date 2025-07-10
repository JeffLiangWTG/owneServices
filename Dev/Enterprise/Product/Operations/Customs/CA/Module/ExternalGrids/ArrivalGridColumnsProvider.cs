using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using IGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Customs.CA.Module
{
	partial class CAShipmentGridColumnsProvider
	{
		[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
		class ArrivalGridColumnsProvider : Integration.Customs.CA.IShipmentGridColumnsProvider
		{
			readonly ArrivalColumnsHelper helper = new ArrivalColumnsHelper();

			public void AddColumns(IGridControl control)
			{
				if (control is ZFilterStripControl)
				{
					var filterStripControl = (ZFilterStripControl)control;
					helper.AddColumns(filterStripControl.FilteredGrid, filterStripControl.GridCollection);
				}
				else
				{
					ZGrid grid = GetGridControl(control);

					if (grid != null)
					{
						helper.AddColumns(grid, (IBusinessObjectCollection)grid.List);
					}
				}
			}
		}
	}
}
