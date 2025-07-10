namespace Enterprise.Registry.GUI
{
	public partial class AdditionalHouseBillOfLadingTypeCollectionControl : RegistryZUserControl
	{
		public AdditionalHouseBillOfLadingTypeCollectionControl()
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
