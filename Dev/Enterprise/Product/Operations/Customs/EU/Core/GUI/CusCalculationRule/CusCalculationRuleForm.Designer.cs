namespace Enterprise.Customs.EU.GUI
{
	partial class CusCalculationRuleForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RuleTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BasedOnDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImporterFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RatesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.rulePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.EndDateEdit.SuspendLayout();
			this.StartDateEdit.SuspendLayout();
			this.RuleTypeDropEdit.SuspendLayout();
			this.CurrencyCodeFindBox.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.BasedOnDropEdit.SuspendLayout();
			this.ImporterFindBox.SuspendLayout();
			this.RatesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RatesGrid)).BeginInit();
			this.RatesGrid.SuspendLayout();
			this.rulePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 423, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusCalculationRule);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(378, 392, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 28, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("677312c3-f569-4353-9413-13a549be6fde", "Details");
			this.DetailsGroupBox.Controls.Add(this.EndDateEdit);
			this.DetailsGroupBox.Controls.Add(this.StartDateEdit);
			this.DetailsGroupBox.Controls.Add(this.RuleTypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.CurrencyCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.TransportModeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.BasedOnDropEdit);
			this.DetailsGroupBox.Controls.Add(this.ImporterFindBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 143, true);
			this.DetailsGroupBox.TabIndex = 1;
			this.DetailsGroupBox.TabStop = false;
			// 
			// EndDateEdit
			// 
			this.EndDateEdit.AllowDrop = true;
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "CCR_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).CCR_EndDate)));
			this.EndDateEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("0bb12809-17df-4e42-95a0-9a4e9218b44a", "End Date");
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 82, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 6;
			// 
			// StartDateEdit
			// 
			this.StartDateEdit.AllowDrop = true;
			this.StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.StartDateEdit, "CCR_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).CCR_StartDate)));
			this.StartDateEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("46cf31bf-aff3-4c90-8e93-b100e9e4d419", "Start Date");
			this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 56, true);
			this.StartDateEdit.Name = "StartDateEdit";
			this.StartDateEdit.TabIndex = 5;
			// 
			// RuleTypeDropEdit
			// 
			this.RuleTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RuleTypeDropEdit, "CCR_RuleType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).CCR_RuleType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).Lookups.RuleTypeList)));
			this.RuleTypeDropEdit.BindToList = "Lookups.RuleTypeList";
			this.RuleTypeDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9ec01045-cc4c-4b70-a682-4ba2562f57f2", "Rule Type");
			this.RuleTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 30, true);
			this.RuleTypeDropEdit.Name = "RuleTypeDropEdit";
			this.RuleTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 22, true);
			this.RuleTypeDropEdit.TabIndex = 4;
			// 
			// CurrencyCodeFindBox
			// 
			this.CurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyCodeFindBox, "CCR_RX_NKCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).CCR_RX_NKCurrency)));
			this.CurrencyCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("42de06e0-20e9-48c3-92bf-229d707b6677", "Currency");
			this.CurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 108, true);
			this.CurrencyCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.CurrencyCodeFindBox.Name = "CurrencyCodeFindBox";
			this.CurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CurrencyCodeFindBox.ParentType = null;
			this.CurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 22, true);
			this.CurrencyCodeFindBox.TabIndex = 3;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "CCR_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).CCR_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).Lookups.TransportModeList)));
			this.TransportModeDropEdit.BindToList = "Lookups.TransportModeList";
			this.TransportModeDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("d6c41734-59a4-4096-8035-0f506e0c46a0", "Transport Mode");
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 82, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 22, true);
			this.TransportModeDropEdit.TabIndex = 2;
			// 
			// BasedOnDropEdit
			// 
			this.BasedOnDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BasedOnDropEdit, "CCR_BasedOn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).CCR_BasedOn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).Lookups.BasedOnList)));
			this.BasedOnDropEdit.BindToList = "Lookups.BasedOnList";
			this.BasedOnDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("ca5b7742-0e44-43f7-a5b7-57b06e143f7d", "Based On");
			this.BasedOnDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 56, true);
			this.BasedOnDropEdit.Name = "BasedOnDropEdit";
			this.BasedOnDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 22, true);
			this.BasedOnDropEdit.TabIndex = 1;
			// 
			// ImporterFindBox
			// 
			this.ImporterFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterFindBox, "CCR_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).CCR_OH_Importer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).Lookups.Importers)));
			this.ImporterFindBox.BindToList = "Lookups.Importers";
			this.ImporterFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("4098ef07-ecdb-4f96-8ee2-49dd0d8c8679", "Importer");
			this.ImporterFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 30, true);
			this.ImporterFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.ImporterFindBox.Name = "ImporterFindBox";
			this.ImporterFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ImporterFindBox.ParentType = null;
			this.ImporterFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 22, true);
			this.ImporterFindBox.TabIndex = 0;
			// 
			// RatesGroupBox
			// 
			this.RatesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.RatesGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("8b54489a-9137-430e-b350-67dd83506299", "Rates");
			this.RatesGroupBox.Controls.Add(this.RatesGrid);
			this.RatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 161, true);
			this.RatesGroupBox.Name = "RatesGroupBox";
			this.RatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 205, true);
			this.RatesGroupBox.TabIndex = 1;
			this.RatesGroupBox.TabStop = false;
			// 
			// RatesGrid
			// 
			this.RatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RatesGrid, "CalculationRuleRateCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).CalculationRuleRateCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusCalculationRuleRate)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).CalculationRuleRateCollection)).SyncRoot)).ValueFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusCalculationRuleRate)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).CalculationRuleRateCollection)).SyncRoot)).FlatRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusCalculationRuleRate)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusCalculationRule)(null)).CalculationRuleRateCollection)).SyncRoot)).Uplift)));
			this.RatesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "ValueFrom";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.IsSortable = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "FlatRate";
			zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo5.IsSortable = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "Uplift";
			zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo6.IsSortable = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			this.RatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.RatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.RatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.RatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RatesGrid.GridId = "4A4A1E37-84A6-4837-A15E-64375F6B3D14";
			this.RatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RatesGrid.LayoutKey = "RatesGrid";
			this.RatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
			this.RatesGrid.Name = "RatesGrid";
			this.RatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(662, 184, true);
			this.RatesGrid.TabIndex = 2;
			// 
			// rulePanel
			// 
			this.rulePanel.BackColor = System.Drawing.SystemColors.Control;
			this.rulePanel.Controls.Add(this.DetailsGroupBox);
			this.rulePanel.Controls.Add(this.RatesGroupBox);
			this.rulePanel.Controls.Add(this.PostingButtonsUserControl);
			this.rulePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rulePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rulePanel.Name = "rulePanel";
			this.rulePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 423, true);
			this.rulePanel.TabIndex = 2;
			// 
			// CusCalculationRuleForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 447, true);
			this.Controls.Add(this.rulePanel);
			this.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusCalculationRule);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 486, true);
			this.Name = "CusCalculationRuleForm";
			this.Text = "CusCalculationRuleForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.rulePanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.EndDateEdit.ResumeLayout(true);
			this.EndDateEdit.PerformLayout();
			this.StartDateEdit.ResumeLayout(true);
			this.StartDateEdit.PerformLayout();
			this.RuleTypeDropEdit.ResumeLayout(true);
			this.RuleTypeDropEdit.PerformLayout();
			this.CurrencyCodeFindBox.ResumeLayout(true);
			this.CurrencyCodeFindBox.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.BasedOnDropEdit.ResumeLayout(true);
			this.BasedOnDropEdit.PerformLayout();
			this.ImporterFindBox.ResumeLayout(true);
			this.ImporterFindBox.PerformLayout();
			this.RatesGroupBox.ResumeLayout(false);
			this.RatesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RatesGrid)).EndInit();
			this.RatesGrid.ResumeLayout(false);
			this.RatesGrid.PerformLayout();
			this.rulePanel.ResumeLayout(false);
			this.rulePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox RatesGroupBox;
		private ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		private ZArchitecture.GUI.ZDropEdit BasedOnDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox ImporterFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CurrencyCodeFindBox;
		private ZArchitecture.GUI.ZDateEdit EndDateEdit;
		private ZArchitecture.GUI.ZDateEdit StartDateEdit;
		private ZArchitecture.GUI.ZDropEdit RuleTypeDropEdit;
		internal ZArchitecture.ZGrid RatesGrid;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.GUI.ZPanel rulePanel;
	}
}
