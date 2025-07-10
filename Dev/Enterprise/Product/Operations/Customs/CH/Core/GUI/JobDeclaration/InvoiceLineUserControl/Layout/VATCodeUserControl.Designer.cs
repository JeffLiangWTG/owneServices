
namespace Enterprise.Customs.CH.GUI;

partial class VATCodeUserControl
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
        this.VATCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.VATCodeConfirmationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.VATCodeDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobComInvoiceLine);
        // 
        // VATCodeDropEdit
        // 
        this.VATCodeDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.VATCodeDropEdit, "JI_ZZF_NKTaxType");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_ZZF_NKTaxType)));
        this.VATCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.VATCodeDropEdit.Name = "VATCodeDropEdit";
        this.VATCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
        this.VATCodeDropEdit.TabIndex = 0;
        // 
        // VATCodeConfirmationCheckBox
        // 
        this.BindingSource.SetBindingMember(this.VATCodeConfirmationCheckBox, "JI_VATCodeConfirmation");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_VATCodeConfirmation)));
        this.VATCodeConfirmationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.VATCodeConfirmationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 0, true);
        this.VATCodeConfirmationCheckBox.Name = "VATCodeConfirmationCheckBox";
        this.VATCodeConfirmationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
        this.VATCodeConfirmationCheckBox.TabIndex = 1;
        this.VATCodeConfirmationCheckBox.UseCompatibleTextRendering = true;
        this.VATCodeConfirmationCheckBox.UseVisualStyleBackColor = true;
        // 
        // VATCodeUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.AutoSize = true;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.VATCodeConfirmationCheckBox);
        this.Controls.Add(this.VATCodeDropEdit);
        this.Name = "VATCodeUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 23, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.VATCodeDropEdit.ResumeLayout(true);
        this.VATCodeDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private Enterprise.ZArchitecture.GUI.ZDropEdit VATCodeDropEdit;
    private Enterprise.ZArchitecture.GUI.ZCheckBox VATCodeConfirmationCheckBox;
}
