
namespace Enterprise.Customs.CH.GUI;

partial class CHStaffCredentialsUserControl
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
        this.DeclarantGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.declarantNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.DeclarantGroupBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.CHGlbStaffWrapper);
        // 
        // Declarant
        // 
        this.DeclarantGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("223e95dc-2067-4245-a7f7-5d990414d501", "Declarant");
        this.DeclarantGroupBox.Controls.Add(this.declarantNumberTextBox);
        this.DeclarantGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 3, true);
        this.DeclarantGroupBox.Name = "Declarant";
        this.DeclarantGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 54, true);
        this.DeclarantGroupBox.TabIndex = 1;
        this.DeclarantGroupBox.TabStop = false;
        // 
        // declarantNumberTextBox
        // 
        this.BindingSource.SetBindingMember(this.declarantNumberTextBox, "CHDPassword.GP_UserID");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CHGlbStaffWrapper)(null)).CHDPassword.GP_UserID)));
        this.declarantNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 19, true);
        this.declarantNumberTextBox.Name = "declarantNumberTextBox";
        this.declarantNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
        this.declarantNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
        this.declarantNumberTextBox.TabIndex = 0;
        // 
        // CHStaffCredentialsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Controls.Add(this.DeclarantGroupBox);
        this.Name = "CHStaffCredentialsUserControl";
        this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
        this.Controls.SetChildIndex(this.DeclarantGroupBox, 0);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.DeclarantGroupBox.ResumeLayout(false);
        this.DeclarantGroupBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private ZArchitecture.GUI.ZGroupBox DeclarantGroupBox;
    private ZArchitecture.ZTextBox declarantNumberTextBox;
}
