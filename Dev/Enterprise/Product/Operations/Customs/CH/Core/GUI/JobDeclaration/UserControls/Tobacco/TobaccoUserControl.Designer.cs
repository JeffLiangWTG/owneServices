namespace Enterprise.Customs.CH.GUI;

partial class TobaccoUserControl
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
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        this.TobaccoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.DynamicTobaccoPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
        this.TobaccosGrid = new Enterprise.ZArchitecture.ZGrid();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.TobaccoGroupBox.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.TobaccosGrid)).BeginInit();
        this.TobaccosGrid.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.Tobacco);
        // 
        // TobaccoGroupBox
        // 
        this.TobaccoGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("3e5bc539-e921-4c3c-963f-2c2b59fe5d84", "Tobacco");
        this.TobaccoGroupBox.Controls.Add(this.DynamicTobaccoPanel);
        this.TobaccoGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.TobaccoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
        this.TobaccoGroupBox.Name = "TobaccoGroupBox";
        this.TobaccoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 263, true);
        this.TobaccoGroupBox.TabIndex = 0;
        this.TobaccoGroupBox.TabStop = false;
        // 
        // DynamicTobaccoPanel
        // 
        this.DynamicTobaccoPanel.AllowDrop = true;
        this.DynamicTobaccoPanel.AutoScroll = true;
        this.DynamicTobaccoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.DynamicTobaccoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
        this.DynamicTobaccoPanel.Name = "DynamicTobaccoPanel";
        this.DynamicTobaccoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 246, true);
        this.DynamicTobaccoPanel.TabIndex = 0;
        // 
        // TobaccosGrid
        // 
        this.TobaccosGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.TobaccosGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.Tobacco)(null)))));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_Code)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_SubType)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_Description)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_ItemNumber)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_Value)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.Tobacco)(null)).CSI_AdditionalDescription)));
        this.TobaccosGrid.CaptionVisible = false;
        zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
        zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
        zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        zDropEditColumnStyleInfo2.ColumnName = "CSI_SubType";
        zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
        zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
        zTextBoxColumnStyleInfo1.ColumnName = "CSI_Description";
        zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
        zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo1.ColumnName = "CSI_ItemNumber";
        zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
        zCalcEditColumnStyleInfo1.ShowGroupSeparators = false;
        zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo2.ColumnName = "CSI_Value";
        zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
        zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zDropEditColumnStyleInfo3.ColumnName = "CSI_AdditionalDescription";
        zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
        zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        this.TobaccosGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
        this.TobaccosGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
        this.TobaccosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
        this.TobaccosGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
        this.TobaccosGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
        this.TobaccosGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
        this.TobaccosGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TobaccosGrid.GridId = "cb41cc33-7d16-49a5-8fc7-f664df2f05a6";
        this.TobaccosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.TobaccosGrid.LayoutKey = "TobaccosGrid";
        this.TobaccosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.TobaccosGrid.Name = "TobaccosGrid";
        this.TobaccosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 40, true);
        this.TobaccosGrid.TabIndex = 1;
        // 
        // TobaccoUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.TobaccosGrid);
        this.Controls.Add(this.TobaccoGroupBox);
        this.Name = "TobaccoUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 303, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.TobaccoGroupBox.ResumeLayout(false);
        this.TobaccoGroupBox.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.TobaccosGrid)).EndInit();
        this.TobaccosGrid.ResumeLayout(false);
        this.TobaccosGrid.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZGroupBox TobaccoGroupBox;
    internal ZArchitecture.ZGrid TobaccosGrid;
    internal ZArchitecture.GUI.DynamicLayoutPanel DynamicTobaccoPanel;
}
