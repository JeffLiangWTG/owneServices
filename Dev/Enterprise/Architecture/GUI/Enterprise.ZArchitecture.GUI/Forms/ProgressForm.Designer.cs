namespace Enterprise.ZArchitecture.GUI
{
	public partial class ProgressForm
	{
		protected CargoWise.Windows.UI.KPanel MainPanel;
		protected Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		protected Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		protected Enterprise.ZArchitecture.GUI.ZPictureBox EnterpriseLogo;
		protected Enterprise.ZArchitecture.GUI.ZButton CancelProgressButton;
		protected CargoWise.Windows.UI.KProgressBar ProgressBar;
		protected Enterprise.ZArchitecture.ZLabel ProgressLabel;

		new void InitializeComponent()
		{
			this.MainPanel = new CargoWise.Windows.UI.KPanel();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CancelProgressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EnterpriseLogo = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.ProgressLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogo)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 84, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 7, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(165);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(165);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.BottomPanel);
			this.MainPanel.Controls.Add(this.TopPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 91, true);
			this.MainPanel.TabIndex = 4;
			// 
			// BottomPanel
			// 
			this.BottomPanel.BackColor = System.Drawing.SystemColors.Control;
			this.BottomPanel.Controls.Add(this.CancelProgressButton);
			this.BottomPanel.Controls.Add(this.ProgressBar);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 39, true);
			this.BottomPanel.TabIndex = 9;
			// 
			// CancelProgressButton
			// 
			this.CancelProgressButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.CancelProgressButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ProgressForm|9f93c5a0-1a9e-451f-a43d-f153315a7435", "Cancel");
			this.CancelProgressButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelProgressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(265, 7, true);
			this.CancelProgressButton.Name = "CancelProgressButton";
			this.CancelProgressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			this.CancelProgressButton.TabIndex = 4;
			this.CancelProgressButton.Click += new System.EventHandler(this.CancelProgressButton_Click);
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 16, true);
			this.ProgressBar.TabIndex = 6;
			// 
			// TopPanel
			// 
			this.TopPanel.BackColor = System.Drawing.SystemColors.Control;
			this.TopPanel.Controls.Add(this.EnterpriseLogo);
			this.TopPanel.Controls.Add(this.ProgressLabel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 52, true);
			this.TopPanel.TabIndex = 8;
			// 
			// EnterpriseLogo
			// 
			this.EnterpriseLogo.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.EnterpriseLogo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 7, true);
			this.EnterpriseLogo.Name = "EnterpriseLogo";
			this.EnterpriseLogo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.EnterpriseLogo.TabIndex = 7;
			this.EnterpriseLogo.TabStop = false;
			// 
			// ProgressLabel
			// 
			this.ProgressLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.ProgressLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ProgressForm|c35a1683-e882-4586-953d-d8e09902dcad", "This process can take some time...");
			this.ProgressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 7, true);
			this.ProgressLabel.Name = "ProgressLabel";
			this.ProgressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 30, true);
			this.ProgressLabel.TabIndex = 5;
			// 
			// ProgressForm
			// 

			this.CancelButton = this.CancelProgressButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 91, true);
			this.ControlBox = false;
			this.Controls.Add(this.MainPanel);
			this.MinimizeBox = false;
			this.Name = "ProgressForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("012133D7-4C59-4C42-8CA1-9B653E509CCA", "Please Wait...");
			this.Load += new System.EventHandler(this.ProgressForm_Load);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogo)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
