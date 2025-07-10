using System;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAMiscOptionsUserControl
	{
		void InitializeComponent()
		{
			this.pGAOptionsGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.bondGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.aTDExCodeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.amendReasonCodeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.bondTypeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.woodPackagingIndCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.cAMergeByDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.permitApplicationCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.inspectionArrangementsCompleteCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.oGDNRCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.oGDTCCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.oGDICCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.oGDCFIACheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.importDeclarationOptionsGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.lPCOGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.lpcoDetailUserControl1 = new LPCODetailUserControl();
			this.LPCOGridUserControl = new LPCOGridUserControl();
			this.anySightDepositAmountCalcEdit = new ZArchitecture.ZCalcEdit();
			this.defaultFreightCalcEdit = new ZArchitecture.ZCalcEdit();
			this.commentLabel = new ZArchitecture.ZLabel();
			this.cSAEntryCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.bondNumberTextBox = new ZArchitecture.ZTextBox();
			this.suretyCodeTextBox = new ZArchitecture.ZTextBox();
			this.refreshBondButton = new ZArchitecture.GUI.ZButton();
			this.PaymentPartyDropEdit.SuspendLayout();
			this.MiscOptionsGroupBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.MergeByDropEdit.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.pGAOptionsGroupBox.SuspendLayout();
			this.bondGroupBox.SuspendLayout();
			this.aTDExCodeDropEdit.SuspendLayout();
			this.amendReasonCodeDropEdit.SuspendLayout();
			this.bondTypeDropEdit.SuspendLayout();
			this.cAMergeByDropEdit.SuspendLayout();
			this.importDeclarationOptionsGroupBox.SuspendLayout();
			this.lPCOGroupBox.SuspendLayout();
			this.lpcoDetailUserControl1.SuspendLayout();
			this.LPCOGridUserControl.SuspendLayout();
			this.suretyCodeTextBox.SuspendLayout();
			this.bondNumberTextBox.SuspendLayout();
			this.refreshBondButton.SuspendLayout();
			this.SuspendLayout();
			// 
			// PaymentPartyDropEdit
			// 
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 73, true);
			this.PaymentPartyDropEdit.TabIndex = 2;
			// 
			// MiscOptionsGroupBox
			// 
			this.MiscOptionsGroupBox.Controls.Add(this.commentLabel);
			this.MiscOptionsGroupBox.Controls.Add(this.defaultFreightCalcEdit);
			this.MiscOptionsGroupBox.Controls.Add(this.anySightDepositAmountCalcEdit);
			this.MiscOptionsGroupBox.Controls.Add(this.cAMergeByDropEdit);
			this.MiscOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 212, true);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.cAMergeByDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.anySightDepositAmountCalcEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.defaultFreightCalcEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.commentLabel, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.PaymentPartyDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.MergeByDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BrokerCodeFindBox, 0);
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.TabIndex = 0;
			// 
			// MergeByDropEdit
			//
			this.MergeByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 99, true);
			this.MergeByDropEdit.TabIndex = 3;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDeclaration);
			//
			// BondGroupBox
			//
			this.bondGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|5AF68C0A-260F-4E1A-93F2-380074ABDFF2", "Bond");
			this.bondGroupBox.Controls.Add(bondNumberTextBox);
			this.bondGroupBox.Controls.Add(bondTypeDropEdit);
			this.bondGroupBox.Controls.Add(suretyCodeTextBox);
			this.bondGroupBox.Controls.Add(refreshBondButton);
			this.bondGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(762, 103, true);
			this.bondGroupBox.Name = "BondGroupBox";
			this.bondGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 117, true);
			this.bondGroupBox.TabIndex = 1;
			this.bondGroupBox.TabStop = false;
			//
			// BondTypeDropEdit
			//
			this.bondTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.bondTypeDropEdit, "CA_BondType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_BondType);
			this.bondTypeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("IIDUserControl|B9903C74-9DF5-43CB-979B-9B5CECAB2B4F", "Type");
			this.bondTypeDropEdit.ShowDescriptionBox = false;
			this.bondTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 20, true);
			this.bondTypeDropEdit.Name = "BondTypeDropEdit";
			this.bondTypeDropEdit.ReadOnly = true;
			this.bondTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.bondTypeDropEdit.TabIndex = 1;
			//
			// SuretyCodeTextBox
			//
			this.BindingSource.SetBindingMember(this.suretyCodeTextBox, "CA_SuretyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_SuretyCode);
			this.suretyCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 20, true);
			this.suretyCodeTextBox.Name = "SuretyCodeTextBox";
			this.suretyCodeTextBox.ReadOnly = true;
			this.suretyCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.suretyCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|58FDD6FD-2F35-4AEC-B827-DE2B7BA2047A", "Surety Code");
			this.suretyCodeTextBox.TabIndex = 2;
			//
			// BondNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.bondNumberTextBox, "CA_BondNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_BondNo);
			this.bondNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 46, true);
			this.bondNumberTextBox.Name = "BondNumberTextBox";
			this.bondNumberTextBox.ReadOnly = true;
			this.bondNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.bondNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|797DFB50-950E-469B-803F-7E955E69C74F", "Bond Number");
			this.bondNumberTextBox.TabIndex = 3;
			//
			// RefreshBondButton
			//
			this.refreshBondButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 72, true);
			this.refreshBondButton.Name = "RefreshBondButton";
			this.refreshBondButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.refreshBondButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 23, true);
			this.refreshBondButton.TabIndex = 6;
			this.refreshBondButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|6f3f46d5-067d-4a3d-a5ed-7ef43f801f1d", "Refresh Bond Details");
			this.refreshBondButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.refreshBondButton.ToolTipCaption = null;
			this.refreshBondButton.UseVisualStyleBackColor = true;
			this.refreshBondButton.Click += new EventHandler(this.RefreshBondButton_Click);
			//
			// PGAOptionsGroupBox
			// 
			this.pGAOptionsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|247f3e57-91b1-456e-a48c-a515201c6ea3", "PGA Options");
			this.pGAOptionsGroupBox.Controls.Add(this.aTDExCodeDropEdit);
			this.pGAOptionsGroupBox.Controls.Add(this.amendReasonCodeDropEdit);
			this.pGAOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(762, 8, true);
			this.pGAOptionsGroupBox.Name = "PGAOptionsGroupBox";
			this.pGAOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 81, true);
			this.pGAOptionsGroupBox.TabIndex = 1;
			this.pGAOptionsGroupBox.TabStop = false;
			// 
			// ATDExCodeDropEdit
			// 
			this.aTDExCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.aTDExCodeDropEdit, "CA_ATDExCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_ATDExCode);
			this.aTDExCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("IIDUserControl|ac6a0c89-b4ac-4523-a765-4a432ca494e9", "ATD Exemption Code");
			this.aTDExCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 20, true);
			this.aTDExCodeDropEdit.Name = "ATDExCodeDropEdit";
			this.aTDExCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.aTDExCodeDropEdit.TabIndex = 1;
			// 
			// AmendReasonCodeDropEdit
			// 
			this.amendReasonCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.amendReasonCodeDropEdit, "CA_AmendReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_AmendReasonCode);
			this.amendReasonCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("IIDUserControl|4a63ed03-cbc0-42c6-be32-8db93f8f9814", "Amendment Reason");
			this.amendReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 46, true);
			this.amendReasonCodeDropEdit.Name = "AmendReasonCodeDropEdit";
			this.amendReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.amendReasonCodeDropEdit.TabIndex = 2;
			// 
			// WoodPackagingIndCheckBox
			// 
			this.woodPackagingIndCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.woodPackagingIndCheckBox, "CA_WoodPackagingInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_WoodPackagingInd);
			this.woodPackagingIndCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|23cc84c1-3836-40f5-bbd2-471b09c64f14", "Wood Packaging", "Wood Packaging Ind.", "Wood Packaging Indicator");
			this.woodPackagingIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.woodPackagingIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 28, true);
			this.woodPackagingIndCheckBox.Name = "WoodPackagingIndCheckBox";
			this.woodPackagingIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.woodPackagingIndCheckBox.TabIndex = 6;
			this.woodPackagingIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// CAMergeByDropEdit
			// 
			this.cAMergeByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cAMergeByDropEdit, "CA_MergeBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_MergeBy);
			this.cAMergeByDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAMiscOptionsUserControl|75152d74-52eb-4d6d-8f2c-8600cfdf53c3", "Entry Merge By");
			this.cAMergeByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 125, true);
			this.cAMergeByDropEdit.Name = "CAMergeByDropEdit";
			this.cAMergeByDropEdit.PreBoundMaxLength = 3;
			this.cAMergeByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.cAMergeByDropEdit.TabIndex = 4;
			// 
			// PermitApplicationCheckBox
			// 
			this.permitApplicationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.permitApplicationCheckBox, "CA_PermitApplication");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_PermitApplication);
			this.permitApplicationCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|c5a64d33-b742-4d94-968b-1b99a4560cb8", "Permit Application");
			this.permitApplicationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.permitApplicationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 51, true);
			this.permitApplicationCheckBox.Name = "PermitApplicationCheckBox";
			this.permitApplicationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.permitApplicationCheckBox.TabIndex = 5;
			this.permitApplicationCheckBox.UseVisualStyleBackColor = true;
			// 
			// InspectionArrangementsCompleteCheckBox
			// 
			this.inspectionArrangementsCompleteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.inspectionArrangementsCompleteCheckBox, "CA_InspectionArrangementsComplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_InspectionArrangementsComplete);
			this.inspectionArrangementsCompleteCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|b0352ca0-7b26-4899-b100-4f510835bdc3", "Inspection Arrangements Complete");
			this.inspectionArrangementsCompleteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.inspectionArrangementsCompleteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 75, true);
			this.inspectionArrangementsCompleteCheckBox.Name = "InspectionArrangementsCompleteCheckBox";
			this.inspectionArrangementsCompleteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.inspectionArrangementsCompleteCheckBox.TabIndex = 6;
			this.inspectionArrangementsCompleteCheckBox.UseVisualStyleBackColor = true;
			// 
			// OGDNRCheckBox
			// 
			this.oGDNRCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.oGDNRCheckBox, "CA_OGDNR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_OGDNR);
			this.oGDNRCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|2a4a1cc2-05cc-4bbd-9a87-7c86b3c92e46", "NRCAN", "NRCAN (Natural Resource Canada)", "Natural Resource Canada.");
			this.oGDNRCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oGDNRCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 75, true);
			this.oGDNRCheckBox.Name = "OGDNRCheckBox";
			this.oGDNRCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.oGDNRCheckBox.TabIndex = 2;
			this.oGDNRCheckBox.UseVisualStyleBackColor = true;
			// 
			// OGDTCCheckBox
			// 
			this.oGDTCCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.oGDTCCheckBox, "CA_OGDTC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_OGDTC);
			this.oGDTCCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|f315e638-04ad-4b0a-a6f3-082a10f6760e", "TC", "Transport Canada (Tires)", "Transport Canada.");
			this.oGDTCCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oGDTCCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 101, true);
			this.oGDTCCheckBox.Name = "OGDTCCheckBox";
			this.oGDTCCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.oGDTCCheckBox.TabIndex = 3;
			this.oGDTCCheckBox.UseVisualStyleBackColor = true;
			// 
			// OGDICCheckBox
			// 
			this.oGDICCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.oGDICCheckBox, "CA_OGDIC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_OGDIC);
			this.oGDICCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|e6dd1c56-8fb3-40bc-86a1-68409134748e", "SITT", "SITT (Industry Canada)", "Industry Canada.");
			this.oGDICCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oGDICCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 51, true);
			this.oGDICCheckBox.Name = "OGDICCheckBox";
			this.oGDICCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.oGDICCheckBox.TabIndex = 1;
			this.oGDICCheckBox.UseVisualStyleBackColor = true;
			// 
			// OGDCFIACheckBox
			// 
			this.oGDCFIACheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.oGDCFIACheckBox, "CA_OGDCFIA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_OGDCFIA);
			this.oGDCFIACheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|bf4203ac-b24b-4d2e-af29-d0becfe463d4", "CFIA", "CFIA (Canadian Food Inspection Agency)", "Canadian Food Inspection Agency.");
			this.oGDCFIACheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oGDCFIACheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 28, true);
			this.oGDCFIACheckBox.Name = "OGDCFIACheckBox";
			this.oGDCFIACheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.oGDCFIACheckBox.TabIndex = 0;
			this.oGDCFIACheckBox.UseVisualStyleBackColor = true;
			// 
			// ImportDeclarationOptionsGroupBox
			// 
			this.importDeclarationOptionsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|a0ae77c5-0dcc-40f7-b6c9-f557f4bd66a8", "Import Declaration Options");
			this.importDeclarationOptionsGroupBox.Controls.Add(this.cSAEntryCheckBox);
			this.importDeclarationOptionsGroupBox.Controls.Add(this.woodPackagingIndCheckBox);
			this.importDeclarationOptionsGroupBox.Controls.Add(this.permitApplicationCheckBox);
			this.importDeclarationOptionsGroupBox.Controls.Add(this.inspectionArrangementsCompleteCheckBox);
			this.importDeclarationOptionsGroupBox.Controls.Add(this.oGDNRCheckBox);
			this.importDeclarationOptionsGroupBox.Controls.Add(this.oGDTCCheckBox);
			this.importDeclarationOptionsGroupBox.Controls.Add(this.oGDICCheckBox);
			this.importDeclarationOptionsGroupBox.Controls.Add(this.oGDCFIACheckBox);
			this.importDeclarationOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(403, 8, true);
			this.importDeclarationOptionsGroupBox.Name = "ImportDeclarationOptionsGroupBox";
			this.importDeclarationOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 212, true);
			this.importDeclarationOptionsGroupBox.TabIndex = 1;
			this.importDeclarationOptionsGroupBox.TabStop = false;
			// 
			// LPCOGroupBox
			// 
			this.lPCOGroupBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.lPCOGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|6CE2F14C-149E-48B5-B38E-98938CB60F9F", "LPCOs");
			this.lPCOGroupBox.Controls.Add(this.lpcoDetailUserControl1);
			this.lPCOGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.lPCOGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 226, true);
			this.lPCOGroupBox.Name = "LPCOGroupBox";
			this.lPCOGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 291, true);
			this.lPCOGroupBox.TabIndex = 2;
			this.lPCOGroupBox.TabStop = false;
			// 
			// lpcoDetailUserControl1
			// 
			this.lpcoDetailUserControl1.AllowDrop = true;
			this.lpcoDetailUserControl1.CaptionRenderingEnabled = true;
			this.BindingSource.SetBindingMember(this.lpcoDetailUserControl1, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).LPCOViews);
			this.lpcoDetailUserControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.lpcoDetailUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 136, true);
			this.lpcoDetailUserControl1.Name = "lpcoDetailUserControl1";
			this.lpcoDetailUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 152, true);
			this.lpcoDetailUserControl1.TabIndex = 1;
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.LPCOGridUserControl.CaptionRenderingEnabled = true;
			this.LPCOGridUserControl.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).LPCOViews);
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 114, true);
			this.LPCOGridUserControl.TabIndex = 0;
			// 
			// AnySightDepositAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.anySightDepositAmountCalcEdit, "CA_AnySightDepositAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CA_AnySightDepositAmount);
			this.anySightDepositAmountCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|1440513b-16a3-4a85-9367-33c53cedfdb7", "Deposit Amount");
			this.anySightDepositAmountCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.anySightDepositAmountCalcEdit.DecimalPlaces = 2;
			this.anySightDepositAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 154, true);
			this.anySightDepositAmountCalcEdit.Name = "AnySightDepositAmountCalcEdit";
			this.anySightDepositAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.anySightDepositAmountCalcEdit.TabIndex = 5;
			this.anySightDepositAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DefaultFreightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.defaultFreightCalcEdit, "CalculatedFreightAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).CalculatedFreightAmount);
			this.defaultFreightCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("MiscOptionsUserControl|92cfeb90-7fe4-460e-95d3-6f53cca8afa7", "Default Freight");
			this.defaultFreightCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.defaultFreightCalcEdit.DecimalPlaces = 2;
			this.defaultFreightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 183, true);
			this.defaultFreightCalcEdit.Name = "DefaultFreightCalcEdit";
			this.defaultFreightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.defaultFreightCalcEdit.TabIndex = 6;
			this.defaultFreightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CommentLabel
			// 
			this.commentLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right);
			this.commentLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("caab9467-fdbc-4d34-af65-9609c116bee4", "(For Entry Box 19 if freight charges not specified on invoices)");
			this.commentLabel.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.commentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 175, true);
			this.commentLabel.Name = "CommentLabel";
			this.commentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 35, true);
			this.commentLabel.TabIndex = 7;
			this.commentLabel.UseMnemonic = false;
			// 
			// CSAEntryCheckBox
			// 
			this.cSAEntryCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.cSAEntryCheckBox, "CA_CSAEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((JobDeclaration)(null)).CA_CSAEntry)));
			this.cSAEntryCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("24f08b5b-5c2e-48cb-97bf-ff7608a31d6f", "CSA Release Only");
			this.cSAEntryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cSAEntryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 101, true);
			this.cSAEntryCheckBox.Name = "CSAEntryCheckBox";
			this.cSAEntryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 17, true);
			this.cSAEntryCheckBox.TabIndex = 7;
			this.cSAEntryCheckBox.UseVisualStyleBackColor = true;
			// 
			// CAMiscOptionsUserControl
			// 
			this.Controls.Add(this.importDeclarationOptionsGroupBox);
			this.Controls.Add(this.lPCOGroupBox);
			this.Controls.Add(this.pGAOptionsGroupBox);
			this.Controls.Add(this.bondGroupBox);
			this.Name = "CAMiscOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1144, 520, true);
			this.Controls.SetChildIndex(this.pGAOptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.lPCOGroupBox, 0);
			this.Controls.SetChildIndex(this.importDeclarationOptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.MiscOptionsGroupBox, 0);
			this.PaymentPartyDropEdit.ResumeLayout(true);
			this.PaymentPartyDropEdit.PerformLayout();
			this.MiscOptionsGroupBox.ResumeLayout(false);
			this.MiscOptionsGroupBox.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.MergeByDropEdit.ResumeLayout(true);
			this.MergeByDropEdit.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.pGAOptionsGroupBox.ResumeLayout(false);
			this.pGAOptionsGroupBox.PerformLayout();
			this.bondGroupBox.ResumeLayout(false);
			this.bondGroupBox.PerformLayout();
			this.aTDExCodeDropEdit.ResumeLayout(true);
			this.aTDExCodeDropEdit.PerformLayout();
			this.amendReasonCodeDropEdit.ResumeLayout(true);
			this.amendReasonCodeDropEdit.PerformLayout();
			this.bondTypeDropEdit.ResumeLayout(true);
			this.bondTypeDropEdit.PerformLayout();
			this.cAMergeByDropEdit.ResumeLayout(true);
			this.cAMergeByDropEdit.PerformLayout();
			this.importDeclarationOptionsGroupBox.ResumeLayout(false);
			this.importDeclarationOptionsGroupBox.PerformLayout();
			this.lPCOGroupBox.ResumeLayout(false);
			this.lPCOGroupBox.PerformLayout();
			this.lpcoDetailUserControl1.ResumeLayout(true);
			this.lpcoDetailUserControl1.PerformLayout();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.bondNumberTextBox.ResumeLayout(true);
			this.bondNumberTextBox.PerformLayout();
			this.suretyCodeTextBox.ResumeLayout(true);
			this.suretyCodeTextBox.PerformLayout();
			this.refreshBondButton.ResumeLayout(true);
			this.refreshBondButton.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal LPCOGridUserControl LPCOGridUserControl;
		ZArchitecture.GUI.ZCheckBox oGDNRCheckBox;
		ZArchitecture.GUI.ZCheckBox oGDTCCheckBox;
		ZArchitecture.GUI.ZCheckBox oGDICCheckBox;
		ZArchitecture.GUI.ZCheckBox oGDCFIACheckBox;
		ZArchitecture.GUI.ZCheckBox woodPackagingIndCheckBox;
		ZArchitecture.GUI.ZCheckBox permitApplicationCheckBox;
		ZArchitecture.GUI.ZCheckBox inspectionArrangementsCompleteCheckBox;
		ZArchitecture.GUI.ZDropEdit cAMergeByDropEdit;
		ZArchitecture.ZCalcEdit anySightDepositAmountCalcEdit;
		ZArchitecture.ZCalcEdit defaultFreightCalcEdit;
		ZArchitecture.ZLabel commentLabel;
		ZArchitecture.GUI.ZGroupBox importDeclarationOptionsGroupBox;
		ZArchitecture.GUI.ZDropEdit amendReasonCodeDropEdit;
		ZArchitecture.GUI.ZDropEdit aTDExCodeDropEdit;
		ZArchitecture.GUI.ZGroupBox lPCOGroupBox;
		LPCODetailUserControl lpcoDetailUserControl1;
		ZArchitecture.GUI.ZCheckBox cSAEntryCheckBox;
		ZArchitecture.GUI.ZGroupBox pGAOptionsGroupBox;
		ZArchitecture.GUI.ZGroupBox bondGroupBox;
		ZArchitecture.GUI.ZDropEdit bondTypeDropEdit;
		ZArchitecture.ZTextBox bondNumberTextBox;
		ZArchitecture.ZTextBox suretyCodeTextBox;
		ZArchitecture.GUI.ZButton refreshBondButton;
	}
}
