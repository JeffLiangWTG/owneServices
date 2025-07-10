using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.GUI.JobDeclarationForms;
using Enterprise.Customs.GB.GUI.Plugin;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class GBImportSupplierHeaderUserControl : EU.GUI.EUNonLayoutImportSupplierHeaderUserControl
	{
		ZArchitecture.ZTextBox splitHouseTextBox;

		void InitializeComponent()
		{
			this.splitHouseTextBox = new ZArchitecture.ZTextBox();
			this.JZ_IncoTermBoundDropDownEdit.SuspendLayout();
			this.GroupInvoiceDropEdit.SuspendLayout();
			this.NoOfPacksCalcDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.JZ_InvoiceAmountBoundCurrencyControl.SuspendLayout();
			this.JZ_ValuationCodeDropEdit.SuspendLayout();
			this.JZ_InvoiceDateEdit.SuspendLayout();
			this.AgreedPlaceCodeFindBox.SuspendLayout();
			this.HeaderDescriptionsTabPage.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.InvoicePaymentTabPage.SuspendLayout();
			this.AdditionalInfoTabPage.SuspendLayout();
			this.PreviousDocumentsTabPage.SuspendLayout();
			this.ValueIndicatorsTabPage.SuspendLayout();
			this.previousDocumentsUserControl1.SuspendLayout();
			this.ValueIndicatorsUserControl.SuspendLayout();
			this.GroupChargesButton.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.InvoiceTabControl.SuspendLayout();
			this.ComInvoiceDetailsTabPage.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ChargesTabControl.SuspendLayout();
			this.InvoiceChargesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ApportionedTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
			this.ApportionedChargesGrid.SuspendLayout();
			this.BaseGroupChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).BeginInit();
			this.BaseGroupChargesGrid.SuspendLayout();
			this.JZ_FOBAmountBoundCurrencyControl.SuspendLayout();
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.SuspendLayout();
			this.JZ_CIFAmountBoundCurrencyControl.SuspendLayout();
			this.LineTotalBoundConvertToLocalCurrencyControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).BeginInit();
			this.JobComInvoiceHeadersBoundGrid.SuspendLayout();
			this.InvCustomFieldsDisplayControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Splitter)).BeginInit();
			this.Splitter.Panel1.SuspendLayout();
			this.Splitter.Panel2.SuspendLayout();
			this.Splitter.SuspendLayout();
			this.InvDetailLeftPanel.SuspendLayout();
			this.InvDetailRightPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).BeginInit();
			this.ChargesGroupsSplitterContainer.Panel1.SuspendLayout();
			this.ChargesGroupsSplitterContainer.Panel2.SuspendLayout();
			this.ChargesGroupsSplitterContainer.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// HeaderDescriptionsTabPage
			// 
			this.HeaderDescriptionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
			// 
			// SupportingDocumentsTabPage
			// 
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
			// 
			// InvoicePaymentTabPage
			// 
			this.InvoicePaymentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
			// 
			// AdditionalInfoTabPage
			// 
			this.AdditionalInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
			// 
			// PreviousDocumentsTabPage
			// 
			this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
			// 
			// ValueIndicatorsTabPage
			// 
			this.ValueIndicatorsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
			// 
			// previousDocumentsUserControl1
			// 
			this.previousDocumentsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 352, true);
			// 
			// ValueIndicatorsUserControl
			// 
			this.ValueIndicatorsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 352, true);
			// 
			// GroupChargesButton
			// 
			this.GroupChargesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 146, true);
			// 
			// ComInvoiceDetailsTabPage
			// 
			this.ComInvoiceDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 358, true);
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 187, true);
			this.ChargesGroupBox.TabIndex = 19;
			// 
			// ChargesTabControl
			// 
			this.ChargesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 168, true);
			// 
			// InvoiceChargesTabPage
			// 
			this.InvoiceChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 141, true);
			// 
			// InvoiceChargesGrid
			// 
			this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 141, true);
			// 
			// BaseGroupChargesGroupBox
			// 
			this.BaseGroupChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 165, true);
			this.BaseGroupChargesGroupBox.TabIndex = 20;
			// 
			// BaseGroupChargesGrid
			// 
			this.BaseGroupChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 146, true);
			// 
			// JobComInvoiceHeadersBoundGrid
			// 
			this.JobComInvoiceHeadersBoundGrid.GridId = null;
			// 
			// 
			// 
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.AllowNavigation = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CaptionVisible = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CheckDatabaseAfterFirstBinding = true;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = null;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.LayoutKey = "JobComInvoiceHeadersBoundGrid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Name = "Grid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 109, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.TabIndex = 0;
			// 
			// Splitter
			// 
			// 
			// InvDetailLeftPanel
			// 
			this.InvDetailLeftPanel.Controls.Add(this.splitHouseTextBox);
			this.InvDetailLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 358, true);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.DetailsGroupBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceDateEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.NoOfPacksCalcDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceCurrLandedCostExRateCalcEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.GroupInvoiceDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceCurrExRateCalcEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceNumberBoundTextBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceAmountBoundCurrencyControl, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.AgreedPlaceCodeFindBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_IncoTermBoundDropDownEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_IncoTermPlaceTextBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_ValuationCodeDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.splitHouseTextBox, 0);
			// 
			// InvDetailRightPanel
			// 
			this.InvDetailRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 358, true);
			// 
			// ChargesGroupsSplitterContainer
			// 
			this.ChargesGroupsSplitterContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 358, true);
			this.ChargesGroupsSplitterContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(187);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDeclaration);
			// 
			// splitHouseTextBox
			// 
			this.BindingSource.SetBindingMember(this.splitHouseTextBox, "Invoices.ZG_HouseSplitReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).ZG_HouseSplitReference);
			this.splitHouseTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("10F05594-CDDC-4010-BEDF-A92D8EB90186", "SRF", "Split", "Split Reference", "Split House Reference. Supplying a value here will wipe that on the front screen of the declaration. Creating multiple invoices with different split references will allow you to create multiple entries, one per split.");
			this.splitHouseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 299, true);
			this.splitHouseTextBox.Name = "splitHouseTextBox";
			this.splitHouseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.splitHouseTextBox.TabIndex = 17;
			// 
			// GBImportSupplierHeaderUserControl
			// 
			this.Name = "GBImportSupplierHeaderUserControl";
			this.JZ_IncoTermBoundDropDownEdit.ResumeLayout(true);
			this.JZ_IncoTermBoundDropDownEdit.PerformLayout();
			this.GroupInvoiceDropEdit.ResumeLayout(true);
			this.GroupInvoiceDropEdit.PerformLayout();
			this.NoOfPacksCalcDropEdit.ResumeLayout(true);
			this.NoOfPacksCalcDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.JZ_InvoiceAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_InvoiceAmountBoundCurrencyControl.PerformLayout();
			this.JZ_ValuationCodeDropEdit.ResumeLayout(true);
			this.JZ_ValuationCodeDropEdit.PerformLayout();
			this.JZ_InvoiceDateEdit.ResumeLayout(true);
			this.JZ_InvoiceDateEdit.PerformLayout();
			this.AgreedPlaceCodeFindBox.ResumeLayout(true);
			this.AgreedPlaceCodeFindBox.PerformLayout();
			this.HeaderDescriptionsTabPage.ResumeLayout(false);
			this.HeaderDescriptionsTabPage.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.InvoicePaymentTabPage.ResumeLayout(false);
			this.InvoicePaymentTabPage.PerformLayout();
			this.AdditionalInfoTabPage.ResumeLayout(false);
			this.AdditionalInfoTabPage.PerformLayout();
			this.PreviousDocumentsTabPage.ResumeLayout(false);
			this.PreviousDocumentsTabPage.PerformLayout();
			this.ValueIndicatorsTabPage.ResumeLayout(false);
			this.ValueIndicatorsTabPage.PerformLayout();
			this.previousDocumentsUserControl1.ResumeLayout(true);
			this.previousDocumentsUserControl1.PerformLayout();
			this.ValueIndicatorsUserControl.ResumeLayout(true);
			this.ValueIndicatorsUserControl.PerformLayout();
			this.GroupChargesButton.ResumeLayout(false);
			this.GroupChargesButton.PerformLayout();
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.InvoiceTabControl.ResumeLayout(false);
			this.InvoiceTabControl.PerformLayout();
			this.ComInvoiceDetailsTabPage.ResumeLayout(false);
			this.ComInvoiceDetailsTabPage.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			this.ChargesTabControl.ResumeLayout(false);
			this.ChargesTabControl.PerformLayout();
			this.InvoiceChargesTabPage.ResumeLayout(false);
			this.InvoiceChargesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ApportionedTabPage.ResumeLayout(false);
			this.ApportionedTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
			this.ApportionedChargesGrid.ResumeLayout(false);
			this.ApportionedChargesGrid.PerformLayout();
			this.BaseGroupChargesGroupBox.ResumeLayout(false);
			this.BaseGroupChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).EndInit();
			this.BaseGroupChargesGrid.ResumeLayout(false);
			this.BaseGroupChargesGrid.PerformLayout();
			this.JZ_FOBAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_FOBAmountBoundCurrencyControl.PerformLayout();
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.ResumeLayout(true);
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.PerformLayout();
			this.JZ_CIFAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_CIFAmountBoundCurrencyControl.PerformLayout();
			this.LineTotalBoundConvertToLocalCurrencyControl.ResumeLayout(true);
			this.LineTotalBoundConvertToLocalCurrencyControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).EndInit();
			this.JobComInvoiceHeadersBoundGrid.ResumeLayout(true);
			this.JobComInvoiceHeadersBoundGrid.PerformLayout();
			this.InvCustomFieldsDisplayControl.ResumeLayout(true);
			this.InvCustomFieldsDisplayControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.Splitter.Panel1.ResumeLayout(false);
			this.Splitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Splitter)).EndInit();
			this.Splitter.ResumeLayout(false);
			this.Splitter.PerformLayout();
			this.InvDetailLeftPanel.ResumeLayout(false);
			this.InvDetailLeftPanel.PerformLayout();
			this.InvDetailRightPanel.ResumeLayout(false);
			this.InvDetailRightPanel.PerformLayout();
			this.ChargesGroupsSplitterContainer.Panel1.ResumeLayout(false);
			this.ChargesGroupsSplitterContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).EndInit();
			this.ChargesGroupsSplitterContainer.ResumeLayout(false);
			this.ChargesGroupsSplitterContainer.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
