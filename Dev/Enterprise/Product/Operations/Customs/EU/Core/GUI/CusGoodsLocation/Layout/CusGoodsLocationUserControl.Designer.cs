namespace Enterprise.Customs.EU.GUI
{
	partial class CusGoodsLocationUserControl
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
			this.QualifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostcodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.UnlocoCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GeoLocationLatitudeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GeoLocationLongitudeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.EoriNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorizationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.StreetAndNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StreetAndNumberWithAddressValidationUserControl = new Enterprise.Customs.EU.GUI.StreetAndNumberWithAddressValidationControl();
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorizationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.QualifierDropEdit.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			this.CountryCodeFindBox.SuspendLayout();
			this.UnlocoCodeFindBox.SuspendLayout();
			this.CustomsOfficeCodeFindBox.SuspendLayout();
			this.OrganisationFindBox.SuspendLayout();
			this.AuthorizationCodeFindBox.SuspendLayout();
			this.StreetAndNumberWithAddressValidationUserControl.SuspendLayout();
			this.AuthorizationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusGoodsLocation);
			// 
			// QualifierDropEdit
			// 
			this.QualifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QualifierDropEdit, "CGL_Qualifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).CGL_Qualifier)));
			this.QualifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 18, true);
			this.QualifierDropEdit.Name = "QualifierDropEdit";
			this.QualifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.QualifierDropEdit.TabIndex = 0;
			// 
			// TypeDropEdit
			// 
			this.TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "CGL_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).CGL_Type)));
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 44, true);
			this.TypeDropEdit.Name = "TypeDropEdit";
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.TypeDropEdit.TabIndex = 1;
			// 
			// AdditionalIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalIdentifierTextBox, "AdditionalIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).AdditionalIdentifier)));
			this.AdditionalIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 70, true);
			this.AdditionalIdentifierTextBox.Name = "AdditionalIdentifierTextBox";
			this.AdditionalIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.AdditionalIdentifierTextBox.TabIndex = 2;
			// 
			// PostcodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PostcodeTextBox, "Address.E2_Postcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.E2_Postcode)));
			this.PostcodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 96, true);
			this.PostcodeTextBox.Name = "PostcodeTextBox";
			this.PostcodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.PostcodeTextBox.TabIndex = 3;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "Address.E2_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.E2_RN_NKCountryCode)));
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 122, true);
			this.CountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryCodeFindBox.ParentType = null;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 16, true);
			this.CountryCodeFindBox.TabIndex = 4;
			// 
			// UnlocoCodeFindBox
			// 
			this.UnlocoCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnlocoCodeFindBox, "Unlocode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Unlocode)));
			this.UnlocoCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 148, true);
			this.UnlocoCodeFindBox.Name = "UnlocoCodeFindBox";
			this.UnlocoCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.UnlocoCodeFindBox.ParentType = null;
			this.UnlocoCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 16, true);
			this.UnlocoCodeFindBox.TabIndex = 5;
			// 
			// CustomsOfficeCodeFindBox
			// 
			this.CustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "CGL_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).CGL_CustomsOffice)));
			this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 174, true);
			this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
			this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsOfficeCodeFindBox.ParentType = null;
			this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 16, true);
			this.CustomsOfficeCodeFindBox.TabIndex = 5;
			// 
			// GeoLocationLatitudeTextBox
			// 
			this.BindingSource.SetBindingMember(this.GeoLocationLatitudeTextBox, "Address.E2_Latitude");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.E2_Latitude)));
			this.GeoLocationLatitudeTextBox.DecimalPlaces = 7;
			this.GeoLocationLatitudeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 200, true);
			this.GeoLocationLatitudeTextBox.Name = "GeoLocationLatitudeTextBox";
			this.GeoLocationLatitudeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.GeoLocationLatitudeTextBox.TabIndex = 6;
			// 
			// GeoLocationLongitudeTextBox
			// 
			this.BindingSource.SetBindingMember(this.GeoLocationLongitudeTextBox, "Address.E2_Longitude");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.E2_Longitude)));
			this.GeoLocationLongitudeTextBox.DecimalPlaces = 7;
			this.GeoLocationLongitudeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 226, true);
			this.GeoLocationLongitudeTextBox.Name = "GeoLocationLongitudeTextBox";
			this.GeoLocationLongitudeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.GeoLocationLongitudeTextBox.TabIndex = 7;
			// 
			// OrganisationFindBox
			// 
			this.OrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganisationFindBox, "Address.IdentificationHolderPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.IdentificationHolderPK)));
			this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 252, true);
			this.OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrganisationFindBox.Name = "OrganisationFindBox";
			this.OrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OrganisationFindBox.ParentType = null;
			this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 16, true);
			this.OrganisationFindBox.TabIndex = 8;
			// 
			// EoriNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EoriNumberTextBox, "Address.E2_GovRegNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.E2_GovRegNum)));
			this.EoriNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 278, true);
			this.EoriNumberTextBox.Name = "EoriNumberTextBox";
			this.EoriNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.EoriNumberTextBox.TabIndex = 8;
			// 
			// AuthorizationCodeFindBox
			// 
			this.AuthorizationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationCodeFindBox, "Address.AuthorisationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.AuthorisationNumber)));
			this.AuthorizationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 304, true);
			this.AuthorizationCodeFindBox.Name = "AuthorizationCodeFindBox";
			this.AuthorizationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AuthorizationCodeFindBox.ParentType = null;
			this.AuthorizationCodeFindBox.ShowDescriptionBox = false;
			this.AuthorizationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.AuthorizationCodeFindBox.TabIndex = 9;
			// 
			// StreetAndNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.StreetAndNumberTextBox, "Address.E2_Address1AndE2_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.E2_Address1AndE2_Address2)));
			this.StreetAndNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StreetAndNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 389, true);
			this.StreetAndNumberTextBox.Name = "StreetAndNumberTextBox";
			this.StreetAndNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.StreetAndNumberTextBox.TabIndex = 10;
			// 
			// StreetAndNumberWithAddressValidationUserControl
			// 
			this.StreetAndNumberWithAddressValidationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StreetAndNumberWithAddressValidationUserControl, ".");
			this.StreetAndNumberWithAddressValidationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 359, true);
			this.StreetAndNumberWithAddressValidationUserControl.Name = "StreetAndNumberWithAddressValidationUserControl";
			this.StreetAndNumberWithAddressValidationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.StreetAndNumberWithAddressValidationUserControl.TabIndex = 11;
			// 
			// CityTextBox
			// 
			this.BindingSource.SetBindingMember(this.CityTextBox, "Address.E2_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.E2_City)));
			this.CityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 415, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.CityTextBox.TabIndex = 12;
			// 
			// ContactTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactTextBox, "Address.E2_Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.E2_Contact)));
			this.ContactTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 445, true);
			this.ContactTextBox.Name = "ContactTextBox";
			this.ContactTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.ContactTextBox.TabIndex = 13;
			// 
			// PhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.PhoneTextBox, "Address.E2_Phone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.E2_Phone)));
			this.PhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 475, true);
			this.PhoneTextBox.Name = "PhoneTextBox";
			this.PhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.PhoneTextBox.TabIndex = 14;
			// 
			// EmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.EmailTextBox, "Address.E2_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.E2_Email)));
			this.EmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 505, true);
			this.EmailTextBox.Name = "EmailTextBox";
			this.EmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.EmailTextBox.TabIndex = 15;
			// 
			// AuthorizationDropEdit
			// 
			this.AuthorizationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationDropEdit, "Address.AuthorisationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.AuthorisationNumber)));
			this.AuthorizationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 331, true);
			this.AuthorizationDropEdit.Name = "AuthorizationDropEdit";
			this.AuthorizationDropEdit.ShowDescriptionBox = false;
			this.AuthorizationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.AuthorizationDropEdit.TabIndex = 16;
			// 
			// CusGoodsLocationUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AuthorizationDropEdit);
			this.Controls.Add(this.QualifierDropEdit);
			this.Controls.Add(this.TypeDropEdit);
			this.Controls.Add(this.AdditionalIdentifierTextBox);
			this.Controls.Add(this.PostcodeTextBox);
			this.Controls.Add(this.CountryCodeFindBox);
			this.Controls.Add(this.UnlocoCodeFindBox);
			this.Controls.Add(this.CustomsOfficeCodeFindBox);
			this.Controls.Add(this.GeoLocationLatitudeTextBox);
			this.Controls.Add(this.GeoLocationLongitudeTextBox);
			this.Controls.Add(this.OrganisationFindBox);
			this.Controls.Add(this.EoriNumberTextBox);
			this.Controls.Add(this.AuthorizationCodeFindBox);
			this.Controls.Add(this.StreetAndNumberTextBox);
			this.Controls.Add(this.StreetAndNumberWithAddressValidationUserControl);
			this.Controls.Add(this.CityTextBox);
			this.Controls.Add(this.ContactTextBox);
			this.Controls.Add(this.PhoneTextBox);
			this.Controls.Add(this.EmailTextBox);
			this.Name = "CusGoodsLocationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 553, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.QualifierDropEdit.ResumeLayout(true);
			this.QualifierDropEdit.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.UnlocoCodeFindBox.ResumeLayout(true);
			this.UnlocoCodeFindBox.PerformLayout();
			this.CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeCodeFindBox.PerformLayout();
			this.OrganisationFindBox.ResumeLayout(true);
			this.OrganisationFindBox.PerformLayout();
			this.AuthorizationCodeFindBox.ResumeLayout(true);
			this.AuthorizationCodeFindBox.PerformLayout();
			this.StreetAndNumberWithAddressValidationUserControl.ResumeLayout(true);
			this.StreetAndNumberWithAddressValidationUserControl.PerformLayout();
			this.AuthorizationDropEdit.ResumeLayout(true);
			this.AuthorizationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit QualifierDropEdit;
		internal ZArchitecture.GUI.ZDropEdit TypeDropEdit;
		internal ZArchitecture.ZTextBox AdditionalIdentifierTextBox;
		internal ZArchitecture.ZTextBox PostcodeTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox UnlocoCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		internal ZArchitecture.ZTextBox GeoLocationLatitudeTextBox;
		internal ZArchitecture.ZTextBox GeoLocationLongitudeTextBox;
		internal Enterprise.MasterFiles.GUI.ZOrganisationFindBox OrganisationFindBox;
		internal ZArchitecture.ZTextBox EoriNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox AuthorizationCodeFindBox;
		internal ZArchitecture.ZTextBox StreetAndNumberTextBox;
		internal StreetAndNumberWithAddressValidationControl StreetAndNumberWithAddressValidationUserControl;
		internal ZArchitecture.ZTextBox CityTextBox;
		internal ZArchitecture.ZTextBox ContactTextBox;
		internal ZArchitecture.ZTextBox PhoneTextBox;
		internal ZArchitecture.ZTextBox EmailTextBox;
		internal ZArchitecture.GUI.ZDropEdit AuthorizationDropEdit;
	}
}
