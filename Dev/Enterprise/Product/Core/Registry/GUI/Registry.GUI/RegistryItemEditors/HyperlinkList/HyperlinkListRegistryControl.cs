namespace Enterprise.Registry.GUI
{
	public partial class HyperlinkListRegistryControl : RegistryZUserControl
	{
		public HyperlinkListRegistryControl()
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
