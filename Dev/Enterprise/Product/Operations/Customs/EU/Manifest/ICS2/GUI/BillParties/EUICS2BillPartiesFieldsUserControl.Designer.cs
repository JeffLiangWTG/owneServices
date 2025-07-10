using Enterprise.Customs.EU.Manifest.ICS2.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class EUICS2BillPartiesFieldsUserControl
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
			this.ShipperPersonTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsigneePersonTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NotifyPartyPersonTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SellerSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.SellerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ConvertSellerToOrganizationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SellerNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SellerStreet1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SellerStreet2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SellerCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SellerCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SellerStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SellerPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SellerPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SellerRegNoTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SellerRegNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BuyerPersonTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SellerPersonTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipperPersonTypeDropEdit.SuspendLayout();
			this.ConsigneePersonTypeDropEdit.SuspendLayout();
			this.NotifyPartyPersonTypeDropEdit.SuspendLayout();
			this.SellerSeparatorUserControl.SuspendLayout();
			this.SellerAddressControl.SuspendLayout();
			this.SellerCountryCodeFindBox.SuspendLayout();
			this.SellerStateDropEdit.SuspendLayout();
			this.SellerRegNoTypeDropEdit.SuspendLayout();
			this.BuyerPersonTypeDropEdit.SuspendLayout();
			this.SellerPersonTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill);
			// 
			// ShipperPersonTypeDropEdit
			// 
			this.ShipperPersonTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperPersonTypeDropEdit, "ShipperPersonType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ShipperPersonType)));
			this.ShipperPersonTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 20, true);
			this.ShipperPersonTypeDropEdit.Name = "ShipperPersonTypeDropEdit";
			this.ShipperPersonTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 31, true);
			this.ShipperPersonTypeDropEdit.TabIndex = 0;
			// 
			// ConsigneePersonTypeDropEdit
			// 
			this.ConsigneePersonTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneePersonTypeDropEdit, "ConsigneePersonType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ConsigneePersonType)));
			this.ConsigneePersonTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 60, true);
			this.ConsigneePersonTypeDropEdit.Name = "ConsigneePersonTypeDropEdit";
			this.ConsigneePersonTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 31, true);
			this.ConsigneePersonTypeDropEdit.TabIndex = 1;
			// 
			// NotifyPartyPersonTypeDropEdit
			// 
			this.NotifyPartyPersonTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyPersonTypeDropEdit, "NotifyPartyPersonType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).NotifyPartyPersonType)));
			this.NotifyPartyPersonTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 100, true);
			this.NotifyPartyPersonTypeDropEdit.Name = "NotifyPartyPersonTypeDropEdit";
			this.NotifyPartyPersonTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 31, true);
			this.NotifyPartyPersonTypeDropEdit.TabIndex = 2;
			// 
			// SellerSeparatorUserControl
			// 
			this.SellerSeparatorUserControl.AllowDrop = true;
			this.SellerSeparatorUserControl.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("C47B61B2-2BE2-4B00-8CAE-EB9576681D7E", "Seller Details");
			this.SellerSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 164, true);
			this.SellerSeparatorUserControl.Name = "SellerSeparatorUserControl";
			this.SellerSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.SellerSeparatorUserControl.TabIndex = 0;
			// 
			// SellerAddressControl
			// 
			this.SellerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellerAddressControl, "ABL_OA_Seller");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ABL_OA_Seller)));
			this.SellerAddressControl.BindToOrgList = "Lookups.Sellers";
			this.SellerAddressControl.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("DC936E1E-A6F0-4CB8-963A-594467E06618", "Party");
			this.SellerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 185, true);
			this.SellerAddressControl.Name = "SellerAddressControl";
			this.SellerAddressControl.PopupCaption = "";
			this.SellerAddressControl.ShowAddress = false;
			this.SellerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 31, true);
			this.SellerAddressControl.TabIndex = 1;
			// 
			// ConvertSellerToOrganizationButton
			// 
			this.ConvertSellerToOrganizationButton.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("6CFB61B1-0FF0-4BF2-9C1A-5A3CC1020603", "Convert Seller To Org.");
			this.ConvertSellerToOrganizationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 211, true);
			this.ConvertSellerToOrganizationButton.Name = "ConvertSellerToOrganizationButton";
			this.ConvertSellerToOrganizationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.ConvertSellerToOrganizationButton.TabIndex = 2;
			this.ConvertSellerToOrganizationButton.ToolTipCaption = null;
			this.ConvertSellerToOrganizationButton.UseVisualStyleBackColor = true;
			this.ConvertSellerToOrganizationButton.Click += new System.EventHandler(this.ConvertSellerToOrganizationButton_Click);
			// 
			// SellerNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.SellerNameTextBox, "ABL_SellerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ABL_SellerName)));
			this.SellerNameTextBox.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("78599B72-79D4-4DF6-AE9B-D260467722D0", "Name");
			this.SellerNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 240, true);
			this.SellerNameTextBox.Name = "SellerNameTextBox";
			this.SellerNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 31, true);
			this.SellerNameTextBox.TabIndex = 3;
			// 
			// SellerStreet1TextBox
			// 
			this.BindingSource.SetBindingMember(this.SellerStreet1TextBox, "ABL_SellerStreet1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ABL_SellerStreet1)));
			this.SellerStreet1TextBox.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("8300A539-EF82-4308-8BEF-76E857ED347B", "Street 1");
			this.SellerStreet1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 277, true);
			this.SellerStreet1TextBox.Name = "SellerStreet1TextBox";
			this.SellerStreet1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 31, true);
			this.SellerStreet1TextBox.TabIndex = 4;
			// 
			// SellerStreet2TextBox
			// 
			this.BindingSource.SetBindingMember(this.SellerStreet2TextBox, "ABL_SellerStreet2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ABL_SellerStreet2)));
			this.SellerStreet2TextBox.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("A56EF48A-B48F-4D0B-BE70-AF1A58B3F9B0", "Street 2");
			this.SellerStreet2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 314, true);
			this.SellerStreet2TextBox.Name = "SellerStreet2TextBox";
			this.SellerStreet2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 31, true);
			this.SellerStreet2TextBox.TabIndex = 5;
			// 
			// SellerCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.SellerCityTextBox, "ABL_SellerCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ABL_SellerCity)));
			this.SellerCityTextBox.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("63EBDED9-0914-4346-A440-BA56AA296E1C", "City");
			this.SellerCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 351, true);
			this.SellerCityTextBox.Name = "SellerCityTextBox";
			this.SellerCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 31, true);
			this.SellerCityTextBox.TabIndex = 6;
			// 
			// SellerCountryCodeFindBox
			// 
			this.SellerCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellerCountryCodeFindBox, "ABL_RN_NKSellerCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ABL_RN_NKSellerCountry)));
			this.SellerCountryCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("D2ED87EA-E304-4829-B71A-8B1BF37C296B", "Ctry/Rgn.", "Country/Region", "The Country/Region code for the Seller address");
			this.SellerCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 388, true);
			this.SellerCountryCodeFindBox.Name = "SellerCountryCodeFindBox";
			this.SellerCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SellerCountryCodeFindBox.ParentType = null;
			this.SellerCountryCodeFindBox.PreBoundMaxLength = 2;
			this.SellerCountryCodeFindBox.ShowDescriptionBox = false;
			this.SellerCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 31, true);
			this.SellerCountryCodeFindBox.TabIndex = 7;
			// 
			// SellerStateDropEdit
			// 
			this.SellerStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellerStateDropEdit, "ABL_SellerState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ABL_SellerState)));
			this.SellerStateDropEdit.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("39426960-344B-4176-BD28-9EC9A5B5A0E9", "State");
			this.SellerStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 414, true);
			this.SellerStateDropEdit.Name = "SellerStateDropEdit";
			this.SellerStateDropEdit.PreBoundMaxLength = 25;
			this.SellerStateDropEdit.ShowDescriptionBox = false;
			this.SellerStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 31, true);
			this.SellerStateDropEdit.TabIndex = 8;
			// 
			// SellerPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.SellerPostCodeTextBox, "ABL_SellerPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ABL_SellerPostcode)));
			this.SellerPostCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("A12F2A60-FAF1-4759-9D62-A1D77DC21275", "Postcode");
			this.SellerPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 440, true);
			this.SellerPostCodeTextBox.Name = "SellerPostCodeTextBox";
			this.SellerPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 31, true);
			this.SellerPostCodeTextBox.TabIndex = 9;
			// 
			// SellerPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.SellerPhoneTextBox, "ABL_SellerPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ABL_SellerPhone)));
			this.SellerPhoneTextBox.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("90B85FC6-24FD-4A4E-AA34-F2EE9350AAD8", "Phone");
			this.SellerPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 477, true);
			this.SellerPhoneTextBox.Name = "SellerPhoneTextBox";
			this.SellerPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 31, true);
			this.SellerPhoneTextBox.TabIndex = 10;
			// 
			// SellerRegNoTypeDropEdit
			// 
			this.SellerRegNoTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellerRegNoTypeDropEdit, "ABL_SellerRegNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ABL_SellerRegNoType)));
			this.SellerRegNoTypeDropEdit.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("91BF899A-64D2-45AB-B31E-58223A43FAA0", "Reg. No. Type");
			this.SellerRegNoTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 551, true);
			this.SellerRegNoTypeDropEdit.Name = "SellerRegNoTypeDropEdit";
			this.SellerRegNoTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 31, true);
			this.SellerRegNoTypeDropEdit.TabIndex = 11;
			// 
			// SellerRegNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.SellerRegNoTextBox, "ABL_SellerRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).ABL_SellerRegNo)));
			this.SellerRegNoTextBox.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("434129C6-4FAB-450C-A8D9-E074F714BF0F", "Reg.No");
			this.SellerRegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(498, 514, true);
			this.SellerRegNoTextBox.Name = "SellerRegNoTextBox";
			this.SellerRegNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 31, true);
			this.SellerRegNoTextBox.TabIndex = 12;
			// 
			// BuyerPersonTypeDropEdit
			// 
			this.BuyerPersonTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerPersonTypeDropEdit, "BuyerPersonType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).BuyerPersonType)));
			this.BuyerPersonTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 20, true);
			this.BuyerPersonTypeDropEdit.Name = "BuyerPersonTypeDropEdit";
			this.BuyerPersonTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 31, true);
			this.BuyerPersonTypeDropEdit.TabIndex = 13;
			// 
			// SellerPersonTypeDropEdit
			// 
			this.SellerPersonTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SellerPersonTypeDropEdit, "SellerPersonType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).SellerPersonType)));
			this.SellerPersonTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 82, true);
			this.SellerPersonTypeDropEdit.Name = "SellerPersonTypeDropEdit";
			this.SellerPersonTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 31, true);
			this.SellerPersonTypeDropEdit.TabIndex = 14;
			// 
			// EUICS2BillPartiesFieldsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SellerPersonTypeDropEdit);
			this.Controls.Add(this.BuyerPersonTypeDropEdit);
			this.Controls.Add(this.NotifyPartyPersonTypeDropEdit);
			this.Controls.Add(this.ConsigneePersonTypeDropEdit);
			this.Controls.Add(this.ShipperPersonTypeDropEdit);
			this.Controls.Add(this.SellerSeparatorUserControl);
			this.Controls.Add(this.SellerAddressControl);
			this.Controls.Add(this.ConvertSellerToOrganizationButton);
			this.Controls.Add(this.SellerNameTextBox);
			this.Controls.Add(this.SellerStreet1TextBox);
			this.Controls.Add(this.SellerStreet2TextBox);
			this.Controls.Add(this.SellerCityTextBox);
			this.Controls.Add(this.SellerCountryCodeFindBox);
			this.Controls.Add(this.SellerStateDropEdit);
			this.Controls.Add(this.SellerPostCodeTextBox);
			this.Controls.Add(this.SellerPhoneTextBox);
			this.Controls.Add(this.SellerRegNoTextBox);
			this.Controls.Add(this.SellerRegNoTypeDropEdit);
			this.Name = "EUICS2BillPartiesFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(925, 743, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipperPersonTypeDropEdit.ResumeLayout(true);
			this.ShipperPersonTypeDropEdit.PerformLayout();
			this.ConsigneePersonTypeDropEdit.ResumeLayout(true);
			this.ConsigneePersonTypeDropEdit.PerformLayout();
			this.NotifyPartyPersonTypeDropEdit.ResumeLayout(true);
			this.NotifyPartyPersonTypeDropEdit.PerformLayout();
			this.SellerSeparatorUserControl.ResumeLayout(true);
			this.SellerSeparatorUserControl.PerformLayout();
			this.SellerAddressControl.ResumeLayout(true);
			this.SellerAddressControl.PerformLayout();
			this.SellerCountryCodeFindBox.ResumeLayout(true);
			this.SellerCountryCodeFindBox.PerformLayout();
			this.SellerStateDropEdit.ResumeLayout(true);
			this.SellerStateDropEdit.PerformLayout();
			this.SellerRegNoTypeDropEdit.ResumeLayout(true);
			this.SellerRegNoTypeDropEdit.PerformLayout();
			this.BuyerPersonTypeDropEdit.ResumeLayout(true);
			this.BuyerPersonTypeDropEdit.PerformLayout();
			this.SellerPersonTypeDropEdit.ResumeLayout(true);
			this.SellerPersonTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit ShipperPersonTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ConsigneePersonTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit NotifyPartyPersonTypeDropEdit;

		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl SellerSeparatorUserControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl SellerAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZButton ConvertSellerToOrganizationButton;
		internal Enterprise.ZArchitecture.ZTextBox SellerNameTextBox;
		internal Enterprise.ZArchitecture.ZTextBox SellerStreet1TextBox;
		internal Enterprise.ZArchitecture.ZTextBox SellerStreet2TextBox;
		internal Enterprise.ZArchitecture.ZTextBox SellerCityTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox SellerCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit SellerStateDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox SellerPostCodeTextBox;
		internal Enterprise.ZArchitecture.ZTextBox SellerPhoneTextBox;
		internal Enterprise.ZArchitecture.ZTextBox SellerRegNoTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit SellerRegNoTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit BuyerPersonTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit SellerPersonTypeDropEdit;
	}
}
