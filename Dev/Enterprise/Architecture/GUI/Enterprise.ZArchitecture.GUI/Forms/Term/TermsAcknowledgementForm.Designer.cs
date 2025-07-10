namespace Enterprise.ZArchitecture.GUI
{
	partial class TermsAcknowledgementForm
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
		protected new void InitializeComponent()
		{
			this.ForbiddenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContinueButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TermsContentRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.SendCopyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 764, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.BaseTermsAgreement);
			// 
			// TitleLabel
			// 
			this.TitleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TitleLabel, "Title");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.ZArchitecture.Business.BaseTermsAgreement)(null)).Title)));
			this.TitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)(((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.Bold) 
            | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TitleLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.TitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 4, true);
			this.TitleLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 20, 3, 5, true);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 28, true);
			this.TitleLabel.TabIndex = 0;
			this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TermsContentRichTextBox
			// 
			this.TermsContentRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TermsContentRichTextBox, "ContentsAsBlob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.ZArchitecture.Business.BaseTermsAgreement)(null)).ContentsAsBlob)));
			this.TermsContentRichTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.TermsContentRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TermsContentRichTextBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.TermsContentRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 37, true);
			this.TermsContentRichTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TermsContentRichTextBox.Name = "TermsContentRichTextBox";
			this.TermsContentRichTextBox.ReadOnly = true;
			this.TermsContentRichTextBox.IsToolBarVisible = false;
			this.TermsContentRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 702, true);
			this.TermsContentRichTextBox.TabIndex = 2;
			this.TermsContentRichTextBox.Text = "";
			// 
			// ForbiddenButton
			// 
			this.ForbiddenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ForbiddenButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("55ae00bf-cdbb-414b-b75c-cc2b79ff33e1", "Cancel");
			this.ForbiddenButton.IsCaptionOverridden = false;
			this.ForbiddenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 748, true);
			this.ForbiddenButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 10, 3, true);
			this.ForbiddenButton.Name = "ForbiddenButton";
			this.ForbiddenButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ForbiddenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 28, true);
			this.ForbiddenButton.TabIndex = 4;
			this.ForbiddenButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ForbiddenButton.ToolTipCaption = null;
			this.ForbiddenButton.UseVisualStyleBackColor = true;
			this.ForbiddenButton.Click += new System.EventHandler(this.ForbiddenButton_Click);
			// 
			// ContinueButton
			// 
			this.ContinueButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ContinueButton.BackColor = System.Drawing.Color.DeepSkyBlue;
			this.ContinueButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("e3f7c562-2df5-45d0-8208-b002ebf86789", "Agree");
			this.ContinueButton.IsCaptionOverridden = false;
			this.ContinueButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(501, 748, true);
			this.ContinueButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 3, 3, 3, true);
			this.ContinueButton.Name = "ContinueButton";
			this.ContinueButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ContinueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 28, true);
			this.ContinueButton.TabIndex = 5;
			this.ContinueButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ContinueButton.ToolTipCaption = null;
			this.ContinueButton.UseVisualStyleBackColor = false;
			this.ContinueButton.Click += new System.EventHandler(this.ContinueButton_Click);
			// 
			// SendCopyCheckBox
			// 
			this.SendCopyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.SendCopyCheckBox, "ShouldSendAgreementCopy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.ZArchitecture.Business.BaseTermsAgreement)(null)).ShouldSendAgreementCopy)));
			this.SendCopyCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("63362896-4ebe-4543-b191-a8b4bec964b9", "Email Agreement");
			this.SendCopyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendCopyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 749, true);
			this.SendCopyCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 3, 3, 3, true);
			this.SendCopyCheckBox.Name = "SendCopyCheckBox";
			this.SendCopyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 28, true);
			this.SendCopyCheckBox.TabIndex = 3;
			this.SendCopyCheckBox.UseVisualStyleBackColor = false;
			// 
			// TermsAcknowledgementForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("092bcb54-bb1c-485b-bfdc-f6f0beeb7b65", "Terms");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 788, true);
			this.Controls.Add(this.ForbiddenButton);
			this.Controls.Add(this.ContinueButton);
			this.Controls.Add(this.SendCopyCheckBox);
			this.Controls.Add(this.TermsContentRichTextBox);
			this.Controls.Add(this.TitleLabel);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.BaseTermsAgreement);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 725, true);
			this.Name = "TermsAcknowledgementForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Activated += new System.EventHandler(this.TermsAcknowledgementForm_Activated);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TitleLabel, 0);
			this.Controls.SetChildIndex(this.TermsContentRichTextBox, 0);
			this.Controls.SetChildIndex(this.SendCopyCheckBox, 0);
			this.Controls.SetChildIndex(this.ContinueButton, 0);
			this.Controls.SetChildIndex(this.ForbiddenButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZLabel TitleLabel;
		private ZRichTextBox TermsContentRichTextBox;
		private ZButton ContinueButton;
		private ZButton ForbiddenButton;
		private ZCheckBox SendCopyCheckBox;
	}
}