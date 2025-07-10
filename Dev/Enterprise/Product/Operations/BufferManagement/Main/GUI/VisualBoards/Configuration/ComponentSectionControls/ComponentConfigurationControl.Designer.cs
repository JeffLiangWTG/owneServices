namespace Enterprise.BufferManagement.GUI
{
	partial class ComponentConfigurationControl
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
			this.SectionAppearanceConfigGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CardTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PanelLayoutStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.showZonesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.showChildComponentZonesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HideResourcesFromCapabilityChannelsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EnableShowCurrentItemsFilterByDefaultCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsReleaseSchedulerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FadeBackgroundAtPercentageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaxOverdueSlotsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TimeFieldDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TimeProgressionModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShowReleaseGroupWorkCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LastCellDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FlowDirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TimePerCellTimeEditEx = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.zCalcEdit6 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SubsectionsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReleaseGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OverriddenSectionNameCheckedTextBox = new ZArchitecture.GUI.ZOverridableTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SectionAppearanceConfigGroupBox.SuspendLayout();
			this.CardTypeDropEdit.SuspendLayout();
			this.PanelLayoutStyleDropEdit.SuspendLayout();
			this.TimeFieldDropEdit.SuspendLayout();
			this.TimeProgressionModeDropEdit.SuspendLayout();
			this.LastCellDropEdit.SuspendLayout();
			this.FlowDirectionDropEdit.SuspendLayout();
			this.ReleaseGroupFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMComponentSectionConfiguration);
			// 
			// SectionAppearanceConfigGroupBox
			// 
			this.SectionAppearanceConfigGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("c23d16f0-d754-48e4-98d3-6afb28e95c8b", "Configuration");
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.OverriddenSectionNameCheckedTextBox);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.CardTypeDropEdit);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.PanelLayoutStyleDropEdit);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.showZonesCheckBox);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.showChildComponentZonesCheckBox);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.zCheckBox1);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.HideResourcesFromCapabilityChannelsCheckBox);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.EnableShowCurrentItemsFilterByDefaultCheckBox);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.IsReleaseSchedulerCheckBox);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.FadeBackgroundAtPercentageCalcEdit);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.MaxOverdueSlotsCalcEdit);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.TimeFieldDropEdit);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.TimeProgressionModeDropEdit);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.ShowReleaseGroupWorkCheckBox);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.LastCellDropEdit);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.FlowDirectionDropEdit);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.TimePerCellTimeEditEx);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.zCalcEdit6);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.SubsectionsCalcEdit);
			this.SectionAppearanceConfigGroupBox.Controls.Add(this.ReleaseGroupFindBox);
			this.SectionAppearanceConfigGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SectionAppearanceConfigGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SectionAppearanceConfigGroupBox.Name = "SectionAppearanceConfigGroupBox";
			this.SectionAppearanceConfigGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 339, true);
			this.SectionAppearanceConfigGroupBox.TabIndex = 4;
			this.SectionAppearanceConfigGroupBox.TabStop = false;
			// 
			// CardTypeDropEdit
			// 
			this.CardTypeDropEdit.AllowDrop = true;
			this.CardTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.BindingSource.SetBindingMember(this.CardTypeDropEdit, "CardType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).CardType)));
			this.CardTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 123, true);
			this.CardTypeDropEdit.Name = "CardTypeDropEdit";
			this.CardTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 17, true);
			this.CardTypeDropEdit.TabIndex = 10;
			// 
			// showZonesCheckBox
			// 
			this.showZonesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.showZonesCheckBox, "ShowZones");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).ShowZones)));
			this.showZonesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.showZonesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 197, true);
			this.showZonesCheckBox.Name = "showZonesCheckBox";
			this.showZonesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 17, true);
			this.showZonesCheckBox.TabIndex = 16;
			this.showZonesCheckBox.UseVisualStyleBackColor = true;
			// 
			// showChildComponentZonesCheckBox
			// 
			this.showChildComponentZonesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.showChildComponentZonesCheckBox, "ShowChildComponentZones");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).ShowChildComponentZones)));
			this.showChildComponentZonesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.showChildComponentZonesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 197, true);
			this.showChildComponentZonesCheckBox.Name = "showChildComponentZonesCheckBox";
			this.showChildComponentZonesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 17, true);
			this.showChildComponentZonesCheckBox.TabIndex = 17;
			this.showChildComponentZonesCheckBox.UseVisualStyleBackColor = true;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "HideCapabilityTasksFromResourceChannels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).HideCapabilityTasksFromResourceChannels)));
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 147, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.zCheckBox1.TabIndex = 12;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// HideResourcesFromCapabilityChannelsCheckBox
			// 
			this.HideResourcesFromCapabilityChannelsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HideResourcesFromCapabilityChannelsCheckBox, "HideResourceTasksFromCapabilityChannels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).HideResourceTasksFromCapabilityChannels)));
			this.HideResourcesFromCapabilityChannelsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HideResourcesFromCapabilityChannelsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 147, true);
			this.HideResourcesFromCapabilityChannelsCheckBox.Name = "HideResourcesFromCapabilityChannelsCheckBox";
			this.HideResourcesFromCapabilityChannelsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.HideResourcesFromCapabilityChannelsCheckBox.TabIndex = 11;
			this.HideResourcesFromCapabilityChannelsCheckBox.UseVisualStyleBackColor = true;
			// 
			// EnableShowCurrentItemsFilterByDefaultCheckBox
			// 
			this.EnableShowCurrentItemsFilterByDefaultCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EnableShowCurrentItemsFilterByDefaultCheckBox, "EnableShowCurrentItemsFilterByDefault");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).EnableShowCurrentItemsFilterByDefault)));
			this.EnableShowCurrentItemsFilterByDefaultCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableShowCurrentItemsFilterByDefaultCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 170, true);
			this.EnableShowCurrentItemsFilterByDefaultCheckBox.Name = "EnableShowCurrentItemsFilterByDefaultCheckBox";
			this.EnableShowCurrentItemsFilterByDefaultCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 17, true);
			this.EnableShowCurrentItemsFilterByDefaultCheckBox.TabIndex = 14;
			this.EnableShowCurrentItemsFilterByDefaultCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsReleaseSchedulerCheckBox
			// 
			this.IsReleaseSchedulerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsReleaseSchedulerCheckBox, "IsReleaseScheduler");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).IsReleaseScheduler)));
			this.IsReleaseSchedulerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsReleaseSchedulerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 124, true);
			this.IsReleaseSchedulerCheckBox.Name = "IsReleaseSchedulerCheckBox";
			this.IsReleaseSchedulerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.IsReleaseSchedulerCheckBox.TabIndex = 9;
			this.IsReleaseSchedulerCheckBox.UseVisualStyleBackColor = true;
			// 
			// FadeBackgroundAtPercentageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FadeBackgroundAtPercentageCalcEdit, "FadeBackgroundAtPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).FadeBackgroundAtPercentage)));
			this.FadeBackgroundAtPercentageCalcEdit.DecimalPlaces = 2;
			this.FadeBackgroundAtPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 96, true);
			this.FadeBackgroundAtPercentageCalcEdit.Name = "FadeBackgroundAtPercentageCalcEdit";
			this.FadeBackgroundAtPercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.FadeBackgroundAtPercentageCalcEdit.TabIndex = 8;
			this.FadeBackgroundAtPercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaxOverdueSlotsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxOverdueSlotsCalcEdit, "MaxOverdueSlots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).MaxOverdueSlots)));
			this.MaxOverdueSlotsCalcEdit.DecimalPlaces = 2;
			this.MaxOverdueSlotsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 70, true);
			this.MaxOverdueSlotsCalcEdit.Name = "MaxOverdueSlotsCalcEdit";
			this.MaxOverdueSlotsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.MaxOverdueSlotsCalcEdit.TabIndex = 5;
			this.MaxOverdueSlotsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PanelLayoutStyleDropEdit
			// 
			this.PanelLayoutStyleDropEdit.AllowDrop = true;
			this.PanelLayoutStyleDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.BindingSource.SetBindingMember(this.PanelLayoutStyleDropEdit, "PanelLayoutStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).PanelLayoutStyle)));
			this.PanelLayoutStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 226, true);
			this.PanelLayoutStyleDropEdit.Name = "PanelLayoutStyleDropEdit";
			this.PanelLayoutStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true); 
			this.PanelLayoutStyleDropEdit.TabIndex = 18;
			// 
			// TimeProgressionModeDropEdit
			// 
			this.TimeProgressionModeDropEdit.AllowDrop = true;
			this.TimeProgressionModeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.BindingSource.SetBindingMember(this.TimeProgressionModeDropEdit, "TimeProgressionMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).TimeProgressionMode)));
			this.TimeProgressionModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 252, true);
			this.TimeProgressionModeDropEdit.Name = "TimeProgressionModeDropEdit";
			this.TimeProgressionModeDropEdit.PreBoundMaxLength = 3;
			this.TimeProgressionModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
			this.TimeProgressionModeDropEdit.TabIndex = 19;
			// 
			// TimeFieldDropEdit
			// 
			this.TimeFieldDropEdit.AllowDrop = true;
			this.TimeFieldDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.BindingSource.SetBindingMember(this.TimeFieldDropEdit, "TimeField");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).TimeField)));
			this.TimeFieldDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TimeFieldDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 278, true);
			this.TimeFieldDropEdit.Name = "TimeFieldDropEdit";
			this.TimeFieldDropEdit.PreBoundMaxLength = 3;
			this.TimeFieldDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 20, true);
			this.TimeFieldDropEdit.TabIndex = 20;
			// 
			// ShowReleaseGroupWorkCheckBox
			// 
			this.ShowReleaseGroupWorkCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowReleaseGroupWorkCheckBox, "ShowWorkInReleaseGroupOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).ShowWorkInReleaseGroupOnly)));
			this.ShowReleaseGroupWorkCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowReleaseGroupWorkCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 46, true);
			this.ShowReleaseGroupWorkCheckBox.Name = "ShowReleaseGroupWorkCheckBox";
			this.ShowReleaseGroupWorkCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 17, true);
			this.ShowReleaseGroupWorkCheckBox.TabIndex = 2;
			this.ShowReleaseGroupWorkCheckBox.UseVisualStyleBackColor = true;
			// 
			// LastCellDropEdit
			// 
			this.LastCellDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LastCellDropEdit, "LastCell");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).LastCell)));
			this.LastCellDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 96, true);
			this.LastCellDropEdit.Name = "LastCellDropEdit";
			this.LastCellDropEdit.PreBoundMaxLength = 3;
			this.LastCellDropEdit.ShowDescriptionBox = false;
			this.LastCellDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.LastCellDropEdit.TabIndex = 7;
			// 
			// FlowDirectionDropEdit
			// 
			this.FlowDirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FlowDirectionDropEdit, "FlowDirection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).FlowDirection)));
			this.FlowDirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 96, true);
			this.FlowDirectionDropEdit.Name = "FlowDirectionDropEdit";
			this.FlowDirectionDropEdit.PreBoundMaxLength = 3;
			this.FlowDirectionDropEdit.ShowDescriptionBox = false;
			this.FlowDirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.FlowDirectionDropEdit.TabIndex = 6;
			// 
			// TimePerCellTimeEditEx
			// 
			this.TimePerCellTimeEditEx.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TimePerCellTimeEditEx, "TimePerCell");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).TimePerCell)));
			this.TimePerCellTimeEditEx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 197, true);
			this.TimePerCellTimeEditEx.Name = "TimePerCellTimeEditEx";
			this.TimePerCellTimeEditEx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.TimePerCellTimeEditEx.TabIndex = 15;
			// 
			// zCalcEdit6
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit6, "CellsPerSubsection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).CellsPerSubsection)));
			this.zCalcEdit6.DecimalPlaces = 2;
			this.zCalcEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 70, true);
			this.zCalcEdit6.Name = "zCalcEdit6";
			this.zCalcEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.zCalcEdit6.TabIndex = 4;
			this.zCalcEdit6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SubsectionsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SubsectionsCalcEdit, "Subsections");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).Subsections)));
			this.SubsectionsCalcEdit.DecimalPlaces = 2;
			this.SubsectionsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 70, true);
			this.SubsectionsCalcEdit.Name = "SubsectionsCalcEdit";
			this.SubsectionsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.SubsectionsCalcEdit.TabIndex = 3;
			this.SubsectionsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReleaseGroupFindBox
			// 
			this.ReleaseGroupFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReleaseGroupFindBox, "ReleaseGroupPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).ReleaseGroupPK)));
			this.ReleaseGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 44, true);
			this.ReleaseGroupFindBox.Name = "ReleaseGroupFindBox";
			this.ReleaseGroupFindBox.ShowDescriptionBox = false;
			this.ReleaseGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ReleaseGroupFindBox.TabIndex = 1;
			// 
			// OverridenSectionNameCheckedTextBox
			// 
			this.OverriddenSectionNameCheckedTextBox.AllowDrop = true;
			this.OverriddenSectionNameCheckedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.BindingSource.SetBindingMember(this.OverriddenSectionNameCheckedTextBox, "SectionNameOverride");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).SectionNameOverride)));
			this.OverriddenSectionNameCheckedTextBox.BindToForPlaceholderText = "DefaultSectionName";
			this.OverriddenSectionNameCheckedTextBox.BindToForTextIsOverridden = "SectionNameIsOverridden";
			this.OverriddenSectionNameCheckedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 16, true);
			this.OverriddenSectionNameCheckedTextBox.Name = "OverriddenSectionNameCheckedTextBox";
			this.OverriddenSectionNameCheckedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
			this.OverriddenSectionNameCheckedTextBox.MaxLength = 200;
			this.OverriddenSectionNameCheckedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OverriddenSectionNameCheckedTextBox.TabIndex = 0;
			// 
			// ComponentConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SectionAppearanceConfigGroupBox);
			this.Name = "ComponentConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 339, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SectionAppearanceConfigGroupBox.ResumeLayout(false);
			this.SectionAppearanceConfigGroupBox.PerformLayout();
			this.CardTypeDropEdit.ResumeLayout(true);
			this.CardTypeDropEdit.PerformLayout();
			this.PanelLayoutStyleDropEdit.ResumeLayout(true);
			this.PanelLayoutStyleDropEdit.PerformLayout();
			this.TimeFieldDropEdit.ResumeLayout(true);
			this.TimeFieldDropEdit.PerformLayout();
			this.TimeProgressionModeDropEdit.ResumeLayout(true);
			this.TimeProgressionModeDropEdit.PerformLayout();
			this.LastCellDropEdit.ResumeLayout(true);
			this.LastCellDropEdit.PerformLayout();
			this.FlowDirectionDropEdit.ResumeLayout(true);
			this.FlowDirectionDropEdit.PerformLayout();
			this.ReleaseGroupFindBox.ResumeLayout(true);
			this.ReleaseGroupFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SectionAppearanceConfigGroupBox;
		private ZArchitecture.GUI.ZCheckBox zCheckBox1;
		private ZArchitecture.GUI.ZCheckBox HideResourcesFromCapabilityChannelsCheckBox;
		private ZArchitecture.GUI.ZCheckBox EnableShowCurrentItemsFilterByDefaultCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsReleaseSchedulerCheckBox;
		private ZArchitecture.ZCalcEdit FadeBackgroundAtPercentageCalcEdit;
		private ZArchitecture.ZCalcEdit MaxOverdueSlotsCalcEdit;
		private ZArchitecture.GUI.ZDropEdit TimeFieldDropEdit;
		private ZArchitecture.GUI.ZDropEdit TimeProgressionModeDropEdit;
		private ZArchitecture.GUI.ZCheckBox ShowReleaseGroupWorkCheckBox;
		private ZArchitecture.GUI.ZDropEdit LastCellDropEdit;
		private ZArchitecture.GUI.ZDropEdit FlowDirectionDropEdit;
		private ZArchitecture.GUI.ZTimeEditEx TimePerCellTimeEditEx;
		private ZArchitecture.ZCalcEdit zCalcEdit6;
		private ZArchitecture.ZCalcEdit SubsectionsCalcEdit;
		private ZArchitecture.GUI.ZGuidFindBox ReleaseGroupFindBox;
		private ZArchitecture.GUI.ZCheckBox showZonesCheckBox;
		private ZArchitecture.GUI.ZCheckBox showChildComponentZonesCheckBox;
		private ZArchitecture.GUI.ZDropEdit PanelLayoutStyleDropEdit;
		private ZArchitecture.GUI.ZDropEdit CardTypeDropEdit;
		private ZArchitecture.GUI.ZOverridableTextBox OverriddenSectionNameCheckedTextBox;
	}
}
