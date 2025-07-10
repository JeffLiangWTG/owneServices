namespace Enterprise.Customs.CH.GUI;

partial class DispatchCountryUserControl
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
        this.DispatchCountryTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.DispatchCountryConfirmationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobDeclaration);
        // 
        // DispatchCountryTextBox
        // 
        this.BindingSource.SetBindingMember(this.DispatchCountryTextBox, "DispatchCountryCode");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).DispatchCountryCode)));
        this.DispatchCountryTextBox.CaptionResourceString = null;
        this.DispatchCountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.DispatchCountryTextBox.Name = "DispatchCountryTextBox";
        this.DispatchCountryTextBox.ShouldEscapeAllSpecialCharacters = false;
        this.DispatchCountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
        this.DispatchCountryTextBox.TabIndex = 0;
        // 
        // DispatchCountryConfirmationCheckBox
        // 
        this.DispatchCountryConfirmationCheckBox.AutoSize = true;
        this.BindingSource.SetBindingMember(this.DispatchCountryConfirmationCheckBox, "JE_DispatchCountryConfirmation");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).JE_DispatchCountryConfirmation)));
        this.DispatchCountryConfirmationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.DispatchCountryConfirmationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 3, true);
        this.DispatchCountryConfirmationCheckBox.Name = "DispatchCountryConfirmationCheckBox";
        this.DispatchCountryConfirmationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
        this.DispatchCountryConfirmationCheckBox.TabIndex = 1;
        this.DispatchCountryConfirmationCheckBox.UseVisualStyleBackColor = true;
        // 
        // DispatchCountryUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.DispatchCountryConfirmationCheckBox);
        this.Controls.Add(this.DispatchCountryTextBox);
        this.Name = "DispatchCountryUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.ZTextBox DispatchCountryTextBox;
    internal ZArchitecture.GUI.ZCheckBox DispatchCountryConfirmationCheckBox;
}
