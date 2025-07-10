namespace Enterprise.Messaging.GUI
{
	partial class ActionLog
	{
		private void InitializeComponent()
		{
            Enterprise.ZArchitecture.GUI.ZPanel spacer2;
            this.spacer1 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.logTextBox = new CargoWise.Windows.UI.KRichTextBox();
            this.debugLoggingLabel = new Enterprise.ZArchitecture.ZLabel();
            spacer2 = new Enterprise.ZArchitecture.GUI.ZPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // spacer2
            // 
            spacer2.Dock = System.Windows.Forms.DockStyle.Top;
            spacer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 8, true);
            spacer2.Name = "spacer2";
            spacer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 3, true);
            spacer2.TabIndex = 4;
            // 
            // spacer1
            // 
            this.spacer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
            this.spacer1.Name = "spacer1";
            this.spacer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 3, true);
            this.spacer1.TabIndex = 2;
            // 
            // logTextBox
            // 
            this.logTextBox.DetectUrls = false;
            this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 12, true);
            this.logTextBox.MaxLength = 128000;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 58, true);
            this.logTextBox.TabIndex = 3;
            this.logTextBox.Text = "";
            // 
            // debugLoggingLabel
            // 
            this.debugLoggingLabel.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("DB5AE233-BB91-419E-9A16-1B74C195ACD1", "* Debug Logging Enabled *");
            this.debugLoggingLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.debugLoggingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.debugLoggingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 53, true);
            this.debugLoggingLabel.Name = "debugLoggingLabel";
            this.debugLoggingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 19, true);
            this.debugLoggingLabel.TabIndex = 5;
            this.debugLoggingLabel.Text = "* Debug Logging Enabled *";
            this.debugLoggingLabel.UseMnemonic = false;
            this.debugLoggingLabel.Visible = false;
            // 
            // ActionLog
            // 
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(spacer2);
            this.Controls.Add(this.spacer1);
            this.Controls.Add(this.debugLoggingLabel);
            this.Name = "ActionLog";
            this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private Enterprise.ZArchitecture.GUI.ZPanel spacer1;
		private CargoWise.Windows.UI.KRichTextBox logTextBox;
		private Enterprise.ZArchitecture.ZLabel debugLoggingLabel;
	}
}
