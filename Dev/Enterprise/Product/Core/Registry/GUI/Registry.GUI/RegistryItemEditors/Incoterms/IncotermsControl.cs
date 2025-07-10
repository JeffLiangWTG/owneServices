namespace Enterprise.Registry.GUI
{
	public partial class IncotermsControl : RegistryZUserControl
	{
		public IncotermsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			IncoTermChargesGrid.ReadOnly = readOnly;
		}
	}
}
