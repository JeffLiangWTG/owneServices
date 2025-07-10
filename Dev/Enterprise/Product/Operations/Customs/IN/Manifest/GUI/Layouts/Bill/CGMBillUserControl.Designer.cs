namespace Enterprise.Customs.IN.Manifest.GUI;

partial class CGMBillUserControl
{
    void InitializeComponent()
    {
        this.BondDetailsUserControl = new Enterprise.Customs.IN.Manifest.GUI.CGMBondDetailsUserControl();
        this.FinalDestinationDetailsUserControl = new Enterprise.Customs.IN.Manifest.GUI.CGMFinalDestinationDetailsUserControl();
        this.TransshipmentDetailsUserControl = new Enterprise.Customs.IN.Manifest.GUI.CGMTransshipmentDetailsUserControl();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.BondDetailsUserControl.SuspendLayout();
        this.FinalDestinationDetailsUserControl.SuspendLayout();
        this.TransshipmentDetailsUserControl.SuspendLayout();
        this.SuspendLayout();
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Manifest.Business.CGMAsycudaBill);
        // 
        // BondDetailsUserControl
        // 
        this.BondDetailsUserControl.AllowDrop = true;
        this.BondDetailsUserControl.BackColor = System.Drawing.Color.Transparent;
        this.BindingSource.SetBindingMember(this.BondDetailsUserControl, ".");
        this.BondDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 67, true);
        this.BondDetailsUserControl.Name = "BondDetailsUserControl";
        this.BondDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 101, true);
        this.BondDetailsUserControl.TabIndex = 30;
        // 
        // FinalDestinationDetailsUserControl
        // 
        this.FinalDestinationDetailsUserControl.AllowDrop = true;
        this.FinalDestinationDetailsUserControl.BackColor = System.Drawing.Color.Transparent;
        this.BindingSource.SetBindingMember(this.FinalDestinationDetailsUserControl, ".");
        this.FinalDestinationDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(41, 235, true);
        this.FinalDestinationDetailsUserControl.Name = "FinalDestinationDetailsUserControl";
        this.FinalDestinationDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 20, true);
        this.FinalDestinationDetailsUserControl.TabIndex = 31;
        // 
        // TransshipmentDetailsUserControl
        // 
        this.TransshipmentDetailsUserControl.AllowDrop = true;
        this.TransshipmentDetailsUserControl.BackColor = System.Drawing.Color.Transparent;
        this.BindingSource.SetBindingMember(this.TransshipmentDetailsUserControl, ".");
        this.TransshipmentDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 183, true);
        this.TransshipmentDetailsUserControl.Name = "TransshipmentDetailsUserControl";
        this.TransshipmentDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 101, true);
        this.TransshipmentDetailsUserControl.TabIndex = 31;
        // 
        // CGMBillUserControl
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.BackColor = System.Drawing.SystemColors.Control;
        this.CaptionRenderingEnabled = true;
        this.Controls.Add(this.FinalDestinationDetailsUserControl);
        this.Controls.Add(this.TransshipmentDetailsUserControl);
        this.Controls.Add(this.BondDetailsUserControl);
        this.Name = "CGMBillUserControl";
        this.ShouldSerializeTabPageMethods = false;
        this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 385, true);
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.BondDetailsUserControl.ResumeLayout(true);
        this.BondDetailsUserControl.PerformLayout();
        this.FinalDestinationDetailsUserControl.ResumeLayout(true);
        this.FinalDestinationDetailsUserControl.PerformLayout();
        this.TransshipmentDetailsUserControl.ResumeLayout(true);
        this.TransshipmentDetailsUserControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    internal CGMBondDetailsUserControl BondDetailsUserControl;
    internal CGMFinalDestinationDetailsUserControl FinalDestinationDetailsUserControl;
    internal CGMTransshipmentDetailsUserControl TransshipmentDetailsUserControl;
}
