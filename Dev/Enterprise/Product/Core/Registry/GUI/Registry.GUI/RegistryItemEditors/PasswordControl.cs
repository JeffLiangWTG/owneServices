using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	[DefaultBindingProperty("Text")]
	public partial class PasswordControl : ZUserControl
	{
		public PasswordControl()
		{
			InitializeComponent();
		}

		[BindingMetaDataProperty(MetaDataTypes.ReadOnly, "ReadOnly")]
		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength")]
		public override string Text
		{
			get { return PasswordTextBox.Text; }
			set { PasswordTextBox.Text = value; }
		}

		public bool ReadOnly
		{
			get { return PasswordTextBox.ReadOnly; }
			set { PasswordTextBox.ReadOnly = value; }
		}

		public int MaxLength
		{
			get { return PasswordTextBox.MaxLength; }
			set { PasswordTextBox.MaxLength = value; }
		}

		public event EventHandler MaxLengthChanged
		{
			add { PasswordTextBox.MaxLengthChanged += value; }
			remove { PasswordTextBox.MaxLengthChanged -= value; }
		}

		#region Viewing the Password

		void ViewButton_Click(object sender, System.EventArgs e)
		{
			ViewPassword();
		}

		void ViewPassword()
		{
			var user = EnvProxy.Instance.CurrentUser;
			if (user.IsDeveloper && !String.IsNullOrEmpty(PasswordTextBox.Text))
			{
				using (DeveloperLoginForm loginForm = new DeveloperLoginForm())
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(loginForm) == DialogResult.OK)
					{
						if (IsValidPassword(loginForm))
						{
							Globals.Message.ShowInformation(PasswordTextBox.Text, Res.GetString("116ba7cc-7014-42bb-8aae-b3b01b42a3d9", "Password"));
						}
						else
						{
							loginForm.ShowIncorrectPasswordMessage();
						}
					}
				}
			}
			else if (user.IsController && !String.IsNullOrEmpty(PasswordTextBox.Text))
			{
				Globals.Message.ShowInformation(PasswordTextBox.Text, Res.GetString("116ba7cc-7014-42bb-8aae-b3b01b42a3d9", "Password"));
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
	}
}
