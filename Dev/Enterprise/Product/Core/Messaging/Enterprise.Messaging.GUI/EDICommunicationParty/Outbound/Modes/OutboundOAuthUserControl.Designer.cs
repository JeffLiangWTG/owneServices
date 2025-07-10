using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	partial class OutboundOAuthUserControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.ClientID = new Enterprise.ZArchitecture.ZTextBox();
            this.AuthorizationURL = new Enterprise.ZArchitecture.ZTextBox();
            this.AuthorizationGrantTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ClientSecret = new Enterprise.ZArchitecture.ZTextBox();
            this.CertificateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ShowCertificateButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.SetCertificateButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.GenerateCSRButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.Username = new Enterprise.ZArchitecture.ZTextBox();
            this.Password = new Enterprise.ZArchitecture.ZTextBox();
            this.ScopesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ScopesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.Tick = new Enterprise.ZArchitecture.ZLabel();
            this.VerifyButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.VerifyText = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.AuthorizationGrantTypeDropEdit.SuspendLayout();
            this.CertificateGroupBox.SuspendLayout();
            this.ScopesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ScopesGrid)).BeginInit();
            this.ScopesGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EDICommunicationParty);
			// 
			// ClientID
			//
			this.ClientID.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left))));
			this.ClientID.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 15, true);
			this.ClientID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.BindingSource.SetBindingMember(this.ClientID, "OutboundConfig.Auth.ECA_ClientID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.Auth.ECA_ClientID)));
            this.ClientID.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("2a58ecd7-a718-41ee-83f1-68d2b2be9e65", "Client Id");
            this.ClientID.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ClientID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 66, true);
            this.ClientID.Name = "ClientID";
            this.ClientID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 17, true);
			this.ClientID.TabIndex = 4;
			// 
			// AuthorizationURL
			//
			this.AuthorizationURL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left))));
			this.AuthorizationURL.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 15, true);
            this.BindingSource.SetBindingMember(this.AuthorizationURL, "OutboundConfig.Auth.ECA_AuthorizationEndpoint");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.Auth.ECA_AuthorizationEndpoint)));
            this.AuthorizationURL.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("cd1bc693-3314-413e-8734-bb7366a5c6d2", "Authorization URL");
            this.AuthorizationURL.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.AuthorizationURL.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 36, true);
            this.AuthorizationURL.Name = "AuthorizationURL";
            this.AuthorizationURL.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 17, true);
			this.AuthorizationURL.TabIndex = 3;
            // 
            // AuthorizationGrantTypeDropEdit
            // 
            this.AuthorizationGrantTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AuthorizationGrantTypeDropEdit, "OutboundConfig.Auth.ECA_FlowCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.Auth.ECA_FlowCode)));
            this.AuthorizationGrantTypeDropEdit.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("0b449ef1-0be1-4038-b727-521bcdec921a", "Grant Type");
            this.AuthorizationGrantTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.AuthorizationGrantTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 6, true);
            this.AuthorizationGrantTypeDropEdit.Name = "AuthorizationGrantTypeDropEdit";
            this.AuthorizationGrantTypeDropEdit.PreBoundMaxLength = 3;
            this.AuthorizationGrantTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 17, true);
            this.AuthorizationGrantTypeDropEdit.TabIndex = 2;
            // 
            // ClientSecret
            // 
            this.ClientSecret.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.BindingSource.SetBindingMember(this.ClientSecret, "OutboundConfig.Auth.ECA_ClientSecret");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.Auth.ECA_ClientSecret)));
            this.ClientSecret.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("0e41f490-dc11-4e8f-ad7a-e0251f78971d", "Client Secret");
            this.ClientSecret.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ClientSecret.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 96, true);
            this.ClientSecret.Name = "ClientSecret";
            this.ClientSecret.PasswordChar = '*';
            this.ClientSecret.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 17, true);
            this.ClientSecret.TabIndex = 6;
            // 
            // CertificateGroupBox
            // 
            this.CertificateGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("6c01a768-b459-4a37-bf5d-503eabab957f", "Certificate");
            this.CertificateGroupBox.Controls.Add(this.ShowCertificateButton);
            this.CertificateGroupBox.Controls.Add(this.SetCertificateButton);
            this.CertificateGroupBox.Controls.Add(this.GenerateCSRButton);
            this.CertificateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 96, true);
            this.CertificateGroupBox.Name = "CertificateGroupBox";
            this.CertificateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 40, true);
            this.CertificateGroupBox.TabIndex = 7;
            this.CertificateGroupBox.TabStop = false;
            // 
            // ShowCertificateButton
            // 
            this.ShowCertificateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShowCertificateButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("9054dde8-d235-476a-89bb-56aeb9caa962", "Show Certificate...", "Shows the certificate.");
            this.ShowCertificateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 12, true);
            this.ShowCertificateButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.ShowCertificateButton.Name = "ShowCertificateButton";
            this.ShowCertificateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 24, true);
            this.ShowCertificateButton.TabIndex = 27;
            this.ShowCertificateButton.ToolTipCaption = null;
            this.ShowCertificateButton.UseVisualStyleBackColor = true;
            this.ShowCertificateButton.Click += new System.EventHandler(this.ShowCertificateButton_Click);
            // 
            // SetCertificateButton
            // 
            this.SetCertificateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SetCertificateButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("65d6c89e-f885-4647-9125-7a3aa322952a", "Set Certificate...", "Sets the certificate.");
            this.SetCertificateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 12, true);
            this.SetCertificateButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.SetCertificateButton.Name = "SetCertificateButton";
            this.SetCertificateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 24, true);
            this.SetCertificateButton.TabIndex = 26;
            this.SetCertificateButton.ToolTipCaption = null;
            this.SetCertificateButton.UseVisualStyleBackColor = true;
            this.SetCertificateButton.Click += new System.EventHandler(this.SetCertificateButton_Click);
            // 
            // GenerateCSRButton
            // 
            this.GenerateCSRButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.GenerateCSRButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("0e05947c-61b3-42e4-86f6-2099d7013ce7", "Generate CSR...", "Generates the certificate signing request.");
            this.GenerateCSRButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 12, true);
            this.GenerateCSRButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
            this.GenerateCSRButton.Name = "GenerateCSRButton";
            this.GenerateCSRButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 24, true);
            this.GenerateCSRButton.TabIndex = 25;
            this.GenerateCSRButton.ToolTipCaption = null;
            this.GenerateCSRButton.UseVisualStyleBackColor = true;
            this.GenerateCSRButton.Click += new System.EventHandler(this.GenerateCsrButton_Click);
            // 
            // Username
            // 
            this.Username.AcceptsReturn = true;
            this.Username.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.Username, "OutboundConfig.Auth.ECA_Username");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.Auth.ECA_Username)));
            this.Username.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("8ca521e2-88a8-4586-9fef-ca512ecb4f8d", "Username");
            this.Username.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.Username.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 96, true);
            this.Username.Name = "Username";
            this.Username.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
            this.Username.TabIndex = 5;
            // 
            // Password
            // 
            this.Password.AcceptsReturn = true;
            this.Password.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.Password, "OutboundConfig.Auth.ECA_Password");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.Auth.ECA_Password)));
            this.Password.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("0647f9d3-a3c2-4a37-bbad-6bf8dc0ae837", "Password");
            this.Password.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.Password.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 126, true);
            this.Password.Name = "Password";
            this.Password.PasswordChar = '*';
            this.Password.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
            this.Password.TabIndex = 8;
            // 
            // ScopesGroupBox
            // 
            this.ScopesGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("be73a8e1-9cb2-4bae-af90-fd5c05c5b475", "Scopes");
            this.ScopesGroupBox.Controls.Add(this.ScopesGrid);
            this.ScopesGroupBox.ImeMode = System.Windows.Forms.ImeMode.On;
            this.ScopesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 141, true);
            this.ScopesGroupBox.Name = "ScopesGroupBox";
            this.ScopesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 127, true);
            this.ScopesGroupBox.TabIndex = 24;
            this.ScopesGroupBox.TabStop = false;
            // 
            // ScopesGrid
            // 
            this.ScopesGrid.AllowNavigation = false;
            this.ScopesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.ScopesGrid, "OutboundConfig.Auth.FormScopes");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.Auth.FormScopes)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OAuth2Scope)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.Auth.FormScopes)).SyncRoot)).ScopeName)));
            this.ScopesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("13ab8712-d6e4-4cb7-8d95-210c6ea31ba9", "Scope Name");
            zTextBoxColumnStyleInfo1.ColumnName = "ScopeName";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo1.MaxLengthOverride = 256;
			this.ScopesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ScopesGrid.CopySelectedRowsAllowed = false;
            this.ScopesGrid.GridId = "2996080f-4fbf-4734-99e5-b063e2824325";
            this.ScopesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ScopesGrid.LayoutKey = "ClaimsMappingGrid";
            this.ScopesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
            this.ScopesGrid.Name = "ScopesGrid";
            this.ScopesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 104, true);
            this.ScopesGrid.TabIndex = 5;
            // 
            // Tick
            // 
            this.Tick.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Largest;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Tick, false);
            this.Tick.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 2, true);
            this.Tick.Name = "Tick";
            this.Tick.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 28, true);
            this.Tick.TabIndex = 14;
            this.Tick.UseMnemonic = false;
            // 
            // VerifyButton
            // 
            this.VerifyButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("9b919664-c547-40d8-9e21-a0ba8fb07a1f", "Verify");
            this.VerifyButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.VerifyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 3, true);
            this.VerifyButton.Name = "VerifyButton";
            this.VerifyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 25, true);
            this.VerifyButton.TabIndex = 13;
            this.VerifyButton.ToolTipCaption = null;
            this.VerifyButton.UseVisualStyleBackColor = true;
            this.VerifyButton.Click += new System.EventHandler(this.VerifyButton_Click);
            // 
            // VerifyText
            // 
            this.VerifyText.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("0ba47179-f304-49c2-ad4c-505dff86c2eb", "Verify the configuration by logging in:");
            this.VerifyText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.VerifyText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 3, true);
            this.VerifyText.Name = "VerifyText";
            this.VerifyText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 31, true);
            this.VerifyText.TabIndex = 25;
            this.VerifyText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.VerifyText.UseMnemonic = false;
            // 
            // OutboundOAuthUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.CertificateGroupBox);
            this.Controls.Add(this.VerifyText);
            this.Controls.Add(this.ScopesGroupBox);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.ClientSecret);
            this.Controls.Add(this.Username);
            this.Controls.Add(this.AuthorizationURL);
            this.Controls.Add(this.ClientID);
            this.Controls.Add(this.AuthorizationGrantTypeDropEdit);
            this.Controls.Add(this.Tick);
            this.Controls.Add(this.VerifyButton);
            this.Name = "OutboundOAuthUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 291, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.AuthorizationGrantTypeDropEdit.ResumeLayout(true);
            this.AuthorizationGrantTypeDropEdit.PerformLayout();
            this.CertificateGroupBox.ResumeLayout(false);
            this.CertificateGroupBox.PerformLayout();
            this.ScopesGroupBox.ResumeLayout(false);
            this.ScopesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ScopesGrid)).EndInit();
            this.ScopesGrid.ResumeLayout(false);
            this.ScopesGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox ClientID;
		private ZArchitecture.ZTextBox AuthorizationURL;
		private ZArchitecture.GUI.ZDropEdit AuthorizationGrantTypeDropEdit;
		private ZArchitecture.ZTextBox ClientSecret;
		private ZArchitecture.ZTextBox Username;
		private ZArchitecture.ZTextBox Password;
		private ZArchitecture.GUI.ZGroupBox ScopesGroupBox;
		private ZArchitecture.ZGrid ScopesGrid;
		internal ZArchitecture.GUI.ZButton GenerateCSRButton;
		private ZArchitecture.GUI.ZGroupBox CertificateGroupBox;
		internal ZArchitecture.GUI.ZButton SetCertificateButton;
		internal ZArchitecture.GUI.ZButton ShowCertificateButton;
		private ZArchitecture.ZLabel Tick;
		private ZArchitecture.GUI.ZButton VerifyButton;
		private ZArchitecture.ZLabel VerifyText;
	}
}
