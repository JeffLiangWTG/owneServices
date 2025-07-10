namespace Enterprise.BufferManagement.GUI
{
	partial class ApprovedShapeDetailsUserControl
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
			this.DiagramNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ScheduledFinishDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ScheduledStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OpenDiagramButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CriticalPathCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PlannedDurationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ApprovedScheduleGroupBox.SuspendLayout();
			this.ScheduledFinishDateEdit.SuspendLayout();
			this.ScheduledStartDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.ProcessHeader);
			// 
			// ApprovedScheduleGroupBox
			// 
			this.ApprovedScheduleGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("3c3ff896-c9b3-4024-a5ec-98fed06bf497", "Approved Schedule");
			this.ApprovedScheduleGroupBox.Controls.Add(this.DiagramNameTextBox);
			this.ApprovedScheduleGroupBox.Controls.Add(this.ScheduledFinishDateEdit);
			this.ApprovedScheduleGroupBox.Controls.Add(this.ScheduledStartDateEdit);
			this.ApprovedScheduleGroupBox.Controls.Add(this.OpenDiagramButton);
			this.ApprovedScheduleGroupBox.Controls.Add(this.CriticalPathCheckBox);
			this.ApprovedScheduleGroupBox.Controls.Add(this.PlannedDurationCalcEdit);
			this.ApprovedScheduleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApprovedScheduleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApprovedScheduleGroupBox.Name = "ApprovedScheduleGroupBox";
			this.ApprovedScheduleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 151, true);
			this.ApprovedScheduleGroupBox.TabIndex = 0;
			this.ApprovedScheduleGroupBox.TabStop = false;
			// 
			// DiagramNameTextBox
			// 
			this.DiagramNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DiagramNameTextBox, "ApprovedShapeRootDiagramName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).ApprovedShapeRootDiagramName)));
			this.DiagramNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DiagramNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 97, true);
			this.DiagramNameTextBox.Name = "DiagramNameTextBox";
			this.DiagramNameTextBox.ReadOnly = true;
			this.DiagramNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.DiagramNameTextBox.TabIndex = 24;
			// 
			// ScheduledFinishDateEdit
			// 
			this.ScheduledFinishDateEdit.AllowDrop = true;
			this.ScheduledFinishDateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduledFinishDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ScheduledFinishDateEdit, "ApprovedShapeScheduledFinishTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).ApprovedShapeScheduledFinishTimeLocal)));
			this.ScheduledFinishDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ScheduledFinishDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 45, true);
			this.ScheduledFinishDateEdit.Name = "ScheduledFinishDateEdit";
			this.ScheduledFinishDateEdit.TabIndex = 15;
			// 
			// ScheduledStartDateEdit
			// 
			this.ScheduledStartDateEdit.AllowDrop = true;
			this.ScheduledStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduledStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ScheduledStartDateEdit, "ApprovedShapeScheduledStartTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).ApprovedShapeScheduledStartTimeLocal)));
			this.ScheduledStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ScheduledStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 19, true);
			this.ScheduledStartDateEdit.Name = "ScheduledStartDateEdit";
			this.ScheduledStartDateEdit.TabIndex = 14;
			// 
			// OpenDiagramButton
			// 
			this.OpenDiagramButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("0aadab16-afce-45a6-8259-9a2e8a571f34", "Open Diagram");
			this.OpenDiagramButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 123, true);
			this.OpenDiagramButton.Name = "OpenDiagramButton";
			this.OpenDiagramButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 22, true);
			this.OpenDiagramButton.TabIndex = 23;
			this.OpenDiagramButton.UseVisualStyleBackColor = true;
			this.OpenDiagramButton.Click += new System.EventHandler(this.OpenDiagramButton_Click);
			// 
			// CriticalPathCheckBox
			// 
			this.CriticalPathCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CriticalPathCheckBox, "ApprovedShapeIsCriticalPath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).ApprovedShapeIsCriticalPath)));
			this.CriticalPathCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CriticalPathCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 73, true);
			this.CriticalPathCheckBox.Name = "CriticalPathCheckBox";
			this.CriticalPathCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 17, true);
			this.CriticalPathCheckBox.TabIndex = 22;
			this.CriticalPathCheckBox.UseVisualStyleBackColor = true;
			// 
			// PlannedDurationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PlannedDurationCalcEdit, "ApprovedShapeExplicitDurationHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).ApprovedShapeExplicitDurationHours)));
			this.PlannedDurationCalcEdit.DecimalPlaces = 2;
			this.PlannedDurationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 71, true);
			this.PlannedDurationCalcEdit.Name = "PlannedDurationCalcEdit";
			this.PlannedDurationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.PlannedDurationCalcEdit.TabIndex = 20;
			this.PlannedDurationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ApprovedShapeDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ApprovedScheduleGroupBox);
			this.Name = "ApprovedShapeDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 151, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ApprovedScheduleGroupBox.ResumeLayout(false);
			this.ApprovedScheduleGroupBox.PerformLayout();
			this.ScheduledFinishDateEdit.ResumeLayout(true);
			this.ScheduledFinishDateEdit.PerformLayout();
			this.ScheduledStartDateEdit.ResumeLayout(true);
			this.ScheduledStartDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ApprovedScheduleGroupBox;
		private ZArchitecture.ZCalcEdit PlannedDurationCalcEdit;
		private ZArchitecture.GUI.ZCheckBox CriticalPathCheckBox;
		public ZArchitecture.GUI.ZButton OpenDiagramButton;
		private ZArchitecture.GUI.ZDateEdit ScheduledFinishDateEdit;
		private ZArchitecture.GUI.ZDateEdit ScheduledStartDateEdit;
		private ZArchitecture.ZTextBox DiagramNameTextBox;
	}
}
