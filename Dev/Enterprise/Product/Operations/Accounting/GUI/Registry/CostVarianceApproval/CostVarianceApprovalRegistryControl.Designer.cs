namespace Enterprise.Accounting.Registry.GUI
{
	partial class CostVarianceApprovalRegistryControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.AuthorisationRequirementsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AutoTickFinalFlagCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.VarianceComparisonOptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.VarianceCalculationStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VarianceComparisonOptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationRequirementsGrid)).BeginInit();
			this.AuthorisationRequirementsGrid.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.VarianceCalculationStyleDropEdit.SuspendLayout();
			this.VarianceComparisonOptionDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.CostVarianceApproval);
			// 
			// AuthorisationRequirementsGrid
			// 
			this.AuthorisationRequirementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AuthorisationRequirementsGrid, "AuthorisationRequirements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).AuthorisationRequirements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).AuthorisationRequirements)).SyncRoot)).VarianceSign)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).AuthorisationRequirements)).SyncRoot)).AuthorisationRequirement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).AuthorisationRequirements)).SyncRoot)).Range)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).AuthorisationRequirements)).SyncRoot)).AmountDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).AuthorisationRequirements)).SyncRoot)).LocalCostAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).AuthorisationRequirements)).SyncRoot)).Percentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).AuthorisationRequirements)).SyncRoot)).MonitorTotalInvoiceVariance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).AuthorisationRequirements)).SyncRoot)).AmountDecimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement)(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).AuthorisationRequirements)).SyncRoot)).TotalInvoiceVarianceAmount)));
			this.AuthorisationRequirementsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("26e9cb29-32eb-4f1c-a348-7aad30864632", "+/-", "Sign", "Variance Sign", "");
			zDropEditColumnStyleInfo4.ColumnName = "VarianceSign";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostVarianceApprovalRegistryControl|97f4940e-5d56-4663-bc82-767ecb7c1fff", "Approval Level");
			zDropEditColumnStyleInfo5.ColumnName = "AuthorisationRequirementLocalized";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostVarianceApprovalRegistryControl|0a9cead5-fc29-4d80-b7f4-d7fbd8ae5300", "Boundary");
			zDropEditColumnStyleInfo6.ColumnName = "RangeLocalized";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = "AmountDecimals";
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostVarianceApprovalRegistryControl|cee2d693-ae29-47e1-a07f-801f28cfa319", "Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "LocalCostAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostVarianceApprovalRegistryControl|e101089c-6f17-4fe2-8189-6388937e8514", "Percentage");
			zCalcEditColumnStyleInfo5.ColumnName = "Percentage";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostVarianceApprovalRegistryControl|3389d425-f706-4255-9e4d-39ebb591a441", "Monitor Total Invoice Variance");
			zCheckBoxColumnStyleInfo2.ColumnName = "MonitorTotalInvoiceVariance";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = "AmountDecimals";
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostVarianceApprovalRegistryControl|fb818bcc-4323-45e7-a88b-f0a0406a714d", "Total Invoice Variance Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "TotalInvoiceVarianceAmount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AuthorisationRequirementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.AuthorisationRequirementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.AuthorisationRequirementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.AuthorisationRequirementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.AuthorisationRequirementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.AuthorisationRequirementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.AuthorisationRequirementsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.AuthorisationRequirementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorisationRequirementsGrid.GridId = "ff10e12e-7f5c-41e3-8e09-e2ec14a69c4f";
			this.AuthorisationRequirementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AuthorisationRequirementsGrid.LayoutKey = "zGrid1";
			this.AuthorisationRequirementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			this.AuthorisationRequirementsGrid.Name = "AuthorisationRequirementsGrid";
			this.AuthorisationRequirementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 215, true);
			this.AuthorisationRequirementsGrid.TabIndex = 2;
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.AutoTickFinalFlagCheckBox);
			this.topPanel.Controls.Add(this.VarianceComparisonOptionLabel);
			this.topPanel.Controls.Add(this.VarianceCalculationStyleDropEdit);
			this.topPanel.Controls.Add(this.VarianceComparisonOptionDropEdit);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 152, true);
			this.topPanel.TabIndex = 3;
			// 
			// AutoTickFinalFlagCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AutoTickFinalFlagCheckBox, "AutoTickFinalFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).AutoTickFinalFlag)));
			this.AutoTickFinalFlagCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostVarianceApprovalRegistryControl|5A9958CF-FC31-4858-AA6A-0C9BB25411F1", "Automatically tick costs as Final when they fall within the \'None\' approval threshold.\r\n\r\nNote: When this registry is \'Ticked\', the following registries must be set to \'No\'.\r\na. Accounting > Payable Defaults > Default Settings > Final Flag.\r\nb. Accounting > Payable Defaults > Default Settings > Final Flag Default for Consol Costs on AP Invoice Screen.");
			this.AutoTickFinalFlagCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutoTickFinalFlagCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 60, true);
			this.AutoTickFinalFlagCheckBox.Name = "AutoTickFinalFlagCheckBox";
			this.AutoTickFinalFlagCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 86, true);
			this.AutoTickFinalFlagCheckBox.TabIndex = 5;
			this.AutoTickFinalFlagCheckBox.UseVisualStyleBackColor = true;
			// 
			// VarianceComparisonOptionLabel
			// 
			this.VarianceComparisonOptionLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostVarianceApprovalRegistryControl|3ce10a6e-f11c-40dd-a743-76de09868f2a", "Variance Comparison Option");
			this.VarianceComparisonOptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 30, true);
			this.VarianceComparisonOptionLabel.Name = "VarianceComparisonOptionLabel";
			this.VarianceComparisonOptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 24, true);
			this.VarianceComparisonOptionLabel.TabIndex = 4;
			// 
			// VarianceCalculationStyleDropEdit
			// 
			this.VarianceCalculationStyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VarianceCalculationStyleDropEdit, "VarianceCalculationStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).VarianceCalculationStyle)));
			this.VarianceCalculationStyleDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostVarianceApprovalRegistryControl|df040735-2338-4a41-82f2-a74ceb83dc34", "Variance Calculation Style");
			this.VarianceCalculationStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 6, true);
			this.VarianceCalculationStyleDropEdit.Name = "VarianceCalculationStyleDropEdit";
			this.VarianceCalculationStyleDropEdit.PreBoundMaxLength = 3;
			this.VarianceCalculationStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 20, true);
			this.VarianceCalculationStyleDropEdit.TabIndex = 2;
			// 
			// VarianceComparisonOptionDropEdit
			// 
			this.VarianceComparisonOptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VarianceComparisonOptionDropEdit, "VarianceComparisonOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.CostVarianceApproval)(null)).VarianceComparisonOption)));
			this.VarianceComparisonOptionDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostVarianceApprovalRegistryControl|ab87dd65-b337-49a4-a03a-3b08887daa2e", "Variance Comparison Option");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VarianceComparisonOptionDropEdit, false);
			this.VarianceComparisonOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 34, true);
			this.VarianceComparisonOptionDropEdit.Name = "VarianceComparisonOptionDropEdit";
			this.VarianceComparisonOptionDropEdit.PreBoundMaxLength = 3;
			this.VarianceComparisonOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 20, true);
			this.VarianceComparisonOptionDropEdit.TabIndex = 3;
			// 
			// CostVarianceApprovalRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AuthorisationRequirementsGrid);
			this.Controls.Add(this.topPanel);
			this.Name = "CostVarianceApprovalRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 367, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationRequirementsGrid)).EndInit();
			this.AuthorisationRequirementsGrid.ResumeLayout(false);
			this.AuthorisationRequirementsGrid.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.VarianceCalculationStyleDropEdit.ResumeLayout(true);
			this.VarianceCalculationStyleDropEdit.PerformLayout();
			this.VarianceComparisonOptionDropEdit.ResumeLayout(true);
			this.VarianceComparisonOptionDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid AuthorisationRequirementsGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel topPanel;
		private Enterprise.ZArchitecture.ZLabel VarianceComparisonOptionLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit VarianceCalculationStyleDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit VarianceComparisonOptionDropEdit;
		private ZArchitecture.GUI.ZCheckBox AutoTickFinalFlagCheckBox;
	}
}
