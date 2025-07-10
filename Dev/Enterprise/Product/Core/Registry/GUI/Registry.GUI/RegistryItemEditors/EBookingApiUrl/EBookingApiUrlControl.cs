namespace Enterprise.Registry.GUI
{
	public partial class EBookingApiUrlControl : RegistryZUserControl
	{
		public EBookingApiUrlControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PresetEBookingApiUrlDropEdit.ReadOnly = readOnly;
		}
	}
}
