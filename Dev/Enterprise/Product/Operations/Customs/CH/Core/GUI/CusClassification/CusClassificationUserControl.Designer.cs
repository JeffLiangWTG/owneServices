namespace Enterprise.Customs.CH.GUI;

partial class CusClassificationUserControl
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
        if (disposing)
        {
            components?.Dispose();
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
        this.BaseClassificationGroupBox.SuspendLayout();
        this.LastAuditDateEdit.SuspendLayout();
        this.AuditStaffCodeFindBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.SuspendLayout();
        // 
        // CC_IsActiveCheckBox
        // 
        this.CC_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
        // 
        // CusClassificationUserControl
        // 
        this.Name = "CusClassificationUserControl";
        this.BaseClassificationGroupBox.ResumeLayout(false);
        this.BaseClassificationGroupBox.PerformLayout();
        this.LastAuditDateEdit.ResumeLayout(true);
        this.LastAuditDateEdit.PerformLayout();
        this.AuditStaffCodeFindBox.ResumeLayout(true);
        this.AuditStaffCodeFindBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
}
