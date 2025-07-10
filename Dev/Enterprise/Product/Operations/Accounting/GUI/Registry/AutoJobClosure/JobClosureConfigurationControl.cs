using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class JobClosureConfigurationControl : RegistryZUserControl
	{
		public JobClosureConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			JobClosureConfigGrid.ReadOnly = readOnly;
		}
	}
}
