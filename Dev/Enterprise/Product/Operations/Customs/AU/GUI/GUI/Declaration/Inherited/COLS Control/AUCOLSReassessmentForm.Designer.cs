namespace Enterprise.Customs.AU.GUI
{
	partial class AUCOLSReassessmentForm
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
			this.DeclarationAgreementControl = new Enterprise.Customs.AU.GUI.DeclarationAgreementUserControl();
			this.RequireDocumentationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReassessmentReasonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SlashLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CharacterCountLimitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CharacterCountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CharacterCountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReassessmentReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeclarationAgreementControl.SuspendLayout();
			this.ReassessmentReasonGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 685, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.COLSDeclarationAcceptance);
			// 
			// DeclarationAgreementControl
			// 
			this.DeclarationAgreementControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationAgreementControl, ".");
			this.DeclarationAgreementControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 180, true);
			this.DeclarationAgreementControl.Name = "DeclarationAgreementControl";
			this.DeclarationAgreementControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 473, true);
			this.DeclarationAgreementControl.TabIndex = 3;
			// 
			// RequireDocumentationCheckBox
			// 
			this.RequireDocumentationCheckBox.Text = "Documentation Required";
			this.RequireDocumentationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 150, true);
			this.RequireDocumentationCheckBox.Name = "RequireDocumentationCheckBox";
			this.RequireDocumentationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 24, true);
			this.RequireDocumentationCheckBox.TabIndex = 2;
			// 
			// ReassessmentReasonGroupBox
			// 
			this.ReassessmentReasonGroupBox.Text = "Reassessment Reason (Mandatory)";
			this.ReassessmentReasonGroupBox.Controls.Add(this.SlashLabel);
			this.ReassessmentReasonGroupBox.Controls.Add(this.CharacterCountLimitTextBox);
			this.ReassessmentReasonGroupBox.Controls.Add(this.CharacterCountTextBox);
			this.ReassessmentReasonGroupBox.Controls.Add(this.CharacterCountLabel);
			this.ReassessmentReasonGroupBox.Controls.Add(this.ReassessmentReasonTextBox);
			this.ReassessmentReasonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.ReassessmentReasonGroupBox.Name = "ReassessmentReasonGroupBox";
			this.ReassessmentReasonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 139, true);
			this.ReassessmentReasonGroupBox.TabIndex = 0;
			this.ReassessmentReasonGroupBox.TabStop = false;
			// 
			// SlashLabel
			// 
			this.SlashLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SlashLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 118, true);
			this.SlashLabel.Name = "SlashLabel";
			this.SlashLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(9, 15, true);
			this.SlashLabel.TabIndex = 0;
			this.SlashLabel.Text = "/";
			this.SlashLabel.UseMnemonic = false;
			// 
			// CharacterCountLimitTextBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CharacterCountLimitTextBox, false);
			this.CharacterCountLimitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 118, true);
			this.CharacterCountLimitTextBox.Name = "CharacterCountLimitTextBox";
			this.CharacterCountLimitTextBox.ReadOnly = true;
			this.CharacterCountLimitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.CharacterCountLimitTextBox.TabIndex = 0;
			this.CharacterCountLimitTextBox.TabStop = false;
			this.CharacterCountLimitTextBox.Text = "1000";
			// 
			// CharacterCountTextBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CharacterCountTextBox, false);
			this.CharacterCountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 118, true);
			this.CharacterCountTextBox.Name = "CharacterCountTextBox";
			this.CharacterCountTextBox.ReadOnly = true;
			this.CharacterCountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.CharacterCountTextBox.TabIndex = 0;
			this.CharacterCountTextBox.TabStop = false;
			// 
			// CharacterCountLabel
			// 
			this.CharacterCountLabel.Text = "Character Count";
			this.CharacterCountLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CharacterCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 118, true);
			this.CharacterCountLabel.Name = "CharacterCountLabel";
			this.CharacterCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 15, true);
			this.CharacterCountLabel.TabIndex = 3;
			this.CharacterCountLabel.UseMnemonic = false;
			// 
			// ReassessmentReasonTextBox
			// 
			this.ReassessmentReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReassessmentReasonTextBox, false);
			this.ReassessmentReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ReassessmentReasonTextBox.Multiline = true;
			this.ReassessmentReasonTextBox.Name = "ReassessmentReasonTextBox";
			this.ReassessmentReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ReassessmentReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 98, true);
			this.ReassessmentReasonTextBox.TabIndex = 1;
			this.ReassessmentReasonTextBox.TextChanged += new System.EventHandler(this.ReassessmentReasonTextBox_TextChanged);
			// 
			// SendButton
			// 
			this.SendButton.Text = "Send";
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(423, 655, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 23, true);
			this.SendButton.TabIndex = 4;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 655, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 23, true);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// AUCOLSReassessmentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 709, true);
			this.Controls.Add(this.DeclarationAgreementControl);
			this.Controls.Add(this.RequireDocumentationCheckBox);
			this.Controls.Add(this.ReassessmentReasonGroupBox);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.cancelButton);
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.COLSDeclarationAcceptance);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "AUCOLSReassessmentForm";
			this.Text = "Request Reassessment";
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.ReassessmentReasonGroupBox, 0);
			this.Controls.SetChildIndex(this.RequireDocumentationCheckBox, 0);
			this.Controls.SetChildIndex(this.DeclarationAgreementControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeclarationAgreementControl.ResumeLayout(true);
			this.DeclarationAgreementControl.PerformLayout();
			this.ReassessmentReasonGroupBox.ResumeLayout(false);
			this.ReassessmentReasonGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal DeclarationAgreementUserControl DeclarationAgreementControl;
		internal ZArchitecture.GUI.ZCheckBox RequireDocumentationCheckBox;
		internal ZArchitecture.GUI.ZGroupBox ReassessmentReasonGroupBox;
		internal ZArchitecture.ZTextBox ReassessmentReasonTextBox;
		internal ZArchitecture.GUI.ZButton SendButton;
		internal ZArchitecture.GUI.ZButton cancelButton;
		internal ZArchitecture.ZTextBox CharacterCountTextBox;
		internal ZArchitecture.ZLabel CharacterCountLabel;
		internal ZArchitecture.ZTextBox CharacterCountLimitTextBox;
		private ZArchitecture.ZLabel SlashLabel;
	}
}
