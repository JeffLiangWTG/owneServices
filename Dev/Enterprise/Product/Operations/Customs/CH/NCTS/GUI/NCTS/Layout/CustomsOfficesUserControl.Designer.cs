namespace Enterprise.Customs.CH.NCTS.GUI;

partial class CustomsOfficesUserControl
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
        Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        this.CustomsOfficesGrid = new Enterprise.ZArchitecture.ZGrid();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).BeginInit();
        this.CustomsOfficesGrid.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsHeader);
        // 
        // CustomsOfficesGrid
        // 
        this.CustomsOfficesGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.CustomsOfficesGrid, "MovementHeader.CustomsOfficesForDeparture");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)).SyncRoot)).CY_Order)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)).SyncRoot)).CY_Code)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)).SyncRoot)).Lookups.CodeList)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)).SyncRoot)).CY_Data)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)).SyncRoot)).CY_OfficeDescription)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.NCTS.Business.NctsEuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.CH.NCTS.Business.NctsHeader)(null)).MovementHeader.CustomsOfficesForDeparture)).SyncRoot)).EstimatedNumberOfDays)));
        this.CustomsOfficesGrid.CaptionText = "Customs Offices (Departure, Destination and Transit)";
        this.CustomsOfficesGrid.CaptionVisible = false;
        zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo1.ColumnName = "CY_Order";
        zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(59);
        zDropEditColumnStyleInfo1.BindToList = "Lookups.CodeList";
        zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
        zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
        zDropEditColumnStyleInfo1.IsMandatory = true;
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(59);
        zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Data";
        zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("E09C807B-10A4-4C6D-8312-6990CC6720DC", "Office Code");
        zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
        zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
        zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("E3AF0B19-0A90-4EED-924C-0D52408315B1", "Desc.", "Office Desc.", "Office Desc.", "Office Description");
        zTextBoxColumnStyleInfo1.ColumnName = "CY_OfficeDescription";
        zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("E09C807B-10A4-4C6D-8312-6990CC6720DC", "Office Code");
        zTextBoxColumnStyleInfo1.IsReadOnly = true;
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
        zTextBoxColumnStyleInfo2.ColumnName = "EstimatedNumberOfDays";
        zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(59);
        this.CustomsOfficesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
        this.CustomsOfficesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.CustomsOfficesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
        this.CustomsOfficesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.CustomsOfficesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
        this.CustomsOfficesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.CustomsOfficesGrid.GridId = "5EF82141-B52B-4C1E-BA2E-F2C5D30CE39D";
        this.CustomsOfficesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.CustomsOfficesGrid.LayoutKey = "CustomsOfficesGrid";
        this.CustomsOfficesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.CustomsOfficesGrid.Name = "CustomsOfficesGrid";
        this.CustomsOfficesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 117, true);
        this.CustomsOfficesGrid.TabIndex = 32;
        // 
        // CustomsOfficesUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.BackColor = System.Drawing.SystemColors.Control;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.CustomsOfficesGrid);
        this.Name = "CustomsOfficesUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 117, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).EndInit();
        this.CustomsOfficesGrid.ResumeLayout(false);
        this.CustomsOfficesGrid.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.ZGrid CustomsOfficesGrid;
}
