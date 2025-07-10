using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public static class SupportingInfoUserControlHelper
	{
		public static void ChangeParentAndSetGridDetails(Control userControl, string parent, string columnLayoutContext, params ColumnWidth[] columnWidths)
		{
			if (!(userControl is ISupportingInfoUserControls supportingInfoUserControl))
			{
				throw new InvalidOperationException("ChangeParentAndSetGridDetails requires that userControl implement ISupportingInfoUserControls");
			}

			new ControlRebinder().Rebind(userControl, supportingInfoUserControl.GridBindingMember, parent);
			var grid = supportingInfoUserControl.Grid;
			if (grid != null)
			{
				grid.ColumnLayoutContext = columnLayoutContext;
				if (columnWidths != null)
				{
					using (grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
					{
						foreach (var columnWidth in columnWidths)
						{
							if (grid.GetColumnStyle(columnWidth.Name) != null)
							{
								grid.SetColumnWidth(columnWidth.Name, columnWidth.Width);
							}
						}
					}
				}
			}
		}
	}
}
