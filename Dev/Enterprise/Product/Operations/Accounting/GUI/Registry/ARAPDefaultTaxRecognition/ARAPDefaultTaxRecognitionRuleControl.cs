using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class ARAPDefaultTaxRecognitionRuleControl : RegistryZUserControl
	{
		#region Controls

		public ZArchitecture.ZGrid ARAPDefaultTaxRecognitionRuleGrid;

		#endregion

		public ARAPDefaultTaxRecognitionRuleControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ARAPDefaultTaxRecognitionRuleGrid.ReadOnly = readOnly;
		}
	}
}

