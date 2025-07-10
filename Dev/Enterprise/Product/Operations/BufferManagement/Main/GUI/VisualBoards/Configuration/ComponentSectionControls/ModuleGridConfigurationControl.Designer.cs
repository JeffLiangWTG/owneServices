namespace Enterprise.BufferManagement.GUI
{
	partial class ModuleGridConfigurationControl
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
			this.SectionAppearanceConfigGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.OverriddenSectionNameCheckedTextBox = new ZArchitecture.GUI.ZOverridableTextBox();
			this.PanelGrid = new ZArchitecture.ZGrid();

			SelectModuleDropEdit = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			SequenceTextBox = new ZArchitecture.ZTextBoxColumnStyleInfo();
			UseCustomPanelNameCheckBox = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			CustomPanelNameTextBox = new ZArchitecture.ZTextBoxColumnStyleInfo();
			SelectFilterLayoutGuidFindBox = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ShowFiltersCheckBox = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			AllowFilterToggleCheckBox = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			AllowOpenModuleCheckBox = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			AllowFilterEditCheckBox = new ZArchitecture.ZCheckBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SectionAppearanceConfigGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.ModuleGridSectionConfiguration);
			// 
			// SectionAppearanceConfigGroupBox
			// 
			this.SectionAppearanceConfigGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("c23d16f0-d754-48e4-98d3-6afb28e95c8b", "Configuration");
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.OverriddenSectionNameCheckedTextBox);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.PanelGrid);
			this.SectionAppearanceConfigGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SectionAppearanceConfigGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SectionAppearanceConfigGroupBox.Name = "SectionAppearanceConfigGroupBox";
			this.SectionAppearanceConfigGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(567, 459, true);
			this.SectionAppearanceConfigGroupBox.TabIndex = 2;
			this.SectionAppearanceConfigGroupBox.TabStop = false;
			// 
			// OverridenSectionNameCheckedTextBox
			// 
			this.OverriddenSectionNameCheckedTextBox.AllowDrop = true;
			this.OverriddenSectionNameCheckedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OverriddenSectionNameCheckedTextBox, "SectionNameOverride");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ModuleGridSectionConfiguration)(null)).SectionNameOverride)));
			this.OverriddenSectionNameCheckedTextBox.BindToForPlaceholderText = "DefaultSectionName";
			this.OverriddenSectionNameCheckedTextBox.BindToForTextIsOverridden = "SectionNameIsOverridden";
			this.OverriddenSectionNameCheckedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 19, true);
			this.OverriddenSectionNameCheckedTextBox.Name = "OverriddenSectionNameCheckedTextBox";
			this.OverriddenSectionNameCheckedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
			this.OverriddenSectionNameCheckedTextBox.MaxLength = 200;
			this.OverriddenSectionNameCheckedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OverriddenSectionNameCheckedTextBox.TabIndex = 0;

			// 
			// PanelGrid
			// 
			this.PanelGrid.AllowNavigation = false;
			this.PanelGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PanelGrid.CaptionVisible = false;
			this.PanelGrid.CopySelectedRowsAllowed = true;

			this.BindingSource.SetBindingMember(this.PanelGrid, "PanelConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ModuleGridSectionConfiguration)(null)).PanelConfigurations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ModuleGridSectionPanelConfiguration)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ModuleGridSectionConfiguration)(null)).PanelConfigurations)).SyncRoot)).ModuleName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.BufferManagement.Business.ModuleGridSectionPanelConfiguration)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ModuleGridSectionConfiguration)(null)).PanelConfigurations)).SyncRoot)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ModuleGridSectionPanelConfiguration)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ModuleGridSectionConfiguration)(null)).PanelConfigurations)).SyncRoot)).SectionNameIsOverridden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ModuleGridSectionPanelConfiguration)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ModuleGridSectionConfiguration)(null)).PanelConfigurations)).SyncRoot)).SectionNameOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.ModuleGridSectionPanelConfiguration)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ModuleGridSectionConfiguration)(null)).PanelConfigurations)).SyncRoot)).FilterLayout)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ModuleGridSectionPanelConfiguration)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ModuleGridSectionConfiguration)(null)).PanelConfigurations)).SyncRoot)).ShowFilters)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ModuleGridSectionPanelConfiguration)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ModuleGridSectionConfiguration)(null)).PanelConfigurations)).SyncRoot)).AllowFilterToggle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ModuleGridSectionPanelConfiguration)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ModuleGridSectionConfiguration)(null)).PanelConfigurations)).SyncRoot)).AllowOpenModule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ModuleGridSectionPanelConfiguration)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.ModuleGridSectionConfiguration)(null)).PanelConfigurations)).SyncRoot)).AllowFilterEdit)));

			SelectModuleDropEdit.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("DEB79FFF-9B6D-4347-B05A-70B5F4DFFC3A", "Module");
			SelectModuleDropEdit.ColumnName = "ModuleName";
			SelectModuleDropEdit.IsMandatory = true;
			SelectModuleDropEdit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			SequenceTextBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("81A902A9-FC30-4AE7-849B-8BFD1C42CA25", "Sequence");
			SequenceTextBox.ColumnName = "Sequence";
			SequenceTextBox.IsMandatory = true;
			SequenceTextBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);

			UseCustomPanelNameCheckBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("A41B2C29-E936-4986-9849-57464A3F012A", "Use Custom Panel Name");
			UseCustomPanelNameCheckBox.ColumnName = "SectionNameIsOverridden";
			UseCustomPanelNameCheckBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			CustomPanelNameTextBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("BB764664-5521-4E72-B536-995D4802ABAF", "Custom Panel Name");
			CustomPanelNameTextBox.ColumnName = "SectionNameOverride";
			CustomPanelNameTextBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			SelectFilterLayoutGuidFindBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("B4AA01FE-FB37-4251-B9B8-0EEA4D881AC8", "Filter Layout");
			SelectFilterLayoutGuidFindBox.ColumnName = "FilterLayout";
			SelectFilterLayoutGuidFindBox.ModuleID = ZArchitecture.Modules.ModuleIDs.StmModuleFilter;
			SelectFilterLayoutGuidFindBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			ShowFiltersCheckBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("0918E080-4DCF-4637-AEAE-F6F667C328C1", "Show Filters");
			ShowFiltersCheckBox.ColumnName = "ShowFilters";
			ShowFiltersCheckBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			AllowFilterToggleCheckBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("16A2A79D-11AA-4730-AC1F-C24B13C39C98", "Allow Filter Toggle");
			AllowFilterToggleCheckBox.ColumnName = "AllowFilterToggle";
			AllowFilterToggleCheckBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			AllowOpenModuleCheckBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("CC2A5010-AA5B-4888-A96B-706EE7B0C0A6", "Allow Open Module");
			AllowOpenModuleCheckBox.ColumnName = "AllowOpenModule";
			AllowOpenModuleCheckBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			AllowFilterEditCheckBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("A6EC730D-0342-45BB-BE01-ABB9DE5C8BCE", "Allow Filter Edit");
			AllowFilterEditCheckBox.ColumnName = "AllowFilterEdit";
			AllowFilterEditCheckBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			this.PanelGrid.ColumnStyles.Add(SelectModuleDropEdit);
			this.PanelGrid.ColumnStyles.Add(SequenceTextBox);
			this.PanelGrid.ColumnStyles.Add(UseCustomPanelNameCheckBox);
			this.PanelGrid.ColumnStyles.Add(CustomPanelNameTextBox);
			this.PanelGrid.ColumnStyles.Add(SelectFilterLayoutGuidFindBox);
			this.PanelGrid.ColumnStyles.Add(ShowFiltersCheckBox);
			this.PanelGrid.ColumnStyles.Add(AllowFilterToggleCheckBox);
			this.PanelGrid.ColumnStyles.Add(AllowOpenModuleCheckBox);
			this.PanelGrid.ColumnStyles.Add(AllowFilterEditCheckBox);
			this.PanelGrid.GridId = "17286226-59BA-49F8-9603-5AEE36D9DF84";
			this.PanelGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PanelGrid.LayoutKey = "PanelGrid";
			this.PanelGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 50, true);
			this.PanelGrid.MaximumRows = 8;
			this.PanelGrid.Name = "PanelGrid";
			this.PanelGrid.ShouldSetErrorsOnTabPage = false;
			this.PanelGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 100, true);
			this.PanelGrid.TabIndex = 1;

			// 
			// ModuleGridConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SectionAppearanceConfigGroupBox);
			this.Name = "ModuleGridConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(567, 459, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SectionAppearanceConfigGroupBox.ResumeLayout(false);
			this.SectionAppearanceConfigGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SectionAppearanceConfigGroupBox;
		private ZArchitecture.GUI.ZOverridableTextBox OverriddenSectionNameCheckedTextBox;
		private ZArchitecture.ZGrid PanelGrid;

		private ZArchitecture.GUI.ZDropEditColumnStyleInfo SelectModuleDropEdit;
		private ZArchitecture.ZTextBoxColumnStyleInfo SequenceTextBox;
		private ZArchitecture.ZCheckBoxColumnStyleInfo UseCustomPanelNameCheckBox;
		private ZArchitecture.ZTextBoxColumnStyleInfo CustomPanelNameTextBox;
		private ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo SelectFilterLayoutGuidFindBox;
		private ZArchitecture.ZCheckBoxColumnStyleInfo ShowFiltersCheckBox;
		private ZArchitecture.ZCheckBoxColumnStyleInfo AllowFilterToggleCheckBox;
		private ZArchitecture.ZCheckBoxColumnStyleInfo AllowOpenModuleCheckBox;
		private ZArchitecture.ZCheckBoxColumnStyleInfo AllowFilterEditCheckBox;
	}
}
