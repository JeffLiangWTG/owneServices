using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ZeroAmountTaxTypesDescriptionsControl : RegistryZUserControl
	{
		public ZeroAmountTaxTypesDescriptionsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			zeroAmountTaxTypesDescriptionsGrid.ReadOnly = readOnly;
		}
	}
}
