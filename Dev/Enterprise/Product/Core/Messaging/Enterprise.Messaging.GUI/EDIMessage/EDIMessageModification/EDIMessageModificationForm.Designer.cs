namespace Enterprise.Messaging.GUI
{
	partial class EDIMessageModificationForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessageModificationUserControl = new Enterprise.Messaging.GUI.EDIMessageModificationUserControl();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 331, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.EDIMessage);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.MessageModificationUserControl);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 303, true);
			this.MainPanel.TabIndex = 0;
			// 
			// MessageModificationUserControl
			// 
			this.BindingSource.SetBindingMember(this.MessageModificationUserControl, ".");
			this.MessageModificationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageModificationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageModificationUserControl.Name = "MessageModificationUserControl";
			this.MessageModificationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 303, true);
			this.MessageModificationUserControl.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.SaveButton);
			this.BottomPanel.Controls.Add(this.CloseButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 303, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 28, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 3, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 23, true);
			this.SaveButton.TabIndex = 0;
			this.SaveButton.UseVisualStyleBackColor = true;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 3, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// EDIMessageModificationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 355, true);
			this.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageModificationForm|907d0472-d692-4c29-9edc-45468e1bb40d", "Message Modification");
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Messaging.Business.EDIMessage);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 300, true);
			this.Name = "EDIMessageModificationForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.Messaging.GUI.EDIMessageModificationUserControl MessageModificationUserControl;

	}
}
