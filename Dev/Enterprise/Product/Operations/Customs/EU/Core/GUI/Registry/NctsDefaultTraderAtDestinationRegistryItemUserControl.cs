using Enterprise.Registry.GUI;

namespace Enterprise.Customs.EU.GUI.Registry
{
	public partial class NctsDefaultTraderAtDestinationRegistryItemUserControl : RegistryZUserControl
	{
		public NctsDefaultTraderAtDestinationRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			LeaveBlankCheckBox.ReadOnly = TraderAtDestinationGuidFindBox.ReadOnly = readOnly;
		}
	}
}
