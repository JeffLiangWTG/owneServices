namespace Enterprise.Customs.EU.GUI
{
	public partial class UCC6EntryLineAdditionalDataUserControl
	{
		void InitializeComponent()
		{
			this.ExtendInfoTabControl.SuspendLayout();
			this.ExtendedInfoTabPage.SuspendLayout();
			this.TaxOrFeeTabPage.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).BeginInit();
			this.EntryLineSupportingDocumentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ExtendedInfoTabPage
			// 
			this.ExtendedInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 26, true);
			this.ExtendedInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 240, true);
			// 
			// TaxOrFeeTabPage
			// 
			this.TaxOrFeeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 26, true);
			this.TaxOrFeeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 240, true);
			// 
			// SupportingDocumentsTabPage
			// 
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 26, true);
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 240, true);
			// 
			// DutyAndTaxDetails
			// 
			this.DutyAndTaxDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 234, true);
			this.DutyAndTaxDetails.UserControlType = typeof(Enterprise.Customs.EU.GUI.EntryLineTaxAndFeeUserControl);
			// 
			// EntryLineSupportingDocumentsGrid
			// 
			this.EntryLineSupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(823, 234, true);
			// 
			// UCC6EntryLineAdditionalDataUserControl
			// 
			this.Name = "UCC6EntryLineAdditionalDataUserControl";
			this.ExtendInfoTabControl.ResumeLayout(false);
			this.ExtendInfoTabControl.PerformLayout();
			this.ExtendedInfoTabPage.ResumeLayout(false);
			this.ExtendedInfoTabPage.PerformLayout();
			this.TaxOrFeeTabPage.ResumeLayout(false);
			this.TaxOrFeeTabPage.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineSupportingDocumentsGrid)).EndInit();
			this.EntryLineSupportingDocumentsGrid.ResumeLayout(false);
			this.EntryLineSupportingDocumentsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
