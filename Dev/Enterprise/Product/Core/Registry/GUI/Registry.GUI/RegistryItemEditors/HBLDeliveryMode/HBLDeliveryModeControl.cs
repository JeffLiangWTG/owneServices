namespace Enterprise.Registry.GUI
{
	public partial class HBLDeliveryModeControl : RegistryZUserControl
	{
		public HBLDeliveryModeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			HBLDeliveryModeGrid.ReadOnly = readOnly;
			DefaultHBLDeliveryModeDropEdit.ReadOnly = readOnly;
		}
	}
}
