using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public partial class HeaderExitReportStatusUcc6GridUserControl : ZUserControl, IDetailsReportsGridUserControl
{
	public HeaderExitReportStatusUcc6GridUserControl()
	{
		InitializeComponent();
		HideColumnsAndSetReadOnly();
	}

	void HideColumnsAndSetReadOnly()
	{
		ExitReportStatusGrid.SetColumnVisible(isVisible: false, [nameof(CusExitReport.TypeDescription), nameof(CusExitReport.CER_Type)]);
		ExitReportStatusGrid.SetReadOnlyIncludingColumnStyles(true);
	}

	public ZGrid ReportsGrid => ExitReportStatusGrid;
}
