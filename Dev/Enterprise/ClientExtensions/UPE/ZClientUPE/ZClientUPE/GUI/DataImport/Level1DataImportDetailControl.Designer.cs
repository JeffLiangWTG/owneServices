namespace Enterprise.Client.UPE.GUI.DataImport
{
	partial class Level1DataImportDetailControl
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
			this.FlightScheduleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MasterBillControl = new Enterprise.ZArchitecture.ZMasterBillControl();
			this.FlightNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArivalDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PortOfDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PortOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ColoadMasterTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SurplusCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FlightScheduleGroupBox.SuspendLayout();
			this.MasterBillControl.SuspendLayout();
			this.ArivalDateDateEdit.SuspendLayout();
			this.PortOfDischargeCodeFindBox.SuspendLayout();
			this.PortOfLoadingCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.DataImport.Level1DataImport);
			// 
			// FlightScheduleGroupBox
			// 
			this.FlightScheduleGroupBox.Controls.Add(this.MasterBillControl);
			this.FlightScheduleGroupBox.Controls.Add(this.FlightNumberTextBox);
			this.FlightScheduleGroupBox.Controls.Add(this.ArivalDateDateEdit);
			this.FlightScheduleGroupBox.Controls.Add(this.PortOfDischargeCodeFindBox);
			this.FlightScheduleGroupBox.Controls.Add(this.PortOfLoadingCodeFindBox);
			this.FlightScheduleGroupBox.Controls.Add(this.ColoadMasterTextBox);
			this.FlightScheduleGroupBox.Controls.Add(this.SurplusCheckBox);
			this.FlightScheduleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FlightScheduleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FlightScheduleGroupBox.Name = "FlightScheduleGroupBox";
			this.FlightScheduleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 123, true);
			this.FlightScheduleGroupBox.TabIndex = 1;
			this.FlightScheduleGroupBox.TabStop = false;
			this.FlightScheduleGroupBox.Text = "Import Details";
			// 
			// MasterBillControl
			// 
			this.MasterBillControl.AllowAlphaInMAWP = false;
			this.MasterBillControl.AllowDrop = true;
			this.MasterBillControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MasterBillControl, "MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).MasterBill)));
			this.MasterBillControl.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("10aa34f4-8073-4b32-8860-72af1b13ff96", "Master Bill");
			this.MasterBillControl.FormattedMasterBill = "";
			this.MasterBillControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 46, true);
			this.MasterBillControl.Name = "MasterBillControl";
			this.MasterBillControl.ReadOnly = false;
			this.MasterBillControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.MasterBillControl.TabIndex = 1;
			// 
			// FlightNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightNumberTextBox, "FlightNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).FlightNumber)));
			this.FlightNumberTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3d219f56-0ec7-4321-81fa-217aa636693f", "Flight Number");
			this.FlightNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 24, true);
			this.FlightNumberTextBox.Name = "FlightNumberTextBox";
			this.FlightNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 18, true);
			this.FlightNumberTextBox.TabIndex = 0;
			// 
			// ArivalDateDateEdit
			// 
			this.ArivalDateDateEdit.AllowDrop = true;
			this.ArivalDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArivalDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ArivalDateDateEdit, "ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).ArrivalDate)));
			this.ArivalDateDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("a8168e55-eaad-4da8-b53d-ee921c26c415", "Arrival Date");
			this.ArivalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 94, true);
			this.ArivalDateDateEdit.Name = "ArivalDateDateEdit";
			this.ArivalDateDateEdit.TabIndex = 3;
			// 
			// PortOfDischargeCodeFindBox
			// 
			this.PortOfDischargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDischargeCodeFindBox, "PortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.PortOfDischargeCodeFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("bd6e82fb-cb81-4689-be11-6ed47fc00af7", "Discharge");
			this.PortOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 49, true);
			this.PortOfDischargeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfDischargeCodeFindBox.Name = "PortOfDischargeCodeFindBox";
			this.PortOfDischargeCodeFindBox.ShouldResize = true;
			this.PortOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 18, true);
			this.PortOfDischargeCodeFindBox.TabIndex = 5;
			// 
			// PortOfLoadingCodeFindBox
			// 
			this.PortOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingCodeFindBox, "PortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).PortOfLoading)));
			this.PortOfLoadingCodeFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("0303a474-f01c-4f31-b3ab-fef06662c097", "Loading");
			this.PortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 24, true);
			this.PortOfLoadingCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfLoadingCodeFindBox.Name = "PortOfLoadingCodeFindBox";
			this.PortOfLoadingCodeFindBox.ShouldResize = true;
			this.PortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 18, true);
			this.PortOfLoadingCodeFindBox.TabIndex = 4;
			// 
			// ColoadMasterTextBox
			// 
			this.BindingSource.SetBindingMember(this.ColoadMasterTextBox, "CoLoadMasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).CoLoadMasterBill)));
			this.ColoadMasterTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("f1637033-e67c-4527-83de-447a3655d7d6", "Co-load Master");
			this.ColoadMasterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 71, true);
			this.ColoadMasterTextBox.Name = "ColoadMasterTextBox";
			this.ColoadMasterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.ColoadMasterTextBox.TabIndex = 2;
			// 
			// SurplusCheckBox
			// 
			this.SurplusCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SurplusCheckBox, "IsSurplus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).IsSurplus)));
			this.SurplusCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("71493537-6cdb-474f-94cb-476d5406e926", "Surplus?");
			this.SurplusCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SurplusCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SurplusCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 73, true);
			this.SurplusCheckBox.Name = "SurplusCheckBox";
			this.SurplusCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.SurplusCheckBox.TabIndex = 6;
			this.SurplusCheckBox.Text = "Surplus";
			// 
			// Level1DataImportDetailControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FlightScheduleGroupBox);
			this.Name = "Level1DataImportDetailControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 123, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FlightScheduleGroupBox.ResumeLayout(false);
			this.FlightScheduleGroupBox.PerformLayout();
			this.MasterBillControl.ResumeLayout(true);
			this.MasterBillControl.PerformLayout();
			this.ArivalDateDateEdit.ResumeLayout(true);
			this.ArivalDateDateEdit.PerformLayout();
			this.PortOfDischargeCodeFindBox.ResumeLayout(true);
			this.PortOfDischargeCodeFindBox.PerformLayout();
			this.PortOfLoadingCodeFindBox.ResumeLayout(true);
			this.PortOfLoadingCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox FlightScheduleGroupBox;
		private ZArchitecture.ZMasterBillControl MasterBillControl;
		private ZArchitecture.ZTextBox FlightNumberTextBox;
		private ZArchitecture.GUI.ZDateEdit ArivalDateDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox PortOfLoadingCodeFindBox;
		private ZArchitecture.ZTextBox ColoadMasterTextBox;
		private ZArchitecture.GUI.ZCheckBox SurplusCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox PortOfDischargeCodeFindBox;
	}
}
