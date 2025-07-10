namespace Enterprise.Registry.GUI
{
	public partial class ABCAnalysisCategoryControl : RegistryZUserControl
	{
		public ABCAnalysisCategoryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ABCAnalysisCategoryGrid.ReadOnly = readOnly;
		}
	}
}
