using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class ComplianceSubTypeDependencyConfigurationControl : RegistryZUserControl
	{
		#region Controls

		ZArchitecture.ZGrid ComplianceSubTypeDependencyConfigurationGrid;

		#endregion

		public ComplianceSubTypeDependencyConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ComplianceSubTypeDependencyConfigurationGrid.ReadOnly = readOnly;
		}
	}
}

