namespace Enterprise.Customs.CH.GUI;

partial class NonCustomsLawUserControl
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
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        this.NonCustomsLawGrid = new Enterprise.ZArchitecture.ZGrid();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NonCustomsLawGrid)).BeginInit();
        this.NonCustomsLawGrid.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.NonCustomsLaw);
        // 
        // NonCustomsLawGrid
        // 
        this.NonCustomsLawGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.NonCustomsLawGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.NonCustomsLaw)(null)))));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.NonCustomsLaw)(null)).CSI_Code)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.NonCustomsLaw)(null)).CodeDescription)));
        this.NonCustomsLawGrid.CaptionVisible = false;
        zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        zTextBoxColumnStyleInfo1.ColumnName = "CodeDescription";
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
        this.NonCustomsLawGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.NonCustomsLawGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.NonCustomsLawGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.NonCustomsLawGrid.GridId = "3078edf6-3ab2-45df-a239-ae25d7e6921e";
        this.NonCustomsLawGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.NonCustomsLawGrid.LayoutKey = "NonCustomsLawGrid";
        this.NonCustomsLawGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.NonCustomsLawGrid.Name = "NonCustomsLawGrid";
        this.NonCustomsLawGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 107, true);
        this.NonCustomsLawGrid.TabIndex = 0;
        // 
        // NonCustomsLawUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.NonCustomsLawGrid);
        this.Name = "NonCustomsLawUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 107, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NonCustomsLawGrid)).EndInit();
        this.NonCustomsLawGrid.ResumeLayout(false);
        this.NonCustomsLawGrid.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.ZGrid NonCustomsLawGrid;
}
