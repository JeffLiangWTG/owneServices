namespace Enterprise.ZArchitecture.GUI
{
	partial class ProgressWithDetailesForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressWithDetailesForm));
			this.LogPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LogTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LogPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.LogPanel);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 378, true);
			this.MainPanel.TabIndex = 0;
			this.MainPanel.Controls.SetChildIndex(this.TopPanel, 0);
			this.MainPanel.Controls.SetChildIndex(this.BottomPanel, 0);
			this.MainPanel.Controls.SetChildIndex(this.LogPanel, 0);
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 52, true);
			this.TopPanel.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 39, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// EnterpriseLogo
			// 
			this.EnterpriseLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.EnterpriseLogo.Image = ((System.Drawing.Image)(resources.GetObject("EnterpriseLogo.Image")));
			this.EnterpriseLogo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 7, true);
			this.EnterpriseLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			// 
			// CancelProgressButton
			// 
			this.CancelProgressButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelProgressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 7, true);
			this.CancelProgressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 21, true);
			this.CancelProgressButton.TabIndex = 1;
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 9, true);
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 16, true);
			this.ProgressBar.TabIndex = 0;
			// 
			// ProgressLabel
			// 
			this.ProgressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 7, true);
			this.ProgressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 30, true);
			this.ProgressLabel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 371, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 7, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// LogPanel
			// 
			this.LogPanel.Controls.Add(this.LogTextBox);
			this.LogPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LogPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 91, true);
			this.LogPanel.Name = "LogPanel";
			this.LogPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 287, true);
			this.LogPanel.TabIndex = 2;
			// 
			// LogTextBox
			// 
			this.LogTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LogTextBox, false);
			this.LogTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LogTextBox.Multiline = true;
			this.LogTextBox.Name = "LogTextBox";
			this.LogTextBox.ReadOnly = true;
			this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.LogTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 287, true);
			this.LogTextBox.TabIndex = 0;
			// 
			// ProgressWithDetailesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 378, true);
			this.Name = "ProgressWithDetailesForm";
			this.Text = "ProgressWithDetailesForm";
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogo)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LogPanel.ResumeLayout(false);
			this.LogPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel LogPanel;
		private ZArchitecture.ZTextBox LogTextBox;
	}
}
