namespace Enterprise.ZArchitecture.GUI.Internal
{
	public partial class EmbeddedModulePopup
	{
		protected ZPanel FilterControlPanel;
		protected ZPanel ButtonPanel;
#if DEBUG
		public
#endif
 ZPanel ToolBarPanel;
		ZPanel ToolbarRightPanel;
		CargoWise.Windows.UI.KToolStrip Toolstrip;
		ZPanel panelOkCancelButtons;
		protected ZButton OK_Button;
		ZButton Cancel_Button;

		new void InitializeComponent()
		{
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.panelOkCancelButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OK_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ToolBarPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ToolbarRightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Toolstrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.FilterControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonPanel.SuspendLayout();
			this.panelOkCancelButtons.SuspendLayout();
			this.ToolBarPanel.SuspendLayout();
			this.ToolbarRightPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 272, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 22, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.panelOkCancelButtons);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 235, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 37, true);
			this.ButtonPanel.TabIndex = 2;
			// 
			// panelOkCancelButtons
			// 
			this.panelOkCancelButtons.Controls.Add(this.OK_Button);
			this.panelOkCancelButtons.Controls.Add(this.Cancel_Button);
			this.panelOkCancelButtons.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelOkCancelButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(918, 0, true);
			this.panelOkCancelButtons.Name = "panelOkCancelButtons";
			this.panelOkCancelButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 37, true);
			this.panelOkCancelButtons.TabIndex = 1;
			// 
			// OK_Button
			// 
			this.OK_Button.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("EmbeddedModulePopup|dbc92c13-3561-4b8a-9c24-53fe08072017", "OK");
			this.OK_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.OK_Button.Name = "OK_Button";
			this.OK_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.OK_Button.TabIndex = 0;
			this.OK_Button.Click += new System.EventHandler(this.OK_Button_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("EmbeddedModulePopup|f5505cdd-eed0-4099-93b4-5edf5e83bb54", "Cancel");
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 7, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.Cancel_Button.TabIndex = 1;
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// ToolBarPanel
			// 
			this.ToolBarPanel.BackColor = System.Drawing.Color.Transparent;
			this.ToolBarPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ToolBarPanel.Controls.Add(this.ToolbarRightPanel);
			this.ToolBarPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToolBarPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolBarPanel.Name = "ToolBarPanel";
			this.ToolBarPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 32, true);
			this.ToolBarPanel.TabIndex = 0;
			// 
			// ToolbarRightPanel
			// 
			this.ToolbarRightPanel.Controls.Add(this.Toolstrip);
			this.ToolbarRightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ToolbarRightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolbarRightPanel.Name = "ToolbarRightPanel";
			this.ToolbarRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 32, true);
			this.ToolbarRightPanel.TabIndex = 0;
			// 
			// Toolstrip
			// 
			this.Toolstrip.BackColor = System.Drawing.Color.Transparent;
			this.Toolstrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Toolstrip.ForeColor = System.Drawing.SystemColors.WindowText;
			this.Toolstrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.Toolstrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Toolstrip.Name = "Toolstrip";
			this.Toolstrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 32, true);
			this.Toolstrip.TabIndex = 0;
			// 
			// FilterControlPanel
			// 
			this.FilterControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.FilterControlPanel.Name = "FilterControlPanel";
			this.FilterControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 203, true);
			this.FilterControlPanel.TabIndex = 1;
			// 
			// EmbeddedModulePopup
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 294, true);
			this.Controls.Add(this.FilterControlPanel);
			this.Controls.Add(this.ButtonPanel);
			this.Controls.Add(this.ToolBarPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 305, true);
			this.Name = "EmbeddedModulePopup";
			this.Controls.SetChildIndex(this.ToolBarPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonPanel, 0);
			this.Controls.SetChildIndex(this.FilterControlPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.panelOkCancelButtons.ResumeLayout(false);
			this.panelOkCancelButtons.PerformLayout();
			this.ToolBarPanel.ResumeLayout(false);
			this.ToolBarPanel.PerformLayout();
			this.ToolbarRightPanel.ResumeLayout(false);
			this.ToolbarRightPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
