namespace Enterprise.Customs.CH.GUI;

partial class PaymentMethodUserControl
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
        this.AccountNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.PaymentMethodDropEdit.SuspendLayout();
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
        this.PaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.PaymentMethodDropEdit.Name = "PaymentMethodDropEdit";
        this.PaymentMethodDropEdit.ShouldResizeByMaxLength = true;
        this.PaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
        this.PaymentMethodDropEdit.TabIndex = 0;
        // 
        // AccountNoTextBox
        // 
        this.BindingSource.SetBindingMember(this.AccountNoTextBox, "DutyPaidByAccountNo");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).DutyPaidByAccountNo)));
        this.AccountNoTextBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("d26a91ef-a702-4b31-a714-73521106e2a7", "Account");
        this.AccountNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 0, true);
        this.AccountNoTextBox.Name = "AccountNoTextBox";
        this.AccountNoTextBox.ShouldEscapeAllSpecialCharacters = false;
        this.AccountNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
        this.AccountNoTextBox.TabIndex = 1;
        // 
        // PaymentMethodUserControl
        // 
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.PaymentMethodDropEdit);
        this.Controls.Add(this.AccountNoTextBox);
        this.Name = "PaymentMethodUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 21, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.PaymentMethodDropEdit.ResumeLayout(true);
        this.PaymentMethodDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZDropEdit PaymentMethodDropEdit;
    internal ZArchitecture.ZTextBox AccountNoTextBox;
}
