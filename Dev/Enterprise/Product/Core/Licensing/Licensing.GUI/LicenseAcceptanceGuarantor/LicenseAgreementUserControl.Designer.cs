using CargoWise.Windows.UI;

namespace Enterprise.Licensing.GUI
{
	partial class LicenseAgreementUserControl
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.formPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zRichTextBox1 = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.acceptanceDescription = new Enterprise.ZArchitecture.ZLabel();
			this.acceptButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.userInfoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.declarationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.jobTitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.fullNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.jobTitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.fullNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.confirmationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.titleLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.formPanel.SuspendLayout();
			this.zRichTextBox1.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.userInfoPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Licensing.LicenseAgreement);
			// 
			// formPanel
			// 
			this.formPanel.BackColor = System.Drawing.Color.White;
			this.formPanel.Controls.Add(this.zRichTextBox1);
			this.formPanel.Controls.Add(this.bottomPanel);
			this.formPanel.Controls.Add(this.titleLabel);
			this.formPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.formPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.formPanel.Name = "formPanel";
			this.formPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, 7, 20, 7, true);
			this.formPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 461, true);
			this.formPanel.TabIndex = 7;
			// 
			// zRichTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zRichTextBox1, "LAG_ContentBlob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Licensing.LicenseAgreement)(null)).LAG_ContentBlob)));
			this.zRichTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zRichTextBox1.IsToolBarVisible = false;
			this.zRichTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 30, true);
			this.zRichTextBox1.MaxLength = 10000000;
			this.zRichTextBox1.Name = "zRichTextBox1";
			this.zRichTextBox1.ParentZForm = null;
			this.zRichTextBox1.PopupFormCaption = null;
			this.zRichTextBox1.ReadOnly = true;
			this.zRichTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
			this.zRichTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 198, true);
			this.zRichTextBox1.TabIndex = 2;
			this.zRichTextBox1.TabStop = false;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.acceptanceDescription);
			this.bottomPanel.Controls.Add(this.acceptButton);
			this.bottomPanel.Controls.Add(this.userInfoPanel);
			this.bottomPanel.Controls.Add(this.cancelButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 228, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 225, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// acceptanceDescription
			// 
			this.acceptanceDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.acceptanceDescription.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.acceptanceDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 133, true);
			this.acceptanceDescription.Name = "acceptanceDescription";
			this.acceptanceDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 31, true);
			this.acceptanceDescription.TabIndex = 5;
			this.acceptanceDescription.UseMnemonic = false;
			// 
			// acceptButton
			// 
			this.acceptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.acceptButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 171, true);
			this.acceptButton.Name = "acceptButton";
			this.acceptButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 53, true);
			this.acceptButton.TabIndex = 6;
			this.acceptButton.ToolTipCaption = null;
			this.acceptButton.UseVisualStyleBackColor = true;
			this.acceptButton.Click += new System.EventHandler(this.AcceptButton_Click);
			// 
			// userInfoPanel
			// 
			this.userInfoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.userInfoPanel.Controls.Add(this.declarationLabel);
			this.userInfoPanel.Controls.Add(this.jobTitleLabel);
			this.userInfoPanel.Controls.Add(this.fullNameLabel);
			this.userInfoPanel.Controls.Add(this.jobTitleTextBox);
			this.userInfoPanel.Controls.Add(this.fullNameTextBox);
			this.userInfoPanel.Controls.Add(this.confirmationCheckBox);
			this.userInfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.userInfoPanel.Name = "userInfoPanel";
			this.userInfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 124, true);
			this.userInfoPanel.TabIndex = 3;
			// 
			// declarationLabel
			// 
			this.declarationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.declarationLabel.IsFontBold = true;
			this.declarationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 6, true);
			this.declarationLabel.Name = "declarationLabel";
			this.declarationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 11, true);
			this.declarationLabel.TabIndex = 8;
			this.declarationLabel.Text = "zLabel1";
			this.declarationLabel.UseMnemonic = false;
			// 
			// jobTitleLabel
			// 
			this.jobTitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.jobTitleLabel.IsFontBold = true;
			this.jobTitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 86, true);
			this.jobTitleLabel.Name = "jobTitleLabel";
			this.jobTitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 15, true);
			this.jobTitleLabel.TabIndex = 7;
			this.jobTitleLabel.Text = "zLabel2";
			this.jobTitleLabel.UseMnemonic = false;
			// 
			// fullNameLabel
			// 
			this.fullNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.fullNameLabel.IsFontBold = true;
			this.fullNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 43, true);
			this.fullNameLabel.Name = "fullNameLabel";
			this.fullNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 15, true);
			this.fullNameLabel.TabIndex = 6;
			this.fullNameLabel.Text = "zLabel1";
			this.fullNameLabel.UseMnemonic = false;
			// 
			// jobTitleTextBox
			// 
			this.jobTitleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.jobTitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 102, true);
			this.jobTitleTextBox.Name = "jobTitleTextBox";
			this.jobTitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 15, true);
			this.jobTitleTextBox.ReadOnly = true;
			this.jobTitleTextBox.TabIndex = 5;
			// 
			// fullNameTextBox
			// 
			this.fullNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.fullNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 60, true);
			this.fullNameTextBox.Name = "fullNameTextBox";
			this.fullNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 15, true);
			this.fullNameTextBox.ReadOnly = true;
			this.fullNameTextBox.TabIndex = 4;
			// 
			// confirmationCheckBox
			// 
			this.confirmationCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.confirmationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.confirmationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 24, true);
			this.confirmationCheckBox.Name = "confirmationCheckBox";
			this.confirmationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 18, true);
			this.confirmationCheckBox.TabIndex = 3;
			this.confirmationCheckBox.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 171, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 53, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// titleLabel
			// 
			this.BindingSource.SetBindingMember(this.titleLabel, "LAG_Title");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Licensing.LicenseAgreement)(null)).LAG_Title)));
			this.titleLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.titleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)(((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold) 
            | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(30)))), ((int)(((byte)(225)))));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.titleLabel, false);
			this.titleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 7, true);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 23, true);
			this.titleLabel.TabIndex = 0;
			this.titleLabel.Text = "Title";
			this.titleLabel.UseMnemonic = false;
			// 
			// LicenseAgreementUserControl
			// 
			this.Controls.Add(this.formPanel);
			this.DoubleBuffered = true;
			this.Name = "LicenseAgreementUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 461, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.formPanel.ResumeLayout(false);
			this.formPanel.PerformLayout();
			this.zRichTextBox1.ResumeLayout(true);
			this.zRichTextBox1.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.userInfoPanel.ResumeLayout(false);
			this.userInfoPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private KTableLayoutPanel rootTableLayout;
		private ZArchitecture.GUI.ZRichTextBox zRichTextBox1;
		private ZArchitecture.GUI.ZButton acceptButton;
		private ZArchitecture.ZLabel acceptanceDescription;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZCheckBox confirmationCheckBox;
		private ZArchitecture.ZLabel titleLabel;
		private ZArchitecture.GUI.ZPanel formPanel;
		private ZArchitecture.GUI.ZPanel userInfoPanel;
		private ZArchitecture.ZTextBox jobTitleTextBox;
		private ZArchitecture.ZTextBox fullNameTextBox;
		private ZArchitecture.GUI.ZPanel bottomPanel;
		private ZArchitecture.ZLabel jobTitleLabel;
		private ZArchitecture.ZLabel fullNameLabel;
		private ZArchitecture.ZLabel declarationLabel;
	}
}
