namespace Enterprise.Customs.CA.DIF.GUI
{
	partial class DIFForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelOrCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DifUserControl = new Enterprise.Customs.CA.DIF.GUI.DIFUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.DifUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 659, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.DIF.Business.DIFHostWrapper);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.SendButton);
			this.MainPanel.Controls.Add(this.CancelOrCloseButton);
			this.MainPanel.Controls.Add(this.SaveButton);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 628, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 31, true);
			this.MainPanel.TabIndex = 1;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(475, 5, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 23, true);
			this.SendButton.TabIndex = 0;
			this.SendButton.CaptionResourceString = Enterprise.Customs.CA.DIF.GUI.Res.GetData("2b535fbb-68f1-45d2-b4f6-aee361e8a2a0", "&Send");
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelOrCloseButton
			// 
			this.CancelOrCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelOrCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelOrCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 5, true);
			this.CancelOrCloseButton.Name = "CancelOrCloseButton";
			this.CancelOrCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelOrCloseButton.TabIndex = 2;
			this.CancelOrCloseButton.CaptionResourceString = Enterprise.Customs.CA.DIF.GUI.Res.GetData("cb3a7b86-f851-4392-9556-fc2cbb60d0f6", "&Close");
			this.CancelOrCloseButton.ToolTipCaption = null;
			this.CancelOrCloseButton.UseVisualStyleBackColor = true;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(614, 5, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 1;
			this.SaveButton.CaptionResourceString = Enterprise.Customs.CA.DIF.GUI.Res.GetData("acfe327b-96c4-4fee-ad67-50a50d451a75", "S&ave");
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.UseVisualStyleBackColor = true;
			// 
			// DifUserControl
			// 
			this.DifUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DifUserControl, ".");
			this.DifUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DifUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DifUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 627, true);
			this.DifUserControl.Name = "DifUserControl";
			this.DifUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 628, true);
			this.DifUserControl.TabIndex = 2;
			// 
			// DIFForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelOrCloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 683, true);
			this.Controls.Add(this.DifUserControl);
			this.Controls.Add(this.MainPanel);
			this.DataSourceType = typeof(Enterprise.Customs.CA.DIF.Business.DIFHostWrapper);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 720, true);
			this.Name = "DIFForm";
			this.Text = "DIFForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.DifUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.DifUserControl.ResumeLayout(true);
			this.DifUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel MainPanel;
		internal ZArchitecture.GUI.ZButton SaveButton;
		internal DIFUserControl DifUserControl;
		internal ZArchitecture.GUI.ZButton SendButton;
		internal ZArchitecture.GUI.ZButton CancelOrCloseButton;
	}
}
