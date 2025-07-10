using System;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class LoginPasswordListEditControlForRegistry : CodeDescriptionListEditControlForRegistry
	{
		public LoginPasswordListEditControlForRegistry(IRegistryItem registryItem, string codeColumnCaption, string descriptionColumnCaption, int codeColumnMaxLength)
			: base(registryItem, true, true, CharacterCasing.Normal, CharacterCasing.Normal, codeColumnCaption, descriptionColumnCaption, codeColumnMaxLength)
		{
			InitializeComponent();

			DescriptionColumnStyle.TextBox.UseSystemPasswordChar = true;

			buttonShowPasswords.Text = ShowPasswordCaption;

			var user = EnvProxy.Instance.CurrentUser;
			buttonShowPasswords.Enabled = user.IsDeveloper || user.IsController;
		}

		ZTextBoxColumnStyle DescriptionColumnStyle => (ZTextBoxColumnStyle)CodeDescriptionGrid.Columns[DescriptionColumnName].ColumnStyle;

		void buttonShowPasswords_Click(object sender, EventArgs e)
		{
			var descriptionColumnStyle = DescriptionColumnStyle;
			if (descriptionColumnStyle.TextBox.UseSystemPasswordChar)
			{
				if (!CanShowPasswords())
				{
					return;
				}

				buttonShowPasswords.Text = HidePasswordCaption;
				buttonShowPasswords.ToolTipCaption = HidePasswordCaption;
				descriptionColumnStyle.TextBox.UseSystemPasswordChar = false;
			}
			else
			{
				buttonShowPasswords.Text = ShowPasswordCaption;
				buttonShowPasswords.ToolTipCaption = ShowPasswordCaption;
				descriptionColumnStyle.TextBox.UseSystemPasswordChar = true;
			}

			CodeDescriptionGrid.Invalidate();
			CodeDescriptionGrid.Update();
		}

		MultilingualString ShowPasswordCaption => ResString.GetMultilingualString("a51f916c-9a19-45d9-b5d9-c00fc5631fca", "Show Passwords");
		MultilingualString HidePasswordCaption => ResString.GetMultilingualString("626c1323-0380-41d1-a8d9-2f20beb25f59", "Hide Passwords");

#if DEBUG
		internal
#endif
		bool CanShowPasswords()
		{
			var user = EnvProxy.Instance.CurrentUser;
			if (user.IsDeveloper)
			{
				return CheckValidDeveloperPassword();
			}
			else if (user.IsController)
			{
				return true;
			}

			return false;
		}

#if DEBUG
		protected virtual
#endif
		bool CheckValidDeveloperPassword()
		{
			using (DeveloperLoginForm loginForm = new DeveloperLoginForm())
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(loginForm) == DialogResult.OK)
				{
					if (loginForm.IsValidPassword)
					{
						return true;
					}
					else
					{
						loginForm.ShowIncorrectPasswordMessage();
					}
				}
			}
			return false;
		}

		#region Test stuff
#if DEBUG

		internal ZButton ButtonShowPasswordsExposedForTest => buttonShowPasswords;

		internal ZTextBoxColumnStyle DescriptionColumnStyleExposedForTest => DescriptionColumnStyle;

#endif

		#endregion
	}
}
