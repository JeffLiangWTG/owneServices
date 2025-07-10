using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.TaxFramework.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class TaxSystemsConfigurationControl : RegistryZUserControl
	{
		public TaxSystemsConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			TaxSystemsGrid.ReadOnly = readOnly;
		}
	}
}
