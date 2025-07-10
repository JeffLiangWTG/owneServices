using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using IGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Customs.Forwarding.GUI
{
	public static class IGridControlExtensions
	{
		public static ZGrid GetGridControl(this IGridControl control)
		{
			var grid = control as ZGrid;

			if (grid != null)
			{
				return grid;
			}

			var moduleButtonGrid = control as ZModuleButtonGrid;

			if (moduleButtonGrid != null)
			{
				grid = moduleButtonGrid.InnerGrid;
			}

			return grid;
		}
	}
}
