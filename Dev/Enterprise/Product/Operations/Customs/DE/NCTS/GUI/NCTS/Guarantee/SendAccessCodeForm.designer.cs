namespace Enterprise.Customs.DE.NCTS.GUI
{
	partial class SendAccessCodeForm
	{
		new void InitializeComponent()
		{
            this.sendAccessCodeUserControl = new Enterprise.Customs.DE.NCTS.GUI.SendAccessCodeUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.sendAccessCodeUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 144, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.NCTS.Business.SendAccessCodeViewModel);
            // 
            // sendAccessCodeUserControl
            // 
            this.sendAccessCodeUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.sendAccessCodeUserControl, ".");
            this.sendAccessCodeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.sendAccessCodeUserControl.Name = "sendAccessCodeUserControl";
            this.sendAccessCodeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 137, true);
            this.sendAccessCodeUserControl.TabIndex = 1;
            // 
            // SendAccessCodeForm
            // 
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("1a42bbee-bdfc-4361-b08e-0f05bd68fe0e", "Send Access Code to Customs");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 168, true);
            this.Controls.Add(this.sendAccessCodeUserControl);
            this.DataSourceType = typeof(Enterprise.Customs.DE.NCTS.Business.SendAccessCodeViewModel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SendAccessCodeForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.sendAccessCodeUserControl, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.sendAccessCodeUserControl.ResumeLayout(true);
            this.sendAccessCodeUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private SendAccessCodeUserControl sendAccessCodeUserControl;
	}
}
