using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.CN.GUI
{
	partial class CNJobDocAddressControl
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
			this.components = new System.ComponentModel.Container();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OverseasPartyCodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OverseasPartyCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CIQCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SocialCreditCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompanyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AddressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AddressDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEdit.Bare();
			this.AddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OverrideAddressTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AddressLine1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AddressLine2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ContactTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContactDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.PhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.OrganisationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.OverrideAddressCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.OverseasPartyCodeTypeDropEdit.SuspendLayout();
			this.AddressTabPage.SuspendLayout();
			this.AddressDropEdit.SuspendLayout();
			this.OverrideAddressTabPage.SuspendLayout();
			this.StateDropEdit.SuspendLayout();
			this.CountryFindBox.SuspendLayout();
			this.ContactTabPage.SuspendLayout();
			this.ContactDropEdit.SuspendLayout();
			this.PhoneNumberControl.SuspendLayout();
			this.OrganisationPanel.SuspendLayout();
			this.OrganisationFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CNJobDocAddress);
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Controls.Add(this.DetailsTabControl);
			this.MainGroupBox.Controls.Add(this.OrganisationPanel);
			this.MainGroupBox.Controls.Add(this.OverrideAddressCheckbox);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.MainGroupBox.TabIndex = 1;
			this.MainGroupBox.TabStop = false;
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsTabControl.Controls.Add(this.DetailsTabPage);
			this.DetailsTabControl.Controls.Add(this.AddressTabPage);
			this.DetailsTabControl.Controls.Add(this.OverrideAddressTabPage);
			this.DetailsTabControl.Controls.Add(this.ContactTabPage);
			this.DetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 44, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.SelectedIndex = 0;
			this.DetailsTabControl.ShowToolTips = true;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 135, true);
			this.DetailsTabControl.TabIndex = 0;
			this.DetailsTabControl.SelectedIndexChanged += new System.EventHandler(this.UpdateControlsLayout);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("59cdb2f8-d47b-4f6c-9f9c-62672449ee0e", "Details");
			this.DetailsTabPage.Controls.Add(this.OverseasPartyCodeTypeDropEdit);
			this.DetailsTabPage.Controls.Add(this.OverseasPartyCodeTextBox);
			this.DetailsTabPage.Controls.Add(this.CIQCodeTextBox);
			this.DetailsTabPage.Controls.Add(this.CustomsCodeTextBox);
			this.DetailsTabPage.Controls.Add(this.SocialCreditCodeTextBox);
			this.DetailsTabPage.Controls.Add(this.CompanyTextBox);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 108, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// OverseasPartyCodeTypeDropEdit
			// 
			this.OverseasPartyCodeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverseasPartyCodeTypeDropEdit, "OverseasPartyCodeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).OverseasPartyCodeType)));
			this.OverseasPartyCodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 28, true);
			this.OverseasPartyCodeTypeDropEdit.Name = "OverseasPartyCodeTypeDropEdit";
			this.OverseasPartyCodeTypeDropEdit.PreBoundMaxLength = 3;
			this.OverseasPartyCodeTypeDropEdit.ShouldResizeByMaxLength = true;
			this.OverseasPartyCodeTypeDropEdit.ShowDescriptionBox = false;
			this.OverseasPartyCodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OverseasPartyCodeTypeDropEdit.TabIndex = 2;
			// 
			// OverseasPartyCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.OverseasPartyCodeTextBox, "OverseasPartyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).OverseasPartyCode)));
			this.OverseasPartyCodeTextBox.CaptionResourceString = null;
			this.OverseasPartyCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 28, true);
			this.OverseasPartyCodeTextBox.Name = "OverseasPartyCodeTextBox";
			this.OverseasPartyCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.OverseasPartyCodeTextBox.TabIndex = 3;
			// 
			// CIQCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CIQCodeTextBox, "CIQCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).CIQCode)));
			this.CIQCodeTextBox.CaptionResourceString = null;
			this.CIQCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 68, true);
			this.CIQCodeTextBox.Name = "CIQCodeTextBox";
			this.CIQCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.CIQCodeTextBox.TabIndex = 7;
			// 
			// CustomsCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsCodeTextBox, "CustomsCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).CustomsCode)));
			this.CustomsCodeTextBox.CaptionResourceString = null;
			this.CustomsCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 48, true);
			this.CustomsCodeTextBox.Name = "CustomsCodeTextBox";
			this.CustomsCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.CustomsCodeTextBox.TabIndex = 6;
			// 
			// SocialCreditCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.SocialCreditCodeTextBox, "SocialCreditCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).SocialCreditCode)));
			this.SocialCreditCodeTextBox.CaptionResourceString = null;
			this.SocialCreditCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 28, true);
			this.SocialCreditCodeTextBox.Name = "SocialCreditCodeTextBox";
			this.SocialCreditCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.SocialCreditCodeTextBox.TabIndex = 2;
			// 
			// CompanyTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyTextBox, "ChineseCompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).ChineseCompanyName)));
			this.CompanyTextBox.CaptionResourceString = null;
			this.CompanyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 8, true);
			this.CompanyTextBox.Name = "CompanyTextBox";
			this.CompanyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.CompanyTextBox.TabIndex = 1;
			// 
			// AddressTabPage
			// 
			this.AddressTabPage.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("6dc3bcbf-9c45-416f-abb3-446a6b23a120", "Address");
			this.AddressTabPage.Controls.Add(this.AddressDropEdit);
			this.AddressTabPage.Controls.Add(this.AddressLabel);
			this.AddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AddressTabPage.Name = "AddressTabPage";
			this.AddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 108, true);
			this.AddressTabPage.TabIndex = 2;
			// 
			// AddressDropEdit
			// 
			this.AddressDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressDropEdit, "E2_OA_Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).E2_OA_Address)));
			this.AddressDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.AddressDropEdit.FilterAddressedByDefaultType = true;
			this.AddressDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.AddressDropEdit.Name = "AddressDropEdit";
			this.AddressDropEdit.PreBoundMaxLength = 34;
			this.AddressDropEdit.ShouldResizeByMaxLength = true;
			this.AddressDropEdit.ShowDescriptionBox = false;
			this.AddressDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.AddressDropEdit.TabIndex = 1;
			// 
			// AddressLabel
			// 
			this.AddressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AddressLabel, "AddressSummaryWithCompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).AddressSummaryWithCompanyName)));
			this.AddressLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 33, true);
			this.AddressLabel.Name = "AddressLabel";
			this.AddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 75, true);
			this.AddressLabel.TabIndex = 1;
			this.AddressLabel.Text = "Address\r\nAddress\r\nAddress\r\nAddress";
			this.AddressLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.AddressLabel.UseCompatibleTextRendering = true;
			this.AddressLabel.UseMnemonic = false;
			// 
			// OverrideAddressTabPage
			// 
			this.OverrideAddressTabPage.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("6dc3bcbf-9c45-416f-abb3-446a6b23a120", "Address");
			this.OverrideAddressTabPage.Controls.Add(this.AddressLine1TextBox);
			this.OverrideAddressTabPage.Controls.Add(this.AddressLine2TextBox);
			this.OverrideAddressTabPage.Controls.Add(this.PostCodeTextBox);
			this.OverrideAddressTabPage.Controls.Add(this.StateDropEdit);
			this.OverrideAddressTabPage.Controls.Add(this.CityTextBox);
			this.OverrideAddressTabPage.Controls.Add(this.CountryFindBox);
			this.OverrideAddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OverrideAddressTabPage.Name = "OverrideAddressTabPage";
			this.OverrideAddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 108, true);
			this.OverrideAddressTabPage.TabIndex = 2;
			this.OverrideAddressTabPage.Text = "Address";
			// 
			// AddressLine1TextBox
			// 
			this.BindingSource.SetBindingMember(this.AddressLine1TextBox, "E2_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).E2_Address1)));
			this.AddressLine1TextBox.CaptionResourceString = null;
			this.AddressLine1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 8, true);
			this.AddressLine1TextBox.Name = "AddressLine1TextBox";
			this.AddressLine1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			this.AddressLine1TextBox.TabIndex = 0;
			// 
			// AddressLine2TextBox
			// 
			this.BindingSource.SetBindingMember(this.AddressLine2TextBox, "E2_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).E2_Address2)));
			this.AddressLine2TextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddressLine2TextBox, false);
			this.AddressLine2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 29, true);
			this.AddressLine2TextBox.Name = "AddressLine2TextBox";
			this.AddressLine2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			this.AddressLine2TextBox.TabIndex = 1;
			// 
			// PostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PostCodeTextBox, "E2_Postcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).E2_Postcode)));
			this.PostCodeTextBox.CaptionResourceString = null;
			this.PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 73, true);
			this.PostCodeTextBox.Name = "PostCodeTextBox";
			this.PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.PostCodeTextBox.TabIndex = 3;
			// 
			// StateDropEdit
			// 
			this.StateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StateDropEdit, "E2_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).E2_State)));
			this.StateDropEdit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.StateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 73, true);
			this.StateDropEdit.Name = "StateDropEdit";
			this.StateDropEdit.PreBoundMaxLength = 4;
			this.StateDropEdit.ShouldResizeByMaxLength = false;
			this.StateDropEdit.ShowDescriptionBox = false;
			this.StateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.StateDropEdit.TabIndex = 5;
			// 
			// CityTextBox
			// 
			this.BindingSource.SetBindingMember(this.CityTextBox, "E2_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).E2_City)));
			this.CityTextBox.CaptionResourceString = null;
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 51, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.CityTextBox.TabIndex = 4;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "E2_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).E2_RN_NKCountryCode)));
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 51, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryFindBox.ParentType = null;
			this.CountryFindBox.PreBoundMaxLength = 3;
			this.CountryFindBox.ShowDescriptionBox = false;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.CountryFindBox.TabIndex = 2;
			// 
			// ContactTabPage
			// 
			this.ContactTabPage.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("f8bb3f43-eb7b-4324-b1bf-6f3a04c79c6e", "Contact");
			this.ContactTabPage.Controls.Add(this.ContactDropEdit);
			this.ContactTabPage.Controls.Add(this.PhoneNumberControl);
			this.ContactTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContactTabPage.Name = "ContactTabPage";
			this.ContactTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 108, true);
			this.ContactTabPage.TabIndex = 3;
			// 
			// ContactDropEdit
			// 
			this.ContactDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactDropEdit, "E2_Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).E2_Contact)));
			this.ContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 9, true);
			this.ContactDropEdit.Name = "ContactDropEdit";
			this.ContactDropEdit.PreBoundMaxLength = 20;
			this.ContactDropEdit.ShouldResizeByMaxLength = true;
			this.ContactDropEdit.ShowDescriptionBox = false;
			this.ContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.ContactDropEdit.TabIndex = 6;
			// 
			// PhoneNumberControl
			// 
			this.PhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PhoneNumberControl, "PhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).PhoneNumber)));
			this.PhoneNumberControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("c6ef5090-82b6-40ab-a8d1-ac888562e042", "", "Contact Phone", "Phone", "");
			this.PhoneNumberControl.EnableValidStateColor = true;
			this.PhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 30, true);
			this.PhoneNumberControl.Name = "PhoneNumberControl";
			this.PhoneNumberControl.NumberTextBoxMaxLength = 32767;
			this.PhoneNumberControl.ShowLocalNumberLabel = false;
			this.PhoneNumberControl.ShowPublishedCheckBox = false;
			this.PhoneNumberControl.ShowToolTip = true;
			this.PhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.PhoneNumberControl.TabIndex = 7;
			this.PhoneNumberControl.UnscaledLeftPadding = -29;
			// 
			// OrganisationPanel
			// 
			this.OrganisationPanel.Controls.Add(this.OrganisationFindBox);
			this.OrganisationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrganisationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrganisationPanel.Name = "OrganisationPanel";
			this.OrganisationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 28, true);
			this.OrganisationPanel.TabIndex = 1;
			// 
			// OrganisationFindBox
			// 
			this.OrganisationFindBox.AllowDrop = true;
			this.OrganisationFindBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("f4d08f09-8029-46ec-a9e8-ce2e86faf26c", "Organization");
			this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 6, true);
			this.OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrganisationFindBox.Name = "OrganisationFindBox";
			this.OrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OrganisationFindBox.ParentType = null;
			this.OrganisationFindBox.ShowDescriptionBox = false;
			this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.OrganisationFindBox.TabIndex = 0;
			// 
			// OverrideAddressCheckbox
			// 
			this.BindingSource.SetBindingMember(this.OverrideAddressCheckbox, "E2_AddressOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.CNJobDocAddress)(null)).E2_AddressOverride)));
			this.OverrideAddressCheckbox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("6c1b242e-23b1-40e7-b336-03a64de1909c", "Override");
			this.OverrideAddressCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideAddressCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OverrideAddressCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 0, true);
			this.OverrideAddressCheckbox.Name = "OverrideAddressCheckbox";
			this.OverrideAddressCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
			this.OverrideAddressCheckbox.TabIndex = 3;
			this.OverrideAddressCheckbox.TabStop = false;
			this.OverrideAddressCheckbox.UseVisualStyleBackColor = false;
			// 
			// CNJobDocAddressControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGroupBox);
			this.Name = "CNJobDocAddressControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.OverseasPartyCodeTypeDropEdit.ResumeLayout(true);
			this.OverseasPartyCodeTypeDropEdit.PerformLayout();
			this.AddressTabPage.ResumeLayout(false);
			this.AddressTabPage.PerformLayout();
			this.AddressDropEdit.ResumeLayout(true);
			this.AddressDropEdit.PerformLayout();
			this.OverrideAddressTabPage.ResumeLayout(false);
			this.OverrideAddressTabPage.PerformLayout();
			this.StateDropEdit.ResumeLayout(true);
			this.StateDropEdit.PerformLayout();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.ContactTabPage.ResumeLayout(false);
			this.ContactTabPage.PerformLayout();
			this.ContactDropEdit.ResumeLayout(true);
			this.ContactDropEdit.PerformLayout();
			this.PhoneNumberControl.ResumeLayout(true);
			this.PhoneNumberControl.PerformLayout();
			this.OrganisationPanel.ResumeLayout(false);
			this.OrganisationPanel.PerformLayout();
			this.OrganisationFindBox.ResumeLayout(true);
			this.OrganisationFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		private ZArchitecture.GUI.ZCheckBox OverrideAddressCheckbox;
		private ZArchitecture.GUI.ZPanel OrganisationPanel;
		private MasterFiles.GUI.ZOrganisationFindBox OrganisationFindBox;
		private ZArchitecture.ZTextBox CompanyTextBox;
		private ZArchitecture.ZTextBox OverseasPartyCodeTextBox;
		private ZArchitecture.GUI.ZDropEdit OverseasPartyCodeTypeDropEdit;
		private ZArchitecture.ZTextBox CIQCodeTextBox;
		private ZArchitecture.ZTextBox CustomsCodeTextBox;
		private ZArchitecture.ZTextBox SocialCreditCodeTextBox;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth ContactDropEdit;
		private MasterFiles.GUI.PhoneNumberUserControl PhoneNumberControl;
		private Enterprise.ZArchitecture.GUI.ZTabControl DetailsTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage ContactTabPage;
		private Enterprise.ZArchitecture.ZTextBox AddressLine1TextBox;
		private Enterprise.ZArchitecture.ZTextBox AddressLine2TextBox;
		private Enterprise.ZArchitecture.ZTextBox PostCodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth StateDropEdit;
		private Enterprise.ZArchitecture.ZTextBox CityTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryFindBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage AddressTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage OverrideAddressTabPage;
		private ZAddressDropEdit.Bare AddressDropEdit;
		private Enterprise.ZArchitecture.ZLabel AddressLabel;
	}
}
