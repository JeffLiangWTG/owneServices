using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class UsageBillingSettingsRegistryControl : RegistryZUserControl
	{
		public UsageBillingSettingsRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			BranchGrid.ReadOnly = PriceListGrid.ReadOnly = readOnly;
		}
	}
}
