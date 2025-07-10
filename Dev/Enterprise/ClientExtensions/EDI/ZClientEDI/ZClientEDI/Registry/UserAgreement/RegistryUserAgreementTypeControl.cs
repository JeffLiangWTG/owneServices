using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class RegistryUserAgreementTypeControl : RegistryZUserControl
	{
		public RegistryUserAgreementTypeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			codeDescriptionGrid.ReadOnly = readOnly;
		}
	}
}
