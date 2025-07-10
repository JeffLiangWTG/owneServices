using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class InboundBasicAuthenticationUserControl : ZUserControl
	{
		const int PasswordLength = 24;
		internal bool isPasswordGenerated;

		public InboundBasicAuthenticationUserControl()
		{
			InitializeComponent();
		}

		string GeneratePassword(int length)
		{
			if (length < 1)
			{
				throw new ArgumentException("Length must be greater than 0", nameof(length));
			}

			var buffer = new byte[(length + 2) / 4 * 3];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(buffer);
			}

			return Convert.ToBase64String(buffer);
		}

		#if DEBUG
		public
		#endif
		void GeneratePasswordButton_Click()
		{
			CopyPasswordToClipboardButton.Visible = false;
			if (string.IsNullOrEmpty(UsernameTextBox.Text))
			{
				Globals.Message.ShowError(Res.GetString("9997021C-4517-40D0-8DD9-E8E97F185787", "Please enter a username."), Res.GetString("92D41853-59FD-4C18-B276-9B68BB57E244", "Invalid User Name"));
				return;
			}

			if (!Regex.IsMatch(UsernameTextBox.Text, "^[a-zA-Z0-9]*$"))
			{
				Globals.Message.ShowError(Res.GetString("2BF5A664-C89B-48FD-A3E7-F12A9E3B70C1", "Username cannot contain special characters. Please use alphanumeric characters only."), Res.GetString("92D41853-59FD-4C18-B276-9B68BB57E244", "Invalid User Name"));
				return;
			}

			if (!isPasswordGenerated && !String.IsNullOrEmpty(PasswordTextBox.Text))
			{
				var result = Globals.Message.ShowConfirmation(Res.GetString("5387DAE6-708C-4E05-B2C5-3846320ADD48", "WARNING: Reset Password - Irreversible Action. Would you like to proceed?"), Res.GetString("BA758577-D024-4CF7-8794-0ABC05F18160", "Reset Password Warning"), "YES", ZMessageBoxIcon.Warning);
				if (result != ZDialogResult.OK)
				{
					return;
				}
			}

			Config.Auth.ECA_Password = GeneratePassword(PasswordLength);
			PasswordTextBox.PasswordChar = '\0';
			CopyPasswordToClipboardButton.Visible = true;
		}

		void GeneratePasswordButton_Click(object sender, EventArgs e)
		{
			GeneratePasswordButton_Click();
		}

		#if DEBUG
		public
		#endif
		void SetPasswordToClipboard()
		{
			SafeClipboard.SetText(PasswordTextBox.Text);
		}

		void CopyPasswordToClipboardButton_Click(object sender, EventArgs e)
		{
			SetPasswordToClipboard();
			Globals.Message.Show(Res.GetString("1d8df91e-e3c7-4f8d-ad79-ed3313b4eb06", "Password has been successfully copied to the clipboard."));
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			((ZForm)ParentForm).Saved += InboundBasicAuthenticationUserControl_Saved;
			if (!string.IsNullOrEmpty(PasswordTextBox.Text))
			{
				PasswordTextBox.PasswordChar = '*';
				isPasswordGenerated = true;
				CopyPasswordToClipboardButton.Visible = false;
			}
		}

		void InboundBasicAuthenticationUserControl_Saved(object sender, EventArgs e)
		{
			PasswordTextBox.PasswordChar = '*';
			CopyPasswordToClipboardButton.Visible = false;
		}

		#if DEBUG
		public
		#endif
		string UsernameTextBoxValue
		{
			get
			{
				return UsernameTextBox.Text;
			}
			set
			{
				UsernameTextBox.Text = value;
			}
		}

		#if DEBUG
		public
		#endif
		ZTextBox PasswordTextBoxObject => PasswordTextBox;

		#if DEBUG
		public
		#endif
		bool CopyPasswordToClipboardButtonVisibility => CopyPasswordToClipboardButton.Visible;

		#if DEBUG
		public
		#endif
		void GeneratePasswordButtonFocus()
		{
			GeneratePasswordButton.Focus();
		}

		public EDICommunicationPartyConfig Config => ((EDICommunicationParty)DataSource)?.InboundConfig;
	}
}
