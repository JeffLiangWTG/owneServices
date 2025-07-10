namespace Enterprise.Registry.GUI
{
	public partial class ContainerPenaltyFreeDaysOptionsRegistryItemControl : RegistryZUserControl
	{
		public ContainerPenaltyFreeDaysOptionsRegistryItemControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			this.UnlimitedFreeDaysCheckBox.ReadOnly = readOnly;
			this.FreeDaysCalcEdit.ReadOnly = readOnly;
		}
	}
}
