namespace Enterprise.BufferManagement.GUI
{
	partial class ScheduleTaskRecurrenceControl
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

		private void InitializeComponent()
		{
			this.recurrenceControl1 = new Enterprise.BufferManagement.GUI.ScheduleRecurrenceControl();
			this.isActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.collectionScheduleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.nextRunTimeLocalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.recurrenceControl1.SuspendLayout();
			this.collectionScheduleGroupBox.SuspendLayout();
			this.nextRunTimeLocalDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Scheduler.Business.StmScheduleTask);
			// 
			// recurrenceControl1
			// 
			this.recurrenceControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.recurrenceControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Scheduler.Business.StmScheduleTask)(((Enterprise.Scheduler.Business.StmScheduleTask)(null)))));
			this.recurrenceControl1.BindTo = null;
			this.recurrenceControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 44, true);
			this.recurrenceControl1.Name = "recurrenceControl1";
			this.recurrenceControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 220, true);
			this.recurrenceControl1.TabIndex = 1;
			// 
			// isActiveCheckBox
			// 
			this.isActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isActiveCheckBox, "S5_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Scheduler.Business.StmScheduleTask)(null)).S5_IsActive)));
			this.isActiveCheckBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("bfb30b8e-2080-49fe-9248-ac2c4d534104", "Is Collection Active", "Is this schedule of collection active. If it is not active this query will not be considered for data collection.");
			this.isActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.isActiveCheckBox.Name = "isActiveCheckBox";
			this.isActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.isActiveCheckBox.TabIndex = 0;
			this.isActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// collectionScheduleGroupBox
			// 
			this.collectionScheduleGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("e987af40-b548-451d-9141-9f8af2339627", "Schedule");
			this.collectionScheduleGroupBox.Controls.Add(this.nextRunTimeLocalDateEdit);
			this.collectionScheduleGroupBox.Controls.Add(this.recurrenceControl1);
			this.collectionScheduleGroupBox.Controls.Add(this.isActiveCheckBox);
			this.collectionScheduleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.collectionScheduleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.collectionScheduleGroupBox.Name = "collectionScheduleGroupBox";
			this.collectionScheduleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 265, true);
			this.collectionScheduleGroupBox.TabIndex = 2;
			this.collectionScheduleGroupBox.TabStop = false;
			// 
			// zDateEdit1
			// 
			this.nextRunTimeLocalDateEdit.AllowDrop = true;
			this.nextRunTimeLocalDateEdit.AutoCompleteMonthThreshold = 1;
			this.nextRunTimeLocalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.nextRunTimeLocalDateEdit, "CalcNextRunTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Scheduler.Business.StmScheduleTask)(null)).CalcNextRunTimeLocal)));
			this.nextRunTimeLocalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.nextRunTimeLocalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 18, true);
			this.nextRunTimeLocalDateEdit.Name = "nextRunTimeLocalDateEdit";
			this.nextRunTimeLocalDateEdit.TabIndex = 2;
			// 
			// ScheduleTaskRecurrenceControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.collectionScheduleGroupBox);
			this.Name = "ScheduleTaskRecurrenceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 265, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.recurrenceControl1.ResumeLayout(true);
			this.recurrenceControl1.PerformLayout();
			this.collectionScheduleGroupBox.ResumeLayout(false);
			this.collectionScheduleGroupBox.PerformLayout();
			this.nextRunTimeLocalDateEdit.ResumeLayout(true);
			this.nextRunTimeLocalDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ScheduleRecurrenceControl recurrenceControl1;
		private ZArchitecture.GUI.ZCheckBox isActiveCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox collectionScheduleGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit nextRunTimeLocalDateEdit;
	}
}
