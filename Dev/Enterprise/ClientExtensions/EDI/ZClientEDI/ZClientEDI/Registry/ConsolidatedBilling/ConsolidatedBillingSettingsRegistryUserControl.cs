using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class ConsolidatedBillingSettingsRegistryUserControl : RegistryZUserControl
	{
		public ConsolidatedBillingSettingsRegistryUserControl() : base()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			grid.ReadOnly = readOnly;
		}
	}
}
