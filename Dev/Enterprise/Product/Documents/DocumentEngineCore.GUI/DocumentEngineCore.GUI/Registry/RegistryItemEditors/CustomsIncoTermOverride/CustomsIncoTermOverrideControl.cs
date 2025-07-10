using Enterprise.Registry.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public partial class CustomsIncoTermOverrideControl : RegistryZUserControl
	{
		public CustomsIncoTermOverrideControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			IncoTermsGrid.ReadOnly = readOnly;
		}
	}
}
