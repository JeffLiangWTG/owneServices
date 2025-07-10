namespace Enterprise.Registry.GUI
{
	public partial class SummarizeULDSLACConfigControl : RegistryZUserControl
	{
		public SummarizeULDSLACConfigControl()
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
