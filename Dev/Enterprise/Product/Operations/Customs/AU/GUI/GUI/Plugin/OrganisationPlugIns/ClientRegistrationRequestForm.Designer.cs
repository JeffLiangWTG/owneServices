using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class ClientRegistrationRequestForm
	{
		protected override void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			this.oKBoundButton = new ZButton();
			this.cancelBoundButton = new ZButton();
			this.detailsGroupBox = new ZGroupBox();
			this.ABNIndivDropEdit = new ZDropEdit();
			this.cACTextBox = new ZTextBox();
			this.aBNTextBox = new ZTextBox();
			this.indCheckBox = new ZCheckBox();
			this.orgCheckBox = new ZCheckBox();
			this.orLabel = new ZLabel();
			this.mainTabControl = new ZTabControl();
			this.detailsPage = new ZTabPage();
			this.eXDOCCheckBox = new ZCheckBox();
			this.evidenceCheckBox = new ZCheckBox();
			this.DOBDateEdit = new ZDateEdit();
			this.GenderDropEdit = new ZDropEdit();
			this.suffixTextBox = new ZTextBox();
			this.FamilyNameTextBox = new ZTextBox();
			this.SecondNameTextBox = new ZTextBox();
			this.FirstNameTextBox = new ZTextBox();
			this.TitleTextBox = new ZTextBox();
			this.RollsGroupBox = new ZGroupBox();
			this.RollsGrid = new ZGrid();
			this.consigneeTabControl = new ZTemplateTabControl();
			this.bATabPage = new ZTabPage();
			this.bAddrGuidDropEdit = new ZGuidDropEdit();
			this.relatedPortFindBox = new ZCodeFindBox();
			this.bStateDropEdit = new ZDropEdit();
			this.bPostcodeBoundTextBox = new ZTextBox();
			this.bCityBoundTextBox = new ZTextBox();
			this.bAddress2BoundTextBox = new ZTextBox();
			this.bAddress1BoundTextBox = new ZTextBox();
			this.pATabPage = new ZTabPage();
			this.postalAddrGuidDropEdit = new ZGuidDropEdit();
			this.postalAdrPortCodeFindBox = new ZCodeFindBox();
			this.postalAdrStateDropEdit = new ZDropEdit();
			this.postalAdrPostCodeTextBox = new ZTextBox();
			this.postalAdrCityTextBox = new ZTextBox();
			this.postalAdr2TextBox = new ZTextBox();
			this.postalAdr1TextBox = new ZTextBox();
			this.TravelDocsGroupBox = new ZGroupBox();
			this.TravelDocsGrid = new ZGrid();
			this.orgNameTextBox = new ZTextBox();
			this.contactPage = new ZTabPage();
			this.contactPhGroupBox = new ZGroupBox();
			this.emlTextBox = new ZTextBox();
			this.contactGuidDropEdit = new ZGuidDropEdit();
			this.mobCommentTextBox = new ZTextBox();
			this.contactNameTextBox = new ZTextBox();
			this.mobTextBox = new ZTextBox();
			this.ahCommentTextBox = new ZTextBox();
			this.contPurpTextBox = new ZTextBox();
			this.faxCommentTextBox = new ZTextBox();
			this.ahPrefTextBox = new ZTextBox();
			this.ahTextBox = new ZTextBox();
			this.phCommentTextBox = new ZTextBox();
			this.faxPrefTextBox = new ZTextBox();
			this.faxTextBox = new ZTextBox();
			this.phPrefixTextBox = new ZTextBox();
			this.phTextBox = new ZTextBox();
			this.contAddressesTabControl = new ZTemplateTabControl();
			this.contactAddressTabPage = new ZTabPage();
			this.contactAddrGuidDropEdit = new ZGuidDropEdit();
			this.contAdrPortCodeFindBox = new ZCodeFindBox();
			this.contStateDropEdit = new ZDropEdit();
			this.contAdrPostCodeTextBox = new ZTextBox();
			this.contAdrCityTextBox = new ZTextBox();
			this.contAddr2TextBox = new ZTextBox();
			this.contAdr1TextBox = new ZTextBox();
			this.contactPostAddressTabPage = new ZTabPage();
			this.contactPostAddrGuidDropEdit = new ZGuidDropEdit();
			this.contPostAdrPortCodeFindBox = new ZCodeFindBox();
			this.contPostAdrStateDropEdit = new ZDropEdit();
			this.contPostAdrPostCodeTextBox = new ZTextBox();
			this.contPostAdrCityTextBox = new ZTextBox();
			this.contPostAdr2TextBox = new ZTextBox();
			this.contPostAdr1TextBox = new ZTextBox();
			this.CACTypeDropEdit = new ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.detailsGroupBox.SuspendLayout();
			this.mainTabControl.SuspendLayout();
			this.detailsPage.SuspendLayout();
			this.RollsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RollsGrid)).BeginInit();
			this.consigneeTabControl.SuspendLayout();
			this.bATabPage.SuspendLayout();
			this.pATabPage.SuspendLayout();
			this.TravelDocsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TravelDocsGrid)).BeginInit();
			this.contactPage.SuspendLayout();
			this.contactPhGroupBox.SuspendLayout();
			this.contAddressesTabControl.SuspendLayout();
			this.contactAddressTabPage.SuspendLayout();
			this.contactPostAddressTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 541, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 22, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(241);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(OrgHeaderWrapper);
			// 
			// OKBoundButton
			// 
			this.oKBoundButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oKBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(669, 513, true);
			this.oKBoundButton.Name = "OKBoundButton";
			this.oKBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.oKBoundButton.TabIndex = 2;
			this.oKBoundButton.Text = "Send";
			this.oKBoundButton.Click += new EventHandler(this.OKBoundButton_Click);
			// 
			// CancelBoundButton
			// 
			this.cancelBoundButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(757, 513, true);
			this.cancelBoundButton.Name = "CancelBoundButton";
			this.cancelBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.cancelBoundButton.TabIndex = 3;
			this.cancelBoundButton.Text = "Cancel";
			this.cancelBoundButton.Click += new EventHandler(this.CancelBoundButton_Click);
			// 
			// DetailsGroupBox
			// 
			this.detailsGroupBox.Controls.Add(this.CACTypeDropEdit);
			this.detailsGroupBox.Controls.Add(this.ABNIndivDropEdit);
			this.detailsGroupBox.Controls.Add(this.cACTextBox);
			this.detailsGroupBox.Controls.Add(this.aBNTextBox);
			this.detailsGroupBox.Controls.Add(this.indCheckBox);
			this.detailsGroupBox.Controls.Add(this.orgCheckBox);
			this.detailsGroupBox.Controls.Add(this.orLabel);
			this.detailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 7, true);
			this.detailsGroupBox.Name = "DetailsGroupBox";
			this.detailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 87, true);
			this.detailsGroupBox.TabIndex = 0;
			this.detailsGroupBox.TabStop = false;
			// 
			// ABNIndivDropEdit
			// 
			this.ABNIndivDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ABNIndivDropEdit, "CLREGInfoProvider.ZA_ABNInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_ABNInd)));
			this.ABNIndivDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|7a9334c4-f8cc-46a3-8ee7-2f09a3a4966d", "ABN Nominated Client Type");
			this.ABNIndivDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(705, 19, true);
			this.ABNIndivDropEdit.Name = "ABNIndivDropEdit";
			this.ABNIndivDropEdit.PreBoundMaxLength = 3;
			this.ABNIndivDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.ABNIndivDropEdit.TabIndex = 3;
			// 
			// CACTextBox
			// 
			this.BindingSource.SetBindingMember(this.cACTextBox, "CLREGInfoProvider.ZA_CAC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_CAC)));
			this.cACTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|4db9e46a-f159-4b79-90f4-00cf7acc2dc7", "CAC");
			this.cACTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 19, true);
			this.cACTextBox.Name = "CACTextBox";
			this.cACTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.cACTextBox.TabIndex = 1;
			// 
			// ABNTextBox
			// 
			this.BindingSource.SetBindingMember(this.aBNTextBox, "CLREGInfoProvider.ZA_ABN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_ABN)));
			this.aBNTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|f1c875c2-ba07-48eb-be3e-6f919c5e92c5", "ABN");
			this.aBNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 19, true);
			this.aBNTextBox.Name = "ABNTextBox";
			this.aBNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 20, true);
			this.aBNTextBox.TabIndex = 0;
			// 
			// IndCheckBox
			// 
			this.BindingSource.SetBindingMember(this.indCheckBox, "CLREGInfoProvider.ZA_IsIndiv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_IsIndiv)));
			this.indCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.indCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 64, true);
			this.indCheckBox.Name = "IndCheckBox";
			this.indCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.indCheckBox.TabIndex = 6;
			this.indCheckBox.Text = "Individual";
			this.indCheckBox.UseVisualStyleBackColor = true;
			// 
			// OrgCheckBox
			// 
			this.BindingSource.SetBindingMember(this.orgCheckBox, "CLREGInfoProvider.ZA_IsOrg");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_IsOrg)));
			this.orgCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.orgCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 64, true);
			this.orgCheckBox.Name = "OrgCheckBox";
			this.orgCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.orgCheckBox.TabIndex = 5;
			this.orgCheckBox.Text = "Organisation";
			this.orgCheckBox.UseVisualStyleBackColor = true;
			// 
			// orLabel
			// 
			this.orLabel.AutoSize = true;
			this.orLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 44, true);
			this.orLabel.Name = "orLabel";
			this.orLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 13, true);
			this.orLabel.TabIndex = 4;
			this.orLabel.Text = "or";
			// 
			// MainTabControl
			// 
			this.mainTabControl.Controls.Add(this.detailsPage);
			this.mainTabControl.Controls.Add(this.contactPage);
			this.mainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 100, true);
			this.mainTabControl.Name = "MainTabControl";
			this.mainTabControl.SelectedIndex = 0;
			this.mainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 394, true);
			this.mainTabControl.TabIndex = 1;
			// 
			// DetailsPage
			// 
			this.detailsPage.Controls.Add(this.eXDOCCheckBox);
			this.detailsPage.Controls.Add(this.evidenceCheckBox);
			this.detailsPage.Controls.Add(this.DOBDateEdit);
			this.detailsPage.Controls.Add(this.GenderDropEdit);
			this.detailsPage.Controls.Add(this.suffixTextBox);
			this.detailsPage.Controls.Add(this.FamilyNameTextBox);
			this.detailsPage.Controls.Add(this.SecondNameTextBox);
			this.detailsPage.Controls.Add(this.FirstNameTextBox);
			this.detailsPage.Controls.Add(this.TitleTextBox);
			this.detailsPage.Controls.Add(this.RollsGroupBox);
			this.detailsPage.Controls.Add(this.consigneeTabControl);
			this.detailsPage.Controls.Add(this.TravelDocsGroupBox);
			this.detailsPage.Controls.Add(this.orgNameTextBox);
			this.detailsPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.detailsPage.Name = "DetailsPage";
			this.detailsPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.detailsPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 367, true);
			this.detailsPage.TabIndex = 0;
			this.detailsPage.Text = "Details";
			// 
			// EXDOCCheckBox
			// 
			this.BindingSource.SetBindingMember(this.eXDOCCheckBox, "CLREGInfoProvider.ZA_IsExDocsUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_IsExDocsUser)));
			this.eXDOCCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.eXDOCCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 332, true);
			this.eXDOCCheckBox.Name = "EXDOCCheckBox";
			this.eXDOCCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.eXDOCCheckBox.TabIndex = 12;
			this.eXDOCCheckBox.Text = "EXDOC User";
			this.eXDOCCheckBox.UseVisualStyleBackColor = true;
			// 
			// EvidenceCheckBox
			// 
			this.BindingSource.SetBindingMember(this.evidenceCheckBox, "CLREGInfoProvider.ZA_IsEvidenceOfID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_IsEvidenceOfID)));
			this.evidenceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.evidenceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 330, true);
			this.evidenceCheckBox.Name = "EvidenceCheckBox";
			this.evidenceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 19, true);
			this.evidenceCheckBox.TabIndex = 11;
			this.evidenceCheckBox.Text = "Evidence of Identity Confirmed";
			this.evidenceCheckBox.UseVisualStyleBackColor = true;
			// 
			// DOBDateEdit
			// 
			this.DOBDateEdit.AllowDrop = true;
			this.DOBDateEdit.AutoCompleteMonthThreshold = 1;
			this.DOBDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DOBDateEdit, "CLREGInfoProvider.ZA_DateofBirth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_DateofBirth)));
			this.DOBDateEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|6e6c122e-15c2-45e9-b0aa-4beaf6e157ce", "Date of Birth");
			this.DOBDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 55, true);
			this.DOBDateEdit.Name = "DOBDateEdit";
			this.DOBDateEdit.TabIndex = 6;
			// 
			// GenderDropEdit
			// 
			this.GenderDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GenderDropEdit, "CLREGInfoProvider.ZA_Gender");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_Gender)));
			this.GenderDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|ebde2d4b-052a-4a26-b9f3-9f62f1006aa5", "Gender");
			this.GenderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 55, true);
			this.GenderDropEdit.Name = "GenderDropEdit";
			this.GenderDropEdit.PreBoundMaxLength = 3;
			this.GenderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.GenderDropEdit.TabIndex = 7;
			// 
			// SuffixTextBox
			// 
			this.BindingSource.SetBindingMember(this.suffixTextBox, "CLREGInfoProvider.ZA_Suffix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_Suffix)));
			this.suffixTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|de8801c2-ba09-47b0-8a5b-cc50ed8ab952", "Suffix");
			this.suffixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(742, 18, true);
			this.suffixTextBox.Name = "SuffixTextBox";
			this.suffixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.suffixTextBox.TabIndex = 5;
			// 
			// FamilyNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.FamilyNameTextBox, "CLREGInfoProvider.ZA_FamilyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_FamilyName)));
			this.FamilyNameTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|a66c7be7-bcbc-4cba-88b2-64f04a749567", "Family Name");
			this.FamilyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 18, true);
			this.FamilyNameTextBox.Name = "FamilyNameTextBox";
			this.FamilyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.FamilyNameTextBox.TabIndex = 4;
			// 
			// SecondNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.SecondNameTextBox, "CLREGInfoProvider.ZA_SecondName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_SecondName)));
			this.SecondNameTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|e1ac0369-e521-4d5c-b4d1-872884adff66", "Second Name");
			this.SecondNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 18, true);
			this.SecondNameTextBox.Name = "SecondNameTextBox";
			this.SecondNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.SecondNameTextBox.TabIndex = 3;
			// 
			// FirstNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.FirstNameTextBox, "CLREGInfoProvider.ZA_FirstName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_FirstName)));
			this.FirstNameTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|32ca76e6-76be-4ceb-92d4-a930f4d17271", "First Name");
			this.FirstNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 18, true);
			this.FirstNameTextBox.Name = "FirstNameTextBox";
			this.FirstNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.FirstNameTextBox.TabIndex = 1;
			// 
			// TitleTextBox
			// 
			this.BindingSource.SetBindingMember(this.TitleTextBox, "CLREGInfoProvider.ZA_Title");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_Title)));
			this.TitleTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|080e28ae-d633-43a5-ba8e-f86c80785181", "Title");
			this.TitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 18, true);
			this.TitleTextBox.Name = "TitleTextBox";
			this.TitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.TitleTextBox.TabIndex = 0;
			// 
			// RollsGroupBox
			// 
			this.RollsGroupBox.Controls.Add(this.RollsGrid);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RollsGroupBox, false);
			this.RollsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 87, true);
			this.RollsGroupBox.Name = "RollsGroupBox";
			this.RollsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 124, true);
			this.RollsGroupBox.TabIndex = 8;
			this.RollsGroupBox.TabStop = false;
			this.RollsGroupBox.Text = "Client Roles";
			// 
			// RollsGrid
			// 
			this.RollsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RollsGrid, "CLREGInfoProvider.Rolls");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.Rolls)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Roll)(((System.Collections.IList)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.Rolls)).SyncRoot)).ZA_Roll)));
			this.RollsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.Caption = "Client Role";
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "ZA_Roll";
			zDropEditColumnStyleInfo3.IsMandatory = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.RollsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.RollsGrid.GridId = "b00527b8-4b36-4adb-9ad0-c63200b626fb";
			this.RollsGrid.CopySelectedRowsAllowed = true;
			this.RollsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RollsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RollsGrid.LayoutKey = "zGrid1";
			this.RollsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.RollsGrid.Name = "RollsGrid";
			this.RollsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 105, true);
			this.RollsGrid.TabIndex = 0;
			// 
			// ConsigneeTabControl
			// 
			this.consigneeTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.consigneeTabControl.Controls.Add(this.bATabPage);
			this.consigneeTabControl.Controls.Add(this.pATabPage);
			this.consigneeTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 87, true);
			this.consigneeTabControl.Multiline = true;
			this.consigneeTabControl.Name = "ConsigneeTabControl";
			this.consigneeTabControl.SelectedIndex = 0;
			this.consigneeTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 229, true);
			this.consigneeTabControl.TabIndex = 10;
			// 
			// BATabPage
			// 
			this.bATabPage.Controls.Add(this.bAddrGuidDropEdit);
			this.bATabPage.Controls.Add(this.relatedPortFindBox);
			this.bATabPage.Controls.Add(this.bStateDropEdit);
			this.bATabPage.Controls.Add(this.bPostcodeBoundTextBox);
			this.bATabPage.Controls.Add(this.bCityBoundTextBox);
			this.bATabPage.Controls.Add(this.bAddress2BoundTextBox);
			this.bATabPage.Controls.Add(this.bAddress1BoundTextBox);
			this.bATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.bATabPage.Name = "BATabPage";
			this.bATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 202, true);
			this.bATabPage.TabIndex = 0;
			this.bATabPage.Text = "Business Address";
			// 
			// BAddrGuidDropEdit
			// 
			this.bAddrGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bAddrGuidDropEdit, "CLREGInfoProvider.ZA_OA_BusinessAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_OA_BusinessAddress)));
			this.bAddrGuidDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|c9cfe171-a832-42d4-a9e4-bcaac3ff58ed", "Address");
			this.bAddrGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 12, true);
			this.bAddrGuidDropEdit.Name = "BAddrGuidDropEdit";
			this.bAddrGuidDropEdit.PreBoundMaxLength = 35;
			this.bAddrGuidDropEdit.ShowDescriptionBox = false;
			this.bAddrGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.bAddrGuidDropEdit.TabIndex = 0;
			// 
			// RelatedPortFindBox
			// 
			this.relatedPortFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.relatedPortFindBox, "CLREGInfoProvider.ZA_BsnPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_BsnPort)));
			this.relatedPortFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|1457390e-076a-4aca-b19e-5be9cf890a61", "UNLOCO");
			this.relatedPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 144, true);
			this.relatedPortFindBox.Name = "RelatedPortFindBox";
			this.relatedPortFindBox.ShowDescriptionBox = false;
			this.relatedPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.relatedPortFindBox.TabIndex = 5;
			// 
			// BStateDropEdit
			// 
			this.bStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bStateDropEdit, "CLREGInfoProvider.ZA_BsnState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_BsnState)));
			this.bStateDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|2d850e0d-8e46-484c-bae8-662e9d55b461", "State/Province");
			this.bStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 170, true);
			this.bStateDropEdit.Name = "BStateDropEdit";
			this.bStateDropEdit.PreBoundMaxLength = 3;
			this.bStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.bStateDropEdit.TabIndex = 6;
			// 
			// BPostcodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.bPostcodeBoundTextBox, "CLREGInfoProvider.ZA_BsnPostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_BsnPostCode)));
			this.bPostcodeBoundTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|217efe77-d58b-49df-9b83-07776332eaab", "Post Code");
			this.bPostcodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 118, true);
			this.bPostcodeBoundTextBox.Name = "BPostcodeBoundTextBox";
			this.bPostcodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.bPostcodeBoundTextBox.TabIndex = 4;
			// 
			// BCityBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.bCityBoundTextBox, "CLREGInfoProvider.ZA_BsnCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_BsnCity)));
			this.bCityBoundTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|059c89ca-aecd-40c3-b7f6-429b9d6c4237", "Town");
			this.bCityBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 91, true);
			this.bCityBoundTextBox.Name = "BCityBoundTextBox";
			this.bCityBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.bCityBoundTextBox.TabIndex = 3;
			// 
			// BAddress2BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.bAddress2BoundTextBox, "CLREGInfoProvider.ZA_Bsn2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_Bsn2)));
			this.bAddress2BoundTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|89a0bdd3-8f57-4bf2-9b09-86cb8e4b98e3", "Line 2");
			this.bAddress2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 64, true);
			this.bAddress2BoundTextBox.Name = "BAddress2BoundTextBox";
			this.bAddress2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.bAddress2BoundTextBox.TabIndex = 2;
			// 
			// BAddress1BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.bAddress1BoundTextBox, "CLREGInfoProvider.ZA_Bsn1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_Bsn1)));
			this.bAddress1BoundTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|5f7a062a-b1bc-45f6-8708-41358fc7064d", "Line 1");
			this.bAddress1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 38, true);
			this.bAddress1BoundTextBox.Name = "BAddress1BoundTextBox";
			this.bAddress1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.bAddress1BoundTextBox.TabIndex = 1;
			// 
			// PATabPage
			// 
			this.pATabPage.Controls.Add(this.postalAddrGuidDropEdit);
			this.pATabPage.Controls.Add(this.postalAdrPortCodeFindBox);
			this.pATabPage.Controls.Add(this.postalAdrStateDropEdit);
			this.pATabPage.Controls.Add(this.postalAdrPostCodeTextBox);
			this.pATabPage.Controls.Add(this.postalAdrCityTextBox);
			this.pATabPage.Controls.Add(this.postalAdr2TextBox);
			this.pATabPage.Controls.Add(this.postalAdr1TextBox);
			this.pATabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.pATabPage.Name = "PATabPage";
			this.pATabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 202, true);
			this.pATabPage.TabIndex = 2;
			this.pATabPage.Text = "Postal Address";
			// 
			// PostalAddrGuidDropEdit
			// 
			this.postalAddrGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.postalAddrGuidDropEdit, "CLREGInfoProvider.ZA_OA_PostalAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_OA_PostalAddress)));
			this.postalAddrGuidDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|b5d8e1c2-ca6b-41f5-8799-b01904f92fe2", "Address");
			this.postalAddrGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 12, true);
			this.postalAddrGuidDropEdit.Name = "PostalAddrGuidDropEdit";
			this.postalAddrGuidDropEdit.PreBoundMaxLength = 35;
			this.postalAddrGuidDropEdit.ShowDescriptionBox = false;
			this.postalAddrGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.postalAddrGuidDropEdit.TabIndex = 0;
			// 
			// PostalAdrPortCodeFindBox
			// 
			this.postalAdrPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.postalAdrPortCodeFindBox, "CLREGInfoProvider.ZA_PostPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_PostPort)));
			this.postalAdrPortCodeFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|ce45eed1-2801-4bd2-b83c-b61dfda47899", "UNLOCO");
			this.postalAdrPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 144, true);
			this.postalAdrPortCodeFindBox.Name = "PostalAdrPortCodeFindBox";
			this.postalAdrPortCodeFindBox.ShowDescriptionBox = false;
			this.postalAdrPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.postalAdrPortCodeFindBox.TabIndex = 5;
			// 
			// PostalAdrStateDropEdit
			// 
			this.postalAdrStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.postalAdrStateDropEdit, "CLREGInfoProvider.ZA_PostState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_PostState)));
			this.postalAdrStateDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|a01c0a57-e3b3-45b9-8de2-6411f9f2cf6f", "State/Province");
			this.postalAdrStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 170, true);
			this.postalAdrStateDropEdit.Name = "PostalAdrStateDropEdit";
			this.postalAdrStateDropEdit.PreBoundMaxLength = 3;
			this.postalAdrStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.postalAdrStateDropEdit.TabIndex = 6;
			// 
			// PostalAdrPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.postalAdrPostCodeTextBox, "CLREGInfoProvider.ZA_PostPostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_PostPostCode)));
			this.postalAdrPostCodeTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|dd18163d-2adc-4f03-a9ab-3b942191b431", "Post Code");
			this.postalAdrPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 118, true);
			this.postalAdrPostCodeTextBox.Name = "PostalAdrPostCodeTextBox";
			this.postalAdrPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.postalAdrPostCodeTextBox.TabIndex = 4;
			// 
			// PostalAdrCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.postalAdrCityTextBox, "CLREGInfoProvider.ZA_PostCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_PostCity)));
			this.postalAdrCityTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|1c3b2941-c2c6-47b8-b56e-ec7cac44b606", "Town");
			this.postalAdrCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 91, true);
			this.postalAdrCityTextBox.Name = "PostalAdrCityTextBox";
			this.postalAdrCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.postalAdrCityTextBox.TabIndex = 3;
			// 
			// PostalAdr2TextBox
			// 
			this.BindingSource.SetBindingMember(this.postalAdr2TextBox, "CLREGInfoProvider.ZA_Post2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_Post2)));
			this.postalAdr2TextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|2b23b765-2571-4856-a2dc-5386767f05ad", "Line 2");
			this.postalAdr2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 64, true);
			this.postalAdr2TextBox.Name = "PostalAdr2TextBox";
			this.postalAdr2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.postalAdr2TextBox.TabIndex = 2;
			// 
			// PostalAdr1TextBox
			// 
			this.BindingSource.SetBindingMember(this.postalAdr1TextBox, "CLREGInfoProvider.ZA_Post1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_Post1)));
			this.postalAdr1TextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|07acf785-0c9e-457f-b885-01dd70aa8a59", "Line 1");
			this.postalAdr1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 38, true);
			this.postalAdr1TextBox.Name = "PostalAdr1TextBox";
			this.postalAdr1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.postalAdr1TextBox.TabIndex = 1;
			// 
			// TravelDocsGroupBox
			// 
			this.TravelDocsGroupBox.Controls.Add(this.TravelDocsGrid);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TravelDocsGroupBox, false);
			this.TravelDocsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 225, true);
			this.TravelDocsGroupBox.Name = "TravelDocsGroupBox";
			this.TravelDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 124, true);
			this.TravelDocsGroupBox.TabIndex = 9;
			this.TravelDocsGroupBox.TabStop = false;
			this.TravelDocsGroupBox.Text = "Travel Documents";
			// 
			// TravelDocsGrid
			// 
			this.TravelDocsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TravelDocsGrid, "CLREGInfoProvider.TravelDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.TravelDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TravelDocument)(((System.Collections.IList)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.TravelDocuments)).SyncRoot)).ZA_DocumentNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TravelDocument)(((System.Collections.IList)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.TravelDocuments)).SyncRoot)).ZA_Country)));
			this.TravelDocsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.Caption = "Document Number";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "ZA_DocumentNo";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo4.Caption = "Ctry/Rgn.";
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "ZA_Country";
			zDropEditColumnStyleInfo4.IsMandatory = true;
			this.TravelDocsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TravelDocsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TravelDocsGrid.GridId = "1eea3a76-d153-47d0-81d8-a88a6d6b1e48";
			this.TravelDocsGrid.CopySelectedRowsAllowed = true;
			this.TravelDocsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TravelDocsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TravelDocsGrid.LayoutKey = "zGrid1";
			this.TravelDocsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TravelDocsGrid.Name = "TravelDocsGrid";
			this.TravelDocsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 105, true);
			this.TravelDocsGrid.TabIndex = 0;
			// 
			// OrgNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.orgNameTextBox, "CLREGInfoProvider.ZA_BusinessName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_BusinessName)));
			this.orgNameTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|ddc44906-6f3c-446a-9259-d95fe8be2ee1", "Organization Name");
			this.orgNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 18, true);
			this.orgNameTextBox.Name = "OrgNameTextBox";
			this.orgNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 20, true);
			this.orgNameTextBox.TabIndex = 0;
			// 
			// ContactPage
			// 
			this.contactPage.Controls.Add(this.contactPhGroupBox);
			this.contactPage.Controls.Add(this.contAddressesTabControl);
			this.contactPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.contactPage.Name = "ContactPage";
			this.contactPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.contactPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 367, true);
			this.contactPage.TabIndex = 1;
			this.contactPage.Text = "Contact";
			// 
			// ContactPhGroupBox
			// 
			this.contactPhGroupBox.Controls.Add(this.emlTextBox);
			this.contactPhGroupBox.Controls.Add(this.contactGuidDropEdit);
			this.contactPhGroupBox.Controls.Add(this.mobCommentTextBox);
			this.contactPhGroupBox.Controls.Add(this.contactNameTextBox);
			this.contactPhGroupBox.Controls.Add(this.mobTextBox);
			this.contactPhGroupBox.Controls.Add(this.ahCommentTextBox);
			this.contactPhGroupBox.Controls.Add(this.contPurpTextBox);
			this.contactPhGroupBox.Controls.Add(this.faxCommentTextBox);
			this.contactPhGroupBox.Controls.Add(this.ahPrefTextBox);
			this.contactPhGroupBox.Controls.Add(this.ahTextBox);
			this.contactPhGroupBox.Controls.Add(this.phCommentTextBox);
			this.contactPhGroupBox.Controls.Add(this.faxPrefTextBox);
			this.contactPhGroupBox.Controls.Add(this.faxTextBox);
			this.contactPhGroupBox.Controls.Add(this.phPrefixTextBox);
			this.contactPhGroupBox.Controls.Add(this.phTextBox);
			this.contactPhGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.contactPhGroupBox.Name = "ContactPhGroupBox";
			this.contactPhGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 356, true);
			this.contactPhGroupBox.TabIndex = 0;
			this.contactPhGroupBox.TabStop = false;
			// 
			// EmlTextBox
			// 
			this.BindingSource.SetBindingMember(this.emlTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContEmail)));
			this.emlTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|42e34d2b-fe24-4dd5-a431-9a8c9de80b23", "Email");
			this.emlTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 329, true);
			this.emlTextBox.Name = "EmlTextBox";
			this.emlTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.emlTextBox.TabIndex = 14;
			// 
			// ContactGuidDropEdit
			// 
			this.contactGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contactGuidDropEdit, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_OC_Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_OC_Contact)));
			this.contactGuidDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|63c89515-bf0a-40f0-bddf-ac9b0a3ede74", "Contact");
			this.contactGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 19, true);
			this.contactGuidDropEdit.Name = "ContactGuidDropEdit";
			this.contactGuidDropEdit.PreBoundMaxLength = 35;
			this.contactGuidDropEdit.ShowDescriptionBox = false;
			this.contactGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.contactGuidDropEdit.TabIndex = 0;
			// 
			// MobCommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.mobCommentTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContMobComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContMobComment)));
			this.mobCommentTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|22cb31c1-b994-47d0-9e4e-046aaec3d0db", "Comments");
			this.mobCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 303, true);
			this.mobCommentTextBox.Name = "MobCommentTextBox";
			this.mobCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.mobCommentTextBox.TabIndex = 13;
			// 
			// ContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.contactNameTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContName)));
			this.contactNameTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|375b4516-ab06-4d70-90ab-55cbcdb6985d", "Contact Name");
			this.contactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 45, true);
			this.contactNameTextBox.Name = "ContactNameTextBox";
			this.contactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.contactNameTextBox.TabIndex = 1;
			// 
			// MobTextBox
			// 
			this.BindingSource.SetBindingMember(this.mobTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContMob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContMob)));
			this.mobTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|fe207c4c-e08a-4914-9280-c87f021b9929", "Mobile");
			this.mobTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 277, true);
			this.mobTextBox.Name = "MobTextBox";
			this.mobTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.mobTextBox.TabIndex = 12;
			// 
			// AhCommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.ahCommentTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContAHComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContAHComment)));
			this.ahCommentTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|3625f882-09e7-4322-95bf-a96e16e2b57b", "Comments");
			this.ahCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 242, true);
			this.ahCommentTextBox.Name = "AhCommentTextBox";
			this.ahCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.ahCommentTextBox.TabIndex = 11;
			// 
			// ContPurpTextBox
			// 
			this.BindingSource.SetBindingMember(this.contPurpTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPurpose");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPurpose)));
			this.contPurpTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|521ea5b2-b58b-4f48-824a-5cff986f9052", "Purpose");
			this.contPurpTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 71, true);
			this.contPurpTextBox.Name = "ContPurpTextBox";
			this.contPurpTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.contPurpTextBox.TabIndex = 2;
			// 
			// FaxCommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.faxCommentTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContFaxComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContFaxComment)));
			this.faxCommentTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|bfcff765-1bdf-48ac-8f07-300fe475b5ea", "Comments");
			this.faxCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 182, true);
			this.faxCommentTextBox.Name = "FaxCommentTextBox";
			this.faxCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.faxCommentTextBox.TabIndex = 8;
			// 
			// AhPrefTextBox
			// 
			this.BindingSource.SetBindingMember(this.ahPrefTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContAHPref");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContAHPref)));
			this.ahPrefTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|1cf5317d-5e84-4873-83be-fdac877a6cab", "After Hours");
			this.ahPrefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 216, true);
			this.ahPrefTextBox.Name = "AhPrefTextBox";
			this.ahPrefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.ahPrefTextBox.TabIndex = 9;
			// 
			// AhTextBox
			// 
			this.BindingSource.SetBindingMember(this.ahTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContAH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContAH)));
			this.ahTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 216, true);
			this.ahTextBox.Name = "AhTextBox";
			this.ahTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.ahTextBox.TabIndex = 10;
			// 
			// PhCommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.phCommentTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPhComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPhComment)));
			this.phCommentTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|4286b0fa-5e37-4e1b-a716-af1d3e799367", "Comments");
			this.phCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 123, true);
			this.phCommentTextBox.Name = "PhCommentTextBox";
			this.phCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.phCommentTextBox.TabIndex = 5;
			// 
			// FaxPrefTextBox
			// 
			this.BindingSource.SetBindingMember(this.faxPrefTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContFaxPref");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContFaxPref)));
			this.faxPrefTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|df369814-b02d-458f-b43b-dc8961275e56", "Fax");
			this.faxPrefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 156, true);
			this.faxPrefTextBox.Name = "FaxPrefTextBox";
			this.faxPrefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.faxPrefTextBox.TabIndex = 6;
			// 
			// FaxTextBox
			// 
			this.BindingSource.SetBindingMember(this.faxTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContFax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContFax)));
			this.faxTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 156, true);
			this.faxTextBox.Name = "FaxTextBox";
			this.faxTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.faxTextBox.TabIndex = 7;
			// 
			// PhPrefixTextBox
			// 
			this.BindingSource.SetBindingMember(this.phPrefixTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPhPref");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPhPref)));
			this.phPrefixTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|442a152c-d829-4be8-919d-2e78d3fbab26", "Business Phone");
			this.phPrefixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 97, true);
			this.phPrefixTextBox.Name = "PhPrefixTextBox";
			this.phPrefixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.phPrefixTextBox.TabIndex = 3;
			// 
			// PhTextBox
			// 
			this.BindingSource.SetBindingMember(this.phTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPh");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPh)));
			this.phTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 97, true);
			this.phTextBox.Name = "PhTextBox";
			this.phTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 20, true);
			this.phTextBox.TabIndex = 4;
			// 
			// ContAddressesTabControl
			// 
			this.contAddressesTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.contAddressesTabControl.Controls.Add(this.contactAddressTabPage);
			this.contAddressesTabControl.Controls.Add(this.contactPostAddressTabPage);
			this.contAddressesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 15, true);
			this.contAddressesTabControl.Multiline = true;
			this.contAddressesTabControl.Name = "ContAddressesTabControl";
			this.contAddressesTabControl.SelectedIndex = 0;
			this.contAddressesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 227, true);
			this.contAddressesTabControl.TabIndex = 1;
			// 
			// ContactAddressTabPage
			// 
			this.contactAddressTabPage.Controls.Add(this.contactAddrGuidDropEdit);
			this.contactAddressTabPage.Controls.Add(this.contAdrPortCodeFindBox);
			this.contactAddressTabPage.Controls.Add(this.contStateDropEdit);
			this.contactAddressTabPage.Controls.Add(this.contAdrPostCodeTextBox);
			this.contactAddressTabPage.Controls.Add(this.contAdrCityTextBox);
			this.contactAddressTabPage.Controls.Add(this.contAddr2TextBox);
			this.contactAddressTabPage.Controls.Add(this.contAdr1TextBox);
			this.contactAddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.contactAddressTabPage.Name = "ContactAddressTabPage";
			this.contactAddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 200, true);
			this.contactAddressTabPage.TabIndex = 0;
			this.contactAddressTabPage.Text = "Contact Address";
			// 
			// ContactAddrGuidDropEdit
			// 
			this.contactAddrGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contactAddrGuidDropEdit, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_OA_ContactAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_OA_ContactAddress)));
			this.contactAddrGuidDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|02c23917-ea49-4b66-85c3-8bcf3a0dc3c4", "Address");
			this.contactAddrGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 14, true);
			this.contactAddrGuidDropEdit.Name = "ContactAddrGuidDropEdit";
			this.contactAddrGuidDropEdit.PreBoundMaxLength = 35;
			this.contactAddrGuidDropEdit.ShowDescriptionBox = false;
			this.contactAddrGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.contactAddrGuidDropEdit.TabIndex = 0;
			// 
			// ContAdrPortCodeFindBox
			// 
			this.contAdrPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contAdrPortCodeFindBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPort)));
			this.contAdrPortCodeFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|eb6a667b-d9fa-4e77-8380-1efb540b06a3", "UNLOCO");
			this.contAdrPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 145, true);
			this.contAdrPortCodeFindBox.Name = "ContAdrPortCodeFindBox";
			this.contAdrPortCodeFindBox.ShowDescriptionBox = false;
			this.contAdrPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.contAdrPortCodeFindBox.TabIndex = 5;
			// 
			// ContStateDropEdit
			// 
			this.contStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contStateDropEdit, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContState)));
			this.contStateDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|25c8d022-18f7-4d85-b1df-2cef6df3eb64", "State");
			this.contStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 171, true);
			this.contStateDropEdit.Name = "ContStateDropEdit";
			this.contStateDropEdit.PreBoundMaxLength = 3;
			this.contStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.contStateDropEdit.TabIndex = 6;
			// 
			// ContAdrPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.contAdrPostCodeTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPostCode)));
			this.contAdrPostCodeTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|c5de31f9-62bf-4eef-a11f-9c558cb96a22", "Post Code");
			this.contAdrPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 119, true);
			this.contAdrPostCodeTextBox.Name = "ContAdrPostCodeTextBox";
			this.contAdrPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.contAdrPostCodeTextBox.TabIndex = 4;
			// 
			// ContAdrCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.contAdrCityTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContCity)));
			this.contAdrCityTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|9b33d4c1-f39a-4ffa-bfc8-3095d4d4ff69", "Town");
			this.contAdrCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 92, true);
			this.contAdrCityTextBox.Name = "ContAdrCityTextBox";
			this.contAdrCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.contAdrCityTextBox.TabIndex = 3;
			// 
			// ContAddr2TextBox
			// 
			this.BindingSource.SetBindingMember(this.contAddr2TextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_Cont2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_Cont2)));
			this.contAddr2TextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|67f617c5-c24f-48e1-9f51-7a5e31ea2da7", "Line 2");
			this.contAddr2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 65, true);
			this.contAddr2TextBox.Name = "ContAddr2TextBox";
			this.contAddr2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.contAddr2TextBox.TabIndex = 2;
			// 
			// ContAdr1TextBox
			// 
			this.BindingSource.SetBindingMember(this.contAdr1TextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_Cont1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_Cont1)));
			this.contAdr1TextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|a498829c-5597-4636-ad1a-b03f9c748179", "Line 1");
			this.contAdr1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 40, true);
			this.contAdr1TextBox.Name = "ContAdr1TextBox";
			this.contAdr1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.contAdr1TextBox.TabIndex = 1;
			// 
			// ContactPostAddressTabPage
			// 
			this.contactPostAddressTabPage.Controls.Add(this.contactPostAddrGuidDropEdit);
			this.contactPostAddressTabPage.Controls.Add(this.contPostAdrPortCodeFindBox);
			this.contactPostAddressTabPage.Controls.Add(this.contPostAdrStateDropEdit);
			this.contactPostAddressTabPage.Controls.Add(this.contPostAdrPostCodeTextBox);
			this.contactPostAddressTabPage.Controls.Add(this.contPostAdrCityTextBox);
			this.contactPostAddressTabPage.Controls.Add(this.contPostAdr2TextBox);
			this.contactPostAddressTabPage.Controls.Add(this.contPostAdr1TextBox);
			this.contactPostAddressTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.contactPostAddressTabPage.Name = "ContactPostAddressTabPage";
			this.contactPostAddressTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 200, true);
			this.contactPostAddressTabPage.TabIndex = 2;
			this.contactPostAddressTabPage.Text = "Contact Postal Address";
			// 
			// ContactPostAddrGuidDropEdit
			// 
			this.contactPostAddrGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contactPostAddrGuidDropEdit, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_OA_ContactPostalAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_OA_ContactPostalAddress)));
			this.contactPostAddrGuidDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|4cb6a276-9e54-4037-8d06-46f5a11a72f8", "Address");
			this.contactPostAddrGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 14, true);
			this.contactPostAddrGuidDropEdit.Name = "ContactPostAddrGuidDropEdit";
			this.contactPostAddrGuidDropEdit.PreBoundMaxLength = 35;
			this.contactPostAddrGuidDropEdit.ShowDescriptionBox = false;
			this.contactPostAddrGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.contactPostAddrGuidDropEdit.TabIndex = 0;
			// 
			// ContPostAdrPortCodeFindBox
			// 
			this.contPostAdrPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contPostAdrPortCodeFindBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPostPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPostPort)));
			this.contPostAdrPortCodeFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|61b4b162-e17b-49c5-9543-9776123830fe", "UNLOCO");
			this.contPostAdrPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 145, true);
			this.contPostAdrPortCodeFindBox.Name = "ContPostAdrPortCodeFindBox";
			this.contPostAdrPortCodeFindBox.ShowDescriptionBox = false;
			this.contPostAdrPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.contPostAdrPortCodeFindBox.TabIndex = 5;
			// 
			// ContPostAdrStateDropEdit
			// 
			this.contPostAdrStateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contPostAdrStateDropEdit, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPostState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPostState)));
			this.contPostAdrStateDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|855cd16d-9415-4569-8cea-ffd391a3014b", "State");
			this.contPostAdrStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 171, true);
			this.contPostAdrStateDropEdit.Name = "ContPostAdrStateDropEdit";
			this.contPostAdrStateDropEdit.PreBoundMaxLength = 3;
			this.contPostAdrStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.contPostAdrStateDropEdit.TabIndex = 6;
			// 
			// ContPostAdrPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.contPostAdrPostCodeTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPostPostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPostPostCode)));
			this.contPostAdrPostCodeTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|9cde065d-18f3-4157-9f1b-73e90c64ab22", "Post Code");
			this.contPostAdrPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 119, true);
			this.contPostAdrPostCodeTextBox.Name = "ContPostAdrPostCodeTextBox";
			this.contPostAdrPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.contPostAdrPostCodeTextBox.TabIndex = 4;
			// 
			// ContPostAdrCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.contPostAdrCityTextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPostCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPostCity)));
			this.contPostAdrCityTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|281e0b50-1a56-4b65-b7b3-8c3776300b3b", "Town");
			this.contPostAdrCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 92, true);
			this.contPostAdrCityTextBox.Name = "ContPostAdrCityTextBox";
			this.contPostAdrCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.contPostAdrCityTextBox.TabIndex = 3;
			// 
			// ContPostAdr2TextBox
			// 
			this.BindingSource.SetBindingMember(this.contPostAdr2TextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPost2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPost2)));
			this.contPostAdr2TextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|659a7928-b18f-4f71-87fe-3f6d511bd0b8", "Line 2");
			this.contPostAdr2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 65, true);
			this.contPostAdr2TextBox.Name = "ContPostAdr2TextBox";
			this.contPostAdr2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.contPostAdr2TextBox.TabIndex = 2;
			// 
			// ContPostAdr1TextBox
			// 
			this.BindingSource.SetBindingMember(this.contPostAdr1TextBox, "CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPost1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.CLREGContactInfoProvider.ZA_ContPost1)));
			this.contPostAdr1TextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|c1aeab27-865b-4aa6-a40b-df6b4533bfcb", "Line 1");
			this.contPostAdr1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 40, true);
			this.contPostAdr1TextBox.Name = "ContPostAdr1TextBox";
			this.contPostAdr1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.contPostAdr1TextBox.TabIndex = 1;
			// 
			// CACTypeDropEdit
			// 
			this.CACTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CACTypeDropEdit, "CLREGInfoProvider.ZA_CACType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrgHeaderWrapper)(null)).CLREGInfoProvider.ZA_CACType)));
			this.CACTypeDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ClientRegistrationRequestForm|29a2ac1c-894f-4e32-9a17-94f2a8afa72d", "CAC Address Type");
			this.CACTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 19, true);
			this.CACTypeDropEdit.Name = "CACTypeDropEdit";
			this.CACTypeDropEdit.PreBoundMaxLength = 3;
			this.CACTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 20, true);
			this.CACTypeDropEdit.TabIndex = 2;
			// 
			// ClientRegistrationRequestForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 563, true);
			this.Controls.Add(this.mainTabControl);
			this.Controls.Add(this.detailsGroupBox);
			this.Controls.Add(this.oKBoundButton);
			this.Controls.Add(this.cancelBoundButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(OrgHeaderWrapper);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.OrgHeaderWrapper";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 267, true);
			this.Name = "ClientRegistrationRequestForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.cancelBoundButton, 0);
			this.Controls.SetChildIndex(this.oKBoundButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.detailsGroupBox, 0);
			this.Controls.SetChildIndex(this.mainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.detailsGroupBox.ResumeLayout(false);
			this.detailsGroupBox.PerformLayout();
			this.mainTabControl.ResumeLayout(false);
			this.detailsPage.ResumeLayout(false);
			this.detailsPage.PerformLayout();
			this.RollsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RollsGrid)).EndInit();
			this.consigneeTabControl.ResumeLayout(false);
			this.bATabPage.ResumeLayout(false);
			this.bATabPage.PerformLayout();
			this.pATabPage.ResumeLayout(false);
			this.pATabPage.PerformLayout();
			this.TravelDocsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TravelDocsGrid)).EndInit();
			this.contactPage.ResumeLayout(false);
			this.contactPhGroupBox.ResumeLayout(false);
			this.contactPhGroupBox.PerformLayout();
			this.contAddressesTabControl.ResumeLayout(false);
			this.contactAddressTabPage.ResumeLayout(false);
			this.contactAddressTabPage.PerformLayout();
			this.contactPostAddressTabPage.ResumeLayout(false);
			this.contactPostAddressTabPage.PerformLayout();
			this.ResumeLayout(false);
		}

		protected internal ZGrid RollsGrid;
		protected internal ZGrid TravelDocsGrid;
		internal ZDropEdit ABNIndivDropEdit;
		internal ZDropEdit CACTypeDropEdit;
		ZButton oKBoundButton;
		ZGroupBox detailsGroupBox;
		ZTextBox cACTextBox;
		ZTextBox aBNTextBox;
		ZCheckBox indCheckBox;
		ZCheckBox orgCheckBox;
		ZLabel orLabel;
		ZTabControl mainTabControl;
		ZTabPage contactPage;
		ZTabPage detailsPage;
		protected internal ZGroupBox TravelDocsGroupBox;
		protected internal ZGroupBox RollsGroupBox;
		ZTemplateTabControl consigneeTabControl;
		ZTabPage bATabPage;
		ZTextBox bPostcodeBoundTextBox;
		ZTextBox bCityBoundTextBox;
		ZTextBox bAddress2BoundTextBox;
		ZTextBox bAddress1BoundTextBox;
		ZTabPage pATabPage;
		ZDropEdit bStateDropEdit;
		ZCodeFindBox relatedPortFindBox;
		ZCodeFindBox postalAdrPortCodeFindBox;
		ZDropEdit postalAdrStateDropEdit;
		ZTextBox postalAdrPostCodeTextBox;
		ZTextBox postalAdrCityTextBox;
		ZTextBox postalAdr2TextBox;
		ZTextBox postalAdr1TextBox;
		ZTextBox orgNameTextBox;
		internal ZTextBox TitleTextBox;
		internal ZTextBox FirstNameTextBox;
		internal ZTextBox SecondNameTextBox;
		internal ZTextBox FamilyNameTextBox;
		ZTextBox suffixTextBox;
		internal ZDropEdit GenderDropEdit;
		internal ZDateEdit DOBDateEdit;
		ZTemplateTabControl contAddressesTabControl;
		ZTabPage contactAddressTabPage;
		ZCodeFindBox contAdrPortCodeFindBox;
		ZDropEdit contStateDropEdit;
		ZTextBox contAdrPostCodeTextBox;
		ZTextBox contAdrCityTextBox;
		ZTextBox contAddr2TextBox;
		ZTextBox contAdr1TextBox;
		ZTabPage contactPostAddressTabPage;
		ZCodeFindBox contPostAdrPortCodeFindBox;
		ZDropEdit contPostAdrStateDropEdit;
		ZTextBox contPostAdrPostCodeTextBox;
		ZTextBox contPostAdrCityTextBox;
		ZTextBox contPostAdr2TextBox;
		ZTextBox contPostAdr1TextBox;
		ZTextBox contPurpTextBox;
		ZTextBox contactNameTextBox;
		ZGroupBox contactPhGroupBox;
		ZTextBox phCommentTextBox;
		ZTextBox phPrefixTextBox;
		ZTextBox phTextBox;
		ZTextBox faxCommentTextBox;
		ZTextBox faxPrefTextBox;
		ZTextBox faxTextBox;
		ZTextBox ahCommentTextBox;
		ZTextBox ahPrefTextBox;
		ZTextBox ahTextBox;
		ZTextBox mobCommentTextBox;
		ZTextBox mobTextBox;
		ZTextBox emlTextBox;
		ZButton cancelBoundButton;
		ZCheckBox eXDOCCheckBox;
		ZCheckBox evidenceCheckBox;
		ZGuidDropEdit bAddrGuidDropEdit;
		ZGuidDropEdit postalAddrGuidDropEdit;
		ZGuidDropEdit contactPostAddrGuidDropEdit;
		ZGuidDropEdit contactGuidDropEdit;
		ZGuidDropEdit contactAddrGuidDropEdit;
	}
}
