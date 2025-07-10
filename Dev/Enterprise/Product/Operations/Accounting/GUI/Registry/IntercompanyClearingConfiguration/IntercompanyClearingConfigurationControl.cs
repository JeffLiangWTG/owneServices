using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class IntercompanyClearingConfigurationControl : RegistryZUserControl
	{
		public IntercompanyClearingConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			IntercompanyClearingConfigurationGrid.ReadOnly = readOnly;
		}
	}
}

