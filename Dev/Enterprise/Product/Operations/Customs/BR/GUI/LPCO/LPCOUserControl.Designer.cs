namespace Enterprise.Customs.BR.GUI
{
	partial class LPCOUserControl
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
			this.LPCODetailGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LPCOJobNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LPCONumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LPCOHolderFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LPCOStartDateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LPCOEndDateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LPCORetroactiveDateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LPCODetailGroupBox.SuspendLayout();
			this.LPCOJobNumberTextBox.SuspendLayout();
			this.LPCONumberTextBox.SuspendLayout();
			this.LPCOHolderFindBox.SuspendLayout();
			this.LPCOStartDateZDateEdit.SuspendLayout();
			this.LPCOEndDateZDateEdit.SuspendLayout();
			this.LPCORetroactiveDateZDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.CusLPCOHeader);
			// 
			// LPCODetailGroupBox
			// 
			this.LPCODetailGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("41b91350-77fc-4eef-b5ce-b79f61300392", "LPCO Details");
			this.LPCODetailGroupBox.Controls.Add(this.LPCOEndDateZDateEdit);
			this.LPCODetailGroupBox.Controls.Add(this.LPCOStartDateZDateEdit);
			this.LPCODetailGroupBox.Controls.Add(this.LPCOJobNumberTextBox);
			this.LPCODetailGroupBox.Controls.Add(this.LPCONumberTextBox);
			this.LPCODetailGroupBox.Controls.Add(this.LPCOHolderFindBox);
			this.LPCODetailGroupBox.Controls.Add(this.LPCORetroactiveDateZDateEdit);
			this.LPCODetailGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LPCODetailGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LPCODetailGroupBox.Name = "LPCODetailGroupBox";
			this.LPCODetailGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 207, true);
			this.LPCODetailGroupBox.TabIndex = 2;
			this.LPCODetailGroupBox.TabStop = false;
			// 
			// LPCOJobNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LPCOJobNumberTextBox, "CPH_JobNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusLPCOHeader)(null)).CPH_JobNumber)));
			this.LPCOJobNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 15, true);
			this.LPCOJobNumberTextBox.Name = "LPCOJobNumberTextBox";
			this.LPCOJobNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.LPCOJobNumberTextBox.TabIndex = 1;
			// 
			// LPCONumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LPCONumberTextBox, "CPH_Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusLPCOHeader)(null)).CPH_Number)));
			this.LPCONumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 41, true);
			this.LPCONumberTextBox.Name = "LPCONumberTextBox";
			this.LPCONumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.LPCONumberTextBox.TabIndex = 2;
			// 
			// LPCOHolderFindBox
			// 
			this.LPCOHolderFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOHolderFindBox, "CPH_OH_PermitHolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.BR.Business.CusLPCOHeader)(null)).CPH_OH_PermitHolder)));
			this.LPCOHolderFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 67, true);
			this.LPCOHolderFindBox.Name = "LPCOHolderFindBox";
			this.LPCOHolderFindBox.ParentModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.LPCOHolderFindBox.ParentType = null;
			this.LPCOHolderFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 20, true);
			this.LPCOHolderFindBox.TabIndex = 3;
			// 
			// LPCOStartDateZDateEdit
			// 
			this.LPCOStartDateZDateEdit.AllowDrop = true;
			this.LPCOStartDateZDateEdit.AutoCompleteMonthThreshold = 1;
			this.LPCOStartDateZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LPCOStartDateZDateEdit, "CPH_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.CusLPCOHeader)(null)).CPH_StartDate)));
			this.LPCOStartDateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 93, true);
			this.LPCOStartDateZDateEdit.Name = "LPCOStartDateZDateEdit";
			this.LPCOStartDateZDateEdit.TabIndex = 4;
			// 
			// LPCOEndDateZDateEdit
			// 
			this.LPCOEndDateZDateEdit.AllowDrop = true;
			this.LPCOEndDateZDateEdit.AutoCompleteMonthThreshold = 1;
			this.LPCOEndDateZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LPCOEndDateZDateEdit, "CPH_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.CusLPCOHeader)(null)).CPH_EndDate)));
			this.LPCOEndDateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 119, true);
			this.LPCOEndDateZDateEdit.Name = "LPCOEndDateZDateEdit";
			this.LPCOEndDateZDateEdit.TabIndex = 5;
			// 
			// LPCORetroactiveDateZDateEdit
			// 
			this.LPCORetroactiveDateZDateEdit.AllowDrop = true;
			this.LPCORetroactiveDateZDateEdit.AutoCompleteMonthThreshold = 1;
			this.LPCORetroactiveDateZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LPCORetroactiveDateZDateEdit, "CPH_RetroactiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.CusLPCOHeader)(null)).CPH_RetroactiveDate)));
			this.LPCORetroactiveDateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 145, true);
			this.LPCORetroactiveDateZDateEdit.Name = "LPCORetroactiveDateZDateEdit";
			this.LPCORetroactiveDateZDateEdit.TabIndex = 6;
			// 
			// LPCOUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LPCODetailGroupBox);
			this.Name = "LPCOUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 271, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LPCODetailGroupBox.ResumeLayout(false);
			this.LPCODetailGroupBox.PerformLayout();
			this.LPCOJobNumberTextBox.ResumeLayout(true);
			this.LPCOJobNumberTextBox.PerformLayout();
			this.LPCONumberTextBox.ResumeLayout(true);
			this.LPCONumberTextBox.PerformLayout();
			this.LPCOHolderFindBox.ResumeLayout(true);
			this.LPCOHolderFindBox.PerformLayout();
			this.LPCOStartDateZDateEdit.ResumeLayout(true);
			this.LPCOStartDateZDateEdit.PerformLayout();
			this.LPCOEndDateZDateEdit.ResumeLayout(true);
			this.LPCOEndDateZDateEdit.PerformLayout();
			this.LPCORetroactiveDateZDateEdit.ResumeLayout(true);
			this.LPCORetroactiveDateZDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZGroupBox LPCODetailGroupBox;
		public ZArchitecture.ZTextBox LPCOJobNumberTextBox;
		public ZArchitecture.ZTextBox LPCONumberTextBox;
		public ZArchitecture.GUI.ZGuidFindBox LPCOHolderFindBox;
		public ZArchitecture.GUI.ZDateEdit LPCOStartDateZDateEdit;
		public ZArchitecture.GUI.ZDateEdit LPCOEndDateZDateEdit;
		public ZArchitecture.GUI.ZDateTimeOffsetEdit LPCORetroactiveDateZDateEdit;
	}
}
