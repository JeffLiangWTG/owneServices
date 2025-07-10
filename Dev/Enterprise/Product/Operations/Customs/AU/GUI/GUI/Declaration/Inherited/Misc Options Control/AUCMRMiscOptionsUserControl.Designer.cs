using System.ComponentModel;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUCMRMiscOptionsUserControl
	{
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AUCMRMiscOptionsUserControl));
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.FirstPaidUnderProtestID = new Enterprise.ZArchitecture.ZTextBox();
			this.UnaccompaniedPersonalEffectsID = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsReceiptForGoodsIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarationIndicators = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SOFACheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UnaccompaniedPersonalEffectsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.reCalculateEffectiveDutDateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.jE_VisialExaminationApplicationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExternalDeclaration = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.externalDecDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.aQISInspectionLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AmberStatementTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HeaderAmberReasonTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.HeaderAmberReasonTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.paidUnderProtestTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AQISConcernTypes = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.aQISConcernType = new Enterprise.ZArchitecture.ZGrid();
			this.AQISInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.aQISCommodityCodeControl = new Enterprise.Customs.AU.Declaration.GUI.AQISCommodityCodeControl();
			this.aQISPermitNumberControl = new Enterprise.Customs.AU.Declaration.GUI.AQISPermitNumberControl();
			this.aQISEntityIDControl = new Enterprise.Customs.AU.Declaration.GUI.AQISEntityIdControl();
			this.aQISProducerCodeControl = new Enterprise.Customs.AU.Declaration.GUI.AQISProducerCodeControl();
			this.AQISPremisesIdAndPackagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.aQISPremisesIdProcessingTypeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AQISDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.aQISDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AQISInspectionLocationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PeriodicSettlementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.settlementPeriodEndDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.settlementPeriodStartDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.NilReturnCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SettlementTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LandedCostingDefaultsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.JE_LandedCostByCostBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.JE_LandedCostByUnitsBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.JE_LandedCostByVolumeBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.JE_LandedCostByWeightBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ForceManualTILVCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.miscOptionsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.miscOptionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.uPETabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.uPEDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.spousePassportCountryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.importerPassportCountryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.importerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.numberOfChildrenCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.spousePassportNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.spousePassportNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.spouseLabel = new Enterprise.ZArchitecture.ZLabel();
			this.importerPassportSexDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.importerPassportDateOfBirthDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.importerPassportNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PaymentPartyDropEdit.SuspendLayout();
			this.MiscOptionsGroupBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.MergeByDropEdit.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeclarationIndicators.SuspendLayout();
			this.ExternalDeclaration.SuspendLayout();
			this.externalDecDate.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.HeaderAmberReasonTypeCodeFindBox.SuspendLayout();
			this.HeaderAmberReasonTypeDropEdit.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.AQISConcernTypes.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.aQISConcernType)).BeginInit();
			this.aQISConcernType.SuspendLayout();
			this.AQISInformationGroupBox.SuspendLayout();
			this.aQISCommodityCodeControl.SuspendLayout();
			this.aQISPermitNumberControl.SuspendLayout();
			this.aQISEntityIDControl.SuspendLayout();
			this.aQISProducerCodeControl.SuspendLayout();
			this.AQISPremisesIdAndPackagesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.aQISPremisesIdProcessingTypeGrid)).BeginInit();
			this.aQISPremisesIdProcessingTypeGrid.SuspendLayout();
			this.AQISDocumentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.aQISDocumentGrid)).BeginInit();
			this.aQISDocumentGrid.SuspendLayout();
			this.AQISInspectionLocationGroupBox.SuspendLayout();
			this.PeriodicSettlementGroupBox.SuspendLayout();
			this.settlementPeriodEndDate.SuspendLayout();
			this.settlementPeriodStartDate.SuspendLayout();
			this.LandedCostingDefaultsGroupBox.SuspendLayout();
			this.miscOptionsTabControl.SuspendLayout();
			this.miscOptionsTabPage.SuspendLayout();
			this.uPETabPage.SuspendLayout();
			this.uPEDetailsGroupBox.SuspendLayout();
			this.spousePassportCountryDropEdit.SuspendLayout();
			this.importerPassportCountryDropEdit.SuspendLayout();
			this.importerPassportSexDropEdit.SuspendLayout();
			this.importerPassportDateOfBirthDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// PaymentPartyDropEdit
			// 
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 88, true);
			this.PaymentPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 17, true);
			// 
			// MiscOptionsGroupBox
			// 
			this.MiscOptionsGroupBox.Controls.Add(this.ForceManualTILVCheckBox);
			this.MiscOptionsGroupBox.Controls.Add(this.CustomsReceiptForGoodsIdTextBox);
			this.MiscOptionsGroupBox.Controls.Add(this.UnaccompaniedPersonalEffectsID);
			this.MiscOptionsGroupBox.Controls.Add(this.FirstPaidUnderProtestID);
			this.MiscOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.MiscOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 137, true);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BrokerCodeFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.PaymentPartyDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.MergeByDropEdit, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.FirstPaidUnderProtestID, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.UnaccompaniedPersonalEffectsID, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.CustomsReceiptForGoodsIdTextBox, 0);
			this.MiscOptionsGroupBox.Controls.SetChildIndex(this.ForceManualTILVCheckBox, 0);
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 16, true);
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 17, true);
			// 
			// MergeByDropEdit
			// 
			this.MergeByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 64, true);
			this.MergeByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 17, true);
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 40, true);
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 17, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobDeclaration);
			// 
			// FirstPaidUnderProtestID
			// 
			this.BindingSource.SetBindingMember(this.FirstPaidUnderProtestID, "AddInfo+ZA_FPUP_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_FPUP_Hidden)));
			this.FirstPaidUnderProtestID.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|6afe73c3-d436-4e4c-b06e-dd9fb64a773c", "First PUP Dec", "First PUP Declaration ID", "First PUP Declaration ID", "If a series of import declarations are paid under protest for the same reason/s, this is the identifier required to nominate the first declaration ID in the series for this Protest.");
			this.FirstPaidUnderProtestID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 40, true);
			this.FirstPaidUnderProtestID.Name = "FirstPaidUnderProtestID";
			this.FirstPaidUnderProtestID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 17, true);
			this.FirstPaidUnderProtestID.TabIndex = 11;
			// 
			// UnaccompaniedPersonalEffectsID
			// 
			this.BindingSource.SetBindingMember(this.UnaccompaniedPersonalEffectsID, "AddInfo+ZA_UPE_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_UPE_Hidden)));
			this.UnaccompaniedPersonalEffectsID.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|1ebc3570-5cf8-42cd-bc6d-464ad90cfd06", "UPE Dec", "PE\'s Declaration ID", "PE\'s Declaration ID", "The ID Number of a Customs Unaccompanied Personal Effects Document which represents a declaration to Customs by a Party concerning Goods that may cross the border.");
			this.UnaccompaniedPersonalEffectsID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 16, true);
			this.UnaccompaniedPersonalEffectsID.Name = "UnaccompaniedPersonalEffectsID";
			this.UnaccompaniedPersonalEffectsID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 17, true);
			this.UnaccompaniedPersonalEffectsID.TabIndex = 9;
			// 
			// CustomsReceiptForGoodsIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsReceiptForGoodsIdTextBox, "AddInfo+ZA_CustomsReceipt_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_CustomsReceipt_Hidden)));
			this.CustomsReceiptForGoodsIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 64, true);
			this.CustomsReceiptForGoodsIdTextBox.Name = "CustomsReceiptForGoodsIdTextBox";
			this.CustomsReceiptForGoodsIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 17, true);
			this.CustomsReceiptForGoodsIdTextBox.TabIndex = 13;
			// 
			// DeclarationIndicators
			// 
			this.DeclarationIndicators.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationIndicators.Controls.Add(this.SOFACheckBox);
			this.DeclarationIndicators.Controls.Add(this.UnaccompaniedPersonalEffectsCheckBox);
			this.DeclarationIndicators.Controls.Add(this.reCalculateEffectiveDutDateCheckBox);
			this.DeclarationIndicators.Controls.Add(this.jE_VisialExaminationApplicationCheckBox);
			this.DeclarationIndicators.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 6, true);
			this.DeclarationIndicators.Name = "DeclarationIndicators";
			this.DeclarationIndicators.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 85, true);
			this.DeclarationIndicators.TabIndex = 1;
			this.DeclarationIndicators.TabStop = false;
			this.DeclarationIndicators.Text = "Declaration Indicators";
			// 
			// SOFACheckBox
			// 
			this.BindingSource.SetBindingMember(this.SOFACheckBox, "AddInfo+ZA_SOFAIndicator_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_SOFAIndicator_Hidden)));
			this.SOFACheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SOFACheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.SOFACheckBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.SOFACheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 15, true);
			this.SOFACheckBox.Name = "SOFACheckBox";
			this.SOFACheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.SOFACheckBox.TabIndex = 3;
			this.SOFACheckBox.Text = "Status of Forces Agreement (SOFA)";
			// 
			// UnaccompaniedPersonalEffectsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UnaccompaniedPersonalEffectsCheckBox, "AddInfo+ZA_UPEIndicator_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_UPEIndicator_Hidden)));
			this.UnaccompaniedPersonalEffectsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.UnaccompaniedPersonalEffectsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.UnaccompaniedPersonalEffectsCheckBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.UnaccompaniedPersonalEffectsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 59, true);
			this.UnaccompaniedPersonalEffectsCheckBox.Name = "UnaccompaniedPersonalEffectsCheckBox";
			this.UnaccompaniedPersonalEffectsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.UnaccompaniedPersonalEffectsCheckBox.TabIndex = 2;
			this.UnaccompaniedPersonalEffectsCheckBox.Text = "Unaccompanied Personal Effects";
			// 
			// ReCalculateEffectiveDutDateCheckBox
			// 
			this.BindingSource.SetBindingMember(this.reCalculateEffectiveDutDateCheckBox, "AddInfo+ZA_EffectDutyDate_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_EffectDutyDate_Hidden)));
			this.reCalculateEffectiveDutDateCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.reCalculateEffectiveDutDateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.reCalculateEffectiveDutDateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 36, true);
			this.reCalculateEffectiveDutDateCheckBox.Name = "ReCalculateEffectiveDutDateCheckBox";
			this.reCalculateEffectiveDutDateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.reCalculateEffectiveDutDateCheckBox.TabIndex = 1;
			this.reCalculateEffectiveDutDateCheckBox.Text = "Re-Calculate Effective Duty Date";
			// 
			// JE_VisialExaminationApplicationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.jE_VisialExaminationApplicationCheckBox, "AddInfo+ZA_VIS_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_VIS_Hidden)));
			this.jE_VisialExaminationApplicationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.jE_VisialExaminationApplicationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.jE_VisialExaminationApplicationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.jE_VisialExaminationApplicationCheckBox.Name = "JE_VisialExaminationApplicationCheckBox";
			this.jE_VisialExaminationApplicationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.jE_VisialExaminationApplicationCheckBox.TabIndex = 0;
			this.jE_VisialExaminationApplicationCheckBox.Text = "Visual Examination Application";
			// 
			// ExternalDeclaration
			// 
			this.ExternalDeclaration.Controls.Add(this.externalDecDate);
			this.ExternalDeclaration.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 99, true);
			this.ExternalDeclaration.Name = "ExternalDeclaration";
			this.ExternalDeclaration.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 41, true);
			this.ExternalDeclaration.TabIndex = 10;
			this.ExternalDeclaration.TabStop = false;
			this.ExternalDeclaration.Text = "External Declaration";
			// 
			// ExternalDecDate
			// 
			this.externalDecDate.AllowDrop = true;
			this.externalDecDate.AutoCompleteMonthThreshold = 1;
			this.externalDecDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.externalDecDate, "ManualClearanceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).ManualClearanceDate)));
			this.externalDecDate.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|c1dbd9b1-fdce-4d45-ae95-d0e76b38ace5", "Declaration Date", "The Declaration Date for an External Declaration.");
			this.externalDecDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 17, true);
			this.externalDecDate.Name = "ExternalDecDate";
			this.externalDecDate.TabIndex = 21;
			// 
			// AQISInspectionLocationTextBox
			// 
			this.aQISInspectionLocationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.aQISInspectionLocationTextBox, "AddInfo+ZA_AQISInspectLocation_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_AQISInspectLocation_Hidden)));
			this.aQISInspectionLocationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.aQISInspectionLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.aQISInspectionLocationTextBox.Multiline = true;
			this.aQISInspectionLocationTextBox.Name = "AQISInspectionLocationTextBox";
			this.aQISInspectionLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 101, true);
			this.aQISInspectionLocationTextBox.TabIndex = 0;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.zGroupBox2.Controls.Add(this.AmberStatementTextBox);
			this.zGroupBox2.Controls.Add(this.HeaderAmberReasonTypeCodeFindBox);
			this.zGroupBox2.Controls.Add(this.HeaderAmberReasonTypeDropEdit);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 149, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 242, true);
			this.zGroupBox2.TabIndex = 2;
			this.zGroupBox2.TabStop = false;
			this.zGroupBox2.Text = "Amber";
			// 
			// AmberStatementTextBox
			// 
			this.AmberStatementTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AmberStatementTextBox, "JE_AmberStatement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_AmberStatement)));
			this.AmberStatementTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|aceccf1b-2eea-485b-9adf-7ed9738dcd0d", "Statement", "The Amber Statement explains the reason/s why the Broker or Importer has nominated the declaration for amber line processing with Customs.");
			this.AmberStatementTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AmberStatementTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 40, true);
			this.AmberStatementTextBox.Multiline = true;
			this.AmberStatementTextBox.Name = "AmberStatementTextBox";
			this.AmberStatementTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.AmberStatementTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 198, true);
			this.AmberStatementTextBox.TabIndex = 1;
			// 
			// HeaderAmberReasonTypeCodeFindBox
			// 
			this.HeaderAmberReasonTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HeaderAmberReasonTypeCodeFindBox, "AddInfo+ZA_HART_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_HART_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.Lookups.ZA_HART_List)));
			this.HeaderAmberReasonTypeCodeFindBox.BindToList = "AddInfo+Lookups+ZA_HART_List";
			this.HeaderAmberReasonTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
			this.HeaderAmberReasonTypeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.CMRCodeLists;
			this.HeaderAmberReasonTypeCodeFindBox.Name = "HeaderAmberReasonTypeCodeFindBox";
			this.HeaderAmberReasonTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 17, true);
			this.HeaderAmberReasonTypeCodeFindBox.TabIndex = 0;
			// 
			// HeaderAmberReasonTypeDropEdit
			// 
			this.HeaderAmberReasonTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HeaderAmberReasonTypeDropEdit, "AddInfo+ZA_DARC_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_DARC_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.Lookups.DrawbackAmberCodeList)));
			this.HeaderAmberReasonTypeDropEdit.BindToList = "AddInfo+Lookups+DrawbackAmberCodeList";
			this.HeaderAmberReasonTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 66, true);
			this.HeaderAmberReasonTypeDropEdit.Name = "HeaderAmberReasonTypeDropEdit";
			this.HeaderAmberReasonTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 17, true);
			this.HeaderAmberReasonTypeDropEdit.TabIndex = 0;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox3.Controls.Add(this.paidUnderProtestTextBox);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(509, 149, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 242, true);
			this.zGroupBox3.TabIndex = 3;
			this.zGroupBox3.TabStop = false;
			this.zGroupBox3.Text = "Paid Under Protest Statement";
			// 
			// PaidUnderProtestTextBox
			// 
			this.paidUnderProtestTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.paidUnderProtestTextBox, "JE_PaidUnderProtestStatement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_PaidUnderProtestStatement)));
			this.paidUnderProtestTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|f120ca8c-a34d-47ab-98b6-7f2c0f492bdb", "", "A description of the reason why one or more of the lines on the declaration has been nominated as being paid under protest");
			this.paidUnderProtestTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.paidUnderProtestTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.paidUnderProtestTextBox.Multiline = true;
			this.paidUnderProtestTextBox.Name = "PaidUnderProtestTextBox";
			this.paidUnderProtestTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.paidUnderProtestTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(657, 223, true);
			this.paidUnderProtestTextBox.TabIndex = 1;
			// 
			// AQISConcernTypes
			// 
			this.AQISConcernTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AQISConcernTypes.Controls.Add(this.aQISConcernType);
			this.AQISConcernTypes.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 420, true);
			this.AQISConcernTypes.Name = "AQISConcernTypes";
			this.AQISConcernTypes.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 120, true);
			this.AQISConcernTypes.TabIndex = 4;
			this.AQISConcernTypes.TabStop = false;
			this.AQISConcernTypes.Text = "Quarantine Concern Types";
			// 
			// aQISConcernType
			// 
			this.aQISConcernType.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.aQISConcernType, "AQISConcernTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AQISConcernTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AQISConcernType)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AQISConcernTypes)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AQISConcernType)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AQISConcernTypes)).SyncRoot)).Lookups.AQISConcernTypeList)));
			this.aQISConcernType.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups+AQISConcernTypeList";
			zDropEditColumnStyleInfo1.Caption = "Concern Type";
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.ToolTip = resources.GetString("zDropEditColumnStyleInfo1.ToolTip");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.aQISConcernType.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.aQISConcernType.CopySelectedRowsAllowed = true;
			this.aQISConcernType.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aQISConcernType.GridId = "608c3388-e871-4cbb-9552-9b5afe4c1035";
			this.aQISConcernType.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.aQISConcernType.LayoutKey = "zGrid1";
			this.aQISConcernType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.aQISConcernType.Name = "aQISConcernType";
			this.aQISConcernType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 103, true);
			this.aQISConcernType.TabIndex = 0;
			// 
			// AQISInformationGroupBox
			// 
			this.AQISInformationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AQISInformationGroupBox.Controls.Add(this.aQISCommodityCodeControl);
			this.AQISInformationGroupBox.Controls.Add(this.aQISPermitNumberControl);
			this.AQISInformationGroupBox.Controls.Add(this.aQISEntityIDControl);
			this.AQISInformationGroupBox.Controls.Add(this.aQISProducerCodeControl);
			this.AQISInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 421, true);
			this.AQISInformationGroupBox.Name = "AQISInformationGroupBox";
			this.AQISInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 120, true);
			this.AQISInformationGroupBox.TabIndex = 5;
			this.AQISInformationGroupBox.TabStop = false;
			this.AQISInformationGroupBox.Text = "Quarantine Information";
			// 
			// AQISCommodityCodeControl
			// 
			this.aQISCommodityCodeControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.aQISCommodityCodeControl, "AddInfo+ZA_AQISCommCodes_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_AQISCommCodes_Hidden)));
			this.aQISCommodityCodeControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|99bd141b-e138-4bf4-bbce-4d42f04e1dd7", "Commodity Code", "Quarantine Commodity Code", "Linked to the ACS tariff. Provides a more detailed breakdown of goods within a classification to enable commodities to be further identified for Quarantine profiling purposes. They are stored against the Tariff Classification - Statistical Classification Combination.");
			this.aQISCommodityCodeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 92, true);
			this.aQISCommodityCodeControl.Name = "AQISCommodityCodeControl";
			this.aQISCommodityCodeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.aQISCommodityCodeControl.TabIndex = 3;
			// 
			// AQISPermitNumberControl
			// 
			this.aQISPermitNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.aQISPermitNumberControl, "AddInfo+ZA_AQISPermitIds_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_AQISPermitIds_Hidden)));
			this.aQISPermitNumberControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|947f4025-6feb-4d8c-950e-861942c7a80b", "Permit Number", "Quarantine Permit Number", "The number of a permit issued by Quarantine that authorizes the importation of certain commodities that are subject to Quarantine controls or restrictions.");
			this.aQISPermitNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 20, true);
			this.aQISPermitNumberControl.Name = "AQISPermitNumberControl";
			this.aQISPermitNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.aQISPermitNumberControl.TabIndex = 0;
			// 
			// AQISEntityIDControl
			// 
			this.aQISEntityIDControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.aQISEntityIDControl, "AddInfo+ZA_AQISEntityIds_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_AQISEntityIds_Hidden)));
			this.aQISEntityIDControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|bf3d4370-32e0-4ff0-93b9-0a0f685965d4", "Entity ID", "Quarantine Entity Identifier", "An identifier of some business objects for significance within the Quarantine system. This is a catchall attribute that allow the importer to quote additional information that may interest Quarantine. Example: Overseas treatment Provider Number, Quarantine Vessel Identifier.");
			this.aQISEntityIDControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 68, true);
			this.aQISEntityIDControl.Name = "AQISEntityIDControl";
			this.aQISEntityIDControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.aQISEntityIDControl.TabIndex = 2;
			// 
			// AQISProducerCodeControl
			// 
			this.aQISProducerCodeControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.aQISProducerCodeControl, "AddInfo+ZA_AQISProducerCodes_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_AQISProducerCodes_Hidden)));
			this.aQISProducerCodeControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|aa427d8e-55a2-4d1a-9a5b-a04cccf53a51", "Producer Code", "Quarantine Producer Code", "Relates to food shipments. A requirement of Quarantine IFP scheme, the producer code indicates who actually manufactured the product, not who supplied it. The broker nominates a producer code for a line of food when required by certain Quarantine profiles.");
			this.aQISProducerCodeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 44, true);
			this.aQISProducerCodeControl.Name = "AQISProducerCodeControl";
			this.aQISProducerCodeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.aQISProducerCodeControl.TabIndex = 1;
			// 
			// AQISPremisesIdAndPackagesGroupBox
			// 
			this.AQISPremisesIdAndPackagesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AQISPremisesIdAndPackagesGroupBox.Controls.Add(this.aQISPremisesIdProcessingTypeGrid);
			this.AQISPremisesIdAndPackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 546, true);
			this.AQISPremisesIdAndPackagesGroupBox.Name = "AQISPremisesIdAndPackagesGroupBox";
			this.AQISPremisesIdAndPackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 117, true);
			this.AQISPremisesIdAndPackagesGroupBox.TabIndex = 8;
			this.AQISPremisesIdAndPackagesGroupBox.TabStop = false;
			this.AQISPremisesIdAndPackagesGroupBox.Text = "Quarantine Premises Id and AEP Processing Type";
			// 
			// AQISPremisesIdProcessingTypeGrid
			// 
			this.aQISPremisesIdProcessingTypeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.aQISPremisesIdProcessingTypeGrid, "AQISPremisesIdAndProcessingTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AQISPremisesIdAndProcessingTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AQISPremisesIdAndProcessingType)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AQISPremisesIdAndProcessingTypes)).SyncRoot)).PremisesId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AQISPremisesIdAndProcessingType)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AQISPremisesIdAndProcessingTypes)).SyncRoot)).Lookups.AQISPremisesIdList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AQISPremisesIdAndProcessingType)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AQISPremisesIdAndProcessingTypes)).SyncRoot)).ProcessingType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AQISPremisesIdAndProcessingType)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AQISPremisesIdAndProcessingTypes)).SyncRoot)).Lookups.AQISProcessingTypeList)));
			this.aQISPremisesIdProcessingTypeGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups+AQISPremisesIdList";
			zCodeFindBoxColumnStyleInfo1.Caption = "Premises Id";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PremisesId";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ToolTip = resources.GetString("zCodeFindBoxColumnStyleInfo1.ToolTip");
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.BindToList = "Lookups+AQISProcessingTypeList";
			zDropEditColumnStyleInfo2.Caption = "AEP Processing Type";
			zDropEditColumnStyleInfo2.ColumnName = "ProcessingType";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.ToolTip = resources.GetString("zDropEditColumnStyleInfo2.ToolTip");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			this.aQISPremisesIdProcessingTypeGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.aQISPremisesIdProcessingTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.aQISPremisesIdProcessingTypeGrid.CopySelectedRowsAllowed = true;
			this.aQISPremisesIdProcessingTypeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aQISPremisesIdProcessingTypeGrid.GridId = "6d2d35f5-ca1e-41ac-a686-53f1c01ccf14";
			this.aQISPremisesIdProcessingTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.aQISPremisesIdProcessingTypeGrid.LayoutKey = "AQISPremisesIdProcessingTypeGrid";
			this.aQISPremisesIdProcessingTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.aQISPremisesIdProcessingTypeGrid.Name = "AQISPremisesIdProcessingTypeGrid";
			this.aQISPremisesIdProcessingTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 100, true);
			this.aQISPremisesIdProcessingTypeGrid.TabIndex = 0;
			// 
			// AQISDocumentGroupBox
			// 
			this.AQISDocumentGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AQISDocumentGroupBox.Controls.Add(this.aQISDocumentGrid);
			this.AQISDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 546, true);
			this.AQISDocumentGroupBox.Name = "AQISDocumentGroupBox";
			this.AQISDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 117, true);
			this.AQISDocumentGroupBox.TabIndex = 7;
			this.AQISDocumentGroupBox.TabStop = false;
			this.AQISDocumentGroupBox.Text = "Quarantine Document";
			// 
			// AQISDocumentGrid
			// 
			this.aQISDocumentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.aQISDocumentGrid, "AQISDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AQISDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AQISDocument)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AQISDocuments)).SyncRoot)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AQISDocument)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AQISDocuments)).SyncRoot)).Lookups.AQISDocumentTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AQISDocument)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AQISDocuments)).SyncRoot)).Number)));
			this.aQISDocumentGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.BindToList = "Lookups+AQISDocumentTypeList";
			zDropEditColumnStyleInfo3.Caption = "Type";
			zDropEditColumnStyleInfo3.ColumnName = "Type";
			zDropEditColumnStyleInfo3.IsMandatory = true;
			zDropEditColumnStyleInfo3.ToolTip = resources.GetString("zDropEditColumnStyleInfo3.ToolTip");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.Caption = "Number";
			zTextBoxColumnStyleInfo1.ColumnName = "Number";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.ToolTip = resources.GetString("zTextBoxColumnStyleInfo1.ToolTip");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.aQISDocumentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.aQISDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.aQISDocumentGrid.CopySelectedRowsAllowed = true;
			this.aQISDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aQISDocumentGrid.GridId = "2ed5c375-06f0-4d66-a7c3-074fa5cbd028";
			this.aQISDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.aQISDocumentGrid.LayoutKey = "AQISDocumentGrid";
			this.aQISDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.aQISDocumentGrid.Name = "AQISDocumentGrid";
			this.aQISDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 100, true);
			this.aQISDocumentGrid.TabIndex = 0;
			// 
			// AQISInspectionLocationGroupBox
			// 
			this.AQISInspectionLocationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AQISInspectionLocationGroupBox.Controls.Add(this.aQISInspectionLocationTextBox);
			this.AQISInspectionLocationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 420, true);
			this.AQISInspectionLocationGroupBox.Name = "AQISInspectionLocationGroupBox";
			this.AQISInspectionLocationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 120, true);
			this.AQISInspectionLocationGroupBox.TabIndex = 6;
			this.AQISInspectionLocationGroupBox.TabStop = false;
			this.AQISInspectionLocationGroupBox.Text = "Quarantine Inspection Location";
			// 
			// PeriodicSettlementGroupBox
			// 
			this.PeriodicSettlementGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PeriodicSettlementGroupBox.Controls.Add(this.settlementPeriodEndDate);
			this.PeriodicSettlementGroupBox.Controls.Add(this.settlementPeriodStartDate);
			this.PeriodicSettlementGroupBox.Controls.Add(this.NilReturnCheckBox);
			this.PeriodicSettlementGroupBox.Controls.Add(this.SettlementTypeDropEdit);
			this.PeriodicSettlementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 420, true);
			this.PeriodicSettlementGroupBox.Name = "PeriodicSettlementGroupBox";
			this.PeriodicSettlementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 120, true);
			this.PeriodicSettlementGroupBox.TabIndex = 4;
			this.PeriodicSettlementGroupBox.TabStop = false;
			this.PeriodicSettlementGroupBox.Text = "Periodic Settlement Details";
			// 
			// SettlementPeriodEndDate
			// 
			this.settlementPeriodEndDate.AllowDrop = true;
			this.settlementPeriodEndDate.AutoCompleteMonthThreshold = 1;
			this.settlementPeriodEndDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.settlementPeriodEndDate, "JE_SettlementPeriodEndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_SettlementPeriodEndDate)));
			this.settlementPeriodEndDate.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|d2c78ee3-7d0c-4c8c-b6ef-bda2c5628e03", "End Date", "Settlement End Date", "Settlement Period End Date", "When goods are cleared on N.30 Import Declarations and payment is made using Periodic Settlement arrangements, it indicates the last day of the settlement period. Required when the Settlement Type has been input.");
			this.settlementPeriodEndDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 93, true);
			this.settlementPeriodEndDate.Name = "SettlementPeriodEndDate";
			this.settlementPeriodEndDate.TabIndex = 24;
			// 
			// SettlementPeriodStartDate
			// 
			this.settlementPeriodStartDate.AllowDrop = true;
			this.settlementPeriodStartDate.AutoCompleteMonthThreshold = 1;
			this.settlementPeriodStartDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.settlementPeriodStartDate, "SettlementPeriodStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).SettlementPeriodStartDate)));
			this.settlementPeriodStartDate.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|1af25ebf-6a97-4396-ab68-db8a304dfd4a", "Start Date", "Settlement Start Date", "Settlement Period Start Date", "When goods are cleared on N.30 Import Declarations and payment is made using Periodic Settlement arrangements, it indicates the first day of the settlement period. Required when the Settlement Type has been input.");
			this.settlementPeriodStartDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 69, true);
			this.settlementPeriodStartDate.Name = "SettlementPeriodStartDate";
			this.settlementPeriodStartDate.TabIndex = 22;
			// 
			// NilReturnCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NilReturnCheckBox, "NilReturnInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).NilReturnInd)));
			this.NilReturnCheckBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|f9380f16-6ca5-4493-9fe7-bd1b352e026f", "Nil Return Indicator", "Nil Return Indicator", "Indicates that for the Importer, Warehouse and Period specified, that nothing was delivered from the warehouse into home consumption. Only allowed for Declarations where the Settlement Type has been input.");
			this.NilReturnCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.NilReturnCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NilReturnCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 19, true);
			this.NilReturnCheckBox.Name = "NilReturnCheckBox";
			this.NilReturnCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 24, true);
			this.NilReturnCheckBox.TabIndex = 15;
			this.NilReturnCheckBox.Text = "Nil Return Indicator";
			// 
			// SettlementTypeDropEdit
			// 
			this.SettlementTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SettlementTypeDropEdit, "JE_SettlementPeriodType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_SettlementPeriodType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.Lookups.SettlementPeriodTypeList)));
			this.SettlementTypeDropEdit.BindToList = "AddInfo+Lookups+SettlementPeriodTypeList";
			this.SettlementTypeDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|A2213664-FB97-4F5C-925E-779F47D48FEE", "Settlement Type", "Settlement Type", "When like customable goods are cleared on N.30 Import Declarations and payment is made using Periodic Settlement arrangements, the Settlement Type must be input. Settlement Start Date and Settlement End Dates are required when this field is used.");
			this.SettlementTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 45, true);
			this.SettlementTypeDropEdit.Name = "SettlementTypeDropEdit";
			this.SettlementTypeDropEdit.PreBoundMaxLength = 3;
			this.SettlementTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.SettlementTypeDropEdit.TabIndex = 16;
			// 
			// LandedCostingDefaultsGroupBox
			// 
			this.LandedCostingDefaultsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.LandedCostingDefaultsGroupBox.Controls.Add(this.zLabel10);
			this.LandedCostingDefaultsGroupBox.Controls.Add(this.JE_LandedCostByCostBoundCalcEdit);
			this.LandedCostingDefaultsGroupBox.Controls.Add(this.zLabel8);
			this.LandedCostingDefaultsGroupBox.Controls.Add(this.JE_LandedCostByUnitsBoundCalcEdit);
			this.LandedCostingDefaultsGroupBox.Controls.Add(this.zLabel6);
			this.LandedCostingDefaultsGroupBox.Controls.Add(this.JE_LandedCostByVolumeBoundCalcEdit);
			this.LandedCostingDefaultsGroupBox.Controls.Add(this.zLabel5);
			this.LandedCostingDefaultsGroupBox.Controls.Add(this.JE_LandedCostByWeightBoundCalcEdit);
			this.LandedCostingDefaultsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 547, true);
			this.LandedCostingDefaultsGroupBox.Name = "LandedCostingDefaultsGroupBox";
			this.LandedCostingDefaultsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 117, true);
			this.LandedCostingDefaultsGroupBox.TabIndex = 9;
			this.LandedCostingDefaultsGroupBox.TabStop = false;
			this.LandedCostingDefaultsGroupBox.Text = "Landed Costing Defaults";
			// 
			// zLabel10
			// 
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 91, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.zLabel10.TabIndex = 24;
			this.zLabel10.Text = "%";
			// 
			// JE_LandedCostByCostBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JE_LandedCostByCostBoundCalcEdit, "JE_LandedCostByCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_LandedCostByCost)));
			this.JE_LandedCostByCostBoundCalcEdit.DecimalPlaces = 2;
			this.JE_LandedCostByCostBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 90, true);
			this.JE_LandedCostByCostBoundCalcEdit.Name = "JE_LandedCostByCostBoundCalcEdit";
			this.JE_LandedCostByCostBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.JE_LandedCostByCostBoundCalcEdit.TabIndex = 3;
			this.JE_LandedCostByCostBoundCalcEdit.Text = "0";
			this.JE_LandedCostByCostBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel8
			// 
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 62, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.zLabel8.TabIndex = 21;
			this.zLabel8.Text = "%";
			// 
			// JE_LandedCostByUnitsBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JE_LandedCostByUnitsBoundCalcEdit, "JE_LandedCostByUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_LandedCostByUnits)));
			this.JE_LandedCostByUnitsBoundCalcEdit.DecimalPlaces = 2;
			this.JE_LandedCostByUnitsBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 64, true);
			this.JE_LandedCostByUnitsBoundCalcEdit.Name = "JE_LandedCostByUnitsBoundCalcEdit";
			this.JE_LandedCostByUnitsBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.JE_LandedCostByUnitsBoundCalcEdit.TabIndex = 2;
			this.JE_LandedCostByUnitsBoundCalcEdit.Text = "0";
			this.JE_LandedCostByUnitsBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel6
			// 
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 38, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.zLabel6.TabIndex = 18;
			this.zLabel6.Text = "%";
			// 
			// JE_LandedCostByVolumeBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JE_LandedCostByVolumeBoundCalcEdit, "JE_LandedCostByVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_LandedCostByVolume)));
			this.JE_LandedCostByVolumeBoundCalcEdit.DecimalPlaces = 2;
			this.JE_LandedCostByVolumeBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 40, true);
			this.JE_LandedCostByVolumeBoundCalcEdit.Name = "JE_LandedCostByVolumeBoundCalcEdit";
			this.JE_LandedCostByVolumeBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.JE_LandedCostByVolumeBoundCalcEdit.TabIndex = 1;
			this.JE_LandedCostByVolumeBoundCalcEdit.Text = "0";
			this.JE_LandedCostByVolumeBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 14, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.zLabel5.TabIndex = 15;
			this.zLabel5.Text = "%";
			// 
			// JE_LandedCostByWeightBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JE_LandedCostByWeightBoundCalcEdit, "JE_LandedCostByWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_LandedCostByWeight)));
			this.JE_LandedCostByWeightBoundCalcEdit.DecimalPlaces = 2;
			this.JE_LandedCostByWeightBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 16, true);
			this.JE_LandedCostByWeightBoundCalcEdit.Name = "JE_LandedCostByWeightBoundCalcEdit";
			this.JE_LandedCostByWeightBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.JE_LandedCostByWeightBoundCalcEdit.TabIndex = 0;
			this.JE_LandedCostByWeightBoundCalcEdit.Text = "0";
			this.JE_LandedCostByWeightBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ForceManualTILVCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ForceManualTILVCheckBox, "AddInfo+ZA_IsManualTILV_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_IsManualTILV_Hidden)));
			this.ForceManualTILVCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ForceManualTILVCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ForceManualTILVCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 84, true);
			this.ForceManualTILVCheckBox.Name = "ForceManualTILVCheckBox";
			this.ForceManualTILVCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 24, true);
			this.ForceManualTILVCheckBox.TabIndex = 14;
			this.ForceManualTILVCheckBox.Text = "Force Manual Line TILV Override";
			// 
			// MiscOptionsTabControl
			// 
			this.miscOptionsTabControl.Controls.Add(this.miscOptionsTabPage);
			this.miscOptionsTabControl.Controls.Add(this.uPETabPage);
			this.miscOptionsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.miscOptionsTabControl.Name = "MiscOptionsTabControl";
			this.miscOptionsTabControl.SelectedIndex = 0;
			this.miscOptionsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 700, true);
			this.miscOptionsTabControl.TabIndex = 15;
			// 
			// MiscOptionsTabPage
			// 
			this.miscOptionsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.miscOptionsTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("38b6d8e9-2617-4704-9a83-5c08ee200fb4", "Misc. Options");
			this.miscOptionsTabPage.Controls.Add(this.PeriodicSettlementGroupBox);
			this.miscOptionsTabPage.Controls.Add(this.AQISConcernTypes);
			this.miscOptionsTabPage.Controls.Add(this.LandedCostingDefaultsGroupBox);
			this.miscOptionsTabPage.Controls.Add(this.AQISInspectionLocationGroupBox);
			this.miscOptionsTabPage.Controls.Add(this.zGroupBox3);
			this.miscOptionsTabPage.Controls.Add(this.zGroupBox2);
			this.miscOptionsTabPage.Controls.Add(this.ExternalDeclaration);
			this.miscOptionsTabPage.Controls.Add(this.DeclarationIndicators);
			this.miscOptionsTabPage.Controls.Add(this.MiscOptionsGroupBox);
			this.miscOptionsTabPage.Controls.Add(this.AQISInformationGroupBox);
			this.miscOptionsTabPage.Controls.Add(this.AQISDocumentGroupBox);
			this.miscOptionsTabPage.Controls.Add(this.AQISPremisesIdAndPackagesGroupBox);
			this.miscOptionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.miscOptionsTabPage.Name = "MiscOptionsTabPage";
			this.miscOptionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.miscOptionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1199, 678, true);
			this.miscOptionsTabPage.TabIndex = 0;
			this.miscOptionsTabPage.Text = "Misc. Options";
			this.miscOptionsTabPage.Controls.SetChildIndex(this.AQISPremisesIdAndPackagesGroupBox, 0);
			this.miscOptionsTabPage.Controls.SetChildIndex(this.AQISDocumentGroupBox, 0);
			this.miscOptionsTabPage.Controls.SetChildIndex(this.AQISInformationGroupBox, 0);
			this.miscOptionsTabPage.Controls.SetChildIndex(this.MiscOptionsGroupBox, 0);
			this.miscOptionsTabPage.Controls.SetChildIndex(this.DeclarationIndicators, 0);
			this.miscOptionsTabPage.Controls.SetChildIndex(this.ExternalDeclaration, 0);
			this.miscOptionsTabPage.Controls.SetChildIndex(this.zGroupBox2, 0);
			this.miscOptionsTabPage.Controls.SetChildIndex(this.zGroupBox3, 0);
			this.miscOptionsTabPage.Controls.SetChildIndex(this.AQISInspectionLocationGroupBox, 0);
			this.miscOptionsTabPage.Controls.SetChildIndex(this.LandedCostingDefaultsGroupBox, 0);
			this.miscOptionsTabPage.Controls.SetChildIndex(this.AQISConcernTypes, 0);
			this.miscOptionsTabPage.Controls.SetChildIndex(this.PeriodicSettlementGroupBox, 0);
			// 
			// UPETabPage
			// 
			this.uPETabPage.BackColor = System.Drawing.SystemColors.Control;
			this.uPETabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("e6e1849c-48f8-41b2-a663-4f096c7bef1b", "UPE");
			this.uPETabPage.Controls.Add(this.uPEDetailsGroupBox);
			this.uPETabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.uPETabPage.Name = "UPETabPage";
			this.uPETabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.uPETabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1199, 678, true);
			this.uPETabPage.TabIndex = 1;
			this.uPETabPage.Text = "UPE";
			// 
			// UPEDetailsGroupBox
			// 
			this.uPEDetailsGroupBox.Controls.Add(this.spousePassportCountryDropEdit);
			this.uPEDetailsGroupBox.Controls.Add(this.importerPassportCountryDropEdit);
			this.uPEDetailsGroupBox.Controls.Add(this.importerLabel);
			this.uPEDetailsGroupBox.Controls.Add(this.numberOfChildrenCalcEdit);
			this.uPEDetailsGroupBox.Controls.Add(this.spousePassportNumberTextBox);
			this.uPEDetailsGroupBox.Controls.Add(this.spousePassportNameTextBox);
			this.uPEDetailsGroupBox.Controls.Add(this.spouseLabel);
			this.uPEDetailsGroupBox.Controls.Add(this.importerPassportSexDropEdit);
			this.uPEDetailsGroupBox.Controls.Add(this.importerPassportDateOfBirthDateEdit);
			this.uPEDetailsGroupBox.Controls.Add(this.importerPassportNumberTextBox);
			this.uPEDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.uPEDetailsGroupBox.Name = "UPEDetailsGroupBox";
			this.uPEDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 295, true);
			this.uPEDetailsGroupBox.TabIndex = 1;
			this.uPEDetailsGroupBox.TabStop = false;
			this.uPEDetailsGroupBox.Text = "Unaccompanied Personal Effects Details";
			// 
			// SpousePassportCountryDropEdit
			// 
			this.spousePassportCountryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.spousePassportCountryDropEdit, "AddInfo+ZA_UPESpousePassportCountry_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_UPESpousePassportCountry_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Lookups.CountryList)));
			this.spousePassportCountryDropEdit.BindToList = "Lookups.CountryList";
			this.spousePassportCountryDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|244d6fc8-5e26-48a5-9b4e-85ea39e5aa42", "Passport Ctry/Rgn. of Issue");
			this.spousePassportCountryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 208, true);
			this.spousePassportCountryDropEdit.Name = "SpousePassportCountryDropEdit";
			this.spousePassportCountryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.spousePassportCountryDropEdit.TabIndex = 6;
			// 
			// ImporterPassportCountryDropEdit
			// 
			this.importerPassportCountryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.importerPassportCountryDropEdit, "AddInfo+ZA_UPEImporterPassportCountry_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_UPEImporterPassportCountry_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Lookups.CountryList)));
			this.importerPassportCountryDropEdit.BindToList = "Lookups.CountryList";
			this.importerPassportCountryDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|484f5f7a-3245-40bd-9252-9b7391349888", "Passport Ctry/Rgn. of Issue");
			this.importerPassportCountryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 52, true);
			this.importerPassportCountryDropEdit.Name = "ImporterPassportCountryDropEdit";
			this.importerPassportCountryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.importerPassportCountryDropEdit.TabIndex = 1;
			// 
			// ImporterLabel
			// 
			this.importerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 26, true);
			this.importerLabel.Name = "ImporterLabel";
			this.importerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.importerLabel.TabIndex = 10;
			this.importerLabel.Text = "Importer:";
			this.importerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// NumberOfChildrenCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.numberOfChildrenCalcEdit, "AddInfo+ZA_UPEChildrenCount_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_UPEChildrenCount_Hidden)));
			this.numberOfChildrenCalcEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|62d8651e-896f-4e87-b4ed-709391aab5e2", "No. of Children");
			this.numberOfChildrenCalcEdit.DecimalPlaces = 2;
			this.numberOfChildrenCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 262, true);
			this.numberOfChildrenCalcEdit.Name = "NumberOfChildrenCalcEdit";
			this.numberOfChildrenCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.numberOfChildrenCalcEdit.TabIndex = 8;
			this.numberOfChildrenCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SpousePassportNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.spousePassportNumberTextBox, "AddInfo+ZA_UPESpousePassportNumber_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_UPESpousePassportNumber_Hidden)));
			this.spousePassportNumberTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|0ff1cdb6-9e54-4589-a858-b6416c212bf8", "Passport Number");
			this.spousePassportNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 234, true);
			this.spousePassportNumberTextBox.Name = "SpousePassportNumberTextBox";
			this.spousePassportNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.spousePassportNumberTextBox.TabIndex = 7;
			// 
			// SpousePassportNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.spousePassportNameTextBox, "AddInfo+ZA_UPESpouseName_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_UPESpouseName_Hidden)));
			this.spousePassportNameTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|3e13f3d0-aa78-4972-8cb2-1b53fd4fdba9", "Name");
			this.spousePassportNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 182, true);
			this.spousePassportNameTextBox.Name = "SpousePassportNameTextBox";
			this.spousePassportNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.spousePassportNameTextBox.TabIndex = 5;
			// 
			// SpouseLabel
			// 
			this.spouseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 156, true);
			this.spouseLabel.Name = "SpouseLabel";
			this.spouseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 23, true);
			this.spouseLabel.TabIndex = 5;
			this.spouseLabel.Text = "Spouse:";
			this.spouseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ImporterPassportSexDropEdit
			// 
			this.importerPassportSexDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.importerPassportSexDropEdit, "AddInfo+ZA_UPEImporterSex_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_UPEImporterSex_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Lookups.Gender)));
			this.importerPassportSexDropEdit.BindToList = "Lookups.Gender";
			this.importerPassportSexDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMROptionsUserControl|c6a7a9d9-9db5-4e0b-a098-589db2fd031b", "Sex");
			this.importerPassportSexDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 131, true);
			this.importerPassportSexDropEdit.Name = "ImporterPassportSexDropEdit";
			this.importerPassportSexDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.importerPassportSexDropEdit.TabIndex = 4;
			// 
			// ImporterPassportDateOfBirthDateEdit
			// 
			this.importerPassportDateOfBirthDateEdit.AllowDrop = true;
			this.importerPassportDateOfBirthDateEdit.AutoCompleteMonthThreshold = 1;
			this.importerPassportDateOfBirthDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.importerPassportDateOfBirthDateEdit, "AddInfo+ZA_UPEImporterDOB_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_UPEImporterDOB_Hidden)));
			this.importerPassportDateOfBirthDateEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|b1294cf6-84bc-4be5-acdf-b485cc626d37", "DOB");
			this.importerPassportDateOfBirthDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 105, true);
			this.importerPassportDateOfBirthDateEdit.Name = "ImporterPassportDateOfBirthDateEdit";
			this.importerPassportDateOfBirthDateEdit.TabIndex = 3;
			// 
			// ImporterPassportNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.importerPassportNumberTextBox, "AddInfo+ZA_UPEImporterPassportNumber_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).AddInfo.ZA_UPEImporterPassportNumber_Hidden)));
			this.importerPassportNumberTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCMRMiscOptionsUserControl|eb25068d-23e9-45c9-bbcc-27c0d881e9d4", "Passport Number");
			this.importerPassportNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 79, true);
			this.importerPassportNumberTextBox.Name = "ImporterPassportNumberTextBox";
			this.importerPassportNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.importerPassportNumberTextBox.TabIndex = 2;
			// 
			// AUCMRMiscOptionsUserControl
			// 
			this.AutoSize = true;
			this.Controls.Add(this.miscOptionsTabControl);
			this.Name = "AUCMRMiscOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 700, true);
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
			this.DeclarationIndicators.ResumeLayout(false);
			this.DeclarationIndicators.PerformLayout();
			this.ExternalDeclaration.ResumeLayout(false);
			this.ExternalDeclaration.PerformLayout();
			this.externalDecDate.ResumeLayout(true);
			this.externalDecDate.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.HeaderAmberReasonTypeCodeFindBox.ResumeLayout(true);
			this.HeaderAmberReasonTypeCodeFindBox.PerformLayout();
			this.HeaderAmberReasonTypeDropEdit.ResumeLayout(true);
			this.HeaderAmberReasonTypeDropEdit.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.AQISConcernTypes.ResumeLayout(false);
			this.AQISConcernTypes.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.aQISConcernType)).EndInit();
			this.aQISConcernType.ResumeLayout(false);
			this.aQISConcernType.PerformLayout();
			this.AQISInformationGroupBox.ResumeLayout(false);
			this.AQISInformationGroupBox.PerformLayout();
			this.aQISCommodityCodeControl.ResumeLayout(true);
			this.aQISCommodityCodeControl.PerformLayout();
			this.aQISPermitNumberControl.ResumeLayout(true);
			this.aQISPermitNumberControl.PerformLayout();
			this.aQISEntityIDControl.ResumeLayout(true);
			this.aQISEntityIDControl.PerformLayout();
			this.aQISProducerCodeControl.ResumeLayout(true);
			this.aQISProducerCodeControl.PerformLayout();
			this.AQISPremisesIdAndPackagesGroupBox.ResumeLayout(false);
			this.AQISPremisesIdAndPackagesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.aQISPremisesIdProcessingTypeGrid)).EndInit();
			this.aQISPremisesIdProcessingTypeGrid.ResumeLayout(false);
			this.aQISPremisesIdProcessingTypeGrid.PerformLayout();
			this.AQISDocumentGroupBox.ResumeLayout(false);
			this.AQISDocumentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.aQISDocumentGrid)).EndInit();
			this.aQISDocumentGrid.ResumeLayout(false);
			this.aQISDocumentGrid.PerformLayout();
			this.AQISInspectionLocationGroupBox.ResumeLayout(false);
			this.AQISInspectionLocationGroupBox.PerformLayout();
			this.PeriodicSettlementGroupBox.ResumeLayout(false);
			this.PeriodicSettlementGroupBox.PerformLayout();
			this.settlementPeriodEndDate.ResumeLayout(true);
			this.settlementPeriodEndDate.PerformLayout();
			this.settlementPeriodStartDate.ResumeLayout(true);
			this.settlementPeriodStartDate.PerformLayout();
			this.LandedCostingDefaultsGroupBox.ResumeLayout(false);
			this.LandedCostingDefaultsGroupBox.PerformLayout();
			this.miscOptionsTabControl.ResumeLayout(false);
			this.miscOptionsTabControl.PerformLayout();
			this.miscOptionsTabPage.ResumeLayout(false);
			this.miscOptionsTabPage.PerformLayout();
			this.uPETabPage.ResumeLayout(false);
			this.uPETabPage.PerformLayout();
			this.uPEDetailsGroupBox.ResumeLayout(false);
			this.uPEDetailsGroupBox.PerformLayout();
			this.spousePassportCountryDropEdit.ResumeLayout(true);
			this.spousePassportCountryDropEdit.PerformLayout();
			this.importerPassportCountryDropEdit.ResumeLayout(true);
			this.importerPassportCountryDropEdit.PerformLayout();
			this.importerPassportSexDropEdit.ResumeLayout(true);
			this.importerPassportSexDropEdit.PerformLayout();
			this.importerPassportDateOfBirthDateEdit.ResumeLayout(true);
			this.importerPassportDateOfBirthDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected internal ZArchitecture.ZTextBox FirstPaidUnderProtestID;
		protected internal ZArchitecture.ZTextBox UnaccompaniedPersonalEffectsID;
		protected internal ZArchitecture.ZTextBox CustomsReceiptForGoodsIdTextBox;
		protected internal ZArchitecture.GUI.ZGroupBox DeclarationIndicators;
		protected internal ZArchitecture.GUI.ZGroupBox ExternalDeclaration;
		private ZArchitecture.GUI.ZCheckBox reCalculateEffectiveDutDateCheckBox;
		private ZArchitecture.GUI.ZCheckBox jE_VisialExaminationApplicationCheckBox;
		private ZArchitecture.ZTextBox aQISInspectionLocationTextBox;
		protected internal ZArchitecture.GUI.ZGroupBox zGroupBox2;
		protected internal ZArchitecture.ZTextBox AmberStatementTextBox;
		protected internal ZArchitecture.GUI.ZGroupBox zGroupBox3;
		protected internal ZArchitecture.GUI.ZCodeFindBox HeaderAmberReasonTypeCodeFindBox;
		protected internal ZArchitecture.GUI.ZGroupBox AQISConcernTypes;
		protected internal ZArchitecture.GUI.ZGroupBox AQISPremisesIdAndPackagesGroupBox;
		protected internal ZArchitecture.GUI.ZGroupBox AQISDocumentGroupBox;
		protected internal ZArchitecture.GUI.ZGroupBox AQISInspectionLocationGroupBox;
		protected internal ZArchitecture.GUI.ZGroupBox PeriodicSettlementGroupBox;
		protected internal ZArchitecture.GUI.ZGroupBox AQISInformationGroupBox;
		private ZArchitecture.ZTextBox paidUnderProtestTextBox;
		protected internal ZArchitecture.GUI.ZGroupBox LandedCostingDefaultsGroupBox;
		protected internal ZArchitecture.ZLabel zLabel10;
		protected internal ZArchitecture.ZCalcEdit JE_LandedCostByCostBoundCalcEdit;
		protected internal ZArchitecture.ZLabel zLabel8;
		protected internal ZArchitecture.ZCalcEdit JE_LandedCostByUnitsBoundCalcEdit;
		protected internal ZArchitecture.ZLabel zLabel6;
		protected internal ZArchitecture.ZCalcEdit JE_LandedCostByVolumeBoundCalcEdit;
		protected internal ZArchitecture.ZLabel zLabel5;
		protected internal ZArchitecture.ZCalcEdit JE_LandedCostByWeightBoundCalcEdit;
		protected internal ZArchitecture.GUI.ZCheckBox ForceManualTILVCheckBox;
		protected internal ZArchitecture.GUI.ZDropEdit HeaderAmberReasonTypeDropEdit;
		private ZArchitecture.GUI.ZDateEdit externalDecDate;
		protected internal ZArchitecture.GUI.ZDropEdit SettlementTypeDropEdit;
		protected internal ZArchitecture.GUI.ZCheckBox NilReturnCheckBox;
		private ZArchitecture.GUI.ZDateEdit settlementPeriodStartDate;
		protected internal ZArchitecture.GUI.ZDateEdit settlementPeriodEndDate;
		protected internal ZArchitecture.GUI.ZCheckBox UnaccompaniedPersonalEffectsCheckBox;
		private ZArchitecture.GUI.ZTabControl miscOptionsTabControl;
		private ZArchitecture.GUI.ZTabPage miscOptionsTabPage;
		private ZArchitecture.GUI.ZTabPage uPETabPage;
		private ZArchitecture.GUI.ZGroupBox uPEDetailsGroupBox;
		private ZArchitecture.ZCalcEdit numberOfChildrenCalcEdit;
		private ZArchitecture.ZTextBox spousePassportNumberTextBox;
		private ZArchitecture.ZTextBox spousePassportNameTextBox;
		private ZArchitecture.ZLabel spouseLabel;
		private ZArchitecture.GUI.ZDropEdit importerPassportSexDropEdit;
		private ZArchitecture.GUI.ZDateEdit importerPassportDateOfBirthDateEdit;
		private ZArchitecture.ZTextBox importerPassportNumberTextBox;
		private ZArchitecture.ZLabel importerLabel;
		private ZArchitecture.ZGrid aQISConcernType;
		internal ZArchitecture.ZGrid aQISPremisesIdProcessingTypeGrid;
		private ZArchitecture.ZGrid aQISDocumentGrid;
		private ZArchitecture.GUI.ZDropEdit importerPassportCountryDropEdit;
		private ZArchitecture.GUI.ZDropEdit spousePassportCountryDropEdit;
		private AQISCommodityCodeControl aQISCommodityCodeControl;
		private AQISPermitNumberControl aQISPermitNumberControl;
		private AQISEntityIdControl aQISEntityIDControl;
		private AQISProducerCodeControl aQISProducerCodeControl;
		protected internal ZArchitecture.GUI.ZCheckBox SOFACheckBox;
		private IContainer components;
	}
}
