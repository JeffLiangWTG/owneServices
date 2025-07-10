using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class ComplianceSubTypeAttributionRuleConfigurationControl : RegistryZUserControl
	{
		#region Controls

		ZArchitecture.ZGrid ComplianceSubTypeAttributionRuleConfigurationGrid;

		#endregion

		public ComplianceSubTypeAttributionRuleConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ComplianceSubTypeAttributionRuleConfigurationGrid.ReadOnly = readOnly;
		}
	}
}

