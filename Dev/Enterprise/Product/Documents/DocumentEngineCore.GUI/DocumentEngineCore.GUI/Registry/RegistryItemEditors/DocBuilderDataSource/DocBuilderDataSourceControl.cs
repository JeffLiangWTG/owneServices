using Enterprise.Registry.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public partial class DocBuilderDataSourceControl : RegistryZUserControl
	{
		public DocBuilderDataSourceControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			FreightRadioButton.ReadOnly = readOnly;
			BrokerageRadioButton.ReadOnly = readOnly;
		}
	}
}
