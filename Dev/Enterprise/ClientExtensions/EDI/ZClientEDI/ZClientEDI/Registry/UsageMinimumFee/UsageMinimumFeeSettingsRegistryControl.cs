using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class UsageMinimumFeeSettingsRegistryControl : RegistryZUserControl
	{
		public UsageMinimumFeeSettingsRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			MinimumFeeUsageGrid.ReadOnly = readOnly;
		}
	}
}
