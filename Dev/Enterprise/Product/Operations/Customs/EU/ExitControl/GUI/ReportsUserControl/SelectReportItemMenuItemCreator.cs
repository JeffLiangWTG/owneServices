using System;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public class SelectReportItemMenuItemCreator
	{
		public SelectReportItemMenuItemCreator(IReportsGridUserControlProvider provider)
		{
			this.provider = provider;
		}
		readonly IReportsGridUserControlProvider provider;

		public const string SelectEditReportItemsMenuItemName = "SelectEditReportItemsMenuItem";
		public ZMenuItem Create() => new ZMenuItem(ResString.GetMultilingualString("17516B62-2EFC-4391-A44A-CB38DD2F01BD", "&Select/Edit Report Items"), SelectReportItemMemuItem_OnClick) { Name = SelectEditReportItemsMenuItemName };

		void SelectReportItemMemuItem_OnClick(object sender, EventArgs e)
		{
			var reportsGrid = provider?.UserControl is IReportsGridUserControl userControl ? userControl.ReportsGrid : null;
			var selectedRowCount = reportsGrid?.SelectedRowCount ?? 0;
			if (selectedRowCount == 0)
			{
				Globals.Message.ShowError(Res.GetString("595BB329-4C50-49DB-99A4-2533418188F3", "Please select an Exit Report first."));
			}
			else if (selectedRowCount == 1)
			{
				if (reportsGrid.GetCurrent() is CusExitReport report)
				{
					var consignment = report.Consignment;
					if (consignment == null)
					{
						Globals.Message.ShowError(Res.GetString("2C698036-B5E0-44CC-B076-A1DDF34D8423", "'Entry/Consignment' must have a valid value before selecting Report Items."));
					}
					else
					{
						ExitReportCreationSelectionFormManager.ShowExitReportCreationSelectionForm(consignment, report);
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("0CE18639-C85D-4E5B-A036-1B0E2482780A", "Multi selection is not allowed."));
			}
		}
	}
}
