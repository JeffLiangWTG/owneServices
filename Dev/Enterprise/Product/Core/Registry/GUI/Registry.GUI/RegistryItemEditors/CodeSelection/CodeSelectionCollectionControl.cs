namespace Enterprise.Registry.GUI
{
	public partial class CodeSelectionCollectionControl : RegistryZUserControl
	{
		public CodeSelectionCollectionControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			codeSelectionGrid.ReadOnly = readOnly;
		}
	}
}
