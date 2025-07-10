namespace Enterprise.ZArchitecture.GUI.Scanning
{
	partial class ScanMessageUserControl
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
			this.MsgLabelOuterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MsgLabelInnerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HideMessageButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MsgLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MsgLabelOuterPanel.SuspendLayout();
			this.MsgLabelInnerPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MsgLabelOuterPanel
			// 
			this.MsgLabelOuterPanel.BackColor = System.Drawing.Color.Red;
			this.MsgLabelOuterPanel.Controls.Add(this.MsgLabelInnerPanel);
			this.MsgLabelOuterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MsgLabelOuterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MsgLabelOuterPanel.Name = "MsgLabelOuterPanel";
			this.MsgLabelOuterPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.MsgLabelOuterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1158, 29, true);
			this.MsgLabelOuterPanel.TabIndex = 20;
			// 
			// MsgLabelInnerPanel
			// 
			this.MsgLabelInnerPanel.Controls.Add(this.HideMessageButton);
			this.MsgLabelInnerPanel.Controls.Add(this.MsgLabel);
			this.MsgLabelInnerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MsgLabelInnerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.MsgLabelInnerPanel.Name = "MsgLabelInnerPanel";
			this.MsgLabelInnerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 27, true);
			this.MsgLabelInnerPanel.TabIndex = 20;
			// 
			// HideMessageButton
			// 
			this.HideMessageButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.HideMessageButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("3f6b2352-c977-496a-b5a4-bdb303d52793", "Hide");
			this.HideMessageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1105, 3, true);
			this.HideMessageButton.Name = "HideMessageButton";
			this.HideMessageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 21, true);
			this.HideMessageButton.TabIndex = 13;
			this.HideMessageButton.UseVisualStyleBackColor = true;
			this.HideMessageButton.Click += new System.EventHandler(this.HideMessageButton_Click);
			// 
			// MsgLabel
			// 
			this.MsgLabel.BackColor = System.Drawing.Color.MistyRose;
			this.MsgLabel.CaptionResourceString = null;
			this.MsgLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MsgLabel.ForeColor = System.Drawing.Color.Red;
			this.MsgLabel.IsFontBold = true;
			this.MsgLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MsgLabel.Name = "MsgLabel";
			this.MsgLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 27, true);
			this.MsgLabel.TabIndex = 12;
			this.MsgLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ScanMessageUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MsgLabelOuterPanel);
			this.Name = "ScanMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1158, 29, true);
			this.MsgLabelOuterPanel.ResumeLayout(false);
			this.MsgLabelInnerPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.GUI.ZPanel MsgLabelInnerPanel;
		protected ZArchitecture.GUI.ZPanel MsgLabelOuterPanel;
		protected ZArchitecture.GUI.ZButton HideMessageButton;
		protected ZArchitecture.ZLabel MsgLabel;
	}
}
