namespace Enterprise.Customs.AU.GUI
{
	partial class DeclarationAgreementUserControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeclarationAgreementUserControl));
			this.GeneralDeclarationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PrivacyDeclarationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalCommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GeneralDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PrivacyDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalCommentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SlashLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CharacterCountLimitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CharacterCountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CharacterCountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AcceptCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GeneralDeclarationGroupBox.SuspendLayout();
			this.PrivacyDeclarationGroupBox.SuspendLayout();
			this.AdditionalCommentGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.COLSDeclarationAcceptance);
			// 
			// GeneralDeclarationTextBox
			// 
			this.GeneralDeclarationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GeneralDeclarationTextBox, false);
			this.GeneralDeclarationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.GeneralDeclarationTextBox.Multiline = true;
			this.GeneralDeclarationTextBox.Name = "GeneralDeclarationTextBox";
			this.GeneralDeclarationTextBox.ReadOnly = true;
			this.GeneralDeclarationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 70, true);
			this.GeneralDeclarationTextBox.TabIndex = 1;
			this.GeneralDeclarationTextBox.Text = resources.GetString("GeneralDeclarationTextBox.Text");
			// 
			// PrivacyDeclarationTextBox
			// 
			this.PrivacyDeclarationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PrivacyDeclarationTextBox, false);
			this.PrivacyDeclarationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.PrivacyDeclarationTextBox.Multiline = true;
			this.PrivacyDeclarationTextBox.Name = "PrivacyDeclarationTextBox";
			this.PrivacyDeclarationTextBox.ReadOnly = true;
			this.PrivacyDeclarationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 176, true);
			this.PrivacyDeclarationTextBox.TabIndex = 1;
			this.PrivacyDeclarationTextBox.Text = resources.GetString("PrivacyDeclarationTextBox.Text");
			// 
			// AdditionalCommentTextBox
			// 
			this.AdditionalCommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalCommentTextBox, false);
			this.AdditionalCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.AdditionalCommentTextBox.Multiline = true;
			this.AdditionalCommentTextBox.Name = "AdditionalCommentTextBox";
			this.AdditionalCommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.AdditionalCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 98, true);
			this.AdditionalCommentTextBox.TabIndex = 1;
			this.AdditionalCommentTextBox.TextChanged += new System.EventHandler(this.AdditionalCommentTextBox_TextChanged);
			// 
			// GeneralDeclarationGroupBox
			// 
			this.GeneralDeclarationGroupBox.Controls.Add(this.GeneralDeclarationTextBox);
			this.GeneralDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 1, true);
			this.GeneralDeclarationGroupBox.Name = "GeneralDeclarationGroupBox";
			this.GeneralDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 92, true);
			this.GeneralDeclarationGroupBox.TabIndex = 1;
			this.GeneralDeclarationGroupBox.TabStop = false;
			this.GeneralDeclarationGroupBox.Text = "General Declaration";
			// 
			// PrivacyDeclarationGroupBox
			// 
			this.PrivacyDeclarationGroupBox.Controls.Add(this.PrivacyDeclarationTextBox);
			this.PrivacyDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 102, true);
			this.PrivacyDeclarationGroupBox.Name = "PrivacyDeclarationGroupBox";
			this.PrivacyDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 199, true);
			this.PrivacyDeclarationGroupBox.TabIndex = 2;
			this.PrivacyDeclarationGroupBox.TabStop = false;
			this.PrivacyDeclarationGroupBox.Text = "Privacy Declaration";
			// 
			// AdditionalCommentGroupBox
			// 
			this.AdditionalCommentGroupBox.Controls.Add(this.SlashLabel);
			this.AdditionalCommentGroupBox.Controls.Add(this.CharacterCountLimitTextBox);
			this.AdditionalCommentGroupBox.Controls.Add(this.CharacterCountTextBox);
			this.AdditionalCommentGroupBox.Controls.Add(this.CharacterCountLabel);
			this.AdditionalCommentGroupBox.Controls.Add(this.AdditionalCommentTextBox);
			this.AdditionalCommentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 331, true);
			this.AdditionalCommentGroupBox.Name = "AdditionalCommentGroupBox";
			this.AdditionalCommentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(574, 139, true);
			this.AdditionalCommentGroupBox.TabIndex = 4;
			this.AdditionalCommentGroupBox.TabStop = false;
			this.AdditionalCommentGroupBox.Text = "Additional Comments (Optional)";
			// 
			// SlashLabel
			// 
			this.SlashLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SlashLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 118, true);
			this.SlashLabel.Name = "SlashLabel";
			this.SlashLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(9, 15, true);
			this.SlashLabel.TabIndex = 7;
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
			this.CharacterCountLimitTextBox.TabIndex = 8;
			this.CharacterCountLimitTextBox.Text = "1000";
			// 
			// CharacterCountTextBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CharacterCountTextBox, false);
			this.CharacterCountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 118, true);
			this.CharacterCountTextBox.Name = "CharacterCountTextBox";
			this.CharacterCountTextBox.ReadOnly = true;
			this.CharacterCountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.CharacterCountTextBox.TabIndex = 6;
			// 
			// CharacterCountLabel
			// 
			this.CharacterCountLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CharacterCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 118, true);
			this.CharacterCountLabel.Name = "CharacterCountLabel";
			this.CharacterCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 15, true);
			this.CharacterCountLabel.TabIndex = 5;
			this.CharacterCountLabel.Text = "Character Count";
			this.CharacterCountLabel.UseMnemonic = false;
			// 
			// AcceptCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AcceptCheckBox, "DeclarationAcceptance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.COLSDeclarationAcceptance)(null)).DeclarationAcceptance)));
			this.AcceptCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 304, true);
			this.AcceptCheckBox.Name = "AcceptCheckBox";
			this.AcceptCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 24, true);
			this.AcceptCheckBox.TabIndex = 3;
			this.AcceptCheckBox.Text = "By selecting this you agree to the terms and conditions set out in the General and Privacy Declarations above";
			this.AcceptCheckBox.UseVisualStyleBackColor = true;
			// 
			// DeclarationAgreementUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AcceptCheckBox);
			this.Controls.Add(this.AdditionalCommentGroupBox);
			this.Controls.Add(this.PrivacyDeclarationGroupBox);
			this.Controls.Add(this.GeneralDeclarationGroupBox);
			this.Name = "DeclarationAgreementUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 473, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GeneralDeclarationGroupBox.ResumeLayout(false);
			this.GeneralDeclarationGroupBox.PerformLayout();
			this.PrivacyDeclarationGroupBox.ResumeLayout(false);
			this.PrivacyDeclarationGroupBox.PerformLayout();
			this.AdditionalCommentGroupBox.ResumeLayout(false);
			this.AdditionalCommentGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox GeneralDeclarationTextBox;
		private ZArchitecture.ZTextBox PrivacyDeclarationTextBox;
		internal ZArchitecture.ZTextBox AdditionalCommentTextBox;
		private ZArchitecture.GUI.ZGroupBox GeneralDeclarationGroupBox;
		private ZArchitecture.GUI.ZGroupBox PrivacyDeclarationGroupBox;
		private ZArchitecture.GUI.ZGroupBox AdditionalCommentGroupBox;
		internal ZArchitecture.GUI.ZCheckBox AcceptCheckBox;
		internal ZArchitecture.ZTextBox CharacterCountTextBox;
		private ZArchitecture.ZLabel CharacterCountLabel;
		private ZArchitecture.ZTextBox CharacterCountLimitTextBox;
		private ZArchitecture.ZLabel SlashLabel;
	}
}
