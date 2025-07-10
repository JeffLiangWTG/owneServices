namespace Enterprise.Registry.GUI
{
	public partial class StaffColumnToGroupDescriptionScimMappingControl : RegistryZUserControl
	{
		public StaffColumnToGroupDescriptionScimMappingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			Grid.ReadOnly = readOnly;
		}
	}
}
