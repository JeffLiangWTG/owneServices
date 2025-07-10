using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
#if DEBUG
using Enterprise.ZArchitecture.Modules.Testing;
#endif
namespace Enterprise.Customs.CH.GUI;

partial class AdditionalFeesUserControl
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
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        this.AdditionalFeesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.AdditionalFeesGrid = new Enterprise.ZArchitecture.ZGrid();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.AdditionalFeesGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.AdditionalFeesGrid)).BeginInit();
        this.AdditionalFeesGrid.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.CusLineTariffDetailCollection);
        // 
        // AdditionalFeesGroupBox
        // 
        this.AdditionalFeesGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("83A73535-5FF8-4018-9C5B-BBABA33DE69D", "Additional Fees");
        this.AdditionalFeesGroupBox.Controls.Add(this.AdditionalFeesGrid);
        this.AdditionalFeesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.AdditionalFeesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.AdditionalFeesGroupBox.Name = "AdditionalFeesGroupBox";
        this.AdditionalFeesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 165, true);
        this.AdditionalFeesGroupBox.TabIndex = 0;
        this.AdditionalFeesGroupBox.TabStop = false;
        // 
        // AdditionalFeesGrid
        // 
        this.AdditionalFeesGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.AdditionalFeesGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)))));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).BZ_Tariff)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).Description)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).BZ_Qty1)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).BZ_ManualRate)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).BZ_Value)));
        this.AdditionalFeesGrid.CaptionVisible = false;
        zDropEditColumnStyleInfo1.ColumnName = "BZ_Tariff";
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo1.ColumnName = "Description";
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
        zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo1.ColumnName = "BZ_Qty1";
        zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo2.ColumnName = "BZ_ManualRate";
        zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo3.ColumnName = "BZ_Value";
        zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        this.AdditionalFeesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.AdditionalFeesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.AdditionalFeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
        this.AdditionalFeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
        this.AdditionalFeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
        this.AdditionalFeesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.AdditionalFeesGrid.GridId = "764626c7-765a-46d6-9563-295948230577";
        this.AdditionalFeesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.AdditionalFeesGrid.LayoutKey = "AdditionalFeesGrid";
        this.AdditionalFeesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
        this.AdditionalFeesGrid.Name = "AdditionalFeesGrid";
        this.AdditionalFeesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 146, true);
        this.AdditionalFeesGrid.TabIndex = 0;
        // 
        // AdditionalFeesUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.AdditionalFeesGroupBox);
        this.Name = "AdditionalFeesUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 165, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.AdditionalFeesGroupBox.ResumeLayout(false);
        this.AdditionalFeesGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.AdditionalFeesGrid)).EndInit();
        this.AdditionalFeesGrid.ResumeLayout(false);
        this.AdditionalFeesGrid.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZGroupBox AdditionalFeesGroupBox;
    internal ZGrid AdditionalFeesGrid;
}
