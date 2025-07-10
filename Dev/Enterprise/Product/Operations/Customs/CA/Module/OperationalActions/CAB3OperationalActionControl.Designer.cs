namespace Enterprise.Customs.CA.Module.OperationalActions
{
	partial class CAB3OperationalActionControl
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
		void InitializeComponent()
		{
			this.CAB3SendingOptionsGroupBOx = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MergeEntriesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SuppressNotificationPopoutCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AutoRecalculateDutyAndTaxCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IgnoreAllWarningsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DefaultScheduleActionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CAB3SendingOptionsGroupBOx.SuspendLayout();
			this.DefaultScheduleActionDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Module.OperationalActions.CAB3OperationalActionMethodApplicator);
			// 
			// CAB3SendingOptionsGroupBOx
			// 
			this.CAB3SendingOptionsGroupBOx.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("ae0704f3-56d7-42cd-9173-ad1f85413913", "CA CAD Sending Options");
			this.CAB3SendingOptionsGroupBOx.Controls.Add(this.DefaultScheduleActionDropEdit);
			this.CAB3SendingOptionsGroupBOx.Controls.Add(this.MergeEntriesCheckBox);
			this.CAB3SendingOptionsGroupBOx.Controls.Add(this.SuppressNotificationPopoutCheckBox);
			this.CAB3SendingOptionsGroupBOx.Controls.Add(this.AutoRecalculateDutyAndTaxCheckBox);
			this.CAB3SendingOptionsGroupBOx.Controls.Add(this.IgnoreAllWarningsCheckBox);
			this.CAB3SendingOptionsGroupBOx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.CAB3SendingOptionsGroupBOx.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CAB3SendingOptionsGroupBOx.Name = "CAB3SendingOptionsGroupBOx";
			this.CAB3SendingOptionsGroupBOx.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CAB3SendingOptionsGroupBOx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 193, true);
			this.CAB3SendingOptionsGroupBOx.TabIndex = 0;
			this.CAB3SendingOptionsGroupBOx.TabStop = false;
			//
			// MergeEntriesCheckBox
			//
			this.MergeEntriesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MergeEntriesCheckBox, "MergeEntries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Module.OperationalActions.CAB3OperationalActionMethodApplicator)(null)).MergeEntries)));
			this.MergeEntriesCheckBox.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("5B095013-7AD1-4F95-BC19-ED434FAC273A", "Merge Entries");
			this.MergeEntriesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MergeEntriesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 123, true);
			this.MergeEntriesCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MergeEntriesCheckBox.Name = "MergeEntriesCheckBox";
			this.MergeEntriesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.MergeEntriesCheckBox.TabIndex = 3;
			this.MergeEntriesCheckBox.UseVisualStyleBackColor = true;
			// 
			// SuppressNotificationPopoutCheckBox
			// 
			this.SuppressNotificationPopoutCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SuppressNotificationPopoutCheckBox, "SuppressNotificationPopout");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Module.OperationalActions.CAB3OperationalActionMethodApplicator)(null)).SuppressNotificationPopout)));
			this.SuppressNotificationPopoutCheckBox.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("c299292f-6023-4d26-a92c-710141ed122c", "Suppress Notification Dialog from popping-out");
			this.SuppressNotificationPopoutCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SuppressNotificationPopoutCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 92, true);
			this.SuppressNotificationPopoutCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SuppressNotificationPopoutCheckBox.Name = "SuppressNotificationPopoutCheckBox";
			this.SuppressNotificationPopoutCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.SuppressNotificationPopoutCheckBox.TabIndex = 2;
			this.SuppressNotificationPopoutCheckBox.UseVisualStyleBackColor = true;
			// 
			// AutoRecalculateDutyAndTaxCheckBox
			// 
			this.AutoRecalculateDutyAndTaxCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoRecalculateDutyAndTaxCheckBox, "AutoRecalculateDutyAndTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Module.OperationalActions.CAB3OperationalActionMethodApplicator)(null)).AutoRecalculateDutyAndTax)));
			this.AutoRecalculateDutyAndTaxCheckBox.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("580c5303-474e-43ee-b1ac-a7ae1eb69b1c", "Automatically recalculate Duty and Tax if required");
			this.AutoRecalculateDutyAndTaxCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutoRecalculateDutyAndTaxCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 61, true);
			this.AutoRecalculateDutyAndTaxCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AutoRecalculateDutyAndTaxCheckBox.Name = "AutoRecalculateDutyAndTaxCheckBox";
			this.AutoRecalculateDutyAndTaxCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.AutoRecalculateDutyAndTaxCheckBox.TabIndex = 1;
			this.AutoRecalculateDutyAndTaxCheckBox.UseVisualStyleBackColor = true;
			// 
			// IgnoreAllWarningsCheckBox
			// 
			this.IgnoreAllWarningsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IgnoreAllWarningsCheckBox, "IgnoreAllWarnings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Module.OperationalActions.CAB3OperationalActionMethodApplicator)(null)).IgnoreAllWarnings)));
			this.IgnoreAllWarningsCheckBox.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("1D4DCD9D-4227-4C59-88F3-EB15004B6BBF", "Ignore warnings and continue sending", "Ignore all warnings and continue sending (e.g. Validation Message Errors, Awaiting for responses warning)");
			this.IgnoreAllWarningsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IgnoreAllWarningsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 31, true);
			this.IgnoreAllWarningsCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.IgnoreAllWarningsCheckBox.Name = "IgnoreAllWarningsCheckBox";
			this.IgnoreAllWarningsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.IgnoreAllWarningsCheckBox.TabIndex = 0;
			this.IgnoreAllWarningsCheckBox.UseVisualStyleBackColor = true;
			// 
			// DefaultScheduleActionDropEdit
			// 
			this.DefaultScheduleActionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultScheduleActionDropEdit, "DefaultScheduleActionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Module.OperationalActions.CAB3OperationalActionMethodApplicator)(null)).DefaultScheduleActionCode)));
			this.DefaultScheduleActionDropEdit.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("fb1d4c5b-ed5f-4891-b6f5-92f35636809d", "Default Action if the due date for accounting of job appears to fall after the monthly ARL cut-off date");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DefaultScheduleActionDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.DefaultScheduleActionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 166, true);
			this.DefaultScheduleActionDropEdit.Name = "DefaultScheduleActionDropEdit";
			this.DefaultScheduleActionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DefaultScheduleActionDropEdit.TabIndex = 4;
			// 
			// CAB3OperationalActionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CAB3SendingOptionsGroupBOx);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Name = "CAB3OperationalActionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 206, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CAB3SendingOptionsGroupBOx.ResumeLayout(false);
			this.CAB3SendingOptionsGroupBOx.PerformLayout();
			this.DefaultScheduleActionDropEdit.ResumeLayout(true);
			this.DefaultScheduleActionDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CAB3SendingOptionsGroupBOx;
		private ZArchitecture.GUI.ZCheckBox IgnoreAllWarningsCheckBox;
		private ZArchitecture.GUI.ZCheckBox AutoRecalculateDutyAndTaxCheckBox;
		private ZArchitecture.GUI.ZCheckBox SuppressNotificationPopoutCheckBox;
		private ZArchitecture.GUI.ZCheckBox MergeEntriesCheckBox;
		private ZArchitecture.GUI.ZDropEdit DefaultScheduleActionDropEdit;
	}
}
