using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class eAdaptorNextOutboundRegistryControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.UpdateNoteHintLabel = new Enterprise.ZArchitecture.ZLabel();
            this.OptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.Tick = new Enterprise.ZArchitecture.ZLabel();
            this.VerifyButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.VerifyLabel = new Enterprise.ZArchitecture.ZLabel();
            this.AuthorizationGrantTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.OAuth2EnabledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.AuthorizationURLGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AuthorizationURL = new Enterprise.ZArchitecture.ZTextBox();
            this.ScopesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ScopesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.ClientIDLabel = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ClientID = new Enterprise.ZArchitecture.ZTextBox();
            this.ClientSecretLabel = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ClientSecret = new Enterprise.ZArchitecture.ZTextBox();
            this.UsernameLabel = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.Username = new Enterprise.ZArchitecture.ZTextBox();
            this.PasswordLabel = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.Password = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.OptionGroupBox.SuspendLayout();
            this.AuthorizationGrantTypeDropEdit.SuspendLayout();
            this.AuthorizationURLGroupBox.SuspendLayout();
            this.ScopesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ScopesGrid)).BeginInit();
            this.ScopesGrid.SuspendLayout();
            this.ClientIDLabel.SuspendLayout();
            this.ClientSecretLabel.SuspendLayout();
            this.UsernameLabel.SuspendLayout();
            this.PasswordLabel.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.eAdaptorNextOutboundConfig);
            // 
            // UpdateNoteHintLabel
            // 
            this.UpdateNoteHintLabel.AutoSize = true;
            this.UpdateNoteHintLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("08aab5db-376a-4b67-9094-2c17257c1f45", "OAuth2 configuration for eAdaptor Outbound messaging");
            this.UpdateNoteHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.UpdateNoteHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 15, true);
            this.UpdateNoteHintLabel.Name = "UpdateNoteHintLabel";
            this.UpdateNoteHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 13, true);
            this.UpdateNoteHintLabel.TabIndex = 11;
            // 
            // OptionGroupBox
            // 
            this.OptionGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("befe6390-c38a-4376-b0b5-d7167605ea39", "eAdaptor Next Outbound Configuration");
            this.OptionGroupBox.Controls.Add(this.Tick);
            this.OptionGroupBox.Controls.Add(this.VerifyButton);
            this.OptionGroupBox.Controls.Add(this.VerifyLabel);
            this.OptionGroupBox.Controls.Add(this.AuthorizationGrantTypeDropEdit);
            this.OptionGroupBox.Controls.Add(this.OAuth2EnabledCheckBox);
            this.OptionGroupBox.Controls.Add(this.UpdateNoteHintLabel);
            this.OptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 2, true);
            this.OptionGroupBox.Name = "OptionGroupBox";
            this.OptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 96, true);
            this.OptionGroupBox.TabIndex = 0;
            this.OptionGroupBox.TabStop = false;
			// 
			// Tick
			//
			this.Tick.CaptionResourceString = null;
			this.Tick.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Largest;
            this.Tick.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 62, true);
            this.Tick.Name = "Tick";
            this.Tick.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 28, true);
            this.Tick.TabIndex = 14;
			LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Tick, false);
			// 
			// VerifyButton
			// 
			this.VerifyButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c92aa2dd-124d-4a49-98b4-613b5dac3f0e", "Verify");
            this.VerifyButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.VerifyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 64, true);
            this.VerifyButton.Name = "VerifyButton";
            this.VerifyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 25, true);
            this.VerifyButton.TabIndex = 13;
            this.VerifyButton.ToolTipCaption = null;
            this.VerifyButton.UseVisualStyleBackColor = true;
            this.VerifyButton.Click += new System.EventHandler(this.VerifyButton_Click);
            // 
            // VerifyLabel
            // 
            this.VerifyLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("795385fa-df2f-4c6d-92da-6e4f5b12bd36", "Before saving, verify the configuration by first logging in:");
            this.VerifyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.VerifyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 66, true);
            this.VerifyLabel.Name = "VerifyLabel";
            this.VerifyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 23, true);
            this.VerifyLabel.TabIndex = 12;
            // 
            // AuthorizationGrantTypeDropEdit
            // 
            this.AuthorizationGrantTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AuthorizationGrantTypeDropEdit, "AuthorizationGrantTypeCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.eAdaptorNextOutboundConfig)(null)).AuthorizationGrantTypeCode)));
            this.AuthorizationGrantTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.AuthorizationGrantTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 42, true);
            this.AuthorizationGrantTypeDropEdit.Name = "AuthorizationGrantTypeDropEdit";
            this.AuthorizationGrantTypeDropEdit.PreBoundMaxLength = 3;
            this.AuthorizationGrantTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 16, true);
            this.AuthorizationGrantTypeDropEdit.TabIndex = 2;
            this.AuthorizationGrantTypeDropEdit.Leave += new System.EventHandler(this.AuthorizationGrantTypeDropEdit_Leave);
            // 
            // OAuth2EnabledCheckBox
            // 
            this.OAuth2EnabledCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.OAuth2EnabledCheckBox, "IsOAuth2Enabled");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.eAdaptorNextOutboundConfig)(null)).IsOAuth2Enabled)));
            this.OAuth2EnabledCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OAuth2EnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 44, true);
            this.OAuth2EnabledCheckBox.Name = "OAuth2EnabledCheckBox";
            this.OAuth2EnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 15, true);
            this.OAuth2EnabledCheckBox.TabIndex = 1;
            this.OAuth2EnabledCheckBox.UseVisualStyleBackColor = true;
            // 
            // AuthorizationURLGroupBox
            // 
            this.AuthorizationURLGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("cec92a76-5e61-4826-93f7-8e2d52fbbd98", "Authorization URL");
            this.AuthorizationURLGroupBox.Controls.Add(this.AuthorizationURL);
            this.AuthorizationURLGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 100, true);
            this.AuthorizationURLGroupBox.Name = "AuthorizationURLGroupBox";
            this.AuthorizationURLGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 39, true);
            this.AuthorizationURLGroupBox.TabIndex = 10;
            this.AuthorizationURLGroupBox.TabStop = false;
            // 
            // AuthorizationURL
            // 
            this.BindingSource.SetBindingMember(this.AuthorizationURL, "AuthorizationURL");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.eAdaptorNextOutboundConfig)(null)).AuthorizationURL)));
            this.AuthorizationURL.CaptionResourceString = null;
            this.AuthorizationURL.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AuthorizationURL, false);
            this.AuthorizationURL.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 16, true);
            this.AuthorizationURL.Name = "AuthorizationURL";
            this.AuthorizationURL.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 16, true);
            this.AuthorizationURL.TabIndex = 2;
            // 
            // ScopesGroupBox
            // 
            this.ScopesGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2e8a68bc-f44a-4fa9-badb-93430f8431ae", "Scopes");
            this.ScopesGroupBox.Controls.Add(this.ScopesGrid);
            this.ScopesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 310, true);
            this.ScopesGroupBox.Name = "ScopesGroupBox";
            this.ScopesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 80, true);
            this.ScopesGroupBox.TabIndex = 23;
            this.ScopesGroupBox.TabStop = false;
            // 
            // ScopesGrid
            // 
            this.ScopesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ScopesGrid, "Scopes");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.eAdaptorNextOutboundConfig)(null)).Scopes)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OAuth2Scope)(((System.Collections.IList)(((Enterprise.Registry.Business.eAdaptorNextOutboundConfig)(null)).Scopes)).SyncRoot)).ScopeName)));
            this.ScopesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e5ac55b7-9147-4ecd-b214-68618f67f547", "Scope Name");
            zTextBoxColumnStyleInfo1.ColumnName = "ScopeName";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            this.ScopesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ScopesGrid.CopySelectedRowsAllowed = false;
            this.ScopesGrid.GridId = "2996080f-4fbf-4734-99e5-b063e2824325";
            this.ScopesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ScopesGrid.LayoutKey = "ClaimsMappingGrid";
            this.ScopesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
            this.ScopesGrid.Name = "ScopesGrid";
            this.ScopesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 60, true);
            this.ScopesGrid.TabIndex = 5;
            // 
            // ClientIDLabel
            // 
            this.ClientIDLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d8b6a85e-4364-4a44-a9eb-05bffb30a651", "Client ID");
            this.ClientIDLabel.Controls.Add(this.ClientID);
            this.ClientIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 142, true);
            this.ClientIDLabel.Name = "ClientIDLabel";
            this.ClientIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 39, true);
            this.ClientIDLabel.TabIndex = 11;
            this.ClientIDLabel.TabStop = false;
            // 
            // ClientID
            // 
            this.BindingSource.SetBindingMember(this.ClientID, "ClientID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.eAdaptorNextOutboundConfig)(null)).ClientID)));
            this.ClientID.CaptionResourceString = null;
            this.ClientID.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClientID, false);
            this.ClientID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 16, true);
            this.ClientID.Name = "ClientID";
            this.ClientID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 16, true);
            this.ClientID.TabIndex = 2;
            // 
            // ClientSecretLabel
            // 
            this.ClientSecretLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("1f8c863a-68c4-4311-ad5c-4c1f3750425c", "Client Secret");
            this.ClientSecretLabel.Controls.Add(this.ClientSecret);
            this.ClientSecretLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 183, true);
            this.ClientSecretLabel.Name = "ClientSecretLabel";
            this.ClientSecretLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 40, true);
            this.ClientSecretLabel.TabIndex = 12;
            this.ClientSecretLabel.TabStop = false;
            // 
            // ClientSecret
            // 
            this.BindingSource.SetBindingMember(this.ClientSecret, "ClientSecret");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.eAdaptorNextOutboundConfig)(null)).ClientSecret)));
            this.ClientSecret.CaptionResourceString = null;
            this.ClientSecret.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClientSecret, false);
            this.ClientSecret.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 16, true);
            this.ClientSecret.Name = "ClientSecret";
            this.ClientSecret.PasswordChar = '*';
            this.ClientSecret.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 16, true);
            this.ClientSecret.TabIndex = 2;
            // 
            // UsernameLabel
            // 
            this.UsernameLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6a4d7f93-4e4e-4fcd-8f4e-299077ed3c29", "Username");
            this.UsernameLabel.Controls.Add(this.Username);
            this.UsernameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 226, true);
            this.UsernameLabel.Name = "UsernameLabel";
            this.UsernameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 40, true);
            this.UsernameLabel.TabIndex = 13;
            this.UsernameLabel.TabStop = false;
            // 
            // Username
            // 
            this.Username.AcceptsReturn = true;
            this.BindingSource.SetBindingMember(this.Username, "Username");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.eAdaptorNextOutboundConfig)(null)).Username)));
            this.Username.CaptionResourceString = null;
            this.Username.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Username, false);
            this.Username.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 16, true);
            this.Username.Multiline = true;
            this.Username.Name = "Username";
            this.Username.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Username.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 16, true);
            this.Username.TabIndex = 2;
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("625186b8-d52f-4e00-a7da-a5f21a37356e", "Password");
            this.PasswordLabel.Controls.Add(this.Password);
            this.PasswordLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 268, true);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 40, true);
            this.PasswordLabel.TabIndex = 14;
            this.PasswordLabel.TabStop = false;
            // 
            // Password
            // 
            this.Password.AcceptsReturn = true;
            this.BindingSource.SetBindingMember(this.Password, "Password");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.eAdaptorNextOutboundConfig)(null)).Password)));
            this.Password.CaptionResourceString = null;
            this.Password.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Password, false);
            this.Password.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 16, true);
            this.Password.Multiline = true;
            this.Password.Name = "Password";
            this.Password.PasswordChar = '*';
            this.Password.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Password.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 16, true);
            this.Password.TabIndex = 2;
            // 
            // eAdaptorNextOutboundRegistryControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.OptionGroupBox);
            this.Controls.Add(this.AuthorizationURLGroupBox);
            this.Controls.Add(this.ClientIDLabel);
            this.Controls.Add(this.ClientSecretLabel);
            this.Controls.Add(this.UsernameLabel);
            this.Controls.Add(this.PasswordLabel);
            this.Controls.Add(this.ScopesGroupBox);
            this.Name = "eAdaptorNextOutboundRegistryControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 395, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.OptionGroupBox.ResumeLayout(false);
            this.OptionGroupBox.PerformLayout();
            this.AuthorizationGrantTypeDropEdit.ResumeLayout(true);
            this.AuthorizationGrantTypeDropEdit.PerformLayout();
            this.AuthorizationURLGroupBox.ResumeLayout(false);
            this.AuthorizationURLGroupBox.PerformLayout();
            this.ScopesGroupBox.ResumeLayout(false);
            this.ScopesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ScopesGrid)).EndInit();
            this.ScopesGrid.ResumeLayout(false);
            this.ScopesGrid.PerformLayout();
            this.ClientIDLabel.ResumeLayout(false);
            this.ClientIDLabel.PerformLayout();
            this.ClientSecretLabel.ResumeLayout(false);
            this.ClientSecretLabel.PerformLayout();
            this.UsernameLabel.ResumeLayout(false);
            this.UsernameLabel.PerformLayout();
            this.PasswordLabel.ResumeLayout(false);
            this.PasswordLabel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZLabel UpdateNoteHintLabel;
		ZArchitecture.GUI.ZGroupBox OptionGroupBox;
		ZArchitecture.GUI.ZCheckBox OAuth2EnabledCheckBox;
		ZArchitecture.GUI.ZGroupBox AuthorizationURLGroupBox;
		ZArchitecture.GUI.ZDropEdit AuthorizationGrantTypeDropEdit;
		ZArchitecture.ZTextBox AuthorizationURL;
		private ZArchitecture.ZLabel VerifyLabel;
		private ZArchitecture.GUI.ZButton VerifyButton;
		private ZArchitecture.GUI.ZGroupBox ScopesGroupBox;
		private ZArchitecture.ZGrid ScopesGrid;
		private ZArchitecture.GUI.ZGroupBox ClientIDLabel;
		private ZArchitecture.ZTextBox ClientID;
		private ZArchitecture.GUI.ZGroupBox ClientSecretLabel;
		private ZArchitecture.ZTextBox ClientSecret;
		private ZArchitecture.GUI.ZGroupBox UsernameLabel;
		private ZArchitecture.ZTextBox Username;
		private ZArchitecture.GUI.ZGroupBox PasswordLabel;
		private ZArchitecture.ZTextBox Password;
		private ZArchitecture.ZLabel Tick;
	}
}
