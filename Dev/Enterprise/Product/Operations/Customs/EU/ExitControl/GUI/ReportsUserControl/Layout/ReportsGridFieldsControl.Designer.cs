using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ReportsGridFieldsControl
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
			this.ConsignmentGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.OfficeOfExitCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportNationalityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DiscrepanciesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FormattedDateTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LocationOfGoodsUserControl = new LocationOfGoodsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsignmentGuidDropEdit.SuspendLayout();
			this.OfficeOfExitCodeFindBox.SuspendLayout();
			this.TransportTypeDropEdit.SuspendLayout();
			this.TransportNationalityDropEdit.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.FormattedDateTimeDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.CusExitReport);
			// 
			// ConsignmentGuidDropEdit
			// 
			this.ConsignmentGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignmentGuidDropEdit, "CER_CXC_Consignment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_CXC_Consignment)));
			this.ConsignmentGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 26, true);
			this.ConsignmentGuidDropEdit.Name = "ConsignmentGuidDropEdit";
			this.ConsignmentGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ConsignmentGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ConsignmentGuidDropEdit.TabIndex = 0;
			// 
			// OfficeOfExitCodeFindBox
			// 
			this.OfficeOfExitCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OfficeOfExitCodeFindBox, "CER_OfficeOfExit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_OfficeOfExit)));
			this.OfficeOfExitCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 52, true);
			this.OfficeOfExitCodeFindBox.Name = "OfficeOfExitCodeFindBox";
			this.OfficeOfExitCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OfficeOfExitCodeFindBox.ParentType = null;
			this.OfficeOfExitCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.OfficeOfExitCodeFindBox.TabIndex = 1;
			// 
			// TransportTypeDropEdit
			// 
			this.TransportTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportTypeDropEdit, "CER_TransportType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_TransportType)));
			this.TransportTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 78, true);
			this.TransportTypeDropEdit.Name = "TransportTypeDropEdit";
			this.TransportTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TransportTypeDropEdit.TabIndex = 2;
			// 
			// TransportIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportIDTextBox, "CER_TransportID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_TransportID)));
			this.TransportIDTextBox.CaptionResourceString = null;
			this.TransportIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 104, true);
			this.TransportIDTextBox.Name = "TransportIDTextBox";
			this.TransportIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TransportIDTextBox.TabIndex = 3;
			// 
			// TransportNationalityDropEdit
			// 
			this.TransportNationalityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportNationalityDropEdit, "CER_RN_NKTransportNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_RN_NKTransportNationality)));
			this.TransportNationalityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 130, true);
			this.TransportNationalityDropEdit.Name = "TransportNationalityDropEdit";
			this.TransportNationalityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TransportNationalityDropEdit.TabIndex = 4;
			// 
			// DiscrepanciesCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DiscrepanciesCheckBox, "CER_Calc_Discrepancies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_Calc_Discrepancies)));
			this.DiscrepanciesCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DiscrepanciesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 160, true);
			this.DiscrepanciesCheckBox.Name = "DiscrepanciesCheckBox";
			this.DiscrepanciesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.DiscrepanciesCheckBox.TabIndex = 4;
			this.DiscrepanciesCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DiscrepanciesCheckBox.UseVisualStyleBackColor = true;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "CER_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_TransportMode)));
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 195, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TransportModeDropEdit.TabIndex = 10;
			// 
			// LocationCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.LocationCodeFindBox, "CER_Location");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_Location)));
			this.LocationCodeFindBox.CaptionResourceString = null;
			this.LocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 220, true);
			this.LocationCodeFindBox.Name = "LocationCodeFindBox";
			this.LocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.LocationCodeFindBox.TabIndex = 15;
			// 
			// FormattedDateTimeDateEdit
			// 
			this.FormattedDateTimeDateEdit.AllowDrop = true;
			this.FormattedDateTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.FormattedDateTimeDateEdit, "CER_DateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CER_DateTime)));
			this.FormattedDateTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 280, true);
			this.FormattedDateTimeDateEdit.Name = "FormattedDateTimeDateEdit";
			this.FormattedDateTimeDateEdit.TabIndex = 0;
			// 
			// LocationOfGoodsUserControl
			// 
			this.LocationOfGoodsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationOfGoodsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.ICusGoodsLocationProvider)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)))));
			this.LocationOfGoodsUserControl.CusGoodsLocationProviderType = null;
			this.LocationOfGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 323, true);
			this.LocationOfGoodsUserControl.Name = "LocationOfGoodsUserControl";
			this.LocationOfGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 21, true);
			this.LocationOfGoodsUserControl.TabIndex = 20;
			// 
			// ReportsGridFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportNationalityDropEdit);
			this.Controls.Add(this.TransportIDTextBox);
			this.Controls.Add(this.TransportTypeDropEdit);
			this.Controls.Add(this.OfficeOfExitCodeFindBox);
			this.Controls.Add(this.ConsignmentGuidDropEdit);
			this.Controls.Add(this.LocationCodeFindBox);
			this.Controls.Add(this.DiscrepanciesCheckBox);
			this.Controls.Add(this.FormattedDateTimeDateEdit);
			this.Controls.Add(this.TransportModeDropEdit);
			this.Controls.Add(this.LocationOfGoodsUserControl);
			this.Name = "ReportsGridFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 503, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsignmentGuidDropEdit.ResumeLayout(true);
			this.ConsignmentGuidDropEdit.PerformLayout();
			this.OfficeOfExitCodeFindBox.ResumeLayout(true);
			this.OfficeOfExitCodeFindBox.PerformLayout();
			this.TransportTypeDropEdit.ResumeLayout(true);
			this.TransportTypeDropEdit.PerformLayout();
			this.TransportNationalityDropEdit.ResumeLayout(true);
			this.TransportNationalityDropEdit.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.FormattedDateTimeDateEdit.ResumeLayout(true);
			this.FormattedDateTimeDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGuidDropEdit ConsignmentGuidDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox OfficeOfExitCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit TransportTypeDropEdit;
		internal ZArchitecture.ZTextBox TransportIDTextBox;
		internal ZArchitecture.GUI.ZDropEdit TransportNationalityDropEdit;
		internal ZArchitecture.GUI.ZCheckBox DiscrepanciesCheckBox;
		internal ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox LocationCodeFindBox;
		internal ZArchitecture.GUI.ZDateEdit FormattedDateTimeDateEdit;
		internal LocationOfGoodsUserControl LocationOfGoodsUserControl;
	}
}
