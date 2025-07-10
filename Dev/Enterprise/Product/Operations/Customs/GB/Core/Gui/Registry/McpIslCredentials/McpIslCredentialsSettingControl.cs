using Enterprise.Registry.GUI;

namespace Enterprise.Customs.GB.GUI.Registry
{
	public partial class McpIslCredentialsSettingControl : RegistryZUserControl
	{
		public McpIslCredentialsSettingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			McpIslCredentialsGrid.ReadOnly = readOnly;
		}
	}
}
