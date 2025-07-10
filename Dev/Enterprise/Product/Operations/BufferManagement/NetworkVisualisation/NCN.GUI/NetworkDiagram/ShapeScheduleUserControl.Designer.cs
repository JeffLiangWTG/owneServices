namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	partial class ShapeScheduleUserControl
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
			this.ApprovedScheduleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SchedulesOutOfDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.shouldShowNonScheduledSectionCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DurationsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EarliestFinishCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EarliestStartCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LatestStartCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LatestFinishCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TimesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EarliestStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LatestFinishDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EarliestFinishDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LatestStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PlannedDurationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FloatCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CriticalPathCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SynchroniseScheduleWithLinkedEntityCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ApprovedScheduleGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.DurationsPanel.SuspendLayout();
			this.TimesPanel.SuspendLayout();
			this.EarliestStartDateEdit.SuspendLayout();
			this.LatestFinishDateEdit.SuspendLayout();
			this.EarliestFinishDateEdit.SuspendLayout();
			this.LatestStartDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity);
			// 
			// ApprovedScheduleGroupBox
			// 
			this.ApprovedScheduleGroupBox.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("54146e0b-48d1-4e7a-8c5c-d55c5941897f", "Schedule");
			this.ApprovedScheduleGroupBox.Controls.Add(this.SplitContainer);
			this.ApprovedScheduleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApprovedScheduleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApprovedScheduleGroupBox.Name = "ApprovedScheduleGroupBox";
			this.ApprovedScheduleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(858, 216, true);
			this.ApprovedScheduleGroupBox.TabIndex = 0;
			this.ApprovedScheduleGroupBox.TabStop = false;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.IsSplitterFixed = true;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.SchedulesOutOfDateLabel);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.SynchroniseScheduleWithLinkedEntityCheckBox);
			this.SplitContainer.Panel2.Controls.Add(this.shouldShowNonScheduledSectionCheckbox);
			this.SplitContainer.Panel2.Controls.Add(this.DurationsPanel);
			this.SplitContainer.Panel2.Controls.Add(this.TimesPanel);
			this.SplitContainer.Panel2.Controls.Add(this.PlannedDurationCalcEdit);
			this.SplitContainer.Panel2.Controls.Add(this.FloatCalcEdit);
			this.SplitContainer.Panel2.Controls.Add(this.CriticalPathCheckBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 197, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(36);
			this.SplitContainer.TabIndex = 23;
			this.SplitContainer.TabStop = false;
			// 
			// SchedulesOutOfDateLabel
			// 
			this.BindingSource.SetBindingMember(this.SchedulesOutOfDateLabel, "ScheduleHintLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).ScheduleHintLabel)));
			this.SchedulesOutOfDateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SchedulesOutOfDateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SchedulesOutOfDateLabel.ForeColor = System.Drawing.Color.Red;
			this.SchedulesOutOfDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SchedulesOutOfDateLabel.Name = "SchedulesOutOfDateLabel";
			this.SchedulesOutOfDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 36, true);
			this.SchedulesOutOfDateLabel.TabIndex = 6;
			// 
			// shouldShowNonScheduledSectionCheckbox
			// 
			this.shouldShowNonScheduledSectionCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.shouldShowNonScheduledSectionCheckbox, "ShouldShowNonScheduledSection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).ShouldShowNonScheduledSection)));
			this.shouldShowNonScheduledSectionCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.shouldShowNonScheduledSectionCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 103, true);
			this.shouldShowNonScheduledSectionCheckbox.Name = "shouldShowNonScheduledSectionCheckbox";
			this.shouldShowNonScheduledSectionCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 17, true);
			this.shouldShowNonScheduledSectionCheckbox.TabIndex = 23;
			this.shouldShowNonScheduledSectionCheckbox.UseVisualStyleBackColor = true;
			// 
			// DurationsPanel
			// 
			this.DurationsPanel.Controls.Add(this.EarliestFinishCalcEdit);
			this.DurationsPanel.Controls.Add(this.EarliestStartCalcEdit);
			this.DurationsPanel.Controls.Add(this.LatestStartCalcEdit);
			this.DurationsPanel.Controls.Add(this.LatestFinishCalcEdit);
			this.DurationsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DurationsPanel.Name = "DurationsPanel";
			this.DurationsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 50, true);
			this.DurationsPanel.TabIndex = 9;
			// 
			// EarliestFinishCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EarliestFinishCalcEdit, "Shape.EarliestFinishHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.EarliestFinishHours)));
			this.EarliestFinishCalcEdit.DecimalPlaces = 2;
			this.EarliestFinishCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 2, true);
			this.EarliestFinishCalcEdit.Name = "EarliestFinishCalcEdit";
			this.EarliestFinishCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.EarliestFinishCalcEdit.TabIndex = 2;
			this.EarliestFinishCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EarliestStartCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EarliestStartCalcEdit, "Shape.EarliestStartHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.EarliestStartHours)));
			this.EarliestStartCalcEdit.DecimalPlaces = 2;
			this.EarliestStartCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 2, true);
			this.EarliestStartCalcEdit.Name = "EarliestStartCalcEdit";
			this.EarliestStartCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.EarliestStartCalcEdit.TabIndex = 0;
			this.EarliestStartCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LatestStartCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LatestStartCalcEdit, "Shape.LatestStartHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.LatestStartHours)));
			this.LatestStartCalcEdit.DecimalPlaces = 2;
			this.LatestStartCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 28, true);
			this.LatestStartCalcEdit.Name = "LatestStartCalcEdit";
			this.LatestStartCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.LatestStartCalcEdit.TabIndex = 1;
			this.LatestStartCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LatestFinishCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LatestFinishCalcEdit, "Shape.LatestFinishHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.LatestFinishHours)));
			this.LatestFinishCalcEdit.DecimalPlaces = 2;
			this.LatestFinishCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 28, true);
			this.LatestFinishCalcEdit.Name = "LatestFinishCalcEdit";
			this.LatestFinishCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.LatestFinishCalcEdit.TabIndex = 3;
			this.LatestFinishCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TimesPanel
			// 
			this.TimesPanel.Controls.Add(this.EarliestStartDateEdit);
			this.TimesPanel.Controls.Add(this.LatestFinishDateEdit);
			this.TimesPanel.Controls.Add(this.EarliestFinishDateEdit);
			this.TimesPanel.Controls.Add(this.LatestStartDateEdit);
			this.TimesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 3, true);
			this.TimesPanel.Name = "TimesPanel";
			this.TimesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 50, true);
			this.TimesPanel.TabIndex = 10;
			this.TimesPanel.Visible = false;
			// 
			// EarliestStartDateEdit
			// 
			this.EarliestStartDateEdit.AllowDrop = true;
			this.EarliestStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.EarliestStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EarliestStartDateEdit, "Shape.EarliestStartTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.EarliestStartTimeLocal)));
			this.EarliestStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EarliestStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 2, true);
			this.EarliestStartDateEdit.Name = "EarliestStartDateEdit";
			this.EarliestStartDateEdit.TabIndex = 10;
			// 
			// LatestFinishDateEdit
			// 
			this.LatestFinishDateEdit.AllowDrop = true;
			this.LatestFinishDateEdit.AutoCompleteMonthThreshold = 1;
			this.LatestFinishDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LatestFinishDateEdit, "Shape.LatestFinishTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.LatestFinishTimeLocal)));
			this.LatestFinishDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LatestFinishDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 28, true);
			this.LatestFinishDateEdit.Name = "LatestFinishDateEdit";
			this.LatestFinishDateEdit.TabIndex = 13;
			// 
			// EarliestFinishDateEdit
			// 
			this.EarliestFinishDateEdit.AllowDrop = true;
			this.EarliestFinishDateEdit.AutoCompleteMonthThreshold = 1;
			this.EarliestFinishDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EarliestFinishDateEdit, "Shape.EarliestFinishTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.EarliestFinishTimeLocal)));
			this.EarliestFinishDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EarliestFinishDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 2, true);
			this.EarliestFinishDateEdit.Name = "EarliestFinishDateEdit";
			this.EarliestFinishDateEdit.TabIndex = 11;
			// 
			// LatestStartDateEdit
			// 
			this.LatestStartDateEdit.AllowDrop = true;
			this.LatestStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.LatestStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LatestStartDateEdit, "Shape.LatestStartTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.LatestStartTimeLocal)));
			this.LatestStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LatestStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 28, true);
			this.LatestStartDateEdit.Name = "LatestStartDateEdit";
			this.LatestStartDateEdit.TabIndex = 12;
			// 
			// PlannedDurationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PlannedDurationCalcEdit, "Shape.ExplicitDurationHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.ExplicitDurationHours)));
			this.PlannedDurationCalcEdit.DecimalPlaces = 2;
			this.PlannedDurationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 57, true);
			this.PlannedDurationCalcEdit.Name = "PlannedDurationCalcEdit";
			this.PlannedDurationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.PlannedDurationCalcEdit.TabIndex = 20;
			this.PlannedDurationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FloatCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FloatCalcEdit, "Shape.FloatHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.FloatHours)));
			this.FloatCalcEdit.DecimalPlaces = 2;
			this.FloatCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 57, true);
			this.FloatCalcEdit.Name = "FloatCalcEdit";
			this.FloatCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.FloatCalcEdit.TabIndex = 21;
			this.FloatCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CriticalPathCheckBox
			// 
			this.CriticalPathCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CriticalPathCheckBox, "Shape.IsCriticalPath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.IsCriticalPath)));
			this.CriticalPathCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CriticalPathCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 80, true);
			this.CriticalPathCheckBox.Name = "CriticalPathCheckBox";
			this.CriticalPathCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 17, true);
			this.CriticalPathCheckBox.TabIndex = 22;
			this.CriticalPathCheckBox.UseVisualStyleBackColor = true;
			// 
			// SynchroniseScheduleWithLinkedEntityCheckBox
			// 
			this.SynchroniseScheduleWithLinkedEntityCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SynchroniseScheduleWithLinkedEntityCheckBox, "Shape.ShouldSynchroniseScheduleWithLinkedEntity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.ShouldSynchroniseScheduleWithLinkedEntity)));
			this.SynchroniseScheduleWithLinkedEntityCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SynchroniseScheduleWithLinkedEntityCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 126, true);
			this.SynchroniseScheduleWithLinkedEntityCheckBox.Name = "SynchroniseScheduleWithLinkedEntityCheckBox";
			this.SynchroniseScheduleWithLinkedEntityCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.SynchroniseScheduleWithLinkedEntityCheckBox.TabIndex = 24;
			this.SynchroniseScheduleWithLinkedEntityCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShapeScheduleUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ApprovedScheduleGroupBox);
			this.Name = "ShapeScheduleUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(858, 216, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ApprovedScheduleGroupBox.ResumeLayout(false);
			this.ApprovedScheduleGroupBox.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.DurationsPanel.ResumeLayout(false);
			this.DurationsPanel.PerformLayout();
			this.TimesPanel.ResumeLayout(false);
			this.TimesPanel.PerformLayout();
			this.EarliestStartDateEdit.ResumeLayout(true);
			this.EarliestStartDateEdit.PerformLayout();
			this.LatestFinishDateEdit.ResumeLayout(true);
			this.LatestFinishDateEdit.PerformLayout();
			this.EarliestFinishDateEdit.ResumeLayout(true);
			this.EarliestFinishDateEdit.PerformLayout();
			this.LatestStartDateEdit.ResumeLayout(true);
			this.LatestStartDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ApprovedScheduleGroupBox;
		private ZArchitecture.ZCalcEdit EarliestStartCalcEdit;
		private ZArchitecture.ZCalcEdit LatestStartCalcEdit;
		private ZArchitecture.ZCalcEdit LatestFinishCalcEdit;
		private ZArchitecture.ZCalcEdit EarliestFinishCalcEdit;
		private ZArchitecture.ZCalcEdit PlannedDurationCalcEdit;
		private ZArchitecture.ZCalcEdit FloatCalcEdit;
		private ZArchitecture.ZLabel SchedulesOutOfDateLabel;
		private ZArchitecture.GUI.ZCheckBox CriticalPathCheckBox;
		public ZArchitecture.GUI.ZPanel DurationsPanel;
		public ZArchitecture.GUI.ZPanel TimesPanel;
		private ZArchitecture.GUI.ZDateEdit EarliestStartDateEdit;
		private ZArchitecture.GUI.ZDateEdit LatestFinishDateEdit;
		private ZArchitecture.GUI.ZDateEdit EarliestFinishDateEdit;
		private ZArchitecture.GUI.ZDateEdit LatestStartDateEdit;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private ZArchitecture.GUI.ZCheckBox shouldShowNonScheduledSectionCheckbox;
		private ZArchitecture.GUI.ZCheckBox SynchroniseScheduleWithLinkedEntityCheckBox;
	}
}
