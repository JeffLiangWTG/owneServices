using System.Collections.Generic;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using IExternalGridColumnsProvider = Enterprise.Integration.Customs.CA.IExternalGridColumnsProvider;
using IGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Customs.CA.Module
{
	public abstract class CAExternalGridColumnsProvider : IExternalGridColumnsProvider
	{
		#region Implementation of IExternalGridColumnsProvider

		public void AddColumns(IGridControl control)
		{
			foreach (var provider in ColumnsProviders)
			{
				provider.AddColumns(control);
			}

			ZGrid grid = GetGridControl(control);
			if (grid != null)
			{
				grid.LoadUserLayoutSettings();
			}
		}

		IEnumerable<IExternalGridColumnsProvider> ColumnsProviders
		{
			get { return columnsProviders ?? (columnsProviders = GetColumnsProviders()); }
		}

		IEnumerable<IExternalGridColumnsProvider> columnsProviders;

		protected abstract IEnumerable<IExternalGridColumnsProvider> GetColumnsProviders();

		#endregion

		public static ZGrid GetGridControl(IGridControl control)
		{
			ZGrid grid = null;

			if (control is ZGrid)
			{
				grid = (ZGrid)control;
			}
			if (control is ZModuleButtonGrid)
			{
				grid = ((ZModuleButtonGrid)control).InnerGrid;
			}

			return grid;
		}
	}
}
