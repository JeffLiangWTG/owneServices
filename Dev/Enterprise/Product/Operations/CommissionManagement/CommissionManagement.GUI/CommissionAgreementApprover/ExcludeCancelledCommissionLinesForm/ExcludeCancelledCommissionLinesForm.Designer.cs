using System;

namespace Enterprise.CommissionManagement.GUI
{
	partial class ExcludeCancelledCommissionLinesForm
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
		public new void InitializeComponent()
		{
			this.FormCaptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AgreementsWithCancelledCommissionLinesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AgreementsWithCancelledCommissionLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContinueButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AgreementsWithCancelledCommissionLinesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 528, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1551, 24, true);
			// 
			// FormCaptionLabel
			// 
			this.FormCaptionLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.FormCaptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FormCaptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FormCaptionLabel.Name = "FormCaptionLabel";
			this.FormCaptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1551, 69, true);
			this.FormCaptionLabel.TabIndex = 5;
			// 
			// AgreementsWithCancelledCommissionLinesPanel
			// 
			this.AgreementsWithCancelledCommissionLinesPanel.AutoScroll = true;
			this.AgreementsWithCancelledCommissionLinesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AgreementsWithCancelledCommissionLinesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.AgreementsWithCancelledCommissionLinesPanel.Name = "AgreementsWithCancelledCommissionLinesPanel";
			this.AgreementsWithCancelledCommissionLinesPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 19, 0, 0, true);
			this.AgreementsWithCancelledCommissionLinesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1527, 415, true);
			this.AgreementsWithCancelledCommissionLinesPanel.TabIndex = 9;
			// 
			// AgreementsWithCancelledCommissionLinesGroupBox
			// 
			this.AgreementsWithCancelledCommissionLinesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AgreementsWithCancelledCommissionLinesGroupBox.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("11724640-2052-413c-85b8-79f732498b48", "Agreements With Canceled Commission Lines");
			this.AgreementsWithCancelledCommissionLinesGroupBox.Controls.Add(this.AgreementsWithCancelledCommissionLinesPanel);
			this.AgreementsWithCancelledCommissionLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 73, true);
			this.AgreementsWithCancelledCommissionLinesGroupBox.Name = "AgreementsWithCancelledCommissionLinesGroupBox";
			this.AgreementsWithCancelledCommissionLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1532, 432, true);
			this.AgreementsWithCancelledCommissionLinesGroupBox.TabIndex = 6;
			this.AgreementsWithCancelledCommissionLinesGroupBox.TabStop = false;
			// 
			// ContinueButton
			// 
			this.ContinueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ContinueButton.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("a0581ff2-8d7c-48ca-b988-2446b946e315", "Continue");
			this.ContinueButton.IsCaptionOverridden = false;
			this.ContinueButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1454, 505, true);
			this.ContinueButton.Name = "ContinueButton";
			this.ContinueButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ContinueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.ContinueButton.TabIndex = 8;
			this.ContinueButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ContinueButton.ToolTipCaption = null;
			this.ContinueButton.Click += ContinueButton_OnClick;
			// 
			// Cancel
			// 
			this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("8acb9609-a6fc-4aac-b826-94070e61f649", "Cancel");
			this.Cancel.IsCaptionOverridden = false;
			this.Cancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1362, 505, true);
			this.Cancel.Name = "Cancel";
			this.Cancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Cancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.Cancel.TabIndex = 7;
			this.Cancel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.Cancel.ToolTipCaption = null;
			this.Cancel.Click += Cancel_OnClick;
			// 
			// ExcludeCancelledCommissionLinesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.CommissionManagement.GUI.Res.GetData("558ca53b-62cb-4b11-b5df-0e645ad870e4", "Previously Canceled Commission Transactions");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1551, 552, true);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.ContinueButton);
			this.Controls.Add(this.AgreementsWithCancelledCommissionLinesGroupBox);
			this.Controls.Add(this.FormCaptionLabel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 320, true);
			this.Name = "ExcludeCancelledCommissionLinesForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FormCaptionLabel, 0);
			this.Controls.SetChildIndex(this.AgreementsWithCancelledCommissionLinesGroupBox, 0);
			this.Controls.SetChildIndex(this.ContinueButton, 0);
			this.Controls.SetChildIndex(this.Cancel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AgreementsWithCancelledCommissionLinesGroupBox.ResumeLayout(false);
			this.AgreementsWithCancelledCommissionLinesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		public ZArchitecture.GUI.ZButton ContinueButton;
		private ZArchitecture.GUI.ZButton Cancel;
		private ZArchitecture.GUI.ZGroupBox AgreementsWithCancelledCommissionLinesGroupBox;
		private ZArchitecture.ZLabel FormCaptionLabel;
		private ZArchitecture.GUI.ZPanel AgreementsWithCancelledCommissionLinesPanel;
	}
}
