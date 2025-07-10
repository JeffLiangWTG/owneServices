using Enterprise.Registry.GUI;
using ZClientEDI.Business;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class WTGActiveDirectoryCredentialsControl : RegistryZUserControl
	{
		public WTGActiveDirectoryCredentialsControl()
		{
			InitializeComponent();
		}

		public WTGActiveDirectoryCredentials Value
		{
			get { return wTGActivieDirectoryCredentials; }
			set
			{
				wTGActivieDirectoryCredentials = value;
				SetDataBinding(value, "");
			}
		}
		WTGActiveDirectoryCredentials wTGActivieDirectoryCredentials;

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			if (readOnly)
			{
				Value = WTGActiveDirectoryCredentials.DefaultValue;
			}
			EnabledCheckBox.ReadOnly = readOnly;
			DomainNameTextBox.ReadOnly = readOnly;
			DomainUsernameTextBox.ReadOnly = readOnly;
			DomainPasswordTextBox.ReadOnly = readOnly;
			OrgUnitPathTextBox.ReadOnly = readOnly;
		}
	}
}
