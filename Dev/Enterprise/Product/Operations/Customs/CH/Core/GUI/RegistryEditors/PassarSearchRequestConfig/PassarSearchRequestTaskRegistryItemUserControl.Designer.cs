namespace Enterprise.Customs.CH.GUI;

partial class PassarSearchRequestConfigRegistryItemUserControl
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
        this.EnabledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
        this.TimeLimitIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.PassarSearchRequestConfig);
        // 
        // EnabledCheckBox
        // 
        this.EnabledCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.BindingSource.SetBindingMember(this.EnabledCheckBox, "IsEnabled");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.PassarSearchRequestConfig)(null)).IsEnabled)));
        this.EnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.EnabledCheckBox.Name = "EnabledCheckBox";
        this.EnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 24, true);
        this.EnabledCheckBox.TabIndex = 0;
        // 
        // TimeLimitIntEdit
        // 
        this.BindingSource.SetBindingMember(this.TimeLimitIntEdit, "TimeLimit");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CH.Business.PassarSearchRequestConfig)(null)).TimeLimit)));
        this.TimeLimitIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 28, true);
        this.TimeLimitIntEdit.Name = "TimeLimitIntEdit";
        this.TimeLimitIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
        this.TimeLimitIntEdit.TabIndex = 1;
        // 
        // PassarSearchRequestConfigRegistryItemUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.EnabledCheckBox);
        this.Controls.Add(this.TimeLimitIntEdit);
        this.Name = "PassarSearchRequestConfigRegistryItemUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 81, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZCheckBox EnabledCheckBox;
    internal ZArchitecture.GUI.ZIntEdit TimeLimitIntEdit;
}
