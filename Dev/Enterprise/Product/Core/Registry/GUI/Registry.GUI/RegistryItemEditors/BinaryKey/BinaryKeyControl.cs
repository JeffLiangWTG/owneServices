using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

using Rfc2898DeriveBytes = WTG.Foundation.Cryptography.Algorithms.Rfc2898DeriveBytes;

namespace Enterprise.Registry.GUI
{
	public partial class BinaryKeyControl : ZUserControl
	{
		public BinaryKeyControl(int keySize)
		{
			this.keySize = keySize;

			InitializeComponent();
		}

		readonly int keySize;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Value
		{
			get { return hexadecimalKeyTextBox.Text; }
			set { hexadecimalKeyTextBox.Text = value; }
		}

		void GenerateKeyButton_Click(object sender, EventArgs e)
		{
			var input = passphraseForKeyGenerationTextBox.Text;
			if (string.IsNullOrWhiteSpace(input))
			{
				Globals.Message.ShowError((NoResString)"Please enter a passphrase to use for key generation.", (NoResString)"Invalid Input");
				return;
			}

			// Use the same salt so that the same input gives the same consistent output.
			using (var derive = new Rfc2898DeriveBytes(input, StaticSalt, 1000, HashAlgorithmName.SHA1))
			{
				var passphrase = derive.GetBytes(keySize);
				hexadecimalKeyTextBox.Text = BitConverter.ToString(passphrase).Replace("-", string.Empty);
				passphraseForKeyGenerationTextBox.Text = string.Empty;
			}
		}

		void viewKeyButton_Click(object sender, EventArgs e)
		{
			ViewPassword();
		}

		void ViewPassword()
		{
			var user = EnvProxy.Instance.CurrentUser;
			if (user.IsDeveloper && !String.IsNullOrEmpty(hexadecimalKeyTextBox.Text))
			{
				using (DeveloperLoginForm loginForm = new DeveloperLoginForm())
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(loginForm) == DialogResult.OK)
					{
						if (IsValidPassword(loginForm))
						{
							Globals.Message.ShowInformation(hexadecimalKeyTextBox.Text, Res.GetString("116ba7cc-7014-42bb-8aae-b3b01b42a3d9", "Password"));
						}
						else
						{
							loginForm.ShowIncorrectPasswordMessage();
						}
					}
				}
			}
			else if (user.IsController && !String.IsNullOrEmpty(hexadecimalKeyTextBox.Text))
			{
				Globals.Message.ShowInformation(hexadecimalKeyTextBox.Text, Res.GetString("116ba7cc-7014-42bb-8aae-b3b01b42a3d9", "Password"));
			}
		}

#if DEBUG
		protected virtual
#endif
		bool IsValidPassword(DeveloperLoginForm loginForm)
		{
			return loginForm.IsValidPassword;
		}
		static readonly byte[] StaticSalt = new byte[] { 0x58, 0xCA, 0x63, 0x36, 0x21, 0x77, 0x4B, 0xD5, 0x9C, 0xB6, 0xCC, 0x92, 0x0A, 0x22, 0x8F, 0x2A };
	}
}
