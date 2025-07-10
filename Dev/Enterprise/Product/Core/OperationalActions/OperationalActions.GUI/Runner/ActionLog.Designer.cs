namespace Enterprise.Services.OperationalActions.GUI
{
	partial class ActionLog
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZPanel spacer2;
			this.masterProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.sectionProgressBar = new CargoWise.Windows.UI.KProgressBar();
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
			spacer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 38, true);
			spacer2.Name = "spacer2";
			spacer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 3, true);
			spacer2.TabIndex = 4;
			// 
			// masterProgressBar
			// 
			this.masterProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
			this.masterProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.masterProgressBar.Name = "masterProgressBar";
			this.masterProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 16, true);
			this.masterProgressBar.TabIndex = 0;
			// 
			// sectionProgressBar
			// 
			this.sectionProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
			this.sectionProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.sectionProgressBar.Name = "sectionProgressBar";
			this.sectionProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 16, true);
			this.sectionProgressBar.TabIndex = 1;
			// 
			// spacer1
			// 
			this.spacer1.Dock = System.Windows.Forms.DockStyle.Top;
			this.spacer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.spacer1.Name = "spacer1";
			this.spacer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 3, true);
			this.spacer1.TabIndex = 2;
			// 
			// logTextBox
			// 
			this.logTextBox.DetectUrls = false;
			this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 41, true);
			this.logTextBox.MaxLength = 128000;
			this.logTextBox.Name = "logTextBox";
			this.logTextBox.ReadOnly = true;
			this.logTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 87, true);
			this.logTextBox.TabIndex = 3;
			this.logTextBox.Text = "";
			// 
			// debugLoggingLabel
			// 
			this.debugLoggingLabel.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("ActionLog|caa99ea0-dba8-46c3-b1a1-b1f3e359123f", "* Debug Logging Enabled *");
			this.debugLoggingLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.debugLoggingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 128, true);
			this.debugLoggingLabel.Name = "debugLoggingLabel";
			this.debugLoggingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 19, true);
			this.debugLoggingLabel.TabIndex = 5;
			this.debugLoggingLabel.Text = "* Debug Logging Enabled *";
			this.debugLoggingLabel.Visible = false;
			// 
			// ActionLog
			// 
			this.Controls.Add(this.logTextBox);
			this.Controls.Add(spacer2);
			this.Controls.Add(this.sectionProgressBar);
			this.Controls.Add(this.spacer1);
			this.Controls.Add(this.masterProgressBar);
			this.Controls.Add(this.debugLoggingLabel);
			this.Name = "ActionLog";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		private CargoWise.Windows.UI.KProgressBar masterProgressBar;
		private CargoWise.Windows.UI.KProgressBar sectionProgressBar;
		private Enterprise.ZArchitecture.GUI.ZPanel spacer1;
		private CargoWise.Windows.UI.KRichTextBox logTextBox;
		private Enterprise.ZArchitecture.ZLabel debugLoggingLabel;
	}
}
