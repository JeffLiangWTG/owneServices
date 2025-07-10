namespace Enterprise.Customs.CH.GUI;

partial class InvoiceLineDetailsUserControl
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
			this.NetDutyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CustomNetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TareSupplementUserControl = new Enterprise.Customs.CH.GUI.TareSupplementUserControl();
			this.VATCodeUserControl = new Enterprise.Customs.CH.GUI.VATCodeUserControl();
			this.CalculatedGrossMassCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.PermitObligationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StorageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NonCustomsLawObligationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GrossMassConfirmationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NetMassConfirmationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AdditionalUnitConfirmationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.StatisticalValueConfirmationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.VATValueConfirmationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonCommercialGoodsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GoodsReturnedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConfirmationCodesSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.DutyRateUserControl = new Enterprise.Customs.CH.GUI.DutyRateUserControl();
			this.RefundTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RefundReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RefundGoodsItemNumberIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.RefundReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CusCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RateOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OverriddenRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RateFormulaDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UNDGCodesUserControl = new Enterprise.Customs.CH.GUI.UNDGCodesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomNetWeightCalcDropEdit.SuspendLayout();
			this.TareSupplementUserControl.SuspendLayout();
			this.VATCodeUserControl.SuspendLayout();
			this.CalculatedGrossMassCalcDropEdit.SuspendLayout();
			this.PermitObligationDropEdit.SuspendLayout();
			this.StorageTypeDropEdit.SuspendLayout();
			this.NonCustomsLawObligationDropEdit.SuspendLayout();
			this.ConfirmationCodesSeparatorUserControl.SuspendLayout();
			this.DutyRateUserControl.SuspendLayout();
			this.RefundTypeDropEdit.SuspendLayout();
			this.CusCodeFindBox.SuspendLayout();
			this.UNDGCodesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobComInvoiceLine);
			// 
			// NetDutyCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NetDutyCheckBox, "NetDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).NetDuty)));
			this.NetDutyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 160, true);
			this.NetDutyCheckBox.Name = "NetDutyCheckBox";
			this.NetDutyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.NetDutyCheckBox.TabIndex = 0;
			this.NetDutyCheckBox.UseVisualStyleBackColor = true;
			// 
			// CustomNetWeightCalcDropEdit
			// 
			this.CustomNetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomNetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_WeightIncludingInnerPackage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_WeightIncludingInnerPackageUQ)));
			this.CustomNetWeightCalcDropEdit.BindToAmount = "JI_WeightIncludingInnerPackage";
			this.CustomNetWeightCalcDropEdit.BindToUnit = "JI_WeightIncludingInnerPackageUQ";
			this.CustomNetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 3, true);
			this.CustomNetWeightCalcDropEdit.Name = "CustomNetWeightCalcDropEdit";
			this.CustomNetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.CustomNetWeightCalcDropEdit.TabIndex = 1;
			// 
			// TareSupplementUserControl
			// 
			this.TareSupplementUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TareSupplementUserControl, ".");
			this.TareSupplementUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 29, true);
			this.TareSupplementUserControl.Name = "TareSupplementUserControl";
			this.TareSupplementUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.TareSupplementUserControl.TabIndex = 5;
			// 
			// VATCodeUserControl
			// 
			this.VATCodeUserControl.AllowDrop = true;
			this.VATCodeUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.VATCodeUserControl, ".");
			this.VATCodeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 186, true);
			this.VATCodeUserControl.Name = "VATCodeUserControl";
			this.VATCodeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 29, true);
			this.VATCodeUserControl.TabIndex = 5;
			// 
			// CalculatedGrossMassCalcDropEdit
			// 
			this.CalculatedGrossMassCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CalculatedGrossMassCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).CalculatedGrossMass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).CalculatedGrossMassUQ)));
			this.CalculatedGrossMassCalcDropEdit.BindToAmount = "CalculatedGrossMass";
			this.CalculatedGrossMassCalcDropEdit.BindToUnit = "CalculatedGrossMassUQ";
			this.CalculatedGrossMassCalcDropEdit.Decimals = 1;
			this.CalculatedGrossMassCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 55, true);
			this.CalculatedGrossMassCalcDropEdit.Name = "CalculatedGrossMassCalcDropEdit";
			this.CalculatedGrossMassCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.CalculatedGrossMassCalcDropEdit.TabIndex = 6;
			// 
			// PermitObligationDropEdit
			// 
			this.PermitObligationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PermitObligationDropEdit, "JI_PermitObligation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_PermitObligation)));
			this.PermitObligationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 82, true);
			this.PermitObligationDropEdit.Name = "PermitObligationDropEdit";
			this.PermitObligationDropEdit.PreBoundMaxLength = 2;
			this.PermitObligationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.PermitObligationDropEdit.TabIndex = 8;
			// 
			// StorageTypeDropEdit
			// 
			this.StorageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StorageTypeDropEdit, "JI_StorageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_StorageType)));
			this.StorageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 134, true);
			this.StorageTypeDropEdit.Name = "StorageTypeDropEdit";
			this.StorageTypeDropEdit.PreBoundMaxLength = 2;
			this.StorageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.StorageTypeDropEdit.TabIndex = 10;
			// 
			// NonCustomsLawObligationDropEdit
			// 
			this.NonCustomsLawObligationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NonCustomsLawObligationDropEdit, "JI_NonCustomsLawObligation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_NonCustomsLawObligation)));
			this.NonCustomsLawObligationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 108, true);
			this.NonCustomsLawObligationDropEdit.Name = "NonCustomsLawObligationDropEdit";
			this.NonCustomsLawObligationDropEdit.PreBoundMaxLength = 2;
			this.NonCustomsLawObligationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.NonCustomsLawObligationDropEdit.TabIndex = 9;
			// 
			// GrossMassConfirmationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.GrossMassConfirmationCheckBox, "JI_GrossMassConfirmation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_GrossMassConfirmation)));
			this.GrossMassConfirmationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 29, true);
			this.GrossMassConfirmationCheckBox.Name = "GrossMassConfirmationCheckBox";
			this.GrossMassConfirmationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.GrossMassConfirmationCheckBox.TabIndex = 11;
			this.GrossMassConfirmationCheckBox.UseVisualStyleBackColor = true;
			// 
			// NetMassConfirmationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NetMassConfirmationCheckBox, "JI_NetMassConfirmation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_NetMassConfirmation)));
			this.NetMassConfirmationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 55, true);
			this.NetMassConfirmationCheckBox.Name = "NetMassConfirmationCheckBox";
			this.NetMassConfirmationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.NetMassConfirmationCheckBox.TabIndex = 12;
			this.NetMassConfirmationCheckBox.UseVisualStyleBackColor = true;
			// 
			// AdditionalUnitConfirmationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalUnitConfirmationCheckBox, "JI_AdditionalUnitConfirmation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_AdditionalUnitConfirmation)));
			this.AdditionalUnitConfirmationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 81, true);
			this.AdditionalUnitConfirmationCheckBox.Name = "AdditionalUnitConfirmationCheckBox";
			this.AdditionalUnitConfirmationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.AdditionalUnitConfirmationCheckBox.TabIndex = 13;
			this.AdditionalUnitConfirmationCheckBox.UseVisualStyleBackColor = true;
			// 
			// StatisticalValueConfirmationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.StatisticalValueConfirmationCheckBox, "JI_StatisticalValueConfirmation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_StatisticalValueConfirmation)));
			this.StatisticalValueConfirmationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 108, true);
			this.StatisticalValueConfirmationCheckBox.Name = "StatisticalValueConfirmationCheckBox";
			this.StatisticalValueConfirmationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.StatisticalValueConfirmationCheckBox.TabIndex = 14;
			this.StatisticalValueConfirmationCheckBox.UseVisualStyleBackColor = true;
			// 
			// VATValueConfirmationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.VATValueConfirmationCheckBox, "JI_VATValueConfirmation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_VATValueConfirmation)));
			this.VATValueConfirmationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 134, true);
			this.VATValueConfirmationCheckBox.Name = "VATValueConfirmationCheckBox";
			this.VATValueConfirmationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.VATValueConfirmationCheckBox.TabIndex = 15;
			this.VATValueConfirmationCheckBox.UseVisualStyleBackColor = true;
			// 
			// NonCommercialGoodsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NonCommercialGoodsCheckBox, "JI_NonTradingGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_NonTradingGoods)));
			this.NonCommercialGoodsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 160, true);
			this.NonCommercialGoodsCheckBox.Name = "NonCommercialGoodsCheckBox";
			this.NonCommercialGoodsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.NonCommercialGoodsCheckBox.TabIndex = 15;
			this.NonCommercialGoodsCheckBox.UseVisualStyleBackColor = true;
			// 
			// GoodsReturnedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsReturnedCheckBox, "JI_GoodsReturned");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_GoodsReturned)));
			this.GoodsReturnedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(431, 160, true);
			this.GoodsReturnedCheckBox.Name = "GoodsReturnedCheckBox";
			this.GoodsReturnedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.GoodsReturnedCheckBox.TabIndex = 15;
			this.GoodsReturnedCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConfirmationCodesSeparatorUserControl
			// 
			this.ConfirmationCodesSeparatorUserControl.AllowDrop = true;
			this.ConfirmationCodesSeparatorUserControl.AutoSize = true;
			this.ConfirmationCodesSeparatorUserControl.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("1eabe3cc-fc86-41c0-adce-e31433ea5e9e", "Confirmation Codes");
			this.ConfirmationCodesSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(428, 3, true);
			this.ConfirmationCodesSeparatorUserControl.Name = "ConfirmationCodesSeparatorUserControl";
			this.ConfirmationCodesSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.ConfirmationCodesSeparatorUserControl.TabIndex = 16;
			// 
			// DutyRateUserControl
			// 
			this.DutyRateUserControl.AllowDrop = true;
			this.DutyRateUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DutyRateUserControl, ".");
			this.DutyRateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 215, true);
			this.DutyRateUserControl.Name = "DutyRateUserControl";
			this.DutyRateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 29, true);
			this.DutyRateUserControl.TabIndex = 5;
			// 
			// RefundTypeDropEdit
			// 
			this.RefundTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RefundTypeDropEdit, "JI_RefundType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_RefundType)));
			this.RefundTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 240, true);
			this.RefundTypeDropEdit.Name = "RefundTypeDropEdit";
			this.RefundTypeDropEdit.PreBoundMaxLength = 1;
			this.RefundTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.RefundTypeDropEdit.TabIndex = 17;
			// 
			// RefundReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RefundReferenceNumberTextBox, "JI_RefundReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_RefundReferenceNumber)));
			this.RefundReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 266, true);
			this.RefundReferenceNumberTextBox.Name = "RefundReferenceNumberTextBox";
			this.RefundReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.RefundReferenceNumberTextBox.TabIndex = 19;
			// 
			// RefundGoodsItemNumberIntEdit
			// 
			this.BindingSource.SetBindingMember(this.RefundGoodsItemNumberIntEdit, "JI_RefundGoodsItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_RefundGoodsItemNumber)));
			this.RefundGoodsItemNumberIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 292, true);
			this.RefundGoodsItemNumberIntEdit.Name = "RefundGoodsItemNumberIntEdit";
			this.RefundGoodsItemNumberIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.RefundGoodsItemNumberIntEdit.TabIndex = 20;
			// 
			// RefundReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.RefundReasonTextBox, "JI_RefundReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_RefundReason)));
			this.RefundReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RefundReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 318, true);
			this.RefundReasonTextBox.Multiline = true;
			this.RefundReasonTextBox.Name = "RefundReasonTextBox";
			this.RefundReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.RefundReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 89, true);
			this.RefundReasonTextBox.TabIndex = 21;
			// 
			// CusCodeFindBox
			// 
			this.CusCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CusCodeFindBox, "JI_CusNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_CusNumber)));
			this.CusCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 413, true);
			this.CusCodeFindBox.Name = "CusCodeFindBox";
			this.CusCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CusCodeFindBox.ParentType = null;
			this.CusCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.CusCodeFindBox.TabIndex = 22;
			// 
			// RateOverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.RateOverrideCheckBox, "JI_RateOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_RateOverride)));
			this.RateOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 439, true);
			this.RateOverrideCheckBox.Name = "RateOverrideCheckBox";
			this.RateOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.RateOverrideCheckBox.TabIndex = 23;
			this.RateOverrideCheckBox.UseVisualStyleBackColor = true;
			// 
			// OverriddenRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OverriddenRateCalcEdit, "JI_OverriddenRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_OverriddenRate)));
			this.OverriddenRateCalcEdit.DecimalPlaces = 2;
			this.OverriddenRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 495, true);
			this.OverriddenRateCalcEdit.Name = "OverriddenRateCalcEdit";
			this.OverriddenRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OverriddenRateCalcEdit.TabIndex = 24;
			this.OverriddenRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.OverriddenRateCalcEdit.TrackDisposedAccess = true;
			// 
			// RateFormulaDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.RateFormulaDescriptionTextBox, "RateFormulaDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).RateFormulaDescription)));
			this.RateFormulaDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 439, true);
			this.RateFormulaDescriptionTextBox.Name = "RateFormulaDescriptionTextBox";
			this.RateFormulaDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 20, true);
			this.RateFormulaDescriptionTextBox.TabIndex = 25;
			// 
			// UNDGCodesUserControl
			// 
			this.UNDGCodesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UNDGCodesUserControl, ".");
			this.UNDGCodesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 467, true);
			this.UNDGCodesUserControl.Name = "UNDGCodesUserControl";
			this.UNDGCodesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.UNDGCodesUserControl.TabIndex = 20;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RateFormulaDescriptionTextBox);
			this.Controls.Add(this.OverriddenRateCalcEdit);
			this.Controls.Add(this.RateOverrideCheckBox);
			this.Controls.Add(this.CusCodeFindBox);
			this.Controls.Add(this.RefundReasonTextBox);
			this.Controls.Add(this.RefundGoodsItemNumberIntEdit);
			this.Controls.Add(this.RefundReferenceNumberTextBox);
			this.Controls.Add(this.RefundTypeDropEdit);
			this.Controls.Add(this.ConfirmationCodesSeparatorUserControl);
			this.Controls.Add(this.VATValueConfirmationCheckBox);
			this.Controls.Add(this.StatisticalValueConfirmationCheckBox);
			this.Controls.Add(this.AdditionalUnitConfirmationCheckBox);
			this.Controls.Add(this.NetMassConfirmationCheckBox);
			this.Controls.Add(this.GrossMassConfirmationCheckBox);
			this.Controls.Add(this.StorageTypeDropEdit);
			this.Controls.Add(this.NonCustomsLawObligationDropEdit);
			this.Controls.Add(this.PermitObligationDropEdit);
			this.Controls.Add(this.CalculatedGrossMassCalcDropEdit);
			this.Controls.Add(this.TareSupplementUserControl);
			this.Controls.Add(this.VATCodeUserControl);
			this.Controls.Add(this.CustomNetWeightCalcDropEdit);
			this.Controls.Add(this.NetDutyCheckBox);
			this.Controls.Add(this.NonCommercialGoodsCheckBox);
			this.Controls.Add(this.DutyRateUserControl);
			this.Controls.Add(this.GoodsReturnedCheckBox);
			this.Controls.Add(this.UNDGCodesUserControl);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 551, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomNetWeightCalcDropEdit.ResumeLayout(true);
			this.CustomNetWeightCalcDropEdit.PerformLayout();
			this.TareSupplementUserControl.ResumeLayout(true);
			this.TareSupplementUserControl.PerformLayout();
			this.VATCodeUserControl.ResumeLayout(true);
			this.VATCodeUserControl.PerformLayout();
			this.CalculatedGrossMassCalcDropEdit.ResumeLayout(true);
			this.CalculatedGrossMassCalcDropEdit.PerformLayout();
			this.PermitObligationDropEdit.ResumeLayout(true);
			this.PermitObligationDropEdit.PerformLayout();
			this.StorageTypeDropEdit.ResumeLayout(true);
			this.StorageTypeDropEdit.PerformLayout();
			this.NonCustomsLawObligationDropEdit.ResumeLayout(true);
			this.NonCustomsLawObligationDropEdit.PerformLayout();
			this.ConfirmationCodesSeparatorUserControl.ResumeLayout(true);
			this.ConfirmationCodesSeparatorUserControl.PerformLayout();
			this.DutyRateUserControl.ResumeLayout(true);
			this.DutyRateUserControl.PerformLayout();
			this.RefundTypeDropEdit.ResumeLayout(true);
			this.RefundTypeDropEdit.PerformLayout();
			this.CusCodeFindBox.ResumeLayout(true);
			this.CusCodeFindBox.PerformLayout();
			this.UNDGCodesUserControl.ResumeLayout(true);
			this.UNDGCodesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZCalcDropEdit CustomNetWeightCalcDropEdit;
    internal TareSupplementUserControl TareSupplementUserControl;
    internal VATCodeUserControl VATCodeUserControl;
    internal ZArchitecture.GUI.ZCheckBox NetDutyCheckBox;
    internal ZArchitecture.GUI.ZCalcDropEdit CalculatedGrossMassCalcDropEdit;
    internal ZArchitecture.GUI.ZDropEdit PermitObligationDropEdit;
    internal ZArchitecture.GUI.ZDropEdit NonCustomsLawObligationDropEdit;
    internal ZArchitecture.GUI.ZDropEdit StorageTypeDropEdit;
    internal ZArchitecture.GUI.ZCheckBox GrossMassConfirmationCheckBox;
    internal ZArchitecture.GUI.ZCheckBox NetMassConfirmationCheckBox;
    internal ZArchitecture.GUI.ZCheckBox AdditionalUnitConfirmationCheckBox;
    internal ZArchitecture.GUI.ZCheckBox StatisticalValueConfirmationCheckBox;
    internal ZArchitecture.GUI.ZCheckBox VATValueConfirmationCheckBox;
    internal ZArchitecture.GUI.ZCheckBox GoodsReturnedCheckBox;
    internal ZArchitecture.GUI.ZCheckBox NonCommercialGoodsCheckBox;
    internal Enterprise.ZArchitecture.GUI.SeparatorUserControl ConfirmationCodesSeparatorUserControl;
    internal DutyRateUserControl DutyRateUserControl;
    internal ZArchitecture.GUI.ZDropEdit RefundTypeDropEdit;
    internal ZArchitecture.ZTextBox RefundReferenceNumberTextBox;
    internal ZArchitecture.ZTextBox RefundReasonTextBox;
    internal ZArchitecture.GUI.ZIntEdit RefundGoodsItemNumberIntEdit;
    internal ZArchitecture.GUI.ZCodeFindBox CusCodeFindBox;
    internal ZArchitecture.GUI.ZCheckBox RateOverrideCheckBox;
    internal ZArchitecture.ZCalcEdit OverriddenRateCalcEdit;
    internal ZArchitecture.ZTextBox RateFormulaDescriptionTextBox;
	internal UNDGCodesUserControl UNDGCodesUserControl;
}
