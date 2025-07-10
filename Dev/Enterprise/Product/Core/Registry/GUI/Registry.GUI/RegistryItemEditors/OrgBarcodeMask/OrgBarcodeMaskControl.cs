namespace Enterprise.Registry.GUI
{
	public partial class OrgBarcodeMaskControl : RegistryZUserControl
	{
#if DEBUG
		public
#endif
		Enterprise.ZArchitecture.ZGrid orgBarcodeMaskGrid;

		public OrgBarcodeMaskControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			orgBarcodeMaskGrid.ReadOnly = readOnly;
		}
	}
}
