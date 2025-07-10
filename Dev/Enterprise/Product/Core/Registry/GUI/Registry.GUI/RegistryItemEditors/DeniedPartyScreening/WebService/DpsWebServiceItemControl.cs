namespace Enterprise.Registry.GUI
{
	public partial class DpsWebServiceItemControl : RegistryZUserControl
	{
		public DpsWebServiceItemControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DpsWebServiceItemGrid.ReadOnly = ReadOnly;
		}
	}
}
