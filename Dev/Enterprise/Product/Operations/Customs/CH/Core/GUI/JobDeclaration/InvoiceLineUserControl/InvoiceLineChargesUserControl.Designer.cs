namespace Enterprise.Customs.CH.GUI;

public partial class InvoiceLineChargesUserControl
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
        this.ChargesGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).BeginInit();
        this.ChargesGrid.SuspendLayout();
        this.ApportionedChargesGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
        this.ApportionedChargesGrid.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
        this.SplitContainer.Panel1.SuspendLayout();
        this.SplitContainer.Panel2.SuspendLayout();
        this.SplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.SuspendLayout();
        // 
        // SplitContainer
        // 
        // 
        // ImportInvoiceLineChargesUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Name = "ImportInvoiceLineChargesUserControl";
        this.ChargesGroupBox.ResumeLayout(false);
        this.ChargesGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).EndInit();
        this.ChargesGrid.ResumeLayout(false);
        this.ChargesGrid.PerformLayout();
        this.ApportionedChargesGroupBox.ResumeLayout(false);
        this.ApportionedChargesGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
        this.ApportionedChargesGrid.ResumeLayout(false);
        this.ApportionedChargesGrid.PerformLayout();
        this.SplitContainer.Panel1.ResumeLayout(false);
        this.SplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
        this.SplitContainer.ResumeLayout(false);
        this.SplitContainer.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
}
