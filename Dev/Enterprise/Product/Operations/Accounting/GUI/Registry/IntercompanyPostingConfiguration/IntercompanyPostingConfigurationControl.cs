using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class IntercompanyPostingConfigurationControl : RegistryZUserControl
	{
		public IntercompanyPostingConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			IntercompanyPostingConfigurationGrid.ReadOnly = readOnly;
		}
	}
}

