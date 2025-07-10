using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class BillingUnitCountAdjustmentRegistryControl : RegistryZUserControl
	{
		public BillingUnitCountAdjustmentRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PriceCodeGrid.ReadOnly = AdjustmentGrid.ReadOnly = readOnly;
		}
	}
}
