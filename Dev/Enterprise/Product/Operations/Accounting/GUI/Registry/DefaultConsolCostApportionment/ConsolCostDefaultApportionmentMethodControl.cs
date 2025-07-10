using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ConsolCostDefaultApportionmentMethodControl : RegistryZUserControl
	{
		ZArchitecture.ZGrid ConsolCostDefaultApportionmentMethodGrid;
		ZArchitecture.GUI.ZGroupBox ConsolCostDefaultApportionmentMethodBox;

		public ConsolCostDefaultApportionmentMethodControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ConsolCostDefaultApportionmentMethodGrid.ReadOnly = readOnly;
		}
	}
}

