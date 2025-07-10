namespace Enterprise.Registry.GUI
{
	public partial class ApplicationIdentifierControl : RegistryZUserControl
	{
		public ApplicationIdentifierControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ApplicationIdentifierGrid.ReadOnly = readOnly;
		}
	}
}
