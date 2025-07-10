using Enterprise.Registry.GUI;

namespace Enterprise.Customs.EU.GUI.Registry
{
	public partial class NctsDefaultPrincipalRegistryItemUserControl : RegistryZUserControl
	{
		public NctsDefaultPrincipalRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			LeaveBlankCheckBox.ReadOnly = PrincipalGuidFindBox.ReadOnly = readOnly;
		}
	}
}
