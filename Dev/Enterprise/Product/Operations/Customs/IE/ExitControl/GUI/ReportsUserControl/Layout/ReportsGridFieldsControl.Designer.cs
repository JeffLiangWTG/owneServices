using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
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
			this.TypeOfLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FormattedDateTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LongFormattedDateTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.UNLOCOCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DiscrepanciesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DeclarantTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarantAddressDropEdit = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.RepresentativeAddressDropEdit = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.OfficeOfExportCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EnquiryInformationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalDeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TypeOfLocationDropEdit.SuspendLayout();
			this.FormattedDateTimeDateEdit.SuspendLayout();
			this.LongFormattedDateTimeDateEdit.SuspendLayout();
			this.UNLOCOCodeFindBox.SuspendLayout();
			this.DeclarantTypeDropEdit.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.DeclarantAddressDropEdit.SuspendLayout();
			this.RepresentativeAddressDropEdit.SuspendLayout();
			this.OfficeOfExportCodeFindBox.SuspendLayout();
			this.EnquiryInformationCodeDropEdit.SuspendLayout();
			this.AdditionalDeclarationTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.ExitControl.Business.CusExitReport);
			// 
			// TypeOfLocationDropEdit
			// 
			this.TypeOfLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeOfLocationDropEdit, "CER_Calc_TypeOfLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_Calc_TypeOfLocation)));
			this.TypeOfLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 55, true);
			this.TypeOfLocationDropEdit.Name = "TypeOfLocationDropEdit";
			this.TypeOfLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TypeOfLocationDropEdit.TabIndex = 1;
			// 
			// FormattedDateTimeDateEdit
			// 
			this.FormattedDateTimeDateEdit.AllowDrop = true;
			this.FormattedDateTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.FormattedDateTimeDateEdit, "CER_DateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_DateTime)));
			this.FormattedDateTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 3, true);
			this.FormattedDateTimeDateEdit.Name = "FormattedDateTimeDateEdit";
			this.FormattedDateTimeDateEdit.TabIndex = 0;
			// 
			// LongFormattedDateTimeDateEdit
			// 
			this.LongFormattedDateTimeDateEdit.AllowDrop = true;
			this.LongFormattedDateTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.LongFormattedDateTimeDateEdit, "CER_DateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_DateTime)));
			this.LongFormattedDateTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LongFormattedDateTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 29, true);
			this.LongFormattedDateTimeDateEdit.Name = "LongFormattedDateTimeDateEdit";
			this.LongFormattedDateTimeDateEdit.TabIndex = 2;
			// 
			// UNLOCOCodeFindBox
			// 
			this.UNLOCOCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UNLOCOCodeFindBox, "CER_Calc_UNLOCO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_Calc_UNLOCO)));
			this.UNLOCOCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 81, true);
			this.UNLOCOCodeFindBox.Name = "UNLOCOCodeFindBox";
			this.UNLOCOCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.UNLOCOCodeFindBox.ParentType = null;
			this.UNLOCOCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.UNLOCOCodeFindBox.TabIndex = 3;
			// 
			// DiscrepanciesCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DiscrepanciesCheckBox, "CER_Calc_Discrepancies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_Calc_Discrepancies)));
			this.DiscrepanciesCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DiscrepanciesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 107, true);
			this.DiscrepanciesCheckBox.Name = "DiscrepanciesCheckBox";
			this.DiscrepanciesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.DiscrepanciesCheckBox.TabIndex = 4;
			this.DiscrepanciesCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DiscrepanciesCheckBox.UseVisualStyleBackColor = true;
			// 
			// DeclarantTypeDropEdit
			// 
			this.DeclarantTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarantTypeDropEdit, "CER_DeclarantType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_DeclarantType)));
			this.DeclarantTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 189, true);
			this.DeclarantTypeDropEdit.Name = "DeclarantTypeDropEdit";
			this.DeclarantTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.DeclarantTypeDropEdit.TabIndex = 9;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "CER_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_TransportMode)));
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 215, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TransportModeDropEdit.TabIndex = 10;
			// 
			// DeclarantAddressDropEdit
			// 
			this.DeclarantAddressDropEdit.AddressValidationProcessCmdKey = null;
			this.DeclarantAddressDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarantAddressDropEdit, "Declarant");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).Declarant)));
			this.DeclarantAddressDropEdit.BindToOrganisations = "Declarant+Lookups+OrgHeader_List";
			this.DeclarantAddressDropEdit.CaptionResourceString = Enterprise.Customs.IE.ExitControl.GUI.Res.GetData("232A32B7-8625-4BF2-8E1F-F1F9DDBF577D", "Declarant");
			this.DeclarantAddressDropEdit.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.DeclarantAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 137, true);
			this.DeclarantAddressDropEdit.Name = "DeclarantAddressDropEdit";
			this.DeclarantAddressDropEdit.ReadOnly = false;
			this.DeclarantAddressDropEdit.SingleLineNoGroupBoxPanelWidth = 320;
			this.DeclarantAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.DeclarantAddressDropEdit.TabIndex = 5;
			this.DeclarantAddressDropEdit.ValidationJustForced = false;
			// 
			// RepresentativeAddressDropEdit
			// 
			this.RepresentativeAddressDropEdit.AddressValidationProcessCmdKey = null;
			this.RepresentativeAddressDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeAddressDropEdit, "Representative");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).Representative)));
			this.RepresentativeAddressDropEdit.BindToOrganisations = "Representative+Lookups+OrgHeader_List";
			this.RepresentativeAddressDropEdit.CaptionResourceString = Enterprise.Customs.IE.ExitControl.GUI.Res.GetData("48985173-AACF-42B9-84F9-83685D0DEE48", "Representative");
			this.RepresentativeAddressDropEdit.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.RepresentativeAddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 163, true);
			this.RepresentativeAddressDropEdit.Name = "RepresentativeAddressDropEdit";
			this.RepresentativeAddressDropEdit.ReadOnly = false;
			this.RepresentativeAddressDropEdit.SingleLineNoGroupBoxPanelWidth = 320;
			this.RepresentativeAddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.RepresentativeAddressDropEdit.TabIndex = 6;
			this.RepresentativeAddressDropEdit.ValidationJustForced = false;
			// 
			// OfficeOfExportCodeFindBox
			// 
			this.OfficeOfExportCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OfficeOfExportCodeFindBox, "CER_OfficeOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_OfficeOfExport)));
			this.OfficeOfExportCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 241, true);
			this.OfficeOfExportCodeFindBox.Name = "OfficeOfExportCodeFindBox";
			this.OfficeOfExportCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OfficeOfExportCodeFindBox.ParentType = null;
			this.OfficeOfExportCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OfficeOfExportCodeFindBox.TabIndex = 12;
			// 
			// EnquiryInformationCodeDropEdit
			// 
			this.EnquiryInformationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EnquiryInformationCodeDropEdit, "CER_EnquiryInformationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_EnquiryInformationCode)));
			this.EnquiryInformationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 267, true);
			this.EnquiryInformationCodeDropEdit.Name = "EnquiryInformationCodeDropEdit";
			this.EnquiryInformationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.EnquiryInformationCodeDropEdit.TabIndex = 13;
			// 
			// AdditionalDeclarationTypeDropEdit
			// 
			this.AdditionalDeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalDeclarationTypeDropEdit, "CER_AdditionalDeclarationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_AdditionalDeclarationType)));
			this.AdditionalDeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 293, true);
			this.AdditionalDeclarationTypeDropEdit.Name = "AdditionalDeclarationTypeDropEdit";
			this.AdditionalDeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.AdditionalDeclarationTypeDropEdit.TabIndex = 14;
			// 
			// LocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocationTextBox, "CER_Location");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.ExitControl.Business.CusExitReport)(null)).CER_Location)));
			this.LocationTextBox.CaptionResourceString = null;
			this.LocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 319, true);
			this.LocationTextBox.Name = "LocationTextBox";
			this.LocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 20, true);
			this.LocationTextBox.TabIndex = 15;
			// 
			// ReportsGridFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LocationTextBox);
			this.Controls.Add(this.AdditionalDeclarationTypeDropEdit);
			this.Controls.Add(this.EnquiryInformationCodeDropEdit);
			this.Controls.Add(this.OfficeOfExportCodeFindBox);
			this.Controls.Add(this.DeclarantTypeDropEdit);
			this.Controls.Add(this.DiscrepanciesCheckBox);
			this.Controls.Add(this.UNLOCOCodeFindBox);
			this.Controls.Add(this.LongFormattedDateTimeDateEdit);
			this.Controls.Add(this.FormattedDateTimeDateEdit);
			this.Controls.Add(this.TypeOfLocationDropEdit);
			this.Controls.Add(this.DeclarantAddressDropEdit);
			this.Controls.Add(this.RepresentativeAddressDropEdit);
			this.Controls.Add(this.TransportModeDropEdit);
			this.Name = "ReportsGridFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 408, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TypeOfLocationDropEdit.ResumeLayout(true);
			this.TypeOfLocationDropEdit.PerformLayout();
			this.FormattedDateTimeDateEdit.ResumeLayout(true);
			this.FormattedDateTimeDateEdit.PerformLayout();
			this.LongFormattedDateTimeDateEdit.ResumeLayout(true);
			this.LongFormattedDateTimeDateEdit.PerformLayout();
			this.UNLOCOCodeFindBox.ResumeLayout(true);
			this.UNLOCOCodeFindBox.PerformLayout();
			this.DeclarantTypeDropEdit.ResumeLayout(true);
			this.DeclarantTypeDropEdit.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.DeclarantAddressDropEdit.ResumeLayout(true);
			this.DeclarantAddressDropEdit.PerformLayout();
			this.RepresentativeAddressDropEdit.ResumeLayout(true);
			this.RepresentativeAddressDropEdit.PerformLayout();
			this.OfficeOfExportCodeFindBox.ResumeLayout(true);
			this.OfficeOfExportCodeFindBox.PerformLayout();
			this.EnquiryInformationCodeDropEdit.ResumeLayout(true);
			this.EnquiryInformationCodeDropEdit.PerformLayout();
			this.AdditionalDeclarationTypeDropEdit.ResumeLayout(true);
			this.AdditionalDeclarationTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDateEdit FormattedDateTimeDateEdit;
		internal ZArchitecture.GUI.ZDropEdit TypeOfLocationDropEdit;
		internal ZArchitecture.GUI.ZDateEdit LongFormattedDateTimeDateEdit;
		internal ZArchitecture.GUI.ZCodeFindBox UNLOCOCodeFindBox;
		internal ZArchitecture.GUI.ZCheckBox DiscrepanciesCheckBox;
		internal ZDropEdit DeclarantTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		internal ZDocAddressControl DeclarantAddressDropEdit;
		internal ZDocAddressControl RepresentativeAddressDropEdit;
		internal ZCodeFindBox OfficeOfExportCodeFindBox;
		internal ZDropEdit EnquiryInformationCodeDropEdit;
		internal ZDropEdit AdditionalDeclarationTypeDropEdit;
		internal ZArchitecture.ZTextBox LocationTextBox;
	}
}
