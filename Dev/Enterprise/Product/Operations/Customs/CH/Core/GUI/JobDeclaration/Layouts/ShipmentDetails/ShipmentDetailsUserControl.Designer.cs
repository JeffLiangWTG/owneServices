namespace Enterprise.Customs.CH.GUI;

partial class ShipmentDetailsUserControl
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
        this.PaymentMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.VatPaidByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.ClearanceLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.AdditionalDecisionInfoCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        this.PaymentMethodUserControl = new Enterprise.Customs.CH.GUI.PaymentMethodUserControl();
        this.VatPaidByUserControl = new Enterprise.Customs.CH.GUI.PaymentMethodUserControl();
        this.LocationOfGoodsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.PaymentMethodDropEdit.SuspendLayout();
        this.VatPaidByDropEdit.SuspendLayout();
        this.ClearanceLocationDropEdit.SuspendLayout();
        this.PaymentMethodUserControl.SuspendLayout();
        this.VatPaidByUserControl.SuspendLayout();
        this.LocationOfGoodsDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobDeclaration);
        // 
        // PaymentMethodDropEdit
        // 
        this.PaymentMethodDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.PaymentMethodDropEdit, "JE_PaymentMethod");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).JE_PaymentMethod)));
        this.PaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 4, true);
        this.PaymentMethodDropEdit.Name = "PaymentMethodDropEdit";
        this.PaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
        this.PaymentMethodDropEdit.TabIndex = 0;
        // 
        // VatPaidByDropEdit
        // 
        this.VatPaidByDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.VatPaidByDropEdit, "JE_VATPaidBy");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).JE_VATPaidBy)));
        this.VatPaidByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 31, true);
        this.VatPaidByDropEdit.Name = "VatPaidByDropEdit";
        this.VatPaidByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
        this.VatPaidByDropEdit.TabIndex = 1;
        // 
        // ClearanceLocationDropEdit
        // 
        this.ClearanceLocationDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.ClearanceLocationDropEdit, "JE_ClearanceLocation");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).JE_ClearanceLocation)));
        this.ClearanceLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 58, true);
        this.ClearanceLocationDropEdit.Name = "ClearanceLocationDropEdit";
        this.ClearanceLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
        this.ClearanceLocationDropEdit.TabIndex = 2;
        // 
        // AdditionalDecisionInfoCheckBox
        // 
        this.BindingSource.SetBindingMember(this.AdditionalDecisionInfoCheckBox, "JE_AdditionalDecisionInfo");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).JE_AdditionalDecisionInfo)));
        this.AdditionalDecisionInfoCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 115, true);
        this.AdditionalDecisionInfoCheckBox.Name = "AdditionalDecisionInfoCheckBox";
        this.AdditionalDecisionInfoCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
        this.AdditionalDecisionInfoCheckBox.TabIndex = 5;
        this.AdditionalDecisionInfoCheckBox.UseVisualStyleBackColor = true;
        // 
        // PaymentMethodUserControl
        // 
        this.PaymentMethodUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.PaymentMethodUserControl, ".");
        this.PaymentMethodUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 141, true);
        this.PaymentMethodUserControl.Name = "PaymentMethodUserControl";
        this.PaymentMethodUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 21, true);
        this.PaymentMethodUserControl.TabIndex = 6;
        // 
        // VatPaidByUserControl
        // 
        this.VatPaidByUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.VatPaidByUserControl, ".");
        this.VatPaidByUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 168, true);
        this.VatPaidByUserControl.Name = "VatPaidByUserControl";
        this.VatPaidByUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 21, true);
        this.VatPaidByUserControl.TabIndex = 7;
        // 
        // LocationOfGoodsDropEdit
        // 
        this.LocationOfGoodsDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.LocationOfGoodsDropEdit, "JE_LocationOfGoods");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).JE_LocationOfGoods)));
        this.LocationOfGoodsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 196, true);
        this.LocationOfGoodsDropEdit.Name = "LocationOfGoodsDropEdit";
        this.LocationOfGoodsDropEdit.ShouldResizeByMaxLength = false;
        this.LocationOfGoodsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
        this.LocationOfGoodsDropEdit.TabIndex = 8;
        // 
        // ShipmentDetailsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.LocationOfGoodsDropEdit);
        this.Controls.Add(this.VatPaidByUserControl);
        this.Controls.Add(this.PaymentMethodUserControl);
        this.Controls.Add(this.AdditionalDecisionInfoCheckBox);
        this.Controls.Add(this.ClearanceLocationDropEdit);
        this.Controls.Add(this.VatPaidByDropEdit);
        this.Controls.Add(this.PaymentMethodDropEdit);
        this.Name = "ShipmentDetailsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(644, 238, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.PaymentMethodDropEdit.ResumeLayout(true);
        this.PaymentMethodDropEdit.PerformLayout();
        this.VatPaidByDropEdit.ResumeLayout(true);
        this.VatPaidByDropEdit.PerformLayout();
        this.ClearanceLocationDropEdit.ResumeLayout(true);
        this.ClearanceLocationDropEdit.PerformLayout();
        this.PaymentMethodUserControl.ResumeLayout(true);
        this.PaymentMethodUserControl.PerformLayout();
        this.VatPaidByUserControl.ResumeLayout(true);
        this.VatPaidByUserControl.PerformLayout();
        this.LocationOfGoodsDropEdit.ResumeLayout(true);
        this.LocationOfGoodsDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZDropEdit PaymentMethodDropEdit;
    internal ZArchitecture.GUI.ZDropEdit VatPaidByDropEdit;
    internal ZArchitecture.GUI.ZDropEdit ClearanceLocationDropEdit;
    internal ZArchitecture.GUI.ZCheckBox AdditionalDecisionInfoCheckBox;
    internal PaymentMethodUserControl PaymentMethodUserControl;
    internal PaymentMethodUserControl VatPaidByUserControl;
    internal ZArchitecture.GUI.ZDropEdit LocationOfGoodsDropEdit;
}
