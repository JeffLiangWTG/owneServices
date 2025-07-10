namespace Enterprise.Customs.JP.GUI
{
	partial class FTPSettingsRegistryItemUserControl
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
			this.InFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OutFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PasswordViewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PortCaclEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ServerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UserNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PassiveYesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.PassiveLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FTPStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FTPStatusTestButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PassiveNoRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Common.FTPSettings);
			// 
			// ServerTextBox
			// 
			this.BindingSource.SetBindingMember(this.ServerTextBox, "Server");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.FTPSettings)(null)).Server)));
			this.ServerTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 3, true);
			this.ServerTextBox.Name = "ServerTextBox";
			this.ServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 16, true);
			this.ServerTextBox.TabIndex = 1;
			// 
			// PortCaclEdit
			// 
			this.BindingSource.SetBindingMember(this.PortCaclEdit, "Port");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Common.FTPSettings)(null)).Port)));
			this.PortCaclEdit.DecimalPlaces = 0;
			this.PortCaclEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 29, true);
			this.PortCaclEdit.Name = "PortCaclEdit";
			this.PortCaclEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 16, true);
			this.PortCaclEdit.TabIndex = 2;
			this.PortCaclEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// UserNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.UserNameTextBox, "UserName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.FTPSettings)(null)).UserName)));
			this.UserNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UserNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 55, true);
			this.UserNameTextBox.Name = "UserNameTextBox";
			this.UserNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 16, true);
			this.UserNameTextBox.TabIndex = 3;
			// 
			// PasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.FTPSettings)(null)).Password)));
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 81, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 16, true);
			this.PasswordTextBox.TabIndex = 4;
			// 
			// PasswordViewButton
			// 
			this.PasswordViewButton.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("5f769470-ea1f-43e9-96ab-676d2493a6ee", "View");
			this.PasswordViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 80, true);
			this.PasswordViewButton.Name = "PasswordViewButton";
			this.PasswordViewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PasswordViewButton.TabIndex = 5;
			this.PasswordViewButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.PasswordViewButton.Click += new System.EventHandler(this.ViewButton_Click);
			// 
			// InFolderTextBox
			// 
			this.BindingSource.SetBindingMember(this.InFolderTextBox, "InFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.FTPSettings)(null)).InFolder)));
			this.InFolderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 107, true);
			this.InFolderTextBox.Name = "InFolderTextBox";
			this.InFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 16, true);
			this.InFolderTextBox.TabIndex = 6;
			// 
			// OutFolderTextBox
			// 
			this.BindingSource.SetBindingMember(this.OutFolderTextBox, "OutFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.FTPSettings)(null)).OutFolder)));
			this.OutFolderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OutFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 133, true);
			this.OutFolderTextBox.Name = "OutFolderTextBox";
			this.OutFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 16, true);
			this.OutFolderTextBox.TabIndex = 8;
			// 
			// PassiveNoRadioButton
			// 
			this.PassiveNoRadioButton.AutoCheck = false;
			this.PassiveNoRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PassiveNoRadioButton, "PassiveNoSelection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.Common.FTPSettings)(null)).PassiveNoSelection)));
			this.PassiveNoRadioButton.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("16e57fee-3642-4486-8b4b-6eac64a32927", "No");
			this.PassiveNoRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PassiveNoRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 159, true);
			this.PassiveNoRadioButton.Name = "PassiveNoRadioButton";
			this.PassiveNoRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 15, true);
			this.PassiveNoRadioButton.TabIndex = 12;
			// 
			// PassiveYesRadioButton
			// 
			this.PassiveYesRadioButton.AutoCheck = false;
			this.PassiveYesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PassiveYesRadioButton, "PassiveYesSelection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.Common.FTPSettings)(null)).PassiveYesSelection)));
			this.PassiveYesRadioButton.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("322ac088-3200-4e6b-82fb-6a2893f913e1", "Yes");
			this.PassiveYesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PassiveYesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 159, true);
			this.PassiveYesRadioButton.Name = "PassiveYesRadioButton";
			this.PassiveYesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 15, true);
			this.PassiveYesRadioButton.TabIndex = 11;
			// 
			// PassiveLabel
			// 
			this.PassiveLabel.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("66f7acee-a157-47f1-8fed-dd016ae360e9", "Passive");
			this.PassiveLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(39, 157, true);
			this.PassiveLabel.Name = "PassiveLabel";
			this.PassiveLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 17, true);
			this.PassiveLabel.TabIndex = 10;
			// 
			// FTPStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.FTPStatusTextBox, "Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.FTPSettings)(null)).Status)));
			this.FTPStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FTPStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 185, true);
			this.FTPStatusTextBox.Multiline = true;
			this.FTPStatusTextBox.Name = "FTPStatusTextBox";
			this.FTPStatusTextBox.ReadOnly = true;
			this.FTPStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 32, true);
			this.FTPStatusTextBox.TabIndex = 13;
			// 
			// FTPStatusTestButton
			// 
			this.FTPStatusTestButton.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("9da8ca38-3e91-4071-a833-794537aa2fcc", "Test FTP connection");
			this.FTPStatusTestButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 190, true);
			this.FTPStatusTestButton.Name = "FTPStatusTestButton";
			this.FTPStatusTestButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 21, true);
			this.FTPStatusTestButton.TabIndex = 14;
			this.FTPStatusTestButton.Click += new System.EventHandler(this.FTPStatusTestButton_Click);
			// 
			// FTPSettingsRegistryItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InFolderTextBox);
			this.Controls.Add(this.OutFolderTextBox);
			this.Controls.Add(this.PasswordTextBox);
			this.Controls.Add(this.PasswordViewButton);
			this.Controls.Add(this.PortCaclEdit);
			this.Controls.Add(this.ServerTextBox);
			this.Controls.Add(this.UserNameTextBox);
			this.Controls.Add(this.PassiveYesRadioButton);
			this.Controls.Add(this.PassiveNoRadioButton);
			this.Controls.Add(this.PassiveLabel);
			this.Controls.Add(this.FTPStatusTestButton);
			this.Controls.Add(this.FTPStatusTextBox);
			this.Name = "FTPSettingsRegistryItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.ZTextBox InFolderTextBox;
		Enterprise.ZArchitecture.ZTextBox OutFolderTextBox;
		Enterprise.ZArchitecture.ZTextBox PasswordTextBox;
		Enterprise.ZArchitecture.GUI.ZButton PasswordViewButton;
		Enterprise.ZArchitecture.ZCalcEdit PortCaclEdit;
		Enterprise.ZArchitecture.ZTextBox ServerTextBox;
		Enterprise.ZArchitecture.ZTextBox UserNameTextBox;
		Enterprise.ZArchitecture.ZLabel PassiveLabel;
		Enterprise.ZArchitecture.GUI.ZRadioButton PassiveNoRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton PassiveYesRadioButton;
		Enterprise.ZArchitecture.GUI.ZButton FTPStatusTestButton;
		Enterprise.ZArchitecture.ZTextBox FTPStatusTextBox;
	}
}
