namespace Enterprise.Customs.CH.NCTS.GUI;

partial class HouseConsignmentDifferencesUserControl
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
        this.UnloadingRemarkTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.UnloadingRemarkCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.UnloadingRemarkCodeDropEdit.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsBill);
        // 
        // UnloadingRemarkTextTextBox
        // 
        this.BindingSource.SetBindingMember(this.UnloadingRemarkTextTextBox, "UnloadingRemarkText");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsBill)(null)).UnloadingRemarkText)));
        this.UnloadingRemarkTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.UnloadingRemarkTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
        this.UnloadingRemarkTextTextBox.Multiline = true;
        this.UnloadingRemarkTextTextBox.Name = "UnloadingRemarkTextTextBox";
        this.UnloadingRemarkTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 45, true);
        this.UnloadingRemarkTextTextBox.TabIndex = 5;
        // 
        // UnloadingRemarkCodeDropEdit
        // 
        this.UnloadingRemarkCodeDropEdit.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.UnloadingRemarkCodeDropEdit, "UnloadingRemarkCode");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.NCTS.Business.NctsBill)(null)).UnloadingRemarkCode)));
        this.UnloadingRemarkCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
        this.UnloadingRemarkCodeDropEdit.Name = "UnloadingRemarkCodeDropEdit";
        this.UnloadingRemarkCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
        this.UnloadingRemarkCodeDropEdit.TabIndex = 4;
        // 
        // HouseConsignmentDifferencesUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.Controls.Add(this.UnloadingRemarkTextTextBox);
        this.Controls.Add(this.UnloadingRemarkCodeDropEdit);
        this.Name = "HouseConsignmentDifferencesUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 80, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.UnloadingRemarkCodeDropEdit.ResumeLayout(true);
        this.UnloadingRemarkCodeDropEdit.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.ZTextBox UnloadingRemarkTextTextBox;
    internal ZArchitecture.GUI.ZDropEdit UnloadingRemarkCodeDropEdit;
}
