namespace Enterprise.ZArchitecture.GUI
{
	partial class ConfirmationDialogForm
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
		private new void InitializeComponent()
		{
			this.HintPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ButtonsPanel = new CargoWise.Windows.UI.KSplitContainer();
			this.OKDialogButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelDialogButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NotificationsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HintPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ButtonsPanel)).BeginInit();
			this.ButtonsPanel.Panel1.SuspendLayout();
			this.ButtonsPanel.Panel2.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 85, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Core.ConfirmationDialogDescriptor);
			// 
			// HintPanel
			// 
			this.HintPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.HintPanel.Controls.Add(this.HintLabel);
			this.HintPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HintPanel.Name = "HintPanel";
			this.HintPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 41, true);
			this.HintPanel.TabIndex = 1;
			// 
			// HintLabel
			// 
			this.BindingSource.SetBindingMember(this.HintLabel, "Text");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Core.ConfirmationDialogDescriptor)(null)).Text)));
			this.HintLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.HintLabel, false);
			this.HintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HintLabel.Name = "HintLabel";
			this.HintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 41, true);
			this.HintLabel.TabIndex = 0;
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsPanel.IsSplitterFixed = true;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 56, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			// 
			// ButtonsPanel.Panel1
			// 
			this.ButtonsPanel.Panel1.Controls.Add(this.OKDialogButton);
			// 
			// ButtonsPanel.Panel2
			// 
			this.ButtonsPanel.Panel2.Controls.Add(this.CancelDialogButton);
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 27, true);
			this.ButtonsPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(304);
			this.ButtonsPanel.TabIndex = 3;
			// 
			// OKDialogButton
			// 
			this.OKDialogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKDialogButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("5ccc0e1f-843d-41f0-9f0f-39752cbc8aa6", "OK", "Proceed with this operation");
			this.OKDialogButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKDialogButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 2, true);
			this.OKDialogButton.Name = "OKDialogButton";
			this.OKDialogButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 24, true);
			this.OKDialogButton.TabIndex = 0;
			this.OKDialogButton.UseVisualStyleBackColor = false;
			this.OKDialogButton.Click += new System.EventHandler(this.OKDialogButton_Click);
			// 
			// CancelDialogButton
			// 
			this.CancelDialogButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("d996d2de-5510-4857-b9e4-a37f2d30db1c", "Cancel", "Cancel this operation");
			this.CancelDialogButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelDialogButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.CancelDialogButton.Name = "CancelDialogButton";
			this.CancelDialogButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 24, true);
			this.CancelDialogButton.TabIndex = 1;
			this.CancelDialogButton.UseVisualStyleBackColor = false;
			this.CancelDialogButton.Click += new System.EventHandler(this.CancelDialogButton_Click);
			// 
			// NotificationsPanel
			// 
			this.NotificationsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.NotificationsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 42, true);
			this.NotificationsPanel.Name = "NotificationsPanel";
			this.NotificationsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 12, true);
			this.NotificationsPanel.TabIndex = 2;
			// 
			// ConfirmationDialogForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CancelButton = this.CancelDialogButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 109, true);
			this.Controls.Add(this.ButtonsPanel);
			this.Controls.Add(this.NotificationsPanel);
			this.Controls.Add(this.HintPanel);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Core.ConfirmationDialogDescriptor);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "ConfirmationDialogForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.HintPanel, 0);
			this.Controls.SetChildIndex(this.NotificationsPanel, 0);
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HintPanel.ResumeLayout(false);
			this.HintPanel.PerformLayout();
			this.ButtonsPanel.Panel1.ResumeLayout(false);
			this.ButtonsPanel.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ButtonsPanel)).EndInit();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel HintPanel;
		private CargoWise.Windows.UI.KSplitContainer ButtonsPanel;
		internal ZArchitecture.GUI.ZPanel NotificationsPanel;
		private ZArchitecture.ZLabel HintLabel;
		public ZArchitecture.GUI.ZButton OKDialogButton;
		public ZArchitecture.GUI.ZButton CancelDialogButton;
	}
}