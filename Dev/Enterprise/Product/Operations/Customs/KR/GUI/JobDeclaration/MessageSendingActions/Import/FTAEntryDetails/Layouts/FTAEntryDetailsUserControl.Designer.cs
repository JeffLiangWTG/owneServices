using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class FTAEntryDetailsUserControl
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
            this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.LawCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DepartureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.DeparturePortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.DepartureCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.ManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.CustomsDisbursementBillNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TransshipmentYNDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TransshipmentDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.TransshipmentPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.TransshipmentCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.ImporterAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.ExporterAddressControl = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.ManufacturerAreaPostcodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.LawCodeDropEdit.SuspendLayout();
            this.DepartureDateEdit.SuspendLayout();
            this.DeparturePortCodeFindBox.SuspendLayout();
            this.DepartureCountryCodeFindBox.SuspendLayout();
            this.ManufacturerAddressControl.SuspendLayout();
            this.TransshipmentYNDropEdit.SuspendLayout();
            this.TransshipmentDateEdit.SuspendLayout();
            this.TransshipmentPortCodeFindBox.SuspendLayout();
            this.TransshipmentCountryCodeFindBox.SuspendLayout();
            this.ImporterAddressControl.SuspendLayout();
            this.ExporterAddressControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent);
            // 
            // EntryNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "SendingObjectsCollection.FormattedEntryNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FormattedEntryNumber)));
            this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 24, true);
            this.EntryNumberTextBox.Name = "EntryNumberTextBox";
            this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.EntryNumberTextBox.TabIndex = 0;
            // 
            // LawCodeDropEdit
            // 
            this.LawCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LawCodeDropEdit, "SendingObjectsCollection.LawCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).LawCode)));
            this.LawCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 50, true);
            this.LawCodeDropEdit.Name = "LawCodeDropEdit";
            this.LawCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
            this.LawCodeDropEdit.TabIndex = 1;
            // 
            // DepartureDateEdit
            // 
            this.DepartureDateEdit.AllowDrop = true;
            this.DepartureDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.DepartureDateEdit, "SendingObjectsCollection.DepartureDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).DepartureDate)));
            this.DepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 76, true);
            this.DepartureDateEdit.Name = "DepartureDateEdit";
            this.DepartureDateEdit.TabIndex = 2;
            // 
            // DeparturePortCodeFindBox
            // 
            this.DeparturePortCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DeparturePortCodeFindBox, "SendingObjectsCollection.DeparturePortOfLoading");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).DeparturePortOfLoading)));
            this.DeparturePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 102, true);
            this.DeparturePortCodeFindBox.Name = "DeparturePortCodeFindBox";
            this.DeparturePortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DeparturePortCodeFindBox.ParentType = null;
            this.DeparturePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.DeparturePortCodeFindBox.TabIndex = 3;
            // 
            // DepartureCountryCodeFindBox
            // 
            this.DepartureCountryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DepartureCountryCodeFindBox, "SendingObjectsCollection.DepartureCountry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).DepartureCountry)));
            this.DepartureCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 128, true);
            this.DepartureCountryCodeFindBox.Name = "DepartureCountryCodeFindBox";
            this.DepartureCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DepartureCountryCodeFindBox.ParentType = null;
            this.DepartureCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.DepartureCountryCodeFindBox.TabIndex = 4;
            // 
            // ManufacturerAddressControl
            // 
            this.ManufacturerAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ManufacturerAddressControl, "SendingObjectsCollection.Manufacturer");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).Manufacturer)));
            this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 154, true);
            this.ManufacturerAddressControl.Name = "ManufacturerAddressControl";
            this.ManufacturerAddressControl.PopupCaption = "";
            this.ManufacturerAddressControl.ShowAddress = false;
            this.ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.ManufacturerAddressControl.TabIndex = 5;
            // 
            // CustomsDisbursementBillNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.CustomsDisbursementBillNoTextBox, "SendingObjectsCollection.CustomsDisbursementBill");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).CustomsDisbursementBill)));
            this.CustomsDisbursementBillNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 24, true);
            this.CustomsDisbursementBillNoTextBox.Name = "CustomsDisbursementBillNoTextBox";
            this.CustomsDisbursementBillNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.CustomsDisbursementBillNoTextBox.TabIndex = 6;
            // 
            // TransshipmentYNDropEdit
            // 
            this.TransshipmentYNDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransshipmentYNDropEdit, "SendingObjectsCollection.TransshipmentYN");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).TransshipmentYN)));
            this.TransshipmentYNDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 50, true);
            this.TransshipmentYNDropEdit.Name = "TransshipmentYNDropEdit";
            this.TransshipmentYNDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.TransshipmentYNDropEdit.TabIndex = 7;
            // 
            // TransshipmentDateEdit
            // 
            this.TransshipmentDateEdit.AllowDrop = true;
            this.TransshipmentDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.TransshipmentDateEdit, "SendingObjectsCollection.TransshipmentDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).TransshipmentDate)));
            this.TransshipmentDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 76, true);
            this.TransshipmentDateEdit.Name = "TransshipmentDateEdit";
            this.TransshipmentDateEdit.TabIndex = 8;
            // 
            // TransshipmentPortCodeFindBox
            // 
            this.TransshipmentPortCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransshipmentPortCodeFindBox, "SendingObjectsCollection.TransshipmentPort");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).TransshipmentPort)));
            this.TransshipmentPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 102, true);
            this.TransshipmentPortCodeFindBox.Name = "TransshipmentPortCodeFindBox";
            this.TransshipmentPortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.TransshipmentPortCodeFindBox.ParentType = null;
            this.TransshipmentPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.TransshipmentPortCodeFindBox.TabIndex = 9;
            // 
            // TransshipmentCountryCodeFindBox
            // 
            this.TransshipmentCountryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransshipmentCountryCodeFindBox, "SendingObjectsCollection.TransshipmentCountry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).TransshipmentCountry)));
            this.TransshipmentCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 128, true);
            this.TransshipmentCountryCodeFindBox.Name = "TransshipmentCountryCodeFindBox";
            this.TransshipmentCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.TransshipmentCountryCodeFindBox.ParentType = null;
            this.TransshipmentCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.TransshipmentCountryCodeFindBox.TabIndex = 10;
            // 
            // ImporterAddressControl
            // 
            this.ImporterAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ImporterAddressControl, "SendingObjectsCollection.Importer");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).Importer)));
            this.ImporterAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 154, true);
            this.ImporterAddressControl.Name = "ImporterAddressControl";
            this.ImporterAddressControl.PopupCaption = "";
            this.ImporterAddressControl.ShowAddress = false;
            this.ImporterAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.ImporterAddressControl.TabIndex = 11;
            // 
            // ExporterAddressControl
            // 
            this.ExporterAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ExporterAddressControl, "SendingObjectsCollection.Exporter");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).Exporter)));
            this.ExporterAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 180, true);
            this.ExporterAddressControl.Name = "ExporterAddressControl";
            this.ExporterAddressControl.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ExporterAddressControl.ParentType = null;
            this.ExporterAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
            this.ExporterAddressControl.TabIndex = 12;
            // 
            // ManufacturerAreaPostcodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.ManufacturerAreaPostcodeTextBox, "SendingObjectsCollection.ManufacturPostCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ManufacturPostCode)));
            this.ManufacturerAreaPostcodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 180, true);
            this.ManufacturerAreaPostcodeTextBox.Name = "ManufacturerAreaPostcodeTextBox";
            this.ManufacturerAreaPostcodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
            this.ManufacturerAreaPostcodeTextBox.TabIndex = 13;
            // 
            // FTAEntryDetailsUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ManufacturerAreaPostcodeTextBox);
            this.Controls.Add(this.ExporterAddressControl);
            this.Controls.Add(this.ImporterAddressControl);
            this.Controls.Add(this.TransshipmentCountryCodeFindBox);
            this.Controls.Add(this.TransshipmentPortCodeFindBox);
            this.Controls.Add(this.TransshipmentDateEdit);
            this.Controls.Add(this.TransshipmentYNDropEdit);
            this.Controls.Add(this.CustomsDisbursementBillNoTextBox);
            this.Controls.Add(this.ManufacturerAddressControl);
            this.Controls.Add(this.DepartureCountryCodeFindBox);
            this.Controls.Add(this.DeparturePortCodeFindBox);
            this.Controls.Add(this.DepartureDateEdit);
            this.Controls.Add(this.LawCodeDropEdit);
            this.Controls.Add(this.EntryNumberTextBox);
            this.Name = "FTAEntryDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1104, 218, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.LawCodeDropEdit.ResumeLayout(true);
            this.LawCodeDropEdit.PerformLayout();
            this.DepartureDateEdit.ResumeLayout(true);
            this.DepartureDateEdit.PerformLayout();
            this.DeparturePortCodeFindBox.ResumeLayout(true);
            this.DeparturePortCodeFindBox.PerformLayout();
            this.DepartureCountryCodeFindBox.ResumeLayout(true);
            this.DepartureCountryCodeFindBox.PerformLayout();
            this.ManufacturerAddressControl.ResumeLayout(true);
            this.ManufacturerAddressControl.PerformLayout();
            this.TransshipmentYNDropEdit.ResumeLayout(true);
            this.TransshipmentYNDropEdit.PerformLayout();
            this.TransshipmentDateEdit.ResumeLayout(true);
            this.TransshipmentDateEdit.PerformLayout();
            this.TransshipmentPortCodeFindBox.ResumeLayout(true);
            this.TransshipmentPortCodeFindBox.PerformLayout();
            this.TransshipmentCountryCodeFindBox.ResumeLayout(true);
            this.TransshipmentCountryCodeFindBox.PerformLayout();
            this.ImporterAddressControl.ResumeLayout(true);
            this.ImporterAddressControl.PerformLayout();
            this.ExporterAddressControl.ResumeLayout(true);
            this.ExporterAddressControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		internal ZDropEdit LawCodeDropEdit;
		internal ZDateEdit DepartureDateEdit;
		internal ZCodeFindBox DeparturePortCodeFindBox;
		internal ZCodeFindBox DepartureCountryCodeFindBox;
		internal ZAddressControl ManufacturerAddressControl;
		internal ZArchitecture.ZTextBox CustomsDisbursementBillNoTextBox;
		internal ZDropEdit TransshipmentYNDropEdit;
		internal ZDateEdit TransshipmentDateEdit;
		internal ZCodeFindBox TransshipmentPortCodeFindBox;
		internal ZCodeFindBox TransshipmentCountryCodeFindBox;
		internal ZAddressControl ImporterAddressControl;
		internal ZGuidFindBox ExporterAddressControl;
		internal ZArchitecture.ZTextBox ManufacturerAreaPostcodeTextBox;
		internal ZArchitecture.ZTextBox EntryNumberTextBox;
	}
}
