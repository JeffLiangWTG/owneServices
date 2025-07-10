#if DEBUG

namespace Enterprise.Accounting.GUI
{
	public partial class CASSExportBillingForm
	{
		public ZArchitecture.ZGrid DiscrepancyReportGrid_ForTestOnly
		{
			get { return DiscrepancyReportGrid; }
			set { DiscrepancyReportGrid = value; }
		}

		public ZArchitecture.ZGrid ExportLinesGrid_ForTestOnly
		{
			get { return ExportLinesGrid; }
			set { ExportLinesGrid = value; }
		}

		public ZArchitecture.ZGrid ImportLinesGrid_ForTestOnly
		{
			get { return ImportLinesGrid; }
			set { ImportLinesGrid = value; }
		}

		public ZArchitecture.ZGrid APTransactionsGrid_ForTestOnly
		{
			get { return APTransactionsGrid; }
			set { APTransactionsGrid = value; }
		}

		public Business.ARAP.Invoicing.CASSBilling CASSBusinessEntity_ForTestOnly => CASSBusinessEntity;

		public ZArchitecture.GUI.ZButton PreviewButton_ForTestOnly
		{
			get { return PreviewButton; }
			set { PreviewButton = value; }
		}

		public ZArchitecture.GUI.ZButton ImportButton_ForTestOnly
		{
			get { return ImportButton; }
			set { ImportButton = value; }
		}

		public ZArchitecture.GUI.ZCheckBox AutoCloseClaimCheckBox_ForTestOnly
		{
			get { return AutoCloseClaimCheckBox; }
			set { AutoCloseClaimCheckBox = value; }
		}

		public ZArchitecture.GUI.ZButton RefreshButton_ForTestOnly
		{
			get { return RefreshButton; }
			set { RefreshButton = value; }
		}

		public ZArchitecture.GUI.ZOpenFileDialog OpenFileDialog_ForTestOnly
		{
			get { return openFileDialog; }
			set { openFileDialog = value; }
		}

		public ZArchitecture.GUI.ZButton PrintButton_ForTestOnly
		{
			get { return PrintButton; }
			set { PrintButton = value; }
		}

		public ZArchitecture.GUI.ZTabControl TabControl_ForTestOnly
		{
			get { return TabControl; }
			set { TabControl = value; }
		}

		public ZArchitecture.GUI.ZTabPage DiscrepancyReportTabPage_ForTestOnly
		{
			get { return DiscrepancyReportTabPage; }
			set { DiscrepancyReportTabPage = value; }
		}

		public ZArchitecture.GUI.ZPanel RejectedClaimsPanel_ForTestOnly
		{
			get { return RejectedClaimsPanel; }
			set { RejectedClaimsPanel = value; }
		}

		public ZArchitecture.GUI.ZGroupBox TotalsGroupBox_ForTestOnly
		{
			get { return TotalsGroupBox; }
			set { TotalsGroupBox = value; }
		}

		public ZArchitecture.GUI.ZPanel SystemCostAccrualPanel_ForTestOnly
		{
			get { return SystemCostAccrualPanel; }
			set { SystemCostAccrualPanel = value; }
		}

		public ZArchitecture.GUI.ZButton ExportButton_ForTestOnly
		{
			get { return exportButton; }
			set { exportButton = value; }
		}

		public ZArchitecture.ZLabel SecurityCheckpointMessageForExportTabLabel_ForTestOnly
		{
			get { return SecurityCheckpointMessageForExportTabLabel; }
			set { SecurityCheckpointMessageForExportTabLabel = value; }
		}

		public ZArchitecture.ZLabel SecurityCheckpointMessageForImportTabLabel_ForTestOnly
		{
			get { return SecurityCheckpointMessageForImportTabLabel; }
			set { SecurityCheckpointMessageForImportTabLabel = value; }
		}

		public bool PromptForAdjustmentFilePrint_ForTestOnly
		{
			get { return promptForAdjustmentFilePrint; }
			set { promptForAdjustmentFilePrint = value; }
		}
	}
}

#endif
