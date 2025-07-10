using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class TaxRecognitionDefaultingRulesControl : RegistryZUserControl
	{
		public TaxRecognitionDefaultingRulesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			Enabled = !readOnly;
		}
	}
}
