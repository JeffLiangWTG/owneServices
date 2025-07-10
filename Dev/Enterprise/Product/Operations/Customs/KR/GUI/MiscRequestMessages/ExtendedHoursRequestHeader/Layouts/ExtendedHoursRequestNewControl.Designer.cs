namespace Enterprise.Customs.KR.GUI
{
	partial class ExtendedHoursRequestNewControl
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
            this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CustomsDivisionCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.RequestReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.RequestPeriodStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.RequestPeriodEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.MessageTypeDropEdit.SuspendLayout();
            this.CustomsOfficeCodeFindBox.SuspendLayout();
            this.CustomsDivisionCodeFindBox.SuspendLayout();
            this.BranchGuidFindBox.SuspendLayout();
            this.RequestPeriodStartDateEdit.SuspendLayout();
            this.RequestPeriodEndDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader);
            // 
            // MessageTypeDropEdit
            // 
            this.MessageTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.MessageTypeDropEdit, "MessageType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader)(null)).MessageType)));
            this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 8, true);
            this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
            this.MessageTypeDropEdit.PreBoundMaxLength = 3;
            this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
            this.MessageTypeDropEdit.TabIndex = 0;
            // 
            // CustomsOfficeCodeFindBox
            // 
            this.CustomsOfficeCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "CustomsOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader)(null)).CustomsOffice)));
            this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 30, true);
            this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
            this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CustomsOfficeCodeFindBox.ParentType = null;
            this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 3;
            this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
            this.CustomsOfficeCodeFindBox.TabIndex = 1;
            // 
            // CustomsDivisionCodeFindBox
            // 
            this.CustomsDivisionCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CustomsDivisionCodeFindBox, "Department");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader)(null)).Department)));
            this.CustomsDivisionCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 53, true);
            this.CustomsDivisionCodeFindBox.Name = "CustomsDivisionCodeFindBox";
            this.CustomsDivisionCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CustomsDivisionCodeFindBox.ParentType = null;
            this.CustomsDivisionCodeFindBox.PreBoundMaxLength = 3;
            this.CustomsDivisionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
            this.CustomsDivisionCodeFindBox.TabIndex = 2;
            // 
            // RequestReasonTextBox
            // 
            this.RequestReasonTextBox.AcceptsReturn = true;
            this.BindingSource.SetBindingMember(this.RequestReasonTextBox, "Reason");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader)(null)).Reason)));
            this.RequestReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(461, 8, true);
            this.RequestReasonTextBox.Multiline = true;
            this.RequestReasonTextBox.Name = "RequestReasonTextBox";
            this.RequestReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.RequestReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 138, true);
            this.RequestReasonTextBox.TabIndex = 6;
            // 
            // BranchGuidFindBox
            // 
            this.BranchGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "BranchPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader)(null)).BranchPK)));
            this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 121, true);
            this.BranchGuidFindBox.Name = "BranchGuidFindBox";
            this.BranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BranchGuidFindBox.ParentType = null;
            this.BranchGuidFindBox.PreBoundMaxLength = 3;
            this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
            this.BranchGuidFindBox.TabIndex = 5;
            // 
            // RequestPeriodStartDateEdit
            // 
            this.RequestPeriodStartDateEdit.AllowDrop = true;
            this.RequestPeriodStartDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.RequestPeriodStartDateEdit, "StartDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader)(null)).StartDate)));
            this.RequestPeriodStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.RequestPeriodStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 75, true);
            this.RequestPeriodStartDateEdit.Name = "RequestPeriodStartDateEdit";
            this.RequestPeriodStartDateEdit.TabIndex = 3;
            // 
            // RequestPeriodEndDateEdit
            // 
            this.RequestPeriodEndDateEdit.AllowDrop = true;
            this.RequestPeriodEndDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.RequestPeriodEndDateEdit, "EndDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.ExtendedHoursRequestHeader)(null)).EndDate)));
            this.RequestPeriodEndDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.RequestPeriodEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 98, true);
            this.RequestPeriodEndDateEdit.Name = "RequestPeriodEndDateEdit";
            this.RequestPeriodEndDateEdit.TabIndex = 4;
            // 
            // ExtendedHoursRequestNewControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.RequestPeriodEndDateEdit);
            this.Controls.Add(this.RequestPeriodStartDateEdit);
            this.Controls.Add(this.BranchGuidFindBox);
            this.Controls.Add(this.RequestReasonTextBox);
            this.Controls.Add(this.CustomsDivisionCodeFindBox);
            this.Controls.Add(this.CustomsOfficeCodeFindBox);
            this.Controls.Add(this.MessageTypeDropEdit);
            this.Name = "ExtendedHoursRequestNewControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(797, 148, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.MessageTypeDropEdit.ResumeLayout(true);
            this.MessageTypeDropEdit.PerformLayout();
            this.CustomsOfficeCodeFindBox.ResumeLayout(true);
            this.CustomsOfficeCodeFindBox.PerformLayout();
            this.CustomsDivisionCodeFindBox.ResumeLayout(true);
            this.CustomsDivisionCodeFindBox.PerformLayout();
            this.BranchGuidFindBox.ResumeLayout(true);
            this.BranchGuidFindBox.PerformLayout();
            this.RequestPeriodStartDateEdit.ResumeLayout(true);
            this.RequestPeriodStartDateEdit.PerformLayout();
            this.RequestPeriodEndDateEdit.ResumeLayout(true);
            this.RequestPeriodEndDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZDropEdit MessageTypeDropEdit;
		public ZArchitecture.GUI.ZCodeFindBox CustomsDivisionCodeFindBox;
		public ZArchitecture.ZTextBox RequestReasonTextBox;
		public ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		public ZArchitecture.GUI.ZDateEdit RequestPeriodStartDateEdit;
		public ZArchitecture.GUI.ZDateEdit RequestPeriodEndDateEdit;
		public ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
	}
}
