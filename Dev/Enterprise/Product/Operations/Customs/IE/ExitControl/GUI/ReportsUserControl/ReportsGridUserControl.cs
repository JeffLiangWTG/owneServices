using System.Windows.Forms;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public sealed partial class ReportsGridUserControl : ZUserControl, IReportsGridUserControl
	{
		public ReportsGridUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
			LoadFieldsControl();
		}

		internal ReportsGridFieldsLayoutUserControl ReportsGridFieldsLayoutUserControl { get; private set; }

		void LoadFieldsControl()
		{
			if (ReportsGridFieldsLayoutUserControl is null)
			{
				ReportsGridFieldsLayoutUserControl = new ReportsGridFieldsLayoutUserControl();
				ReportsGridFieldsLayoutUserControl.Dock = DockStyle.Fill;
				ReportsGridFieldsLayoutUserControl.CaptionRenderingEnabled = true;
				BindingSource.SetBindingMember(ReportsGridFieldsLayoutUserControl, ".");
				BottomPanel.Controls.Add(ReportsGridFieldsLayoutUserControl);
			}
		}

		ZGrid IReportsGridUserControl.ReportsGrid => ReportsGrid;

		void InitializeGridLayout()
		{
			using (ReportsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				ReportsGrid.SetAllColumnsVisible(false);
				ReportsGrid.SetColumnVisible(true, orderedColumns);
				ReportsGrid.ReOrderColumns(orderedColumns);
			}
		}

		readonly string[] orderedColumns = new[]
		{
			CusExitReport.Schema.CER_Type,
			CusExitReport.Schema.CER_CXC_Consignment,
			CusExitReport.Schema.CER_OfficeOfExit,
			CusExitReport.Schema.CER_Calc_FormattedDateTime,
			CusExitReport.Schema.CER_Calc_Discrepancies,
			CusExitReport.Schema.CER_EnquiryInformationCode,
			CusExitReport.Schema.CER_Calc_TypeOfLocation,
			CusExitReport.Schema.CER_Calc_UNLOCO,
			CusExitReport.Schema.CER_Status,
			CusExitReport.Schema.StatusDescription,
			CusExitReport.Schema.CER_MessageStatus,
			CusExitReport.Schema.MessageStatusDescription,
		};
	}
}
