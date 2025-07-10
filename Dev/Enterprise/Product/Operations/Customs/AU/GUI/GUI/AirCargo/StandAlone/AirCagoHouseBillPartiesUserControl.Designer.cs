namespace Enterprise.Customs.AU.GUI
{
	partial class AirCagoHouseBillPartiesUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components;

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
			this.components = new System.ComponentModel.Container();
			this.miscInfoTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.consigneeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.consigneeStreet2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consigneeStreet2Label = new Enterprise.ZArchitecture.ZLabel();
			this.ConsigneezAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.cS_OH_ConsigneeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.cS_OH_ConsigneeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consigneePhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consigneeImporterABNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consigneeImporterIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consigneePostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consigneeCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consigneeStreetTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consigneeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consigneePostCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeStreetLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeCityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeStateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneePhoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeImporterABNLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeImporterIdentifierLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeCountryCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consigneeCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.consignorTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.consignorPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consignorPhoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.cS_ConsignorIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.cS_ConsignorIdentifierLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consignorStreet2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consignorStreet2Label = new Enterprise.ZArchitecture.ZLabel();
			this.ConsignorzAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.cS_OH_ConsignorGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.cS_OH_ConsignorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consignorStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consignorCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.consignorPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consignorVendorIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consignorCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consignorStreetTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consignorNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consignorPostCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consignorVendorIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consignorStreetLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consignorCityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consignorNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consignorStateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.consignorCountryCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.miscInfoTabControl.SuspendLayout();
			this.consigneeTabPage.SuspendLayout();
			this.ConsigneezAddressControl.SuspendLayout();
			this.cS_OH_ConsigneeGuidFindBox.SuspendLayout();
			this.consigneeCountryCodeFindBox.SuspendLayout();
			this.consignorTabPage.SuspendLayout();
			this.ConsignorzAddressControl.SuspendLayout();
			this.cS_OH_ConsignorGuidFindBox.SuspendLayout();
			this.consignorCountryCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusHAWBBase);
			// 
			// miscInfoTabControl
			// 
			this.miscInfoTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.miscInfoTabControl.Controls.Add(this.consigneeTabPage);
			this.miscInfoTabControl.Controls.Add(this.consignorTabPage);
			this.miscInfoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.miscInfoTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.miscInfoTabControl.Name = "miscInfoTabControl";
			this.miscInfoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 270, true);
			this.miscInfoTabControl.TabIndex = 0;
			// 
			// consigneeTabPage
			// 
			this.consigneeTabPage.AutoScroll = true;
			this.consigneeTabPage.Controls.Add(this.consigneeStreet2TextBox);
			this.consigneeTabPage.Controls.Add(this.consigneeStreet2Label);
			this.consigneeTabPage.Controls.Add(this.ConsigneezAddressControl);
			this.consigneeTabPage.Controls.Add(this.cS_OH_ConsigneeGuidFindBox);
			this.consigneeTabPage.Controls.Add(this.cS_OH_ConsigneeLabel);
			this.consigneeTabPage.Controls.Add(this.consigneeStateTextBox);
			this.consigneeTabPage.Controls.Add(this.consigneePhoneTextBox);
			this.consigneeTabPage.Controls.Add(this.consigneeImporterABNTextBox);
			this.consigneeTabPage.Controls.Add(this.consigneeImporterIdentifierTextBox);
			this.consigneeTabPage.Controls.Add(this.consigneePostCodeTextBox);
			this.consigneeTabPage.Controls.Add(this.consigneeCityTextBox);
			this.consigneeTabPage.Controls.Add(this.consigneeStreetTextBox);
			this.consigneeTabPage.Controls.Add(this.consigneeNameTextBox);
			this.consigneeTabPage.Controls.Add(this.consigneePostCodeLabel);
			this.consigneeTabPage.Controls.Add(this.consigneeStreetLabel);
			this.consigneeTabPage.Controls.Add(this.consigneeCityLabel);
			this.consigneeTabPage.Controls.Add(this.consigneeNameLabel);
			this.consigneeTabPage.Controls.Add(this.consigneeStateLabel);
			this.consigneeTabPage.Controls.Add(this.consigneePhoneLabel);
			this.consigneeTabPage.Controls.Add(this.consigneeImporterABNLabel);
			this.consigneeTabPage.Controls.Add(this.consigneeImporterIdentifierLabel);
			this.consigneeTabPage.Controls.Add(this.consigneeCountryCodeLabel);
			this.consigneeTabPage.Controls.Add(this.consigneeCountryCodeFindBox);
			this.consigneeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.consigneeTabPage.Name = "consigneeTabPage";
			this.consigneeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 243, true);
			this.consigneeTabPage.TabIndex = 0;
			this.consigneeTabPage.Text = "Consignee";
			// 
			// consigneeStreet2TextBox
			// 
			this.BindingSource.SetBindingMember(this.consigneeStreet2TextBox, "CS_ConsigneeStreet2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsigneeStreet2)));
			this.consigneeStreet2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consigneeStreet2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 79, true);
			this.consigneeStreet2TextBox.Name = "consigneeStreet2TextBox";
			this.consigneeStreet2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consigneeStreet2TextBox.TabIndex = 7;
			// 
			// consigneeStreet2Label
			// 
			this.consigneeStreet2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consigneeStreet2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 80, true);
			this.consigneeStreet2Label.Name = "consigneeStreet2Label";
			this.consigneeStreet2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 15, true);
			this.consigneeStreet2Label.TabIndex = 6;
			this.consigneeStreet2Label.Text = "Street 2";
			this.consigneeStreet2Label.UseMnemonic = false;
			// 
			// ConsigneezAddressControl
			// 
			this.ConsigneezAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneezAddressControl, "CS_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_OA_ConsigneeAddress)));
			this.ConsigneezAddressControl.BindToOrgList = "Lookups+ConsigneeList";
			this.ConsigneezAddressControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AC10133D-0AC2-44ED-BB5E-AA9858053AC6", "Org. Code", "Consignee Address", "Consignee Organization Address");
			this.ConsigneezAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 8, true);
			this.ConsigneezAddressControl.Name = "ConsigneezAddressControl";
			this.ConsigneezAddressControl.PopupCaption = "";
			this.ConsigneezAddressControl.ShowAddress = false;
			this.ConsigneezAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ConsigneezAddressControl.TabIndex = 1;
			// 
			// cS_OH_ConsigneeGuidFindBox
			// 
			this.cS_OH_ConsigneeGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cS_OH_ConsigneeGuidFindBox, "CS_OH_Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_OH_Consignee)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).Lookups.ConsigneeList)));
			this.cS_OH_ConsigneeGuidFindBox.BindToList = "Lookups+ConsigneeList";
			this.cS_OH_ConsigneeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 8, true);
			this.cS_OH_ConsigneeGuidFindBox.Name = "cS_OH_ConsigneeGuidFindBox";
			this.cS_OH_ConsigneeGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.cS_OH_ConsigneeGuidFindBox.ParentType = null;
			this.cS_OH_ConsigneeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.cS_OH_ConsigneeGuidFindBox.TabIndex = 1;
			// 
			// cS_OH_ConsigneeLabel
			// 
			this.cS_OH_ConsigneeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.cS_OH_ConsigneeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 5, true);
			this.cS_OH_ConsigneeLabel.Name = "cS_OH_ConsigneeLabel";
			this.cS_OH_ConsigneeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 23, true);
			this.cS_OH_ConsigneeLabel.TabIndex = 0;
			this.cS_OH_ConsigneeLabel.Text = "Party";
			this.cS_OH_ConsigneeLabel.UseMnemonic = false;
			// 
			// consigneeStateTextBox
			// 
			this.BindingSource.SetBindingMember(this.consigneeStateTextBox, "CS_ConsigneeState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsigneeState)));
			this.consigneeStateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consigneeStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 148, true);
			this.consigneeStateTextBox.Name = "consigneeStateTextBox";
			this.consigneeStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.consigneeStateTextBox.TabIndex = 13;
			// 
			// consigneePhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.consigneePhoneTextBox, "CS_ConsigneePhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsigneePhone)));
			this.consigneePhoneTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consigneePhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 170, true);
			this.consigneePhoneTextBox.Name = "consigneePhoneTextBox";
			this.consigneePhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consigneePhoneTextBox.TabIndex = 17;
			// 
			// consigneeImporterABNTextBox
			// 
			this.BindingSource.SetBindingMember(this.consigneeImporterABNTextBox, "CS_ConsigneeBusinessNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsigneeBusinessNumber)));
			this.consigneeImporterABNTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consigneeImporterABNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 193, true);
			this.consigneeImporterABNTextBox.Name = "consigneeImporterABNTextBox";
			this.consigneeImporterABNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consigneeImporterABNTextBox.TabIndex = 19;
			// 
			// consigneeImporterIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.consigneeImporterIdentifierTextBox, "CS_ConsigneeIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsigneeIdentifier)));
			this.consigneeImporterIdentifierTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consigneeImporterIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 215, true);
			this.consigneeImporterIdentifierTextBox.Name = "consigneeImporterIdentifierTextBox";
			this.consigneeImporterIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consigneeImporterIdentifierTextBox.TabIndex = 21;
			// 
			// consigneePostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.consigneePostCodeTextBox, "CS_ConsigneePostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsigneePostcode)));
			this.consigneePostCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consigneePostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 148, true);
			this.consigneePostCodeTextBox.Name = "consigneePostCodeTextBox";
			this.consigneePostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.consigneePostCodeTextBox.TabIndex = 15;
			// 
			// consigneeCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.consigneeCityTextBox, "CS_ConsigneeCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsigneeCity)));
			this.consigneeCityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consigneeCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 102, true);
			this.consigneeCityTextBox.Name = "consigneeCityTextBox";
			this.consigneeCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consigneeCityTextBox.TabIndex = 9;
			// 
			// consigneeStreetTextBox
			// 
			this.BindingSource.SetBindingMember(this.consigneeStreetTextBox, "CS_ConsigneeStreet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsigneeStreet)));
			this.consigneeStreetTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consigneeStreetTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 57, true);
			this.consigneeStreetTextBox.Name = "consigneeStreetTextBox";
			this.consigneeStreetTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consigneeStreetTextBox.TabIndex = 5;
			// 
			// consigneeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.consigneeNameTextBox, "CS_ConsigneeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsigneeName)));
			this.consigneeNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consigneeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 33, true);
			this.consigneeNameTextBox.Name = "consigneeNameTextBox";
			this.consigneeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consigneeNameTextBox.TabIndex = 3;
			// 
			// consigneePostCodeLabel
			// 
			this.consigneePostCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consigneePostCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 149, true);
			this.consigneePostCodeLabel.Name = "consigneePostCodeLabel";
			this.consigneePostCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 16, true);
			this.consigneePostCodeLabel.TabIndex = 14;
			this.consigneePostCodeLabel.Text = "PostCode";
			this.consigneePostCodeLabel.UseMnemonic = false;
			// 
			// consigneeStreetLabel
			// 
			this.consigneeStreetLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consigneeStreetLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 58, true);
			this.consigneeStreetLabel.Name = "consigneeStreetLabel";
			this.consigneeStreetLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 15, true);
			this.consigneeStreetLabel.TabIndex = 4;
			this.consigneeStreetLabel.Text = "Street 1";
			this.consigneeStreetLabel.UseMnemonic = false;
			// 
			// consigneeCityLabel
			// 
			this.consigneeCityLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consigneeCityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 103, true);
			this.consigneeCityLabel.Name = "consigneeCityLabel";
			this.consigneeCityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.consigneeCityLabel.TabIndex = 8;
			this.consigneeCityLabel.Text = "City";
			this.consigneeCityLabel.UseMnemonic = false;
			// 
			// consigneeNameLabel
			// 
			this.consigneeNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consigneeNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 34, true);
			this.consigneeNameLabel.Name = "consigneeNameLabel";
			this.consigneeNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.consigneeNameLabel.TabIndex = 2;
			this.consigneeNameLabel.Text = "Name";
			this.consigneeNameLabel.UseMnemonic = false;
			// 
			// consigneeStateLabel
			// 
			this.consigneeStateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consigneeStateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 149, true);
			this.consigneeStateLabel.Name = "consigneeStateLabel";
			this.consigneeStateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.consigneeStateLabel.TabIndex = 12;
			this.consigneeStateLabel.Text = "State";
			this.consigneeStateLabel.UseMnemonic = false;
			// 
			// consigneePhoneLabel
			// 
			this.consigneePhoneLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consigneePhoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 172, true);
			this.consigneePhoneLabel.Name = "consigneePhoneLabel";
			this.consigneePhoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.consigneePhoneLabel.TabIndex = 16;
			this.consigneePhoneLabel.Text = "Phone";
			this.consigneePhoneLabel.UseMnemonic = false;
			// 
			// consigneeImporterABNLabel
			// 
			this.consigneeImporterABNLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consigneeImporterABNLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 195, true);
			this.consigneeImporterABNLabel.Name = "consigneeImporterABNLabel";
			this.consigneeImporterABNLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.consigneeImporterABNLabel.TabIndex = 18;
			this.consigneeImporterABNLabel.Text = "ABN";
			this.consigneeImporterABNLabel.UseMnemonic = false;
			// 
			// consigneeImporterIdentifierLabel
			// 
			this.consigneeImporterIdentifierLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consigneeImporterIdentifierLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 217, true);
			this.consigneeImporterIdentifierLabel.Name = "consigneeImporterIdentifierLabel";
			this.consigneeImporterIdentifierLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 15, true);
			this.consigneeImporterIdentifierLabel.TabIndex = 20;
			this.consigneeImporterIdentifierLabel.Text = "Identifier";
			this.consigneeImporterIdentifierLabel.UseMnemonic = false;
			// 
			// consigneeCountryCodeLabel
			// 
			this.consigneeCountryCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consigneeCountryCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 126, true);
			this.consigneeCountryCodeLabel.Name = "consigneeCountryCodeLabel";
			this.consigneeCountryCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 15, true);
			this.consigneeCountryCodeLabel.TabIndex = 10;
			this.consigneeCountryCodeLabel.Text = "Ctry/Rgn.";
			this.consigneeCountryCodeLabel.UseMnemonic = false;
			// 
			// consigneeCountryCodeFindBox
			// 
			this.consigneeCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.consigneeCountryCodeFindBox, "CS_RN_NKConsigneeCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_RN_NKConsigneeCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).Lookups.ConsigneeCountryList)));
			this.consigneeCountryCodeFindBox.BindToList = "Lookups+ConsigneeCountryList";
			this.consigneeCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 125, true);
			this.consigneeCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.consigneeCountryCodeFindBox.Name = "consigneeCountryCodeFindBox";
			this.consigneeCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.consigneeCountryCodeFindBox.ParentType = null;
			this.consigneeCountryCodeFindBox.PreBoundMaxLength = 2;
			this.consigneeCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consigneeCountryCodeFindBox.TabIndex = 11;
			// 
			// consignorTabPage
			// 
			this.consignorTabPage.AutoScroll = true;
			this.consignorTabPage.Controls.Add(this.consignorPhoneTextBox);
			this.consignorTabPage.Controls.Add(this.consignorPhoneLabel);
			this.consignorTabPage.Controls.Add(this.cS_ConsignorIdentifierTextBox);
			this.consignorTabPage.Controls.Add(this.cS_ConsignorIdentifierLabel);
			this.consignorTabPage.Controls.Add(this.consignorStreet2TextBox);
			this.consignorTabPage.Controls.Add(this.consignorStreet2Label);
			this.consignorTabPage.Controls.Add(this.ConsignorzAddressControl);
			this.consignorTabPage.Controls.Add(this.cS_OH_ConsignorGuidFindBox);
			this.consignorTabPage.Controls.Add(this.cS_OH_ConsignorLabel);
			this.consignorTabPage.Controls.Add(this.consignorStateTextBox);
			this.consignorTabPage.Controls.Add(this.consignorCountryCodeFindBox);
			this.consignorTabPage.Controls.Add(this.consignorPostCodeTextBox);
			this.consignorTabPage.Controls.Add(this.consignorVendorIDTextBox);
			this.consignorTabPage.Controls.Add(this.consignorCityTextBox);
			this.consignorTabPage.Controls.Add(this.consignorStreetTextBox);
			this.consignorTabPage.Controls.Add(this.consignorNameTextBox);
			this.consignorTabPage.Controls.Add(this.consignorPostCodeLabel);
			this.consignorTabPage.Controls.Add(this.consignorVendorIDLabel);
			this.consignorTabPage.Controls.Add(this.consignorStreetLabel);
			this.consignorTabPage.Controls.Add(this.consignorCityLabel);
			this.consignorTabPage.Controls.Add(this.consignorNameLabel);
			this.consignorTabPage.Controls.Add(this.consignorStateLabel);
			this.consignorTabPage.Controls.Add(this.consignorCountryCodeLabel);
			this.consignorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.consignorTabPage.Name = "consignorTabPage";
			this.consignorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 243, true);
			this.consignorTabPage.TabIndex = 1;
			this.consignorTabPage.Text = "Consignor";
			// 
			// consignorPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.consignorPhoneTextBox, "CS_ConsignorPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsignorPhone)));
			this.consignorPhoneTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consignorPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 170, true);
			this.consignorPhoneTextBox.Name = "consignorPhoneTextBox";
			this.consignorPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consignorPhoneTextBox.TabIndex = 17;
			// 
			// consignorPhoneLabel
			// 
			this.consignorPhoneLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consignorPhoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 172, true);
			this.consignorPhoneLabel.Name = "consignorPhoneLabel";
			this.consignorPhoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.consignorPhoneLabel.TabIndex = 16;
			this.consignorPhoneLabel.Text = "Phone";
			this.consignorPhoneLabel.UseMnemonic = false;
			// 
			// cS_ConsignorIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.cS_ConsignorIdentifierTextBox, "CS_ConsignorIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsignorIdentifier)));
			this.cS_ConsignorIdentifierTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.cS_ConsignorIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 215, true);
			this.cS_ConsignorIdentifierTextBox.Name = "cS_ConsignorIdentifierTextBox";
			this.cS_ConsignorIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.cS_ConsignorIdentifierTextBox.TabIndex = 21;
			// 
			// cS_ConsignorIdentifierLabel
			// 
			this.cS_ConsignorIdentifierLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.cS_ConsignorIdentifierLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 213, true);
			this.cS_ConsignorIdentifierLabel.Name = "cS_ConsignorIdentifierLabel";
			this.cS_ConsignorIdentifierLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 23, true);
			this.cS_ConsignorIdentifierLabel.TabIndex = 20;
			this.cS_ConsignorIdentifierLabel.Text = "Supplier ID";
			this.cS_ConsignorIdentifierLabel.UseMnemonic = false;
			// 
			// consignorStreet2TextBox
			// 
			this.BindingSource.SetBindingMember(this.consignorStreet2TextBox, "CS_ConsignorStreet2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsignorStreet2)));
			this.consignorStreet2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consignorStreet2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 79, true);
			this.consignorStreet2TextBox.Name = "consignorStreet2TextBox";
			this.consignorStreet2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consignorStreet2TextBox.TabIndex = 7;
			// 
			// consignorStreet2Label
			// 
			this.consignorStreet2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consignorStreet2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 76, true);
			this.consignorStreet2Label.Name = "consignorStreet2Label";
			this.consignorStreet2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 23, true);
			this.consignorStreet2Label.TabIndex = 6;
			this.consignorStreet2Label.Text = "Street 2";
			this.consignorStreet2Label.UseMnemonic = false;
			// 
			// ConsignorzAddressControl
			// 
			this.ConsignorzAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorzAddressControl, "CS_OA_ConsignorAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_OA_ConsignorAddress)));
			this.ConsignorzAddressControl.BindToOrgList = "Lookups+ConsignorList";
			this.ConsignorzAddressControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("727A153D-A08F-40BE-882F-48D004AD87AD", "Org. Code", "Consignor Address", "Consignor Organization Address");
			this.ConsignorzAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 8, true);
			this.ConsignorzAddressControl.Name = "ConsignorzAddressControl";
			this.ConsignorzAddressControl.PopupCaption = "";
			this.ConsignorzAddressControl.ShowAddress = false;
			this.ConsignorzAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ConsignorzAddressControl.TabIndex = 1;
			// 
			// cS_OH_ConsignorGuidFindBox
			// 
			this.cS_OH_ConsignorGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cS_OH_ConsignorGuidFindBox, "CS_OH_Consignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_OH_Consignor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).Lookups.ConsignorList)));
			this.cS_OH_ConsignorGuidFindBox.BindToList = "Lookups+ConsignorList";
			this.cS_OH_ConsignorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 8, true);
			this.cS_OH_ConsignorGuidFindBox.Name = "cS_OH_ConsignorGuidFindBox";
			this.cS_OH_ConsignorGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.cS_OH_ConsignorGuidFindBox.ParentType = null;
			this.cS_OH_ConsignorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.cS_OH_ConsignorGuidFindBox.TabIndex = 0;
			// 
			// cS_OH_ConsignorLabel
			// 
			this.cS_OH_ConsignorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.cS_OH_ConsignorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 5, true);
			this.cS_OH_ConsignorLabel.Name = "cS_OH_ConsignorLabel";
			this.cS_OH_ConsignorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 23, true);
			this.cS_OH_ConsignorLabel.TabIndex = 0;
			this.cS_OH_ConsignorLabel.Text = "Party";
			this.cS_OH_ConsignorLabel.UseMnemonic = false;
			// 
			// consignorStateTextBox
			// 
			this.BindingSource.SetBindingMember(this.consignorStateTextBox, "CS_ConsignorState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsignorState)));
			this.consignorStateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consignorStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 148, true);
			this.consignorStateTextBox.Name = "consignorStateTextBox";
			this.consignorStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.consignorStateTextBox.TabIndex = 13;
			// 
			// consignorCountryCodeFindBox
			// 
			this.consignorCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.consignorCountryCodeFindBox, "CS_RN_NKConsignorCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_RN_NKConsignorCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).Lookups.ConsignorCountryList)));
			this.consignorCountryCodeFindBox.BindToList = "Lookups+ConsignorCountryList";
			this.consignorCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 125, true);
			this.consignorCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.consignorCountryCodeFindBox.Name = "consignorCountryCodeFindBox";
			this.consignorCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.consignorCountryCodeFindBox.ParentType = null;
			this.consignorCountryCodeFindBox.PreBoundMaxLength = 2;
			this.consignorCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consignorCountryCodeFindBox.TabIndex = 11;
			// 
			// consignorPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.consignorPostCodeTextBox, "CS_ConsignorPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsignorPostcode)));
			this.consignorPostCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consignorPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 148, true);
			this.consignorPostCodeTextBox.Name = "consignorPostCodeTextBox";
			this.consignorPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.consignorPostCodeTextBox.TabIndex = 15;
			// 
			// consignorVendorIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.consignorVendorIDTextBox, "CS_VendorIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_VendorIdentifier)));
			this.consignorVendorIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consignorVendorIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 193, true);
			this.consignorVendorIDTextBox.Name = "consignorVendorIDTextBox";
			this.consignorVendorIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consignorVendorIDTextBox.TabIndex = 19;
			// 
			// consignorCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.consignorCityTextBox, "CS_ConsignorCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsignorCity)));
			this.consignorCityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consignorCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 102, true);
			this.consignorCityTextBox.Name = "consignorCityTextBox";
			this.consignorCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consignorCityTextBox.TabIndex = 9;
			// 
			// consignorStreetTextBox
			// 
			this.BindingSource.SetBindingMember(this.consignorStreetTextBox, "CS_ConsignorStreet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsignorStreet)));
			this.consignorStreetTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consignorStreetTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 57, true);
			this.consignorStreetTextBox.Name = "consignorStreetTextBox";
			this.consignorStreetTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consignorStreetTextBox.TabIndex = 5;
			// 
			// consignorNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.consignorNameTextBox, "CS_ConsignorName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusHAWBBase)(null)).CS_ConsignorName)));
			this.consignorNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.consignorNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 33, true);
			this.consignorNameTextBox.Name = "consignorNameTextBox";
			this.consignorNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.consignorNameTextBox.TabIndex = 3;
			// 
			// consignorPostCodeLabel
			// 
			this.consignorPostCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consignorPostCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 148, true);
			this.consignorPostCodeLabel.Name = "consignorPostCodeLabel";
			this.consignorPostCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 19, true);
			this.consignorPostCodeLabel.TabIndex = 14;
			this.consignorPostCodeLabel.Text = "PostCode";
			this.consignorPostCodeLabel.UseMnemonic = false;
			// 
			// consignorVendorIDLabel
			// 
			this.consignorVendorIDLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consignorVendorIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 192, true);
			this.consignorVendorIDLabel.Name = "consignorVendorIDLabel";
			this.consignorVendorIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 23, true);
			this.consignorVendorIDLabel.TabIndex = 18;
			this.consignorVendorIDLabel.Text = "Vendor";
			this.consignorVendorIDLabel.UseMnemonic = false;
			// 
			// consignorStreetLabel
			// 
			this.consignorStreetLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consignorStreetLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 54, true);
			this.consignorStreetLabel.Name = "consignorStreetLabel";
			this.consignorStreetLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 23, true);
			this.consignorStreetLabel.TabIndex = 4;
			this.consignorStreetLabel.Text = "Street 1";
			this.consignorStreetLabel.UseMnemonic = false;
			// 
			// consignorCityLabel
			// 
			this.consignorCityLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consignorCityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 99, true);
			this.consignorCityLabel.Name = "consignorCityLabel";
			this.consignorCityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 23, true);
			this.consignorCityLabel.TabIndex = 8;
			this.consignorCityLabel.Text = "City";
			this.consignorCityLabel.UseMnemonic = false;
			// 
			// consignorNameLabel
			// 
			this.consignorNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consignorNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 30, true);
			this.consignorNameLabel.Name = "consignorNameLabel";
			this.consignorNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 23, true);
			this.consignorNameLabel.TabIndex = 2;
			this.consignorNameLabel.Text = "Name";
			this.consignorNameLabel.UseMnemonic = false;
			// 
			// consignorStateLabel
			// 
			this.consignorStateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consignorStateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 145, true);
			this.consignorStateLabel.Name = "consignorStateLabel";
			this.consignorStateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 23, true);
			this.consignorStateLabel.TabIndex = 12;
			this.consignorStateLabel.Text = "State";
			this.consignorStateLabel.UseMnemonic = false;
			// 
			// consignorCountryCodeLabel
			// 
			this.consignorCountryCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.consignorCountryCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 122, true);
			this.consignorCountryCodeLabel.Name = "consignorCountryCodeLabel";
			this.consignorCountryCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 23, true);
			this.consignorCountryCodeLabel.TabIndex = 10;
			this.consignorCountryCodeLabel.Text = "Ctry/Rgn.";
			this.consignorCountryCodeLabel.UseMnemonic = false;
			// 
			// AirCagoHouseBillPartiesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.miscInfoTabControl);
			this.Name = "AirCagoHouseBillPartiesUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 270, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.miscInfoTabControl.ResumeLayout(false);
			this.miscInfoTabControl.PerformLayout();
			this.consigneeTabPage.ResumeLayout(false);
			this.consigneeTabPage.PerformLayout();
			this.ConsigneezAddressControl.ResumeLayout(true);
			this.ConsigneezAddressControl.PerformLayout();
			this.cS_OH_ConsigneeGuidFindBox.ResumeLayout(true);
			this.cS_OH_ConsigneeGuidFindBox.PerformLayout();
			this.consigneeCountryCodeFindBox.ResumeLayout(true);
			this.consigneeCountryCodeFindBox.PerformLayout();
			this.consignorTabPage.ResumeLayout(false);
			this.consignorTabPage.PerformLayout();
			this.ConsignorzAddressControl.ResumeLayout(true);
			this.ConsignorzAddressControl.PerformLayout();
			this.cS_OH_ConsignorGuidFindBox.ResumeLayout(true);
			this.cS_OH_ConsignorGuidFindBox.PerformLayout();
			this.consignorCountryCodeFindBox.ResumeLayout(true);
			this.consignorCountryCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZTemplateTabControl miscInfoTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage consigneeTabPage;
		Enterprise.ZArchitecture.ZTextBox consigneeStreet2TextBox;
		Enterprise.ZArchitecture.ZLabel consigneeStreet2Label;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox cS_OH_ConsigneeGuidFindBox;
		Enterprise.ZArchitecture.ZLabel cS_OH_ConsigneeLabel;
		Enterprise.ZArchitecture.ZTextBox consigneeStateTextBox;
		Enterprise.ZArchitecture.ZTextBox consigneePhoneTextBox;
		Enterprise.ZArchitecture.ZTextBox consigneePostCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox consigneeCityTextBox;
		Enterprise.ZArchitecture.ZTextBox consigneeStreetTextBox;
		Enterprise.ZArchitecture.ZTextBox consigneeNameTextBox;
		Enterprise.ZArchitecture.ZTextBox consigneeImporterABNTextBox;
		Enterprise.ZArchitecture.ZTextBox consigneeImporterIdentifierTextBox;
		Enterprise.ZArchitecture.ZLabel consigneePostCodeLabel;
		Enterprise.ZArchitecture.ZLabel consigneeStreetLabel;
		Enterprise.ZArchitecture.ZLabel consigneeCityLabel;
		Enterprise.ZArchitecture.ZLabel consigneeNameLabel;
		Enterprise.ZArchitecture.ZLabel consigneeStateLabel;
		Enterprise.ZArchitecture.ZLabel consigneePhoneLabel;
		Enterprise.ZArchitecture.ZLabel consigneeCountryCodeLabel;
		Enterprise.ZArchitecture.ZLabel consigneeImporterABNLabel;
		Enterprise.ZArchitecture.ZLabel consigneeImporterIdentifierLabel;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox consigneeCountryCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZTabPage consignorTabPage;
		Enterprise.ZArchitecture.ZTextBox consignorStreet2TextBox;
		Enterprise.ZArchitecture.ZLabel consignorStreet2Label;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox cS_OH_ConsignorGuidFindBox;
		Enterprise.ZArchitecture.ZLabel cS_OH_ConsignorLabel;
		Enterprise.ZArchitecture.ZTextBox consignorStateTextBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox consignorCountryCodeFindBox;
		Enterprise.ZArchitecture.ZTextBox consignorPostCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox consignorVendorIDTextBox;
		Enterprise.ZArchitecture.ZTextBox consignorCityTextBox;
		Enterprise.ZArchitecture.ZTextBox consignorStreetTextBox;
		Enterprise.ZArchitecture.ZTextBox consignorNameTextBox;
		Enterprise.ZArchitecture.ZLabel consignorPostCodeLabel;
		Enterprise.ZArchitecture.ZLabel consignorVendorIDLabel;
		Enterprise.ZArchitecture.ZLabel consignorStreetLabel;
		Enterprise.ZArchitecture.ZLabel consignorCityLabel;
		Enterprise.ZArchitecture.ZLabel consignorNameLabel;
		Enterprise.ZArchitecture.ZLabel consignorStateLabel;
		Enterprise.ZArchitecture.ZLabel consignorCountryCodeLabel;
		Enterprise.ZArchitecture.ZTextBox cS_ConsignorIdentifierTextBox;
		Enterprise.ZArchitecture.ZLabel cS_ConsignorIdentifierLabel;
		internal ZArchitecture.GUI.ZAddressControl ConsigneezAddressControl;
		internal ZArchitecture.GUI.ZAddressControl ConsignorzAddressControl;
		Enterprise.ZArchitecture.ZTextBox consignorPhoneTextBox;
		Enterprise.ZArchitecture.ZLabel consignorPhoneLabel;
	}
}
