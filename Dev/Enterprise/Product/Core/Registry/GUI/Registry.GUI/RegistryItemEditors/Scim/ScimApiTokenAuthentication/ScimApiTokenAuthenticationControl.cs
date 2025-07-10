using System;
using CargoWise.Types;
namespace Enterprise.Registry.GUI
{
	public partial class ScimApiTokenAuthenticationControl : RegistryZUserControl
	{
		bool IsTokenGenerated;
		string tokenValue;

		public ScimApiTokenAuthenticationControl()
		{
			InitializeComponent();
			tbApiToken.Text = $"Value Hidden";
			IsTokenGenerated = false;
		}

		internal void CreateToken()
		{
			bGenerateNew.PerformClick();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			bGenerateNew.ReadOnly = readOnly;
		}

		public string Value
		{
			get
			{
				return IsTokenGenerated ? tokenValue : string.Empty;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					IsTokenGenerated = true;
					tokenValue = value;
					tbApiToken.Text = $"Value Hidden";
					return;
				}

				IsTokenGenerated = false;
				tokenValue = string.Empty;
				tbApiToken.Text = string.Empty;
			}
		}

		void bGenerateNew_Click(object sender, EventArgs e)
		{
			var newToken = ZGuid.NewZGuid().ToString();
			tokenValue = newToken;
			tbApiToken.Text = newToken;
			IsTokenGenerated = true;
			NotifyChanges();
		}
	}
}
