using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class SupernumeraryGoodsUserControl
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
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
        this.SupernumeraryGoodsGrid = new Enterprise.ZArchitecture.ZGrid();
        this.SupernumeraryGoodsDetailsUserControl = new Enterprise.Customs.CH.NCTS.GUI.SupernumeraryGoodsDetailsUserControl();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
        this.SplitContainer.Panel1.SuspendLayout();
        this.SplitContainer.Panel2.SuspendLayout();
        this.SplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.SupernumeraryGoodsGrid)).BeginInit();
        this.SupernumeraryGoodsGrid.SuspendLayout();
        this.SupernumeraryGoodsDetailsUserControl.SuspendLayout();
        this.SuspendLayout();
        //
        // BindingSource
        //
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsHeader);
        //
        // SplitContainer
        //
        this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
        this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.SplitContainer.Name = "SplitContainer";
        this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
        //
        // SplitContainer.Panel1
        //
        this.SplitContainer.Panel1.Controls.Add(this.SupernumeraryGoodsGrid);
        //
        // SplitContainer.Panel2
        //
        this.SplitContainer.Panel2.Controls.Add(this.SupernumeraryGoodsDetailsUserControl);
        this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 351, true);
        this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(67);
        this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(133);
        this.SplitContainer.SplitterWidth = 4;
        this.SplitContainer.TabIndex = 0;
        //
        // SupernumeraryGoodsGrid
        //
        this.SupernumeraryGoodsGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.SupernumeraryGoodsGrid, "ArrivalMovementHeader.SupernumeraryGoods");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.SupernumeraryGoods)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.SupernumeraryGoods)).SyncRoot)).CSI_LineNo)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.SupernumeraryGoods)).SyncRoot)).CSI_Description)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.SupernumeraryGoods)).SyncRoot)).CSI_Quantity)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.SupernumeraryGoods)).SyncRoot)).CSI_UnitOfQuantity)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.SupernumeraryGoods)).SyncRoot)).CSI_Tariff)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.SupernumeraryGoods)).SyncRoot)).CSI_PackQty)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.SupernumeraryGoods)).SyncRoot)).CSI_PackType)));
        this.SupernumeraryGoodsGrid.CaptionVisible = false;
        zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
        zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zMultiLineTextBoxColumnInfo1.ColumnName = "CSI_Description";
        zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
        zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
        zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo2.ColumnName = "CSI_Quantity";
        zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("1a2dbf5e-1f35-4a4c-91b3-883676cf8acb", "Gross mass");
        zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo1.ColumnName = "CSI_UnitOfQuantity";
        zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("1a2dbf5e-1f35-4a4c-91b3-883676cf8acb", "Gross mass");
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        tariffColumnStyleInfo1.ColumnName = "CSI_Tariff";
        tariffColumnStyleInfo1.SelectNomenclatureModes = null;
        tariffColumnStyleInfo1.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
        tariffColumnStyleInfo1.TariffType = null;
        tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo3.ColumnName = "CSI_PackQty";
        zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
        zDropEditColumnStyleInfo1.ColumnName = "CSI_PackType";
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
        this.SupernumeraryGoodsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
        this.SupernumeraryGoodsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
        this.SupernumeraryGoodsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
        this.SupernumeraryGoodsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.SupernumeraryGoodsGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
        this.SupernumeraryGoodsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
        this.SupernumeraryGoodsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.SupernumeraryGoodsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SupernumeraryGoodsGrid.GridId = "79ce749a-2ee9-49c8-b932-f494415570d0";
        this.SupernumeraryGoodsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.SupernumeraryGoodsGrid.LayoutKey = "SupernumeraryGoodsGrid";
        this.SupernumeraryGoodsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.SupernumeraryGoodsGrid.Name = "SupernumeraryGoodsGrid";
        this.SupernumeraryGoodsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 133, true);
        this.SupernumeraryGoodsGrid.TabIndex = 0;
        //
        // SupernumeraryGoodsDetailsUserControl
        //
        this.SupernumeraryGoodsDetailsUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.SupernumeraryGoodsDetailsUserControl, "ArrivalMovementHeader.SupernumeraryGoods");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(((Enterprise.Customs.CH.NCTS.Business.SupernumeraryGoods)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.SupernumeraryGoods)).SyncRoot)))));
        this.SupernumeraryGoodsDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SupernumeraryGoodsDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.SupernumeraryGoodsDetailsUserControl.Name = "SupernumeraryGoodsDetailsUserControl";
        this.SupernumeraryGoodsDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 209, true);
        this.SupernumeraryGoodsDetailsUserControl.TabIndex = 0;
        //
        // SupernumeraryGoodsUserControl
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.SplitContainer);
        this.Name = "SupernumeraryGoodsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 351, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.SplitContainer.Panel1.ResumeLayout(false);
        this.SplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
        this.SplitContainer.ResumeLayout(false);
        this.SplitContainer.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.SupernumeraryGoodsGrid)).EndInit();
        this.SupernumeraryGoodsGrid.ResumeLayout(false);
        this.SupernumeraryGoodsGrid.PerformLayout();
        this.SupernumeraryGoodsDetailsUserControl.ResumeLayout(true);
        this.SupernumeraryGoodsDetailsUserControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
    internal ZGrid SupernumeraryGoodsGrid;
    internal SupernumeraryGoodsDetailsUserControl SupernumeraryGoodsDetailsUserControl;
}
