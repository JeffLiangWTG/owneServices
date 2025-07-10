namespace Enterprise.Registry.GUI.eHub
{
	public partial class ScavengingSettingsUserControl : RegistryZUserControl
	{
		public ScavengingSettingsUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ScavengingSettingsGrid.ReadOnly = readOnly;
		}
	}
}
