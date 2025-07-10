using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed partial class HeaderExitReportStatusGridUserControl : ZUserControl, IDetailsReportsGridUserControl
	{
		public HeaderExitReportStatusGridUserControl()
		{
			InitializeComponent();
			ExitReportStatusGrid.SetReadOnlyIncludingColumnStyles(true);
		}

		public ZGrid ReportsGrid => ExitReportStatusGrid;
	}
}
