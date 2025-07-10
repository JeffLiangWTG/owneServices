using Enterprise.Customs.ASYCUDA.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class EUICS2ManifestFieldsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SpecificCircumstanceIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MOTIdentifierTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReEntryIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SplitConsignmentIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PreviousMRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MOTIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LocalReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportDocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AddressedMemberStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsProfileDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ActualDepartureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MeansOfTransportTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VehicleRegistrationAndNationalityUserControl = new Enterprise.Customs.ASYCUDA.GUI.VehicleRegistrationAndNationalityUserControl();
			this.ReceptacleUserControl = new Enterprise.Customs.EU.Manifest.ICS2.GUI.ReceptacleUserControl();
			this.OriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FinalDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SpecificCircumstanceIndicatorDropEdit.SuspendLayout();
			this.MOTIdentifierTypeDropEdit.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.TransportDocumentTypeDropEdit.SuspendLayout();
			this.AddressedMemberStateDropEdit.SuspendLayout();
			this.CustomsProfileDropEdit.SuspendLayout();
			this.ActualDepartureDateEdit.SuspendLayout();
			this.MeansOfTransportTypeDropEdit.SuspendLayout();
			this.VehicleRegistrationAndNationalityUserControl.SuspendLayout();
			this.ReceptacleUserControl.SuspendLayout();
			this.OriginCodeFindBox.SuspendLayout();
			this.FinalDestinationCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader);
			// 
			// SpecificCircumstanceIndicatorDropEdit
			// 
			this.SpecificCircumstanceIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecificCircumstanceIndicatorDropEdit, "SpecificCircumstanceIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).SpecificCircumstanceIndicator)));
			this.SpecificCircumstanceIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 128, true);
			this.SpecificCircumstanceIndicatorDropEdit.Name = "SpecificCircumstanceIndicatorDropEdit";
			this.SpecificCircumstanceIndicatorDropEdit.PreBoundMaxLength = 1;
			this.SpecificCircumstanceIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.SpecificCircumstanceIndicatorDropEdit.TabIndex = 0;
			// 
			// MOTIdentifierTypeDropEdit
			// 
			this.MOTIdentifierTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MOTIdentifierTypeDropEdit, "MOTIdentifierType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).MOTIdentifierType)));
			this.MOTIdentifierTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 232, true);
			this.MOTIdentifierTypeDropEdit.Name = "MOTIdentifierTypeDropEdit";
			this.MOTIdentifierTypeDropEdit.PreBoundMaxLength = 1;
			this.MOTIdentifierTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.MOTIdentifierTypeDropEdit.TabIndex = 7;
			// 
			// ReEntryIndicatorCheckBox
			// 
			this.ReEntryIndicatorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ReEntryIndicatorCheckBox, "ReEntryIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).ReEntryIndicator)));
			this.ReEntryIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 53, true);
			this.ReEntryIndicatorCheckBox.Name = "ReEntryIndicatorCheckBox";
			this.ReEntryIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			this.ReEntryIndicatorCheckBox.TabIndex = 1;
			this.ReEntryIndicatorCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ReEntryIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// SplitConsignmentIndicatorCheckBox
			// 
			this.SplitConsignmentIndicatorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SplitConsignmentIndicatorCheckBox, "SplitConsignmentIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).SplitConsignmentIndicator)));
			this.SplitConsignmentIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitConsignmentIndicatorCheckBox.Name = "SplitConsignmentIndicatorCheckBox";
			this.SplitConsignmentIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 16, true);
			this.SplitConsignmentIndicatorCheckBox.TabIndex = 3;
			this.SplitConsignmentIndicatorCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.SplitConsignmentIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// PreviousMRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousMRNTextBox, "PreviousMRN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).PreviousMRN)));
			this.PreviousMRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 154, true);
			this.PreviousMRNTextBox.Name = "PreviousMRNTextBox";
			this.PreviousMRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PreviousMRNTextBox.TabIndex = 3;
			// 
			// MOTIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.MOTIdentifierTextBox, "MOTIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).MOTIdentifier)));
			this.MOTIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 180, true);
			this.MOTIdentifierTextBox.Name = "MOTIdentifierTextBox";
			this.MOTIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MOTIdentifierTextBox.TabIndex = 6;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "AMA_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).AMA_GB)));
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 284, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.ParentType = null;
			this.BranchGuidFindBox.ShowDescriptionBox = false;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.BranchGuidFindBox.TabIndex = 4;
			// 
			// LocalReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalReferenceNumberTextBox, "LocalReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).LocalReferenceNumber)));
			this.LocalReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 27, true);
			this.LocalReferenceNumberTextBox.Name = "LocalReferenceNumberTextBox";
			this.LocalReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.LocalReferenceNumberTextBox.TabIndex = 5;
			// 
			// TransportDocumentTypeDropEdit
			// 
			this.TransportDocumentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportDocumentTypeDropEdit, "MasterBill+TransportDocumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).MasterBill.TransportDocumentType)));
			this.TransportDocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 362, true);
			this.TransportDocumentTypeDropEdit.Name = "TransportDocumentTypeDropEdit";
			this.TransportDocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TransportDocumentTypeDropEdit.TabIndex = 8;
			// 
			// RegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegistrationNumberTextBox, "RegistrationNumberForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).RegistrationNumberForBinding)));
			this.RegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 388, true);
			this.RegistrationNumberTextBox.Name = "RegistrationNumberTextBox";
			this.RegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.RegistrationNumberTextBox.TabIndex = 9;
			// 
			// AddressedMemberStateDropEdit
			// 
			this.AddressedMemberStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressedMemberStateDropEdit, "AddressedMemberState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).AddressedMemberState)));
			this.AddressedMemberStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 258, true);
			this.AddressedMemberStateDropEdit.Name = "AddressedMemberStateDropEdit";
			this.AddressedMemberStateDropEdit.PreBoundMaxLength = 1;
			this.AddressedMemberStateDropEdit.ShowDescriptionBox = false;
			this.AddressedMemberStateDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.AddressedMemberStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.AddressedMemberStateDropEdit.TabIndex = 8;
			// 
			// CustomsProfileDropEdit
			// 
			this.CustomsProfileDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsProfileDropEdit, "AMA_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).AMA_CustomsProfile)));
			this.CustomsProfileDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 76, true);
			this.CustomsProfileDropEdit.Name = "CustomsProfileDropEdit";
			this.CustomsProfileDropEdit.PreBoundMaxLength = 1;
			this.CustomsProfileDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.CustomsProfileDropEdit.TabIndex = 9;
			// 
			// ActualDepartureDateEdit
			// 
			this.ActualDepartureDateEdit.AllowDrop = true;
			this.ActualDepartureDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ActualDepartureDateEdit, "AMA_A_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).AMA_A_DEP)));
			this.ActualDepartureDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ActualDepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 206, true);
			this.ActualDepartureDateEdit.Name = "ActualDepartureDateEdit";
			this.ActualDepartureDateEdit.TabIndex = 10;
			// 
			// MeansOfTransportTypeDropEdit
			// 
			this.MeansOfTransportTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MeansOfTransportTypeDropEdit, "AMA_TransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).AMA_TransportMeans)));
			this.MeansOfTransportTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 102, true);
			this.MeansOfTransportTypeDropEdit.Name = "MeansOfTransportTypeDropEdit";
			this.MeansOfTransportTypeDropEdit.PreBoundMaxLength = 1;
			this.MeansOfTransportTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.MeansOfTransportTypeDropEdit.TabIndex = 11;
			// 
			// VehicleRegistrationAndNationalityUserControl
			// 
			this.VehicleRegistrationAndNationalityUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleRegistrationAndNationalityUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)))));
			this.VehicleRegistrationAndNationalityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 336, true);
			this.VehicleRegistrationAndNationalityUserControl.Name = "VehicleRegistrationAndNationalityUserControl";
			this.VehicleRegistrationAndNationalityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 20, true);
			this.VehicleRegistrationAndNationalityUserControl.TabIndex = 12;
			// 
			// ReceptacleUserControl
			// 
			this.ReceptacleUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceptacleUserControl, ".");
			this.ReceptacleUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 310, true);
			this.ReceptacleUserControl.Name = "ReceptacleUserControl";
			this.ReceptacleUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 20, true);
			this.ReceptacleUserControl.TabIndex = 13;
			// 
			// OriginCodeFindBox
			// 
			this.OriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginCodeFindBox, "MasterBill.ABL_RL_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).MasterBill.ABL_RL_NKOrigin)));
			this.OriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 414, true);
			this.OriginCodeFindBox.Name = "OriginCodeFindBox";
			this.OriginCodeFindBox.ParentType = null;
			this.OriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OriginCodeFindBox.TabIndex = 0;
			// 
			// FinalDestinationCodeFindBox
			// 
			this.FinalDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FinalDestinationCodeFindBox, "MasterBill.ABL_RL_NKFinalDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).MasterBill.ABL_RL_NKFinalDestination)));
			this.FinalDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 440, true);
			this.FinalDestinationCodeFindBox.Name = "FinalDestinationCodeFindBox";
			this.FinalDestinationCodeFindBox.ParentType = null;
			this.FinalDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.FinalDestinationCodeFindBox.TabIndex = 0;
			// 
			// EUICS2ManifestFieldsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SpecificCircumstanceIndicatorDropEdit);
			this.Controls.Add(this.MOTIdentifierTypeDropEdit);
			this.Controls.Add(this.ReEntryIndicatorCheckBox);
			this.Controls.Add(this.SplitConsignmentIndicatorCheckBox);
			this.Controls.Add(this.PreviousMRNTextBox);
			this.Controls.Add(this.MOTIdentifierTextBox);
			this.Controls.Add(this.BranchGuidFindBox);
			this.Controls.Add(this.LocalReferenceNumberTextBox);
			this.Controls.Add(this.TransportDocumentTypeDropEdit);
			this.Controls.Add(this.RegistrationNumberTextBox);
			this.Controls.Add(this.AddressedMemberStateDropEdit);
			this.Controls.Add(this.CustomsProfileDropEdit);
			this.Controls.Add(this.ActualDepartureDateEdit);
			this.Controls.Add(this.MeansOfTransportTypeDropEdit);
			this.Controls.Add(this.VehicleRegistrationAndNationalityUserControl);
			this.Controls.Add(this.ReceptacleUserControl);
			this.Controls.Add(this.FinalDestinationCodeFindBox);
			this.Controls.Add(this.OriginCodeFindBox);
			this.Name = "EUICS2ManifestFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 616, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SpecificCircumstanceIndicatorDropEdit.ResumeLayout(true);
			this.SpecificCircumstanceIndicatorDropEdit.PerformLayout();
			this.MOTIdentifierTypeDropEdit.ResumeLayout(true);
			this.MOTIdentifierTypeDropEdit.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.TransportDocumentTypeDropEdit.ResumeLayout(true);
			this.TransportDocumentTypeDropEdit.PerformLayout();
			this.AddressedMemberStateDropEdit.ResumeLayout(true);
			this.AddressedMemberStateDropEdit.PerformLayout();
			this.CustomsProfileDropEdit.ResumeLayout(true);
			this.CustomsProfileDropEdit.PerformLayout();
			this.ActualDepartureDateEdit.ResumeLayout(true);
			this.ActualDepartureDateEdit.PerformLayout();
			this.MeansOfTransportTypeDropEdit.ResumeLayout(true);
			this.MeansOfTransportTypeDropEdit.PerformLayout();
			this.VehicleRegistrationAndNationalityUserControl.ResumeLayout(true);
			this.VehicleRegistrationAndNationalityUserControl.PerformLayout();
			this.ReceptacleUserControl.ResumeLayout(true);
			this.ReceptacleUserControl.PerformLayout();
			this.OriginCodeFindBox.ResumeLayout(true);
			this.OriginCodeFindBox.PerformLayout();
			this.FinalDestinationCodeFindBox.ResumeLayout(true);
			this.FinalDestinationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit SpecificCircumstanceIndicatorDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit MOTIdentifierTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox ReEntryIndicatorCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox SplitConsignmentIndicatorCheckBox;
		internal Enterprise.ZArchitecture.ZTextBox PreviousMRNTextBox;
		internal Enterprise.ZArchitecture.ZTextBox MOTIdentifierTextBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		internal Enterprise.ZArchitecture.ZTextBox LocalReferenceNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit TransportDocumentTypeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox RegistrationNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit AddressedMemberStateDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit CustomsProfileDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit ActualDepartureDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit MeansOfTransportTypeDropEdit;
		internal VehicleRegistrationAndNationalityUserControl VehicleRegistrationAndNationalityUserControl;
		internal ReceptacleUserControl ReceptacleUserControl;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox OriginCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox FinalDestinationCodeFindBox;
	}
}
