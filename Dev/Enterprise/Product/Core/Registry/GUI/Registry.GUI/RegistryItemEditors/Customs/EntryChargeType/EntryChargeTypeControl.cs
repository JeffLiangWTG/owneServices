namespace Enterprise.Registry.GUI.Customs
{
	public partial class EntryChargeTypeControl : RegistryZUserControl
	{
		public EntryChargeTypeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			EntryChargeTypesGrid.ReadOnly = readOnly;
		}
	}
}
