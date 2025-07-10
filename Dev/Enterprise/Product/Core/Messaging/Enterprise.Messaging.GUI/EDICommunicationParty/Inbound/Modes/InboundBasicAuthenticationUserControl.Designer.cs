namespace Enterprise.Messaging.GUI
{
	partial class InboundBasicAuthenticationUserControl
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
            this.UsernameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GeneratePasswordButton = new ZArchitecture.GUI.ZButton();
			this.CopyPasswordToClipboardButton = new ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EDICommunicationParty);
            // 
            // UsernameTextBox
            // 
            this.BindingSource.SetBindingMember(this.UsernameTextBox, "InboundConfig.Auth.ECA_Username");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).InboundConfig.Auth.ECA_Username)));
            this.UsernameTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("2933547d-fe31-432f-9185-a31a40fe54b5", "Username");
            this.UsernameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.UsernameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 3, true);
            this.UsernameTextBox.Name = "UsernameTextBox";
            this.UsernameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.UsernameTextBox.TabIndex = 3;
            // 
            // PasswordTextBox
            // 
            this.BindingSource.SetBindingMember(this.PasswordTextBox, "InboundConfig.Auth.ECA_Password");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).InboundConfig.Auth.ECA_Password)));
            this.PasswordTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("1fa43d1d-8ab2-4e54-9928-aab731495232", "Password");
            this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Enabled = false;
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 27, true);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
            this.PasswordTextBox.TabIndex = 4;
			// 
			// GeneratePasswordButton
			//
			this.GeneratePasswordButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("AuthenticationControl|C0124316-055F-49DA-8A95-1BD278C68574", "Generate Password");
			this.GeneratePasswordButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 27, true);
			this.GeneratePasswordButton.Name = "GeneratePasswordButton";
			this.GeneratePasswordButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 19, true);
			this.GeneratePasswordButton.TabIndex = 8;
			this.GeneratePasswordButton.ToolTipCaption = null;
			this.GeneratePasswordButton.UseVisualStyleBackColor = true;
			this.GeneratePasswordButton.Click += new System.EventHandler(this.GeneratePasswordButton_Click);
			// 
			// CopyPasswordToClipboardButton
			//
			this.CopyPasswordToClipboardButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("AuthenticationControl|1C57DDC9-F044-41F5-B44B-3EAD10A9AABE", "Copy to Clipboard");
			this.CopyPasswordToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 27, true);
			this.CopyPasswordToClipboardButton.Name = "CopyPasswordToClipboardButton";
			this.CopyPasswordToClipboardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 19, true);
			this.CopyPasswordToClipboardButton.TabIndex = 8;
			this.CopyPasswordToClipboardButton.ToolTipCaption = null;
			this.CopyPasswordToClipboardButton.UseVisualStyleBackColor = true;
			this.CopyPasswordToClipboardButton.Visible = false;
			this.CopyPasswordToClipboardButton.Click += new System.EventHandler(this.CopyPasswordToClipboardButton_Click);
			// 
			// InboundBasicAuthenticationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CopyPasswordToClipboardButton);
			this.Controls.Add(this.GeneratePasswordButton);
			this.Controls.Add(this.PasswordTextBox);
            this.Controls.Add(this.UsernameTextBox);
            this.Name = "InboundBasicAuthenticationUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 218, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox UsernameTextBox;
		private ZArchitecture.ZTextBox PasswordTextBox;
		private ZArchitecture.GUI.ZButton GeneratePasswordButton;
		private ZArchitecture.GUI.ZButton CopyPasswordToClipboardButton;
	}
}
