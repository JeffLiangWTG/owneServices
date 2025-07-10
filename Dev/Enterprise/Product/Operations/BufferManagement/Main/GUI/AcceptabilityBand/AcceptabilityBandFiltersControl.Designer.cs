namespace Enterprise.BufferManagement.GUI
{
	partial class AcceptabilityBandFiltersControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FilterSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.FilterStripsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FilterRuleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.filterCustomisationControl = new Enterprise.BufferManagement.GUI.BMFilterStripWrapperControl();
			this.SQLGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SQLTextBox = new Enterprise.ZArchitecture.GUI.ZSqlTextBox();
			this.AcceptabilityBandSQLLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SupersetFilterStripsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SupersetFilterRuleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.supersetFilterCustomisationControl = new Enterprise.BufferManagement.GUI.BMFilterStripWrapperControl();
			this.FilterSettingsFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.filtersByReleaseGroupCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.filtersBySectionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.VisualizationOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VisualizationOptionsSummaryLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FilterSplitContainer)).BeginInit();
			this.FilterSplitContainer.Panel1.SuspendLayout();
			this.FilterSplitContainer.Panel2.SuspendLayout();
			this.FilterSplitContainer.SuspendLayout();
			this.FilterStripsGroupBox.SuspendLayout();
			this.SQLGroupBox.SuspendLayout();
			this.SupersetFilterStripsGroupBox.SuspendLayout();
			this.FilterSettingsFlowLayoutPanel.SuspendLayout();
			this.VisualizationOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand);
			// 
			// FilterSplitContainer
			// 
			this.FilterSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 71, true);
			this.FilterSplitContainer.Name = "FilterSplitContainer";
			this.FilterSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// FilterSplitContainer.Panel1
			// 
			this.FilterSplitContainer.Panel1.Controls.Add(this.FilterStripsGroupBox);
			// 
			// FilterSplitContainer.Panel2
			// 
			this.FilterSplitContainer.Panel2.Controls.Add(this.SQLGroupBox);
			this.FilterSplitContainer.Panel2.Controls.Add(this.SupersetFilterStripsGroupBox);
			this.FilterSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 418, true);
			this.FilterSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(185);
			this.FilterSplitContainer.TabIndex = 8;
			// 
			// FilterStripsGroupBox
			// 
			this.FilterStripsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("2aeb5572-2f14-4263-ae60-d9512232a7cf", "Value Calculation Filters");
			this.FilterStripsGroupBox.Controls.Add(this.FilterRuleLabel);
			this.FilterStripsGroupBox.Controls.Add(this.filterCustomisationControl);
			this.FilterStripsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterStripsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterStripsGroupBox.Name = "FilterStripsGroupBox";
			this.FilterStripsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 185, true);
			this.FilterStripsGroupBox.TabIndex = 5;
			this.FilterStripsGroupBox.TabStop = false;
			// 
			// FilterRuleLabel
			// 
			this.FilterRuleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FilterRuleLabel, "ValueCalculationFilterRuleDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.ResourceString)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).ValueCalculationFilterRuleDescription)));
			this.FilterRuleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 16, true);
			this.FilterRuleLabel.Name = "FilterRuleLabel";
			this.FilterRuleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 25, true);
			this.FilterRuleLabel.TabIndex = 7;
			// 
			// filterCustomisationControl
			// 
			this.filterCustomisationControl.AllowDrop = true;
			this.filterCustomisationControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.filterCustomisationControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.filterCustomisationControl, "FilterRule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.StmModuleFilter)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).FilterRule)));
			this.filterCustomisationControl.FilterControlIdentifier = null;
			this.filterCustomisationControl.IsPreviewAllowed = true;
			this.filterCustomisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 44, true);
			this.filterCustomisationControl.Name = "filterCustomisationControl";
			this.filterCustomisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 138, true);
			this.filterCustomisationControl.TabIndex = 6;
			// 
			// SQLGroupBox
			// 
			this.SQLGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ca62ec81-f6d3-4ef8-8954-cbc33057d5ad", "Acceptability Band SQL");
			this.SQLGroupBox.Controls.Add(this.SQLTextBox);
			this.SQLGroupBox.Controls.Add(this.AcceptabilityBandSQLLabel);
			this.SQLGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SQLGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SQLGroupBox.Name = "SQLGroupBox";
			this.SQLGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 229, true);
			this.SQLGroupBox.TabIndex = 6;
			this.SQLGroupBox.TabStop = false;
			// 
			// SQLTextBox
			// 
			this.SQLTextBox.AcceptsReturn = true;
			this.SQLTextBox.AcceptsTab = true;
			this.SQLTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SQLTextBox, "BAB_SqlText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_SqlText)));
			this.SQLTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SQLTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SQLTextBox, false);
			this.SQLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 81, true);
			this.SQLTextBox.Multiline = true;
			this.SQLTextBox.Name = "SQLTextBox";
			this.SQLTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.SQLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 142, true);
			this.SQLTextBox.TabIndex = 8;
			// 
			// AcceptabilityBandSQLLabel
			// 
			this.AcceptabilityBandSQLLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AcceptabilityBandSQLLabel, "SQLDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.ResourceString)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).SQLDescription)));
			this.AcceptabilityBandSQLLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.AcceptabilityBandSQLLabel.Name = "AcceptabilityBandSQLLabel";
			this.AcceptabilityBandSQLLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 62, true);
			this.AcceptabilityBandSQLLabel.TabIndex = 7;
			// 
			// SupersetFilterStripsGroupBox
			// 
			this.SupersetFilterStripsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("75530126-32a2-499c-b897-34e60b7f5987", "Superset Filters");
			this.SupersetFilterStripsGroupBox.Controls.Add(this.SupersetFilterRuleLabel);
			this.SupersetFilterStripsGroupBox.Controls.Add(this.supersetFilterCustomisationControl);
			this.SupersetFilterStripsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupersetFilterStripsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupersetFilterStripsGroupBox.Name = "SupersetFilterStripsGroupBox";
			this.SupersetFilterStripsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 229, true);
			this.SupersetFilterStripsGroupBox.TabIndex = 5;
			this.SupersetFilterStripsGroupBox.TabStop = false;
			// 
			// SupersetFilterRuleLabel
			// 
			this.SupersetFilterRuleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SupersetFilterRuleLabel, "SupersetFilterRuleDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.ResourceString)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).SupersetFilterRuleDescription)));
			this.SupersetFilterRuleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 16, true);
			this.SupersetFilterRuleLabel.Name = "SupersetFilterRuleLabel";
			this.SupersetFilterRuleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 25, true);
			this.SupersetFilterRuleLabel.TabIndex = 7;
			// 
			// supersetFilterCustomisationControl
			// 
			this.supersetFilterCustomisationControl.AllowDrop = true;
			this.supersetFilterCustomisationControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.supersetFilterCustomisationControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.supersetFilterCustomisationControl, "SupersetItemsFilterRule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.StmModuleFilter)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).SupersetItemsFilterRule)));
			this.supersetFilterCustomisationControl.FilterControlIdentifier = null;
			this.supersetFilterCustomisationControl.IsPreviewAllowed = true;
			this.supersetFilterCustomisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 44, true);
			this.supersetFilterCustomisationControl.Name = "supersetFilterCustomisationControl";
			this.supersetFilterCustomisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 179, true);
			this.supersetFilterCustomisationControl.TabIndex = 6;
			// 
			// FilterSettingsFlowLayoutPanel
			// 
			this.FilterSettingsFlowLayoutPanel.Controls.Add(this.filtersByReleaseGroupCheckbox);
			this.FilterSettingsFlowLayoutPanel.Controls.Add(this.filtersBySectionCheckBox);
			this.FilterSettingsFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 32, true);
			this.FilterSettingsFlowLayoutPanel.Name = "FilterSettingsFlowLayoutPanel";
			this.FilterSettingsFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 24, true);
			this.FilterSettingsFlowLayoutPanel.TabIndex = 10;
			// 
			// filtersByReleaseGroupCheckbox
			// 
			this.filtersByReleaseGroupCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.filtersByReleaseGroupCheckbox, "BAB_FiltersByReleaseGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_FiltersByReleaseGroup)));
			this.filtersByReleaseGroupCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.filtersByReleaseGroupCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.filtersByReleaseGroupCheckbox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 0, true);
			this.filtersByReleaseGroupCheckbox.Name = "filtersByReleaseGroupCheckbox";
			this.filtersByReleaseGroupCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 17, true);
			this.filtersByReleaseGroupCheckbox.TabIndex = 8;
			this.filtersByReleaseGroupCheckbox.UseVisualStyleBackColor = true;
			// 
			// filtersBySectionCheckBox
			// 
			this.filtersBySectionCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.filtersBySectionCheckBox, "BAB_FiltersBySection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_FiltersBySection)));
			this.filtersBySectionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.filtersBySectionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 3, true);
			this.filtersBySectionCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(15, 3, 3, 3, true);
			this.filtersBySectionCheckBox.Name = "filtersBySectionCheckBox";
			this.filtersBySectionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 17, true);
			this.filtersBySectionCheckBox.TabIndex = 9;
			this.filtersBySectionCheckBox.UseVisualStyleBackColor = true;
			// 
			// VisualizationOptionsGroupBox
			// 
			this.VisualizationOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.VisualizationOptionsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("1d1fd97a-d947-4872-b665-daa481a0e0bd", "Visualization Options");
			this.VisualizationOptionsGroupBox.Controls.Add(this.FilterSettingsFlowLayoutPanel);
			this.VisualizationOptionsGroupBox.Controls.Add(this.VisualizationOptionsSummaryLabel);
			this.VisualizationOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.VisualizationOptionsGroupBox.Name = "VisualizationOptionsGroupBox";
			this.VisualizationOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 62, true);
			this.VisualizationOptionsGroupBox.TabIndex = 12;
			this.VisualizationOptionsGroupBox.TabStop = false;
			// 
			// VisualizationOptionsSummaryLabel
			// 
			this.VisualizationOptionsSummaryLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.VisualizationOptionsSummaryLabel, "VisualizationOptionsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.ResourceString)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).VisualizationOptionsDescription)));
			this.VisualizationOptionsSummaryLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("6294a899-b1a8-4258-b54a-b2909fdba112", "Value calculation options for acceptability bands when shown on a visual board");
			this.VisualizationOptionsSummaryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.VisualizationOptionsSummaryLabel.Name = "VisualizationOptionsSummaryLabel";
			this.VisualizationOptionsSummaryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.VisualizationOptionsSummaryLabel.TabIndex = 0;
			// 
			// AcceptabilityBandFiltersControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FilterSplitContainer);
			this.Controls.Add(this.VisualizationOptionsGroupBox);
			this.Name = "AcceptabilityBandFiltersControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 493, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterSplitContainer.Panel1.ResumeLayout(false);
			this.FilterSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.FilterSplitContainer)).EndInit();
			this.FilterSplitContainer.ResumeLayout(false);
			this.FilterSplitContainer.PerformLayout();
			this.FilterStripsGroupBox.ResumeLayout(false);
			this.FilterStripsGroupBox.PerformLayout();
			this.SQLGroupBox.ResumeLayout(false);
			this.SQLGroupBox.PerformLayout();
			this.SupersetFilterStripsGroupBox.ResumeLayout(false);
			this.SupersetFilterStripsGroupBox.PerformLayout();
			this.FilterSettingsFlowLayoutPanel.ResumeLayout(false);
			this.FilterSettingsFlowLayoutPanel.PerformLayout();
			this.VisualizationOptionsGroupBox.ResumeLayout(false);
			this.VisualizationOptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer FilterSplitContainer;
		private ZArchitecture.GUI.ZGroupBox FilterStripsGroupBox;
		private ZArchitecture.GUI.ZGroupBox SupersetFilterStripsGroupBox;
		private ZArchitecture.ZLabel FilterRuleLabel;
		private ZArchitecture.ZLabel SupersetFilterRuleLabel;
		private BMFilterStripWrapperControl filterCustomisationControl;
		private BMFilterStripWrapperControl supersetFilterCustomisationControl;
		private ZArchitecture.GUI.ZGroupBox SQLGroupBox;
		private ZArchitecture.ZLabel AcceptabilityBandSQLLabel;
		private ZArchitecture.GUI.ZSqlTextBox SQLTextBox;
		private ZArchitecture.GUI.ZCheckBox filtersByReleaseGroupCheckbox;
		private ZArchitecture.GUI.ZCheckBox filtersBySectionCheckBox;
		private CargoWise.Windows.UI.KFlowLayoutPanel FilterSettingsFlowLayoutPanel;
		private ZArchitecture.GUI.ZGroupBox VisualizationOptionsGroupBox;
		private ZArchitecture.ZLabel VisualizationOptionsSummaryLabel;
	}
}
