using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class AuthorizationModeAndSettingsControl : RegistryZUserControl
	{
		public AuthorizationModeAndSettingsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			this.AuthorizationModeDropEdit.Enabled = !readOnly;
			this.AuthorizationRequirementsGrid.ReadOnly = readOnly;
		}
	}
}
