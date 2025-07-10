namespace Enterprise.Registry.GUI
{
	public partial class HouseBillOfLadingTypeCollectionControl : RegistryZUserControl
	{
		public HouseBillOfLadingTypeCollectionControl()
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
