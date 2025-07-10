namespace Enterprise.Registry.GUI
{
	public partial class OpportunityClosedReasonsControl : RegistryZUserControl
	{
		public OpportunityClosedReasonsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			MainGrid.ReadOnly = readOnly;
			SubGrid.ReadOnly = readOnly;
		}
	}
}
