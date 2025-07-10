namespace ZClientEDI.GUI.Rating
{
	partial class SSOAuthTokenGeneratorForm
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
            this.GenerateButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CompanyCodeLabel = new Enterprise.ZArchitecture.ZTextBox();
            this.EnterpriseCodeLabel = new Enterprise.ZArchitecture.ZTextBox();
            this.UserCodeLabel = new Enterprise.ZArchitecture.ZTextBox();
            this.ServerCodeLabel = new Enterprise.ZArchitecture.ZTextBox();
            this.UserFullNameLabel = new Enterprise.ZArchitecture.ZTextBox();
            this.CompanyNameLabel = new Enterprise.ZArchitecture.ZTextBox();
            this.UserEmailLabel = new Enterprise.ZArchitecture.ZTextBox();
            this.TokenResultTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RolesCheckedListBox = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
            this.RolesLabel = new Enterprise.ZArchitecture.ZLabel();
            this.TokenResultLabel = new Enterprise.ZArchitecture.ZLabel();
            this.DatabaseNumberUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DatabaseNumberUpDown)).BeginInit();
            this.DatabaseNumberUpDown.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 451, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(ZClientEDI.Business.Rating.SubmitAuthTokenInputs);
            // 
            // GenerateButton
            // 
            this.GenerateButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GenerateButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9ad8d7c0-3071-4f07-9029-cdfb24610e19", "Generate");
            this.GenerateButton.IsCaptionOverridden = true;
            this.GenerateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 220, true);
            this.GenerateButton.Name = "GenerateButton";
            this.GenerateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 27, true);
            this.GenerateButton.TabIndex = 11;
            this.GenerateButton.Text = "Generate";
            this.GenerateButton.ToolTipCaption = null;
            this.GenerateButton.UseVisualStyleBackColor = true;
            this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
            // 
            // CompanyCodeLabel
            // 
            this.BindingSource.SetBindingMember(this.CompanyCodeLabel, "CompanyCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZClientEDI.Business.Rating.SubmitAuthTokenInputs)(null)).CompanyCode)));
            this.CompanyCodeLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3617a1ee-414c-44fd-8c9a-6d26f85e1b5f", "Company Code");
            this.CompanyCodeLabel.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CompanyCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 168, true);
            this.CompanyCodeLabel.Name = "CompanyCodeLabel";
            this.CompanyCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
            this.CompanyCodeLabel.TabIndex = 7;
            // 
            // EnterpriseCodeLabel
            // 
            this.BindingSource.SetBindingMember(this.EnterpriseCodeLabel, "EnterpriseCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZClientEDI.Business.Rating.SubmitAuthTokenInputs)(null)).EnterpriseCode)));
            this.EnterpriseCodeLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("bfb71456-61e7-412f-953e-049766de1554", "Enterprise Code");
            this.EnterpriseCodeLabel.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.EnterpriseCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 38, true);
            this.EnterpriseCodeLabel.Name = "EnterpriseCodeLabel";
            this.EnterpriseCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
            this.EnterpriseCodeLabel.TabIndex = 2;
            // 
            // UserCodeLabel
            // 
            this.BindingSource.SetBindingMember(this.UserCodeLabel, "UserCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZClientEDI.Business.Rating.SubmitAuthTokenInputs)(null)).UserCode)));
            this.UserCodeLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("18b1dc60-eead-4caa-9e3f-6eec5e2b7ccf", "User Code");
            this.UserCodeLabel.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.UserCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 90, true);
            this.UserCodeLabel.Name = "UserCodeLabel";
            this.UserCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
            this.UserCodeLabel.TabIndex = 4;
            // 
            // ServerCodeLabel
            // 
            this.BindingSource.SetBindingMember(this.ServerCodeLabel, "ServerCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZClientEDI.Business.Rating.SubmitAuthTokenInputs)(null)).ServerCode)));
            this.ServerCodeLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("094f8c1a-67fc-4291-9924-3ce87e667b0e", "Server Code");
            this.ServerCodeLabel.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ServerCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 64, true);
            this.ServerCodeLabel.Name = "ServerCodeLabel";
            this.ServerCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
            this.ServerCodeLabel.TabIndex = 3;
            // 
            // UserFullNameLabel
            // 
            this.BindingSource.SetBindingMember(this.UserFullNameLabel, "UserFullName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZClientEDI.Business.Rating.SubmitAuthTokenInputs)(null)).UserFullName)));
            this.UserFullNameLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d66f02e6-7480-431b-9c26-d54e6f5887fb", "User Full Name");
            this.UserFullNameLabel.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.UserFullNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 116, true);
            this.UserFullNameLabel.Name = "UserFullNameLabel";
            this.UserFullNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
            this.UserFullNameLabel.TabIndex = 5;
            // 
            // CompanyNameLabel
            // 
            this.BindingSource.SetBindingMember(this.CompanyNameLabel, "CompanyName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZClientEDI.Business.Rating.SubmitAuthTokenInputs)(null)).CompanyName)));
            this.CompanyNameLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("6b063b0d-5092-44e1-a526-ffc6d3d7cf0b", "Company Name");
            this.CompanyNameLabel.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CompanyNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 194, true);
            this.CompanyNameLabel.Name = "CompanyNameLabel";
            this.CompanyNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
            this.CompanyNameLabel.TabIndex = 8;
            // 
            // UserEmailLabel
            // 
            this.BindingSource.SetBindingMember(this.UserEmailLabel, "UserEmail");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZClientEDI.Business.Rating.SubmitAuthTokenInputs)(null)).UserEmail)));
            this.UserEmailLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ce4843a2-8ad0-4769-9ef0-5a1d7abada33", "User Email");
            this.UserEmailLabel.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.UserEmailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 142, true);
            this.UserEmailLabel.Name = "UserEmailLabel";
            this.UserEmailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
            this.UserEmailLabel.TabIndex = 6;
            // 
            // TokenResultTextBox
            // 
            this.TokenResultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TokenResultTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d365573b-e8e9-4709-ab2e-0bc6e5872340", "", "Token Result");
            this.TokenResultTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.TokenResultTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 279, true);
            this.TokenResultTextBox.Multiline = true;
            this.TokenResultTextBox.Name = "TokenResultTextBox";
            this.TokenResultTextBox.ReadOnly = true;
            this.TokenResultTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 166, true);
            this.TokenResultTextBox.TabIndex = 10;
            // 
            // RolesCheckedListBox
            // 
            this.RolesCheckedListBox.BindingItems = null;
            this.BindingSource.SetBindingMember(this.RolesCheckedListBox, "Roles");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZBoolDescriptionPairList)(((ZClientEDI.Business.Rating.SubmitAuthTokenInputs)(null)).Roles)));
            this.RolesCheckedListBox.CheckOnClick = true;
            this.RolesCheckedListBox.FormattingEnabled = true;
            this.RolesCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 38, true);
            this.RolesCheckedListBox.Name = "RolesCheckedListBox";
            this.RolesCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 94, true);
            this.RolesCheckedListBox.TabIndex = 9;
            // 
            // RolesLabel
            // 
            this.RolesLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2e03cf18-e25d-4521-95f6-de9eaa7e9a75", "Roles");
            this.RolesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.RolesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 12, true);
            this.RolesLabel.Name = "RolesLabel";
            this.RolesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
            this.RolesLabel.TabIndex = 10;
            this.RolesLabel.Text = "Roles";
            this.RolesLabel.UseMnemonic = false;
            // 
            // TokenResultLabel
            // 
            this.TokenResultLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TokenResultLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("613457e3-9181-4ffa-bdc5-4ebcd1b2c752", "Token Result");
            this.TokenResultLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.TokenResultLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 253, true);
            this.TokenResultLabel.Name = "TokenResultLabel";
            this.TokenResultLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
            this.TokenResultLabel.TabIndex = 12;
            this.TokenResultLabel.Text = "Token Result";
            this.TokenResultLabel.UseMnemonic = false;
            // 
            // DatabaseNumberUpDown
            // 
            this.BindingSource.SetBindingMember(this.DatabaseNumberUpDown, "DatabaseNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((ZClientEDI.Business.Rating.SubmitAuthTokenInputs)(null)).DatabaseNumber)));
            this.DatabaseNumberUpDown.BindTo = "DatabaseNumber";
            this.DatabaseNumberUpDown.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("25a59d21-b1da-4f7e-8b21-4c104f48e8a7", "Database Number");
            this.DatabaseNumberUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 12, true);
            this.DatabaseNumberUpDown.Name = "DatabaseNumberUpDown";
            this.DatabaseNumberUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
            this.DatabaseNumberUpDown.TabIndex = 1;
            // 
            // SSOAuthTokenGeneratorForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("aa633bbb-a105-42b8-b77b-4a9648c52649", "SSO Auth Token");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 475, true);
            this.Controls.Add(this.DatabaseNumberUpDown);
            this.Controls.Add(this.EnterpriseCodeLabel);
            this.Controls.Add(this.ServerCodeLabel);
            this.Controls.Add(this.UserCodeLabel);
            this.Controls.Add(this.UserFullNameLabel);
            this.Controls.Add(this.UserEmailLabel);
            this.Controls.Add(this.CompanyCodeLabel);
            this.Controls.Add(this.CompanyNameLabel);
            this.Controls.Add(this.GenerateButton);
            this.Controls.Add(this.RolesLabel);
            this.Controls.Add(this.RolesCheckedListBox);
            this.Controls.Add(this.TokenResultLabel);
            this.Controls.Add(this.TokenResultTextBox);
            this.DataSourceType = typeof(ZClientEDI.Business.Rating.SubmitAuthTokenInputs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SSOAuthTokenGeneratorForm";
            this.Text = "SSOAuthTokenGeneratorForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.TokenResultTextBox, 0);
            this.Controls.SetChildIndex(this.TokenResultLabel, 0);
            this.Controls.SetChildIndex(this.RolesCheckedListBox, 0);
            this.Controls.SetChildIndex(this.RolesLabel, 0);
            this.Controls.SetChildIndex(this.GenerateButton, 0);
            this.Controls.SetChildIndex(this.CompanyNameLabel, 0);
            this.Controls.SetChildIndex(this.CompanyCodeLabel, 0);
            this.Controls.SetChildIndex(this.UserEmailLabel, 0);
            this.Controls.SetChildIndex(this.UserFullNameLabel, 0);
            this.Controls.SetChildIndex(this.UserCodeLabel, 0);
            this.Controls.SetChildIndex(this.ServerCodeLabel, 0);
            this.Controls.SetChildIndex(this.EnterpriseCodeLabel, 0);
            this.Controls.SetChildIndex(this.DatabaseNumberUpDown, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DatabaseNumberUpDown)).EndInit();
            this.DatabaseNumberUpDown.ResumeLayout(false);
            this.DatabaseNumberUpDown.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZButton GenerateButton;
		private Enterprise.ZArchitecture.ZTextBox CompanyCodeLabel;
		private Enterprise.ZArchitecture.ZTextBox EnterpriseCodeLabel;
		private Enterprise.ZArchitecture.ZTextBox UserCodeLabel;
		private Enterprise.ZArchitecture.ZTextBox ServerCodeLabel;
		private Enterprise.ZArchitecture.ZTextBox UserFullNameLabel;
		private Enterprise.ZArchitecture.ZTextBox CompanyNameLabel;
		private Enterprise.ZArchitecture.ZTextBox UserEmailLabel;
		private Enterprise.ZArchitecture.ZTextBox TokenResultTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckedListBox RolesCheckedListBox;
		private Enterprise.ZArchitecture.ZLabel RolesLabel;
		private Enterprise.ZArchitecture.ZLabel TokenResultLabel;
		private Enterprise.ZArchitecture.GUI.ZNumericUpDown DatabaseNumberUpDown;
	}
}
