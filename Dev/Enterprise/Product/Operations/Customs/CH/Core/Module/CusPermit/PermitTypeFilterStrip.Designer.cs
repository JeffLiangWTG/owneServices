
namespace Enterprise.Customs.CH.Module;

partial class PermitTypeFilterStrip
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
        this.CountryDropEdit.SuspendLayout();
        this.TypeDropEdit.SuspendLayout();
        this.SubTypeDropEdit.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.SuspendLayout();
        // 
        // TypeDropEdit
        // 
        this.TypeDropEdit.ShowDescriptionBox = true;
        this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 17, true);
        // 
        // SubTypeDropEdit
        // 
        this.SubTypeDropEdit.Visible = false;
        // 
        // PermitTypeFilterStrip
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Name = "PermitTypeFilterStrip";
        this.CountryDropEdit.ResumeLayout(true);
        this.CountryDropEdit.PerformLayout();
        this.TypeDropEdit.ResumeLayout(true);
        this.TypeDropEdit.PerformLayout();
        this.SubTypeDropEdit.ResumeLayout(true);
        this.SubTypeDropEdit.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
}
