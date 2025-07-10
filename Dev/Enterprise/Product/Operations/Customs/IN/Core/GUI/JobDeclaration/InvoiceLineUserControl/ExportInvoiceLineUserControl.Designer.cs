namespace Enterprise.Customs.IN.GUI;

partial class ExportInvoiceLineUserControl
{
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
			this.DutyFreeImportAuthorizationPanel = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.JobWorkTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.JobWorkPanel = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.InvoiceLineSWProductionUserControl = new Enterprise.Customs.IN.GUI.LayoutSWProductionUserControl();
			this.SWProductionTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SWConstituentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SWConstituentUserControl = new Enterprise.Customs.IN.GUI.InvoiceLineSWConstituentUserControl();
			this.PackagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PackagesPivotUserControl = new Enterprise.Customs.GUI.BaseLineLevelPackingPivotControl();
			this.PartiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PartiesUserControl = new Enterprise.Customs.IN.GUI.InvoiceLinePartiesUserControl();
			this.DutyFreeImportAuthorizationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InvoiceLineSWControlsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InvoiceLineSWControlsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.JI_CountryOfOriginBoundFindBox.SuspendLayout();
			this.JI_RH_NKCommodity_CodeBoundFindBox.SuspendLayout();
			this.JI_LinePriceBoundCurrencyControl.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
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
			this.CustomFieldsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JobWorkTabPage.SuspendLayout();
			this.SWConstituentTabPage.SuspendLayout();
			this.SWConstituentUserControl.SuspendLayout();
			this.PackagesTabPage.SuspendLayout();
			this.PackagesPivotUserControl.SuspendLayout();
			this.PartiesTabPage.SuspendLayout();
			this.PartiesUserControl.SuspendLayout();
			this.DutyFreeImportAuthorizationTabPage.SuspendLayout();
			this.InvoiceLineSWControlsTabPage.SuspendLayout();
			this.InvoiceLineSWControlsUserControl.SuspendLayout();
			this.InvoiceLineSWProductionUserControl.SuspendLayout();
			this.SWProductionTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// TopPanel
			// 
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 533, true);
			// 
			// LineDetailTabControl
			// 
			this.LineDetailTabControl.Controls.Add(this.SWConstituentTabPage);
			this.LineDetailTabControl.Controls.Add(this.InvoiceLineSWControlsTabPage);
			this.LineDetailTabControl.Controls.Add(this.JobWorkTabPage);
			this.LineDetailTabControl.Controls.Add(this.DutyFreeImportAuthorizationTabPage);
			this.LineDetailTabControl.Controls.Add(this.PackagesTabPage);
			this.LineDetailTabControl.Controls.Add(this.SWProductionTabPage);
			this.LineDetailTabControl.Controls.Add(this.PartiesTabPage);
			this.LineDetailTabControl.Controls.SetChildIndex(this.CustomFieldsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.PartiesTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.SWProductionTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.DutyFreeImportAuthorizationTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.JobWorkTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.SWConstituentTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.InvoiceLineSWControlsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.NewLineDetailsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineDetailsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.PackagesTabPage, 0);
			// 
			// CantCreateInvoiceLinesLabel
			// 
			this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
			this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 533, true);
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 533, true);
			// 
			// Splitter
			// 
			this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			// 
			// DutyFreeImportAuthorizationPanel
			// 
			this.DutyFreeImportAuthorizationPanel.AllowDrop = true;
			this.DutyFreeImportAuthorizationPanel.BindingMember = "FilteredInvoiceLines.DfiaExportItemDetails";
			this.DutyFreeImportAuthorizationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DutyFreeImportAuthorizationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DutyFreeImportAuthorizationPanel.Name = "DutyFreeImportAuthorizationPanel";
			this.DutyFreeImportAuthorizationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 287, true);
			this.DutyFreeImportAuthorizationPanel.TabIndex = 0;
			// 
			// JobWorkTabPage
			// 
			this.JobWorkTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("ExportInvoiceLineUserControl|736DB64C-B6EB-4CBE-9071-40CF220D18D3", "Job Work");
			this.JobWorkTabPage.Controls.Add(this.JobWorkPanel);
			this.JobWorkTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.JobWorkTabPage.Name = "JobWorkTabPage";
			this.JobWorkTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.JobWorkTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			this.JobWorkTabPage.TabIndex = 3;
			this.JobWorkTabPage.UseVisualStyleBackColor = true;
			// 
			// JobWorkPanel
			// 
			this.JobWorkPanel.AllowDrop = true;
			this.JobWorkPanel.BindingMember = "FilteredInvoiceLines";
			this.JobWorkPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobWorkPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JobWorkPanel.Name = "JobWorkPanel";
			this.JobWorkPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 287, true);
			this.JobWorkPanel.TabIndex = 0;
			// 
			// SWConstituentTabPage
			// 
			this.SWConstituentTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("c35aebc0-3ec8-4fd1-9865-3bbd855b03e9", "SW Constituent");
			this.SWConstituentTabPage.Controls.Add(this.SWConstituentUserControl);
			this.SWConstituentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SWConstituentTabPage.Name = "SWConstituentTabPage";
			this.SWConstituentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			this.SWConstituentTabPage.TabIndex = 1;
			this.SWConstituentTabPage.UseVisualStyleBackColor = true;
			// 
			// SWConstituentUserControl
			// 
			this.SWConstituentUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SWConstituentUserControl, "FilteredInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)))));
			this.SWConstituentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SWConstituentUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SWConstituentUserControl.Name = "SWConstituentUserControl";
			this.SWConstituentUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			this.SWConstituentUserControl.TabIndex = 0;
			// 
			// PartiesTabPage
			// 
			this.PartiesTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("8FCBE658-7182-49AA-94EC-F83480EAC9E0", "Parties");
			this.PartiesTabPage.Controls.Add(this.PartiesUserControl);
			this.PartiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PartiesTabPage.Name = "PartiesTabPage";
			this.PartiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			this.PartiesTabPage.TabIndex = 2;
			this.PartiesTabPage.UseVisualStyleBackColor = true;
			// 
			// PartiesUserControl
			// 
			this.PartiesUserControl.AllowDrop = true;
			this.PartiesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PartiesUserControl.Name = "PartiesUserControl";
			this.PartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			this.PartiesUserControl.TabIndex = 0;
			// 
			// DutyFreeImportAuthorizationTabPage
			// 
			this.DutyFreeImportAuthorizationTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("23016955-2000-4d93-9a85-8ea91c149064", "DFIA");
			this.DutyFreeImportAuthorizationTabPage.Controls.Add(this.DutyFreeImportAuthorizationPanel);
			this.DutyFreeImportAuthorizationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DutyFreeImportAuthorizationTabPage.Name = "DutyFreeImportAuthorizationTabPage";
			this.DutyFreeImportAuthorizationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DutyFreeImportAuthorizationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			this.DutyFreeImportAuthorizationTabPage.TabIndex = 3;
			this.DutyFreeImportAuthorizationTabPage.UseVisualStyleBackColor = true;
			// 
			// InvoiceLineSWProductionUserControl
			// 
			this.InvoiceLineSWProductionUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceLineSWProductionUserControl, "FilteredInvoiceLines.SWProductions");
			this.InvoiceLineSWProductionUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLineSWProductionUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InvoiceLineSWProductionUserControl.Name = "InvoiceLineSWProductionUserControl";
			this.InvoiceLineSWProductionUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 276, true);
			this.InvoiceLineSWProductionUserControl.TabIndex = 0;
			// 
			// SWProductionTabPage
			// 
			this.SWProductionTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("887ED838-3EA6-43F2-9F13-E850315B337E", "SW Production Details");
			this.SWProductionTabPage.Controls.Add(this.InvoiceLineSWProductionUserControl);
			this.SWProductionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 34, true);
			this.SWProductionTabPage.Name = "SWProductionTabPage";
			this.SWProductionTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SWProductionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 282, true);
			this.SWProductionTabPage.TabIndex = 3;
			this.SWProductionTabPage.UseVisualStyleBackColor = true;
			// 
			// SWControlsTabPage
			// 
			this.InvoiceLineSWControlsTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("c35aebc0-3ec8-4fd1-9865-3bbd855b03e8", "SW Controls");
			this.InvoiceLineSWControlsTabPage.Controls.Add(this.InvoiceLineSWControlsUserControl);
			this.InvoiceLineSWControlsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InvoiceLineSWControlsTabPage.Name = "InvoiceLineSWControlsTabPage";
			this.InvoiceLineSWControlsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			this.InvoiceLineSWControlsTabPage.TabIndex = 1;
			this.InvoiceLineSWControlsTabPage.UseVisualStyleBackColor = true;
			// 
			// SWControlsUserControl
			// 
			this.InvoiceLineSWControlsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceLineSWControlsUserControl, "FilteredInvoiceLines.SWControls");
			this.InvoiceLineSWControlsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLineSWControlsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceLineSWControlsUserControl.Name = "InvoiceLineSWControlsUserControl";
			this.InvoiceLineSWControlsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			this.InvoiceLineSWControlsUserControl.TabIndex = 0;
			// 
			// PackagesTabPage
			// 
			this.PackagesTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("ExportInvoiceLineUserControl|1B32A9AF-C7C5-4F7A-9E78-03B7EE72B521", "Packages");
			this.PackagesTabPage.Controls.Add(this.PackagesPivotUserControl);
			this.PackagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.PackagesTabPage.Name = "PackagesTabPage";
			this.PackagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 296, true);
			this.PackagesTabPage.TabIndex = 4;
			// 
			// PackagesPivotUserControl
			// 
			this.PackagesPivotUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackagesPivotUserControl, "FilteredInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((Enterprise.Customs.IN.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)))));
			this.PackagesPivotUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesPivotUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PackagesPivotUserControl.Name = "PackagesPivotUserControl";
			this.PackagesPivotUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 290, true);
			this.PackagesPivotUserControl.TabIndex = 0;
			// 
			// ExportInvoiceLineUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ExportInvoiceLineUserControl";
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
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JobWorkTabPage.ResumeLayout(false);
			this.JobWorkTabPage.PerformLayout();
			this.SWConstituentTabPage.ResumeLayout(false);
			this.SWConstituentTabPage.PerformLayout();
			this.SWConstituentUserControl.ResumeLayout(true);
			this.SWConstituentUserControl.PerformLayout();
			this.PackagesTabPage.ResumeLayout(false);
			this.PackagesTabPage.PerformLayout();
			this.PackagesPivotUserControl.ResumeLayout(true);
			this.PackagesPivotUserControl.PerformLayout();
			this.PartiesTabPage.ResumeLayout(false);
			this.PartiesTabPage.PerformLayout();
			this.PartiesUserControl.ResumeLayout(true);
			this.PartiesUserControl.PerformLayout();
			this.DutyFreeImportAuthorizationTabPage.ResumeLayout(false);
			this.DutyFreeImportAuthorizationTabPage.PerformLayout();
			this.InvoiceLineSWControlsTabPage.ResumeLayout(false);
			this.InvoiceLineSWControlsTabPage.PerformLayout();
			this.InvoiceLineSWControlsUserControl.ResumeLayout(true);
			this.InvoiceLineSWControlsUserControl.PerformLayout();
			this.InvoiceLineSWProductionUserControl.ResumeLayout(true);
			this.InvoiceLineSWProductionUserControl.PerformLayout();
			this.SWProductionTabPage.ResumeLayout(false);
			this.SWProductionTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal Enterprise.ZArchitecture.GUI.ZTabPage SWConstituentTabPage;
	internal InvoiceLineSWConstituentUserControl SWConstituentUserControl;
	internal ZArchitecture.GUI.ZTabPage JobWorkTabPage;
	internal ZArchitecture.GUI.ZTabPage PackagesTabPage;
	internal Customs.GUI.BaseLineLevelPackingPivotControl PackagesPivotUserControl;
	internal ZArchitecture.GUI.ZDynamicControlCreationUserControl JobWorkPanel;
	private ZArchitecture.GUI.ZDynamicControlCreationUserControl DutyFreeImportAuthorizationPanel;
	private ZArchitecture.GUI.ZTabPage DutyFreeImportAuthorizationTabPage;
	internal Enterprise.ZArchitecture.GUI.ZTabPage InvoiceLineSWControlsTabPage;
	internal ZArchitecture.GUI.ZDynamicControlCreationUserControl InvoiceLineSWControlsUserControl;
	internal ZArchitecture.GUI.ZTabPage SWProductionTabPage;
	internal LayoutSWProductionUserControl InvoiceLineSWProductionUserControl;
	internal Enterprise.ZArchitecture.GUI.ZTabPage PartiesTabPage;
	internal InvoiceLinePartiesUserControl PartiesUserControl;
}
