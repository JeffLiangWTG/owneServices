using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class AdditionalTransitOperationsUserControl
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
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
        this.AdditionalTransitOperationsGrid = new Enterprise.ZArchitecture.ZGrid();
        this.AdditionalTransitOperationDetailsUserControl = new Enterprise.Customs.CH.NCTS.GUI.AdditionalTransitOperationDetailsUserControl();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
        this.SplitContainer.Panel1.SuspendLayout();
        this.SplitContainer.Panel2.SuspendLayout();
        this.SplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.AdditionalTransitOperationsGrid)).BeginInit();
        this.AdditionalTransitOperationsGrid.SuspendLayout();
        this.AdditionalTransitOperationDetailsUserControl.SuspendLayout();
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
        this.SplitContainer.Panel1.Controls.Add(this.AdditionalTransitOperationsGrid);
        // 
        // SplitContainer.Panel2
        // 
        this.SplitContainer.Panel2.Controls.Add(this.AdditionalTransitOperationDetailsUserControl);
        this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 351, true);
        this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(67);
        this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(89);
        this.SplitContainer.TabIndex = 0;
        // 
        // AdditionalTransitOperationsGrid
        // 
        this.AdditionalTransitOperationsGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.AdditionalTransitOperationsGrid, "ArrivalMovementHeader.AdditionalTransitOperations");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AdditionalTransitOperations)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AdditionalTransitOperations)).SyncRoot)).CSI_LineNo)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AdditionalTransitOperations)).SyncRoot)).CSI_IssuerType)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AdditionalTransitOperations)).SyncRoot)).CSI_ReferenceNumber)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AdditionalTransitOperations)).SyncRoot)).CSI_Description)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AdditionalTransitOperations)).SyncRoot)).CSI_Quantity)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AdditionalTransitOperations)).SyncRoot)).CSI_UnitOfQuantity)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AdditionalTransitOperations)).SyncRoot)).CSI_PackQty)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AdditionalTransitOperations)).SyncRoot)).CSI_PackType)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AdditionalTransitOperations)).SyncRoot)).CSI_Status)));
        this.AdditionalTransitOperationsGrid.CaptionVisible = false;
        zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
        zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zDropEditColumnStyleInfo1.ColumnName = "CSI_IssuerType";
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
        zMultiLineTextBoxColumnInfo1.ColumnName = "CSI_Description";
        zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
        zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
        zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo2.ColumnName = "CSI_Quantity";
        zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("87694da6-d1db-4dbd-988b-72a13de0637e", "Gross mass");
        zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo2.ColumnName = "CSI_UnitOfQuantity";
        zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("87694da6-d1db-4dbd-988b-72a13de0637e", "Gross mass");
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo3.ColumnName = "CSI_PackQty";
        zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
        zDropEditColumnStyleInfo2.ColumnName = "CSI_PackType";
        zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
        zDropEditColumnStyleInfo3.ColumnName = "CSI_Status";
        zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        this.AdditionalTransitOperationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
        this.AdditionalTransitOperationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.AdditionalTransitOperationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.AdditionalTransitOperationsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
        this.AdditionalTransitOperationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
        this.AdditionalTransitOperationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.AdditionalTransitOperationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
        this.AdditionalTransitOperationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
        this.AdditionalTransitOperationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
        this.AdditionalTransitOperationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.AdditionalTransitOperationsGrid.GridId = "55cdb483-013e-4531-96db-2cf997fc3752";
        this.AdditionalTransitOperationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.AdditionalTransitOperationsGrid.LayoutKey = "AdditionalTransitOperationGrid";
        this.AdditionalTransitOperationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.AdditionalTransitOperationsGrid.Name = "AdditionalTransitOperationsGrid";
        this.AdditionalTransitOperationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 89, true);
        this.AdditionalTransitOperationsGrid.TabIndex = 0;
        // 
        // AdditionalTransitOperationDetailsUserControl
        // 
        this.AdditionalTransitOperationDetailsUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.AdditionalTransitOperationDetailsUserControl, "ArrivalMovementHeader.AdditionalTransitOperations");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(((Enterprise.Customs.CH.NCTS.Business.AdditionalTransitOperation)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.AdditionalTransitOperations)).SyncRoot)))));
        this.AdditionalTransitOperationDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.AdditionalTransitOperationDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.AdditionalTransitOperationDetailsUserControl.Name = "AdditionalTransitOperationDetailsUserControl";
        this.AdditionalTransitOperationDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 258, true);
        this.AdditionalTransitOperationDetailsUserControl.TabIndex = 0;
        // 
        // AdditionalTransitOperationsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.SplitContainer);
        this.Name = "AdditionalTransitOperationsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 351, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.SplitContainer.Panel1.ResumeLayout(false);
        this.SplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
        this.SplitContainer.ResumeLayout(false);
        this.SplitContainer.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.AdditionalTransitOperationsGrid)).EndInit();
        this.AdditionalTransitOperationsGrid.ResumeLayout(false);
        this.AdditionalTransitOperationsGrid.PerformLayout();
        this.AdditionalTransitOperationDetailsUserControl.ResumeLayout(true);
        this.AdditionalTransitOperationDetailsUserControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
    internal ZGrid AdditionalTransitOperationsGrid;
    internal AdditionalTransitOperationDetailsUserControl AdditionalTransitOperationDetailsUserControl;
}
