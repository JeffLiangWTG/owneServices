namespace Enterprise.Customs.KR.GUI
{
	partial class CusMiscRequestHeaderViewControl
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
      this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
      this.RequestDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
      this.ApplicationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
      this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.RequestDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
      this.EntryCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
      this.CustomsDivisionCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
      this.CustomsReviewStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.ReviewDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.MessageTypeDropEdit.SuspendLayout();
      this.CustomsOfficeCodeFindBox.SuspendLayout();
      this.BranchGuidFindBox.SuspendLayout();
      this.StatusDropEdit.SuspendLayout();
      this.RequestDateEdit.SuspendLayout();
      this.CustomsDivisionCodeFindBox.SuspendLayout();
      this.CustomsReviewStatusDropEdit.SuspendLayout();
      this.ReviewDateEdit.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusMiscRequestHeader);
      // 
      // MessageTypeDropEdit
      // 
      this.MessageTypeDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.MessageTypeDropEdit, "CMR_MessageType");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).CMR_MessageType)));
      this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 16, true);
      this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
      this.MessageTypeDropEdit.PreBoundMaxLength = 3;
      this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
      this.MessageTypeDropEdit.TabIndex = 0;
      // 
      // CustomsOfficeCodeFindBox
      // 
      this.CustomsOfficeCodeFindBox.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "CustomsOffice");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).CustomsOffice)));
      this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 39, true);
      this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
      this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
      this.CustomsOfficeCodeFindBox.ParentType = null;
      this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 3;
      this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
      this.CustomsOfficeCodeFindBox.TabIndex = 1;
      // 
      // BranchGuidFindBox
      // 
      this.BranchGuidFindBox.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "CMR_GB");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).CMR_GB)));
      this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 84, true);
      this.BranchGuidFindBox.Name = "BranchGuidFindBox";
      this.BranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
      this.BranchGuidFindBox.ParentType = null;
      this.BranchGuidFindBox.PreBoundMaxLength = 3;
      this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
      this.BranchGuidFindBox.TabIndex = 3;
      // 
      // RequestDetailsTextBox
      // 
      this.BindingSource.SetBindingMember(this.RequestDetailsTextBox, "CMR_RequestDetails");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).CMR_RequestDetails)));
      this.RequestDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 107, true);
      this.RequestDetailsTextBox.Multiline = true;
      this.RequestDetailsTextBox.Name = "RequestDetailsTextBox";
      this.RequestDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 72, true);
      this.RequestDetailsTextBox.TabIndex = 4;
      // 
      // ApplicationNumberTextBox
      // 
      this.BindingSource.SetBindingMember(this.ApplicationNumberTextBox, "FormattedApplicationNumber");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).FormattedApplicationNumber)));
      this.ApplicationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 16, true);
      this.ApplicationNumberTextBox.Name = "ApplicationNumberTextBox";
      this.ApplicationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
      this.ApplicationNumberTextBox.TabIndex = 5;
      // 
      // StatusDropEdit
      // 
      this.StatusDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.StatusDropEdit, "CMR_Status");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).CMR_Status)));
      this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 39, true);
      this.StatusDropEdit.Name = "StatusDropEdit";
      this.StatusDropEdit.PreBoundMaxLength = 3;
      this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
      this.StatusDropEdit.TabIndex = 6;
      // 
      // RequestDateEdit
      // 
      this.RequestDateEdit.AllowDrop = true;
      this.RequestDateEdit.AutoCompleteMonthThreshold = 1;
      this.BindingSource.SetBindingMember(this.RequestDateEdit, "CMR_RequestDate");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).CMR_RequestDate)));
      this.RequestDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 61, true);
      this.RequestDateEdit.Name = "RequestDateEdit";
      this.RequestDateEdit.TabIndex = 7;
      // 
      // EntryCountCalcEdit
      // 
      this.BindingSource.SetBindingMember(this.EntryCountCalcEdit, "LinesCount");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).LinesCount)));
      this.EntryCountCalcEdit.DecimalPlaces = 2;
      this.EntryCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 84, true);
      this.EntryCountCalcEdit.Name = "EntryCountCalcEdit";
      this.EntryCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 15, true);
      this.EntryCountCalcEdit.TabIndex = 8;
      this.EntryCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.EntryCountCalcEdit.TrackDisposedAccess = true;
      // 
      // CustomsDivisionCodeFindBox
      // 
      this.CustomsDivisionCodeFindBox.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.CustomsDivisionCodeFindBox, "CustomsDivision");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).CustomsDivision)));
      this.CustomsDivisionCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 61, true);
      this.CustomsDivisionCodeFindBox.Name = "CustomsDivisionCodeFindBox";
      this.CustomsDivisionCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
      this.CustomsDivisionCodeFindBox.ParentType = null;
      this.CustomsDivisionCodeFindBox.PreBoundMaxLength = 3;
      this.CustomsDivisionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
      this.CustomsDivisionCodeFindBox.TabIndex = 2;
      // 
      // CustomsReviewStatusDropEdit
      // 
      this.CustomsReviewStatusDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.CustomsReviewStatusDropEdit, "CustomsReviewStatus");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).CustomsReviewStatus)));
      this.CustomsReviewStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 107, true);
      this.CustomsReviewStatusDropEdit.Name = "CustomsReviewStatusDropEdit";
      this.CustomsReviewStatusDropEdit.PreBoundMaxLength = 3;
      this.CustomsReviewStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
      this.CustomsReviewStatusDropEdit.TabIndex = 9;
      // 
      // ReviewDateEdit
      // 
      this.ReviewDateEdit.AllowDrop = true;
      this.ReviewDateEdit.AutoCompleteMonthThreshold = 1;
      this.BindingSource.SetBindingMember(this.ReviewDateEdit, "ReviewDate5SG");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusMiscRequestHeader)(null)).ReviewDate5SG)));
      this.ReviewDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 131, true);
      this.ReviewDateEdit.Name = "ReviewDateEdit";
      this.ReviewDateEdit.TabIndex = 10;
      // 
      // ExtendedHoursRequestViewControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.Controls.Add(this.ReviewDateEdit);
      this.Controls.Add(this.CustomsReviewStatusDropEdit);
      this.Controls.Add(this.CustomsDivisionCodeFindBox);
      this.Controls.Add(this.EntryCountCalcEdit);
      this.Controls.Add(this.RequestDateEdit);
      this.Controls.Add(this.StatusDropEdit);
      this.Controls.Add(this.ApplicationNumberTextBox);
      this.Controls.Add(this.RequestDetailsTextBox);
      this.Controls.Add(this.BranchGuidFindBox);
      this.Controls.Add(this.CustomsOfficeCodeFindBox);
      this.Controls.Add(this.MessageTypeDropEdit);
      this.Name = "ExtendedHoursRequestViewControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(797, 189, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.MessageTypeDropEdit.ResumeLayout(true);
      this.MessageTypeDropEdit.PerformLayout();
      this.CustomsOfficeCodeFindBox.ResumeLayout(true);
      this.CustomsOfficeCodeFindBox.PerformLayout();
      this.BranchGuidFindBox.ResumeLayout(true);
      this.BranchGuidFindBox.PerformLayout();
      this.StatusDropEdit.ResumeLayout(true);
      this.StatusDropEdit.PerformLayout();
      this.RequestDateEdit.ResumeLayout(true);
      this.RequestDateEdit.PerformLayout();
      this.CustomsDivisionCodeFindBox.ResumeLayout(true);
      this.CustomsDivisionCodeFindBox.PerformLayout();
      this.CustomsReviewStatusDropEdit.ResumeLayout(true);
      this.CustomsReviewStatusDropEdit.PerformLayout();
      this.ReviewDateEdit.ResumeLayout(true);
      this.ReviewDateEdit.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZDropEdit MessageTypeDropEdit;
		public ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		public ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		public ZArchitecture.ZTextBox RequestDetailsTextBox;
		public ZArchitecture.ZTextBox ApplicationNumberTextBox;
		public ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		public ZArchitecture.GUI.ZDateEdit RequestDateEdit;
		public ZArchitecture.ZCalcEdit EntryCountCalcEdit;
		public ZArchitecture.GUI.ZCodeFindBox CustomsDivisionCodeFindBox;
		public ZArchitecture.GUI.ZDropEdit CustomsReviewStatusDropEdit;
		public ZArchitecture.GUI.ZDateEdit ReviewDateEdit;
	}
}
