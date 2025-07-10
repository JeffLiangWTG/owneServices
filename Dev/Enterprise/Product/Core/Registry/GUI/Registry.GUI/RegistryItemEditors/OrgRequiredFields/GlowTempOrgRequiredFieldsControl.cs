namespace Enterprise.Registry.GUI
{
	public partial class GlowTempOrgRequiredFieldsControl : RegistryZUserControl
	{
		public GlowTempOrgRequiredFieldsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			GlowTempOrgRequiredFieldsGrid.ReadOnly = readOnly;
		}
	}
}
