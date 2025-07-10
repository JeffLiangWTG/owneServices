namespace Enterprise.Registry.GUI
{
	public partial class DefaultMinimumStayAndTravelTimeControl : RegistryZUserControl
	{
		public DefaultMinimumStayAndTravelTimeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DefaultMinimumStayAndTravelTimeGrid.ReadOnly = readOnly;
		}
	}
}
