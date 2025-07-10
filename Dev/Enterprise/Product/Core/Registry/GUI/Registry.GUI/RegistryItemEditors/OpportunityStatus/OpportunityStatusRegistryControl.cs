namespace Enterprise.Registry.GUI
{
	public partial class OpportunityStatusRegistryControl : RegistryZUserControl
	{
		public OpportunityStatusRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			OpportunityStatusGrid.ReadOnly = readOnly;
		}
	}
}
