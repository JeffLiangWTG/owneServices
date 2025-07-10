namespace Enterprise.Registry.GUI
{
	public partial class TransportModeCombinationBufferTimeRegistryControl : RegistryZUserControl
	{
		public TransportModeCombinationBufferTimeRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			TransportModeCombinationBufferTimeGrid.ReadOnly = readOnly;
		}
	}
}
