using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class ComplianceSubTypeAllocationOverrideConfigurationControl : RegistryZUserControl
	{
		#region Controls

		ZArchitecture.ZGrid ComplianceSubTypeAllocationOverrideConfigurationGrid;

		#endregion

		public ComplianceSubTypeAllocationOverrideConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ComplianceSubTypeAllocationOverrideConfigurationGrid.ReadOnly = readOnly;
		}
	}
}

