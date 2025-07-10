using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class ArrivalSealsGridUserControl
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
        this.components = new System.ComponentModel.Container();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        this.SealsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
        this.SealsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.SealsGrid = new Enterprise.ZArchitecture.ZGrid();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.SealsTabControl.SuspendLayout();
        this.SealsTabPage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.SealsGrid)).BeginInit();
        this.SealsGrid.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.CusSealCollection);
        // 
        // SealsTabControl
        // 
        this.SealsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
        this.SealsTabControl.Controls.Add(this.SealsTabPage);
        this.SealsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SealsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.SealsTabControl.Name = "SealsTabControl";
        this.SealsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 133, true);
        this.SealsTabControl.TabIndex = 0;
        // 
        // SealsTabPage
        // 
        this.SealsTabPage.CaptionResourceString = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("4CD92EDA-3B04-48DB-A35A-56332AFE96BA", "Seals");
        this.SealsTabPage.Controls.Add(this.SealsGrid);
        this.SealsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
        this.SealsTabPage.Name = "SealsTabPage";
        this.SealsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
        this.SealsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 106, true);
        this.SealsTabPage.TabIndex = 0;
        // 
        // SealsGrid
        // 
        this.SealsGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.SealsGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.CusSeal)(null)))));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.CusSeal)(null)).BK_SequenceNumber)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.CusSeal)(null)).BK_UnloadingState)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.CusSeal)(null)).BK_SealNumber)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.CusSeal)(null)).UnloadingRemarksText)));
        this.SealsGrid.CaptionVisible = false;
        zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo1.ColumnName = "BK_SequenceNumber";
        zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
        zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zDropEditColumnStyleInfo1.ColumnName = "BK_UnloadingState";
        zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo1.ColumnName = "BK_SealNumber";
        zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo2.ColumnName = "UnloadingRemarksText";
        zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
        this.SealsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
        this.SealsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.SealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.SealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.SealsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SealsGrid.GridId = "59ad3f7d-3b5c-421e-ad1d-c4dd936a1e8c";
        this.SealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.SealsGrid.LayoutKey = "SealsGrid";
        this.SealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
        this.SealsGrid.Name = "SealsGrid";
        this.SealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 100, true);
        this.SealsGrid.TabIndex = 0;
        // 
        // ArrivalSealsGridUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.SealsTabControl);
        this.Name = "ArrivalSealsGridUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 133, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.SealsTabControl.ResumeLayout(false);
        this.SealsTabControl.PerformLayout();
        this.SealsTabPage.ResumeLayout(false);
        this.SealsTabPage.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.SealsGrid)).EndInit();
        this.SealsGrid.ResumeLayout(false);
        this.SealsGrid.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZTabControl SealsTabControl;
    internal ZArchitecture.GUI.ZTabPage SealsTabPage;
    public ZArchitecture.ZGrid SealsGrid;
}
