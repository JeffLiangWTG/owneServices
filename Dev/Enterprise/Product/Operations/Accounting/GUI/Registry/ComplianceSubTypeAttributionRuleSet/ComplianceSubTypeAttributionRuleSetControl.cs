using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ComplianceSubTypeAttributionRuleSetControl : RegistryZUserControl
	{
		ZArchitecture.GUI.ZDropEdit zDropEdit1;
		ZArchitecture.ZLabel zLabel1;
		ZArchitecture.GUI.ZPanel zPanel1;

		public ComplianceSubTypeAttributionRuleSetControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			zDropEdit1.ReadOnly = readOnly;
		}
	}
}

