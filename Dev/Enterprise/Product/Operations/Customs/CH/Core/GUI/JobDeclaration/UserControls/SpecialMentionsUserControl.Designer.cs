
namespace Enterprise.Customs.CH.GUI;

partial class SpecialMentionsUserControl
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
        this.SpecialMentionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.SpecialMentionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.SpecialMentionsGroupBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.ISpecialMentions);
        // 
        // SpecialMentionsGroupBox
        // 
        this.SpecialMentionsGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("4d6284e6-6984-447e-af52-d43283606136", "Special Mentions");
        this.SpecialMentionsGroupBox.Controls.Add(this.SpecialMentionsTextBox);
        this.SpecialMentionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SpecialMentionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.SpecialMentionsGroupBox.Name = "SpecialMentionsGroupBox";
        this.SpecialMentionsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, 3, 6, 6, true);
        this.SpecialMentionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 156, true);
        this.SpecialMentionsGroupBox.TabIndex = 1;
        this.SpecialMentionsGroupBox.TabStop = false;
        // 
        // SpecialMentionsTextBox
        // 
        this.SpecialMentionsTextBox.AcceptsReturn = true;
        this.BindingSource.SetBindingMember(this.SpecialMentionsTextBox, "SpecialMentions");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.ISpecialMentions)(null)).SpecialMentions)));
        this.SpecialMentionsTextBox.CaptionResourceString = null;
        this.SpecialMentionsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SpecialMentionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
        this.SpecialMentionsTextBox.Multiline = true;
        this.SpecialMentionsTextBox.Name = "SpecialMentionsTextBox";
        this.SpecialMentionsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.SpecialMentionsTextBox.ShouldEscapeAllSpecialCharacters = false;
        this.SpecialMentionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 134, true);
        this.SpecialMentionsTextBox.TabIndex = 1;
        this.SpecialMentionsTextBox.WordWrap = false;
        // 
        // SpecialMentionsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.SpecialMentionsGroupBox);
        this.Name = "SpecialMentionsUserControl";
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.SpecialMentionsGroupBox.ResumeLayout(false);
        this.SpecialMentionsGroupBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private ZArchitecture.GUI.ZGroupBox SpecialMentionsGroupBox;
    private ZArchitecture.ZTextBox SpecialMentionsTextBox;
}
