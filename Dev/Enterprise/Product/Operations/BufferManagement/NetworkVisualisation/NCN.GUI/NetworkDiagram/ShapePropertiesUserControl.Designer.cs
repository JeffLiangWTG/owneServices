using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	partial class ShapePropertiesUserControl
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
			this.ShapePropertiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShapeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ScrollPositionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DisplayCompletenessCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BufferTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ApprovedByFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JobTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ScaledHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.resolutionIncrementTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.scaleTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.isScaledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ScaleDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ScheduleDepartmentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ScheduleBranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ScheduledFinishDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ScheduledStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.NCNOffsetTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.ShapeScheduleUserControl = new Enterprise.BufferManagement.NetworkVisualisation.GUI.ShapeScheduleUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShapePropertiesGroupBox.SuspendLayout();
			this.ScrollPositionDropEdit.SuspendLayout();
			this.BufferTypeDropEdit.SuspendLayout();
			this.ApprovedByFindBox.SuspendLayout();
			this.JobTypeDropEdit.SuspendLayout();
			this.ScaleDetailsGroupBox.SuspendLayout();
			this.ScheduleDepartmentFindBox.SuspendLayout();
			this.ScheduleBranchFindBox.SuspendLayout();
			this.ScheduledFinishDateEdit.SuspendLayout();
			this.ScheduledStartDateEdit.SuspendLayout();
			this.ShapeScheduleUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity);
			// 
			// ShapePropertiesGroupBox
			// 
			this.ShapePropertiesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ShapePropertiesGroupBox.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("e489770e-51b6-4289-8f10-2db706e79d93", "Shape Properties");
			this.ShapePropertiesGroupBox.Controls.Add(this.ShapeNameTextBox);
			this.ShapePropertiesGroupBox.Controls.Add(this.ScrollPositionDropEdit);
			this.ShapePropertiesGroupBox.Controls.Add(this.DisplayCompletenessCheckBox);
			this.ShapePropertiesGroupBox.Controls.Add(this.BufferTypeDropEdit);
			this.ShapePropertiesGroupBox.Controls.Add(this.ApprovedByFindBox);
			this.ShapePropertiesGroupBox.Controls.Add(this.JobTypeDropEdit);
			this.ShapePropertiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.ShapePropertiesGroupBox.Name = "ShapePropertiesGroupBox";
			this.ShapePropertiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(727, 177, true);
			this.ShapePropertiesGroupBox.TabIndex = 0;
			this.ShapePropertiesGroupBox.TabStop = false;
			// 
			// ShapeNameTextBox
			// 
			this.ShapeNameTextBox.AllowDrop = true;
			this.ShapeNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShapeNameTextBox, "Shape.BNS_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.BNS_Name)));
			this.ShapeNameTextBox.CaptionResourceString = null;
			this.ShapeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 24, true);
			this.ShapeNameTextBox.Name = "ShapeNameTextBox";
			this.ShapeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 20, true);
			this.ShapeNameTextBox.TabIndex = 1;
			this.ShapeNameTextBox.MaxLength = AutoBMNCNShape.Schema.BNS_NameMaxLength;
			this.ShapeNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			// 
			// ScrollPositionDropEdit
			// 
			this.ScrollPositionDropEdit.AllowDrop = true;
			this.ScrollPositionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ScrollPositionDropEdit, "ScrollPosition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).ScrollPosition)));
			this.ScrollPositionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 128, true);
			this.ScrollPositionDropEdit.Name = "ScrollPositionDropEdit";
			this.ScrollPositionDropEdit.PreBoundMaxLength = 3;
			this.ScrollPositionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 20, true);
			this.ScrollPositionDropEdit.TabIndex = 7;
			// 
			// DisplayCompletenessCheckBox
			// 
			this.DisplayCompletenessCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DisplayCompletenessCheckBox, "DisplayCompletenessIndicator_ForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).DisplayCompletenessIndicator_ForBinding)));
			this.DisplayCompletenessCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DisplayCompletenessCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 154, true);
			this.DisplayCompletenessCheckBox.Name = "DisplayCompletenessCheckBox";
			this.DisplayCompletenessCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.DisplayCompletenessCheckBox.TabIndex = 8;
			this.DisplayCompletenessCheckBox.UseVisualStyleBackColor = true;
			// 
			// BufferTypeDropEdit
			// 
			this.BufferTypeDropEdit.AllowDrop = true;
			this.BufferTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BufferTypeDropEdit, "Shape.BufferType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.BufferType)));
			this.BufferTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 102, true);
			this.BufferTypeDropEdit.Name = "BufferTypeDropEdit";
			this.BufferTypeDropEdit.PreBoundMaxLength = 3;
			this.BufferTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 20, true);
			this.BufferTypeDropEdit.TabIndex = 6;
			// 
			// ApprovedByFindBox
			// 
			this.ApprovedByFindBox.AllowDrop = true;
			this.ApprovedByFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ApprovedByFindBox, "Shape.BNS_GS_NKApprovedBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.BNS_GS_NKApprovedBy)));
			this.ApprovedByFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 76, true);
			this.ApprovedByFindBox.Name = "ApprovedByFindBox";
			this.ApprovedByFindBox.PreBoundMaxLength = 3;
			this.ApprovedByFindBox.ShouldResize = true;
			this.ApprovedByFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 20, true);
			this.ApprovedByFindBox.TabIndex = 5;
			// 
			// JobTypeDropEdit
			// 
			this.JobTypeDropEdit.AllowDrop = true;
			this.JobTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobTypeDropEdit, "Shape.BNS_JobType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.BNS_JobType)));
			this.JobTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 50, true);
			this.JobTypeDropEdit.Name = "JobTypeDropEdit";
			this.JobTypeDropEdit.PreBoundMaxLength = 3;
			this.JobTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 20, true);
			this.JobTypeDropEdit.TabIndex = 4;
			// 
			// ScaledHintLabel
			// 
			this.ScaledHintLabel.AutoSize = true;
			this.ScaledHintLabel.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("fd8fe903-806f-4abe-87a2-35157a6b7d0c", "To use scaled mode, right-click the diagram surface and select Actions, Switch to Scaled Mode.");
			this.ScaledHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ScaledHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.ScaledHintLabel.Name = "ScaledHintLabel";
			this.ScaledHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 13, true);
			this.ScaledHintLabel.TabIndex = 3;
			this.ScaledHintLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// resolutionIncrementTimeEdit
			// 
			this.resolutionIncrementTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.resolutionIncrementTimeEdit, "ResolutionIncrement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).ResolutionIncrement)));
			this.resolutionIncrementTimeEdit.CaptionResourceString = null;
			this.resolutionIncrementTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 117, true);
			this.resolutionIncrementTimeEdit.Name = "resolutionIncrementTimeEdit";
			this.resolutionIncrementTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.resolutionIncrementTimeEdit.TabIndex = 3;
			// 
			// scaleTimeEdit
			// 
			this.scaleTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.scaleTimeEdit, "Scale");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Scale)));
			this.scaleTimeEdit.CaptionResourceString = null;
			this.scaleTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 65, true);
			this.scaleTimeEdit.Name = "scaleTimeEdit";
			this.scaleTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.scaleTimeEdit.TabIndex = 1;
			// 
			// isScaledCheckBox
			// 
			this.isScaledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isScaledCheckBox, "Shape.IsScaled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.IsScaled)));
			this.isScaledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isScaledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 42, true);
			this.isScaledCheckBox.Name = "isScaledCheckBox";
			this.isScaledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 17, true);
			this.isScaledCheckBox.TabIndex = 0;
			this.isScaledCheckBox.UseVisualStyleBackColor = true;
			// 
			// ScaleDetailsGroupBox
			// 
			this.ScaleDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ScaleDetailsGroupBox.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("d8128e42-ccf2-4cdb-bd77-d5383bab4869", "Diagram Scale");
			this.ScaleDetailsGroupBox.Controls.Add(this.ScheduleDepartmentFindBox);
			this.ScaleDetailsGroupBox.Controls.Add(this.ScheduleBranchFindBox);
			this.ScaleDetailsGroupBox.Controls.Add(this.ScheduledFinishDateEdit);
			this.ScaleDetailsGroupBox.Controls.Add(this.ScheduledStartDateEdit);
			this.ScaleDetailsGroupBox.Controls.Add(this.NCNOffsetTimeEdit);
			this.ScaleDetailsGroupBox.Controls.Add(this.ScaledHintLabel);
			this.ScaleDetailsGroupBox.Controls.Add(this.isScaledCheckBox);
			this.ScaleDetailsGroupBox.Controls.Add(this.resolutionIncrementTimeEdit);
			this.ScaleDetailsGroupBox.Controls.Add(this.scaleTimeEdit);
			this.ScaleDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 185, true);
			this.ScaleDetailsGroupBox.Name = "ScaleDetailsGroupBox";
			this.ScaleDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(727, 146, true);
			this.ScaleDetailsGroupBox.TabIndex = 1;
			this.ScaleDetailsGroupBox.TabStop = false;
			// 
			// ScheduleDepartmentFindBox
			// 
			this.ScheduleDepartmentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ScheduleDepartmentFindBox, "Shape.ScheduleBizo.BNC_GE_Department");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.ScheduleBizo.BNC_GE_Department)));
			this.ScheduleDepartmentFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ScheduleDepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 65, true);
			this.ScheduleDepartmentFindBox.Name = "ScheduleDepartmentFindBox";
			this.ScheduleDepartmentFindBox.ShouldResize = true;
			this.ScheduleDepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ScheduleDepartmentFindBox.TabIndex = 5;
			// 
			// ScheduleBranchFindBox
			// 
			this.ScheduleBranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ScheduleBranchFindBox, "Shape.ScheduleBizo.BNC_GB_Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.ScheduleBizo.BNC_GB_Branch)));
			this.ScheduleBranchFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ScheduleBranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 39, true);
			this.ScheduleBranchFindBox.Name = "ScheduleBranchFindBox";
			this.ScheduleBranchFindBox.ShouldResize = true;
			this.ScheduleBranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ScheduleBranchFindBox.TabIndex = 4;
			// 
			// ScheduledFinishDateEdit
			// 
			this.ScheduledFinishDateEdit.AllowDrop = true;
			this.ScheduledFinishDateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduledFinishDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ScheduledFinishDateEdit, "ScheduledFinishTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).ScheduledFinishTimeLocal)));
			this.ScheduledFinishDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ScheduledFinishDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 117, true);
			this.ScheduledFinishDateEdit.Name = "ScheduledFinishDateEdit";
			this.ScheduledFinishDateEdit.TabIndex = 7;
			// 
			// ScheduledStartDateEdit
			// 
			this.ScheduledStartDateEdit.AllowDrop = true;
			this.ScheduledStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduledStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ScheduledStartDateEdit, "ScheduledStartTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).ScheduledStartTimeLocal)));
			this.ScheduledStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ScheduledStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 91, true);
			this.ScheduledStartDateEdit.Name = "ScheduledStartDateEdit";
			this.ScheduledStartDateEdit.TabIndex = 6;
			// 
			// NCNOffsetTimeEdit
			// 
			this.NCNOffsetTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.NCNOffsetTimeEdit, "Shape.NCNReleaseOffsetTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(null)).Shape.NCNReleaseOffsetTime)));
			this.NCNOffsetTimeEdit.CaptionResourceString = null;
			this.NCNOffsetTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 91, true);
			this.NCNOffsetTimeEdit.Name = "NCNOffsetTimeEdit";
			this.NCNOffsetTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.NCNOffsetTimeEdit.TabIndex = 2;
			// 
			// ShapeScheduleUserControl
			// 
			this.ShapeScheduleUserControl.AllowDrop = true;
			this.ShapeScheduleUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShapeScheduleUserControl, ".");
			this.ShapeScheduleUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 337, true);
			this.ShapeScheduleUserControl.Name = "ShapeScheduleUserControl";
			this.ShapeScheduleUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(727, 207, true);
			this.ShapeScheduleUserControl.TabIndex = 2;
			// 
			// ShapePropertiesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ShapeScheduleUserControl);
			this.Controls.Add(this.ScaleDetailsGroupBox);
			this.Controls.Add(this.ShapePropertiesGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 480, true);
			this.Name = "ShapePropertiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 547, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShapePropertiesGroupBox.ResumeLayout(false);
			this.ShapePropertiesGroupBox.PerformLayout();
			this.ScrollPositionDropEdit.ResumeLayout(true);
			this.ScrollPositionDropEdit.PerformLayout();
			this.BufferTypeDropEdit.ResumeLayout(true);
			this.BufferTypeDropEdit.PerformLayout();
			this.ApprovedByFindBox.ResumeLayout(true);
			this.ApprovedByFindBox.PerformLayout();
			this.JobTypeDropEdit.ResumeLayout(true);
			this.JobTypeDropEdit.PerformLayout();
			this.ScaleDetailsGroupBox.ResumeLayout(false);
			this.ScaleDetailsGroupBox.PerformLayout();
			this.ScheduleDepartmentFindBox.ResumeLayout(true);
			this.ScheduleDepartmentFindBox.PerformLayout();
			this.ScheduleBranchFindBox.ResumeLayout(true);
			this.ScheduleBranchFindBox.PerformLayout();
			this.ScheduledFinishDateEdit.ResumeLayout(true);
			this.ScheduledFinishDateEdit.PerformLayout();
			this.ScheduledStartDateEdit.ResumeLayout(true);
			this.ScheduledStartDateEdit.PerformLayout();
			this.ShapeScheduleUserControl.ResumeLayout(true);
			this.ShapeScheduleUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ShapePropertiesGroupBox;
		private ZArchitecture.GUI.ZCheckBox isScaledCheckBox;
		private ZArchitecture.GUI.ZTimeEditEx scaleTimeEdit;
		private ZArchitecture.GUI.ZTimeEditEx resolutionIncrementTimeEdit;
		private ZArchitecture.ZLabel ScaledHintLabel;
		private ZArchitecture.GUI.ZDropEdit JobTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit ScrollPositionDropEdit;
		private ZArchitecture.GUI.ZGroupBox ScaleDetailsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox ApprovedByFindBox;
		private Enterprise.BufferManagement.NetworkVisualisation.GUI.ShapeScheduleUserControl ShapeScheduleUserControl;
		private ZArchitecture.GUI.ZTimeEditEx NCNOffsetTimeEdit;
		private ZArchitecture.GUI.ZDropEdit BufferTypeDropEdit;
		private ZArchitecture.GUI.ZDateEdit ScheduledFinishDateEdit;
		private ZArchitecture.GUI.ZDateEdit ScheduledStartDateEdit;
		private ZArchitecture.GUI.ZCheckBox DisplayCompletenessCheckBox;
		private ZArchitecture.GUI.ZGuidFindBox ScheduleBranchFindBox;
		private ZArchitecture.GUI.ZGuidFindBox ScheduleDepartmentFindBox;
		private ZArchitecture.ZTextBox ShapeNameTextBox;
	}
}
