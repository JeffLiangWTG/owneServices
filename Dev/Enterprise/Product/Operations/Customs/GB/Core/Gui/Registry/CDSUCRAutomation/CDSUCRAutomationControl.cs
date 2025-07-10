using Enterprise.Registry.GUI;

namespace Enterprise.Customs.GB.GUI.Registry.CDSUCRAutomation
{
	public partial class CDSUCRAutomationControl : RegistryZUserControl
	{
		public CDSUCRAutomationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			comboBoxCDSAutomationSettings.ReadOnly = readOnly;
		}
	}
}
