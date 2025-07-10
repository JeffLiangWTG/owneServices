using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class ExportDeclarationsUserControl
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
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        this.ExportDeclarationsModuleButtonGrid = new Enterprise.Customs.CH.NCTS.GUI.ExportDeclarationsModuleButtonGrid();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.ExportDeclarationsModuleButtonGrid.InnerGrid)).BeginInit();
        this.ExportDeclarationsModuleButtonGrid.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsHeader);
        // 
        // ExportDeclarationsModuleButtonGrid
        // 
        this.ExportDeclarationsModuleButtonGrid.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.ExportDeclarationsModuleButtonGrid, "MovementHeader.RelatedExportEntryHeaders");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).MovementHeader.RelatedExportEntryHeaders)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).MovementHeader.Lookups.ExportEntryHeaderCollection)));
        this.ExportDeclarationsModuleButtonGrid.BindToFindBoxList = "MovementHeader.Lookups.ExportEntryHeaderCollection";
        zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        zCalcEditColumnStyleInfo1.ColumnName = "XX_Sequence";
        zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
        zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        zTextBoxColumnStyleInfo1.ColumnName = "ShipmentType";
        zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo2.ColumnName = "EntryNumber";
        zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
        zTextBoxColumnStyleInfo3.ColumnName = "JobNumber";
        zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo4.ColumnName = "ReferenceNumber";
        zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
        zTextBoxColumnStyleInfo5.ColumnName = "EntryStatus";
        zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zTextBoxColumnStyleInfo6.ColumnName = "EntryStatusDescription";
        zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
        this.ExportDeclarationsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
        this.ExportDeclarationsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.ExportDeclarationsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.ExportDeclarationsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
        this.ExportDeclarationsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
        this.ExportDeclarationsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
        this.ExportDeclarationsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
        this.ExportDeclarationsModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ExportDeclarationsModuleButtonGrid.GridId = null;
        // 
        // 
        // 
        this.ExportDeclarationsModuleButtonGrid.InnerGrid.GridId = null;
        this.ExportDeclarationsModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.ExportDeclarationsModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
        this.ExportDeclarationsModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
        this.ExportDeclarationsModuleButtonGrid.InnerGrid.Name = "Grid";
        this.ExportDeclarationsModuleButtonGrid.InnerGrid.ReadOnly = true;
        this.ExportDeclarationsModuleButtonGrid.InnerGrid.TabIndex = 0;
        this.ExportDeclarationsModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.ExportDeclarationsModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.EntryHeader;
        this.ExportDeclarationsModuleButtonGrid.Name = "ExportDeclarationsModuleButtonGrid";
        this.ExportDeclarationsModuleButtonGrid.ReadOnly = true;
        this.ExportDeclarationsModuleButtonGrid.ShowEditButton = false;
        this.ExportDeclarationsModuleButtonGrid.ShowNewButton = false;
        this.ExportDeclarationsModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 211, true);
        this.ExportDeclarationsModuleButtonGrid.TabIndex = 0;
        // 
        // ExportDeclarationsUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.ExportDeclarationsModuleButtonGrid);
        this.Name = "ExportDeclarationsUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 211, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.ExportDeclarationsModuleButtonGrid.InnerGrid)).EndInit();
        this.ExportDeclarationsModuleButtonGrid.ResumeLayout(true);
        this.ExportDeclarationsModuleButtonGrid.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ExportDeclarationsModuleButtonGrid ExportDeclarationsModuleButtonGrid;
}
