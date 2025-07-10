using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class AdditionalGoodsInformationUserControl
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
        this.AdditionalGoodsInformationTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
        this.SupernumeraryGoodsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.SupernumeraryGoodsUserControl = new Enterprise.Customs.CH.NCTS.GUI.SupernumeraryGoodsUserControl();
        this.AdditionalTransitOperationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
        this.AdditionalTransitOperationsUserControl = new Enterprise.Customs.CH.NCTS.GUI.AdditionalTransitOperationsUserControl();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.AdditionalGoodsInformationTabControl.SuspendLayout();
        this.SupernumeraryGoodsTabPage.SuspendLayout();
        this.SupernumeraryGoodsUserControl.SuspendLayout();
        this.AdditionalTransitOperationsTabPage.SuspendLayout();
        this.AdditionalTransitOperationsUserControl.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsHeader);
        // 
        // AdditionalGoodsInformationTabControl
        // 
        this.AdditionalGoodsInformationTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
        this.AdditionalGoodsInformationTabControl.Controls.Add(this.SupernumeraryGoodsTabPage);
        this.AdditionalGoodsInformationTabControl.Controls.Add(this.AdditionalTransitOperationsTabPage);
        this.AdditionalGoodsInformationTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.AdditionalGoodsInformationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.AdditionalGoodsInformationTabControl.Name = "AdditionalGoodsInformationTabControl";
        this.AdditionalGoodsInformationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
        this.AdditionalGoodsInformationTabControl.TabIndex = 0;
        // 
        // SupernumeraryGoodsTabPage
        // 
        this.SupernumeraryGoodsTabPage.CaptionResourceString = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("2AC8CEA8-6A2C-4F63-B3C2-89F622773AE9", "Additional Goods");
        this.SupernumeraryGoodsTabPage.Controls.Add(this.SupernumeraryGoodsUserControl);
        this.SupernumeraryGoodsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
        this.SupernumeraryGoodsTabPage.Name = "SupernumeraryGoodsTabPage";
        this.SupernumeraryGoodsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 78, true);
        this.SupernumeraryGoodsTabPage.TabIndex = 0;
        // 
        // SupernumeraryGoodsUserControl
        // 
        this.SupernumeraryGoodsUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.SupernumeraryGoodsUserControl, ".");
        this.SupernumeraryGoodsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SupernumeraryGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.SupernumeraryGoodsUserControl.Name = "SupernumeraryGoodsUserControl";
        this.SupernumeraryGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 78, true);
        this.SupernumeraryGoodsUserControl.TabIndex = 0;
        // 
        // AdditionalTransitOperationsTabPage
        // 
        this.AdditionalTransitOperationsTabPage.CaptionResourceString = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("D28E61BA-FC55-43A5-9D6D-35156FF84266", "Additional Transit Operations");
        this.AdditionalTransitOperationsTabPage.Controls.Add(this.AdditionalTransitOperationsUserControl);
        this.AdditionalTransitOperationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
        this.AdditionalTransitOperationsTabPage.Name = "AdditionalTransitOperationsTabPage";
        this.AdditionalTransitOperationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 78, true);
        this.AdditionalTransitOperationsTabPage.TabIndex = 1;
        // 
        // AdditionalTransitOperationsUserControl
        // 
        this.AdditionalTransitOperationsUserControl.AllowDrop = true;
        this.BindingSource.SetBindingMember(this.AdditionalTransitOperationsUserControl, ".");
        this.AdditionalTransitOperationsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.AdditionalTransitOperationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
        this.AdditionalTransitOperationsUserControl.Name = "AdditionalTransitOperationsUserControl";
        this.AdditionalTransitOperationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 78, true);
        this.AdditionalTransitOperationsUserControl.TabIndex = 0;
        // 
        // AdditionalGoodsInformationUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.AdditionalGoodsInformationTabControl);
        this.Name = "AdditionalGoodsInformationUserControl";
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.AdditionalGoodsInformationTabControl.ResumeLayout(false);
        this.AdditionalGoodsInformationTabControl.PerformLayout();
        this.SupernumeraryGoodsTabPage.ResumeLayout(false);
        this.SupernumeraryGoodsTabPage.PerformLayout();
        this.SupernumeraryGoodsUserControl.ResumeLayout(true);
        this.SupernumeraryGoodsUserControl.PerformLayout();
        this.AdditionalTransitOperationsTabPage.ResumeLayout(false);
        this.AdditionalTransitOperationsTabPage.PerformLayout();
        this.AdditionalTransitOperationsUserControl.ResumeLayout(true);
        this.AdditionalTransitOperationsUserControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal ZTabControl AdditionalGoodsInformationTabControl;
    internal ZTabPage SupernumeraryGoodsTabPage;
    internal SupernumeraryGoodsUserControl SupernumeraryGoodsUserControl;
    internal ZTabPage AdditionalTransitOperationsTabPage;
    internal AdditionalTransitOperationsUserControl AdditionalTransitOperationsUserControl;
}
