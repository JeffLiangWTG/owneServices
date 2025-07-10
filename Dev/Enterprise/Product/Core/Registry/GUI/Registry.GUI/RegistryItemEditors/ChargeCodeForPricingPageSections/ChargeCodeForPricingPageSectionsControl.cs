namespace Enterprise.Registry.GUI
{
	public partial class ChargeCodeForPricingPageSectionsControl : RegistryZUserControl
	{
		public ChargeCodeForPricingPageSectionsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ConfigurationGrid.ReadOnly = readOnly;
			ChargesGrid.ReadOnly = readOnly;
		}
	}
}
