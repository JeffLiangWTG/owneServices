using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.Module;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ES.ExitControl.Module
{
	public partial class ExitControlReportFilterStripControl : EU.ExitControl.Module.ExitControlReportFilterStripControl
	{
		public ExitControlReportFilterStripControl(IBusinessObjectCollection gridCollection, ExitControlReportFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			AddExtraColumns();
		}

		#region Grid columns & ordering.

		void AddExtraColumns()
		{
			using (grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusExitReport.ArrivalDate),
					CaptionResourceString = Res.GetData("A02FB178-EFA1-4EA2-8FF7-4E7652FC3642", "Arrival Date"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(83),
				});

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusExitReport.ClearanceDate),
					CaptionResourceString = Res.GetData("73E82591-0D21-4582-9671-662996B8A8D3", "Clearance Date"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(83),
				});

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusExitReport.Circuit),
					CaptionResourceString = Res.GetData("4343208C-F8DF-4985-B408-5258CA5B7B83", "Circuit"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				});
			}
		}

		#endregion
	}
}
