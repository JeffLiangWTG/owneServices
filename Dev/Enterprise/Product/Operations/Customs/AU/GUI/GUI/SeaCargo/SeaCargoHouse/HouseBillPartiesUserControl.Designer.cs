namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	partial class HouseBillPartiesUserControl
	{
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ConsigneeTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ConsigneeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsigneezAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CA_ConsigneeIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsigneeBusinessNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsigneeIdentifierLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsigneeBusinessNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel21 = new Enterprise.ZArchitecture.ZLabel();
			this.ConsigneeCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_OH_ConsigneeBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel36 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel13 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsigneePhoneBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel14 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsigneePostcodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel15 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsigneeAddress3BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel16 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsigneeAddress2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel19 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsigneeAddress1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel20 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsigneeNameBoundTextBox19 = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsignorTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CA_ConsignorPhoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsignorPhoneBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsignorzAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CA_ConsignorIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ConsignorIdentifierLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CA_VendorIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_VendorIdentifierLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel34 = new Enterprise.ZArchitecture.ZLabel();
			this.ConsignorCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_OH_ConsignorBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel40 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel26 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsignorPostcodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel27 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsignorAddress3BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel28 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsignorAddress2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel29 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsignorAddress1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel30 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ConsignorNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLabel35 = new Enterprise.ZArchitecture.ZLabel();
			this.NotifyCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_OH_NotifyBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel33 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel17 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_NotifyPhoneBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_NotifyPostcodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_NotifyAddress3BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_NotifyAddress2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_NotifyAddress1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_NotifyNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsigneeTabControl.SuspendLayout();
			this.ConsigneeTabPage.SuspendLayout();
			this.ConsigneezAddressControl.SuspendLayout();
			this.ConsigneeCountryCodeFindBox.SuspendLayout();
			this.CA_OH_ConsigneeBoundGuidFindBox.SuspendLayout();
			this.ConsignorTabPage.SuspendLayout();
			this.ConsignorzAddressControl.SuspendLayout();
			this.ConsignorCountryCodeFindBox.SuspendLayout();
			this.CA_OH_ConsignorBoundGuidFindBox.SuspendLayout();
			this.NotifyPartyTabPage.SuspendLayout();
			this.NotifyCountryCodeFindBox.SuspendLayout();
			this.CA_OH_NotifyBoundGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusSCAHouse);
			// 
			// ConsigneeTabControl
			// 
			this.ConsigneeTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ConsigneeTabControl.Controls.Add(this.ConsigneeTabPage);
			this.ConsigneeTabControl.Controls.Add(this.ConsignorTabPage);
			this.ConsigneeTabControl.Controls.Add(this.NotifyPartyTabPage);
			this.ConsigneeTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsigneeTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsigneeTabControl.Multiline = true;
			this.ConsigneeTabControl.Name = "ConsigneeTabControl";
			this.ConsigneeTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 303, true);
			this.ConsigneeTabControl.TabIndex = 0;
			// 
			// ConsigneeTabPage
			// 
			this.ConsigneeTabPage.Controls.Add(this.ConsigneezAddressControl);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeIdentifierTextBox);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeBusinessNumberTextBox);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeIdentifierLabel);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeBusinessNumberLabel);
			this.ConsigneeTabPage.Controls.Add(this.zLabel21);
			this.ConsigneeTabPage.Controls.Add(this.ConsigneeCountryCodeFindBox);
			this.ConsigneeTabPage.Controls.Add(this.CA_OH_ConsigneeBoundGuidFindBox);
			this.ConsigneeTabPage.Controls.Add(this.zLabel36);
			this.ConsigneeTabPage.Controls.Add(this.zLabel13);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneePhoneBoundTextBox);
			this.ConsigneeTabPage.Controls.Add(this.zLabel14);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneePostcodeBoundTextBox);
			this.ConsigneeTabPage.Controls.Add(this.zLabel15);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeAddress3BoundTextBox);
			this.ConsigneeTabPage.Controls.Add(this.zLabel16);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeAddress2BoundTextBox);
			this.ConsigneeTabPage.Controls.Add(this.zLabel19);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeAddress1BoundTextBox);
			this.ConsigneeTabPage.Controls.Add(this.zLabel20);
			this.ConsigneeTabPage.Controls.Add(this.CA_ConsigneeNameBoundTextBox19);
			this.ConsigneeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsigneeTabPage.Name = "ConsigneeTabPage";
			this.ConsigneeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 276, true);
			this.ConsigneeTabPage.TabIndex = 0;
			this.ConsigneeTabPage.Text = "Consignee";
			// 
			// ConsigneezAddressControl
			// 
			this.ConsigneezAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneezAddressControl, "CA_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OA_ConsigneeAddress)));
			this.ConsigneezAddressControl.BindToOrgList = "Lookups+Consignee_List";
			this.ConsigneezAddressControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("64E28D85-F9F3-4CA5-A628-7FBF5D3D7338", "Org. Code", "Consignee Address", "Consignee Organization Address");
			this.ConsigneezAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 13, true);
			this.ConsigneezAddressControl.Name = "ConsigneezAddressControl";
			this.ConsigneezAddressControl.PopupCaption = "";
			this.ConsigneezAddressControl.ShowAddress = false;
			this.ConsigneezAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ConsigneezAddressControl.TabIndex = 1;
			// 
			// CA_ConsigneeIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsigneeIdentifierTextBox, "CA_ConsigneeIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsigneeIdentifier)));
			this.CA_ConsigneeIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 210, true);
			this.CA_ConsigneeIdentifierTextBox.Name = "CA_ConsigneeIdentifierTextBox";
			this.CA_ConsigneeIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsigneeIdentifierTextBox.TabIndex = 19;
			// 
			// CA_ConsigneeBusinessNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsigneeBusinessNumberTextBox, "CA_ConsigneeBusinessNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsigneeBusinessNumber)));
			this.CA_ConsigneeBusinessNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 184, true);
			this.CA_ConsigneeBusinessNumberTextBox.Name = "CA_ConsigneeBusinessNumberTextBox";
			this.CA_ConsigneeBusinessNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsigneeBusinessNumberTextBox.TabIndex = 17;
			// 
			// CA_ConsigneeIdentifierLabel
			// 
			this.CA_ConsigneeIdentifierLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CA_ConsigneeIdentifierLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 207, true);
			this.CA_ConsigneeIdentifierLabel.Name = "CA_ConsigneeIdentifierLabel";
			this.CA_ConsigneeIdentifierLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.CA_ConsigneeIdentifierLabel.TabIndex = 18;
			this.CA_ConsigneeIdentifierLabel.Text = "Identifier";
			this.CA_ConsigneeIdentifierLabel.UseMnemonic = false;
			// 
			// CA_ConsigneeBusinessNumberLabel
			// 
			this.CA_ConsigneeBusinessNumberLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CA_ConsigneeBusinessNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 181, true);
			this.CA_ConsigneeBusinessNumberLabel.Name = "CA_ConsigneeBusinessNumberLabel";
			this.CA_ConsigneeBusinessNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.CA_ConsigneeBusinessNumberLabel.TabIndex = 16;
			this.CA_ConsigneeBusinessNumberLabel.Text = "ABN";
			this.CA_ConsigneeBusinessNumberLabel.UseMnemonic = false;
			// 
			// zLabel21
			// 
			this.zLabel21.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 139, true);
			this.zLabel21.Name = "zLabel21";
			this.zLabel21.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 19, true);
			this.zLabel21.TabIndex = 12;
			this.zLabel21.Text = "Ctry/Rgn.:";
			this.zLabel21.UseMnemonic = false;
			// 
			// ConsigneeCountryCodeFindBox
			// 
			this.ConsigneeCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeCountryCodeFindBox, "CA_RN_NKConsigneeCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_RN_NKConsigneeCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CountryOfOriginList)));
			this.ConsigneeCountryCodeFindBox.BindToList = "CountryOfOriginList";
			this.ConsigneeCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 136, true);
			this.ConsigneeCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.ConsigneeCountryCodeFindBox.Name = "ConsigneeCountryCodeFindBox";
			this.ConsigneeCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ConsigneeCountryCodeFindBox.ParentType = null;
			this.ConsigneeCountryCodeFindBox.PreBoundMaxLength = 2;
			this.ConsigneeCountryCodeFindBox.ShowDescriptionBox = false;
			this.ConsigneeCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.ConsigneeCountryCodeFindBox.TabIndex = 13;
			// 
			// CA_OH_ConsigneeBoundGuidFindBox
			// 
			this.CA_OH_ConsigneeBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_OH_ConsigneeBoundGuidFindBox, "CA_OH_Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OH_Consignee)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OH_Consignee_List)));
			this.CA_OH_ConsigneeBoundGuidFindBox.BindToList = "CA_OH_Consignee_List";
			this.CA_OH_ConsigneeBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 13, true);
			this.CA_OH_ConsigneeBoundGuidFindBox.Name = "CA_OH_ConsigneeBoundGuidFindBox";
			this.CA_OH_ConsigneeBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CA_OH_ConsigneeBoundGuidFindBox.ParentType = null;
			this.CA_OH_ConsigneeBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_OH_ConsigneeBoundGuidFindBox.TabIndex = 1;
			// 
			// zLabel36
			// 
			this.zLabel36.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel36.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 10, true);
			this.zLabel36.Name = "zLabel36";
			this.zLabel36.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel36.TabIndex = 0;
			this.zLabel36.Text = "Org.";
			this.zLabel36.UseMnemonic = false;
			// 
			// zLabel13
			// 
			this.zLabel13.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 157, true);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel13.TabIndex = 14;
			this.zLabel13.Text = "Telephone";
			this.zLabel13.UseMnemonic = false;
			// 
			// CA_ConsigneePhoneBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsigneePhoneBoundTextBox, "CA_ConsigneePhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsigneePhone)));
			this.CA_ConsigneePhoneBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 160, true);
			this.CA_ConsigneePhoneBoundTextBox.Name = "CA_ConsigneePhoneBoundTextBox";
			this.CA_ConsigneePhoneBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsigneePhoneBoundTextBox.TabIndex = 15;
			// 
			// zLabel14
			// 
			this.zLabel14.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 133, true);
			this.zLabel14.Name = "zLabel14";
			this.zLabel14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel14.TabIndex = 10;
			this.zLabel14.Text = "Postal Code";
			this.zLabel14.UseMnemonic = false;
			// 
			// CA_ConsigneePostcodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsigneePostcodeBoundTextBox, "CA_ConsigneePostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsigneePostcode)));
			this.CA_ConsigneePostcodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 136, true);
			this.CA_ConsigneePostcodeBoundTextBox.Name = "CA_ConsigneePostcodeBoundTextBox";
			this.CA_ConsigneePostcodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CA_ConsigneePostcodeBoundTextBox.TabIndex = 11;
			// 
			// zLabel15
			// 
			this.zLabel15.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 109, true);
			this.zLabel15.Name = "zLabel15";
			this.zLabel15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel15.TabIndex = 8;
			this.zLabel15.Text = "Suburb";
			this.zLabel15.UseMnemonic = false;
			// 
			// CA_ConsigneeAddress3BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsigneeAddress3BoundTextBox, "CA_ConsigneeSuburb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsigneeSuburb)));
			this.CA_ConsigneeAddress3BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 112, true);
			this.CA_ConsigneeAddress3BoundTextBox.Name = "CA_ConsigneeAddress3BoundTextBox";
			this.CA_ConsigneeAddress3BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsigneeAddress3BoundTextBox.TabIndex = 9;
			// 
			// zLabel16
			// 
			this.zLabel16.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 85, true);
			this.zLabel16.Name = "zLabel16";
			this.zLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel16.TabIndex = 6;
			this.zLabel16.Text = "Address 2";
			this.zLabel16.UseMnemonic = false;
			// 
			// CA_ConsigneeAddress2BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsigneeAddress2BoundTextBox, "CA_ConsigneeAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsigneeAddress2)));
			this.CA_ConsigneeAddress2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 88, true);
			this.CA_ConsigneeAddress2BoundTextBox.Name = "CA_ConsigneeAddress2BoundTextBox";
			this.CA_ConsigneeAddress2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsigneeAddress2BoundTextBox.TabIndex = 7;
			// 
			// zLabel19
			// 
			this.zLabel19.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 61, true);
			this.zLabel19.Name = "zLabel19";
			this.zLabel19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel19.TabIndex = 4;
			this.zLabel19.Text = "Address 1";
			this.zLabel19.UseMnemonic = false;
			// 
			// CA_ConsigneeAddress1BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsigneeAddress1BoundTextBox, "CA_ConsigneeAddress1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsigneeAddress1)));
			this.CA_ConsigneeAddress1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 64, true);
			this.CA_ConsigneeAddress1BoundTextBox.Name = "CA_ConsigneeAddress1BoundTextBox";
			this.CA_ConsigneeAddress1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsigneeAddress1BoundTextBox.TabIndex = 5;
			// 
			// zLabel20
			// 
			this.zLabel20.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel20.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.zLabel20.Name = "zLabel20";
			this.zLabel20.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 23, true);
			this.zLabel20.TabIndex = 2;
			this.zLabel20.Text = "Name";
			this.zLabel20.UseMnemonic = false;
			// 
			// CA_ConsigneeNameBoundTextBox19
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsigneeNameBoundTextBox19, "CA_ConsigneeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsigneeName)));
			this.CA_ConsigneeNameBoundTextBox19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 40, true);
			this.CA_ConsigneeNameBoundTextBox19.Name = "CA_ConsigneeNameBoundTextBox19";
			this.CA_ConsigneeNameBoundTextBox19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsigneeNameBoundTextBox19.TabIndex = 3;
			// 
			// ConsignorTabPage
			// 
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorPhoneLabel);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorPhoneBoundTextBox);
			this.ConsignorTabPage.Controls.Add(this.ConsignorzAddressControl);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorIdentifierTextBox);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorIdentifierLabel);
			this.ConsignorTabPage.Controls.Add(this.CA_VendorIdentifierTextBox);
			this.ConsignorTabPage.Controls.Add(this.CA_VendorIdentifierLabel);
			this.ConsignorTabPage.Controls.Add(this.zLabel34);
			this.ConsignorTabPage.Controls.Add(this.ConsignorCountryCodeFindBox);
			this.ConsignorTabPage.Controls.Add(this.CA_OH_ConsignorBoundGuidFindBox);
			this.ConsignorTabPage.Controls.Add(this.zLabel40);
			this.ConsignorTabPage.Controls.Add(this.zLabel26);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorPostcodeBoundTextBox);
			this.ConsignorTabPage.Controls.Add(this.zLabel27);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorAddress3BoundTextBox);
			this.ConsignorTabPage.Controls.Add(this.zLabel28);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorAddress2BoundTextBox);
			this.ConsignorTabPage.Controls.Add(this.zLabel29);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorAddress1BoundTextBox);
			this.ConsignorTabPage.Controls.Add(this.zLabel30);
			this.ConsignorTabPage.Controls.Add(this.CA_ConsignorNameBoundTextBox);
			this.ConsignorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsignorTabPage.Name = "ConsignorTabPage";
			this.ConsignorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 276, true);
			this.ConsignorTabPage.TabIndex = 2;
			this.ConsignorTabPage.Text = "Consignor";
			// 
			// CA_ConsignorPhoneLabel
			// 
			this.CA_ConsignorPhoneLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CA_ConsignorPhoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 157, true);
			this.CA_ConsignorPhoneLabel.Name = "CA_ConsignorPhoneLabel";
			this.CA_ConsignorPhoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.CA_ConsignorPhoneLabel.TabIndex = 14;
			this.CA_ConsignorPhoneLabel.Text = "Telephone";
			this.CA_ConsignorPhoneLabel.UseMnemonic = false;
			// 
			// CA_ConsignorPhoneBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsignorPhoneBoundTextBox, "CA_ConsignorPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsignorPhone)));
			this.CA_ConsignorPhoneBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 160, true);
			this.CA_ConsignorPhoneBoundTextBox.Name = "CA_ConsignorPhoneBoundTextBox";
			this.CA_ConsignorPhoneBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsignorPhoneBoundTextBox.TabIndex = 15;
			// 
			// ConsignorzAddressControl
			// 
			this.ConsignorzAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorzAddressControl, "CA_OA_ConsignorAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OA_ConsignorAddress)));
			this.ConsignorzAddressControl.BindToOrgList = "Lookups+Consignor_List";
			this.ConsignorzAddressControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("1F3FEEDC-6FD5-4F99-850C-989642964840", "Org. Code", "Consignor Address", "Consignor Organization Address");
			this.ConsignorzAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 13, true);
			this.ConsignorzAddressControl.Name = "ConsignorzAddressControl";
			this.ConsignorzAddressControl.PopupCaption = "";
			this.ConsignorzAddressControl.ShowAddress = false;
			this.ConsignorzAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ConsignorzAddressControl.TabIndex = 1;
			// 
			// CA_ConsignorIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsignorIdentifierTextBox, "CA_ConsignorIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsignorIdentifier)));
			this.CA_ConsignorIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 208, true);
			this.CA_ConsignorIdentifierTextBox.Name = "CA_ConsignorIdentifierTextBox";
			this.CA_ConsignorIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsignorIdentifierTextBox.TabIndex = 19;
			// 
			// CA_ConsignorIdentifierLabel
			// 
			this.CA_ConsignorIdentifierLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CA_ConsignorIdentifierLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 205, true);
			this.CA_ConsignorIdentifierLabel.Name = "CA_ConsignorIdentifierLabel";
			this.CA_ConsignorIdentifierLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.CA_ConsignorIdentifierLabel.TabIndex = 18;
			this.CA_ConsignorIdentifierLabel.Text = "Supplier ID";
			this.CA_ConsignorIdentifierLabel.UseMnemonic = false;
			// 
			// CA_VendorIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_VendorIdentifierTextBox, "CA_VendorIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_VendorIdentifier)));
			this.CA_VendorIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 184, true);
			this.CA_VendorIdentifierTextBox.Name = "CA_VendorIdentifierTextBox";
			this.CA_VendorIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_VendorIdentifierTextBox.TabIndex = 17;
			// 
			// CA_VendorIdentifierLabel
			// 
			this.CA_VendorIdentifierLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CA_VendorIdentifierLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 181, true);
			this.CA_VendorIdentifierLabel.Name = "CA_VendorIdentifierLabel";
			this.CA_VendorIdentifierLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.CA_VendorIdentifierLabel.TabIndex = 16;
			this.CA_VendorIdentifierLabel.Text = "Vendor";
			this.CA_VendorIdentifierLabel.UseMnemonic = false;
			// 
			// zLabel34
			// 
			this.zLabel34.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel34.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 139, true);
			this.zLabel34.Name = "zLabel34";
			this.zLabel34.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 17, true);
			this.zLabel34.TabIndex = 12;
			this.zLabel34.Text = "Ctry/Rgn.:";
			this.zLabel34.UseMnemonic = false;
			// 
			// ConsignorCountryCodeFindBox
			// 
			this.ConsignorCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorCountryCodeFindBox, "CA_RN_NKConsignorCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_RN_NKConsignorCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CountryOfOriginList)));
			this.ConsignorCountryCodeFindBox.BindToList = "CountryOfOriginList";
			this.ConsignorCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 136, true);
			this.ConsignorCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.ConsignorCountryCodeFindBox.Name = "ConsignorCountryCodeFindBox";
			this.ConsignorCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ConsignorCountryCodeFindBox.ParentType = null;
			this.ConsignorCountryCodeFindBox.PreBoundMaxLength = 2;
			this.ConsignorCountryCodeFindBox.ShowDescriptionBox = false;
			this.ConsignorCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.ConsignorCountryCodeFindBox.TabIndex = 13;
			// 
			// CA_OH_ConsignorBoundGuidFindBox
			// 
			this.CA_OH_ConsignorBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_OH_ConsignorBoundGuidFindBox, "CA_OH_Consignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OH_Consignor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OH_Consignor_List)));
			this.CA_OH_ConsignorBoundGuidFindBox.BindToList = "CA_OH_Consignor_List";
			this.CA_OH_ConsignorBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 13, true);
			this.CA_OH_ConsignorBoundGuidFindBox.Name = "CA_OH_ConsignorBoundGuidFindBox";
			this.CA_OH_ConsignorBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CA_OH_ConsignorBoundGuidFindBox.ParentType = null;
			this.CA_OH_ConsignorBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_OH_ConsignorBoundGuidFindBox.TabIndex = 1;
			// 
			// zLabel40
			// 
			this.zLabel40.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel40.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 10, true);
			this.zLabel40.Name = "zLabel40";
			this.zLabel40.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel40.TabIndex = 0;
			this.zLabel40.Text = "Org.";
			this.zLabel40.UseMnemonic = false;
			// 
			// zLabel26
			// 
			this.zLabel26.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel26.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 133, true);
			this.zLabel26.Name = "zLabel26";
			this.zLabel26.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel26.TabIndex = 10;
			this.zLabel26.Text = "Postal Code";
			this.zLabel26.UseMnemonic = false;
			// 
			// CA_ConsignorPostcodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsignorPostcodeBoundTextBox, "CA_ConsignorPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsignorPostcode)));
			this.CA_ConsignorPostcodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 136, true);
			this.CA_ConsignorPostcodeBoundTextBox.Name = "CA_ConsignorPostcodeBoundTextBox";
			this.CA_ConsignorPostcodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CA_ConsignorPostcodeBoundTextBox.TabIndex = 11;
			// 
			// zLabel27
			// 
			this.zLabel27.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel27.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 109, true);
			this.zLabel27.Name = "zLabel27";
			this.zLabel27.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel27.TabIndex = 8;
			this.zLabel27.Text = "Suburb";
			this.zLabel27.UseMnemonic = false;
			// 
			// CA_ConsignorAddress3BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsignorAddress3BoundTextBox, "CA_ConsignorSuburb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsignorSuburb)));
			this.CA_ConsignorAddress3BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 112, true);
			this.CA_ConsignorAddress3BoundTextBox.Name = "CA_ConsignorAddress3BoundTextBox";
			this.CA_ConsignorAddress3BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsignorAddress3BoundTextBox.TabIndex = 9;
			// 
			// zLabel28
			// 
			this.zLabel28.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel28.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 85, true);
			this.zLabel28.Name = "zLabel28";
			this.zLabel28.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel28.TabIndex = 6;
			this.zLabel28.Text = "Address 2";
			this.zLabel28.UseMnemonic = false;
			// 
			// CA_ConsignorAddress2BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsignorAddress2BoundTextBox, "CA_ConsignorAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsignorAddress2)));
			this.CA_ConsignorAddress2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 88, true);
			this.CA_ConsignorAddress2BoundTextBox.Name = "CA_ConsignorAddress2BoundTextBox";
			this.CA_ConsignorAddress2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsignorAddress2BoundTextBox.TabIndex = 7;
			// 
			// zLabel29
			// 
			this.zLabel29.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel29.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 61, true);
			this.zLabel29.Name = "zLabel29";
			this.zLabel29.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel29.TabIndex = 4;
			this.zLabel29.Text = "Address 1";
			this.zLabel29.UseMnemonic = false;
			// 
			// CA_ConsignorAddress1BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsignorAddress1BoundTextBox, "CA_ConsignorAddress1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsignorAddress1)));
			this.CA_ConsignorAddress1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 64, true);
			this.CA_ConsignorAddress1BoundTextBox.Name = "CA_ConsignorAddress1BoundTextBox";
			this.CA_ConsignorAddress1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsignorAddress1BoundTextBox.TabIndex = 5;
			// 
			// zLabel30
			// 
			this.zLabel30.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.zLabel30.Name = "zLabel30";
			this.zLabel30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel30.TabIndex = 2;
			this.zLabel30.Text = "Name";
			this.zLabel30.UseMnemonic = false;
			// 
			// CA_ConsignorNameBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_ConsignorNameBoundTextBox, "CA_ConsignorName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ConsignorName)));
			this.CA_ConsignorNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 40, true);
			this.CA_ConsignorNameBoundTextBox.Name = "CA_ConsignorNameBoundTextBox";
			this.CA_ConsignorNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_ConsignorNameBoundTextBox.TabIndex = 3;
			// 
			// NotifyPartyTabPage
			// 
			this.NotifyPartyTabPage.Controls.Add(this.zLabel35);
			this.NotifyPartyTabPage.Controls.Add(this.NotifyCountryCodeFindBox);
			this.NotifyPartyTabPage.Controls.Add(this.CA_OH_NotifyBoundGuidFindBox);
			this.NotifyPartyTabPage.Controls.Add(this.zLabel33);
			this.NotifyPartyTabPage.Controls.Add(this.zLabel17);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyPhoneBoundTextBox);
			this.NotifyPartyTabPage.Controls.Add(this.zLabel7);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyPostcodeBoundTextBox);
			this.NotifyPartyTabPage.Controls.Add(this.zLabel8);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyAddress3BoundTextBox);
			this.NotifyPartyTabPage.Controls.Add(this.zLabel9);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyAddress2BoundTextBox);
			this.NotifyPartyTabPage.Controls.Add(this.zLabel10);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyAddress1BoundTextBox);
			this.NotifyPartyTabPage.Controls.Add(this.zLabel11);
			this.NotifyPartyTabPage.Controls.Add(this.CA_NotifyNameBoundTextBox);
			this.NotifyPartyTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NotifyPartyTabPage.Name = "NotifyPartyTabPage";
			this.NotifyPartyTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 276, true);
			this.NotifyPartyTabPage.TabIndex = 1;
			this.NotifyPartyTabPage.Text = "Notify Party";
			// 
			// zLabel35
			// 
			this.zLabel35.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel35.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 139, true);
			this.zLabel35.Name = "zLabel35";
			this.zLabel35.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
			this.zLabel35.TabIndex = 12;
			this.zLabel35.Text = "Ctry/Rgn.:";
			this.zLabel35.UseMnemonic = false;
			// 
			// NotifyCountryCodeFindBox
			// 
			this.NotifyCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyCountryCodeFindBox, "CA_RN_NKNotifyCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_RN_NKNotifyCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CountryOfOriginList)));
			this.NotifyCountryCodeFindBox.BindToList = "CountryOfOriginList";
			this.NotifyCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 136, true);
			this.NotifyCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.NotifyCountryCodeFindBox.Name = "NotifyCountryCodeFindBox";
			this.NotifyCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.NotifyCountryCodeFindBox.ParentType = null;
			this.NotifyCountryCodeFindBox.PreBoundMaxLength = 2;
			this.NotifyCountryCodeFindBox.ShowDescriptionBox = false;
			this.NotifyCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.NotifyCountryCodeFindBox.TabIndex = 13;
			// 
			// CA_OH_NotifyBoundGuidFindBox
			// 
			this.CA_OH_NotifyBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_OH_NotifyBoundGuidFindBox, "CA_OH_Notify");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OH_Notify)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_OH_Notify_List)));
			this.CA_OH_NotifyBoundGuidFindBox.BindToList = "CA_OH_Notify_List";
			this.CA_OH_NotifyBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 13, true);
			this.CA_OH_NotifyBoundGuidFindBox.Name = "CA_OH_NotifyBoundGuidFindBox";
			this.CA_OH_NotifyBoundGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CA_OH_NotifyBoundGuidFindBox.ParentType = null;
			this.CA_OH_NotifyBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_OH_NotifyBoundGuidFindBox.TabIndex = 1;
			// 
			// zLabel33
			// 
			this.zLabel33.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel33.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 10, true);
			this.zLabel33.Name = "zLabel33";
			this.zLabel33.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel33.TabIndex = 0;
			this.zLabel33.Text = "Org.";
			this.zLabel33.UseMnemonic = false;
			// 
			// zLabel17
			// 
			this.zLabel17.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 157, true);
			this.zLabel17.Name = "zLabel17";
			this.zLabel17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel17.TabIndex = 14;
			this.zLabel17.Text = "Telephone";
			this.zLabel17.UseMnemonic = false;
			// 
			// CA_NotifyPhoneBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_NotifyPhoneBoundTextBox, "CA_NotifyPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_NotifyPhone)));
			this.CA_NotifyPhoneBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 160, true);
			this.CA_NotifyPhoneBoundTextBox.Name = "CA_NotifyPhoneBoundTextBox";
			this.CA_NotifyPhoneBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_NotifyPhoneBoundTextBox.TabIndex = 15;
			// 
			// zLabel7
			// 
			this.zLabel7.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 133, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel7.TabIndex = 10;
			this.zLabel7.Text = "Postal Code";
			this.zLabel7.UseMnemonic = false;
			// 
			// CA_NotifyPostcodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_NotifyPostcodeBoundTextBox, "CA_NotifyPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_NotifyPostcode)));
			this.CA_NotifyPostcodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 136, true);
			this.CA_NotifyPostcodeBoundTextBox.Name = "CA_NotifyPostcodeBoundTextBox";
			this.CA_NotifyPostcodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CA_NotifyPostcodeBoundTextBox.TabIndex = 11;
			// 
			// zLabel8
			// 
			this.zLabel8.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 109, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel8.TabIndex = 8;
			this.zLabel8.Text = "Suburb";
			this.zLabel8.UseMnemonic = false;
			// 
			// CA_NotifyAddress3BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_NotifyAddress3BoundTextBox, "CA_NotifySuburb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_NotifySuburb)));
			this.CA_NotifyAddress3BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 112, true);
			this.CA_NotifyAddress3BoundTextBox.Name = "CA_NotifyAddress3BoundTextBox";
			this.CA_NotifyAddress3BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_NotifyAddress3BoundTextBox.TabIndex = 9;
			// 
			// zLabel9
			// 
			this.zLabel9.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 85, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel9.TabIndex = 6;
			this.zLabel9.Text = "Address 2";
			this.zLabel9.UseMnemonic = false;
			// 
			// CA_NotifyAddress2BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_NotifyAddress2BoundTextBox, "CA_NotifyAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_NotifyAddress2)));
			this.CA_NotifyAddress2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 88, true);
			this.CA_NotifyAddress2BoundTextBox.Name = "CA_NotifyAddress2BoundTextBox";
			this.CA_NotifyAddress2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_NotifyAddress2BoundTextBox.TabIndex = 7;
			// 
			// zLabel10
			// 
			this.zLabel10.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 61, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel10.TabIndex = 4;
			this.zLabel10.Text = "Address 1";
			this.zLabel10.UseMnemonic = false;
			// 
			// CA_NotifyAddress1BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_NotifyAddress1BoundTextBox, "CA_NotifyAddress1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_NotifyAddress1)));
			this.CA_NotifyAddress1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 64, true);
			this.CA_NotifyAddress1BoundTextBox.Name = "CA_NotifyAddress1BoundTextBox";
			this.CA_NotifyAddress1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_NotifyAddress1BoundTextBox.TabIndex = 5;
			// 
			// zLabel11
			// 
			this.zLabel11.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.zLabel11.TabIndex = 2;
			this.zLabel11.Text = "Name";
			this.zLabel11.UseMnemonic = false;
			// 
			// CA_NotifyNameBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_NotifyNameBoundTextBox, "CA_NotifyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_NotifyName)));
			this.CA_NotifyNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 40, true);
			this.CA_NotifyNameBoundTextBox.Name = "CA_NotifyNameBoundTextBox";
			this.CA_NotifyNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.CA_NotifyNameBoundTextBox.TabIndex = 3;
			// 
			// HouseBillPartiesUserControl
			// 
			this.Controls.Add(this.ConsigneeTabControl);
			this.Name = "HouseBillPartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 303, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsigneeTabControl.ResumeLayout(false);
			this.ConsigneeTabControl.PerformLayout();
			this.ConsigneeTabPage.ResumeLayout(false);
			this.ConsigneeTabPage.PerformLayout();
			this.ConsigneezAddressControl.ResumeLayout(true);
			this.ConsigneezAddressControl.PerformLayout();
			this.ConsigneeCountryCodeFindBox.ResumeLayout(true);
			this.ConsigneeCountryCodeFindBox.PerformLayout();
			this.CA_OH_ConsigneeBoundGuidFindBox.ResumeLayout(true);
			this.CA_OH_ConsigneeBoundGuidFindBox.PerformLayout();
			this.ConsignorTabPage.ResumeLayout(false);
			this.ConsignorTabPage.PerformLayout();
			this.ConsignorzAddressControl.ResumeLayout(true);
			this.ConsignorzAddressControl.PerformLayout();
			this.ConsignorCountryCodeFindBox.ResumeLayout(true);
			this.ConsignorCountryCodeFindBox.PerformLayout();
			this.CA_OH_ConsignorBoundGuidFindBox.ResumeLayout(true);
			this.CA_OH_ConsignorBoundGuidFindBox.PerformLayout();
			this.NotifyPartyTabPage.ResumeLayout(false);
			this.NotifyPartyTabPage.PerformLayout();
			this.NotifyCountryCodeFindBox.ResumeLayout(true);
			this.NotifyCountryCodeFindBox.PerformLayout();
			this.CA_OH_NotifyBoundGuidFindBox.ResumeLayout(true);
			this.CA_OH_NotifyBoundGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.GUI.ZTemplateTabControl ConsigneeTabControl;
		#region ConsigneeTabPage
		Enterprise.ZArchitecture.GUI.ZTabPage ConsigneeTabPage;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl ConsigneezAddressControl;
		Enterprise.ZArchitecture.ZTextBox CA_ConsigneeIdentifierTextBox;
		Enterprise.ZArchitecture.ZTextBox CA_ConsigneeBusinessNumberTextBox;
		Enterprise.ZArchitecture.ZLabel CA_ConsigneeIdentifierLabel;
		Enterprise.ZArchitecture.ZLabel CA_ConsigneeBusinessNumberLabel;
		Enterprise.ZArchitecture.ZLabel zLabel21;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox ConsigneeCountryCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox CA_OH_ConsigneeBoundGuidFindBox;
		Enterprise.ZArchitecture.ZLabel zLabel36;
		Enterprise.ZArchitecture.ZLabel zLabel13;
		Enterprise.ZArchitecture.ZTextBox CA_ConsigneePhoneBoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel14;
		Enterprise.ZArchitecture.ZTextBox CA_ConsigneePostcodeBoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel15;
		Enterprise.ZArchitecture.ZTextBox CA_ConsigneeAddress3BoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel16;
		Enterprise.ZArchitecture.ZTextBox CA_ConsigneeAddress2BoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel19;
		Enterprise.ZArchitecture.ZTextBox CA_ConsigneeAddress1BoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel20;
		Enterprise.ZArchitecture.ZTextBox CA_ConsigneeNameBoundTextBox19;
		#endregion
		#region ConsignorTabPage
		Enterprise.ZArchitecture.GUI.ZTabPage ConsignorTabPage;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl ConsignorzAddressControl;
		Enterprise.ZArchitecture.ZTextBox CA_ConsignorIdentifierTextBox;
		Enterprise.ZArchitecture.ZLabel CA_ConsignorIdentifierLabel;
		Enterprise.ZArchitecture.ZTextBox CA_VendorIdentifierTextBox;
		Enterprise.ZArchitecture.ZLabel CA_VendorIdentifierLabel;
		Enterprise.ZArchitecture.ZLabel zLabel34;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox ConsignorCountryCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox CA_OH_ConsignorBoundGuidFindBox;
		Enterprise.ZArchitecture.ZLabel zLabel40;
		Enterprise.ZArchitecture.ZLabel zLabel26;
		Enterprise.ZArchitecture.ZTextBox CA_ConsignorPostcodeBoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel27;
		Enterprise.ZArchitecture.ZTextBox CA_ConsignorAddress3BoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel28;
		Enterprise.ZArchitecture.ZTextBox CA_ConsignorAddress2BoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel29;
		Enterprise.ZArchitecture.ZTextBox CA_ConsignorAddress1BoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel30;
		Enterprise.ZArchitecture.ZTextBox CA_ConsignorNameBoundTextBox;
		Enterprise.ZArchitecture.ZLabel CA_ConsignorPhoneLabel;
		Enterprise.ZArchitecture.ZTextBox CA_ConsignorPhoneBoundTextBox;
		#endregion
		#region NotifyPartyTabPage
		Enterprise.ZArchitecture.GUI.ZTabPage NotifyPartyTabPage;
		Enterprise.ZArchitecture.ZLabel zLabel35;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox NotifyCountryCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox CA_OH_NotifyBoundGuidFindBox;
		Enterprise.ZArchitecture.ZLabel zLabel33;
		Enterprise.ZArchitecture.ZLabel zLabel17;
		Enterprise.ZArchitecture.ZTextBox CA_NotifyPhoneBoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel7;
		Enterprise.ZArchitecture.ZTextBox CA_NotifyPostcodeBoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel8;
		Enterprise.ZArchitecture.ZTextBox CA_NotifyAddress3BoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel9;
		Enterprise.ZArchitecture.ZTextBox CA_NotifyAddress2BoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel10;
		Enterprise.ZArchitecture.ZTextBox CA_NotifyAddress1BoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel11;
		Enterprise.ZArchitecture.ZTextBox CA_NotifyNameBoundTextBox;
		#endregion

		private System.ComponentModel.IContainer components;
	}
}
