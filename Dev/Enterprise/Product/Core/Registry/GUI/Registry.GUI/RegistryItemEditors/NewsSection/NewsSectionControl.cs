namespace Enterprise.Registry.GUI
{
	public partial class NewsSectionControl : RegistryZUserControl
	{
		public NewsSectionControl()
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
