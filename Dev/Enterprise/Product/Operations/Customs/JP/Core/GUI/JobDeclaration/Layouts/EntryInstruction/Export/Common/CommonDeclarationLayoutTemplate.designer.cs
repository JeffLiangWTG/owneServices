using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class CommonDeclarationLayoutTemplate
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

		void InitializeComponent()
		{
			this.ExportCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarantCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CarrierCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VesselNameCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DeclarationReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FinalDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExportControlNumberUserControl = new Enterprise.Customs.JP.GUI.ExportControlNumberUserControl();
			this.VoyageFlightNoBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DateOfArrivalBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PortOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExportDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PortOfDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReceiptModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeliveryModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BookingNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AllEntryInsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VesselCodeFindBox.SuspendLayout();
			this.VesselNameCodeFindBox.SuspendLayout();
			this.FinalDestinationCodeFindBox.SuspendLayout();
			this.ExportControlNumberUserControl.SuspendLayout();
			this.DateOfArrivalBoundDateEdit.SuspendLayout();
			this.PortOfLoadingCodeFindBox.SuspendLayout();
			this.ExportDateBoundDateEdit.SuspendLayout();
			this.PortOfDischargeCodeFindBox.SuspendLayout();
			this.ReceiptModeDropEdit.SuspendLayout();
			this.DeliveryModeDropEdit.SuspendLayout();
			this.AllEntryInsSeparatorUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.CusEntryInstruction);
			//
			// 
			// ExportCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportCodeTextBox, "CEI_ExporterCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_ExporterCode)));
			this.ExportCodeTextBox.Name = "ExportCodeTextBox";
			this.ExportCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.ExportCodeTextBox.ReadOnly = true;
			//
			// 
			// ExportNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportNameTextBox, "CEI_ExporterName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_ExporterName)));
			this.ExportNameTextBox.Name = "ExportNameTextBox";
			this.ExportNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.ExportNameTextBox.ReadOnly = true;
			//
			// 
			// DeclarantCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclarantCodeTextBox, "CEI_DeclarantCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_DeclarantCode)));
			this.DeclarantCodeTextBox.Name = "DeclarantCodeTextBox";
			this.DeclarantCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.DeclarantCodeTextBox.TabIndex = 1;
			this.DeclarantCodeTextBox.ReadOnly = true;
			// 
			// CarrierCodeCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.CarrierCodeCodeFindBox, "JobDeclaration.JE_CarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.Customs.JP.Business.JobDeclaration)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration)).JE_CarrierCode);
			this.CarrierCodeCodeFindBox.Name = "CarrierCodeCodeFindBox";
			this.CarrierCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "JobDeclaration.JE_RadioCallSign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.Customs.JP.Business.JobDeclaration)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration)).JE_RadioCallSign);
			this.VesselCodeFindBox.CaptionResourceString = GUI.Res.GetData("9E92DAF0-53EA-4DD2-A15D-C03FED069291", "Vessel Code");
			this.VesselCodeFindBox.Name = "VesselCodeFindBox";
			this.VesselCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVesselZZ;
			this.VesselCodeFindBox.ParentType = null;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// VesselNameCodeFindBox
			// 
			this.VesselNameCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselNameCodeFindBox, "JobDeclaration.JE_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.Customs.JP.Business.JobDeclaration)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration)).JE_VesselName);
			this.VesselNameCodeFindBox.CaptionResourceString = GUI.Res.GetData("7ACDAC36-BBAF-4D8C-AF25-8B8EC521F85C", "Vessel Name");
			this.VesselNameCodeFindBox.Name = "VesselNameCodeFindBox";
			this.VesselNameCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVesselZZ;
			this.VesselNameCodeFindBox.ParentType = null;
			this.VesselNameCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			//
			// 
			// DeclarationReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclarationReferenceTextBox, "CEI_DeclarationReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_DeclarationReference)));
			this.DeclarationReferenceTextBox.Name = "DeclarationReferenceTextBox";
			this.DeclarationReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.DeclarationReferenceTextBox.ReadOnly = true;
			// 
			// FinalDestinationCodeFindBox
			// 
			this.FinalDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FinalDestinationCodeFindBox, "FinalDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).FinalDestination)));
			this.FinalDestinationCodeFindBox.CaptionResourceString = GUI.Res.GetData("FC1F8DEF-1036-4FA1-89C7-10E5ED177288", "Final Destination");
			this.FinalDestinationCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.FinalDestinationCodeFindBox.Name = "FinalDestinationCodeFindBox";
			this.FinalDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FinalDestinationCodeFindBox.ParentType = null;
			this.FinalDestinationCodeFindBox.PreBoundMaxLength = 5;
			this.FinalDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// ExportControlNumberUserControl
			// 
			this.ExportControlNumberUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportControlNumberUserControl, ".");
			this.ExportControlNumberUserControl.Name = "ExportControlNumberUserControl";
			this.ExportControlNumberUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 22, true);
			// 
			// VoyageFlightNoBoundTextBox
			// 
			this.VoyageFlightNoBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.VoyageFlightNoBoundTextBox, "JobDeclaration.JE_VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.Customs.JP.Business.JobDeclaration)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration)).JE_VoyageFlightNo);
			this.VoyageFlightNoBoundTextBox.CaptionResourceString = GUI.Res.GetData("4624E5B9-B987-405B-87F5-71B3087FFEE8", "Voyage");
			this.VoyageFlightNoBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 64, true);
			this.VoyageFlightNoBoundTextBox.Name = "VoyageFlightNoBoundTextBox";
			this.VoyageFlightNoBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			// 
			// DateOfArrivalBoundDateEdit
			// 
			this.DateOfArrivalBoundDateEdit.AllowDrop = true;
			this.DateOfArrivalBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateOfArrivalBoundDateEdit, "JobDeclaration.JE_DateOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZDateTime)((Enterprise.Customs.JP.Business.JobDeclaration)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration)).JE_DateOfArrival);
			this.DateOfArrivalBoundDateEdit.CaptionResourceString = GUI.Res.GetData("941434C1-3935-4AEA-8E64-121FA6AE26E4", "Arrival Date");
			this.DateOfArrivalBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 159, true);
			this.DateOfArrivalBoundDateEdit.TabIndex = 11;
			this.DateOfArrivalBoundDateEdit.Name = "DateOfArrivalBoundDateEdit";
			// 
			// PortOfLoadingCodeFindBox
			// 
			this.PortOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingCodeFindBox, "JobDeclaration.JE_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.Customs.JP.Business.JobDeclaration)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration)).JE_RL_NKPortOfLoading);
			this.PortOfLoadingCodeFindBox.CaptionResourceString = GUI.Res.GetData("0637F8D7-1BA4-430E-B28C-5A4F6618958F", "Port Of Loading");
			this.PortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 88, true);
			this.PortOfLoadingCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfLoadingCodeFindBox.Name = "PortOfLoadingCodeFindBox";
			this.PortOfLoadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfLoadingCodeFindBox.ParentType = null;
			this.PortOfLoadingCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			// 
			// ExportDateBoundDateEdit
			// 
			this.ExportDateBoundDateEdit.AllowDrop = true;
			this.ExportDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ExportDateBoundDateEdit, "JobDeclaration.JE_ExportDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZDateTime)((Enterprise.Customs.JP.Business.JobDeclaration)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration)).JE_ExportDate);
			this.ExportDateBoundDateEdit.CaptionResourceString = GUI.Res.GetData("39519EC0-E307-40DA-A913-B68D94C3E0FF", "Departure Date");
			this.ExportDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 88, true);
			this.ExportDateBoundDateEdit.Name = "ExportDateBoundDateEdit";
			// 
			// PortOfDischargeCodeFindBox
			// 
			this.PortOfDischargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDischargeCodeFindBox, "JobDeclaration.JE_RL_NKPortOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.Customs.JP.Business.JobDeclaration)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration)).JE_RL_NKPortOfArrival);
			this.PortOfDischargeCodeFindBox.CaptionResourceString = GUI.Res.GetData("7DF676BA-E166-4AFA-A7BF-BCC049D3907C", "Port Of Discharge");
			this.PortOfDischargeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.PortOfDischargeCodeFindBox.Name = "PortOfDischargeCodeFindBox";
			this.PortOfDischargeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfDischargeCodeFindBox.ParentType = null;
			this.PortOfDischargeCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			// 
			// ReceiptModeDropEdit
			// 
			this.ReceiptModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceiptModeDropEdit, "ReceiptMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).ReceiptMode)));
			this.ReceiptModeDropEdit.Name = "ReceiptModeDropEdit";
			this.ReceiptModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// DeliveryModeDropEdit
			// 
			this.DeliveryModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryModeDropEdit, "JobDeclaration.JE_DeliveryMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.Customs.JP.Business.JobDeclaration)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration)).JE_DeliveryMode);
			this.DeliveryModeDropEdit.Name = "DeliveryModeDropEdit";
			this.DeliveryModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// BookingNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BookingNumberTextBox, "JobDeclaration.JE_BookingNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.Customs.JP.Business.JobDeclaration)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).JobDeclaration)).JE_BookingNumber);
			this.BookingNumberTextBox.Name = "BookingNumberTextBox";
			this.BookingNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// AllEntryInsSeparatorUserControl
			// 
			this.AllEntryInsSeparatorUserControl.AllowDrop = true;
			this.AllEntryInsSeparatorUserControl.CaptionResourceString = Res.GetData("38514363-7058-43CF-87A3-8D5F198827F5", "All Entry Instructions");
			this.AllEntryInsSeparatorUserControl.Name = "AllEntryInsSeparatorUserControl";
			this.AllEntryInsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// EntryInstructionLayoutTemplate
			// 
			this.Controls.Add(this.ExportCodeTextBox);
			this.Controls.Add(this.ExportNameTextBox);
			this.Controls.Add(this.DeclarantCodeTextBox);
			this.Controls.Add(this.CarrierCodeCodeFindBox);
			this.Controls.Add(this.VesselCodeFindBox);
			this.Controls.Add(this.VesselNameCodeFindBox);
			this.Controls.Add(this.DeclarationReferenceTextBox);
			this.Controls.Add(this.FinalDestinationCodeFindBox);
			this.Controls.Add(this.ExportControlNumberUserControl);
			this.Controls.Add(this.VoyageFlightNoBoundTextBox);
			this.Controls.Add(this.DateOfArrivalBoundDateEdit);
			this.Controls.Add(this.PortOfLoadingCodeFindBox);
			this.Controls.Add(this.ExportDateBoundDateEdit);
			this.Controls.Add(this.PortOfDischargeCodeFindBox);
			this.Controls.Add(this.ReceiptModeDropEdit);
			this.Controls.Add(this.DeliveryModeDropEdit);
			this.Controls.Add(this.BookingNumberTextBox);
			this.Controls.Add(this.AllEntryInsSeparatorUserControl);
			this.Name = "EntryInstructionLayoutTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 83, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CarrierCodeCodeFindBox.ResumeLayout(true);
			this.CarrierCodeCodeFindBox.PerformLayout();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.VesselNameCodeFindBox.ResumeLayout(true);
			this.VesselNameCodeFindBox.PerformLayout();
			this.FinalDestinationCodeFindBox.ResumeLayout(true);
			this.FinalDestinationCodeFindBox.PerformLayout();
			this.ExportControlNumberUserControl.ResumeLayout(true);
			this.ExportControlNumberUserControl.PerformLayout();
			this.DateOfArrivalBoundDateEdit.ResumeLayout(true);
			this.DateOfArrivalBoundDateEdit.PerformLayout();
			this.PortOfLoadingCodeFindBox.ResumeLayout(true);
			this.PortOfLoadingCodeFindBox.PerformLayout();
			this.ExportDateBoundDateEdit.ResumeLayout(true);
			this.ExportDateBoundDateEdit.PerformLayout();
			this.PortOfDischargeCodeFindBox.ResumeLayout(true);
			this.PortOfDischargeCodeFindBox.PerformLayout();
			this.ReceiptModeDropEdit.ResumeLayout(true);
			this.ReceiptModeDropEdit.PerformLayout();
			this.DeliveryModeDropEdit.ResumeLayout(true);
			this.DeliveryModeDropEdit.PerformLayout();
			this.AllEntryInsSeparatorUserControl.ResumeLayout(true);
			this.AllEntryInsSeparatorUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZTextBox ExportCodeTextBox;
		ZTextBox ExportNameTextBox;
		ZTextBox DeclarantCodeTextBox;
		ZCodeFindBox CarrierCodeCodeFindBox;
		ZCodeFindBox VesselCodeFindBox;
		ZCodeFindBox VesselNameCodeFindBox;
		ZTextBox DeclarationReferenceTextBox;
		ZCodeFindBox FinalDestinationCodeFindBox;
		ExportControlNumberUserControl ExportControlNumberUserControl;
		ZTextBox VoyageFlightNoBoundTextBox;
		ZDateEdit DateOfArrivalBoundDateEdit;
		ZCodeFindBox PortOfLoadingCodeFindBox;
		ZDateEdit ExportDateBoundDateEdit;
		ZCodeFindBox PortOfDischargeCodeFindBox;
		ZDropEdit ReceiptModeDropEdit;
		ZDropEdit DeliveryModeDropEdit;
		ZTextBox BookingNumberTextBox;
		SeparatorUserControl AllEntryInsSeparatorUserControl;
	}
}
