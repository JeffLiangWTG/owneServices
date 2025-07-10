namespace Enterprise.Registry.GUI
{
	public partial class SupplyChainSecurityOrganisationToUseControl : RegistryZUserControl
	{
		public SupplyChainSecurityOrganisationToUseControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			OrganisationsToUseGrid.ReadOnly = readOnly;
		}
	}
}
