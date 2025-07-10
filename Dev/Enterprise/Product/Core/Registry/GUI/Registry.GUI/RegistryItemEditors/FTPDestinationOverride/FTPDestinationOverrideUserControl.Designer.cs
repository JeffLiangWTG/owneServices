using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class FTPDestinationOverrideUserControl : RegistryBusinessObjectTemplateZUserControl
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
		void InitializeComponent()
		{
			this.ftpAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.userNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.passwordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.viewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.FTPDestinationOverrideInfo);
			// 
			// ftpAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.ftpAddressTextBox, "FtpAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.FTPDestinationOverrideInfo)(null)).FtpAddress)));
			this.ftpAddressTextBox.CaptionResourceString = Res.GetData("5021B9D4-4C5F-4133-B6CC-3DD182EA4D4B", "FTP Address");
			this.ftpAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ftpAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 13, true);
			this.ftpAddressTextBox.Name = "ftpAddressTextBox";
			this.ftpAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.ftpAddressTextBox.TabIndex = 0;
			// 
			// userNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.userNameTextBox, "UserName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.FTPDestinationOverrideInfo)(null)).UserName)));
			this.userNameTextBox.CaptionResourceString = Res.GetData("484F10AE-D717-43F0-87EF-84D87977BA0E", "User Name");
			this.userNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.userNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 35, true);
			this.userNameTextBox.Name = "userNameTextBox";
			this.userNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.userNameTextBox.TabIndex = 1;
			// 
			// passwordTextBox
			// 
			this.BindingSource.SetBindingMember(this.passwordTextBox, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.FTPDestinationOverrideInfo)(null)).Password)));
			this.passwordTextBox.CaptionResourceString = Res.GetData("711C608F-9E9F-4EA3-9587-199B29D35D91", "Password");
			this.passwordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.passwordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 56, true);
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.PasswordChar = '*';
			this.passwordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);
			this.passwordTextBox.TabIndex = 2;
			// 
			// ViewButton
			// 
			this.viewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.viewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 56, true);
			this.viewButton.Name = "viewButton";
			this.viewButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.viewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.viewButton.TabIndex = 3;
			this.viewButton.Text = "View";
			this.viewButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.viewButton.ToolTipCaption = null;
			this.viewButton.Click += new System.EventHandler(this.ViewButton_Click);
			// 
			// FTPDestinationOverrideUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.viewButton);
			this.Controls.Add(this.passwordTextBox);
			this.Controls.Add(this.userNameTextBox);
			this.Controls.Add(this.ftpAddressTextBox);
			this.Name = "FTPDestinationOverrideUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		#region Viewing the Password

		void ViewButton_Click(object sender, System.EventArgs e)
		{
			ViewPassword();
		}

		void ViewPassword()
		{
			using (var loginForm = new DeveloperLoginForm())
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(loginForm) == DialogResult.OK)
				{
					if (IsValidPassword(loginForm))
					{
						Globals.Message.ShowInformation(passwordTextBox.Text, Res.GetString("116ba7cc-7014-42bb-8aae-b3b01b42a3d9", "Password"));
					}
					else
					{
						loginForm.ShowIncorrectPasswordMessage();
					}
				}
			}
		}

#if DEBUG
		protected virtual
#endif
		bool IsValidPassword(DeveloperLoginForm loginForm)
		{
			return loginForm.IsValidPassword;
		}

		#endregion

		public ZTextBox ftpAddressTextBox;
		public ZTextBox userNameTextBox;
		public ZTextBox passwordTextBox;
		public ZButton viewButton;
	}
}
