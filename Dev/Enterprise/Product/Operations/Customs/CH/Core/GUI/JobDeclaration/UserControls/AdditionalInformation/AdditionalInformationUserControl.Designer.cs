
namespace Enterprise.Customs.CH.GUI;

partial class AdditionalInformationUserControl
{
    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
        Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
        this.AdditionalInformationGrid = new Enterprise.ZArchitecture.ZGrid();
        this.AdditionalInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
        this.DynamicAdditionalInformationPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.AdditionalInformationGrid)).BeginInit();
        this.AdditionalInformationGrid.SuspendLayout();
        this.AdditionalInformationGroupBox.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.AdditionalInformationCollection);
        // 
        // AdditionalInformationGrid
        // 
        this.AdditionalInformationGrid.AllowNavigation = false;
        this.BindingSource.SetBindingMember(this.AdditionalInformationGrid, ".");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.AdditionalInformation)(null)))));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.AdditionalInformation)(null)).CSI_LineNo)));
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.AdditionalInformation)(null)).CSI_Code)));
        this.AdditionalInformationGrid.CaptionVisible = false;
        zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
        zCalcEditColumnStyleInfo2.ColumnName = "CSI_LineNo";
        zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
        zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        zDropEditColumnStyleInfo2.ColumnName = "CSI_Code";
        zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
        zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
        this.AdditionalInformationGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
        this.AdditionalInformationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
        this.AdditionalInformationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.AdditionalInformationGrid.GridId = "87c47089-3854-4be4-82f5-2eb8f1486ec9";
        this.AdditionalInformationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.AdditionalInformationGrid.LayoutKey = "AdditionalInfomrationGrid";
        this.AdditionalInformationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.AdditionalInformationGrid.Name = "AdditionalInformationGrid";
        this.AdditionalInformationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 30, true);
        this.AdditionalInformationGrid.TabIndex = 1;
        // 
        // AdditionalInformationGroupBox
        // 
        this.AdditionalInformationGroupBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("574591c1-f2cb-49ba-ba30-5927aa154474", "Additional Information");
        this.AdditionalInformationGroupBox.Controls.Add(this.DynamicAdditionalInformationPanel);
        this.AdditionalInformationGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.AdditionalInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
        this.AdditionalInformationGroupBox.Name = "AdditionalInformationGroupBox";
        this.AdditionalInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 140, true);
        this.AdditionalInformationGroupBox.TabIndex = 0;
        this.AdditionalInformationGroupBox.TabStop = false;
        // 
        // DynamicAdditionalInformationPanel
        // 
        this.DynamicAdditionalInformationPanel.AllowDrop = true;
        this.DynamicAdditionalInformationPanel.AutoScroll = true;
        this.DynamicAdditionalInformationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.DynamicAdditionalInformationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
        this.DynamicAdditionalInformationPanel.Name = "DynamicAdditionalInformationPanel";
        this.DynamicAdditionalInformationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 125, true);
        this.DynamicAdditionalInformationPanel.TabIndex = 0;
        // 
        // AdditionalInformationUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.AdditionalInformationGrid);
        this.Controls.Add(this.AdditionalInformationGroupBox);
        this.Name = "AdditionalInformationUserControl";
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 170, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.AdditionalInformationGrid)).EndInit();
        this.AdditionalInformationGrid.ResumeLayout(false);
        this.AdditionalInformationGrid.PerformLayout();
        this.AdditionalInformationGroupBox.ResumeLayout(false);
        this.AdditionalInformationGroupBox.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private ZArchitecture.GUI.ZGroupBox AdditionalInformationGroupBox;
    internal Enterprise.ZArchitecture.ZGrid AdditionalInformationGrid;
    internal Enterprise.ZArchitecture.GUI.DynamicLayoutPanel DynamicAdditionalInformationPanel;
}
