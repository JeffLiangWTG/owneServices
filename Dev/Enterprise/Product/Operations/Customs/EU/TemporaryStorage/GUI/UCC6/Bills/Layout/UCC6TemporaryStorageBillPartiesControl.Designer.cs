using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStorageBillPartiesControl
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
		///
		private void InitializeComponent()
		{
			this.ShipperSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.ShipperAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ShipperNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperStreet1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperStreet2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ShipperStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShipperPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperRegNoTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShipperRegNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.ConsigneeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ConsigneeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeStreet1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeStreet2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConigneeCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ConsigneeStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsigneePostcodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneePhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeRegNoTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsigneeRegoNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartySeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.NotifyPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.NotifyPartyNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyStreet1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyStreet2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.NotifyPartyStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NotifyPartyPostcodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyRegNoTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NotifyPartyRegNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipperSeparatorUserControl.SuspendLayout();
			this.ShipperAddressControl.SuspendLayout();
			this.ShipperCountryCodeFindBox.SuspendLayout();
			this.ShipperStateDropEdit.SuspendLayout();
			this.ShipperRegNoTypeDropEdit.SuspendLayout();
			this.ConsigneeSeparatorUserControl.SuspendLayout();
			this.ConsigneeAddressControl.SuspendLayout();
			this.ConigneeCountryCodeFindBox.SuspendLayout();
			this.ConsigneeStateDropEdit.SuspendLayout();
			this.ConsigneeRegNoTypeDropEdit.SuspendLayout();
			this.NotifyPartySeparatorUserControl.SuspendLayout();
			this.NotifyPartyAddressControl.SuspendLayout();
			this.NotifyPartyCountryCodeFindBox.SuspendLayout();
			this.NotifyPartyStateDropEdit.SuspendLayout();
			this.NotifyPartyRegNoTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// ShipperSeparatorUserControl
			// 
			this.ShipperSeparatorUserControl.AllowDrop = true;
			this.ShipperSeparatorUserControl.CaptionResourceString = Res.GetData("1F9CB129-2182-47CB-BA88-CDD2A21B36AD", "Shipper Details");
			this.ShipperSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 4, true);
			this.ShipperSeparatorUserControl.Name = "ShipperSeparatorUserControl";
			this.ShipperSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.ShipperSeparatorUserControl.TabIndex = 0;
			// 
			// ShipperAddressControl
			//
			this.ShipperAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperAddressControl, "Bills.ABL_OA_Shipper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_OA_Shipper)));
			this.ShipperAddressControl.CaptionResourceString = null;
			this.ShipperAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 20, true);
			this.ShipperAddressControl.Name = "ShipperAddressControl";
			this.ShipperAddressControl.PopupCaption = "";
			this.ShipperAddressControl.ReadOnly = false;
			this.ShipperAddressControl.ShowAddress = false;
			this.ShipperAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ShipperAddressControl.TabIndex = 1;
			// 
			// ShipperNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperNameTextBox, "Bills.ABL_ShipperName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ShipperName)));
			this.ShipperNameTextBox.CaptionResourceString = Res.GetData("EB417101-7BBF-49C1-8198-17DEC838E11F", "Name");
			this.ShipperNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 65, true);
			this.ShipperNameTextBox.Name = "ShipperNameTextBox";
			this.ShipperNameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ShipperNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ShipperNameTextBox.TabIndex = 3;
			// 
			// ShipperStreet1TextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperStreet1TextBox, "Bills.ABL_ShipperStreet1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ShipperStreet1)));
			this.ShipperStreet1TextBox.CaptionResourceString = Res.GetData("4F7C8950-08B9-4B01-9A7D-DA687AA4B42D", "Street 1");
			this.ShipperStreet1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 86, true);
			this.ShipperStreet1TextBox.Name = "ShipperStreet1TextBox";
			this.ShipperStreet1TextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ShipperStreet1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ShipperStreet1TextBox.TabIndex = 4;
			// 
			// ShipperStreet2TextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperStreet2TextBox, "Bills.ABL_ShipperStreet2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ShipperStreet2)));
			this.ShipperStreet2TextBox.CaptionResourceString = Res.GetData("2D3FDC64-5654-48A0-9D05-CBCBDF104679", "Street 2");
			this.ShipperStreet2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 107, true);
			this.ShipperStreet2TextBox.Name = "ShipperStreet2TextBox";
			this.ShipperStreet2TextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ShipperStreet2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ShipperStreet2TextBox.TabIndex = 5;
			// 
			// ShipperCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperCityTextBox, "Bills.ABL_ShipperCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ShipperCity)));
			this.ShipperCityTextBox.CaptionResourceString = Res.GetData("481E3751-70AA-4FA7-BFBF-DE90FDDBCB12", "City");
			this.ShipperCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 128, true);
			this.ShipperCityTextBox.Name = "ShipperCityTextBox";
			this.ShipperCityTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ShipperCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ShipperCityTextBox.TabIndex = 6;
			// 
			// ShipperCountryCodeFindBox
			// 
			this.ShipperCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperCountryCodeFindBox, "Bills.ABL_RN_NKShipperCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_RN_NKShipperCountry)));
			this.ShipperCountryCodeFindBox.CaptionResourceString = Res.GetData("6E067C86-5C49-42AA-9DF7-59F2CCF4EF4C", "Country");
			this.ShipperCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 149, true);
			this.ShipperCountryCodeFindBox.Name = "ShipperCountryCodeFindBox";
			this.ShipperCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ShipperCountryCodeFindBox.ParentType = null;
			this.ShipperCountryCodeFindBox.PreBoundMaxLength = 2;
			this.ShipperCountryCodeFindBox.ShowDescriptionBox = false;
			this.ShipperCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.ShipperCountryCodeFindBox.TabIndex = 7;
			// 
			// ShipperStateDropEdit
			// 
			this.ShipperStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperStateDropEdit, "Bills.ABL_ShipperState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ShipperState)));
			this.ShipperStateDropEdit.CaptionResourceString = Res.GetData("D1C662DD-ACEA-4F6D-9E4B-ECAAE5A9A1DA", "State");
			this.ShipperStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 170, true);
			this.ShipperStateDropEdit.Name = "ShipperStateDropEdit";
			this.ShipperStateDropEdit.PreBoundMaxLength = 25;
			this.ShipperStateDropEdit.ShouldResizeByMaxLength = true;
			this.ShipperStateDropEdit.ShowDescriptionBox = false;
			this.ShipperStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.ShipperStateDropEdit.TabIndex = 8;
			// 
			// ShipperPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperPostCodeTextBox, "Bills.ABL_ShipperPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ShipperPostcode)));
			this.ShipperPostCodeTextBox.CaptionResourceString = Res.GetData("7CF9E42D-CD3F-4B70-96F4-FE15172B3501", "Postcode");
			this.ShipperPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 191, true);
			this.ShipperPostCodeTextBox.Name = "ShipperPostCodeTextBox";
			this.ShipperPostCodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ShipperPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ShipperPostCodeTextBox.TabIndex = 9;
			// 
			// ShipperPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperPhoneTextBox, "Bills.ABL_ShipperPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ShipperPhone)));
			this.ShipperPhoneTextBox.CaptionResourceString = Res.GetData("92420857-7BBE-4647-9E0E-F260BF614884", "Phone");
			this.ShipperPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 212, true);
			this.ShipperPhoneTextBox.Name = "ShipperPhoneTextBox";
			this.ShipperPhoneTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ShipperPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.ShipperPhoneTextBox.TabIndex = 10;
			// 
			// ShipperRegNoTypeDropEdit
			// 
			this.ShipperRegNoTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperRegNoTypeDropEdit, "Bills.ABL_ShipperRegNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ShipperRegNoType)));
			this.ShipperRegNoTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 254, true);
			this.ShipperRegNoTypeDropEdit.Name = "ShipperRegNoTypeDropEdit";
			this.ShipperRegNoTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ShipperRegNoTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ShipperRegNoTypeDropEdit.TabIndex = 11;
			// 
			// ShipperRegNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperRegNoTextBox, "Bills.ABL_ShipperRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ShipperRegNo)));
			this.ShipperRegNoTextBox.CaptionResourceString = Res.GetData("D7531C3B-68DF-4D21-AD15-8671AECCB001", "Reg.No");
			this.ShipperRegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 233, true);
			this.ShipperRegNoTextBox.Name = "ShipperRegNoTextBox";
			this.ShipperRegNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ShipperRegNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.ShipperRegNoTextBox.TabIndex = 12;
			// 
			// ConsigneeSeparatorUserControl
			// 
			this.ConsigneeSeparatorUserControl.AllowDrop = true;
			this.ConsigneeSeparatorUserControl.CaptionResourceString = Res.GetData("46D39A02-7C76-4D2D-9BEB-8A7307EF7224", "Consignee Details");
			this.ConsigneeSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 0, true);
			this.ConsigneeSeparatorUserControl.Name = "ConsigneeSeparatorUserControl";
			this.ConsigneeSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.ConsigneeSeparatorUserControl.TabIndex = 13;
			// 
			// ConsigneeAddressControl
			// 
			this.ConsigneeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeAddressControl, "Bills.ABL_OA_Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_OA_Consignee)));
			this.ConsigneeAddressControl.CaptionResourceString = null;
			this.ConsigneeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 17, true);
			this.ConsigneeAddressControl.Name = "ConsigneeAddressControl";
			this.ConsigneeAddressControl.PopupCaption = "";
			this.ConsigneeAddressControl.ReadOnly = false;
			this.ConsigneeAddressControl.ShowAddress = false;
			this.ConsigneeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ConsigneeAddressControl.TabIndex = 14;
			// 
			// ConsigneeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeNameTextBox, "Bills.ABL_ConsigneeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ConsigneeName)));
			this.ConsigneeNameTextBox.CaptionResourceString = Res.GetData("6A287B37-A09D-4011-956D-8AE92D20DBB3", "Name");
			this.ConsigneeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 64, true);
			this.ConsigneeNameTextBox.Name = "ConsigneeNameTextBox";
			this.ConsigneeNameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ConsigneeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ConsigneeNameTextBox.TabIndex = 16;
			// 
			// ConsigneeStreet1TextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeStreet1TextBox, "Bills.ABL_ConsigneeStreet1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ConsigneeStreet1)));
			this.ConsigneeStreet1TextBox.CaptionResourceString = Res.GetData("50B126D5-676D-4C7A-8DBB-129B235C4BE9", "Street 1");
			this.ConsigneeStreet1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 86, true);
			this.ConsigneeStreet1TextBox.Name = "ConsigneeStreet1TextBox";
			this.ConsigneeStreet1TextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ConsigneeStreet1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ConsigneeStreet1TextBox.TabIndex = 17;
			// 
			// ConsigneeStreet2TextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeStreet2TextBox, "Bills.ABL_ConsigneeStreet2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ConsigneeStreet2)));
			this.ConsigneeStreet2TextBox.CaptionResourceString = Res.GetData("D0C5F8DA-9EAD-4F9D-BF38-6EAC82CCE1B9", "Street 2");
			this.ConsigneeStreet2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 108, true);
			this.ConsigneeStreet2TextBox.Name = "ConsigneeStreet2TextBox";
			this.ConsigneeStreet2TextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ConsigneeStreet2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ConsigneeStreet2TextBox.TabIndex = 18;
			// 
			// ConsigneeCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeCityTextBox, "Bills.ABL_ConsigneeCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ConsigneeCity)));
			this.ConsigneeCityTextBox.CaptionResourceString = Res.GetData("DDEC0705-7691-4EC7-8AE5-4D12A3049861", "City");
			this.ConsigneeCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 130, true);
			this.ConsigneeCityTextBox.Name = "ConsigneeCityTextBox";
			this.ConsigneeCityTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ConsigneeCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ConsigneeCityTextBox.TabIndex = 19;
			// 
			// ConigneeCountryCodeFindBox
			// 
			this.ConigneeCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConigneeCountryCodeFindBox, "Bills.ABL_RN_NKConsigneeCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_RN_NKConsigneeCountry)));
			this.ConigneeCountryCodeFindBox.CaptionResourceString = Res.GetData("20DEBE92-C4B0-482A-BC2D-8AA24AB1003D", "Country");
			this.ConigneeCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 152, true);
			this.ConigneeCountryCodeFindBox.Name = "ConigneeCountryCodeFindBox";
			this.ConigneeCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ConigneeCountryCodeFindBox.ParentType = null;
			this.ConigneeCountryCodeFindBox.PreBoundMaxLength = 2;
			this.ConigneeCountryCodeFindBox.ShowDescriptionBox = false;
			this.ConigneeCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.ConigneeCountryCodeFindBox.TabIndex = 20;
			// 
			// ConsigneeStateDropEdit
			// 
			this.ConsigneeStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeStateDropEdit, "Bills.ABL_ConsigneeState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ConsigneeState)));
			this.ConsigneeStateDropEdit.CaptionResourceString = Res.GetData("C58EEA77-67C7-4519-92FE-4F23B9C8EEC4", "State");
			this.ConsigneeStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 174, true);
			this.ConsigneeStateDropEdit.Name = "ConsigneeStateDropEdit";
			this.ConsigneeStateDropEdit.PreBoundMaxLength = 25;
			this.ConsigneeStateDropEdit.ShouldResizeByMaxLength = true;
			this.ConsigneeStateDropEdit.ShowDescriptionBox = false;
			this.ConsigneeStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.ConsigneeStateDropEdit.TabIndex = 21;
			// 
			// ConsigneePostcodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneePostcodeTextBox, "Bills.ABL_ConsigneePostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ConsigneePostcode)));
			this.ConsigneePostcodeTextBox.CaptionResourceString = Res.GetData("E815FB4D-4505-4600-870C-37F0FB1E5C60", "Postcode");
			this.ConsigneePostcodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 196, true);
			this.ConsigneePostcodeTextBox.Name = "ConsigneePostcodeTextBox";
			this.ConsigneePostcodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ConsigneePostcodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ConsigneePostcodeTextBox.TabIndex = 22;
			// 
			// ConsigneePhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneePhoneTextBox, "Bills.ABL_ConsigneePhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ConsigneePhone)));
			this.ConsigneePhoneTextBox.CaptionResourceString = Res.GetData("EC28CD8C-E5A1-4E96-B7E2-F057BA88F40F", "Phone");
			this.ConsigneePhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 218, true);
			this.ConsigneePhoneTextBox.Name = "ConsigneePhoneTextBox";
			this.ConsigneePhoneTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ConsigneePhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.ConsigneePhoneTextBox.TabIndex = 23;
			// 
			// ConsigneeRegNoTypeDropEdit
			// 
			this.ConsigneeRegNoTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeRegNoTypeDropEdit, "Bills.ABL_ConsigneeRegNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ConsigneeRegNoType)));
			this.ConsigneeRegNoTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 262, true);
			this.ConsigneeRegNoTypeDropEdit.Name = "ConsigneeRegNoTypeDropEdit";
			this.ConsigneeRegNoTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ConsigneeRegNoTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ConsigneeRegNoTypeDropEdit.TabIndex = 24;
			// 
			// ConsigneeRegoNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeRegoNoTextBox, "Bills.ABL_ConsigneeRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_ConsigneeRegNo)));
			this.ConsigneeRegoNoTextBox.CaptionResourceString = Res.GetData("FDF1BADD-DDC4-4A87-95FD-0A0F09E543BC", "Reg.No");
			this.ConsigneeRegoNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 240, true);
			this.ConsigneeRegoNoTextBox.Name = "ConsigneeRegoNoTextBox";
			this.ConsigneeRegoNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ConsigneeRegoNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.ConsigneeRegoNoTextBox.TabIndex = 25;
			// 
			// NotifyPartySeparatorUserControl
			// 
			this.NotifyPartySeparatorUserControl.AllowDrop = true;
			this.NotifyPartySeparatorUserControl.CaptionResourceString = Res.GetData("24C9FF9A-976C-481E-A43A-1B5F483F6264", "Notify Party Details");
			this.NotifyPartySeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 0, true);
			this.NotifyPartySeparatorUserControl.Name = "NotifyPartySeparatorUserControl";
			this.NotifyPartySeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.NotifyPartySeparatorUserControl.TabIndex = 26;
			// 
			// NotifyPartyAddressControl
			// 
			this.NotifyPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyAddressControl, "Bills.ABL_OA_NotifyParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_OA_NotifyParty)));
			this.NotifyPartyAddressControl.CaptionResourceString = null;
			this.NotifyPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 17, true);
			this.NotifyPartyAddressControl.Name = "NotifyPartyAddressControl";
			this.NotifyPartyAddressControl.PopupCaption = "";
			this.NotifyPartyAddressControl.ReadOnly = false;
			this.NotifyPartyAddressControl.ShowAddress = false;
			this.NotifyPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.NotifyPartyAddressControl.TabIndex = 27;
			// 
			// NotifyPartyNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyPartyNameTextBox, "Bills.ABL_NotifyPartyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_NotifyPartyName)));
			this.NotifyPartyNameTextBox.CaptionResourceString = Res.GetData("47364EE4-3622-4976-875D-18BE3319B54F", "Name");
			this.NotifyPartyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 64, true);
			this.NotifyPartyNameTextBox.Name = "NotifyPartyNameTextBox";
			this.NotifyPartyNameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.NotifyPartyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.NotifyPartyNameTextBox.TabIndex = 29;
			// 
			// NotifyPartyStreet1TextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyPartyStreet1TextBox, "Bills.ABL_NotifyPartyStreet1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_NotifyPartyStreet1)));
			this.NotifyPartyStreet1TextBox.CaptionResourceString = Res.GetData("C9E0325F-59D5-43CD-8181-A005EE5E3B1D", "Street 1");
			this.NotifyPartyStreet1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 86, true);
			this.NotifyPartyStreet1TextBox.Name = "NotifyPartyStreet1TextBox";
			this.NotifyPartyStreet1TextBox.ShouldEscapeAllSpecialCharacters = false;
			this.NotifyPartyStreet1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.NotifyPartyStreet1TextBox.TabIndex = 30;
			// 
			// NotifyPartyStreet2TextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyPartyStreet2TextBox, "Bills.ABL_NotifyPartyStreet2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_NotifyPartyStreet2)));
			this.NotifyPartyStreet2TextBox.CaptionResourceString = Res.GetData("D31E61B9-8B71-4870-BF85-CD8D086B1D8A", "Street 2");
			this.NotifyPartyStreet2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 108, true);
			this.NotifyPartyStreet2TextBox.Name = "NotifyPartyStreet2TextBox";
			this.NotifyPartyStreet2TextBox.ShouldEscapeAllSpecialCharacters = false;
			this.NotifyPartyStreet2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.NotifyPartyStreet2TextBox.TabIndex = 31;
			// 
			// NotifyPartyCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyPartyCityTextBox, "Bills.ABL_NotifyPartyCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_NotifyPartyCity)));
			this.NotifyPartyCityTextBox.CaptionResourceString = Res.GetData("82019CF8-82A7-4C4A-828D-C58406480ED7", "City");
			this.NotifyPartyCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 130, true);
			this.NotifyPartyCityTextBox.Name = "NotifyPartyCityTextBox";
			this.NotifyPartyCityTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.NotifyPartyCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.NotifyPartyCityTextBox.TabIndex = 32;
			// 
			// NotifyPartyCountryCodeFindBox
			// 
			this.NotifyPartyCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyCountryCodeFindBox, "Bills.ABL_RN_NKNotifyPartyCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_RN_NKNotifyPartyCountry)));
			this.NotifyPartyCountryCodeFindBox.CaptionResourceString = Res.GetData("0715C827-ED6E-42F4-B25A-E6E1CF0DF0AA", "Country");
			this.NotifyPartyCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 152, true);
			this.NotifyPartyCountryCodeFindBox.Name = "NotifyPartyCountryCodeFindBox";
			this.NotifyPartyCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.NotifyPartyCountryCodeFindBox.ParentType = null;
			this.NotifyPartyCountryCodeFindBox.PreBoundMaxLength = 2;
			this.NotifyPartyCountryCodeFindBox.ShowDescriptionBox = false;
			this.NotifyPartyCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.NotifyPartyCountryCodeFindBox.TabIndex = 33;
			// 
			// NotifyPartyStateDropEdit
			// 
			this.NotifyPartyStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyStateDropEdit, "Bills.ABL_NotifyPartyState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_NotifyPartyState)));
			this.NotifyPartyStateDropEdit.CaptionResourceString = Res.GetData("99B72A9C-4463-441A-914F-F60F57CC49F1", "State");
			this.NotifyPartyStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 174, true);
			this.NotifyPartyStateDropEdit.Name = "NotifyPartyStateDropEdit";
			this.NotifyPartyStateDropEdit.PreBoundMaxLength = 31;
			this.NotifyPartyStateDropEdit.ShouldResizeByMaxLength = true;
			this.NotifyPartyStateDropEdit.ShowDescriptionBox = false;
			this.NotifyPartyStateDropEdit.ShowHorizontalScrollBar = true;
			this.NotifyPartyStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.NotifyPartyStateDropEdit.TabIndex = 34;
			// 
			// NotifyPartyPostcodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyPartyPostcodeTextBox, "Bills.ABL_NotifyPartyPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_NotifyPartyPostcode)));
			this.NotifyPartyPostcodeTextBox.CaptionResourceString = Res.GetData("EA250683-C4D2-4DF3-9C4A-FD08178FAE80", "Postcode");
			this.NotifyPartyPostcodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 196, true);
			this.NotifyPartyPostcodeTextBox.Name = "NotifyPartyPostcodeTextBox";
			this.NotifyPartyPostcodeTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.NotifyPartyPostcodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.NotifyPartyPostcodeTextBox.TabIndex = 35;
			// 
			// NotifyPartyPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyPartyPhoneTextBox, "Bills.ABL_NotifyPartyPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_NotifyPartyPhone)));
			this.NotifyPartyPhoneTextBox.CaptionResourceString = Res.GetData("79E4D5FD-0D5A-42BC-941C-149B418D47BA", "Phone");
			this.NotifyPartyPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 218, true);
			this.NotifyPartyPhoneTextBox.Name = "NotifyPartyPhoneTextBox";
			this.NotifyPartyPhoneTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.NotifyPartyPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.NotifyPartyPhoneTextBox.TabIndex = 36;
			// 
			// NotifyPartyRegNoTypeDropEdit
			// 
			this.NotifyPartyRegNoTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyRegNoTypeDropEdit, "Bills.ABL_NotifyPartyRegNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_NotifyPartyRegNoType)));
			this.NotifyPartyRegNoTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 262, true);
			this.NotifyPartyRegNoTypeDropEdit.Name = "NotifyPartyRegNoTypeDropEdit";
			this.NotifyPartyRegNoTypeDropEdit.ShouldResizeByMaxLength = true;
			this.NotifyPartyRegNoTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.NotifyPartyRegNoTypeDropEdit.TabIndex = 37;
			// 
			// NotifyPartyRegNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyPartyRegNoTextBox, "Bills.ABL_NotifyPartyRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_NotifyPartyRegNo)));
			this.NotifyPartyRegNoTextBox.CaptionResourceString = Res.GetData("CE4052E9-DCC3-4F91-A9CE-9D8CCDDBA6C6", "Reg.No");
			this.NotifyPartyRegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 240, true);
			this.NotifyPartyRegNoTextBox.Name = "NotifyPartyRegNoTextBox";
			this.NotifyPartyRegNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.NotifyPartyRegNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.NotifyPartyRegNoTextBox.TabIndex = 38;
			// 
			// UCC6TemporaryStorageBillPartiesControl
			// 
			this.AutoScroll = true;
			this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 0, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ShipperSeparatorUserControl);
			this.Controls.Add(this.ShipperAddressControl);
			this.Controls.Add(this.ShipperNameTextBox);
			this.Controls.Add(this.ShipperStreet1TextBox);
			this.Controls.Add(this.ShipperStreet2TextBox);
			this.Controls.Add(this.ShipperCityTextBox);
			this.Controls.Add(this.ShipperCountryCodeFindBox);
			this.Controls.Add(this.ShipperStateDropEdit);
			this.Controls.Add(this.ShipperPostCodeTextBox);
			this.Controls.Add(this.ShipperPhoneTextBox);
			this.Controls.Add(this.ShipperRegNoTypeDropEdit);
			this.Controls.Add(this.ShipperRegNoTextBox);
			this.Controls.Add(this.ConsigneeSeparatorUserControl);
			this.Controls.Add(this.ConsigneeAddressControl);
			this.Controls.Add(this.ConsigneeNameTextBox);
			this.Controls.Add(this.ConsigneeStreet1TextBox);
			this.Controls.Add(this.ConsigneeStreet2TextBox);
			this.Controls.Add(this.ConsigneeCityTextBox);
			this.Controls.Add(this.ConigneeCountryCodeFindBox);
			this.Controls.Add(this.ConsigneeStateDropEdit);
			this.Controls.Add(this.ConsigneePostcodeTextBox);
			this.Controls.Add(this.ConsigneePhoneTextBox);
			this.Controls.Add(this.ConsigneeRegNoTypeDropEdit);
			this.Controls.Add(this.ConsigneeRegoNoTextBox);
			this.Controls.Add(this.NotifyPartySeparatorUserControl);
			this.Controls.Add(this.NotifyPartyAddressControl);
			this.Controls.Add(this.NotifyPartyNameTextBox);
			this.Controls.Add(this.NotifyPartyStreet1TextBox);
			this.Controls.Add(this.NotifyPartyStreet2TextBox);
			this.Controls.Add(this.NotifyPartyCityTextBox);
			this.Controls.Add(this.NotifyPartyCountryCodeFindBox);
			this.Controls.Add(this.NotifyPartyStateDropEdit);
			this.Controls.Add(this.NotifyPartyPostcodeTextBox);
			this.Controls.Add(this.NotifyPartyPhoneTextBox);
			this.Controls.Add(this.NotifyPartyRegNoTypeDropEdit);
			this.Controls.Add(this.NotifyPartyRegNoTextBox);
			this.Name = "UCC6TemporaryStorageBillPartiesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 341, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipperSeparatorUserControl.ResumeLayout(true);
			this.ShipperSeparatorUserControl.PerformLayout();
			this.ShipperAddressControl.ResumeLayout(true);
			this.ShipperAddressControl.PerformLayout();
			this.ShipperCountryCodeFindBox.ResumeLayout(true);
			this.ShipperCountryCodeFindBox.PerformLayout();
			this.ShipperStateDropEdit.ResumeLayout(true);
			this.ShipperStateDropEdit.PerformLayout();
			this.ShipperRegNoTypeDropEdit.ResumeLayout(true);
			this.ShipperRegNoTypeDropEdit.PerformLayout();
			this.ConsigneeSeparatorUserControl.ResumeLayout(true);
			this.ConsigneeSeparatorUserControl.PerformLayout();
			this.ConsigneeAddressControl.ResumeLayout(true);
			this.ConsigneeAddressControl.PerformLayout();
			this.ConigneeCountryCodeFindBox.ResumeLayout(true);
			this.ConigneeCountryCodeFindBox.PerformLayout();
			this.ConsigneeStateDropEdit.ResumeLayout(true);
			this.ConsigneeStateDropEdit.PerformLayout();
			this.ConsigneeRegNoTypeDropEdit.ResumeLayout(true);
			this.ConsigneeRegNoTypeDropEdit.PerformLayout();
			this.NotifyPartySeparatorUserControl.ResumeLayout(true);
			this.NotifyPartySeparatorUserControl.PerformLayout();
			this.NotifyPartyAddressControl.ResumeLayout(true);
			this.NotifyPartyAddressControl.PerformLayout();
			this.NotifyPartyCountryCodeFindBox.ResumeLayout(true);
			this.NotifyPartyCountryCodeFindBox.PerformLayout();
			this.NotifyPartyStateDropEdit.ResumeLayout(true);
			this.NotifyPartyStateDropEdit.PerformLayout();
			this.NotifyPartyRegNoTypeDropEdit.ResumeLayout(true);
			this.NotifyPartyRegNoTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl ShipperSeparatorUserControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl ShipperAddressControl;
		internal Enterprise.ZArchitecture.ZTextBox ShipperNameTextBox;
		internal Enterprise.ZArchitecture.ZTextBox ShipperStreet1TextBox;
		internal Enterprise.ZArchitecture.ZTextBox ShipperStreet2TextBox;
		internal Enterprise.ZArchitecture.ZTextBox ShipperCityTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox ShipperCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ShipperStateDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox ShipperPostCodeTextBox;
		internal Enterprise.ZArchitecture.ZTextBox ShipperPhoneTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ShipperRegNoTypeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox ShipperRegNoTextBox;

		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl ConsigneeSeparatorUserControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl ConsigneeAddressControl;
		internal Enterprise.ZArchitecture.ZTextBox ConsigneeNameTextBox;
		internal Enterprise.ZArchitecture.ZTextBox ConsigneeStreet1TextBox;
		internal Enterprise.ZArchitecture.ZTextBox ConsigneeStreet2TextBox;
		internal Enterprise.ZArchitecture.ZTextBox ConsigneeCityTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox ConigneeCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ConsigneeStateDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox ConsigneePostcodeTextBox;
		internal Enterprise.ZArchitecture.ZTextBox ConsigneePhoneTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ConsigneeRegNoTypeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox ConsigneeRegoNoTextBox;

		internal Enterprise.ZArchitecture.GUI.SeparatorUserControl NotifyPartySeparatorUserControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl NotifyPartyAddressControl;
		internal Enterprise.ZArchitecture.ZTextBox NotifyPartyNameTextBox;
		internal Enterprise.ZArchitecture.ZTextBox NotifyPartyStreet1TextBox;
		internal Enterprise.ZArchitecture.ZTextBox NotifyPartyStreet2TextBox;
		internal Enterprise.ZArchitecture.ZTextBox NotifyPartyCityTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox NotifyPartyCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit NotifyPartyStateDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox NotifyPartyPostcodeTextBox;
		internal Enterprise.ZArchitecture.ZTextBox NotifyPartyPhoneTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit NotifyPartyRegNoTypeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox NotifyPartyRegNoTextBox;


		#endregion

	}
}

