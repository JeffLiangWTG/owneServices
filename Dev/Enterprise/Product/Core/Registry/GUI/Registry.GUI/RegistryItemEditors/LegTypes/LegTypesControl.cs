namespace Enterprise.Registry.GUI
{
	public partial class LegTypesControl : RegistryZUserControl
	{
		public LegTypesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			LegTypesGrid.ReadOnly = readOnly;
		}
	}
}
