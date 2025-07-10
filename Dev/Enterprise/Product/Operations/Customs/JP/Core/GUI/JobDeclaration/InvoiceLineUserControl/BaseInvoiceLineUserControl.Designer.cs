namespace Enterprise.Customs.JP.GUI
{
	public partial class BaseInvoiceLineUserControl
	{
		void InitializeComponent()
		{
			this.AdditionalDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TradeControlOrderAppendixDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.JI_CountryOfOriginBoundFindBox.SuspendLayout();
			this.JI_RH_NKCommodity_CodeBoundFindBox.SuspendLayout();
			this.JI_LinePriceBoundCurrencyControl.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
			this.LineChargesTabPage.SuspendLayout();
			this.CurrentInvoicePanel.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
			this.CusContainerInvoiceLineGrid.SuspendLayout();
			this.LineDetailsTabPage.SuspendLayout();
			this.NewLineDetailsTabPage.SuspendLayout();
			this.InvoiceLineDetailsUserControl.SuspendLayout();
			this.ClassificationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
			this.CustomsInvoiceLinesBoundGrid.SuspendLayout();
			this.JI_Calc_CIFConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_FreightConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_FOBConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_GSTConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_DutyConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.SuspendLayout();
			this.CustomsQuantityCalcDropEdit.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.JI_WeightCalcDropEdit.SuspendLayout();
			this.InvoiceQuantityCalcDropEdit.SuspendLayout();
			this.JI_DescriptionBoundTextBox.SuspendLayout();
			this.TradeControlOrderAppendixDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalDetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// TopPanel
			// 
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 534, true);
			// 
			// LineDetailTabControl
			// 
			this.LineDetailTabControl.Controls.Add(this.AdditionalDetailsTabPage);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineChargesTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.NewLineDetailsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineDetailsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.AdditionalDetailsTabPage, 0);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 199, true);
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 96, true);
			// 
			// JI_RH_NKCommodity_CodeBoundFindBox
			// 
			this.JI_RH_NKCommodity_CodeBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 16, true);
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 295, true);
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 295, true);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 295, true);
			// 
			// CusContainerInvoiceLineGrid
			// 
			this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 280, true);
			// 
			// CantCreateInvoiceLinesLabel
			// 
			this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 534, true);
			// 
			// LineDetailsTabPage
			// 
			this.LineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 295, true);
			// 
			// NewLineDetailsTabPage
			// 
			this.NewLineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.NewLineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 295, true);
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.InvoiceLineDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 295, true);
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 534, true);
			// 
			// Splitter
			// 
			this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.JobDeclaration);
			// 
			// AdditionalDetailsTabPage
			// 
			this.AdditionalDetailsTabPage.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("2c4e5ab6-8b80-4767-ba2a-8c2873aadd79", "Additional Details");
			this.AdditionalDetailsTabPage.Controls.Add(this.AdditionalDetailsPanel);
			this.AdditionalDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.AdditionalDetailsTabPage.Name = "AdditionalDetailsTabPage";
			this.AdditionalDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 295, true);
			this.AdditionalDetailsTabPage.TabIndex = 3;
			// 
			// AdditionalDetailsPanel
			// 
			this.AdditionalDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalDetailsPanel.Name = "AdditionalDetailsPanel";
			this.AdditionalDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 288, true);
			this.AdditionalDetailsPanel.TabIndex = 0;
			// 
			// TradeControlOrderAppendixDropEdit
			// 
			this.TradeControlOrderAppendixDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TradeControlOrderAppendixDropEdit, "FilteredInvoiceLines.JI_TradeControlOrderAppendix");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TradeControlOrderAppendix)));
			this.TradeControlOrderAppendixDropEdit.Name = "TradeControlOrderAppendixDropEdit";
			this.TradeControlOrderAppendixDropEdit.PreBoundMaxLength = 2;
			this.TradeControlOrderAppendixDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 37, true);
			this.TradeControlOrderAppendixDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 15, true);
			this.TradeControlOrderAppendixDropEdit.TabIndex = 3;
			// 
			// BaseInvoiceLineUserControl
			// 
			this.Name = "BaseInvoiceLineUserControl";
			this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
			this.InvoiceLinesSummaryGroupBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.LineDetailTabControl.ResumeLayout(false);
			this.LineDetailTabControl.PerformLayout();
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.PerformLayout();
			this.JI_CountryOfOriginBoundFindBox.ResumeLayout(true);
			this.JI_CountryOfOriginBoundFindBox.PerformLayout();
			this.JI_RH_NKCommodity_CodeBoundFindBox.ResumeLayout(true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.PerformLayout();
			this.JI_LinePriceBoundCurrencyControl.ResumeLayout(true);
			this.JI_LinePriceBoundCurrencyControl.PerformLayout();
			this.ClassificationDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.PerformLayout();
			this.LineChargesTabPage.ResumeLayout(false);
			this.LineChargesTabPage.PerformLayout();
			this.CurrentInvoicePanel.ResumeLayout(false);
			this.CurrentInvoicePanel.PerformLayout();
			this.LineSummaryPanel.ResumeLayout(false);
			this.LineSummaryPanel.PerformLayout();
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersTabPage.PerformLayout();
			this.ContainersGroupBox.ResumeLayout(false);
			this.ContainersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
			this.CusContainerInvoiceLineGrid.ResumeLayout(false);
			this.CusContainerInvoiceLineGrid.PerformLayout();
			this.LineDetailsTabPage.ResumeLayout(false);
			this.LineDetailsTabPage.PerformLayout();
			this.NewLineDetailsTabPage.ResumeLayout(false);
			this.NewLineDetailsTabPage.PerformLayout();
			this.InvoiceLineDetailsUserControl.ResumeLayout(true);
			this.InvoiceLineDetailsUserControl.PerformLayout();
			this.ClassificationPanel.ResumeLayout(false);
			this.ClassificationPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
			this.CustomsInvoiceLinesBoundGrid.ResumeLayout(false);
			this.CustomsInvoiceLinesBoundGrid.PerformLayout();
			this.JI_Calc_CIFConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_CIFConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_FreightConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_FreightConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_FOBConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_FOBConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_GSTConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_DutyConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_DutyConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.PerformLayout();
			this.CustomsQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsQuantityCalcDropEdit.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.JI_WeightCalcDropEdit.ResumeLayout(true);
			this.JI_WeightCalcDropEdit.PerformLayout();
			this.InvoiceQuantityCalcDropEdit.ResumeLayout(true);
			this.InvoiceQuantityCalcDropEdit.PerformLayout();
			this.JI_DescriptionBoundTextBox.ResumeLayout(true);
			this.JI_DescriptionBoundTextBox.PerformLayout();
			this.TradeControlOrderAppendixDropEdit.ResumeLayout(true);
			this.TradeControlOrderAppendixDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalDetailsTabPage.ResumeLayout(false);
			this.AdditionalDetailsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected Enterprise.ZArchitecture.GUI.ZTabPage AdditionalDetailsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZPanel AdditionalDetailsPanel;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit TradeControlOrderAppendixDropEdit;
	}
}
