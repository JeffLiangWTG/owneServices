namespace Enterprise.Registry.GUI
{
	public partial class CargoImpVersionControl : RegistryZUserControl
	{
		public CargoImpVersionControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.airlineImpVersionsGrid.ReadOnly = readOnly;
			this.defaultVersionDropEdit.ReadOnly = readOnly;
		}
	}
}
