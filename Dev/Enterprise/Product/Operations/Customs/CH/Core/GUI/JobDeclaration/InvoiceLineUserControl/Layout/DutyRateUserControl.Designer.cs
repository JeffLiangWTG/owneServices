
namespace Enterprise.Customs.CH.GUI;

partial class DutyRateUserControl
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
        this.DutyRateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        this.DutyRateConfirmationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        this.DutyRateTextBox = new Enterprise.ZArchitecture.ZTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.DutyRateDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobComInvoiceLine);
        // 
        // DutyRateDropEdit
        // 
        this.DutyRateDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.DutyRateDropEdit, "DutyRateDescription");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).DutyRateDescription)));
        this.DutyRateDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.DutyRateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.DutyRateDropEdit.Name = "DutyRateDropEdit";
        this.DutyRateDropEdit.ShouldResizeByMaxLength = false;
        this.DutyRateDropEdit.ShowDescriptionBox = false;
        this.DutyRateDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
        this.DutyRateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
        this.DutyRateDropEdit.TabIndex = 0;
        // 
        // DutyRateConfirmationCheckBox
        // 
        this.BindingSource.SetBindingMember(this.DutyRateConfirmationCheckBox, "DutyRateConfirmation");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).DutyRateConfirmation)));
        this.DutyRateConfirmationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 0, true);
        this.DutyRateConfirmationCheckBox.Name = "DutyRateConfirmationCheckBox";
        this.DutyRateConfirmationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
        this.DutyRateConfirmationCheckBox.TabIndex = 1;
        this.DutyRateConfirmationCheckBox.UseCompatibleTextRendering = true;
        this.DutyRateConfirmationCheckBox.UseVisualStyleBackColor = true;
        // 
        // DutyRateTextBox
        // 
        this.BindingSource.SetBindingMember(this.DutyRateTextBox, "DutyRateFormulaNumber");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).DutyRateFormulaNumber)));
        this.DutyRateTextBox.CaptionResourceString = null;
        this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DutyRateTextBox, false);
        this.DutyRateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 0, true);
        this.DutyRateTextBox.Name = "DutyRateTextBox";
        this.DutyRateTextBox.ReadOnly = true;
        this.DutyRateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
        this.DutyRateTextBox.TabIndex = 1;
        // 
        // DutyRateUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.AutoSize = true;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.DutyRateDropEdit);
        this.Controls.Add(this.DutyRateTextBox);
        this.Controls.Add(this.DutyRateConfirmationCheckBox);
        this.Name = "DutyRateUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 161, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.DutyRateDropEdit.ResumeLayout(true);
        this.DutyRateDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal Enterprise.ZArchitecture.GUI.ZDropEdit DutyRateDropEdit;
    internal Enterprise.ZArchitecture.GUI.ZCheckBox DutyRateConfirmationCheckBox;
    internal Enterprise.ZArchitecture.ZTextBox DutyRateTextBox;
}
