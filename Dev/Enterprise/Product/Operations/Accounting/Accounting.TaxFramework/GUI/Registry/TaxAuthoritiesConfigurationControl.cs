using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.TaxFramework.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class TaxAuthoritiesConfigurationControl : RegistryZUserControl
	{
		public TaxAuthoritiesConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			TaxAuthoritiesGrid.ReadOnly = readOnly;
		}
	}
}
