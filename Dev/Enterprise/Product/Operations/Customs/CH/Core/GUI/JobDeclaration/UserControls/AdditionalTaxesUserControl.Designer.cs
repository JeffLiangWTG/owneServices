namespace Enterprise.Customs.CH.GUI;

partial class AdditionalTaxesUserControl
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
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        this.AdditionalTaxesGrid = new Enterprise.ZArchitecture.ZGrid();
        this.AdditionalTaxesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.AdditionalTaxesGrid)).BeginInit();
        this.AdditionalTaxesGrid.SuspendLayout();
        this.AdditionalTaxesGroupBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.CusLineTariffDetailCollection);
        // 
        // AdditionalTaxesGrid
        // 
        this.AdditionalTaxesGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.AdditionalTaxesGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)))));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).BZ_TaxType)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).BZ_Tariff)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).BZ_AlcoholPercentage)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).BZ_Qty1)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).BZ_UQ1)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).BZ_BaseValue)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).BZ_ManualRate)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CusLineTariffDetail)(null)).BZ_Value)));
        this.AdditionalTaxesGrid.CaptionVisible = false;
        zTextBoxColumnStyleInfo1.ColumnName = "BZ_TaxType";
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zDropEditColumnStyleInfo1.ColumnName = "BZ_Tariff";
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo1.ColumnName = "BZ_AlcoholPercentage";
        zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo2.ColumnName = "BZ_Qty1";
        zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo2.ColumnName = "BZ_UQ1";
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo3.ColumnName = "BZ_BaseValue";
        zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo4.ColumnName = "BZ_ManualRate";
        zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo5.ColumnName = "BZ_Value";
        zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        this.AdditionalTaxesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.AdditionalTaxesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.AdditionalTaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
        this.AdditionalTaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
        this.AdditionalTaxesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.AdditionalTaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
        this.AdditionalTaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
        this.AdditionalTaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
        this.AdditionalTaxesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.AdditionalTaxesGrid.GridId = "927c69fd-767a-45c2-a9f2-16c42a5c734e";
        this.AdditionalTaxesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.AdditionalTaxesGrid.LayoutKey = "AdditionalTaxesGrid";
        this.AdditionalTaxesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 27, true);
        this.AdditionalTaxesGrid.Name = "AdditionalTaxesGrid";
        this.AdditionalTaxesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 135, true);
        this.AdditionalTaxesGrid.TabIndex = 0;
        // 
        // AdditionalTaxesGroupBox
        // 
        this.AdditionalTaxesGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("816945f7-272a-42b2-a616-c9a4ccf03575", "Additional Taxes");
        this.AdditionalTaxesGroupBox.Controls.Add(this.AdditionalTaxesGrid);
        this.AdditionalTaxesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
        this.AdditionalTaxesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.AdditionalTaxesGroupBox.Name = "AdditionalTaxesGroupBox";
        this.AdditionalTaxesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 165, true);
        this.AdditionalTaxesGroupBox.TabIndex = 0;
        this.AdditionalTaxesGroupBox.TabStop = false;
        // 
        // AdditionalTaxesUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.AdditionalTaxesGroupBox);
        this.Name = "AdditionalTaxesUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 165, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.AdditionalTaxesGrid)).EndInit();
        this.AdditionalTaxesGrid.ResumeLayout(false);
        this.AdditionalTaxesGrid.PerformLayout();
        this.AdditionalTaxesGroupBox.ResumeLayout(false);
        this.AdditionalTaxesGroupBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZGroupBox AdditionalTaxesGroupBox;
    internal ZArchitecture.ZGrid AdditionalTaxesGrid;
}
