namespace Enterprise.BufferManagement.GUI
{
	partial class ProcessTemplateReleaseGroupRulesControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo zMacrosFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RulesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.WorkflowTypesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.RulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ApplicableWorkflowCategoriesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ApplicableWorkflowCategoriesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RuleMappingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RuleMappingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ReleaseGroupRulesFallbackDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RulesSplitContainer)).BeginInit();
			this.RulesSplitContainer.Panel1.SuspendLayout();
			this.RulesSplitContainer.Panel2.SuspendLayout();
			this.RulesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WorkflowTypesSplitContainer)).BeginInit();
			this.WorkflowTypesSplitContainer.Panel1.SuspendLayout();
			this.WorkflowTypesSplitContainer.Panel2.SuspendLayout();
			this.WorkflowTypesSplitContainer.SuspendLayout();
			this.RulesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RulesGrid)).BeginInit();
			this.RulesGrid.SuspendLayout();
			this.ApplicableWorkflowCategoriesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApplicableWorkflowCategoriesGrid)).BeginInit();
			this.ApplicableWorkflowCategoriesGrid.SuspendLayout();
			this.RuleMappingGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RuleMappingGrid)).BeginInit();
			this.RuleMappingGrid.SuspendLayout();
			this.ReleaseGroupRulesFallbackDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTaskTemplate);
			// 
			// RulesSplitContainer
			// 
			this.RulesSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RulesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.RulesSplitContainer.Name = "RulesSplitContainer";
			this.RulesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// RulesSplitContainer.Panel1
			// 
			this.RulesSplitContainer.Panel1.Controls.Add(this.WorkflowTypesSplitContainer);
			// 
			// RulesSplitContainer.Panel2
			// 
			this.RulesSplitContainer.Panel2.Controls.Add(this.RuleMappingGroupBox);
			this.RulesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 361, true);
			this.RulesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(165);
			this.RulesSplitContainer.TabIndex = 17;
			// 
			// WorkflowTypesSplitContainer
			// 
			this.WorkflowTypesSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WorkflowTypesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkflowTypesSplitContainer.Name = "WorkflowTypesSplitContainer";
			this.WorkflowTypesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// WorkflowTypesSplitContainer.Panel1
			// 
			this.WorkflowTypesSplitContainer.Panel1.Controls.Add(this.RulesGroupBox);
			// 
			// WorkflowTypesSplitContainer.Panel2
			// 
			this.WorkflowTypesSplitContainer.Panel2.Controls.Add(this.ApplicableWorkflowCategoriesGroupBox);
			this.WorkflowTypesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 161, true);
			this.WorkflowTypesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			this.WorkflowTypesSplitContainer.TabIndex = 0;
			// 
			// RulesGroupBox
			// 
			this.RulesGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("a11b9bd7-42df-436f-b7ac-02ec737583e6", "Release Group Rules");
			this.RulesGroupBox.Controls.Add(this.RulesGrid);
			this.RulesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RulesGroupBox.Name = "RulesGroupBox";
			this.RulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 80, true);
			this.RulesGroupBox.TabIndex = 0;
			this.RulesGroupBox.TabStop = false;
			// 
			// RulesGrid
			// 
			this.RulesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RulesGrid, "ReleaseGroupRules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)).SyncRoot)).PTR_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)).SyncRoot)).PTR_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)).SyncRoot)).PTR_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)).SyncRoot)).PTR_ValueSelectionMacro)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)).SyncRoot)).PTR_AreAllWorkflowCategoriesApplicable)));
			this.RulesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PTR_Sequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "PTR_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "PTR_Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zMacrosFindBoxColumnStyleInfo1.ColumnName = "PTR_ValueSelectionMacro";
			zMacrosFindBoxColumnStyleInfo1.DataFieldsOnly = true;
			zMacrosFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			zCheckBoxColumnStyleInfo2.ColumnName = "PTR_AreAllWorkflowCategoriesApplicable";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.RulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zMacrosFindBoxColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.RulesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RulesGrid.GridId = "79f8bc68-c44f-4899-a679-70088fca3e2e";
			this.RulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RulesGrid.LayoutKey = "MilestonesGrid";
			this.RulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.RulesGrid.Name = "RulesGrid";
			this.RulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 61, true);
			this.RulesGrid.TabIndex = 7;
			// 
			// ApplicableWorkflowCategoriesGroupBox
			// 
			this.ApplicableWorkflowCategoriesGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("6970f2bc-d50e-486d-b2db-bd864a9df88b", "Applicable Workflow Categories");
			this.ApplicableWorkflowCategoriesGroupBox.Controls.Add(this.ApplicableWorkflowCategoriesGrid);
			this.ApplicableWorkflowCategoriesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApplicableWorkflowCategoriesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApplicableWorkflowCategoriesGroupBox.Name = "ApplicableWorkflowTypesGroupBox";
			this.ApplicableWorkflowCategoriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 77, true);
			this.ApplicableWorkflowCategoriesGroupBox.TabIndex = 0;
			this.ApplicableWorkflowCategoriesGroupBox.TabStop = false;
			// 
			// ApplicableWorkflowCategoriesGrid
			// 
			this.ApplicableWorkflowCategoriesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ApplicableWorkflowCategoriesGrid, "ReleaseGroupRules.Categories");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)).SyncRoot)).Categories)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRuleCategory)(((System.Collections.IList)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)).SyncRoot)).Categories)).SyncRoot)).PTC_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRuleCategory)(((System.Collections.IList)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)).SyncRoot)).Categories)).SyncRoot)).CategoryDescription)));
			this.ApplicableWorkflowCategoriesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "PTC_Category";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "CategoryDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.ApplicableWorkflowCategoriesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ApplicableWorkflowCategoriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ApplicableWorkflowCategoriesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApplicableWorkflowCategoriesGrid.GridId = "de54259e-8ed0-41d1-a3ee-27a808fea5f1";
			this.ApplicableWorkflowCategoriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApplicableWorkflowCategoriesGrid.LayoutKey = "TagsGrid";
			this.ApplicableWorkflowCategoriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ApplicableWorkflowCategoriesGrid.Name = "ApplicableWorkflowTypesGrid";
			this.ApplicableWorkflowCategoriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 58, true);
			this.ApplicableWorkflowCategoriesGrid.TabIndex = 0;
			// 
			// RuleMappingGroupBox
			// 
			this.RuleMappingGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("8a998368-575e-4cf5-bb41-4e758ee0c6ff", "Value to Group Mapping");
			this.RuleMappingGroupBox.Controls.Add(this.RuleMappingGrid);
			this.RuleMappingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RuleMappingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RuleMappingGroupBox.Name = "RuleMappingGroupBox";
			this.RuleMappingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 177, true);
			this.RuleMappingGroupBox.TabIndex = 0;
			this.RuleMappingGroupBox.TabStop = false;
			// 
			// RuleMappingGrid
			// 
			this.RuleMappingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RuleMappingGrid, "ReleaseGroupRules.GroupMappings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)).SyncRoot)).GroupMappings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRuleMapping)(((System.Collections.IList)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)).SyncRoot)).GroupMappings)).SyncRoot)).PTM_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRuleMapping)(((System.Collections.IList)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)).SyncRoot)).GroupMappings)).SyncRoot)).PTM_GG_Group)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRuleMapping)(((System.Collections.IList)(((Enterprise.BufferManagement.Integration.IProcessTemplateReleaseGroupRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).ReleaseGroupRules)).SyncRoot)).GroupMappings)).SyncRoot)).ReleaseGroup.GG_Desc)));
			this.RuleMappingGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "PTM_Value";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PTM_GG_Group";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo4.ColumnName = "ReleaseGroup+GG_Desc";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.RuleMappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RuleMappingGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RuleMappingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RuleMappingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RuleMappingGrid.GridId = "ad2e6535-ebe2-4bc4-b87f-c7f290999de2";
			this.RuleMappingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RuleMappingGrid.LayoutKey = "TagsGrid";
			this.RuleMappingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.RuleMappingGrid.Name = "RuleMappingGrid";
			this.RuleMappingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 158, true);
			this.RuleMappingGrid.TabIndex = 0;
			// 
			// ReleaseGroupRulesFallbackDropEdit
			// 
			this.ReleaseGroupRulesFallbackDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReleaseGroupRulesFallbackDropEdit, "P0_ReleaseGroupFallbackMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessTaskTemplate)(null)).P0_ReleaseGroupFallbackMethod)));
			this.ReleaseGroupRulesFallbackDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 3, true);
			this.ReleaseGroupRulesFallbackDropEdit.Name = "ReleaseGroupRulesFallbackDropEdit";
			this.ReleaseGroupRulesFallbackDropEdit.ShouldResizeByMaxLength = true;
			this.ReleaseGroupRulesFallbackDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ReleaseGroupRulesFallbackDropEdit.TabIndex = 18;
			// 
			// ProcessTemplateReleaseGroupRulesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReleaseGroupRulesFallbackDropEdit);
			this.Controls.Add(this.RulesSplitContainer);
			this.Name = "ProcessTemplateReleaseGroupRulesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 401, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RulesSplitContainer.Panel1.ResumeLayout(false);
			this.RulesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RulesSplitContainer)).EndInit();
			this.RulesSplitContainer.ResumeLayout(false);
			this.RulesSplitContainer.PerformLayout();
			this.WorkflowTypesSplitContainer.Panel1.ResumeLayout(false);
			this.WorkflowTypesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.WorkflowTypesSplitContainer)).EndInit();
			this.WorkflowTypesSplitContainer.ResumeLayout(false);
			this.WorkflowTypesSplitContainer.PerformLayout();
			this.RulesGroupBox.ResumeLayout(false);
			this.RulesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RulesGrid)).EndInit();
			this.RulesGrid.ResumeLayout(false);
			this.RulesGrid.PerformLayout();
			this.ApplicableWorkflowCategoriesGroupBox.ResumeLayout(false);
			this.ApplicableWorkflowCategoriesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApplicableWorkflowCategoriesGrid)).EndInit();
			this.ApplicableWorkflowCategoriesGrid.ResumeLayout(false);
			this.ApplicableWorkflowCategoriesGrid.PerformLayout();
			this.RuleMappingGroupBox.ResumeLayout(false);
			this.RuleMappingGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RuleMappingGrid)).EndInit();
			this.RuleMappingGrid.ResumeLayout(false);
			this.RuleMappingGrid.PerformLayout();
			this.ReleaseGroupRulesFallbackDropEdit.ResumeLayout(true);
			this.ReleaseGroupRulesFallbackDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer RulesSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer WorkflowTypesSplitContainer;
		private ZArchitecture.GUI.ZGroupBox RulesGroupBox;
		public ZArchitecture.ZGrid RulesGrid;
		private ZArchitecture.GUI.ZGroupBox ApplicableWorkflowCategoriesGroupBox;
		public ZArchitecture.ZGrid ApplicableWorkflowCategoriesGrid;
		private ZArchitecture.GUI.ZGroupBox RuleMappingGroupBox;
		private ZArchitecture.ZGrid RuleMappingGrid;
		private ZArchitecture.GUI.ZDropEdit ReleaseGroupRulesFallbackDropEdit;
	}
}
