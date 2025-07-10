using System.Collections.Generic;
using IGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Customs.Forwarding.GUI
{
	public class ForwardingShipmentGridCustomColumnsProvider : Integration.Customs.IForwardingShipmentGridCustomColumnsProvider
	{
		public void AddColumns(IGridControl control)
		{
			foreach (var provider in ColumnsProviders)
			{
				provider.AddColumns(control);
			}

			var grid = control.GetGridControl();

			if (grid != null)
			{
				grid.LoadUserLayoutSettings();
			}
		}

		IEnumerable<IGridCustomColumnsProvider> ColumnsProviders
		{
			get { return columnsProviders ?? (columnsProviders = GetColumnsProviders()); }
		}
		IEnumerable<IGridCustomColumnsProvider> columnsProviders;

		IEnumerable<IGridCustomColumnsProvider> GetColumnsProviders()
		{
			yield return new ForwardingShipmentColumnsProvider();
		}
	}
}
