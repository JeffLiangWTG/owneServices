using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	partial class AirCTOHawbImportUserControl
	{
		private void InitializeComponent()
		{
			this.consignorGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConsignorStreet2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consignorStreet2Label = new Enterprise.ZArchitecture.ZLabel();
			this.ConsignorAddressFindBox = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.consignorPartyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConsignorStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsignorCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ConsignorPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsignorCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsignorStreetTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsignorNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consignorPostCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consignorStreet1Label = new Enterprise.ZArchitecture.ZLabel();
			this.consignorCityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consignorNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consignorStateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consignorCountryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConsigneeStreet2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consigneeStreet2Label = new Enterprise.ZArchitecture.ZLabel();
			this.ConsigneeAddressFindBox = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.consigneePartyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConsigneeStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneePhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneePostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeStreetTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consigneePostCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeStreet1Label = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeCityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeStateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneePhoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeCountryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConsigneeCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ctoHouseDetailsUserControl1 = new Enterprise.Customs.AU.AirCargo.GUI.CTOHouseDetailsUserControl();
			this.consignorGroupBox.SuspendLayout();
			this.consigneeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// consignorGroupBox
			// 
			this.consignorGroupBox.Controls.Add(this.ConsignorStreet2TextBox);
			this.consignorGroupBox.Controls.Add(this.consignorStreet2Label);
			this.consignorGroupBox.Controls.Add(this.ConsignorAddressFindBox);
			this.consignorGroupBox.Controls.Add(this.consignorPartyLabel);
			this.consignorGroupBox.Controls.Add(this.ConsignorStateTextBox);
			this.consignorGroupBox.Controls.Add(this.ConsignorCountryCodeFindBox);
			this.consignorGroupBox.Controls.Add(this.ConsignorPostCodeTextBox);
			this.consignorGroupBox.Controls.Add(this.ConsignorCityTextBox);
			this.consignorGroupBox.Controls.Add(this.ConsignorStreetTextBox);
			this.consignorGroupBox.Controls.Add(this.ConsignorNameTextBox);
			this.consignorGroupBox.Controls.Add(this.consignorPostCodeLabel);
			this.consignorGroupBox.Controls.Add(this.consignorStreet1Label);
			this.consignorGroupBox.Controls.Add(this.consignorCityLabel);
			this.consignorGroupBox.Controls.Add(this.consignorNameLabel);
			this.consignorGroupBox.Controls.Add(this.consignorStateLabel);
			this.consignorGroupBox.Controls.Add(this.consignorCountryLabel);
			this.consignorGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.consignorGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 24, true);
			this.consignorGroupBox.Name = "consignorGroupBox";
			this.consignorGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 208, true);
			this.consignorGroupBox.TabIndex = 1;
			this.consignorGroupBox.TabStop = false;
			this.consignorGroupBox.Text = "Consignor";
			// 
			// ConsignorStreet2TextBox
			// 
			this.ConsignorStreet2TextBox.BindTo = "CS_ConsignorStreet2";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsignorStreet2Info)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsignorStreet2)));
			this.ConsignorStreet2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsignorStreet2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 88, true);
			this.ConsignorStreet2TextBox.Name = "ConsignorStreet2TextBox";
			this.ConsignorStreet2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ConsignorStreet2TextBox.TabIndex = 7;
			// 
			// consignorStreet2Label
			// 
			this.consignorStreet2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 88, true);
			this.consignorStreet2Label.Name = "consignorStreet2Label";
			this.consignorStreet2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consignorStreet2Label.TabIndex = 6;
			this.consignorStreet2Label.Text = "Street 2";
			// 
			// ConsignorAddressFindBox
			// 
			this.BindingSource.SetBindingMember(this.ConsignorAddressFindBox, "CS_OA_ConsignorAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_OA_ConsignorAddress)));
			this.ConsignorAddressFindBox.BindToOrgList = "Lookups+ConsignorList";
			this.ConsignorAddressFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 16, true);
			this.ConsignorAddressFindBox.Name = "ConsignorAddressFindBox";
			this.ConsignorAddressFindBox.PopupCaption = "";
			this.ConsignorAddressFindBox.ShowAddress = false;
			this.ConsignorAddressFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.ConsignorAddressFindBox.TabIndex = 1;
			// 
			// consignorPartyLabel
			// 
			this.consignorPartyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.consignorPartyLabel.Name = "consignorPartyLabel";
			this.consignorPartyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consignorPartyLabel.TabIndex = 0;
			this.consignorPartyLabel.Text = "Party";
			// 
			// ConsignorStateTextBox
			// 
			this.ConsignorStateTextBox.BindTo = "CS_ConsignorState";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsignorStateInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsignorState)));
			this.ConsignorStateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsignorStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 160, true);
			this.ConsignorStateTextBox.Name = "ConsignorStateTextBox";
			this.ConsignorStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.ConsignorStateTextBox.TabIndex = 13;
			// 
			// ConsignorCountryCodeFindBox
			// 
			this.ConsignorCountryCodeFindBox.BindTo = "CS_RN_NKConsignorCountry";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_RN_NKConsignorCountryInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_RN_NKConsignorCountry)));
			this.ConsignorCountryCodeFindBox.BindToList = "Lookups+ConsignorCountryList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).Lookups.ConsignorCountryList)));
			this.ConsignorCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 136, true);
			this.ConsignorCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.ConsignorCountryCodeFindBox.Name = "ConsignorCountryCodeFindBox";
			this.ConsignorCountryCodeFindBox.PreBoundMaxLength = 2;
			this.ConsignorCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ConsignorCountryCodeFindBox.TabIndex = 11;
			// 
			// ConsignorPostCodeTextBox
			// 
			this.ConsignorPostCodeTextBox.BindTo = "CS_ConsignorPostcode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsignorPostcodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsignorPostcode)));
			this.ConsignorPostCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsignorPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 160, true);
			this.ConsignorPostCodeTextBox.Name = "ConsignorPostCodeTextBox";
			this.ConsignorPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 20, true);
			this.ConsignorPostCodeTextBox.TabIndex = 15;
			// 
			// ConsignorCityTextBox
			// 
			this.ConsignorCityTextBox.BindTo = "CS_ConsignorCity";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsignorCityInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsignorCity)));
			this.ConsignorCityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsignorCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 112, true);
			this.ConsignorCityTextBox.Name = "ConsignorCityTextBox";
			this.ConsignorCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ConsignorCityTextBox.TabIndex = 9;
			// 
			// ConsignorStreetTextBox
			// 
			this.ConsignorStreetTextBox.BindTo = "CS_ConsignorStreet";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsignorStreetInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsignorStreet)));
			this.ConsignorStreetTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsignorStreetTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 64, true);
			this.ConsignorStreetTextBox.Name = "ConsignorStreetTextBox";
			this.ConsignorStreetTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ConsignorStreetTextBox.TabIndex = 5;
			// 
			// ConsignorNameTextBox
			// 
			this.ConsignorNameTextBox.BindTo = "CS_ConsignorName";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsignorNameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsignorName)));
			this.ConsignorNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsignorNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 40, true);
			this.ConsignorNameTextBox.Name = "ConsignorNameTextBox";
			this.ConsignorNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ConsignorNameTextBox.TabIndex = 3;
			// 
			// consignorPostCodeLabel
			// 
			this.consignorPostCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 160, true);
			this.consignorPostCodeLabel.Name = "consignorPostCodeLabel";
			this.consignorPostCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 23, true);
			this.consignorPostCodeLabel.TabIndex = 14;
			this.consignorPostCodeLabel.Text = "PostCode";
			// 
			// consignorStreet1Label
			// 
			this.consignorStreet1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.consignorStreet1Label.Name = "consignorStreet1Label";
			this.consignorStreet1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consignorStreet1Label.TabIndex = 4;
			this.consignorStreet1Label.Text = "Street 1";
			// 
			// consignorCityLabel
			// 
			this.consignorCityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 112, true);
			this.consignorCityLabel.Name = "consignorCityLabel";
			this.consignorCityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consignorCityLabel.TabIndex = 8;
			this.consignorCityLabel.Text = "City";
			// 
			// consignorNameLabel
			// 
			this.consignorNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.consignorNameLabel.Name = "consignorNameLabel";
			this.consignorNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consignorNameLabel.TabIndex = 2;
			this.consignorNameLabel.Text = "Name";
			// 
			// consignorStateLabel
			// 
			this.consignorStateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 160, true);
			this.consignorStateLabel.Name = "consignorStateLabel";
			this.consignorStateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consignorStateLabel.TabIndex = 12;
			this.consignorStateLabel.Text = "State";
			// 
			// consignorCountryLabel
			// 
			this.consignorCountryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 136, true);
			this.consignorCountryLabel.Name = "consignorCountryLabel";
			this.consignorCountryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consignorCountryLabel.TabIndex = 10;
			this.consignorCountryLabel.Text = "Ctry/Rgn.";
			// 
			// consigneeGroupBox
			// 
			this.consigneeGroupBox.Controls.Add(this.ConsigneeStreet2TextBox);
			this.consigneeGroupBox.Controls.Add(this.consigneeStreet2Label);
			this.consigneeGroupBox.Controls.Add(this.ConsigneeAddressFindBox);
			this.consigneeGroupBox.Controls.Add(this.consigneePartyLabel);
			this.consigneeGroupBox.Controls.Add(this.ConsigneeStateTextBox);
			this.consigneeGroupBox.Controls.Add(this.ConsigneePhoneTextBox);
			this.consigneeGroupBox.Controls.Add(this.ConsigneePostCodeTextBox);
			this.consigneeGroupBox.Controls.Add(this.ConsigneeCityTextBox);
			this.consigneeGroupBox.Controls.Add(this.ConsigneeStreetTextBox);
			this.consigneeGroupBox.Controls.Add(this.ConsigneeNameTextBox);
			this.consigneeGroupBox.Controls.Add(this.consigneePostCodeLabel);
			this.consigneeGroupBox.Controls.Add(this.consigneeStreet1Label);
			this.consigneeGroupBox.Controls.Add(this.consigneeCityLabel);
			this.consigneeGroupBox.Controls.Add(this.consigneeNameLabel);
			this.consigneeGroupBox.Controls.Add(this.consigneeStateLabel);
			this.consigneeGroupBox.Controls.Add(this.consigneePhoneLabel);
			this.consigneeGroupBox.Controls.Add(this.consigneeCountryLabel);
			this.consigneeGroupBox.Controls.Add(this.ConsigneeCountryCodeFindBox);
			this.consigneeGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.consigneeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 24, true);
			this.consigneeGroupBox.Name = "consigneeGroupBox";
			this.consigneeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 208, true);
			this.consigneeGroupBox.TabIndex = 0;
			this.consigneeGroupBox.TabStop = false;
			this.consigneeGroupBox.Text = "Consignee";
			// 
			// ConsigneeStreet2TextBox
			// 
			this.ConsigneeStreet2TextBox.BindTo = "CS_ConsigneeStreet2";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneeStreet2Info)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneeStreet2)));
			this.ConsigneeStreet2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsigneeStreet2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 88, true);
			this.ConsigneeStreet2TextBox.Name = "ConsigneeStreet2TextBox";
			this.ConsigneeStreet2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ConsigneeStreet2TextBox.TabIndex = 7;
			// 
			// consigneeStreet2Label
			// 
			this.consigneeStreet2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 88, true);
			this.consigneeStreet2Label.Name = "consigneeStreet2Label";
			this.consigneeStreet2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consigneeStreet2Label.TabIndex = 6;
			this.consigneeStreet2Label.Text = "Street 2";
			// 
			// ConsigneeAddressFindBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeAddressFindBox, "CS_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_OA_ConsigneeAddress)));
			this.ConsigneeAddressFindBox.BindToOrgList = "Lookups+ConsigneeList";
			this.ConsigneeAddressFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 16, true);
			this.ConsigneeAddressFindBox.Name = "ConsigneeAddressFindBox";
			this.ConsigneeAddressFindBox.PopupCaption = "";
			this.ConsigneeAddressFindBox.ShowAddress = false;
			this.ConsigneeAddressFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.ConsigneeAddressFindBox.TabIndex = 1;
			// 
			// consigneePartyLabel
			// 
			this.consigneePartyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.consigneePartyLabel.Name = "consigneePartyLabel";
			this.consigneePartyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consigneePartyLabel.TabIndex = 0;
			this.consigneePartyLabel.Text = "Party";
			// 
			// ConsigneeStateTextBox
			// 
			this.ConsigneeStateTextBox.BindTo = "CS_ConsigneeState";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneeStateInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneeState)));
			this.ConsigneeStateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsigneeStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 160, true);
			this.ConsigneeStateTextBox.Name = "ConsigneeStateTextBox";
			this.ConsigneeStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.ConsigneeStateTextBox.TabIndex = 13;
			// 
			// ConsigneePhoneTextBox
			// 
			this.ConsigneePhoneTextBox.BindTo = "CS_ConsigneePhone";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneePhoneInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneePhone)));
			this.ConsigneePhoneTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsigneePhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 184, true);
			this.ConsigneePhoneTextBox.Name = "ConsigneePhoneTextBox";
			this.ConsigneePhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ConsigneePhoneTextBox.TabIndex = 17;
			// 
			// ConsigneePostCodeTextBox
			// 
			this.ConsigneePostCodeTextBox.BindTo = "CS_ConsigneePostcode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneePostcodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneePostcode)));
			this.ConsigneePostCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsigneePostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 160, true);
			this.ConsigneePostCodeTextBox.Name = "ConsigneePostCodeTextBox";
			this.ConsigneePostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 20, true);
			this.ConsigneePostCodeTextBox.TabIndex = 15;
			// 
			// ConsigneeCityTextBox
			// 
			this.ConsigneeCityTextBox.BindTo = "CS_ConsigneeCity";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneeCityInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneeCity)));
			this.ConsigneeCityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsigneeCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 112, true);
			this.ConsigneeCityTextBox.Name = "ConsigneeCityTextBox";
			this.ConsigneeCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ConsigneeCityTextBox.TabIndex = 9;
			// 
			// ConsigneeStreetTextBox
			// 
			this.ConsigneeStreetTextBox.BindTo = "CS_ConsigneeStreet";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneeStreetInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneeStreet)));
			this.ConsigneeStreetTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsigneeStreetTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 64, true);
			this.ConsigneeStreetTextBox.Name = "ConsigneeStreetTextBox";
			this.ConsigneeStreetTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ConsigneeStreetTextBox.TabIndex = 5;
			// 
			// ConsigneeNameTextBox
			// 
			this.ConsigneeNameTextBox.BindTo = "CS_ConsigneeName";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneeNameInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_ConsigneeName)));
			this.ConsigneeNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsigneeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 40, true);
			this.ConsigneeNameTextBox.Name = "ConsigneeNameTextBox";
			this.ConsigneeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ConsigneeNameTextBox.TabIndex = 3;
			// 
			// consigneePostCodeLabel
			// 
			this.consigneePostCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 160, true);
			this.consigneePostCodeLabel.Name = "consigneePostCodeLabel";
			this.consigneePostCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consigneePostCodeLabel.TabIndex = 14;
			this.consigneePostCodeLabel.Text = "PostCode";
			// 
			// consigneeStreet1Label
			// 
			this.consigneeStreet1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.consigneeStreet1Label.Name = "consigneeStreet1Label";
			this.consigneeStreet1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consigneeStreet1Label.TabIndex = 4;
			this.consigneeStreet1Label.Text = "Street 1";
			// 
			// consigneeCityLabel
			// 
			this.consigneeCityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 112, true);
			this.consigneeCityLabel.Name = "consigneeCityLabel";
			this.consigneeCityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consigneeCityLabel.TabIndex = 8;
			this.consigneeCityLabel.Text = "City";
			// 
			// consigneeNameLabel
			// 
			this.consigneeNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.consigneeNameLabel.Name = "consigneeNameLabel";
			this.consigneeNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consigneeNameLabel.TabIndex = 2;
			this.consigneeNameLabel.Text = "Name";
			// 
			// consigneeStateLabel
			// 
			this.consigneeStateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 160, true);
			this.consigneeStateLabel.Name = "consigneeStateLabel";
			this.consigneeStateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consigneeStateLabel.TabIndex = 12;
			this.consigneeStateLabel.Text = "State";
			// 
			// consigneePhoneLabel
			// 
			this.consigneePhoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 184, true);
			this.consigneePhoneLabel.Name = "consigneePhoneLabel";
			this.consigneePhoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 15, true);
			this.consigneePhoneLabel.TabIndex = 16;
			this.consigneePhoneLabel.Text = "Phone";
			// 
			// consigneeCountryLabel
			// 
			this.consigneeCountryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 136, true);
			this.consigneeCountryLabel.Name = "consigneeCountryLabel";
			this.consigneeCountryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 23, true);
			this.consigneeCountryLabel.TabIndex = 10;
			this.consigneeCountryLabel.Text = "Ctry/Rgn.";
			// 
			// ConsigneeCountryCodeFindBox
			// 
			this.ConsigneeCountryCodeFindBox.BindTo = "CS_RN_NKConsigneeCountry";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_RN_NKConsigneeCountryInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).CS_RN_NKConsigneeCountry)));
			this.ConsigneeCountryCodeFindBox.BindToList = "Lookups+ConsigneeCountryList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB)(null)).Lookups.ConsigneeCountryList)));
			this.ConsigneeCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 136, true);
			this.ConsigneeCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.ConsigneeCountryCodeFindBox.Name = "ConsigneeCountryCodeFindBox";
			this.ConsigneeCountryCodeFindBox.PreBoundMaxLength = 2;
			this.ConsigneeCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ConsigneeCountryCodeFindBox.TabIndex = 11;
			// 
			// ctoHouseDetailsUserControl1
			// 
			this.ctoHouseDetailsUserControl1.ShouldSerializeTabPageMethods = true;
			this.ctoHouseDetailsUserControl1.HAWB = null;
			this.ctoHouseDetailsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 256, true);
			this.ctoHouseDetailsUserControl1.Name = "ctoHouseDetailsUserControl1";
			this.ctoHouseDetailsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 243, true);
			this.ctoHouseDetailsUserControl1.TabIndex = 2;
			// 
			// AirCTOHawbImportUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.consigneeGroupBox);
			this.Controls.Add(this.consignorGroupBox);
			this.Controls.Add(this.ctoHouseDetailsUserControl1);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB";
			this.Name = "AirCTOHawbImportUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 508, true);
			this.consignorGroupBox.ResumeLayout(false);
			this.consignorGroupBox.PerformLayout();
			this.consigneeGroupBox.ResumeLayout(false);
			this.consigneeGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		private CTOHouseDetailsUserControl ctoHouseDetailsUserControl1;
		private Enterprise.ZArchitecture.GUI.ZGroupBox consignorGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox consigneeGroupBox;
		protected Enterprise.ZArchitecture.ZTextBox ConsigneeStreet2TextBox;
		private Enterprise.ZArchitecture.ZLabel consigneeStreet2Label;
		protected Enterprise.ZArchitecture.GUI.ZAddressControl ConsigneeAddressFindBox;
		private Enterprise.ZArchitecture.ZLabel consigneePartyLabel;
		protected Enterprise.ZArchitecture.ZTextBox ConsigneeStateTextBox;
		protected Enterprise.ZArchitecture.ZTextBox ConsigneePhoneTextBox;
		protected Enterprise.ZArchitecture.ZTextBox ConsigneePostCodeTextBox;
		protected Enterprise.ZArchitecture.ZTextBox ConsigneeCityTextBox;
		protected Enterprise.ZArchitecture.ZTextBox ConsigneeStreetTextBox;
		protected Enterprise.ZArchitecture.ZTextBox ConsigneeNameTextBox;
		private Enterprise.ZArchitecture.ZLabel consigneePostCodeLabel;
		private Enterprise.ZArchitecture.ZLabel consigneeStreet1Label;
		private Enterprise.ZArchitecture.ZLabel consigneeCityLabel;
		private Enterprise.ZArchitecture.ZLabel consigneeNameLabel;
		private Enterprise.ZArchitecture.ZLabel consigneeStateLabel;
		private Enterprise.ZArchitecture.ZLabel consigneePhoneLabel;
		private Enterprise.ZArchitecture.ZLabel consigneeCountryLabel;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox ConsigneeCountryCodeFindBox;
		protected Enterprise.ZArchitecture.ZTextBox ConsignorStreet2TextBox;
		private Enterprise.ZArchitecture.ZLabel consignorStreet2Label;
		protected Enterprise.ZArchitecture.GUI.ZAddressControl ConsignorAddressFindBox;
		private Enterprise.ZArchitecture.ZLabel consignorPartyLabel;
		protected Enterprise.ZArchitecture.ZTextBox ConsignorStateTextBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox ConsignorCountryCodeFindBox;
		protected Enterprise.ZArchitecture.ZTextBox ConsignorPostCodeTextBox;
		protected Enterprise.ZArchitecture.ZTextBox ConsignorCityTextBox;
		protected Enterprise.ZArchitecture.ZTextBox ConsignorStreetTextBox;
		protected Enterprise.ZArchitecture.ZTextBox ConsignorNameTextBox;
		private Enterprise.ZArchitecture.ZLabel consignorPostCodeLabel;
		private Enterprise.ZArchitecture.ZLabel consignorStreet1Label;
		private Enterprise.ZArchitecture.ZLabel consignorCityLabel;
		private Enterprise.ZArchitecture.ZLabel consignorNameLabel;
		private Enterprise.ZArchitecture.ZLabel consignorStateLabel;
		private Enterprise.ZArchitecture.ZLabel consignorCountryLabel;
	}
}
