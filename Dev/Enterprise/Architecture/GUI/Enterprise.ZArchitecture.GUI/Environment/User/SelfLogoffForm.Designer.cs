namespace Enterprise.ZArchitecture.GUI
{
	partial class SelfLogoffForm
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
			this.components = new System.ComponentModel.Container();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.exitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.label = new Enterprise.ZArchitecture.ZLabel();
			this.messageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 125, true);
			this.MainStatusBar.ShowPanels = false;
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// timer
			// 
			this.timer.Interval = 1000;
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// exitButton
			// 
			this.exitButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.exitButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("SelfLogoffForm|3557485a-a82c-4ed0-a73d-32c0e3cf2002", "Exit Now");
			this.exitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 114, true);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.exitButton.TabIndex = 1;
			this.exitButton.UseVisualStyleBackColor = true;
			this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
			// 
			// label
			// 
			this.label.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 83, true);
			this.label.Name = "label";
			this.label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 23, true);
			this.label.TabIndex = 0;
			this.label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// messageLabel
			// 
			this.messageLabel.BackColor = System.Drawing.SystemColors.Window;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.messageLabel, false);
			this.messageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 10, true);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 68, true);
			this.messageLabel.TabIndex = 3;
			// 
			// SelfLogoffForm
			// 
			this.AcceptButton = this.exitButton;
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 149, true);
			this.ControlBox = false;
			this.Controls.Add(this.messageLabel);
			this.Controls.Add(this.label);
			this.Controls.Add(this.exitButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "SelfLogoffForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.exitButton, 0);
			this.Controls.SetChildIndex(this.label, 0);
			this.Controls.SetChildIndex(this.messageLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Timer timer;
		private ZArchitecture.GUI.ZButton exitButton;
		private ZArchitecture.ZLabel label;
		private ZArchitecture.ZLabel messageLabel;
	}
}
